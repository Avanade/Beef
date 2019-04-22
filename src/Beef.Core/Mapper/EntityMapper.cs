using Beef.Entities;
using Beef.Mapper.Converters;
using Beef.Reflection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Beef.Mapper
{
    /// <summary>
    /// Enables the base two-entity (source and destination) mapping capabilities.
    /// </summary>
    public interface IEntityMapper : IEntityMapperBase
    {
        /// <summary>
        /// Gets the destination <see cref="Type"/>.
        /// </summary>
        Type DestType { get ; }

        /// <summary>
        /// Maps the source to the destination.
        /// </summary>
        /// <param name="sourceEntity">The source entity.</param>
        /// <param name="operationType">The single <see cref="Mapper.OperationTypes"/> being performed to enable selection.</param>
        /// <returns>The destination value.</returns>
        object MapToDest(object sourceEntity, OperationTypes operationType);

        /// <summary>
        /// Maps the source to the destination updating an existing object.
        /// </summary>
        /// <param name="sourceEntity">The source entity.</param>
        /// <param name="destinationEntity">The destination entity.</param>
        /// <param name="operationType">The single <see cref="Mapper.OperationTypes"/> being performed to enable selection.</param>
        void MapToDest(object sourceEntity, object destinationEntity, OperationTypes operationType);

        /// <summary>
        /// Maps the destination to the source.
        /// </summary>
        /// <param name="destinationEntity">The destination entity.</param>
        /// <param name="operationType">The single <see cref="Mapper.OperationTypes"/> being performed to enable selection.</param>
        /// <returns>The source value.</returns>
        object MapToSrce(object destinationEntity, OperationTypes operationType);
    }

    /// <summary>
    /// Enables the base base two-entity (source and destination) mapping capabilities.
    /// </summary>
    /// <typeparam name="TSrce">The source entity <see cref="Type"/>.</typeparam>
    /// <typeparam name="TDest">The destination entity <see cref="Type"/>.</typeparam>
    public interface IEntityMapper<TSrce, TDest> : IEntityMapper 
    {
        /// <summary>
        /// Gets the <see cref="IPropertySrceMapper{TSrce}"/> mapping by source property name.
        /// </summary>
        /// <param name="name">The source property name.</param>
        /// <returns>The <see cref="IPropertySrceMapper{TSrce}"/> where found; otherwise, <c>null</c>.</returns>
        new IPropertyMapper<TSrce, TDest> GetBySrcePropertyName(string name);

        /// <summary>
        /// Gets the <see cref="IPropertySrceMapper{TSrce}"/> mapping by destination property name.
        /// </summary>
        /// <param name="name">The source property name.</param>
        /// <returns>The <see cref="IPropertySrceMapper{TSrce}"/> where found; otherwise, <c>null</c>.</returns>
        new IPropertyMapper<TSrce, TDest> GetByDestPropertyName(string name);

        /// <summary>
        /// Maps the source to the destination.
        /// </summary>
        /// <param name="sourceEntity">The source entity.</param>
        /// <param name="operationType">The single <see cref="Mapper.OperationTypes"/> being performed to enable selection.</param>
        /// <returns>The destination value.</returns>
        TDest MapToDest(TSrce sourceEntity, OperationTypes operationType);

        /// <summary>
        /// Maps the source to the destination updating an existing object.
        /// </summary>
        /// <param name="sourceEntity">The source entity.</param>
        /// <param name="destinationEntity">The destination entity.</param>
        /// <param name="operationType">The single <see cref="Mapper.OperationTypes"/> being performed to enable selection.</param>
        void MapToDest(TSrce sourceEntity, TDest destinationEntity, OperationTypes operationType);

        /// <summary>
        /// Maps the destination to the source.
        /// </summary>
        /// <param name="destinationEntity">The destination entity.</param>
        /// <param name="operationType">The single <see cref="Mapper.OperationTypes"/> being performed to enable selection.</param>
        /// <returns>The source value.</returns>
        TSrce MapToSrce(TDest destinationEntity, OperationTypes operationType);

        /// <summary>
        /// Gets the <see cref="IPropertyMapper{TSrce, TDest}"/> mappings.
        /// </summary>
        new IEnumerable<IPropertyMapper<TSrce, TDest>> Mappings { get; }

        /// <summary>
        /// Gets the properties that form the unique key.
        /// </summary>
        IPropertyMapper<TSrce, TDest>[] UniqueKey { get; }
    }

    /// <summary>
    /// Provides the base two-entity mapping capabilities.
    /// </summary>
    /// <typeparam name="TSrce">The source entity <see cref="Type"/>.</typeparam>
    /// <typeparam name="TDest">The destination entity <see cref="Type"/>.</typeparam>
    public class EntityMapper<TSrce, TDest> : IEntityMapper<TSrce, TDest>
        where TSrce : class, new()
        where TDest : class, new()
    {
        private List<IPropertyMapper<TSrce, TDest>> _mappings = new List<IPropertyMapper<TSrce, TDest>>();
        private Dictionary<string, IPropertyMapper<TSrce, TDest>> _srceMappings = new Dictionary<string, IPropertyMapper<TSrce, TDest>>();
        private Dictionary<string, IPropertyMapper<TSrce, TDest>> _destMappings = new Dictionary<string, IPropertyMapper<TSrce, TDest>>();
        private IPropertyMapper<TSrce, TDest>[] _uniqueKey;

        /// <summary>
        /// Creates an <see cref="EntityMapper{TSrce, TDest}"/> automatically mapping the properties where they share the same name.
        /// </summary>
        /// <param name="ignoreSrceProperties">An array of source property names to ignore.</param>
        /// <returns>An <see cref="EntityMapper{TSrce, TDest}"/>.</returns>
        public static EntityMapper<TSrce, TDest> CreateAuto(params string[] ignoreSrceProperties)
        {
            return new EntityMapper<TSrce, TDest>(true, ignoreSrceProperties);
        }

        /// <summary>
        /// Creates an <see cref="EntityMapper{TSrce, TDest}"/> where properties are added manually.
        /// </summary>
        /// <returns>An <see cref="EntityMapper{TSrce, TDest}"/>.</returns>
        public static EntityMapper<TSrce, TDest> Create()
        {
            return new EntityMapper<TSrce, TDest>(false);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EntityMapper{TSrce, TDest}"/> class.
        /// </summary>
        /// <param name="autoMap">Indicates whether the two entities should automatically map where the properties share the same name.</param>
        /// <param name="ignoreSrceProperties">An array of source property names to ignore.</param>
        public EntityMapper(bool autoMap = false, params string[] ignoreSrceProperties)
        {
            if (typeof(TSrce) == typeof(string))
                throw new InvalidOperationException("SrceType must not be a String.");

            if (typeof(TDest) == typeof(string))
                throw new InvalidOperationException("DestType must not be a String.");

            if (autoMap)
                AutomagicallyMap(ignoreSrceProperties);
        }

        /// <summary>
        /// Automatically map commonly named properties.
        /// </summary>
        private void AutomagicallyMap(string[] ignoreSrceProperties)
        {
            MapperPropertyAttribute mpa = null;

            foreach (var sp in SrceType.GetProperties(BindingFlags.Public | BindingFlags.GetProperty | BindingFlags.SetProperty | BindingFlags.Instance))
            {
                // Do not auto-map where ignore has been specified.
                if (ignoreSrceProperties.Contains(sp.Name))
                    continue;

                if (sp.GetCustomAttributes(typeof(MapperIgnoreAttribute), true).OfType<MapperIgnoreAttribute>().FirstOrDefault() != null)
                    continue;

                // Find corresponding property.
                mpa = sp.GetCustomAttributes(typeof(MapperPropertyAttribute), true).OfType<MapperPropertyAttribute>().FirstOrDefault();
                var dname = mpa == null || string.IsNullOrEmpty(mpa.Name) ? sp.Name : mpa.Name;
                var dp = DestType.GetProperty(dname, BindingFlags.Public | BindingFlags.GetProperty | BindingFlags.SetProperty | BindingFlags.Instance);
                if (dp == null)
                {
                    if (mpa != null)
                        throw new InvalidOperationException($"Type '{SrceType.Name}' Property '{sp.Name}' has 'MapperPropertyAttribute' with Name set to '{dname}' which does not exist for destination Type '{DestType.Name}'.");

                    continue;
                }

                // Create the lambda expressions for the property and add to the mapper.
                var spe = Expression.Parameter(SrceType, "x");
                var sex = Expression.Lambda(Expression.Property(spe, sp.Name), spe);
                var dpe = Expression.Parameter(DestType, "x");
                var dex = Expression.Lambda(Expression.Property(dpe, dname), dpe);
                var pmap = (IPropertyMapper<TSrce, TDest>)typeof(EntityMapper<TSrce, TDest>)
                    .GetMethod("PropertySrceAndDest", BindingFlags.NonPublic | BindingFlags.Instance)
                    .MakeGenericMethod(new Type[] { sp.PropertyType, dp.PropertyType })
                    .Invoke(this, new object[] { sex, dex });

                if (mpa == null)
                    continue;

                // Apply auto-map Property attribute IsUnique configuration.
                if (mpa.IsUniqueKey)
                    pmap.SetUniqueKey(mpa.IsUniqueKeyAutoGeneratedOnCreate);

                // Apply auto-map Property attribute ConverterType configuration.
                if (mpa.ConverterType != null)
                {
                    if (!typeof(IPropertyMapperConverter).IsAssignableFrom(mpa.ConverterType))
                        throw new MapperException($"Type '{SrceType.Name}' Property '{sp.Name}' has 'MapperPropertyAttribute' with ConverterType set to '{mpa.ConverterType.Name}' which does not implement 'IPropertyMapperConverter'.");

                    IPropertyMapperConverter pmc = null;
                    var pdef = mpa.ConverterType.GetProperty("Default", BindingFlags.FlattenHierarchy | BindingFlags.Public | BindingFlags.Static);
                    if (pdef == null)
                    {
                        if (mpa.ConverterType.GetConstructor(Type.EmptyTypes) == null)
                            throw new MapperException($"Type '{SrceType.Name}' Property '{sp.Name}' has 'MapperPropertyAttribute' with ConverterType set to '{mpa.ConverterType.Name}' does not have a static 'Default' property or default constructor.");

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
                    if (mpa.MapperType != null)
                    {
                        if (!typeof(IEntityMapperBase).IsAssignableFrom(mpa.MapperType))
                            throw new MapperException($"Type '{SrceType.Name}' Property '{sp.Name}' has 'MapperPropertyAttribute' with MapperType set to '{mpa.MapperType.Name}' which does not implement 'IEntityMapper'.");

                        var mdef = mpa.MapperType.GetProperty("Default", BindingFlags.FlattenHierarchy | BindingFlags.Public | BindingFlags.Static);
                        if (mdef == null)
                        {
                            if (mpa.ConverterType.GetConstructor(Type.EmptyTypes) == null)
                                throw new MapperException($"Type '{SrceType.Name}' Property '{sp.Name}' has 'MapperPropertyAttribute' with MapperType set to '{mpa.MapperType.Name}' does not have a static 'Default' property or default constructor.");

                            em = (IEntityMapperBase)Activator.CreateInstance(mpa.MapperType);
                        }
                        else
                            em = (IEntityMapperBase)mdef.GetValue(null);
                    }

                    if (em != null)
                        pmap.SetMapper(em);
                }
                else if (mpa.MapperType != null)
                    throw new MapperException($"Type '{SrceType.Name}' Property '{sp.Name}' has 'MapperPropertyAttribute' with MapperType set to '{mpa.ConverterType.Name}' although the property is not a complex type.");
            }
        }

        /// <summary>
        /// Gets the source <see cref="Type"/>.
        /// </summary>
        public Type SrceType { get; } = typeof(TSrce);

        /// <summary>
        /// Gets the destination <see cref="Type"/>.
        /// </summary>
        public Type DestType { get; } = typeof(TDest);

        /// <summary>
        /// Indicates whether when performing a <see cref="MapToSrce(TDest, OperationTypes)"/> and the result implements <see cref="ICleanUp"/> and <see cref="ICleanUp.IsInitial"/>
        /// is <c>true</c> then the result should be null, versus an initial instance value. Defaults to <c>true</c> which will result in a null where an initial instance value is created from a mapping.
        /// </summary>
        public bool MapToSrceNullWhenIsInitial { get; set; } = true;

        /// <summary>
        /// Adds a <see cref="PropertyMapper{TSrce, TSrceProperty, TDest, TDestProperty}"/> to the mapper.
        /// </summary>
        /// <typeparam name="TSrceProperty">The source property <see cref="Type"/>.</typeparam>
        /// <typeparam name="TDestProperty">The destination property <see cref="Type"/>.</typeparam>
        /// <param name="srcePropertyExpression">The <see cref="Expression"/> to reference the source entity property.</param>
        /// <param name="destPropertyExpression">The <see cref="Expression"/> to reference the destination entity property.</param>
        /// <returns>The <see cref="PropertyMapper{TSrce, TSrceProperty, TDest, TDestProperty}"/>.</returns>
        public virtual PropertyMapper<TSrce, TSrceProperty, TDest, TDestProperty> Property<TSrceProperty, TDestProperty>(Expression<Func<TSrce, TSrceProperty>> srcePropertyExpression, Expression<Func<TDest, TDestProperty>> destPropertyExpression)
        {
            return PropertySrceAndDest(srcePropertyExpression, destPropertyExpression);
        }

        /// <summary>
        /// Adds the source and destination properties (unique name to make reflection simpler for auto-mapping).
        /// </summary>
        protected PropertyMapper<TSrce, TSrceProperty, TDest, TDestProperty> PropertySrceAndDest<TSrceProperty, TDestProperty>(Expression<Func<TSrce, TSrceProperty>> srcePropertyExpression, Expression<Func<TDest, TDestProperty>> destPropertyExpression)
        {
            if (srcePropertyExpression == null)
                throw new ArgumentNullException(nameof(srcePropertyExpression));

            if (destPropertyExpression == null)
                throw new ArgumentNullException(nameof(destPropertyExpression));

            PropertyMapper<TSrce, TSrceProperty, TDest, TDestProperty> mapping = new PropertyMapper<TSrce, TSrceProperty, TDest, TDestProperty>(srcePropertyExpression, destPropertyExpression);
            AddPropertyMapper(mapping);
            return mapping;
        }

        /// <summary>
        /// Adds (or gets) a <see cref="PropertyMapper{TSrce, TSrceProperty, TDest, TDestProperty}"/>.
        /// </summary>
        /// <typeparam name="TSrceProperty">The source property <see cref="Type"/>.</typeparam>
        /// <typeparam name="TDestProperty">The destination property <see cref="Type"/>.</typeparam>
        /// <param name="srcePropertyExpression">The <see cref="Expression"/> to reference the source entity property.</param>
        /// <param name="destPropertyExpression">The <see cref="Expression"/> to reference the destination entity property.</param>
        /// <param name="property">An <see cref="Action"/> enabling access to the created <see cref="PropertyMapper{TSrce, TSrceProperty, TDest, TDestProperty}"/>.</param>
        /// <returns>The <see cref="EntityMapper{TSrce, TDest}"/>.</returns>
        public virtual EntityMapper<TSrce, TDest> HasProperty<TSrceProperty, TDestProperty>(Expression<Func<TSrce, TSrceProperty>> srcePropertyExpression, Expression<Func<TDest, TDestProperty>> destPropertyExpression, Action<PropertyMapper<TSrce, TSrceProperty, TDest, TDestProperty>> property = null)
        {
            if (srcePropertyExpression == null)
                throw new ArgumentNullException(nameof(srcePropertyExpression));

            if (destPropertyExpression == null)
                throw new ArgumentNullException(nameof(destPropertyExpression));

            var spe = PropertyExpression<TSrce, TSrceProperty>.Create(srcePropertyExpression);
            var dpe = PropertyExpression<TDest, TDestProperty>.Create(destPropertyExpression);

            var px = GetBySrcePropertyName(spe.Name);
            if (px != null && (px.DestPropertyName != dpe.Name))
                throw new ArgumentException($"Source property '{srcePropertyExpression.Name}' mapping already exists with a different destination property name");

            PropertyMapper<TSrce, TSrceProperty, TDest, TDestProperty> p = null;
            if (px == null)
            {
                p = new PropertyMapper<TSrce, TSrceProperty, TDest, TDestProperty>(spe, dpe);
                AddPropertyMapper(p);
            }
            else
                p = (PropertyMapper<TSrce, TSrceProperty, TDest, TDestProperty>)px;

            property?.Invoke(p);
            return this;
        }

        /// <summary>
        /// Adds a source <see cref="PropertySrceMapper{TSrce, TSrceProperty, TDest}"/> to the mapper.
        /// </summary>
        /// <typeparam name="TSrceProperty">The source property <see cref="Type"/>.</typeparam>
        /// <param name="srcePropertyExpression">The <see cref="Expression"/> to reference the source entity property.</param>
        /// <returns>The <see cref="PropertySrceMapper{TSrce, TSrceProperty, TDest}"/>.</returns>
        public virtual PropertySrceMapper<TSrce, TSrceProperty, TDest> SrceProperty<TSrceProperty>(Expression<Func<TSrce, TSrceProperty>> srcePropertyExpression)
            where TSrceProperty : class
        {
            if (srcePropertyExpression == null)
                throw new ArgumentNullException(nameof(srcePropertyExpression));

            PropertySrceMapper<TSrce, TSrceProperty, TDest> mapping = new PropertySrceMapper<TSrce, TSrceProperty, TDest>(srcePropertyExpression);
            AddPropertyMapper(mapping);
            return mapping;
        }

        /// <summary>
        /// Adds (or gets) a source <see cref="PropertySrceMapper{TSrce, TSrceProperty, TDest}"/>.
        /// </summary>
        /// <typeparam name="TSrceProperty">The source property <see cref="Type"/>.</typeparam>
        /// <param name="srcePropertyExpression">The <see cref="Expression"/> to reference the source entity property.</param>
        /// <param name="property">An <see cref="Action"/> enabling access to the created <see cref="PropertyMapper{TSrce, TSrceProperty, TDest, TDestProperty}"/>.</param>
        /// <returns>The <see cref="EntityMapper{TSrce, TDest}"/>.</returns>
        public virtual EntityMapper<TSrce, TDest> HasSrceProperty<TSrceProperty>(Expression<Func<TSrce, TSrceProperty>> srcePropertyExpression, Action<PropertySrceMapper<TSrce, TSrceProperty, TDest>> property = null)
            where TSrceProperty : class
        {
            if (srcePropertyExpression == null)
                throw new ArgumentNullException(nameof(srcePropertyExpression));

            var spe = PropertyExpression<TSrce, TSrceProperty>.Create(srcePropertyExpression);

            var px = GetBySrcePropertyName(spe.Name);
            if (px != null && (px.DestPropertyName != null))
                throw new ArgumentException($"Source property '{srcePropertyExpression.Name}' mapping already exists with a different destination property name");

            PropertySrceMapper<TSrce, TSrceProperty, TDest> p = null;
            if (px == null)
            {
                p = new PropertySrceMapper<TSrce, TSrceProperty, TDest>(spe);
                AddPropertyMapper(p);
            }
            else
                p = (PropertySrceMapper<TSrce, TSrceProperty, TDest>)px;

            property?.Invoke(p);
            return this;
        }

        /// <summary>
        /// Adds the <see cref="PropertyMapperCustomBase{TSrce, TSrceProperty}"/> to the underlying <see cref="Mappings"/> collection.
        /// </summary>
        /// <param name="mapping">The <see cref="PropertyMapperCustomBase{TSrce, TSrceProperty}"/>.</param>
        protected void AddPropertyMapper(IPropertyMapper<TSrce, TDest> mapping)
        {
            if (_srceMappings.ContainsKey(mapping.SrcePropertyName))
                throw new ArgumentException(string.Format("Source property '{0}' mapping can not be specified more than once.", mapping.SrcePropertyName), "SourcePropertyExpression");

            if (mapping.DestPropertyName != null && _destMappings.ContainsKey(mapping.DestPropertyName))
                throw new ArgumentException(string.Format("Destination property '{0}' mapping can not be specified more than once.", mapping.DestPropertyName), "DestPropertyExpression");

            _srceMappings.Add(mapping.SrcePropertyName, mapping);
            if (mapping.DestPropertyName != null)
                _destMappings.Add(mapping.DestPropertyName, mapping);

            _mappings.Add(mapping);
        }

        /// <summary>
        /// Removes the named property from the mapper (typically used to remove properties automatically added).
        /// </summary>
        /// <param name="srcePropertyName">The source property name.</param>
        /// <returns>The <see cref="EntityMapper{TSrce, TDest}"/>.</returns>
        public virtual EntityMapper<TSrce, TDest> Ignore(string srcePropertyName)
        {
            if (string.IsNullOrEmpty(srcePropertyName))
                throw new ArgumentNullException(nameof(srcePropertyName));

            var p = GetBySrcePropertyName(srcePropertyName);
            if (p == null)
                return this;

            _srceMappings.Remove(p.SrcePropertyName);
            _destMappings.Remove(p.DestPropertyName);
            _mappings.Remove(p);

            return this;
        }

        /// <summary>
        /// Removes the property from the mapper (typically used to remove properties automatically added).
        /// </summary>
        /// <typeparam name="TSrceProperty">The source property <see cref="Type"/>.</typeparam>
        /// <param name="srcePropertyExpression">The <see cref="Expression"/> to reference the source entity property.</param>
        /// <returns>The <see cref="EntityMapper{TSrce, TDest}"/>.</returns>
        public virtual EntityMapper<TSrce, TDest> Ignore<TSrceProperty>(Expression<Func<TSrce, TSrceProperty>> srcePropertyExpression)
        {
            return Ignore(PropertyExpression<TSrce, TSrceProperty>.GetPropertyName(srcePropertyExpression));
        }

        /// <summary>
        /// Gets the <see cref="IPropertyMapper{TSrce, TDest}"/> mappings.
        /// </summary>
        IEnumerable<IPropertyMapperBase> IEntityMapperBase.Mappings => _mappings;

        /// <summary>
        /// Gets the <see cref="IPropertyMapper{TSrce, TDest}"/> mappings.
        /// </summary>
        public IEnumerable<IPropertyMapper<TSrce, TDest>> Mappings
        {
            get { return _mappings; }
        }

        /// <summary>
        /// Maps the source to the destination.
        /// </summary>
        /// <param name="sourceEntity">The source entity.</param>
        /// <param name="operationType">The single <see cref="Mapper.OperationTypes"/> being performed to enable selection.</param>
        /// <returns>The destination value.</returns>
        object IEntityMapper.MapToDest(object sourceEntity, OperationTypes operationType)
        {
            return MapToDest((TSrce)sourceEntity);
        }

        /// <summary>
        /// Maps the source to the destination.
        /// </summary>
        /// <param name="sourceEntity">The source entity.</param>
        /// <param name="operationType">The single <see cref="Mapper.OperationTypes"/> being performed to enable selection.</param>
        /// <returns>The destination entity.</returns>
        public TDest MapToDest(TSrce sourceEntity, OperationTypes operationType = OperationTypes.Unspecified)
        {
            if (sourceEntity == null)
                return null;

            var dest = new TDest();

            foreach (var map in Mappings)
            {
                map.MapToDest(sourceEntity, dest, operationType);
            }

            dest = OnMapToDest(sourceEntity, dest, operationType);
            return dest;
        }

        /// <summary>
        /// Maps the source to the destination updating an existing object.
        /// </summary>
        /// <param name="sourceEntity">The source entity.</param>
        /// <param name="destinationEntity">The destination entity.</param>
        /// <param name="operationType">The single <see cref="Mapper.OperationTypes"/> being performed to enable selection.</param>
        void IEntityMapper.MapToDest(object sourceEntity, object destinationEntity, OperationTypes operationType)
        {
            MapToDest((TSrce)sourceEntity, (TDest)destinationEntity);
        }

        /// <summary>
        /// Maps the source to the destination updating an existing object.
        /// </summary>
        /// <param name="sourceEntity">The source entity.</param>
        /// <param name="destinationEntity">The destination entity.</param>
        /// <param name="operationType">The single <see cref="Mapper.OperationTypes"/> being performed to enable selection.</param>
        public void MapToDest(TSrce sourceEntity, TDest destinationEntity, OperationTypes operationType = OperationTypes.Unspecified)
        {
            foreach (var map in Mappings)
            {
                map.MapToDest(sourceEntity, destinationEntity, operationType);
            }

            destinationEntity = OnMapToDest(sourceEntity, destinationEntity, operationType);
        }

        /// <summary>
        /// Extension opportunity when performing a <see cref="MapToDest(TSrce, OperationTypes)"/>.
        /// </summary>
        /// <param name="sourceEntity">The source entity.</param>
        /// <param name="destinationEntity">The destination entity.</param>
        /// <param name="operationType">The single <see cref="Mapper.OperationTypes"/> being performed to enable selection.</param>
        /// <returns>The destination entity.</returns>
        protected virtual TDest OnMapToDest(TSrce sourceEntity, TDest destinationEntity, OperationTypes operationType)
        {
            return destinationEntity;
        }

        /// <summary>
        /// Maps the destination to the source.
        /// </summary>
        /// <param name="destinationEntity">The destination entity.</param>
        /// <param name="operationType">The single <see cref="Mapper.OperationTypes"/> being performed to enable selection.</param>
        /// <returns>The source value.</returns>
        object IEntityMapper.MapToSrce(object destinationEntity, OperationTypes operationType)
        {
            return MapToSrce((TDest)destinationEntity);
        }

        /// <summary>
        /// Maps the destination to the source.
        /// </summary>
        /// <param name="destinationEntity">The destination entity.</param>
        /// <param name="operationType">The single <see cref="Mapper.OperationTypes"/> being performed to enable selection.</param>
        /// <returns>The source entity.</returns>
        public TSrce MapToSrce(TDest destinationEntity, OperationTypes operationType = OperationTypes.Unspecified)
        {
            if (destinationEntity == null)
                return null;

            var srce = new TSrce();

            foreach (var map in Mappings)
            {
                if (map.OperationTypes.HasFlag(operationType))
                    map.MapToSrce(destinationEntity, srce, operationType);
            }

            srce = OnMapToSrce(destinationEntity, srce, operationType);

            if (srce != null && MapToSrceNullWhenIsInitial && srce is ICleanUp ic && ic.IsInitial)
                return null;

            return srce;
        }

        /// <summary>
        /// Extenstion opportunity when performing a <see cref="MapToSrce(TDest, OperationTypes)"/>.
        /// </summary>
        /// <param name="destinationEntity">The destination entity.</param>
        /// <param name="sourceEntity">The source entity.</param>
        /// <param name="operationType">The single <see cref="Mapper.OperationTypes"/> being performed to enable selection.</param>
        /// <returns>The source entity.</returns>
        protected virtual TSrce OnMapToSrce(TDest destinationEntity, TSrce sourceEntity, OperationTypes operationType)
        {
            return sourceEntity;
        }

        /// <summary>
        /// Gets the <see cref="IPropertySrceMapper{TSrce}"/> mapping by source property name.
        /// </summary>
        /// <param name="name">The source property name.</param>
        /// <returns>The <see cref="IPropertySrceMapper{TSrce}"/> where found; otherwise, <c>null</c>.</returns>
        public IPropertyMapper<TSrce, TDest> GetBySrcePropertyName(string name)
        {
            if (!_srceMappings.ContainsKey(name))
                return null;

            return _srceMappings[name];
        }

        /// <summary>
        /// Gets the <see cref="IPropertySrceMapper{TSrce}"/> mapping by destination property name.
        /// </summary>
        /// <param name="name">The source property name.</param>
        /// <returns>The <see cref="IPropertySrceMapper{TSrce}"/> where found; otherwise, <c>null</c>.</returns>
        public IPropertyMapper<TSrce, TDest> GetByDestPropertyName(string name)
        {
            if (!_destMappings.ContainsKey(name))
                return null;

            return _destMappings[name];
        }

        /// <summary>
        /// Gets the <see cref="IPropertyMapperBase"/> mapping by source property name.
        /// </summary>
        /// <param name="name">The source property name.</param>
        /// <returns>The <see cref="IPropertyMapperBase"/> where found; otherwise, <c>null</c>.</returns>
        IPropertyMapperBase IEntityMapperBase.GetBySrcePropertyName(string name)
        {
            return GetBySrcePropertyName(name);
        }

        /// <summary>
        /// Gets the <see cref="IPropertyMapperBase"/> mapping by destination property name.
        /// </summary>
        /// <param name="name">The source property name.</param>
        /// <returns>The <see cref="IPropertyMapperBase"/> where found; otherwise, <c>null</c>.</returns>
        IPropertyMapperBase IEntityMapperBase.GetByDestPropertyName(string name)
        {
            return GetByDestPropertyName(name);
        }

        /// <summary>
        /// Gets the properties that form the unique key.
        /// </summary>
        public IPropertyMapper<TSrce, TDest>[] UniqueKey
        {
            get
            {
                if (_uniqueKey != null)
                    return _uniqueKey;

                _uniqueKey = Mappings.Where(x => x.IsUniqueKey).ToArray();
                return _uniqueKey;
            }
        }
    }

    /// <summary>
    /// Provides <see cref="EntityMapper{TSrce, TDest}"/> with a singleton <see cref="Default"/>.
    /// </summary>
    /// <typeparam name="TSrce">The source entity <see cref="Type"/>.</typeparam>
    /// <typeparam name="TDest">The destination entity <see cref="Type"/>.</typeparam>
    /// <typeparam name="TMapper">The entity mapper <see cref="Type"/>.</typeparam>
    public abstract class EntityMapper<TSrce, TDest, TMapper> : EntityMapper<TSrce, TDest>
        where TSrce : class, new()
        where TDest : class, new()
        where TMapper : EntityMapper<TSrce, TDest, TMapper>, new()
    {
        private static readonly TMapper _default = new TMapper();

        /// <summary>
        /// Gets the current instance of the mapper.
        /// </summary>
        public static TMapper Default
        {
            get
            {
                if (_default == null)
                    throw new InvalidOperationException("An instance of this Mapper cannot be referenced as it is still being constructed; beware that you may have a circular reference within the constructor.");

                return _default;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EntityMapper{TSrce, TDest, TMapper}"/> class.
        /// </summary>
        /// <param name="autoMap">Indicates whether the two entities should automatically map where the properties share the same name and <see cref="Type"/>.</param>
        public EntityMapper(bool autoMap = false) : base(autoMap) { }
    }
}
