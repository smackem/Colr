using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Colr.Imaging
{
    public class ColorDistribution
    {
        internal ColorDistribution(ColorHsv dominantColor, int[] distribution,
            int[] dominantHueSaturationDistribution, int[] dominantHueValueDistribution)
        {
            DominantColor = dominantColor;
            HueDistribution = distribution;
            DominantHueSaturationDistribution = dominantHueSaturationDistribution;
            DominantHueValueDistribution = dominantHueValueDistribution;
        }

        public ColorHsv DominantColor { get; }
        public int[] HueDistribution { get; }
        public int[] DominantHueSaturationDistribution { get; }
        public int[] DominantHueValueDistribution { get; }
    }
}
