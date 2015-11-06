using System;

namespace Colr.Imaging
{
    public struct ColorHsv : IEquatable<ColorHsv>
    {
        public readonly double H;
        public readonly double S;
        public readonly double V;

        public static ColorHsv FromHsv(double h, double s, double v)
        {
            return new ColorHsv(h, s, v);
        }

        public static ColorHsv FromRgb(ColorArgb rgb)
        {
            double min;
            double max;
            var h = rgb.GetHue(out min, out max);
            var s = (max == 0.0) ? 0.0 : (1.0 - min / max);
            var v = max;

            return new ColorHsv(h, s, v);
        }

        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
                return false;

            return Equals((ColorHsv)obj);
        }

        public override int GetHashCode()
        {
            return (int)H << 16 | (int)(S * 255.0) << 8 | (int)(V * 255.0);
        }

        public bool Equals(ColorHsv other)
        {
            return other.H == H && other.S == S && other.V == V;
        }

        public static bool operator ==(ColorHsv hsv1, ColorHsv hsv2)
        {
            return hsv1.Equals(hsv2);
        }

        public static bool operator !=(ColorHsv hsv1, ColorHsv hsv2)
        {
            return hsv1.Equals(hsv2) == false;
        }

        public override string ToString()
        {
            return string.Format("H:{000.00} S:{1.000} V:{2.000}", H, S, V);
        }


        ///////////////////////////////////////////////////////////////////////

        private ColorHsv(double h, double s, double v)
        : this()
        {
            if (h < 0.0 || h > 360.0)
                throw new ArgumentOutOfRangeException("value", "0.0 - 360.0");
            if (s < 0.0 || s > 1.0)
                throw new ArgumentOutOfRangeException("value", "0.0 - 1.0");
            if (v < 0.0 || v > 1.0)
                throw new ArgumentOutOfRangeException("value", "0.0 - 1.0");

            this.H = h;
            this.S = s;
            this.V = v;
        }
    }
}
