using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Colr.Imaging
{
    public sealed class ColorDistribution
    {
        readonly int[,,] cube;

        internal ColorDistribution(int hues, int saturations, int values)
        {
            Contract.Requires(hues > 0);
            Contract.Requires(saturations > 0);
            Contract.Requires(values > 0);

            this.cube = new int[hues, saturations, values];
        }

        public int Hues
        {
            get { return this.cube.GetLength(0); }
        }

        public int Saturations
        {
            get { return this.cube.GetLength(1); }
        }

        public int Values
        {
            get { return this.cube.GetLength(2); }
        }

        public ColorHsv GetMostCommonColor()
        {
            var maxWeight = -1;
            var maxH = -1;
            var maxS = -1;
            var maxV = -1;
            var hues = Hues;
            var saturations = Saturations;
            var values = Values;

            for (var indexH = 0; indexH < hues; indexH++)
            {
                for (var indexS = 0; indexS < saturations; indexS++)
                {
                    for (var indexV = 0; indexV < values; indexV++)
                    {
                        var weight = this.cube[indexH, indexS, indexV];

                        if (weight > maxWeight)
                        {
                            maxWeight = weight;
                            maxH = indexH;
                            maxS = indexS;
                            maxV = indexV;
                        }
                    }
                }
            }

            return ColorHsv.FromHsv(
                maxH / HueGranularity,
                maxS / SaturationGranularity,
                maxV / ValueGranularity);
        }

        public ColorHsv? GetDominantColor()
        {
            var minS = GetSaturationIndex(0.25);
            var minV = GetValueIndex(0.25);

            var hueDistribution = InternalGetHueDistribution(minS, minV);
            var maxWeight = 0;
            var maxH = -1;

            for (var indexH = 0; indexH < hueDistribution.Count; indexH++)
            {
                var weight = hueDistribution[indexH];

                if (weight > maxWeight)
                {
                    maxWeight = weight;
                    maxH = indexH;
                }
            }

            if (maxH >= 0)
            {
                var saturations = Saturations;
                var values = Values;
                var maxS = -1;
                var maxV = -1;

                maxWeight = 0;

                for (var indexS = minS; indexS < saturations; indexS++)
                {
                    for (var indexV = minV; indexV < values; indexV++)
                    {
                        var weight = this.cube[maxH, indexS, indexV];

                        if (weight > maxWeight)
                        {
                            maxWeight = weight;
                            maxS = indexS;
                            maxV = indexV;
                        }
                    }
                }

                return ColorHsv.FromHsv(
                    maxH / HueGranularity,
                    maxS / SaturationGranularity,
                    maxV / ValueGranularity);
            }
            else
            {
                return null;
            }
        }

        public int GetColorWeight(ColorHsv hsv)
        {
            var indexH = GetHueIndex(hsv.H);
            var indexS = GetSaturationIndex(hsv.S);
            var indexV = GetValueIndex(hsv.V);

            return this.cube[indexH, indexS, indexV];
        }

        public IReadOnlyList<int> GetHueDistribution()
        {
            Contract.Ensures(Contract.Result<IReadOnlyList<int>>() != null);
            Contract.Ensures(Contract.Result<IReadOnlyList<int>>().Count == Hues);

            return InternalGetHueDistribution(0, 0);
        }

        public IReadOnlyList<int> GetHueDistribution(double minSaturation, double minValue)
        {
            Contract.Requires(minSaturation >= 0.0 && minSaturation <= 1.0);
            Contract.Requires(minValue >= 0.0 && minValue <= 1.0);
            Contract.Ensures(Contract.Result<IReadOnlyList<int>>() != null);
            Contract.Ensures(Contract.Result<IReadOnlyList<int>>().Count == Hues);

            var minS = GetSaturationIndex(minSaturation);
            var minV = GetValueIndex(minValue);

            return InternalGetHueDistribution(minS, minV);
        }

        public IReadOnlyList<int> GetSaturationDistribution(double? hue)
        {
            Contract.Requires(hue == null || hue >= 0.0 && hue < 360.0);
            Contract.Ensures(Contract.Result<IReadOnlyList<int>>() != null);
            Contract.Ensures(Contract.Result<IReadOnlyList<int>>().Count == Saturations);

            var saturations = Saturations;
            var values = Values;
            var distribution = new int[saturations];

            if (hue != null)
            {
                var indexH = GetHueIndex(hue.Value);

                Parallel.For(0, saturations, indexS =>
                {
                    var weight = 0;

                    for (var indexV = 0; indexV < values; indexV++)
                        weight += this.cube[indexH, indexS, indexV];

                    distribution[indexS] = weight;
                });
            }
            else
            {
                var hues = Hues;

                Parallel.For(0, saturations, indexS =>
                {
                    var weight = 0;

                    for (var indexH = 0; indexH < hues; indexH++)
                    {
                        for (var indexV = 0; indexV < values; indexV++)
                            weight += this.cube[indexH, indexS, indexV];
                    };

                    distribution[indexS] = weight;
                });
            }

            return distribution;
        }

        public IReadOnlyList<int> GetValueDistribution(double? hue)
        {
            Contract.Requires(hue == null || hue >= 0.0 && hue < 360.0);
            Contract.Ensures(Contract.Result<IReadOnlyList<int>>() != null);
            Contract.Ensures(Contract.Result<IReadOnlyList<int>>().Count == Values);

            var saturations = Saturations;
            var values = Values;
            var distribution = new int[values];

            if (hue != null)
            {
                var indexH = GetHueIndex(hue.Value);

                Parallel.For(0, values, indexV =>
                {
                    var weight = 0;

                    for (var indexS = 0; indexS < saturations; indexS++)
                        weight += this.cube[indexH, indexS, indexV];

                    distribution[indexV] = weight;
                });
            }
            else
            {
                var hues = Hues;

                Parallel.For(0, values, indexV =>
                {
                    var weight = 0;

                    for (var indexH = 0; indexH < hues; indexH++)
                    {
                        for (var indexS = 0; indexS < saturations; indexS++)
                            weight += this.cube[indexH, indexS, indexV];
                    }

                    distribution[indexV] = weight;
                });
            }

            return distribution;
        }

        public static ColorDistribution Add(ColorDistribution dist1, ColorDistribution dist2)
        {
            Contract.Requires(dist1 != null);
            Contract.Requires(dist2 != null);

            var result = new ColorDistribution(dist1.Hues, dist1.Saturations, dist1.Values);
            result.Add(dist1);
            result.Add(dist2);
            return result;
        }

        public static double GetCorrelationCoefficient(ColorDistribution dist1, ColorDistribution dist2)
        {
            Contract.Requires(dist1 != null);
            Contract.Requires(dist2 != null);
            Contract.Requires(dist1.Hues == dist2.Hues);
            Contract.Requires(dist1.Saturations == dist2.Saturations);
            Contract.Requires(dist1.Values == dist2.Values);

            var a = dist1.GetMostCommonColor();
            var b = dist2.GetMostCommonColor();

            // normalize deltas to 0..100
            var deltaH = (a.H - b.H) * 100.0 / 360.0;
            var deltaS = (a.S - b.S) * 100.0;
            var deltaV = (a.V - b.V) * 100.0;
            var distance = Math.Sqrt(deltaH * deltaH + deltaS * deltaS + deltaV * deltaV);
            const double maxDistance = 173.2051; //Math.Sqrt(100*100 + 100*100 + 100*100);
            return distance / maxDistance;
        }

        internal double HueGranularity
        {
            get { return Hues / 360.0; }
        }

        internal double SaturationGranularity
        {
            get { return Saturations; }
        }

        internal double ValueGranularity
        {
            get { return Values; }
        }

        internal void Add(ColorDistribution other)
        {
            Contract.Requires(other != null);
            Contract.Requires(other.Hues == Hues);
            Contract.Requires(other.Saturations == Saturations);
            Contract.Requires(other.Values == Values);

            var countH = Hues;
            var countS = Saturations;
            var countV = Values;

            Parallel.For(0, countH, indexH =>
            {
                for (var indexS = 0; indexS < countS; indexS++)
                {
                    for (var indexV = 0; indexV < countV; indexV++)
                        this.cube[indexH, indexS, indexV] += other.cube[indexH, indexS, indexV];
                }
            });
        }

        internal ColorDistribution AddPixel(ColorHsv hsv)
        {
            var indexH = GetHueIndex(hsv.H);
            var indexS = GetSaturationIndex(hsv.S);
            var indexV = GetValueIndex(hsv.V);

            this.cube[indexH, indexS, indexV]++;

            return this;
        }

        ///////////////////////////////////////////////////////////////////////

        ColorDistribution(int[,,] cube)
        {
            this.cube = cube;
        }

        int GetHueIndex(double h)
        {
            return (int)(h * HueGranularity);
        }

        int GetSaturationIndex(double s)
        {
            return (int)(s * (SaturationGranularity - 1));
        }

        int GetValueIndex(double v)
        {
            return (int)(v * (ValueGranularity - 1));
        }

        IReadOnlyList<int> InternalGetHueDistribution(int minS, int minV)
        {
            var hues = Hues;
            var saturations = Saturations;
            var values = Values;
            var distribution = new int[hues];

            Parallel.For(0, hues, indexH =>
            {
                var weight = 0;

                for (var indexS = minS; indexS < saturations; indexS++)
                {
                    for (var indexV = minV; indexV < values; indexV++)
                        weight += this.cube[indexH, indexS, indexV];
                }

                distribution[indexH] = weight;
            });

            return distribution;
        }
    }
}
