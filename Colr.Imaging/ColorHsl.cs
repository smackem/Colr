using System;

namespace Lsb.Common.Drawing
{
    public struct ColorHsl : IComparable<ColorHsl>
    {
        private double h;
        private double s;
        private double l;

        public double Hue
        {
            get
            {
                return this.h;
            }
            set
            {
                if (value < 0.0 || value > 360.0)
                {
                    throw new ArgumentOutOfRangeException("value", "0.0 - 360.0");
                }
                this.h = value;
            }
        }

        public double Saturation
        {
            get
            {
                return this.s;
            }
            set
            {
                if (value < 0.0 || value > 1.0)
                {
                    throw new ArgumentOutOfRangeException("value", "0.0 - 1.0");
                }
                this.s = value;
            }
        }

        public double Lightness
        {
            get
            {
                return this.l;
            }
            set
            {
                if (value < 0.0 || value > 1.0)
                {
                    throw new ArgumentOutOfRangeException("value", "0.0 - 1.0");
                }
                this.l = value;
            }
        }

        internal ColorHsl(double hue, double saturation, double lightness)
        {
            this = default(ColorHsl);
            this.Hue = hue;
            this.Saturation = saturation;
            this.Lightness = lightness;
        }

        public void AddHue(double delta)
        {
            int num = (int)delta % 360;
            delta = (double)num + (delta - (double)((int)delta));
            this.h += delta;
            if (this.h < 0.0)
            {
                this.h += 360.0;
                return;
            }
            if (this.h > 360.0)
            {
                this.h -= 360.0;
            }
        }

        public void AddSaturation(double delta)
        {
            delta -= (double)((int)delta);
            this.s += delta;
            if (this.s < 0.0)
            {
                this.s = 0.0;
                return;
            }
            if (this.s > 1.0)
            {
                this.s = 1.0;
            }
        }

        public void AddLightness(double delta)
        {
            delta -= (double)((int)delta);
            this.l += delta;
            if (this.l < 0.0)
            {
                this.l = 0.0;
                return;
            }
            if (this.l > 1.0)
            {
                this.l = 1.0;
            }
        }

        public static ColorHsl FromHsl(double hue, double saturation, double lightness)
        {
            return new ColorHsl(hue, saturation, lightness);
        }

        public static ColorHsl FromRgb(ColorArgb rgb)
        {
            double num;
            double num2;
            double hue = rgb.GetHue(out num, out num2);
            double num3 = (num2 + num) / 2.0;
            double saturation;
            if (num3 == 0.0 || num == num2)
            {
                saturation = 0.0;
            }
            else if (num3 > 0.0 && num3 <= 0.5)
            {
                saturation = (num2 - num) / (num2 + num);
            }
            else if (num3 > 0.5)
            {
                saturation = (num2 - num) / (2.0 - (num2 + num));
            }
            else
            {
                saturation = 0.0;
            }
            return new ColorHsl(hue, saturation, num3);
        }

        public override bool Equals(object obj)
        {
            if (obj == null || base.GetType() != obj.GetType())
            {
                return false;
            }
            ColorHsl colorHsl = (ColorHsl)obj;
            return colorHsl.h == this.h && colorHsl.s == this.s && colorHsl.l == this.l;
        }

        public override int GetHashCode()
        {
            return (int)this.h << 16 | (int)(this.s * 255.0) << 8 | (int)(this.l * 255.0);
        }

        public static bool operator ==(ColorHsl hsl1, ColorHsl hsl2)
        {
            return hsl1.Equals(hsl2);
        }

        public static bool operator !=(ColorHsl hsl1, ColorHsl hsl2)
        {
            return !hsl1.Equals(hsl2);
        }

        public override string ToString()
        {
            return string.Format("H:{000.00} S:{1.000} L:{2.000}", this.h, this.s, this.l);
        }

        public int CompareTo(ColorHsl other)
        {
            if (this.h > other.h)
            {
                return 1;
            }
            if (this.h < other.h)
            {
                return -1;
            }
            if (this.s > other.s)
            {
                return 1;
            }
            if (this.s < other.s)
            {
                return -1;
            }
            if (this.l > other.l)
            {
                return 1;
            }
            if (this.l < other.l)
            {
                return -1;
            }
            return 0;
        }
    }
}
