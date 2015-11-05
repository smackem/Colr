using System;

namespace Colr.Imaging
{
    public struct ColorHsv
    {
        private double h;
        private double s;
        private double v;

        public double Hue
        {
            get { return this.h; }
            set
            {
                if (value < 0.0 || value > 360.0)
                    throw new ArgumentOutOfRangeException("value", "0.0 - 360.0");

                this.h = value;
            }
        }

        public double Saturation
        {
            get { return this.s; }
            set
            {
                if (value < 0.0 || value > 1.0)
                    throw new ArgumentOutOfRangeException("value", "0.0 - 1.0");

                this.s = value;
            }
        }

        public double Value
        {
            get { return this.v; }
            set
            {
                if (value < 0.0 || value > 1.0)
                    throw new ArgumentOutOfRangeException("value", "0.0 - 1.0");

                this.v = value;
            }
        }

        internal ColorHsv(double hue, double saturation, double value)
        {
            this = default(ColorHsv);
            this.Hue = hue;
            this.Saturation = saturation;
            this.Value = value;
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
                this.h -= 360.0;
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
                this.s = 1.0;
        }

        public void AddValue(double delta)
        {
            delta -= (double)((int)delta);
            this.v += delta;

            if (this.v < 0.0)
            {
                this.v = 0.0;
                return;
            }

            if (this.v > 1.0)
                this.v = 1.0;
        }

        public static ColorHsv FromHsv(double hue, double saturation, double value)
        {
            return new ColorHsv(hue, saturation, value);
        }

        public static ColorHsv FromRgb(ColorArgb rgb)
        {
            double num;
            double num2;
            double hue = rgb.GetHue(out num, out num2);
            double saturation = (num2 == 0.0) ? 0.0 : (1.0 - num / num2);
            double value = num2;
            return new ColorHsv(hue, saturation, value);
        }

        public override bool Equals(object obj)
        {
            if (obj == null || base.GetType() != obj.GetType())
                return false;

            var peer = (ColorHsv)obj;
            return peer.h == this.h && peer.s == this.s && peer.v == this.v;
        }

        public override int GetHashCode()
        {
            return (int)this.h << 16 | (int)(this.s * 255.0) << 8 | (int)(this.v * 255.0);
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
            return string.Format("H:{000.00} S:{1.000} V:{2.000}", this.h, this.s, this.v);
        }
    }
}
