using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Colr.Imaging
{
    public class ColorDistribution
    {
        internal ColorDistribution(double? dominantHue, int[] distribution,
            int[] dominantHueSaturationDistribution, int[] dominantHueValueDistribution)
        {
            DominantHue = dominantHue;
            HueDistribution = distribution;
        }

        public double? DominantHue { get; }
        public int[] HueDistribution { get; }
        public int[] DominantHueSaturationDistribution { get; }
        public int[] DominantHueValueDistribution { get; }
    }
}
