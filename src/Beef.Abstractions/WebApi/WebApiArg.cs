// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.Entities;
using Beef.Mapper.Converters;
using Beef.RefData;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Beef.WebApi
{
    /// <summary>
    /// Represents the <b>Web API</b> argument types.
    /// </summary>
    public enum WebApiArgType
    {
        /// <summary>
        /// Indicates the argument should be passed in the body.
        /// </summary>
        FromBody,

        /// <summary>
        /// Indicates the argument should be passed as part of the URI query string.
        /// </summary>
        FromUri,

        /// <summary>
        /// Indicates the properties of the argument should be passed as part of the URI query string with no prefix.
        /// </summary>
        FromUriUseProperties,

        /// <summary>
        /// Indicates the properties of the argument should be passed as part of the URI query string with a prefix.
        /// </summary>
        FromUriUsePropertiesAndPrefix
    }

    /// <summary>
    /// Represents a <b>Web API</b> argument base class.
    /// </summary>
    public abstract class WebApiArg
    {
        /// <summary>
        /// Gets the query string format constant.
        /// </summary>
        protected const string QueryStringFormat = "{0}={1}";

        /// <summary>
        /// Initializes a new instance of the <see cref="WebApiArg"/> class.
        /// </summary>
        /// <param name="name">The argument <see cref="Name"/>.</param>
        /// <param name="argType">The <see cref="WebApiArgType"/>.</param>
        protected WebApiArg(string name, WebApiArgType argType = WebApiArgType.FromUri)
        {
            Name = name;
            ArgType = argType;
            if (ArgType == WebApiArgType.FromBody)
                IsUsed = true;
        }

        /// <summary>
        /// Gets the argument name.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Gets the <see cref="WebApiArgType"/>.
        /// </summary>
        public WebApiArgType ArgType { get; private set; }

        /// <summary>
        /// Indicates whether the value has been used/referenced.
        /// </summary>
        public bool IsUsed { get; internal set; }

        /// <summary>
        /// Indicates whether the value is null or default and therefore should be ignored.
        /// </summary>
        public abstract bool IsDefault { get; }

        /// <summary>
        /// Returns the name and value formatted (see <see cref="QueryStringFormat"/>) for a URL query string.
        /// </summary>
        /// <returns>The URL string.</returns>
        public abstract string? ToUrlQueryString();

        /// <summary>
        /// Gets the underlying value.
        /// </summary>
        /// <returns>The underlying value.</returns>
        public abstract object? GetValue();
    }

    /// <summary>
    /// Represents a typed <see cref="WebApiArg"/> argument.
    /// </summary>
    public class WebApiArg<T> : WebApiArg
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WebApiArg{Object}"/> class.
        /// </summary>
        /// <param name="name">The argument <see cref="WebApiArg.Name"/>.</param>
        /// <param name="value">The argument <see cref="Value"/>.</param>
        /// <param name="argType">The <see cref="WebApiArgType"/>.</param>
        public WebApiArg(string name, T value, WebApiArgType argType = WebApiArgType.FromUri)
            : base(name, argType)
        {
            Value = value;
        }

        /// <summary>
        /// Gets (same as <see cref="GetValue"/>) or sets the argument value.
        /// </summary>
        public T Value { get; set; }

        /// <summary>
        /// Indicates whether the value is null or default and therefore should be ignored.
        /// </summary>
        public override bool IsDefault
        {
            get { return Comparer<T>.Default.Compare(Value, default!) == 0; }
        }

        /// <summary>
        /// Gets the underlying value.
        /// </summary>
        /// <returns>The underlying value.</returns>
        public override object? GetValue() => Value;

        /// <summary>
        /// Returns a string representation of just the <see cref="Value"/> itself.
        /// </summary>
        /// <returns>The string value.</returns>
        public override string? ToString()
        {
            if (Value is ReferenceDataBase rd)
                return rd.Code;
            else
                return Value?.ToString();
        }

        /// <summary>
        /// Returns the name and value formatted (see <see cref="WebApiArg.QueryStringFormat"/>) for a URL query string.
        /// </summary>
        /// <returns>The URL string.</returns>
        public override string? ToUrlQueryString()
        {
            if (IsDefault)
                return null;

            if (!(Value is string) && Value is IEnumerable enumerable)
            {
                var sb = new StringBuilder();
                foreach (var v in enumerable)
                {
                    if (v != null)
                        UriAppend(sb, CreateNameValue(base.Name, v, false));
                }

                return sb.Length == 0 ? null : sb.ToString();
            }

            return CreateNameValue(Name, Value, ArgType == WebApiArgType.FromUriUseProperties || ArgType == WebApiArgType.FromUriUsePropertiesAndPrefix);
        }

        /// <summary>
        /// Create the URL name and value pair.
        /// </summary>
        private string CreateNameValue(string name, object? value, bool isClassAllowed)
        {
            if (value == null)
                return UriFormat(name, null);

            if (value is string str)
                return UriFormat(name, str);

            if (value is DateTime dt)
                return UriFormat(name, dt.ToString("o", System.Globalization.CultureInfo.InvariantCulture));

            TypeInfo ti = value.GetType().GetTypeInfo();
            if (ti.IsEnum || ti.IsValueType)
                return UriFormat(name, value.ToString());

            if (ti.IsClass)
            {
                if (!isClassAllowed)
                    ThrowComplexityException(ti);

                var sb = new StringBuilder();
                foreach (var pi in ti.DeclaredProperties.Where(x => x.CanRead && x.CanWrite))
                {
                    // Only support serialization of JsonProperty's.
                    var jpa = pi.GetCustomAttribute<JsonPropertyAttribute>(true);
                    if (jpa == null)
                        continue;

                    // Ignore nulls.
                    object pVal = pi.GetValue(value);
                    if (pVal == null)
                        continue;

                    // Define name, and out strings directly.
                    string pName = ArgType == WebApiArgType.FromUriUseProperties ? jpa.PropertyName! : name + "." + jpa.PropertyName;
                    if (pVal is string pstr)
                    {
                        UriAppend(sb, UriFormat(pName, pstr));
                        continue;
                    }

                    // Iterate enumerables where they contain non-class types only.
                    if (pVal is IEnumerable enumerable)
                    {
                        foreach (var item in enumerable)
                        {
                            if (item != null)
                                UriAppend(sb, CreateNameValue(pName, item, false));
                        }

                        continue;
                    }

                    // Ignore default values.
                    var pti = pi.PropertyType.GetTypeInfo();
                    if (pti.IsValueType || pti.IsEnum)
                    {
                        object defaultValue = Activator.CreateInstance(pi.PropertyType);
                        if (defaultValue is IComparable comparer && comparer.CompareTo(pVal) == 0)
                            continue;
                    }
                    else
                    {
                        var waafa = pi.GetCustomAttribute<WebApiArgFormatterAttribute>();
                        if (waafa != null)
                        {
                            var converter = (IPropertyMapperConverter)Activator.CreateInstance(waafa.ConverterType);
                            if (converter.SrceType != pi.PropertyType || converter.DestType != typeof(string))
                                throw new InvalidOperationException($"Converter Type '{waafa.ConverterType.Name}' must have SrceType of '{pi.PropertyType.Name}' and DestType of 'string'.");

                            UriAppend(sb, CreateNameValue(pName, converter.ConvertToDest(pVal), false));
                            continue;
                        }

                        ThrowComplexityException(ti);
                    }

                    UriAppend(sb, UriFormat(pName, pVal is DateTime dt2 ? dt2.ToString("o", System.Globalization.CultureInfo.InvariantCulture) : pVal.ToString()));
                }

                return sb.ToString();
            }

            return string.Format(System.Globalization.CultureInfo.InvariantCulture, QueryStringFormat, base.Name, Uri.EscapeDataString(value.ToString()));
        }

        /// <summary>
        /// Appends the name+value pair value to the URI.
        /// </summary>
        protected void UriAppend(StringBuilder sb, string uriValue)
        {
            if (Check.NotNull(sb, nameof(sb)).Length > 0)
                sb.Append('&');

            sb.Append(uriValue);
        }

        /// <summary>
        /// Formats the name+value URI.
        /// </summary>
        protected string UriFormat(string name, string? value)
        {
            Check.NotNull(name, nameof(name));
            if (value == null)
                return Uri.EscapeUriString(name);
            else
                return string.Format(System.Globalization.CultureInfo.InvariantCulture, QueryStringFormat, Uri.EscapeUriString(name), Uri.EscapeDataString(value));
        }

        /// <summary>
        /// Type is too complex and can not be converted to a URI.
        /// </summary>
        private static void ThrowComplexityException(TypeInfo ti) => throw new InvalidOperationException($"Type '{ti.FullName}' Property '{ti.Name}' cannot be serialized to a URI; Type should be passed using Request Body [FromBody] given complexity.");
    }

    /// <summary>
    /// Represents a <see cref="PagingArgs"/> <see cref="WebApiArg"/> argument.
    /// </summary>
    public class WebApiPagingArgsArg : WebApiArg<PagingArgs?>
    {
        /// <summary>
        /// Gets or sets the <see cref="PagingArgs.Page"/> query string name.
        /// </summary>
        public static string PagingArgsPageQueryStringName { get; set; } = "$page";

        /// <summary>
        /// Gets or sets the <see cref="PagingArgs.Size"/> query string name.
        /// </summary>
        public static string PagingArgsSizeQueryStringName { get; set; } = "$size";

        /// <summary>
        /// Gets or sets the <see cref="PagingArgs.Skip"/> query string name.
        /// </summary>
        public static string PagingArgsSkipQueryStringName { get; set; } = "$skip";

        /// <summary>
        /// Gets or sets the <see cref="PagingArgs.Take"/> query string name.
        /// </summary>
        public static string PagingArgsTakeQueryStringName { get; set; } = "$take";

        /// <summary>
        /// Gets or sets the <see cref="PagingArgs.IsGetCount"/> query string name.
        /// </summary>
        public static string PagingArgsCountQueryStringName { get; set; } = "$count";

        /// <summary>
        /// Initializes a new instance of the <see cref="WebApiPagingArgsArg"/> class.
        /// </summary>
        /// <param name="name">The argument <see cref="WebApiArg.Name"/>.</param>
        /// <param name="value">The argument <see cref="WebApiArg{PagingArgs}.Value"/>.</param>
        public WebApiPagingArgsArg(string name, PagingArgs? value)
            : base(name, value) { }

        /// <summary>
        /// Returns a string that represents the name and value formatted for a URL.
        /// </summary>
        /// <returns>The URL string format.</returns>
        public override string? ToUrlQueryString()
        {
            if (Value == null)
                return null;

            var sb = new StringBuilder();
            if (Value.IsSkipTake)
            {
                if (Value.Skip > 0)
                    UriAppend(sb, UriFormat(PagingArgsSkipQueryStringName, Value.Skip.ToString(System.Globalization.CultureInfo.InvariantCulture)));

                if (Value.Take > 0)
                    UriAppend(sb, UriFormat(PagingArgsTakeQueryStringName, Value.Take.ToString(System.Globalization.CultureInfo.InvariantCulture)));
            }
            else
            {
                if (Value.Page.HasValue && Value.Page.Value > 0)
                    UriAppend(sb, UriFormat(PagingArgsPageQueryStringName, Value.Page?.ToString(System.Globalization.CultureInfo.InvariantCulture)));

                if (Value.Size > 0)
                    UriAppend(sb, UriFormat(PagingArgsSizeQueryStringName, Value.Size.ToString(System.Globalization.CultureInfo.InvariantCulture)));
            }

            if (Value.IsGetCount)
                UriAppend(sb, UriFormat(PagingArgsCountQueryStringName, "true"));

            return sb.ToString();
        }
    }
}