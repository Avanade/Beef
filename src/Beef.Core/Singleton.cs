// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using System;

namespace Beef
{
    /// <summary>
    /// Base class to enable <see cref="Default"/> (singleton) instance.
    /// </summary>
    /// <typeparam name="T">The <see cref="Default"/> instance <see cref="Type"/>.</typeparam>
    public abstract class Singleton<T> where T : Singleton<T>, new()
    {
        private static readonly Lazy<T> _default = new(() => new T(), true);

#pragma warning disable CA1000 // Do not declare static members on generic types; by-design, results in a consistent static defined default instance without the need to specify generic type to consume.
        /// <summary>
        /// Gets the default (singleton) instance.
        /// </summary>
        public static T Default { get { return _default.Value; } }
#pragma warning restore CA1000

        /// <summary>
        /// Initializes a new instance of the <see cref="Singleton{T}"/> class.
        /// </summary>
        protected Singleton() { }
    }
}