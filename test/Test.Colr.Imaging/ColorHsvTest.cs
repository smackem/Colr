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
    public class ColorHsvTest
    {
        [Test]
        public void TestConstruction()
        {
            var hsv = ColorHsv.FromHsv(120.0, 0.5, 0.5);
            Assert.That(hsv.H, Is.EqualTo(120.0));
            Assert.That(hsv.S, Is.EqualTo(0.5));
            Assert.That(hsv.V, Is.EqualTo(0.5));
        }

        [Test]
        public void TestFromRgb()
        {
            var red = ColorHsv.FromRgb(ColorArgb.FromArgb(255, 255, 0, 0));
            Assert.That(red.H, Is.EqualTo(0.0));
            Assert.That(red.S, Is.EqualTo(1.0));
            Assert.That(red.V, Is.EqualTo(1.0));

            var green = ColorHsv.FromRgb(ColorArgb.FromArgb(255, 0, 255, 0));
            Assert.That(green.H, Is.EqualTo(120.0));
            Assert.That(green.S, Is.EqualTo(1.0));
            Assert.That(green.V, Is.EqualTo(1.0));

            var blue = ColorHsv.FromRgb(ColorArgb.FromArgb(255, 0, 0, 255));
            Assert.That(blue.H, Is.EqualTo(240.0));
            Assert.That(blue.S, Is.EqualTo(1.0));
            Assert.That(blue.V, Is.EqualTo(1.0));

            var black = ColorHsv.FromRgb(ColorArgb.FromArgb(255, 0, 0, 0));
            Assert.That(black.H, Is.EqualTo(0.0));
            Assert.That(black.S, Is.EqualTo(0.0));
            Assert.That(black.V, Is.EqualTo(0.0));

            var white = ColorHsv.FromRgb(ColorArgb.FromArgb(255, 255, 255, 255));
            Assert.That(white.H, Is.EqualTo(0.0));
            Assert.That(white.S, Is.EqualTo(0.0));
            Assert.That(white.V, Is.EqualTo(1.0));
        }

        [Test]
        public void TestEquals()
        {
            var hsv = ColorHsv.FromHsv(120.0, 0.5, 0.5);
            Assert.That(hsv, Is.EqualTo(hsv));
            Assert.That(ColorHsv.FromHsv(120.0, 0.5, 0.5), Is.EqualTo(ColorHsv.FromHsv(120.0, 0.5, 0.5)));
            Assert.That(ColorHsv.FromHsv(120.0, 0.5, 0.5), Is.Not.EqualTo(ColorHsv.FromHsv(120.0, 0.5, 0.75)));
        }

        [Test]
        public void TestHashCode()
        {
            Assert.That(ColorHsv.FromHsv(120.0, 0.5, 0.5).GetHashCode(),
                Is.EqualTo(ColorHsv.FromHsv(120.0, 0.5, 0.5).GetHashCode()));
            Assert.That(ColorHsv.FromHsv(120.0, 0.5, 0.5).GetHashCode(),
                Is.Not.EqualTo(ColorHsv.FromHsv(120.0, 0.5, 0.75).GetHashCode()));
        }

        [Test]
        public void TestEqualityOperators()
        {
            Assert.That(ColorHsv.FromHsv(120.0, 0.5, 0.5) == ColorHsv.FromHsv(120.0, 0.5, 0.5), Is.True);
            Assert.That(ColorHsv.FromHsv(120.0, 0.5, 0.5) == ColorHsv.FromHsv(120.0, 0.5, 0.75), Is.False);
            Assert.That(ColorHsv.FromHsv(120.0, 0.5, 0.5) != ColorHsv.FromHsv(120.0, 0.5, 0.5), Is.False);
            Assert.That(ColorHsv.FromHsv(120.0, 0.5, 0.5) != ColorHsv.FromHsv(120.0, 0.5, 0.75), Is.True);
        }
    }
}
