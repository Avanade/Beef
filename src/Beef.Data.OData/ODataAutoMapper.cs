// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using System;
using System.Linq;
using Beef.Mapper;
using System.Reflection;
using System.Linq.Expressions;
using Beef.Mapper.Converters;
using System.Collections.Generic;
using Beef.Reflection;

namespace Beef.Data.OData
{
    /// <summary>
    /// An <see cref="ODataMapper{TSrce}"/> (Entity to/from JSON) auto-mapper; by default all public instance get/set properties are automatically mapped. Use <see cref="MapperEntityAttribute"/>,
    /// <see cref="MapperPropertyAttribute"/> and <see cref="MapperIgnoreAttribute"/> to further control the auto-mapping capabilities. 
    /// </summary>
    public static class ODataAutoMapper
    {
        private static readonly object _lock = new object();
        private static Dictionary<Type, IODataMapper> _cache = new Dictionary<Type, IODataMapper>();

        /// <summary>
        /// Indicates whether the entity <typeparamref name="TSrce"/> <see cref="Type"/> is cached.
        /// </summary>
        /// <typeparam name="TSrce">The entity <see cref="Type"/>.</typeparam>
        /// <returns><b>true</b> indicates that the type exists within the cache; otherwise, <b>false</b>.</returns>
        public static bool IsTypeCached<TSrce>()
        {
            return IsTypeCached(typeof(TSrce));
        }

        /// <summary>
        /// Indicates whether the entity <see cref="Type"/> is cached.
        /// </summary>
        /// <param name="type">The entity <see cref="Type"/>.</param>
        /// <returns><b>true</b> indicates that the type exists within the cache; otherwise, <b>false</b>.</returns>
        public static bool IsTypeCached(Type type)
        {
            return _cache.ContainsKey(type);
        }

        /// <summary>
        /// Gets (creates) the <see cref="IODataMapper"/> for the <typeparamref name="TSrce"/> automatically.
        /// </summary>
        /// <typeparam name="TSrce">The entity <see cref="Type"/>.</typeparam>
        /// <param name="overrideEntityName">Overrides the entity name.</param>
        /// <returns>The auto-mapped <see cref="IODataMapper"/>.</returns>
        public static IODataMapper GetMapper<TSrce>(string overrideEntityName = null) where TSrce : class
        {
            return GetMapper(typeof(TSrce), overrideEntityName);
        }

        /// <summary>
        /// Gets (creates) the <see cref="IODataMapper"/> for the specified <paramref name="type"/> automatically.
        /// </summary>
        /// <param name="type">The entity <see cref="Type"/>.</param>
        /// <param name="overrideEntityName">Overrides the entity name.</param>
        /// <returns>The auto-mapped <see cref="IODataMapper"/>.</returns>
        public static IODataMapper GetMapper(Type type, string overrideEntityName = null)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            return GetOrCreateMapper(type, null, overrideEntityName);
        }

        /// <summary>
        /// Gets (creates) the <see cref="IODataMapper"/> for the specified <paramref name="type"/> and <paramref name="altProps"/> automatically.
        /// </summary>
        /// <param name="type">The entity <see cref="Type"/>.</param>
        /// <param name="altProps">The alternate property information.</param>
        /// <param name="overrideEntityName">Overrides the entity name.</param>
        /// <returns>The auto-mapped <see cref="IODataMapper"/>.</returns>
        internal static IODataMapper GetMapper(Type type, Dictionary<string, MemberInfo> altProps, string overrideEntityName = null)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            if (altProps == null || altProps.Count == 0)
                throw new ArgumentNullException(nameof(altProps));

            return GetOrCreateMapper(type, altProps, overrideEntityName);
        }

        /// <summary>
        /// Gets from cache (or creates and adds to cache) the <see cref="IODataMapper"/> (with option to merge alternate property configuration).
        /// </summary>
        private static IODataMapper GetOrCreateMapper(Type type, Dictionary<string, MemberInfo> altProps, string overrideEntityName)
        {
            if (_cache.TryGetValue(type, out IODataMapper map))
                return map;

            lock (_lock)
            {
                if (_cache.TryGetValue(type, out map))
                    return map;

                if (!type.IsClass)
                    throw new MapperException($"Type '{type.Name}' must be a class for auto-mapping.");

                var mea = type.GetCustomAttributes(typeof(MapperEntityAttribute), true).OfType<MapperEntityAttribute>().FirstOrDefault();
                map = (IODataMapper)Activator.CreateInstance(typeof(ODataMapper<>).MakeGenericType(type), (overrideEntityName ?? mea?.Name) ?? type.Name);
                var pe = Expression.Parameter(type, "x");
                string dname = null;
                var hasProperties = false;

                MapperPropertyAttribute mpa = null;

                foreach (var p in type.GetProperties(BindingFlags.Public | BindingFlags.GetProperty | BindingFlags.SetProperty | BindingFlags.Instance))
                {
                    // Do not auto-map where the Ignore attribute has been specified.
                    if (p.GetCustomAttributes(typeof(MapperIgnoreAttribute), true).OfType<MapperIgnoreAttribute>().FirstOrDefault() != null)
                        continue;

                    // Get property merge config.
                    if (altProps == null)
                    {
                        mpa = p.GetCustomAttributes(typeof(MapperPropertyAttribute), true).OfType<MapperPropertyAttribute>().FirstOrDefault();
                        dname = mpa?.Name;
                    }
                    else
                    {
                        if (!altProps.TryGetValue(p.Name, out MemberInfo alt))
                            throw new InvalidOperationException($"Type '{type.Name}' Property '{p.Name}' does not have an alternative property configuration specified.");

                        // Do not auto-map where the Ignore attribute has been specified.
                        if (alt.GetCustomAttributes(typeof(MapperIgnoreAttribute), true).OfType<MapperIgnoreAttribute>().FirstOrDefault() != null)
                            continue;

                        mpa = alt.GetCustomAttributes(typeof(MapperPropertyAttribute), true).OfType<MapperPropertyAttribute>().FirstOrDefault();
                        dname = mpa?.Name ?? alt.Name;
                    }

                    // Create the lambda expression for the property and add to the mapper.
                    hasProperties = true;

                    var lex = Expression.Lambda(Expression.Property(pe, p.Name), pe);
                    var pmap = (IODataPropertyMapper)map.GetType()
                        .GetMethod("Property", BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly)
                        .MakeGenericMethod(p.PropertyType)
                        .Invoke(map, new object[] { lex, dname, mpa == null ? OperationTypes.Any : mpa.OperationTypes });

                    if (mpa == null)
                        continue;

                    // Apply auto-map Property attribute IsUnique configuration.
                    if (mpa.IsUniqueKey)
                        pmap.SetUniqueKey(mpa.IsUniqueKeyAutoGeneratedOnCreate);

                    // Apply auto-map Property attribute ConverterType configuration.
                    if (mpa.ConverterType != null)
                    {
                        if (!typeof(IPropertyMapperConverter).IsAssignableFrom(mpa.ConverterType))
                            throw new MapperException($"Type '{type.Name}' Property '{p.Name}' has 'MapperPropertyAttribute' with ConverterType set to '{mpa.ConverterType.Name}' which does not implement 'IPropertyMapperConverter'.");

                        IPropertyMapperConverter pmc = null;
                        var pdef = mpa.ConverterType.GetProperty("Default", BindingFlags.FlattenHierarchy | BindingFlags.Public | BindingFlags.Static);
                        if (pdef == null)
                        {
                            if (mpa.ConverterType.GetConstructor(Type.EmptyTypes) == null)
                                throw new MapperException($"Type '{type.Name}' Property '{p.Name}' has 'MapperPropertyAttribute' with ConverterType set to '{mpa.ConverterType.Name}' does not have a static 'Default' property or default constructor.");

                            pmc = (IPropertyMapperConverter)Activator.CreateInstance(mpa.ConverterType);
                        }
                        else
                            pmc = (IPropertyMapperConverter)pdef.GetValue(null);

                        pmap.SetConverter(pmc);
                        continue;
                    }

                    // Apply auto-map Property attribute MapperType configuration for complex types.
                    if (pmap.IsSrceComplexType)
                    {
                        IEntityMapperBase em = null;
                        if (mpa.MapperType == null)
                            em = GetMapper(pmap.SrceComplexTypeReflector.ItemType);
                        else
                        {
                            if (!typeof(IEntityMapperBase).IsAssignableFrom(mpa.MapperType))
                                throw new MapperException($"Type '{type.Name}' Property '{p.Name}' has 'MapperPropertyAttribute' with MapperType set to '{mpa.MapperType.Name}' which does not implement 'IEntityMapper'.");

                            var mdef = mpa.MapperType.GetProperty("Default", BindingFlags.FlattenHierarchy | BindingFlags.Public | BindingFlags.Static);
                            if (mdef == null)
                            {
                                if (mpa.ConverterType.GetConstructor(Type.EmptyTypes) == null)
                                    throw new MapperException($"Type '{type.Name}' Property '{p.Name}' has 'MapperPropertyAttribute' with MapperType set to '{mpa.MapperType.Name}' does not have a static 'Default' property or default constructor.");

                                em = (IEntityMapperBase)Activator.CreateInstance(mpa.MapperType);
                            }
                            else
                                em = (IEntityMapperBase)mdef.GetValue(null);
                        }

                        if (em != null)
                            pmap.SetMapper(em);
                    }
                    else if (mpa.MapperType != null)
                         throw new MapperException($"Type '{type.Name}' Property '{p.Name}' has 'MapperPropertyAttribute' with MapperType set to '{mpa.ConverterType.Name}' although the property is not a complex type.");
                }

                if (!hasProperties)
                    throw new MapperException($"AutoMapper has found no properties for Type '{type.Name}' that it is able to auto-map.");

                _cache.Add(type, map);
                return map;
            }
        }
    }
}
