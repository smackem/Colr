using Colr.Imaging;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Test.Colr.Imaging
{
    [TestFixture]
    public class ColorArgbTest
    {
        [Test]
        public void TestConstruction()
        {
            var argb1 = ColorArgb.FromArgb(0x11223344);
            Assert.That(argb1.A, Is.EqualTo(0x11));
            Assert.That(argb1.R, Is.EqualTo(0x22));
            Assert.That(argb1.G, Is.EqualTo(0x33));
            Assert.That(argb1.B, Is.EqualTo(0x44));

            var argb2 = ColorArgb.FromArgb(0x11, 0x22, 0x33, 0x44);
            Assert.That(argb1.A, Is.EqualTo(0x11));
            Assert.That(argb1.R, Is.EqualTo(0x22));
            Assert.That(argb1.G, Is.EqualTo(0x33));
            Assert.That(argb1.B, Is.EqualTo(0x44));
        }

        [Test]
        public void TestFromHsv()
        {
            var hsv1 = ColorHsv.FromHsv(100.0, 0.5, 0.5);
            var argb1 = ColorArgb.FromHsv(0x11, hsv1);
            Assert.That(argb1.A, Is.EqualTo(0x11));
            Assert.That(argb1.R, Is.EqualTo(85));
            Assert.That(argb1.G, Is.EqualTo(128));
            Assert.That(argb1.B, Is.EqualTo(64));

            var hsv2 = ColorHsv.FromHsv(0.0, 0.0, 0.0);
            var argb2 = ColorArgb.FromHsv(0x11, hsv2);
            Assert.That(argb2.R, Is.EqualTo(0));
            Assert.That(argb2.G, Is.EqualTo(0));
            Assert.That(argb2.B, Is.EqualTo(0));

            var hsv3 = ColorHsv.FromHsv(0.0, 1.0, 1.0);
            var argb3 = ColorArgb.FromHsv(0x11, hsv3);
            Assert.That(argb3.R, Is.EqualTo(255));
            Assert.That(argb3.G, Is.EqualTo(0));
            Assert.That(argb3.B, Is.EqualTo(0));

            var hsv4 = ColorHsv.FromHsv(359.0, 1.0, 1.0);
            var argb4 = ColorArgb.FromHsv(0x11, hsv4);
            Assert.That(argb4.R, Is.EqualTo(255));
            Assert.That(argb4.G, Is.EqualTo(0));
            Assert.That(argb4.B, Is.EqualTo(4));
        }

        [Test]
        public void TestEquals()
        {
            var argb = ColorArgb.FromArgb(0x11223344);
            Assert.That(argb, Is.EqualTo(argb));
            Assert.That(ColorArgb.FromArgb(0x11223344),
                Is.EqualTo(ColorArgb.FromArgb(0x11223344)));
            Assert.That(ColorArgb.FromArgb(0x11, 0x22, 0x33, 0x44),
                Is.EqualTo(ColorArgb.FromArgb(0x11, 0x22, 0x33, 0x44)));
            Assert.That(ColorArgb.FromArgb(0x11223344),
                Is.EqualTo(ColorArgb.FromArgb(0x11, 0x22, 0x33, 0x44)));
        }

        [Test]
        public void TestHashCode()
        {
            var argb = ColorArgb.FromArgb(0x11223344);
            Assert.That(argb.GetHashCode(), Is.EqualTo(argb.GetHashCode()));
            Assert.That(ColorArgb.FromArgb(0x11223344).GetHashCode(),
                Is.EqualTo(ColorArgb.FromArgb(0x11223344).GetHashCode()));
            Assert.That(ColorArgb.FromArgb(0x11, 0x22, 0x33, 0x44).GetHashCode(),
                Is.EqualTo(ColorArgb.FromArgb(0x11, 0x22, 0x33, 0x44).GetHashCode()));
            Assert.That(ColorArgb.FromArgb(0x11223344).GetHashCode(),
                Is.EqualTo(ColorArgb.FromArgb(0x11, 0x22, 0x33, 0x44).GetHashCode()));
            Assert.That(ColorArgb.FromArgb(0x11223344).GetHashCode(),
                Is.EqualTo(ColorArgb.FromArgb(0x11, 0x22, 0x33, 0x44).GetHashCode()));

            Assert.That(ColorArgb.FromArgb(1).GetHashCode(),
                Is.Not.EqualTo(ColorArgb.FromArgb(2).GetHashCode()));
            Assert.That(ColorArgb.FromArgb(2).GetHashCode(),
                Is.Not.EqualTo(ColorArgb.FromArgb(3).GetHashCode()));
        }

        [Test]
        public void TestIntensity()
        {
            unchecked
            {
                Assert.That(ColorArgb.FromArgb((int)0xffff0000).GetIntensity(), Is.EqualTo(0.299));
                Assert.That(IsClose(ColorArgb.FromArgb((int)0xff00ff00).GetIntensity(), 0.587));
                Assert.That(IsClose(ColorArgb.FromArgb((int)0xff0000ff).GetIntensity(), 0.114));
                Assert.That(IsClose(ColorArgb.FromArgb((int)0x80ffffff).GetIntensity(), 0.5019607));
                Assert.That(ColorArgb.FromArgb((int)0xffffffff).GetIntensity(), Is.EqualTo(1.0));
                Assert.That(ColorArgb.FromArgb(0).GetIntensity(), Is.EqualTo(0.0));
            }
        }

        [Test]
        public void TestInvert()
        {
            var argb = ColorArgb.FromArgb(1, 2, 3, 4);

            Assert.That(argb.Invert(), Is.EqualTo(ColorArgb.FromArgb(1, 255 - 2, 255 - 3, 255 - 4)));
        }

        [Test]
        public void TestNewAlpha()
        {
            var argb = ColorArgb.FromArgb(1, 2, 3, 4);

            Assert.That(argb.NewAlpha(0), Is.EqualTo(ColorArgb.FromArgb(0, 2, 3, 4)));
        }

        [Test]
        public void TestHue()
        {
            unchecked
            {
                Assert.That(ColorArgb.FromArgb((int)0xffff0000).GetHue(), Is.EqualTo(0.0));
                Assert.That(ColorArgb.FromArgb((int)0xff00ff00).GetHue(), Is.EqualTo(120.0));
                Assert.That(ColorArgb.FromArgb((int)0xff0000ff).GetHue(), Is.EqualTo(240.0));
            }
        }

        [Test]
        public void TestEqualityOperators()
        {
            Assert.That(ColorArgb.FromArgb(0x11223344) == ColorArgb.FromArgb(0x11223344), Is.True);
            Assert.That(ColorArgb.FromArgb(0x11223355) == ColorArgb.FromArgb(0x11223344), Is.False);
            Assert.That(ColorArgb.FromArgb(0x11223344) != ColorArgb.FromArgb(0x11223344), Is.False);
            Assert.That(ColorArgb.FromArgb(0x11223355) != ColorArgb.FromArgb(0x11223344), Is.True);
        }

        [Test]
        public void TestHtmlStrings()
        {
            var argb = ColorArgb.FromArgb(unchecked((int)0xff112233));
            Assert.That(argb.ToString(isHtmlFormat: true), Is.EqualTo("#112233"));
            Assert.That(ColorArgb.Parse("#112233"), Is.EqualTo(argb));
        }

        [Test]
        public void TestParse()
        {
            var argb = ColorArgb.FromArgb(unchecked((int)0xff112233));
            Assert.That(ColorArgb.Parse(argb.ToString()), Is.EqualTo(argb));
        }

        [Test]
        public void TestAlphaComposite()
        {
            Assert.That(ColorArgb.AlphaComposite(ColorArgb.White, ColorArgb.Transparent),
                Is.EqualTo(ColorArgb.White));
            Assert.That(ColorArgb.AlphaComposite(ColorArgb.White, ColorArgb.Black),
                Is.EqualTo(ColorArgb.Black));
            Assert.That(ColorArgb.AlphaComposite(ColorArgb.White, ColorArgb.FromArgb(100, 255, 0, 0)),
                Is.EqualTo(ColorArgb.FromArgb(255, 255, 155, 155)));
            Assert.That(ColorArgb.AlphaComposite(ColorArgb.FromArgb(100, 0, 255, 0), ColorArgb.FromArgb(100, 255, 0, 0)),
                Is.EqualTo(ColorArgb.FromArgb(161, 159, 96, 0)));
        }


        ///////////////////////////////////////////////////////////////////////

        private static bool IsClose(double a, double b)
        {
            return Math.Abs(a - b) < 0.0000001;
        }
    }
}
