using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Colr.Imaging
{
    public class HueDistribution
    {
        internal HueDistribution(double? dominantHue, int[] distribution)
        {
            DominantHue = dominantHue;
            Distribution = distribution;
        }

        public double? DominantHue { get; }
        public int[] Distribution { get; }
    }
}
