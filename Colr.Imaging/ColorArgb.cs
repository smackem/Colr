using System;
using System.ComponentModel;
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

        public static readonly ColorArgb Zero = ColorArgb.FromArgb(0);
        public static readonly ColorArgb Transparent = ColorArgb.FromArgb(0x00ffffff);
        public static readonly ColorArgb Black = ColorArgb.FromRgb(0);
        public static readonly ColorArgb White = ColorArgb.FromRgb(0x00ffffff);

        public int Rgb
        {
            get { return Argb & 0x00FFFFFF; }
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

        public byte PaR
        {
            get { return ColorArgb.ClampDouble(ScA * (double)R); }
        }

        public byte PaG
        {
            get { return ColorArgb.ClampDouble(ScA * (double)G); }
        }

        public byte PaB
        {
            get { return ColorArgb.ClampDouble(ScA * (double)B); }
        }

        public int Pargb
        {
            get
            {
                var scA = ScA;
                return (int)A << 24 | (int)ColorArgb.ClampDouble(scA * (double)R) << 16 | (int)ColorArgb.ClampDouble(scA * (double)G) << 8 | (int)ColorArgb.ClampDouble(scA * (double)B);
            }
        }

        public double GetIntensity()
        {
            return (0.299 * R + 0.587 * G + 0.114 * B) * A / (double)(0xFF * 0xFF);
        }

        public byte GetIntensityByte()
        {
            return (byte)((0.299f * (float)R + 0.587f * (float)G + 0.114f * (float)B) * (float)A / 255f + 0.5f);
        }

        public ColorArgb Invert()
        {
            return ColorArgb.FromArgb(A, (byte)(255 - R), (byte)(255 - G), (byte)(255 - B));
        }

        public ColorArgb NewAlpha(byte alpha)
        {
            return ColorArgb.FromArgb(alpha, Argb);
        }

        public double GetHue()
        {
            double num;
            double num2;
            return GetHue(out num, out num2);
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

        public static ColorArgb FromArgb(byte a, int rgb)
        {
            return new ColorArgb(a, rgb);
        }

        public static ColorArgb FromRgb(int rgb)
        {
            return new ColorArgb(255, rgb);
        }

        public static ColorArgb FromRgb(byte r, byte g, byte b)
        {
            return new ColorArgb(255, r, g, b);
        }

        public static ColorArgb FromHsv(byte a, ColorHsv hsv)
        {
            double h = hsv.Hue;
            double s = hsv.Saturation;
            double v = hsv.Value;
            int num = (int)h / 60 % 6;
            double num2 = 0.0;
            double num3 = 0.0;
            double num4 = 0.0;
            double num5 = h / 60.0 - (double)num;
            double num6 = v * (1.0 - s);
            double num7 = v * (1.0 - num5 * s);
            double num8 = v * (1.0 - (1.0 - num5) * s);

            switch (num)
            {
                case 0:
                    num2 = v;
                    num3 = num8;
                    num4 = num6;
                    break;
                case 1:
                    num2 = num7;
                    num3 = v;
                    num4 = num6;
                    break;
                case 2:
                    num2 = num6;
                    num3 = v;
                    num4 = num8;
                    break;
                case 3:
                    num2 = num6;
                    num3 = num7;
                    num4 = v;
                    break;
                case 4:
                    num2 = num8;
                    num3 = num6;
                    num4 = v;
                    break;
                case 5:
                    num2 = v;
                    num3 = num6;
                    num4 = num7;
                    break;
            }

            return ColorArgb.FromArgb(a, (byte)(num2 * 255.0 + 0.5), (byte)(num3 * 255.0 + 0.5), (byte)(num4 * 255.0 + 0.5));
        }

        public static ColorArgb FromHsl(byte nAlpha, ColorHsl hsl)
        {
            double h = hsl.Hue;
            double s = hsl.Saturation;
            double l = hsl.Lightness;
            double num = (l < 0.5) ? (l * (1.0 + s)) : (l + s - l * s);
            double num2 = 2.0 * l - num;
            double num3 = h / 360.0;
            double[] array = new double[]
            {
                num3 + 0.33333333333333331,
                num3,
                num3 - 0.33333333333333331
            };
            for (int i = 0; i < array.Length; i++)
            {
                if (array[i] < 0.0)
                {
                    array[i] += 1.0;
                }
                else if (array[i] > 1.0)
                {
                    array[i] -= 1.0;
                }
            }
            double[] array2 = new double[3];
            for (int j = 0; j < array2.Length; j++)
            {
                if (array[j] < 0.16666666666666666)
                {
                    array2[j] = num2 + (num - num2) * 6.0 * array[j];
                }
                else if (array[j] >= 0.16666666666666666 && array[j] < 0.5)
                {
                    array2[j] = num;
                }
                else if (array[j] >= 0.5 && array[j] < 0.66666666666666663)
                {
                    array2[j] = num2 + (num - num2) * (0.66666666666666663 - array[j]) * 6.0;
                }
                else
                {
                    array2[j] = num2;
                }
            }
            return ColorArgb.FromArgb(nAlpha, (byte)(array2[0] * 255.0 + 0.5), (byte)(array2[1] * 255.0 + 0.5), (byte)(array2[2] * 255.0 + 0.5));
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
            if (isHtmlFormat)
                return "#" + R.ToString("X2") + G.ToString("X2") + B.ToString("X2");

            return A.ToString("X2")
            + "-"
            + R.ToString("X2")
            + "-"
            + G.ToString("X2")
            + "-"
            + B.ToString("X2");
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

        #region IEquatable implementation

        public bool Equals(ColorArgb other)
        {
            return Argb == other.Argb;
        }

        #endregion


        /////////////////////////////////////////////////////////////////////////////

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
