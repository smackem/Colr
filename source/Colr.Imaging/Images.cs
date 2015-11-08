using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Net;
using System.Drawing.Imaging;

namespace Colr.Imaging
{
    public static class Images
    {
        public static async Task<Bitmap> LoadFromHttp(string uriString)
        {
            var bitmap = await FromHttp(uriString);

            return BltConvert(bitmap);
        }

        public static Bitmap LoadFromFile(string path)
        {
            var bitmap = new Bitmap(path);

            return BltConvert(bitmap);
        }

        public static unsafe ColorArgb GetDominantColor(this Bitmap bitmap)
        {
            var hueCounts = new int[3600];
            var maxHue = default(int?);

            using (var bits = BitsLock.FromBitmap(bitmap))
            {
                var pPixel = bits.Ptr;
                var pixelCount = bitmap.Width * bitmap.Height;

                for (int i = 0; i < pixelCount; i++, pPixel++)
                {
                    var hue = pPixel->GetHue();
                    var multipliedHue = (int)(hue * 10.0);
                    var hueCount = ++hueCounts[multipliedHue];

                    if (maxHue == null || hueCount > hueCounts[maxHue.Value])
                        maxHue = multipliedHue;
                }
            }

            var dominantHue = ((double)maxHue.Value) / 10.0;

            return ColorArgb.FromHsv(255, ColorHsv.FromHsv(dominantHue, 1.0, 1.0));
        }


        ////////////////////////////////////////////////////////////////

        static async Task<Bitmap> FromHttp(string uriString)
        {
            var request = HttpWebRequest.CreateHttp(uriString);

            using (var response = await request.GetResponseAsync())
            using (var stream = response.GetResponseStream())
                return new Bitmap(stream);
        }

        static Bitmap BltConvert(Bitmap bitmap)
        {
            var targetBitmap = new Bitmap(bitmap.Width, bitmap.Height, PixelFormat.Format32bppArgb);

            using (var g = Graphics.FromImage(bitmap))
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
                              PixelFormat.Format32bppArgb);

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
