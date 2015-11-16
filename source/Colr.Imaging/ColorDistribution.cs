using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Colr.Imaging
{
    public class ColorDistribution
    {
        internal ColorDistribution(ColorHsv dominantColor, IReadOnlyList<int> distribution,
            IReadOnlyList<int> dominantHueSaturationDistribution, IReadOnlyList<int> dominantHueValueDistribution)
        {
            DominantColor = dominantColor;
            HueDistribution = distribution;
            DominantHueSaturationDistribution = dominantHueSaturationDistribution;
            DominantHueValueDistribution = dominantHueValueDistribution;
        }

        public ColorHsv DominantColor { get; }
        public IReadOnlyList<int> HueDistribution { get; }
        public IReadOnlyList<int> DominantHueSaturationDistribution { get; }
        public IReadOnlyList<int> DominantHueValueDistribution { get; }
    }
}
