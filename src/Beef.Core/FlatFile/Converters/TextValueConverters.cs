// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using System;
using System.Collections.Generic;

namespace Beef.FlatFile.Converters
{
    /// <summary>
    /// Manages an <see cref="ITextValueConverter{T}"/> collection.
    /// </summary>
    public class TextValueConverters
    {
        private struct TextValueConverter
        {
            public Type Type { get; set; }
            public object Converter { get; set; }
        }

        private readonly Dictionary<string, TextValueConverter> _dict = new Dictionary<string, TextValueConverter>();

        /// <summary>
        /// Adds a default <see cref="ITextValueConverter{T}"/> for the <see cref="Type"/>.
        /// </summary>
        /// <typeparam name="T">The underlying <see cref="Type"/> being converted.</typeparam>
        /// <param name="converter">The <see cref="ITextValueConverter{T}"/>.</param>
        public void Add<T>(ITextValueConverter<T> converter)
        {
            if (converter == null)
                throw new ArgumentNullException(nameof(converter));

            _dict.Add(GetKey(null, typeof(T)), new TextValueConverter { Type = typeof(T), Converter = converter });
        }

        /// <summary>
        /// Adds an <see cref="ITextValueConverter{T}"/> with a corresponding <paramref name="key"/>.
        /// </summary>
        /// <typeparam name="T">The underlying <see cref="Type"/> being converted.</typeparam>
        /// <param name="key">The key.</param>
        /// <param name="converter">The <see cref="ITextValueConverter{T}"/>.</param>
        public void Add<T>(string key, ITextValueConverter<T> converter)
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException(nameof(key));

            if (converter == null)
                throw new ArgumentNullException(nameof(converter));

            _dict.Add(GetKey(key, typeof(T)), new TextValueConverter { Type = typeof(T), Converter = converter });
        }

        /// <summary>
        /// Gets the <see cref="ITextValueConverter{T}"/> for the corresponding <paramref name="key"/>.
        /// </summary>
        /// <typeparam name="T">The underlying <see cref="Type"/> being converted.</typeparam>
        /// <param name="key">The key (where <c>null</c> this indicates the default converter).</param>
        /// <param name="converterType">The <see cref="Type"/> (where <c>null</c> this indicates the default converter).</param>
        /// <returns>The <see cref="ITextValueConverter{T}"/> where found; otherwise, <c>null</c>.</returns>
        /// <remarks>The <paramref name="key"/> and <paramref name="converterType"/> are mutually exclusive.</remarks>
        public ITextValueConverter<T> Get<T>(string key = null, Type converterType = null)
        {
            if (key != null && converterType != null)
                throw new ArgumentException("The key and converterType are mutually exclusive; at least one must have a null value.");

            if (converterType != null)
                return (ITextValueConverter<T>)Activator.CreateInstance(converterType);

            return (ITextValueConverter<T>)Get(typeof(T), GetKey(key, typeof(T)), null);
        }

        /// <summary>
        /// Gets the <see cref="ITextValueConverter{T}"/> for the corresponding <paramref name="key"/> where found;
        /// otherwise, the default.
        /// </summary>
        /// <typeparam name="T">The underlying <see cref="Type"/> being converted.</typeparam>
        /// <param name="key">The key (where <c>null</c> this indicates the default converter).</param>
        /// <param name="converterType">The <see cref="Type"/> (where <c>null</c> this indicates the default converter).</param>
        /// <returns>The <see cref="ITextValueConverter{T}"/> where found; otherwise, <c>null</c>.</returns>
        /// <remarks>The <paramref name="key"/> and <paramref name="converterType"/> are mutually exclusive.</remarks>
        public ITextValueConverter<T> GetOrDefault<T>(string key = null, Type converterType = null)
        {
            if (key != null && converterType != null)
                throw new ArgumentException("The key and converterType are mutually exclusive; at least one must have a null value.");

            if (converterType != null)
                return (ITextValueConverter<T>)Activator.CreateInstance(converterType);

            return (ITextValueConverter<T>)GetOrDefault(typeof(T), GetKey(key, typeof(T)));
        }

        /// <summary>
        /// Gets the <see cref="ITextValueConverter{T}"/> for the corresponding <paramref name="type"/> and <paramref name="key"/>.
        /// </summary>
        /// <param name="type">The underlying <see cref="Type"/> being converted.</param>
        /// <param name="key">The key (where <c>null</c> this indicates the default converter).</param>
        /// <param name="converterType">The <see cref="Type"/> (where <c>null</c> this indicates the default converter).</param>
        /// <returns>The <see cref="ITextValueConverter{T}"/> where found; otherwise, <c>null</c>.</returns>
        /// <remarks>The <paramref name="key"/> and <paramref name="converterType"/> are mutually exclusive.</remarks>
        public ITextValueConverter Get(Type type, string key = null, Type converterType = null)
        {
            if (key != null && converterType != null)
                throw new ArgumentException("The key and converterType are mutually exclusive; at least one must have a null value.");

            if (converterType != null)
                return (ITextValueConverter)Activator.CreateInstance(converterType);

            if (!_dict.TryGetValue(GetKey(key, Check.NotNull(type, nameof(type))), out TextValueConverter tvc) || tvc.Type != type)
                return null;

            return (ITextValueConverter)tvc.Converter;
        }

        /// <summary>
        /// Gets the <see cref="ITextValueConverter{T}"/> for the corresponding <paramref name="type"/> and <paramref name="key"/> where found;
        /// otherwise, the default (key is <c>null</c>).
        /// </summary>
        /// <param name="type">The underlying <see cref="Type"/> being converted.</param>
        /// <param name="key">The key (where <c>null</c> this indicates the default converter).</param>
        /// <param name="converterType">The <see cref="Type"/> (where <c>null</c> this indicates the default converter).</param>
        /// <returns>The <see cref="ITextValueConverter{T}"/> where found; otherwise, <c>null</c>.</returns>
        /// <remarks>The <paramref name="key"/> and <paramref name="converterType"/> are mutually exclusive.</remarks>
        public ITextValueConverter GetOrDefault(Type type, string key = null, Type converterType = null)
        {
            if (key != null && converterType != null)
                throw new ArgumentException("The key and converterType are mutually exclusive; at least one must have a null value.");

            if (converterType != null)
                return (ITextValueConverter)Activator.CreateInstance(converterType);

            return Get(Check.NotNull(type, nameof(type)), key) ?? Get(type, null);
        }

        /// <summary>
        /// Gets the key.
        /// </summary>
        private static string GetKey(string key, Type type)
        {
            return key ?? type.FullName;
        }
    }
}
