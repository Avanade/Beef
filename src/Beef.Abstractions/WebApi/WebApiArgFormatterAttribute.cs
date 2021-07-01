// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.Mapper.Converters;
using System;
using System.Reflection;

namespace Beef.WebApi
{
    /// <summary>
    /// Specifies the <see cref="IPropertyMapperConverter"/> to be used to convert a <see cref="WebApiArg{T}.Value"/> into the corresponding Web API query <see cref="string"/> value. 
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
    public sealed class WebApiArgFormatterAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WebApiArgFormatterAttribute"/> class.
        /// </summary>
        /// <param name="converterType">The converter <see cref="Type"/> (implements <see cref="IPropertyMapperConverter"/>).</param>
        public WebApiArgFormatterAttribute(Type converterType)
        {
            ConverterType = Check.NotNull(converterType, nameof(converterType));
            if (!typeof(IPropertyMapperConverter).IsAssignableFrom(ConverterType.GetTypeInfo()))
                throw new ArgumentException($"Type '{converterType.Name}' must implement 'IPropertyMapperConverter'.", nameof(converterType));
        }

        /// <summary>
        /// Gets the converter <see cref="Type"/> (implements <see cref="IPropertyMapperConverter"/>).
        /// </summary>
        public Type ConverterType { get; private set; }
    }
}