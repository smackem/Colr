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
            using (var bitmap = ColrBitmap.LoadFromStream(OpenResource("TestImage1.jpg")))
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

            using (var bitmap = ColrBitmap.LoadFromStream(OpenResource("TestImage1SmallWithAlpha.png")))
            {
                Assert.That(bitmap.Bitmap.PixelFormat, Is.EqualTo(ColrBitmap.RequiredPixelFormat));
                Assert.That(bitmap.Bitmap.Width, Is.EqualTo(96));
                Assert.That(bitmap.Bitmap.Height, Is.EqualTo(60));

                var color = bitmap.Bitmap.GetPixel(0, 0);
                Assert.That(color.A, Is.EqualTo(100));
                Assert.That(color.R, Is.EqualTo(96));
                Assert.That(color.G, Is.EqualTo(99));
                Assert.That(color.B, Is.EqualTo(140));
            }
        }

#pragma warning disable 618 // disable "deprecated" warning for GetHueDistribution
        [Test]
        public void TestDominantHueMethods()
        {
            using (var stream = OpenResource("TestImage1.jpg"))
            using (var bitmap = ColrBitmap.LoadFromStream(stream))
            {
                var dist1 = bitmap.GetColorDistribution(3600);
                var dist2 = bitmap.GetColorDistributionAsync(3600).Result;

                Assert.That(dist1, Is.Not.Null);
                Assert.That(dist1.GetDominantColor().H == dist2.GetDominantColor().H);
                Assert.That(dist1.GetDominantColor().H == 210.0);
            }

            using (var stream = OpenResource("TestImage1SmallWithAlpha.png"))
            using (var bitmap = ColrBitmap.LoadFromStream(stream))
            {
                var dist1 = bitmap.GetColorDistribution(3600);
                var dist2 = bitmap.GetColorDistributionAsync(3600).Result;
                var dominantColor1 = dist1.GetDominantColor();
                var dominantColor2 = dist2.GetDominantColor();

                Assert.That(dist1, Is.Not.Null);
                Assert.That(dominantColor1.H == dominantColor2.H);
                Assert.That(dominantColor1.H == 210.0);
            }
        }

        [Test]
        public void TestDominantHueFailure()
        {
            using (var stream = OpenResource("BlackWhite.png"))
            using (var bitmap = ColrBitmap.LoadFromStream(stream))
            {
                var dist1 = bitmap.GetColorDistribution(3600);
                var dist2 = bitmap.GetColorDistributionAsync(3600).Result;
                var dominantColor1 = dist1.GetDominantColor();
                var dominantColor2 = dist2.GetDominantColor();

                Assert.That(dominantColor1.H, Is.EqualTo(0.0));
                Assert.That(dominantColor2.H, Is.EqualTo(0.0));
                Assert.That(dominantColor2.S, Is.EqualTo(0.0));
            }
        }

        [Test]
        [Explicit]
        public void SpeedTestGetHueDistribution()
        {
            using (var stream = OpenResource("TestImage1.jpg"))
            using (var bitmap = ColrBitmap.LoadFromStream(stream))
            {
                for (var i = 0; i < 10; i++)
                    bitmap.GetColorDistribution(3600);
            }
        }
#pragma warning restore 618

        [Test]
        [Explicit]
        public void SpeedTestGetHueDistributionAsync()
        {
            using (var stream = OpenResource("TestImage1.jpg"))
            using (var bitmap = ColrBitmap.LoadFromStream(stream))
            {
                for (var i = 0; i < 10; i++)
                    bitmap.GetColorDistributionAsync(3600).Wait();
            }
        }


        ///////////////////////////////////////////////////////////////////////

        Stream OpenResource(String name)
        {
            var assembly = GetType().Assembly;

            return assembly.GetManifestResourceStream(assembly.GetName().Name + ".Resources." + name);
        }
    }
}
