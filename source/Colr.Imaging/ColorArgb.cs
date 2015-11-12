using System;
using System.Globalization;
using System.Runtime.InteropServices;

namespace Colr.Imaging
{
    /// <summary>
    /// An immutable color class that stores color information in a 32 bit word
    /// containing the alpha, red, green and blue channels.
    /// </summary>
    /// <remarks>
    /// The memory layout of the struct is actually BGRA. One <see cref="ColorArgb"/> matches
    /// one pixel of an image encoded in Bgra32 format.
    /// </remarks>
    [StructLayout(LayoutKind.Explicit)]
    public struct ColorArgb : IEquatable<ColorArgb>
    {
        /// <summary>
        /// The size of this struct.
        /// </summary>
        public const int SizeOf = 4;

        /// <summary>
        /// The value of the blue channel (0..255).
        /// </summary>
        [FieldOffset(0)]
        public readonly byte B;

        /// <summary>
        /// The value of the green channel (0..255).
        /// </summary>
        [FieldOffset(1)]
        public readonly byte G;

        /// <summary>
        /// The value of the red channel (0..255).
        /// </summary>
        [FieldOffset(2)]
        public readonly byte R;

        /// <summary>
        /// The value of the alpha channel (0..255).
        /// </summary>
        [FieldOffset(3)]
        public readonly byte A;

        /// <summary>
        /// A 32 bit word containing all channel values.
        /// </summary>
        [FieldOffset(0)]
        public readonly int Argb;

        /// <summary>
        /// A constant that equals an uninitialized instance of <see cref="ColorArgb"/>.
        /// </summary>
        public static readonly ColorArgb Zero = new ColorArgb(0);

        /// <summary>
        /// A constant that holds the color transparent (A = 0, all other channels = 255).
        /// </summary>
        public static readonly ColorArgb Transparent = new ColorArgb(0x00ffffff);

        /// <summary>
        /// A constant that holds the color black (A = 255, all other channels = 0).
        /// </summary>
        public static readonly ColorArgb Black = new ColorArgb(255, 0);

        /// <summary>
        /// A constant that holds the color white (all channels = 255).
        /// </summary>
        public static readonly ColorArgb White = new ColorArgb(255, 0x00ffffff);

        /// <summary>
        /// Gets the value of the alpha channel (0.0 .. 1.0).
        /// </summary>
        public double ScA
        {
            get { return A / 255.0; }
        }

        /// <summary>
        /// Gets the value of the red channel (0.0 .. 1.0).
        /// </summary>
        public double ScR
        {
            get { return R / 255.0; }
        }

        /// <summary>
        /// Gets the value of the green channel (0.0 .. 1.0).
        /// </summary>
        public double ScG
        {
            get { return G / 255.0; }
        }

        /// <summary>
        /// Gets the value of the blue channel (0.0 .. 1.0).
        /// </summary>
        public double ScB
        {
            get { return B / 255.0; }
        }

        /// <summary>
        /// Gets the color intensity (0.0 .. 1.0).
        /// This is the weighted lightness that takes different
        /// channel brightnesses into account.
        /// </summary>
        /// <returns>The color intensity.</returns>
        public double GetIntensity()
        {
            return (0.299 * R + 0.587 * G + 0.114 * B) * A / (255.0 * 255.0);
        }

        /// <summary>
        /// Gets a <see cref="ColorArgb"/> instance with all channels inverted.
        /// </summary>
        /// <returns>A <see cref="ColorArgb"/> instance with all channels inverted.</returns>
        public ColorArgb Invert()
        {
            return new ColorArgb(A, (byte)(255 - R), (byte)(255 - G), (byte)(255 - B));
        }

        /// <summary>
        /// Gets a <see cref="ColorArgb"/> instance that is identical to the current
        /// instance but with a new value for the alpha channel.
        /// </summary>
        /// <param name="a">The new alpha</param>
        /// <returns>a <see cref="ColorArgb"/> instance that is identical to the current
        /// instance but with a new value for the alpha channel.</returns>
        public ColorArgb NewAlpha(byte a)
        {
            return new ColorArgb(a, R, G, B);
        }

        /// <summary>
        /// Gets the hue.
        /// </summary>
        /// <returns>The hue (0.0 .. 360 exclusive).</returns>
        public double GetHue()
        {
            double min;
            double max;

            return GetHue(out min, out max);
        }

        /// <summary>
        /// Compares to <see cref="ColorArgb"/> instances for equality. 
        /// </summary>
        /// <param name="argb1">The first instance.</param>
        /// <param name="argb2">THe second instance.</param>
        public static bool operator ==(ColorArgb argb1, ColorArgb argb2)
        {
            return argb1.Argb == argb2.Argb;
        }

        /// <summary>
        /// Compares to <see cref="ColorArgb"/> instances for inequality. 
        /// </summary>
        /// <param name="argb1">The first instance.</param>
        /// <param name="argb2">THe second instance.</param>
        public static bool operator !=(ColorArgb argb1, ColorArgb argb2)
        {
            return argb1.Argb != argb2.Argb;
        }

        /// <summary>
        /// Creates a new instance of <see cref="ColorArgb"/> from individual component values.
        /// </summary>
        /// <param name="a">The alpha component.</param>
        /// <param name="r">The red component.</param>
        /// <param name="g">The green component.</param>
        /// <param name="b">The blue component.</param>
        /// <returns>The new instance.</returns>
        public static ColorArgb FromArgb(byte a, byte r, byte g, byte b)
        {
            return new ColorArgb(a, r, g, b);
        }

        /// <summary>
        /// Creates a new instance of <see cref="ColorArgb"/> from an integer containing
        /// all component values.
        /// </summary>
        /// <param name="argb">An integer containing all component values. <see cref="ColorArgb.Argb"/>.</param>
        /// <returns>The new instance.</returns>
        public static ColorArgb FromArgb(int argb)
        {
            return new ColorArgb(argb);
        }

        /// <summary>
        /// Creates a new instance of <see cref="ColorArgb"/> from a
        /// HSV (hue-saturation-value) color.
        /// </summary>
        /// <param name="a">The alpha component for the new <see cref="ColorAgb"/>.</param>
        /// <param name="hsv">The <see cref="ColorHsv"/> containing the hue, saturation and value
        /// components.</param>
        /// <returns>The new <see cref="ColorArgb"/> instance.</returns>
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

        /// <summary>
        /// Parse the specified string.
        /// Supported string formats are #RRGGBB, #AARRGGBB,
        /// RR-GG-BB, AA-RR-GG-BB (each in hex).
        /// </summary>
        /// <param name="str">The string representation of a color.</param>
        /// <exception cref="ArgumentException">Thrown when specified string
        /// is not in a supported format.</exception>
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

        /// <summary>
        /// Builds the result of a color composition that paints one color
        /// over the other, taking into account alpha values.
        /// </summary>
        /// <param name="lower">Lower color.</param>
        /// <param name="upper">Upper color.</param>
        /// <returns>The color that results from the composition.</returns>
        public static ColorArgb AlphaComposite(ColorArgb lower, ColorArgb upper)
        {
            var lowerA = lower.ScA;
            var upperA = upper.ScA;
            var inverseUpperA = 1.0 - upperA;
            var a = lowerA + (1.0 - lowerA) * upperA;

            return new ColorArgb(
                ClampDouble(255.0 * a),
                ClampDouble((upper.R * upperA + lower.R * lowerA * inverseUpperA) / a),
                ClampDouble((upper.G * upperA + lower.G * lowerA * inverseUpperA) / a),
                ClampDouble((upper.B * upperA + lower.B * lowerA * inverseUpperA) / a));
        }

        /// <summary>
        /// Determines whether the specified <see cref="System.Object"/> is equal to the current <see cref="Colr.Imaging.ColorArgb"/>.
        /// </summary>
        /// <param name="obj">The <see cref="System.Object"/> to compare with the current <see cref="Colr.Imaging.ColorArgb"/>.</param>
        /// <returns><c>true</c> if the specified <see cref="System.Object"/> is equal to the current
        /// <see cref="Colr.Imaging.ColorArgb"/>; otherwise, <c>false</c>.</returns>
        public override bool Equals(object obj)
        {
            return obj != null
                && obj.GetType() == typeof(ColorArgb)
                && ((ColorArgb)obj).Argb == Argb;
        }

        /// <summary>
        /// Serves as a hash function for a <see cref="Colr.Imaging.ColorArgb"/> object.
        /// </summary>
        /// <returns>A hash code for this instance that is suitable for use in hashing algorithms and data structures such as a
        /// hash table.</returns>
        public override int GetHashCode()
        {
            return Argb;
        }

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents the current <see cref="Colr.Imaging.ColorArgb"/>.
        /// </summary>
        /// <returns>A <see cref="System.String"/> that represents the current <see cref="Colr.Imaging.ColorArgb"/>.</returns>
        public override string ToString()
        {
            return ToString(false);
        }

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents the current <see cref="Colr.Imaging.ColorArgb"/>.
        /// </summary>
        /// <returns>A <see cref="System.String"/> that represents the current <see cref="Colr.Imaging.ColorArgb"/>.</returns>
        /// <param name="isHtmlFormat">If set to <c>true</c> returns html format, excluding the alpha channel.</param>
        public string ToString(bool isHtmlFormat)
        {
            return isHtmlFormat
                   ? String.Format("#{0:X2}{1:X2}{2:X2}", R, G, B)
                   : String.Format("{0:X2}-{1:X2}-{2:X2}-{3:X2}", A, R, G, B);
        }

        /// <summary>
        /// Determines whether the specified <see cref="Colr.Imaging.ColorArgb"/> is equal to the current <see cref="Colr.Imaging.ColorArgb"/>.
        /// </summary>
        /// <param name="other">The <see cref="Colr.Imaging.ColorArgb"/> to compare with the current <see cref="Colr.Imaging.ColorArgb"/>.</param>
        /// <returns><c>true</c> if the specified <see cref="Colr.Imaging.ColorArgb"/> is equal to the current
        /// <see cref="Colr.Imaging.ColorArgb"/>; otherwise, <c>false</c>.</returns>
        public bool Equals(ColorArgb other)
        {
            return Argb == other.Argb;
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


        ///////////////////////////////////////////////////////////////////////

        ColorArgb(byte a, byte r, byte g, byte b)
            : this()
        {
            A = a;
            R = r;
            G = g;
            B = b;
        }

        ColorArgb(int argb)
            : this()
        {
            Argb = argb;
        }

        ColorArgb(byte a, int rgb)
            : this()
        {
            Argb = rgb;
            A = a;
        }

        static byte ClampDouble(double value)
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
