using System;
using System.Globalization;
using System.Runtime.InteropServices;

namespace Colr.Imaging
{
    [StructLayout(LayoutKind.Explicit)]
    public struct ColorArgb : IEquatable<ColorArgb>
    {
        public const int SizeOf = 4;

        [FieldOffset(0)]
        public readonly byte B;

        [FieldOffset(1)]
        public readonly byte G;

        [FieldOffset(2)]
        public readonly byte R;

        [FieldOffset(3)]
        public readonly byte A;

        [FieldOffset(0)]
        public readonly int Argb;

        public static readonly ColorArgb Zero = new ColorArgb(0);
        public static readonly ColorArgb Transparent = new ColorArgb(0x00ffffff);
        public static readonly ColorArgb Black = new ColorArgb(255, 0);
        public static readonly ColorArgb White = new ColorArgb(255, 0x00ffffff);

        public int Rgb
        {
            get { return Argb & 0x00ffffff; }
        }

        public double ScA
        {
            get { return (double)A / 255.0; }
        }

        public double ScR
        {
            get { return (double)R / 255.0; }
        }

        public double ScG
        {
            get { return (double)G / 255.0; }
        }

        public double ScB
        {
            get { return (double)B / 255.0; }
        }

        public double GetIntensity()
        {
            return (0.299 * R + 0.587 * G + 0.114 * B) * A / (255.0 * 255.0);
        }

        public ColorArgb Invert()
        {
            return new ColorArgb(A, (byte)(255 - R), (byte)(255 - G), (byte)(255 - B));
        }

        public ColorArgb NewAlpha(byte a)
        {
            return new ColorArgb(a, R, G, B);
        }

        public double GetHue()
        {
            double min;
            double max;

            return GetHue(out min, out max);
        }

        public static bool operator ==(ColorArgb argb1, ColorArgb argb2)
        {
            return argb1.Argb == argb2.Argb;
        }

        public static bool operator !=(ColorArgb argb1, ColorArgb argb2)
        {
            return argb1.Argb != argb2.Argb;
        }

        public static ColorArgb FromArgb(byte a, byte r, byte g, byte b)
        {
            return new ColorArgb(a, r, g, b);
        }

        public static ColorArgb FromArgb(int argb)
        {
            return new ColorArgb(argb);
        }

        public static ColorArgb FromHsv(byte a, ColorHsv hsv)
        {
            var h = hsv.H;
            var s = hsv.S;
            var v = hsv.V;

            var hi = (int)h / 60 % 6;
            var f = h / 60.0 - (double)hi;

            var r = 0.0;
            var g = 0.0;
            var b = 0.0;

            var p = v * (1.0 - s);
            var q = v * (1.0 - f * s);
            var t = v * (1.0 - (1.0 - f) * s);

            switch (hi)
            {
                case 0:
                    r = v;
                    g = t;
                    b = p;
                    break;
                case 1:
                    r = q;
                    g = v;
                    b = p;
                    break;
                case 2:
                    r = p;
                    g = v;
                    b = t;
                    break;
                case 3:
                    r = p;
                    g = q;
                    b = v;
                    break;
                case 4:
                    r = t;
                    g = p;
                    b = v;
                    break;
                case 5:
                    r = v;
                    g = p;
                    b = q;
                    break;
            }

            return ColorArgb.FromArgb(a, (byte)(r * 255.0 + 0.5), (byte)(g * 255.0 + 0.5), (byte)(b * 255.0 + 0.5));
        }

        public static ColorArgb Parse(string str)
        {
            byte a = 255;
            int index;
            byte r;
            byte g;
            byte b;

            if ((str.Length == 7 || str.Length == 9) && str[0] == '#')
            {
                try
                {
                    index = 1;

                    if (str.Length == 9)
                    {
                        a = byte.Parse(str.Substring(index, 2), NumberStyles.HexNumber);
                        index += 2;
                    }

                    r = byte.Parse(str.Substring(index, 2), NumberStyles.HexNumber);
                    g = byte.Parse(str.Substring(index + 2, 2), NumberStyles.HexNumber);
                    b = byte.Parse(str.Substring(index + 4, 2), NumberStyles.HexNumber);
                }
                catch (Exception innerException)
                {
                    throw new ArgumentException("Invalid Color Format", "str", innerException);
                }

                return ColorArgb.FromArgb(a, r, g, b);
            }

            var tokens = str.Split('-', ';', ',', ':');

            if (tokens.Length != 4 && tokens.Length != 3)
                throw new ArgumentException("Invalid Color Format", "str");

            index = 0;

            try
            {
                if (tokens.Length == 4)
                    a = byte.Parse(tokens[index++], NumberStyles.HexNumber);

                r = byte.Parse(tokens[index++], NumberStyles.HexNumber);
                g = byte.Parse(tokens[index++], NumberStyles.HexNumber);
                b = byte.Parse(tokens[index], NumberStyles.HexNumber);
            }
            catch (Exception innerException2)
            {
                throw new ArgumentException("Invalid Color Format", "sValue", innerException2);
            }

            return ColorArgb.FromArgb(a, r, g, b);
        }

        public static ColorArgb AlphaBlend(ColorArgb lower, ColorArgb upper)
        {
            var upperA = upper.ScA;
            var inverseUpperA = 1.0 - upperA;
            var lowerA = lower.ScA;
            var a = lowerA + (1.0 - lowerA) * upperA;

            return new ColorArgb(
                ClampDouble(255.0 * a),
                ClampDouble(inverseUpperA * (double)lower.R + upperA * (double)upper.R),
                ClampDouble(inverseUpperA * (double)lower.G + upperA * (double)upper.G),
                ClampDouble(inverseUpperA * (double)lower.B + upperA * (double)upper.B));
        }

        public static ColorArgb AlphaComposite(ColorArgb lower, ColorArgb upper)
        {
            var lowerA = lower.ScA;
            var upperA = upper.ScA;
            var inverseUpperA = 1.0 - upperA;
            var a = lowerA + (1.0 - lowerA) * upperA;

            return new ColorArgb(
                ClampDouble(255.0 * a),
                ClampDouble(((double)upper.R * upperA + (double)lower.R * lowerA * inverseUpperA) / a),
                ClampDouble(((double)upper.G * upperA + (double)lower.G * lowerA * inverseUpperA) / a),
                ClampDouble(((double)upper.B * upperA + (double)lower.B * lowerA * inverseUpperA) / a));
        }

        public override bool Equals(object obj)
        {
            return obj != null
                && obj.GetType() == typeof(ColorArgb)
                && ((ColorArgb)obj).Argb == Argb;
        }

        public override int GetHashCode()
        {
            return Argb;
        }

        public override string ToString()
        {
            return ToString(false);
        }

        public string ToString(bool isHtmlFormat)
        {
            return isHtmlFormat
                   ? String.Format("#{0:X2}{1:X2}{2:X2}", R, G, B)
                   : String.Format("{0:X2}-{1:X2}-{2:X2}-{3:X2}", A, R, G, B);
        }

        internal double GetHue(out double min, out double max)
        {
            var scR = ScR;
            var scG = ScG;
            var scB = ScB;

            max = Math.Max(scR, Math.Max(scG, scB));
            min = Math.Min(scR, Math.Min(scG, scB));

            double result;

            if (max == min)
            {
                result = 0.0;
            }
            else if (max == scR && scG >= scB)
            {
                result = 60.0 * (scG - scB) / (max - min) + 0.0;
            }
            else if (max == scR && scG < scB)
            {
                result = 60.0 * (scG - scB) / (max - min) + 360.0;
            }
            else if (max == scG)
            {
                result = 60.0 * (scB - scR) / (max - min) + 120.0;
            }
            else if (max == scB)
            {
                result = 60.0 * (scR - scG) / (max - min) + 240.0;
            }
            else
            {
                result = 0.0;
            }

            return result;
        }

        public bool Equals(ColorArgb other)
        {
            return Argb == other.Argb;
        }


        ///////////////////////////////////////////////////////////////////////

        private ColorArgb(byte a, byte r, byte g, byte b)
            : this()
        {
            A = a;
            R = r;
            G = g;
            B = b;
        }

        private ColorArgb(int argb)
            : this()
        {
            Argb = argb;
        }

        private ColorArgb(byte a, int rgb)
            : this()
        {
            Argb = rgb;
            A = a;
        }

        private static byte ClampDouble(double value)
        {
            if (value < 0.0)
            {
                value = 0.0;
            }
            else if (value > 255.0)
            {
                value = 255.0;
            }

            return (byte)(value + 0.5);
        }
    }
}
