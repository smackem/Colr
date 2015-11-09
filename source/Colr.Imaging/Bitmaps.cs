﻿using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Net;
using System.Drawing.Imaging;
using System.Linq;
using System.IO;

namespace Colr.Imaging
{
    public static class Bitmaps
    {
        public static readonly PixelFormat RequiredPixelFormat = PixelFormat.Format32bppArgb;

        public static async Task<Bitmap> LoadFromHttp(string uriString)
        {
            using (var bitmap = await FromHttp(uriString))
                return BltConvert(bitmap);
        }

        public static Bitmap LoadFromFile(string path)
        {
            using (var bitmap = new Bitmap(path))
                return BltConvert(bitmap);
        }

        public static Bitmap LoadFromStream(Stream stream)
        {
            using (var bitmap = new Bitmap(stream))
                return BltConvert(bitmap);
        }

        public static unsafe double? GetDominantHue(this Bitmap bitmap)
        {
            var hueCounts = new int[3600];
            var mostCommonHue = default(int?);

            using (var bits = BitsLock.FromBitmap(bitmap))
            {
                var pPixel = bits.Ptr;
                var pixelCount = bitmap.Width * bitmap.Height;

                for (int i = 0; i < pixelCount; i++, pPixel++)
                {
                    var hsv = ColorHsv.FromRgb(*pPixel);

                    if (hsv.S > 0.02)
                    {
                        var multipliedHue = (int)(hsv.H * 10.0);
                        var hueCount = ++hueCounts[multipliedHue];

                        if (mostCommonHue == null || hueCount > hueCounts[mostCommonHue.Value])
                            mostCommonHue = multipliedHue;
                    }
                }
            }

            return mostCommonHue != null
                   ? (double?)(mostCommonHue.Value / 10.0)
                   : null;
        }

        public static unsafe double? GetDominantHueParallel(this Bitmap bitmap)
        {
            var distribution = new int[3600];

            using (var bits = BitsLock.FromBitmap(bitmap))
            {
                var pPixel = bits.Ptr;
                var segmentWidth = bitmap.Width / 2;
                var segmentHeight = bitmap.Height / 2;
                var stride = bitmap.Width;

                var tasks = new[]
                {
                    Task.Run(() => GetHueDistribution(pPixel, segmentWidth, segmentHeight, stride)),
                    Task.Run(() => GetHueDistribution(pPixel + segmentWidth, segmentWidth, segmentHeight, stride)),
                    Task.Run(() => GetHueDistribution(pPixel + segmentHeight * stride, segmentWidth, segmentHeight, stride)),
                    Task.Run(() => GetHueDistribution(pPixel + segmentHeight * stride + segmentWidth, segmentWidth, segmentHeight, stride)),
                };

                Task.WaitAll(tasks);

                foreach (var task in tasks)
                {
                    for (var i = 0; i < distribution.Length; i++)
                        distribution[i] += task.Result[i];
                }
            }

            int mostCommonHue = 0;
            for (var i = 1; i < distribution.Length; i++)
            {
                if (distribution[i] > distribution[mostCommonHue])
                    mostCommonHue = i;
            }

            return distribution[mostCommonHue] > 0
                   ? (double?)(mostCommonHue / 10.0)
                   : null;
        }


        ////////////////////////////////////////////////////////////////

        static unsafe int[] GetHueDistribution(ColorArgb* pBits, int width, int height, int stride)
        {
            var hueCounts = new int[3600];

            for (var y = 0; y < height; y++)
            {
                for (var x = 0; x < width; x++)
                {
                    var hsv = ColorHsv.FromRgb(pBits[x]);

                    if (hsv.S > 0.02)
                    {
                        var multipliedHue = (int)(hsv.H * 10.0);

                        hueCounts[multipliedHue]++;
                    }
                }

                pBits += stride;
            }

            return hueCounts;
        }

        static async Task<Bitmap> FromHttp(string uriString)
        {
            var request = WebRequest.CreateHttp(uriString);

            using (var response = await request.GetResponseAsync())
            using (var stream = response.GetResponseStream())
                return new Bitmap(stream);
        }

        static Bitmap BltConvert(Bitmap bitmap)
        {
            var targetBitmap = new Bitmap(bitmap.Width, bitmap.Height, RequiredPixelFormat);

            using (var g = Graphics.FromImage(targetBitmap))
                g.DrawImageUnscaled(bitmap, Point.Empty);

            return targetBitmap;
        }

        unsafe class BitsLock : IDisposable
        {
            readonly ColorArgb* ptr;
            readonly BitmapData bmd;
            readonly Bitmap bitmap;

            public static BitsLock FromBitmap(Bitmap bitmap)
            {
                var bmd = bitmap.LockBits(
                    new Rectangle(0, 0, bitmap.Width, bitmap.Height),
                    ImageLockMode.ReadOnly,
                    RequiredPixelFormat);

                return new BitsLock((ColorArgb*)bmd.Scan0, bitmap, bmd);
            }

            public ColorArgb* Ptr
            {
                get { return this.ptr; }
            }

            public void Dispose()
            {
                this.bitmap.UnlockBits(this.bmd);
            }

            BitsLock(ColorArgb* bits, Bitmap bitmap, BitmapData bmd)
            {
                this.ptr = bits;
                this.bitmap = bitmap;
                this.bmd = bmd;
            }
        }
    }
}
