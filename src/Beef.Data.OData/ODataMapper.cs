// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.Entities;
using Beef.Mapper;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Beef.Data.OData
{
    /// <summary>
    /// Enables the <b>OData</b> entity mapping capabilities.
    /// </summary>
    public interface IODataMapper : IEntityMapperBase
    {
        /// <summary>
        /// Gets the <b>OData</b> entity name as represented within the URL.
        /// </summary>
        string ODataEntityName { get; }

        /// <summary>
        /// Gets the <see cref="OData"/> field names.
        /// </summary>
        /// <returns>The formatted field list.</returns>
        string[] GetODataFieldNames();

        /// <summary>
        /// Gets the <see cref="OData"/> field names as a query string.
        /// </summary>
        /// <returns>The query string.</returns>
        string GetODataFieldNamesQuery();

#pragma warning disable CA1055 // Uri return values should not be strings; by-design, is ok.
        /// <summary>
        /// Gets the unique key formatted for a URL.
        /// </summary>
        /// <param name="keys">The unique key values.</param>
        /// <returns>The formatted unique key.</returns>
        string GetKeyUrl(params IComparable[] keys);

        /// <summary>
        /// Gets the unique key formatted for a URL from an entity <paramref name="value"/>.
        /// </summary>
        /// <param name="value">The entity value.</param>
        /// <returns>The formatted unique key.</returns>
        string GetKeyUrl(object value);
#pragma warning restore CA1055

        /// <summary>
        /// Creates a source entity mapping values from the <see cref="JToken"/>.
        /// </summary>
        /// <param name="json">The <see cref="JToken"/>.</param>
        /// <param name="operationType">The single <see cref="Mapper.OperationTypes"/> being performed to enable selection.</param>
        /// <returns>An object populated from the <see cref="JToken"/>.</returns>
        object MapFromOData(JToken json, OperationTypes operationType);

        /// <summary>
        /// Maps values from the source entity <paramref name="value"/> into <b>OData</b> using the <see cref="JToken"/> .
        /// </summary>
        /// <param name="value">The entity value.</param>
        /// <param name="operationType">The single <see cref="Mapper.OperationTypes"/> being performed to enable selection.</param>
        /// <returns>The <see cref="JToken"/>.</returns>
        JToken MapToOData(object value, OperationTypes operationType);
    }

    /// <summary>
    /// Enables the <b>OData</b> entity <see cref="Type"/> mapping capabilities.
    /// </summary>
    /// <typeparam name="TSrce">The source entity <see cref="Type"/>.</typeparam>
    public interface IODataMapper<TSrce> : IEntitySrceMapper<TSrce>, IODataMapper where TSrce : class
    {
        /// <summary>
        /// Creates a source entity mapping values from the <see cref="JsonTextReader"/>.
        /// </summary>
        /// <param name="json">The <see cref="JToken"/>.</param>
        /// <param name="operationType">The single <see cref="Mapper.OperationTypes"/> being performed to enable selection.</param>
        /// <returns>A <typeparamref name="TSrce"/> instance populated from the <see cref="JToken"/>.</returns>
        new TSrce MapFromOData(JToken json, OperationTypes operationType);

        /// <summary>
        /// Maps values from the source entity <paramref name="value"/> into <b>OData</b> using the <see cref="JToken"/> .
        /// </summary>
        /// <param name="value">The entity value.</param>
        /// <param name="operationType">The single <see cref="Mapper.OperationTypes"/> being performed to enable selection.</param>
        /// <returns>The <see cref="JToken"/>.</returns>
        JToken MapToOData(TSrce value, OperationTypes operationType);
    }


    /// <summary>
    /// Provides access to the common <b>OData</b> mapper capabilities.
    /// </summary>
    public static class ODataMapper
    {
        /// <summary>
        /// Gets or sets the <b>OData</b> <see cref="IETag.ETag"/> property name.
        /// </summary>
        public static string ODataETagPropertyName { get; set; } = "@odata.etag";
    }

    /// <summary>
    /// Provides the <b>OData</b> entity mapping capabilities.
    /// </summary>
    /// <typeparam name="TSrce">The source entity <see cref="Type"/>.</typeparam>
    public class ODataMapper<TSrce> : EntitySrceMapper<TSrce>, IODataMapper<TSrce>
        where TSrce : class
    {
        private readonly bool _hasDefaultCtor = true;
        private readonly string[] _orderedProperties;

        /// <summary>
        /// Initialises a new instance of the <see cref="ODataMapper{TSrce}"/> class.
        /// </summary>
        /// <param name="odataEntityName">The <b>OData</b> entity name as represented within the URL.</param>
        public ODataMapper(string odataEntityName)
        {
            ODataEntityName = string.IsNullOrEmpty(odataEntityName) ? throw new ArgumentNullException(nameof(odataEntityName)) : odataEntityName;
            if (typeof(IETag).IsAssignableFrom(typeof(TSrce)))
                Property(x => ((IETag)x).ETag, ODataMapper.ODataETagPropertyName).MapSrceToDestWhen((v) => false);

            _hasDefaultCtor = SrceType.GetConstructor(Type.EmptyTypes) != null;
            if (!_hasDefaultCtor)
            {
                // This is required to support the likes of compiler generated Types when using LINQ queries.
                _orderedProperties = SrceType.GetProperties(BindingFlags.Public | BindingFlags.Instance).Select(x => x.Name).ToArray();
                if (!SrceType.GetConstructors().Where(x => x.GetParameters().Length == _orderedProperties.Length).Any())
                    throw new MapperException($"Type {SrceType.Name} does not have default constructor, or a constructor that accepts all properties; unable to determine constructor.");
            }
        }

        /// <summary>
        /// Gets the <b>OData</b> entity name as represented within the URL.
        /// </summary>
        public string ODataEntityName { get; private set; }

        /// <summary>
        /// Gets the <b>OData</b> entity name as represented within the URL.
        /// </summary>
        string IODataMapper.ODataEntityName => ODataEntityName;

        /// <summary>
        /// Adds a <see cref="ODataPropertyMapper{TSrce, TSrceProperty}"/> to the mapper.
        /// </summary>
        /// <typeparam name="TSrceProperty">The source property <see cref="Type"/>.</typeparam>
        /// <param name="srcePropertyExpression">The <see cref="Expression"/> to reference the source entity property.</param>
        /// <param name="odataPropertyName">The <b>OData</b> property name (defaults to the source property name where not specified).</param>
        /// <param name="operationTypes">The <see cref="Mapper.OperationTypes"/> selection to enable inclusion or exclusion of property (default to <see cref="OperationTypes.Any"/>).</param>
        /// <returns>The <see cref="ODataPropertyMapper{TEntity, TProperty}"/>.</returns>
        public ODataPropertyMapper<TSrce, TSrceProperty> Property<TSrceProperty>(Expression<Func<TSrce, TSrceProperty>> srcePropertyExpression, string odataPropertyName = null, OperationTypes operationTypes = OperationTypes.Any)
        {
            if (srcePropertyExpression == null)
                throw new ArgumentNullException(nameof(srcePropertyExpression));

            ODataPropertyMapper<TSrce, TSrceProperty> mapping = new ODataPropertyMapper<TSrce, TSrceProperty>(srcePropertyExpression, odataPropertyName, operationTypes);
            AddPropertyMapper<TSrceProperty>(mapping);
            return mapping;
        }

        /// <summary>
        /// Inherits the properties from the selected <paramref name="inheritMapper"/>.
        /// </summary>
        /// <typeparam name="T">The mapper <see cref="Type"/> to inherit from.</typeparam>
        /// <param name="inheritMapper">The <see cref="IODataMapper{T}"/> to inherit from.</param>
        public void InheritPropertiesFrom<T>(ODataMapper<T> inheritMapper) where T : class
        {
            if (inheritMapper == null)
                throw new ArgumentNullException(nameof(inheritMapper));

            if (!SrceType.GetTypeInfo().IsSubclassOf(typeof(T)))
                throw new ArgumentException($"Type {typeof(T).Name} must inherit from {SrceType.Name}.");

            var pe = Expression.Parameter(SrceType, "x");
            var type = typeof(ODataMapper<>).MakeGenericType(SrceType);

            foreach (var p in inheritMapper.Mappings.OfType<IODataPropertyMapper>())
            {
                var lex = Expression.Lambda(Expression.Property(pe, p.SrcePropertyName), pe);
                var pmap = (IODataPropertyMapper)type
                    .GetMethod("Property", BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly)
                    .MakeGenericMethod(p.SrcePropertyType)
                    .Invoke(this, new object[] { lex, p.DestPropertyName, p.OperationTypes });

                if (p.IsUniqueKey)
                    pmap.SetUniqueKey(p.IsUniqueKeyAutoGeneratedOnCreate);

                pmap.SetConverter(p.Converter);
                pmap.SetMapper(p.Mapper);
            }
        }

        /// <summary>
        /// Creates a source entity mapping values from the <see cref="JToken"/>.
        /// </summary>
        /// <param name="json">The <see cref="JToken"/>.</param>
        /// <param name="operationType">The single <see cref="Mapper.OperationTypes"/> being performed to enable selection.</param>
        /// <returns>A <typeparamref name="TSrce"/> instance populated from the <see cref="JToken"/>.</returns>
        public TSrce MapFromOData(JToken json, OperationTypes operationType = OperationTypes.Any)
        {
            if (json == null)
                throw new ArgumentNullException(nameof(json));

            // Use the default (parameter-less) constructor where available.
            if (_hasDefaultCtor)
            {
                TSrce value = Activator.CreateInstance<TSrce>();

                foreach (var map in Mappings.Cast<IODataPropertyMapper<TSrce>>())
                {
                    if (!map.OperationTypes.HasFlag(operationType))
                        continue;

                    var jp = json[map.DestPropertyName];
                    if (jp == null)
                        continue;

                    map.SetSrceValue(value, jp, operationType);
                }

                return value;
            }

            // Assumed the constructor is all properties in the declared order; so let's try it!
            var args = new object[_orderedProperties.Length];
            for (int i = 0; i < args.Length; i++)
            {
                var map = (IODataPropertyMapper<TSrce>)GetBySrcePropertyName(_orderedProperties[i]);
                if (map != null && map.OperationTypes.HasFlag(operationType))
                {
                    var jp = json[map.DestPropertyName];
                    args[i] = jp == null ? null : map.GetSrceValue(jp, operationType);
                }
            }

            return (TSrce)Activator.CreateInstance(SrceType, args);
        }

        /// <summary>
        /// Creates a source entity mapping values from the <see cref="JToken"/>.
        /// </summary>
        /// <param name="json">The <see cref="JToken"/>.</param>
        /// <param name="operationType">The single <see cref="Mapper.OperationTypes"/> being performed to enable selection.</param>
        /// <returns>A <typeparamref name="TSrce"/> instance populated from the <see cref="JToken"/>.</returns>
        object IODataMapper.MapFromOData(JToken json, OperationTypes operationType)
        {
            return MapFromOData(json, operationType);
        }

        /// <summary>
        /// Maps values from the source entity <paramref name="value"/> into <b>OData</b> using a <see cref="JToken"/> .
        /// </summary>
        /// <param name="value">The entity value.</param>
        /// <param name="operationType">The single <see cref="Mapper.OperationTypes"/> being performed to enable selection.</param>
        /// <returns>A <see cref="JToken"/>.</returns>
        public JToken MapToOData(TSrce value, OperationTypes operationType = OperationTypes.Any)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            var json = new JObject();

            foreach (IODataPropertyMapper<TSrce> map in Mappings)
            {
                if (map.OperationTypes.HasFlag(operationType) && map.MapSrceToDestWhen(value))
                    map.SetDestValue(value, json, operationType);
            }

            return json;
        }

        /// <summary>
        /// Maps values from the source entity <paramref name="value"/> into <b>OData</b> using the <see cref="JToken"/> .
        /// </summary>
        /// <param name="value">The entity value.</param>
        /// <param name="operationType">The single <see cref="Mapper.OperationTypes"/> being performed to enable selection.</param>
        /// <returns>The <see cref="JToken"/>.</returns>
        JToken IODataMapper.MapToOData(object value, OperationTypes operationType)
        {
            return MapToOData((TSrce)value, operationType);
        }

#pragma warning disable CA1055 // Uri return values should not be strings; by-design, is ok.
        /// <summary>
        /// Gets the unique key formatted for a URL from an entity <paramref name="value"/>.
        /// </summary>
        /// <param name="value">The entity value.</param>
        /// <returns>The formatted unique key.</returns>
        public string GetKeyUrl(TSrce value)

        {
            var keys = new List<IComparable>();
            foreach (IODataPropertyMapper<TSrce> map in UniqueKey)
            {
                keys.Add((IComparable)map.GetDestValue(value, OperationTypes.Any));
            }

            return GetKeyUrl(keys.ToArray());
        }

        /// <summary>
        /// Gets the unique key formatted for a URL from an entity <paramref name="value"/>.
        /// </summary>
        /// <param name="value">The entity value.</param>
        /// <returns>The formatted unique key.</returns>
        string IODataMapper.GetKeyUrl(object value)
        {
            return GetKeyUrl((TSrce)value);
        }

        /// <summary>
        /// Gets the unique key formatted for a URL for the listed key values.
        /// </summary>
        /// <param name="keys">The unique key values.</param>
        /// <returns>The formatted unique key.</returns>
        public string GetKeyUrl(params IComparable[] keys)
        {
            if (keys == null || keys.Length != UniqueKey.Count)
                throw new ArgumentException("The number of keys supplied must equal the number of properties identified as IsUniqueKey.", nameof(keys));

            var sb = new StringBuilder("(");
            if (keys.Length == 0)
                return null;
            if (keys.Length == 1)
            {
                if (keys[0] == null)
                    throw new MapperException("A key value cannot be null.");

                FormatKeyValue(sb, keys[0]);
            }
            else
            {
                for (int i = 0; i < keys.Length; i++)
                {
                    if (keys[i] == null)
                        throw new MapperException("A key value cannot be null.");

                    if (sb.Length > 1)
                        sb.Append(",");

                    sb.Append(UniqueKey[i].DestPropertyName);
                    sb.Append("=");
                    FormatKeyValue(sb, keys[i]);
                }
            }

            sb.Append(")");
            return sb.ToString();
        }
#pragma warning restore CA1055

        /// <summary>
        /// Formats the key value.
        /// </summary>
        private void FormatKeyValue(StringBuilder sb, IComparable key)
        {
            if (key is string)
            {
                sb.Append("'");
                sb.Append(key?.ToString());
                sb.Append("'");
            }
            else
            {
                if (key is Guid || key is Guid?)
                {
                    sb.Append("guid'");
                    sb.Append(key?.ToString());
                    sb.Append("'");
                }
                else
                    sb.Append(key?.ToString());
            }
        }

        /// <summary>
        /// Gets the <see cref="OData"/> field names.
        /// </summary>
        /// <returns>The formatted field list.</returns>
        public string[] GetODataFieldNames()
        {
            return Mappings.Select(x => x.DestPropertyName).Where(x => x != ODataMapper.ODataETagPropertyName).ToArray();
        }

        /// <summary>
        /// Gets the <see cref="OData"/> field names as a query string.
        /// </summary>
        /// <returns>The query string.</returns>
        public string GetODataFieldNamesQuery()
        {
            var q = string.Join(",", Mappings.Select(x => x.DestPropertyName).Where(x => x != ODataMapper.ODataETagPropertyName));
            return string.IsNullOrEmpty(q) ? null : q;
        }
    }

    /// <summary>
    /// Provides <see cref="ODataMapper{TSrce}"/> with a singleton <see cref="Default"/>.
    /// </summary>
    /// <typeparam name="TSrce">The source entity <see cref="Type"/>.</typeparam>
    /// <typeparam name="TMapper">The mapper <see cref="Type"/>.</typeparam>
    public abstract class ODataMapper<TSrce, TMapper> : ODataMapper<TSrce>
        where TSrce : class, new()
        where TMapper : ODataMapper<TSrce, TMapper>, new()
    {
        private static readonly TMapper _default = new TMapper();

        /// <summary>
        /// Initialises a new instance of the <see cref="ODataMapper{TSrce}"/> class.
        /// </summary>
        /// <param name="odataEntityName">The <b>OData</b> entity name as represented within the URL.</param>
        protected ODataMapper(string odataEntityName) : base(odataEntityName) { }

        /// <summary>
        /// Gets the current instance of the mapper.
        /// </summary>
#pragma warning disable CA1000 // Do not declare static members on generic types; by-design, is ok.
        public static TMapper Default
#pragma warning restore CA1000 
        {
            get
            {
                if (_default == null)
                    throw new InvalidOperationException("An instance of this Mapper cannot be referenced as it is still being constructed; beware that you may have a circular reference within the constructor.");

                return _default;
            }
        }

        #region CreateArgs

        /// <summary>
        /// Creates a <see cref="ODataArgs{T}"/>.
        /// </summary>
        /// <returns>A <see cref="ODataArgs{T}"/>.</returns>
        public ODataArgs<TSrce> CreateArgs()
        {
            return new ODataArgs<TSrce>(this);
        }

        /// <summary>
        /// Creates a <see cref="ODataArgs{T}"/> with a specified <see cref="ODataIfMatch"/> condition directive.
        /// </summary>
        /// <param name="ifMatch">The <see cref="ODataIfMatch"/> condition directive (only used for <see cref="ODataBase.UpdateAsync{T}(ODataArgs, T)"/> operations).</param>
        /// <returns>A <see cref="ODataArgs{T}"/>.</returns>
        public ODataArgs<TSrce> CreateArgs(ODataIfMatch ifMatch)
        {
            return new ODataArgs<TSrce>(this) { IfMatch = ifMatch };
        }

        /// <summary>
        /// Creates a <see cref="ODataArgs{T}"/> with a <see cref="PagingArgs"/>.
        /// </summary>
        /// <param name="paging">The <see cref="PagingArgs"/>.</param>
        /// <returns>A <see cref="ODataArgs{T}"/>.</returns>
        public ODataArgs<TSrce> CreateArgs(PagingArgs paging)
        {
            return new ODataArgs<TSrce>(this, paging);
        }

        /// <summary>
        /// Creates a <see cref="ODataArgs{T}"/> with a <see cref="PagingResult"/>.
        /// </summary>
        /// <param name="paging">The <see cref="PagingResult"/>.</param>
        /// <returns>A <see cref="ODataArgs{T}"/>.</returns>
        public ODataArgs<TSrce> CreateArgs(PagingResult paging)
        {
            return new ODataArgs<TSrce>(this, paging);
        }

        #endregion
    }
}