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
    /// <summary>
    /// A bitmap class aimed at easy color analysis.
    /// </summary>
    public sealed class ColrBitmap : IDisposable
    {
        /// <summary>
        /// The <see cref="PixelFormat"/> used by this class.
        /// </summary>
        internal static readonly PixelFormat RequiredPixelFormat = PixelFormat.Format32bppArgb;

        readonly List<ColorArgb> argbPixels;
        readonly List<ColorHsv> hsvPixels;
        readonly int stride;

        /// <summary>
        /// Gets the <see cref="System.Drawing.Bitmap"/> containing the image data of
        /// the <see cref="ColrBitmap"/>; the image data is in the <see cref="RequiredPixelFormat"/>.
        /// </summary>
        public Bitmap Bitmap { get; }

        /// <summary>
        /// Gets the image pixels in ARGB color space.
        /// </summary>
        public IReadOnlyList<ColorArgb> ArgbPixels
        {
            get { return this.argbPixels; }
        }

        /// <summary>
        /// Gets the image pixels in HSV color space.
        /// </summary>
        public IReadOnlyList<ColorHsv> HsvPixels
        {
            get { return this.hsvPixels; }
        }

        /// <summary>
        /// Loads a <see cref="ColrBitmap"/> from a remote resource via HTTP.
        /// </summary>
        /// <param name="uriString">The URI of the remote resource.</param>
        /// <returns>An awaitable <see cref="Task"/> that yields the bitmap.</returns>
        public static async Task<ColrBitmap> LoadFromHttp(string uriString)
        {
            using (var bitmap = await BitmapFromHttp(uriString))
                return CreateFromBitmap(bitmap);
        }

        /// <summary>
        /// Loads a <see cref="ColrBitmap"/> from a file.
        /// </summary>
        /// <param name="path">The path to the file.</param>
        /// <returns>The loaded <see cref="ColrBitmap"/>.</returns>
        public static ColrBitmap LoadFromFile(string path)
        {
            using (var bitmap = new Bitmap(path))
                return CreateFromBitmap(bitmap);
        }

        /// <summary>
        /// Loads a <see cref="ColrBitmap"/> from a stream.
        /// </summary>
        /// <param name="stream">The stream to load from.</param>
        /// <returns>The loaded <see cref="ColrBitmap"/>.</returns>
        public static ColrBitmap LoadFromStream(Stream stream)
        {
            using (var bitmap = new Bitmap(stream))
                return CreateFromBitmap(bitmap);
        }

        /// <summary>
        /// Analyzes the image and calculates the color distribution.
        /// </summary>
        /// <param name="hueSteps">The number of steps to use for the hue distribution. The granularity
        /// of the hue distribution is <paramref name="hueSteps"/> divided by 360 degrees.
        /// </param>
        /// <param name="saturationSteps">The number of steps to use for the saturation distribution.</param>
        /// <param name="valueSteps">The number of steps to use for the value distribution.</param>
        /// <returns>An instance of <see cref="ColorDistribution"/>. The method
        /// <see cref="ColorDistribution.GetHueDistribution"/> returns <paramref name="hueSteps"/> elements.</returns>
        public ColorDistribution GetColorDistribution(int hueSteps, int saturationSteps, int valueSteps)
        {
            var distribution = new ColorDistribution(hueSteps, saturationSteps, valueSteps);

            foreach (var hsv in hsvPixels)
                distribution.AddPixel(hsv);

            return distribution;
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
    }
}
