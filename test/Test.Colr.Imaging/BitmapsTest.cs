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
    public class BitmapsTest
    {
        [Test]
        public void TestLoadFromStream()
        {
            using (var bitmap = Bitmaps.LoadFromStream(OpenResource("TestImage1.jpg")))
            {
                Assert.That(bitmap.PixelFormat, Is.EqualTo(Bitmaps.RequiredPixelFormat));
                Assert.That(bitmap.Width, Is.EqualTo(1920));
                Assert.That(bitmap.Height, Is.EqualTo(1200));

                Action<Color, byte, byte, byte> assertIsColor = (color, r, g, b) =>
                    Assert.That(color.R == r && color.G == g && color.B == b);

                assertIsColor(bitmap.GetPixel(0, 0), 90, 199, 219);
                assertIsColor(bitmap.GetPixel(1919, 0), 31, 76, 118);
                assertIsColor(bitmap.GetPixel(0, 1199), 55, 107, 167);
                assertIsColor(bitmap.GetPixel(1919, 1199), 68, 40, 39);
            }

            using (var bitmap = Bitmaps.LoadFromStream(OpenResource("TestImage1SmallWithAlpha.png")))
            {
                Assert.That(bitmap.PixelFormat, Is.EqualTo(Bitmaps.RequiredPixelFormat));
                Assert.That(bitmap.Width, Is.EqualTo(96));
                Assert.That(bitmap.Height, Is.EqualTo(60));

                var color = bitmap.GetPixel(0, 0);
                Assert.That(color.A, Is.EqualTo(100));
                Assert.That(color.R, Is.EqualTo(96));
                Assert.That(color.G, Is.EqualTo(99));
                Assert.That(color.B, Is.EqualTo(140));
            }
        }

#pragma warning disable 618 // disable "deprecated" warning for GetDominantHue_Slow
        [Test]
        public void TestDominantHueMethods()
        {
            using (var stream = OpenResource("TestImage1.jpg"))
            using (var bitmap = Bitmaps.LoadFromStream(stream))
            {
                var hue = bitmap.GetDominantHue_Slow();
                var dist = bitmap.GetHueDistribution(3600);

                Assert.That(hue, Is.Not.Null);
                Assert.That(hue == dist.DominantHue);
                Assert.That(hue == 210.0);
            }

            using (var stream = OpenResource("TestImage1SmallWithAlpha.png"))
            using (var bitmap = Bitmaps.LoadFromStream(stream))
            {
                var hue = bitmap.GetDominantHue_Slow();
                var dist = bitmap.GetHueDistribution(3600);

                Assert.That(hue, Is.Not.Null);
                Assert.That(hue == dist.DominantHue);
                Assert.That(hue == 210.0);
            }
        }

        [Test]
        public void TestDominantHueFailure()
        {
            using (var stream = OpenResource("BlackWhite.png"))
            using (var bitmap = Bitmaps.LoadFromStream(stream))
            {
                var hue = bitmap.GetDominantHue_Slow();
                var dist = bitmap.GetHueDistribution(3600);

                Assert.That(hue, Is.Null);
                Assert.That(dist.DominantHue, Is.Null);
            }
        }

        [Test]
        [Explicit]
        public void SpeedTestGetDominantHue_Slow()
        {
            using (var stream = OpenResource("TestImage1.jpg"))
            using (var bitmap = Bitmaps.LoadFromStream(stream))
            {
                for (var i = 0; i < 20; i++)
                    bitmap.GetDominantHue_Slow();
            }
        }
#pragma warning restore 618

        [Test]
        [Explicit]
        public void SpeedTestGetDominantHueParallel()
        {
            using (var stream = OpenResource("TestImage1.jpg"))
            using (var bitmap = Bitmaps.LoadFromStream(stream))
            {
                for (var i = 0; i < 20; i++)
                    bitmap.GetHueDistribution(3600);
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
