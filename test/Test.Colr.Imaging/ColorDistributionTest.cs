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
    public class ColorDistributionTest
    {
        [Test]
        public void TestGranularities()
        {
            var distribution = new ColorDistribution(180, 10, 5);
            Assert.That(distribution.Hues, Is.EqualTo(180));
            Assert.That(distribution.Saturations, Is.EqualTo(10));
            Assert.That(distribution.Values, Is.EqualTo(5));
            Assert.That(distribution.HueGranularity, Is.EqualTo(0.5));
            Assert.That(distribution.SaturationGranularity, Is.EqualTo(10.0));
            Assert.That(distribution.ValueGranularity, Is.EqualTo(5.0));
        }

        [Test]
        public void TestHueDistribution()
        {
            var distribution = CreateSampleDistribution();
            var hues = distribution.GetHueDistribution();
            Assert.That(hues, Is.Not.Null);
            Assert.That(hues.Count, Is.EqualTo(distribution.Hues));

            var expected = new int[360];
            expected[30] = 4;
            expected[120] = 5;
            CollectionAssert.AreEqual(expected, hues);
        }

        [Test]
        public void TestSaturationDistribution()
        {
            var distribution = CreateSampleDistribution();
            var saturations = distribution.GetSaturationDistribution(30.0);

            Assert.That(saturations, Is.Not.Null);
            Assert.That(saturations.Count, Is.EqualTo(distribution.Saturations));

            var expected = new int[100];
            expected[0] = 2;
            expected[49] = 2;
            CollectionAssert.AreEqual(expected, saturations);
        }

        [Test]
        public void TestValueDistribution()
        {
            var distribution = CreateSampleDistribution();
            var values = distribution.GetValueDistribution(30.0);

            Assert.That(values, Is.Not.Null);
            Assert.That(values.Count, Is.EqualTo(distribution.Values));

            var expected = new int[100];
            expected[0] = 2;
            expected[49] = 2;
            CollectionAssert.AreEqual(expected, values);
        }

        [Test]
        public void TestMostCommonColor()
        {
            var distribution = CreateSampleDistribution();

            Assert.That(distribution.GetMostCommonColor(), Is.EqualTo(
                ColorHsv.FromHsv(120.0, 0.49, 0.49)));
        }

        [Test]
        public void TestDominantColor()
        {
            var distribution = CreateSampleDistribution();

            Assert.That(distribution.GetDominantColor(), Is.EqualTo(
                ColorHsv.FromHsv(120.0, 0.49, 0.49)));

            distribution = new ColorDistribution(360, 100, 100)
                .AddPixel(ColorHsv.FromHsv(100.0, 0.0, 0.0))
                .AddPixel(ColorHsv.FromHsv(100.0, 0.0, 0.0))
                .AddPixel(ColorHsv.FromHsv(100.0, 0.0, 0.1))
                .AddPixel(ColorHsv.FromHsv(100.0, 0.0, 0.1))
                .AddPixel(ColorHsv.FromHsv(100.0, 0.1, 0.0))
                .AddPixel(ColorHsv.FromHsv(100.0, 0.1, 0.0))
                .AddPixel(ColorHsv.FromHsv(200.0, 0.5, 0.5));

            Assert.That(distribution.GetDominantColor(), Is.EqualTo(
                ColorHsv.FromHsv(200.0, 0.49, 0.49)));
        }

        [Test]
        public void TestColorWeight()
        {
            var distribution = CreateSampleDistribution();
            Assert.That(distribution.GetColorWeight(ColorHsv.FromHsv(30.0, 0.0, 0.0)),
                Is.EqualTo(1));
            Assert.That(distribution.GetColorWeight(ColorHsv.FromHsv(30.0, 0.0, 0.5)),
                Is.EqualTo(1));
            Assert.That(distribution.GetColorWeight(ColorHsv.FromHsv(0.0, 0.0, 0.0)),
                Is.EqualTo(0));
            Assert.That(distribution.GetColorWeight(ColorHsv.FromHsv(120.0, 0.5, 0.5)),
                Is.EqualTo(2));
        }

        [Test]
        public void TestAdd()
        {
            var distribution = CreateSampleDistribution();
            distribution.AddPixel(ColorHsv.FromHsv(240.0, 1.0, 1.0));

            distribution.Add(CreateSampleDistribution());

            Assert.That(distribution.GetColorWeight(ColorHsv.FromHsv(30.0, 0.0, 0.0)),
                Is.EqualTo(2));
            Assert.That(distribution.GetColorWeight(ColorHsv.FromHsv(30.0, 0.0, 0.5)),
                Is.EqualTo(2));
            Assert.That(distribution.GetColorWeight(ColorHsv.FromHsv(0.0, 0.0, 0.0)),
                Is.EqualTo(0));
            Assert.That(distribution.GetColorWeight(ColorHsv.FromHsv(120.0, 0.5, 0.5)),
                Is.EqualTo(4));
            Assert.That(distribution.GetColorWeight(ColorHsv.FromHsv(240.0, 1.0, 1.0)),
                Is.EqualTo(1));
        }

        /// <summary>
        /// Should complete in about 3 seconds in RELEASE configuration with 2 processors
        /// </summary>
        [Test]
        public void TestAddSpeed()
        {
            var distribution = CreateSampleDistribution();
            var dist2 = CreateSampleDistribution();

            for (var i = 0; i < 200; i++)
                distribution.Add(dist2);
        }

        /// <summary>
        /// Should complete in about 3 seconds in RELEASE configuration with 2 processors
        /// </summary>
        [Test]
        public void TestHueDistributionSpeed()
        {
            var distribution = CreateSampleDistribution();

            for (var i = 0; i < 500; i++)
                distribution.GetHueDistribution();
        }

        [Test]
        public void TestCorrelation()
        {
            var dist1 = new ColorDistribution(36, 10, 10);
            var dist2 = new ColorDistribution(36, 10, 10);

            Assert.That(ColorDistribution.GetCorrelationCoefficient(dist1, dist2),
                Is.EqualTo(0.0));

            dist1 = CreateSampleDistribution();
            dist2 = CreateSampleDistribution();

            Assert.That(ColorDistribution.GetCorrelationCoefficient(dist1, dist2),
                Is.EqualTo(0.0));

            dist1 = new ColorDistribution(36, 10, 10)
                .AddPixel(ColorHsv.FromHsv(0.0, 0.0, 0.0));
            dist2 = new ColorDistribution(36, 10, 10)
                .AddPixel(ColorHsv.FromHsv(359.0, 1.0, 1.0));

            Assert.That(ColorDistribution.GetCorrelationCoefficient(dist1, dist2),
                Is.GreaterThan(0.95).And.LessThanOrEqualTo(1.0));
        }

        ///////////////////////////////////////////////////////////////////////

        ColorDistribution CreateSampleDistribution()
        {
            return new ColorDistribution(360, 100, 100)
                .AddPixel(ColorHsv.FromHsv(30.0, 0.0, 0.0))
                .AddPixel(ColorHsv.FromHsv(30.0, 0.0, 0.5))
                .AddPixel(ColorHsv.FromHsv(30.0, 0.5, 0.0))
                .AddPixel(ColorHsv.FromHsv(30.0, 0.5, 0.5))
                .AddPixel(ColorHsv.FromHsv(120.0, 0.0, 0.0))
                .AddPixel(ColorHsv.FromHsv(120.0, 0.0, 0.5))
                .AddPixel(ColorHsv.FromHsv(120.0, 0.5, 0.0))
                .AddPixel(ColorHsv.FromHsv(120.0, 0.5, 0.5))
                .AddPixel(ColorHsv.FromHsv(120.0, 0.5, 0.5));
        }
    }
}
