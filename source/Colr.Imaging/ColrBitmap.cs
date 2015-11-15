﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Colr.Imaging
{
    public sealed class ColrBitmap : IDisposable
    {
        internal static readonly PixelFormat RequiredPixelFormat = PixelFormat.Format32bppArgb;

        readonly List<ColorArgb> argbPixels;
        readonly List<ColorHsv> hsvPixels;
        readonly int stride;

        public Bitmap Bitmap { get; }

        public IReadOnlyList<ColorArgb> ArgbPixels
        {
            get { return this.argbPixels; }
        }

        public IReadOnlyList<ColorHsv> HsvPixels
        {
            get { return this.hsvPixels; }
        }

        public static async Task<ColrBitmap> LoadFromHttp(string uriString)
        {
            using (var bitmap = await BitmapFromHttp(uriString))
                return CreateFromBitmap(bitmap);
        }

        public static ColrBitmap LoadFromFile(string path)
        {
            using (var bitmap = new Bitmap(path))
                return CreateFromBitmap(bitmap);
        }

        public static ColrBitmap LoadFromStream(Stream stream)
        {
            using (var bitmap = new Bitmap(stream))
                return CreateFromBitmap(bitmap);
        }

        [Obsolete("This is just a naive reference implementation for GetHueDistributionAsync")]
        internal ColorDistribution GetHueDistribution(int hueSteps)
        {
            var hueCounts = new int[hueSteps];
            var granularity = hueSteps / 360.0;
            var mostCommonHue = 0;

            foreach (var hsv in this.hsvPixels)
            {
                var multipliedHue = (int)(hsv.H * granularity);
                var hueCount = ++hueCounts[multipliedHue];

                if (hueCount > hueCounts[mostCommonHue])
                    mostCommonHue = multipliedHue;
            }

            var dominantColor = ColorHsv.FromHsv(mostCommonHue / 10.0, 1.0, 1.0);

            return new ColorDistribution(dominantColor, hueCounts, null, null);
        }

        public async Task<ColorDistribution> GetHueDistributionAsync(int hueSteps)
        {
            var distribution = await GetHueDistributionParallel(hueSteps);
            var mostCommonHue = GetIndexOfMaxValue(distribution);
            var granularity = hueSteps / 360.0;
            var dominantHue = mostCommonHue / granularity;
            var svDistributions = await Task.WhenAll(
                GetDistributionForHueParallel(dominantHue, granularity, 100, GetSaturationDistribution),
                GetDistributionForHueParallel(dominantHue, granularity, 100, GetValueDistribution));

            var mostCommonSaturation = GetIndexOfMaxValue(svDistributions[0]);
            var mostCommonValue = GetIndexOfMaxValue(svDistributions[1]);

            return new ColorDistribution(
                ColorHsv.FromHsv(dominantHue, mostCommonSaturation / 100.0, mostCommonValue / 100.0),
                distribution, svDistributions[0], svDistributions[1]);
        }

        int GetIndexOfMaxValue(int[] distribution)
        {
            var indexOfMaxValue = 0;

            for (var i = 1; i < distribution.Length; i++)
            {
                if (distribution[i] > distribution[indexOfMaxValue])
                    indexOfMaxValue = i;
            }

            return indexOfMaxValue;
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Bitmap.Dispose();
        }

        ///////////////////////////////////////////////////////////////////////

        ColrBitmap(Bitmap bitmap, List<ColorArgb> argbPixels, List<ColorHsv> hsvPixels, int stride)
        {
            Bitmap = bitmap;
            this.argbPixels = argbPixels;
            this.hsvPixels = hsvPixels;
            this.stride = stride;
        }

        static async Task<Bitmap> BitmapFromHttp(string uriString)
        {
            var request = WebRequest.CreateHttp(uriString);

            using (var response = await request.GetResponseAsync())
            using (var stream = response.GetResponseStream())
                return new Bitmap(stream);
        }

        static ColrBitmap CreateFromBitmap(Bitmap bitmap)
        {
            var convertedBitmap = BltConvert(bitmap);
            List<ColorArgb> argbPixels;
            List<ColorHsv> hsvPixels;

            FetchPixels(convertedBitmap, out argbPixels, out hsvPixels);

            return new ColrBitmap(convertedBitmap, argbPixels, hsvPixels, convertedBitmap.Width);
        }

        static Bitmap BltConvert(Bitmap bitmap)
        {
            var targetBitmap = new Bitmap(bitmap.Width, bitmap.Height, RequiredPixelFormat);

            using (var g = Graphics.FromImage(targetBitmap))
                g.DrawImageUnscaled(bitmap, Point.Empty);

            return targetBitmap;
        }

        static unsafe void FetchPixels(Bitmap bitmap, out List<ColorArgb> argbPixels, out List<ColorHsv> hsvPixels)
        {
            var pixelCount = bitmap.Width * bitmap.Height;

            argbPixels = new List<ColorArgb>(pixelCount);
            hsvPixels = new List<ColorHsv>(pixelCount);

            var bmd = bitmap.LockBits(
                new Rectangle(0, 0, bitmap.Width, bitmap.Height),
                ImageLockMode.ReadOnly,
                RequiredPixelFormat);

            try
            {
                var pPixel = (ColorArgb*)bmd.Scan0.ToPointer();

                for (int i = 0; i < pixelCount; i++, pPixel++)
                {
                    var argb = *pPixel;

                    argbPixels.Add(argb);
                    hsvPixels.Add(ColorHsv.FromRgb(argb));
                }
            }
            finally
            {
                bitmap.UnlockBits(bmd);
            }
        }

        async Task<int[]> GetHueDistributionParallel(int hueSteps)
        {
            var distribution = new int[hueSteps];
            var taskCount = Environment.ProcessorCount;
            var pixelsPerTask = Bitmap.Width * Bitmap.Height / taskCount;

            var tasks = Enumerable.Range(0, taskCount)
                .Select(i => Task.Run(() => GetHueDistribution(pixelsPerTask * i, pixelsPerTask, hueSteps)))
                .ToArray();

            var segmentDistributions = await Task.WhenAll(tasks);

            foreach (var dist in segmentDistributions)
            {
                for (var i = 0; i < distribution.Length; i++)
                    distribution[i] += dist[i];
            }

            return distribution;
        }

        int[] GetHueDistribution(int offset, int count, int hueSteps)
        {
            var distribution = new int[hueSteps];
            var granularity = hueSteps / 360.0;
            var hsvPixels = this.hsvPixels;
            var end = offset + count;

            for (var i = offset; i < end; i++)
            {
                var hsv = hsvPixels[i];

                if (hsv.S > 0.02)
                {
                    var multipliedHue = (int)(hsv.H * granularity);

                    distribution[multipliedHue]++;
                }
            }

            return distribution;
        }

        delegate int[] DistributionFunction(int offset, int count, double hue, double hueGranularity, int steps);

        async Task<int[]> GetDistributionForHueParallel(double hue, double hueGranularity, int steps,
            DistributionFunction distFunction)
        {
            var distribution = new int[steps];
            var taskCount = Environment.ProcessorCount;
            var pixelsPerTask = Bitmap.Width * Bitmap.Height / taskCount;

            var tasks = Enumerable.Range(0, taskCount)
                .Select(i => Task.Run(() => distFunction(
                    pixelsPerTask * i, pixelsPerTask, hue, hueGranularity, steps)))
                .ToArray();

            var segmentDistributions = await Task.WhenAll(tasks);

            foreach (var dist in segmentDistributions)
            {
                for (var i = 0; i < distribution.Length; i++)
                    distribution[i] += dist[i];
            }

            return distribution;
        }

        int[] GetSaturationDistribution(int offset, int count, double hue, double hueGranularity, int saturationSteps)
        {
            var distribution = new int[saturationSteps];
            var hsvPixels = this.hsvPixels;
            var end = offset + count;

            for (var i = offset; i < end; i++)
            {
                var hsv = hsvPixels[i];
                var h = (int)(hsv.H * hueGranularity) / hueGranularity;

                if (hue == h)
                {
                    var index = Math.Min((int)(hsv.S * saturationSteps), saturationSteps - 1);

                    distribution[index]++;
                }
            }

            return distribution;
        }

        int[] GetValueDistribution(int offset, int count, double hue, double hueGranularity, int valueSteps)
        {
            var distribution = new int[valueSteps];
            var hsvPixels = this.hsvPixels;
            var end = offset + count;

            for (var i = offset; i < end; i++)
            {
                var hsv = hsvPixels[i];
                var h = (int)(hsv.H * hueGranularity) / hueGranularity;

                if (hue == h)
                {
                    var index = Math.Min((int)(hsv.V * valueSteps), valueSteps - 1);

                    distribution[index]++;
                }
            }

            return distribution;
        }
    }
}
