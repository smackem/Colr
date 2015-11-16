using Colr.Imaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Colr.DesktopApp.Util
{
    static class ColorExtensions
    {
        public static Color ToColor(this ColorArgb argb)
        {
            return Color.FromArgb(argb.A, argb.R, argb.G, argb.B);
        }

        public static Color ToColor(this ColorHsv hsv, byte alpha)
        {
            var argb = ColorArgb.FromHsv(alpha, hsv);

            return ToColor(argb);
        }
    }
}
