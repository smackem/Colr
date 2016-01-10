using Colr.Imaging;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Test.Colr.Imaging
{
    [TestFixture]
    public class ColrBitmapTest
    {
        [Test]
        public void TestLoadFromStream()
        {
            using (var bitmap = ColrBitmap.LoadFromStream(OpenResource("TestImage1.jpg"), null))
            {
                Assert.That(bitmap.Bitmap.PixelFormat, Is.EqualTo(ColrBitmap.RequiredPixelFormat));
                Assert.That(bitmap.Bitmap.Width, Is.EqualTo(1920));
                Assert.That(bitmap.Bitmap.Height, Is.EqualTo(1200));

                Action<Color, byte, byte, byte> assertIsColor = (color, r, g, b) =>
                    Assert.That(color.R == r && color.G == g && color.B == b);

                assertIsColor(bitmap.Bitmap.GetPixel(0, 0), 90, 199, 219);
                assertIsColor(bitmap.Bitmap.GetPixel(1919, 0), 31, 76, 118);
                assertIsColor(bitmap.Bitmap.GetPixel(0, 1199), 55, 107, 167);
                assertIsColor(bitmap.Bitmap.GetPixel(1919, 1199), 68, 40, 39);
            }

            using (var bitmap = ColrBitmap.LoadFromStream(OpenResource("TestImage1SmallWithAlpha.png"), null))
            {
                Assert.That(bitmap.Bitmap.PixelFormat, Is.EqualTo(ColrBitmap.RequiredPixelFormat));
                Assert.That(bitmap.Bitmap.Width, Is.EqualTo(192));
                Assert.That(bitmap.Bitmap.Height, Is.EqualTo(120));

                var color = bitmap.Bitmap.GetPixel(0, 0);
                Assert.That(color.A, Is.EqualTo(100));
                Assert.That(color.R, Is.EqualTo(96));
                Assert.That(color.G, Is.EqualTo(124));
                Assert.That(color.B, Is.EqualTo(173));
            }

            using (var bitmap = ColrBitmap.LoadFromStream(OpenResource("TestImage1.jpg"), new Size(100, 100)))
            {
                Assert.That(bitmap.Bitmap.PixelFormat, Is.EqualTo(ColrBitmap.RequiredPixelFormat));
                Assert.That(bitmap.Bitmap.Width, Is.EqualTo(100));
                Assert.That(bitmap.Bitmap.Height, Is.EqualTo(100));
            }
        }

        [Test]
        public void TestDominantHueMethods()
        {
            using (var stream = OpenResource("TestImage1.jpg"))
            using (var bitmap = ColrBitmap.LoadFromStream(stream, null))
            {
                var dist = bitmap.GetColorDistribution(3600, 10, 10);
                var dominantColor = dist.GetDominantColor();

                Assert.That(dist, Is.Not.Null);
                Assert.That(dominantColor, Is.Not.Null);
                Assert.That(dominantColor.Value.H, Is.EqualTo(210.0));
            }

            using (var stream = OpenResource("TestImage1SmallWithAlpha.png"))
            using (var bitmap = ColrBitmap.LoadFromStream(stream, null))
            {
                var dist = bitmap.GetColorDistribution(3600, 10, 10);
                var dominantColor = dist.GetDominantColor();

                Assert.That(dist, Is.Not.Null);
                Assert.That(dominantColor, Is.Not.Null);
                Assert.That(dominantColor.Value.H, Is.EqualTo(210.0));
            }
        }

        [Test]
        public void TestDominantHueFailure()
        {
            using (var stream = OpenResource("BlackWhite.png"))
            using (var bitmap = ColrBitmap.LoadFromStream(stream, null))
            {
                var dist = bitmap.GetColorDistribution(360, 40, 40);
                var dominantColor = dist.GetDominantColor();

                Assert.That(dominantColor, Is.Null);
            }
        }

        [Test]
        public void TestCorrelation()
        {
            var size = new Size(100, 100);

            using (var bitmap1 = ColrBitmap.LoadFromStream(OpenResource("TestImage1.jpg"), size))
            using (var bitmap2 = ColrBitmap.LoadFromStream(OpenResource("TestImage1Medium.png"), size))
            {
                Assert.That(ColrBitmap.GetCorrelationCoefficient(bitmap1, bitmap2),
                    Is.EqualTo(0.0));
            }

            size = new Size(32, 32);

            using (var bitmap1 = ColrBitmap.LoadFromStream(OpenResource("TestImage1.jpg"), size))
            using (var bitmap2 = ColrBitmap.LoadFromStream(OpenResource("TestImage1SmallWithAlpha.png"), size))
            {
                Assert.That(ColrBitmap.GetCorrelationCoefficient(bitmap1, bitmap2),
                    Is.LessThan(0.05));
            }

            using (var bitmap1 = ColrBitmap.LoadFromStream(OpenResource("BlackWhite.png"), size))
            using (var bitmap2 = ColrBitmap.LoadFromStream(OpenResource("BlackWhiteLarge.png"), size))
            {
                Assert.That(ColrBitmap.GetCorrelationCoefficient(bitmap1, bitmap2),
                    Is.EqualTo(0.0));
            }

            using (var bitmap1 = ColrBitmap.LoadFromStream(OpenResource("Black.png"), size))
            using (var bitmap2 = ColrBitmap.LoadFromStream(OpenResource("AntiBlack.png"), size))
            {
                Assert.That(ColrBitmap.GetCorrelationCoefficient(bitmap1, bitmap2),
                    Is.GreaterThan(0.95));
            }
        }


        ///////////////////////////////////////////////////////////////////////

        Stream OpenResource(string name)
        {
            var assembly = GetType().Assembly;

            return assembly.GetManifestResourceStream(assembly.GetName().Name + ".Resources." + name);
        }
    }
}
