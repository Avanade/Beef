// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using System;
using System.Collections.Generic;

namespace Beef.Entities
{
    /// <summary>
    /// Represents a composite unique key.
    /// </summary>
    [System.Diagnostics.DebuggerStepThrough]
    public struct UniqueKey : IEquatable<UniqueKey>
    {
        /// <summary>
        /// Represents an empty <see cref="UniqueKey"/>.
        /// </summary>
        public static readonly UniqueKey Empty;

        private object?[] _args;

        /// <summary>
        /// Initializes a new <see cref="UniqueKey"/> structure with 
        /// </summary>
        /// <param name="args">Tthe argument values for the key.</param>
        public UniqueKey(params object?[] args)
        {
            _args = Check.NotNull(args, nameof(args));
        }

        /// <summary>
        /// Gets the argument values for the key.
        /// </summary>
        public object?[] Args
        {
            get
            {
                if (_args == null)
                    _args = Array.Empty<object?>();

                return _args;
            }
        }

        /// <summary>
        /// Determines whether the current <see cref="UniqueKey"/> is equal to another <see cref="UniqueKey"/>.
        /// </summary>
        /// <param name="other">The other <see cref="UniqueKey"/>.</param>
        /// <returns><c>true</c> if the values are equal; otherwise, <c>false</c>.</returns>
        /// <remarks>Uses the <see cref="UniqueKeyComparer.Equals(UniqueKey, UniqueKey)"/>.</remarks>
        public bool Equals(UniqueKey other)
        {
            return (new UniqueKeyComparer()).Equals(this, other);
        }

        /// <summary>
        /// Determines whether the current <see cref="UniqueKey"/> is equal to another <see cref="Object"/>.
        /// </summary>
        /// <param name="obj">The other <see cref="Object"/>.</param>
        /// <returns><c>true</c> if the values are equal; otherwise, <c>false</c>.</returns>
        public override bool Equals(object obj)
        {
            if (!(obj is UniqueKey))
                return false;

            return Equals((UniqueKey)obj);
        }

        /// <summary>
        /// Returns a hash code for the <see cref="UniqueKey"/>.
        /// </summary>
        /// <returns>A hash code for the <see cref="UniqueKey"/>.</returns>
        /// <remarks>Uses the <see cref="UniqueKeyComparer.GetHashCode(UniqueKey)"/>.</remarks>
        public override int GetHashCode()
        {
            return (new UniqueKeyComparer()).GetHashCode(this);
        }

        /// <summary>
        /// Compares two <see cref="UniqueKey"/> types for equality.
        /// </summary>
        /// <param name="left">The left <see cref="UniqueKey"/>.</param>
        /// <param name="right">The right <see cref="UniqueKey"/>.</param>
        /// <returns><c>true</c> indicates equal; otherwise, <c>false</c> for not equal.</returns>
        public static bool operator ==(UniqueKey left, UniqueKey right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// Compares two <see cref="UniqueKey"/> types for non-equality.
        /// </summary>
        /// <param name="left">The left <see cref="UniqueKey"/>.</param>
        /// <param name="right">The right <see cref="UniqueKey"/>.</param>
        /// <returns><c>true</c> indicates not equal; otherwise, <c>false</c> for equal.</returns>
        public static bool operator !=(UniqueKey left, UniqueKey right)
        {
            return !(left == right);
        }

        /// <summary>
        /// Determines whether the <see cref="UniqueKey"/> is considered initial; i.e. all <see cref="Args"/> have their default value.
        /// </summary>
        /// <returns><c>true</c> indicates that the <see cref="UniqueKey"/> is initial; otherwise, <c>false</c>.</returns>
        public bool IsInitial
        {
            get
            {
                if (_args == null || _args.Length == 0)
                    return true;

                foreach (var arg in _args)
                {
                    if (arg != null && !arg.Equals(GetDefaultValue(arg.GetType())))
                        return false;
                }

                return true;
            }
        }

        /// <summary>
        /// Gets the default value for a specified <paramref name="type"/>.
        /// </summary>
        private static object? GetDefaultValue(Type type) => Check.NotNull(type, nameof(type)).IsValueType ? Activator.CreateInstance(type) : null;
    }

    /// <summary>
    /// Represents a comparer of equality for a <see cref="UniqueKey"/>.
    /// </summary>
    public class UniqueKeyComparer : IEqualityComparer<UniqueKey>
    {
        private static readonly object _nullObject = new();

        /// <summary>
        /// Determines whether the specified <see cref="UniqueKey"/> values are equal.
        /// </summary>
        /// <param name="x">The first <see cref="UniqueKey"/> to compare.</param>
        /// <param name="y">The second <see cref="UniqueKey"/> to compare.</param>
        /// <returns><c>true</c> if the values are equal; otherwise, <c>false</c>.</returns>
        public bool Equals(UniqueKey x, UniqueKey y)
        {
            if (x.Args.Length != y.Args.Length)
                return false;

            for (int i = 0; i < x.Args.Length; i++)
            {
                if (!GetArgValue(x.Args[i]).Equals(GetArgValue(y.Args[i])))
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Returns a hash code for the <see cref="UniqueKey"/>.
        /// </summary>
        /// <param name="uKey">The <see cref="UniqueKey"/> for which a hash code is to be returned.</param>
        /// <returns>A hash code for the <see cref="UniqueKey"/>.</returns>
        public int GetHashCode(UniqueKey uKey)
        {
            int hashCode = 0;
            for (int i = 0; i < uKey.Args.Length; i++)
                hashCode ^= GetArgValue(uKey.Args[i]).GetHashCode();

            return hashCode;
        }

        /// <summary>
        /// Gets the argument value (handles a null value).
        /// </summary>
        private static object GetArgValue(object? arg)
        {
            return arg ?? _nullObject;
        }
    }
}