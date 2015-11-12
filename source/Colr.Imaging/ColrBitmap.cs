using System;
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
        internal HueDistribution GetHueDistribution(int hueSteps)
        {
            var hueCounts = new int[hueSteps];
            var granularity = hueSteps / 360.0;
            var mostCommonHue = default(int?);

            foreach (var hsv in this.hsvPixels)
            {
                if (hsv.S > 0.02)
                {
                    var multipliedHue = (int)(hsv.H * granularity);
                    var hueCount = ++hueCounts[multipliedHue];

                    if (mostCommonHue == null || hueCount > hueCounts[mostCommonHue.Value])
                        mostCommonHue = multipliedHue;
                }
            }

            var dominantHue = mostCommonHue != null
                              ? (double?)(mostCommonHue.Value / 10.0)
                              : null;

            return new HueDistribution(dominantHue, hueCounts);
        }

        public async Task<HueDistribution> GetHueDistributionAsync(int hueSteps)
        {
            var distribution = await GetHueDistributionParallel(hueSteps);
            var mostCommonHue = 0;

            for (var i = 1; i < distribution.Length; i++)
            {
                if (distribution[i] > distribution[mostCommonHue])
                    mostCommonHue = i;
            }

            var dominantHue = distribution[mostCommonHue] > 0
                              ? (double?)(mostCommonHue / (hueSteps / 360.0))
                              : null;

            return new HueDistribution(dominantHue, distribution);
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
            var pixelsPerTask = Bitmap.Width * Bitmap.Height / 4;

            var tasks = new[]
            {
                Task.Run(() => GetHueDistribution(0, pixelsPerTask, hueSteps)),
                Task.Run(() => GetHueDistribution(pixelsPerTask, pixelsPerTask, hueSteps)),
                Task.Run(() => GetHueDistribution(pixelsPerTask * 2, pixelsPerTask, hueSteps)),
                Task.Run(() => GetHueDistribution(pixelsPerTask * 3, pixelsPerTask, hueSteps)),
            };

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
    }
}
