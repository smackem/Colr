using System;
using System.Globalization;

namespace Colr.Imaging
{
    /// <summary>
    /// An immutable color class that stores color information in HSV color
    /// space, storing the hue, saturation and value channels with
    /// double precision.
    /// </summary>
    public struct ColorHsv : IEquatable<ColorHsv>
    {
        /// <summary>
        /// The hue from 0.0 to 360.0 exclusively.
        /// </summary>
        public readonly double H;

        /// <summary>
        /// The saturation from 0.0 through 1.0 inclusively.
        /// </summary>
        public readonly double S;

        /// <summary>
        /// The value from 0.0 through 1.0 inclusively.
        /// </summary>
        public readonly double V;

        /// <summary>
        /// Creates a new instance of <see cref="ColorHsv"/> from the
        /// given hue, saturation and value.
        /// </summary>
        /// <param name="h">The hue from 0.0 to 360.0 exclusively.</param>
        /// <param name="s">The saturation from 0.0 through 1.0 inclusively.</param>
        /// <param name="v">The value from 0.0 through 1.0 inclusively.</param>
        /// <returns>A new instance of <see cref="ColorHsv"/>.</returns>
        public static ColorHsv FromHsv(double h, double s, double v)
        {
            return new ColorHsv(h, s, v);
        }

        /// <summary>
        /// Creates a new instance of <see cref="ColorHsv"/> from the
        /// given hue <see cref="ColorArgb"/>. The new HSV color will
        /// represent the same color as <paramref name="rgb"/> but
        /// in HSV color space.
        /// </summary>
        /// <param name="rgb">The <see cref="ColorArgb"/> to create
        /// the <see cref="ColorHsv"/> from.</param>
        /// <returns>A new instance of <see cref="ColorHsv"/>.</returns>
        public static ColorHsv FromRgb(ColorArgb rgb)
        {
            double min;
            double max;
            var h = rgb.GetHue(out min, out max);
            var s = (max == 0.0) ? 0.0 : (1.0 - min / max);
            var v = max;

            return new ColorHsv(h, s, v);
        }

        /// <summary>
        /// Determines whether the specified <see cref="System.Object" />, is equal to this instance.
        /// </summary>
        /// <param name="obj">The <see cref="System.Object" /> to compare with this instance.</param>
        /// <returns>
        ///   <c>true</c> if the specified <see cref="System.Object" /> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
                return false;

            return Equals((ColorHsv)obj);
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
        /// </returns>
        public override int GetHashCode()
        {
            return (int)H << 16 | (int)(S * 255.0) << 8 | (int)(V * 255.0);
        }

        /// <summary>
        /// Determines whether the specified <see cref="ColorHsv" /> is equal to this instance.
        /// </summary>
        /// <param name="obj">The <see cref="ColorHsv" /> to compare with this instance.</param>
        /// <returns>
        ///   <c>true</c> if the specified <see cref="ColorHsv" /> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        public bool Equals(ColorHsv other)
        {
            return other.H == H && other.S == S && other.V == V;
        }

        /// <summary>
        /// Implements the operator ==, which compares two
        /// <see cref="ColorHsv"/> instances for equality. 
        /// </summary>
        /// <param name="hsv1">The first instance.</param>
        /// <param name="hsv2">The second instance.</param>
        /// <returns><c>true</c> if the two instances are equal; otherwise <c>false</c>.</returns>
        public static bool operator ==(ColorHsv hsv1, ColorHsv hsv2)
        {
            return hsv1.Equals(hsv2);
        }

        /// <summary>
        /// Implements the operator !=, which compares two
        /// <see cref="ColorHsv"/> instances for inequality. 
        /// </summary>
        /// <param name="hsv1">The first instance.</param>
        /// <param name="hsv2">The second instance.</param>
        /// <returns><c>true</c> if the two instances are not equal; otherwise <c>false</c>.</returns>
        public static bool operator !=(ColorHsv hsv1, ColorHsv hsv2)
        {
            return hsv1.Equals(hsv2) == false;
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture,
                "H:{0,6:F2} S:{1,4:F2} V:{2,4:F2}", H, S, V);
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
