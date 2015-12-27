using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Colr.Imaging
{
    public class ColorDistribution
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

        public ColorHsv GetDominantColor()
        {
            var maxWeight = 0;
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

        public IReadOnlyList<int> GetHueDistribution()
        {
            var hues = Hues;
            var saturations = Saturations;
            var values = Values;
            var distribution = new List<int>(hues);

            for (var indexH = 0; indexH < hues; indexH++)
            {
                var weight = 0;

                for (var indexS = 0; indexS < saturations; indexS++)
                {
                    for (var indexV = 0; indexV < values; indexV++)
                        weight += this.cube[indexH, indexS, indexV];
                }
            }

            Contract.Ensures(distribution.Count == Hues);
            return distribution;
        }

        public IReadOnlyList<int> GetSaturationDistribution(double hue)
        {
            var saturations = Saturations;
            var values = Values;
            var distribution = new List<int>(saturations);
            var indexH = (int)(hue * HueGranularity);

            for (var indexS = 0; indexS < saturations; indexS++)
            {
                var weight = 0;

                for (var indexV = 0; indexV < values; indexV++)
                    weight += this.cube[indexH, indexS, indexV];

                distribution.Add(weight);
            }

            Contract.Ensures(distribution.Count == Saturations);
            return distribution;
        }

        public IReadOnlyList<int> GetValueDistribution(double hue)
        {
            var saturations = Saturations;
            var values = Values;
            var distribution = new List<int>(values);
            var indexH = (int)(hue * HueGranularity);

            for (var indexV = 0; indexV < values; indexV++)
            {
                var weight = 0;

                for (var indexS = 0; indexS < saturations; indexS++)
                    weight += this.cube[indexH, indexS, indexV];

                distribution.Add(weight);
            }

            Contract.Ensures(distribution.Count == Values);
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

        internal void Add(ColorDistribution other)
        {
            Contract.Requires(other != null);
            Contract.Requires(other.Hues == Hues);
            Contract.Requires(other.Saturations == Saturations);
            Contract.Requires(other.Values == Values);

            var countH = Hues;
            var countS = Saturations;
            var countV = Values;

            for (var h = 0; h < countH; h++)
            {
                for (var s = 0; s < countS; s++)
                {
                    for (var v = 0; v < countV; v++)
                        this.cube[h, s, v] += other.cube[h, s, v];
                }
            }
        }

        internal void AddPixel(ColorHsv hsv)
        {
            var indexH = (int)(hsv.H * HueGranularity);
            var indexS = (int)(hsv.S * SaturationGranularity);
            var indexV = (int)(hsv.V * ValueGranularity);

            this.cube[indexH, indexS, indexV]++;
        }

        ///////////////////////////////////////////////////////////////////////

        ColorDistribution(int[,,] cube)
        {
            this.cube = cube;
        }

        double HueGranularity
        {
            get { return Hues / 360.0; }
        }

        double SaturationGranularity
        {
            get { return Saturations / 100.0; }
        }

        double ValueGranularity
        {
            get { return Values / 100.0; }
        }
    }
}
