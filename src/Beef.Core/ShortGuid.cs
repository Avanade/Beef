// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using System;

namespace Beef
{
#pragma warning disable CA1036 // Override methods on comparable types; by-design, do not want to directly support.
    /// <summary>
    /// Represents a URL friendly Base-64 encoded <see cref="System.Guid"/>.
    /// </summary>
    /// <remarks>For example a GUID with a value of 'fc5ab925-2418-4c53-bb76-de5296f1f5ef' would be represented as 'Jbla_BgkU0y7dt5SlvH17w'.</remarks>
    public struct ShortGuid : IComparable, IComparable<ShortGuid>, IComparable<Guid>, IEquatable<ShortGuid>
#pragma warning restore CA1036
    {
        // Base64 encoding guidance thanks to: http://madskristensen.net/post/A-shorter-and-URL-friendly-GUID

        private const string _formatError = "Input string must be a Base64 encoded GUID.";
        private Guid _guid;

        /// <summary>
        /// Creates a new <see cref="ShortGuid"/>.
        /// </summary>
        /// <returns></returns>
        public static ShortGuid NewGuid()
        {
            return new ShortGuid(Guid.NewGuid());
        }

        /// <summary>
        /// Gets a read-only instance of an empty <see cref="ShortGuid"/> (<seealso cref="Guid.Empty"/>). 
        /// </summary>
        public static readonly ShortGuid Empty = new ShortGuid();

        /// <summary>
        /// Initializes a new instance of the <see cref="ShortGuid"/> structure from a Base64 encoded GUID.
        /// </summary>
        /// <param name="g">The Base64 encoded GUID.</param>
        public ShortGuid(string g)
        {
            Check.NotNull(g, nameof(g));
            if (g.Length != 22)
                throw new FormatException(_formatError);

            try
            {
                _guid = new Guid(Convert.FromBase64String(g.Replace("_", "/", StringComparison.InvariantCulture).Replace("-", "+", StringComparison.InvariantCulture) + "=="));
            }
            catch (FormatException fe)
            {
                throw new FormatException(_formatError, fe);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ShortGuid"/> structure from a <see cref="System.Guid"/>.
        /// </summary>
        /// <param name="g">The <see cref="Guid"/>.</param>
        public ShortGuid(Guid g)
        {
            Check.NotNull(g, nameof(g));
            _guid = g;
        }

        /// <summary>
        /// Indicates whether the values of two specified <see cref="ShortGuid"/> values are equal.
        /// </summary>
        /// <param name="a">The first <see cref="ShortGuid"/> value.</param>
        /// <param name="b">The second <see cref="ShortGuid"/> value.</param>
        /// <returns><c>true</c> if <paramref name="a"/> and <paramref name="b"/> are equal; otherwise, <c>false</c>.</returns>
        public static bool operator ==(ShortGuid a, ShortGuid b) => (a._guid == b._guid);

        /// <summary>
        /// Indicates whether the values of two specified <see cref="ShortGuid"/> values are not equal.
        /// </summary>
        /// <param name="a">The first <see cref="ShortGuid"/> value.</param>
        /// <param name="b">The second <see cref="ShortGuid"/> value.</param>
        /// <returns><c>true</c> if <paramref name="a"/> and <paramref name="b"/> are not equal; otherwise, <c>false</c>.</returns>
        public static bool operator !=(ShortGuid a, ShortGuid b) => !(a == b);

        /// <summary>
        /// Converts a <see cref="ShortGuid"/> to a <see cref="System.Guid"/>.
        /// </summary>
        /// <param name="value">The <see cref="ShortGuid"/>.</param>
        /// <returns>The corresponding <see cref="Guid"/>.</returns>
        public static implicit operator Guid(ShortGuid value)
        {
            Check.NotNull(value, nameof(value));
            return value._guid;
        }

        /// <summary>
        /// Converts the <see cref="ShortGuid"/> to a <see cref="System.Guid"/>.
        /// </summary>
        /// <returns>The corresponding <see cref="Guid"/>.</returns>
        public Guid ToGuid() => _guid;

        /// <summary>
        /// Converts a <see cref="ShortGuid"/> to a Base64 encoded GUID <see cref="string"/>.
        /// </summary>
        /// <param name="value">The <see cref="ShortGuid"/>.</param>
        /// <returns>The corresponding Base64 encoded GUID <see cref="string"/>.</returns>
        public static implicit operator string(ShortGuid value)
        {
            Check.NotNull(value, nameof(value));
            return value.ToString();
        }

        /// <summary>
        /// Returns a value that indicates whether this instance is equal to a specified object.
        /// </summary>
        /// <param name="o">The object to compare with this instance.</param>
        /// <returns><c>true</c> if <paramref name="o"/> is a <see cref="ShortGuid"/> that has the same value as this instance; otherwise, <c>false</c>.</returns>
        public override bool Equals(object o)
        {
            if (o == null)
                return false;

            if (o is ShortGuid)
                return Equals((ShortGuid)o);

            if (o is Guid)
                return Equals((Guid)o);

            return false;
        }

        /// <summary>
        /// Returns a value that indicates whether this <see cref="ShortGuid"/> instance is equal to a specified <see cref="ShortGuid"/>.
        /// </summary>
        /// <param name="sg">The <see cref="ShortGuid"/> to compare with this instance.</param>
        /// <returns><c>true</c> if <paramref name="sg"/> has the same value as this instance; otherwise, <c>false</c>.</returns>
        public bool Equals(ShortGuid sg)
        {
            return this == sg;
        }

        /// <summary>
        /// Returns a value that indicates whether this <see cref="ShortGuid"/> instance is equal to a specified <see cref="Guid"/>.
        /// </summary>
        /// <param name="sg">The <see cref="Guid"/> to compare with this instance.</param>
        /// <returns><c>true</c> if <paramref name="sg"/> has the same value as this instance; otherwise, <c>false</c>.</returns>
        public bool Equals(Guid sg)
        {
            return this == sg;
        }

        /// <summary>
        /// Compares this instance to a specified object and returns an indication of their relative values.
        /// </summary>
        /// <param name="value">An object to compare, or null.</param>
        /// <returns>A value that indicates the relative order of the objects being compared (see <see cref="IComparable.CompareTo"/>).</returns>
        public int CompareTo(object value)
        {
            if (value == null)
                return 1;

            if (value is ShortGuid)
            {
                return _guid.CompareTo(((ShortGuid)value)._guid);
            }

            if (value is Guid)
            {
                return _guid.CompareTo((Guid)value);
            }

            throw new ArgumentException("Value must be a ShortGuid or Guid");
        }

        /// <summary>
        /// Compares this instance to a specified <see cref="Guid"/> and returns an indication of their relative values.
        /// </summary>
        /// <param name="value">The <see cref="Guid"/> to compare..</param>
        /// <returns>A value that indicates the relative order of the objects being compared (see <see cref="IComparable.CompareTo"/>).</returns>
        public int CompareTo(Guid value)
        {
            return _guid.CompareTo(value);
        }

        /// <summary>
        /// Compares this instance to a specified <see cref="ShortGuid"/> and returns an indication of their relative values.
        /// </summary>
        /// <param name="value">The <see cref="ShortGuid"/> to compare.</param>
        /// <returns>A value that indicates the relative order of the objects being compared (see <see cref="IComparable.CompareTo"/>).</returns>
        public int CompareTo(ShortGuid value)
        {
            if (value == null)
                return 1;

            return _guid.CompareTo(value._guid);
        }

        /// <summary>
        /// Returns a hash code for the <see cref="ShortGuid"/>.
        /// </summary>
        /// <returns>The hash code for the <see cref="ShortGuid"/>.</returns>
        public override int GetHashCode()
        {
            return _guid.GetHashCode();
        }

        /// <summary>
        /// Returns the Base64 encoded <see cref="Guid"/> <see cref="string"/>.
        /// </summary>
        /// <returns>The Base64 encoded <see cref="Guid"/> <see cref="string"/>.</returns>
        public override string ToString()
        {
            return Convert.ToBase64String(_guid.ToByteArray()).Substring(0, 22).Replace("/", "_", StringComparison.InvariantCulture).Replace("+", "-", StringComparison.InvariantCulture);
        }
    }
}