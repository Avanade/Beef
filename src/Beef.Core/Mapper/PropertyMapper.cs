// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

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
    /// Enables two-entity (source and destination) property mapping capabilities.
    /// </summary>
    /// <typeparam name="TSrce">The source entity <see cref="Type"/>.</typeparam>
    /// <typeparam name="TDest">The destination entity <see cref="Type"/>.</typeparam>
    public interface IPropertyMapper<TSrce, TDest> : IPropertySrceMapper<TSrce>
    {
        /// <summary>
        /// Gets the destination property value.
        /// </summary>
        /// <param name="entity">The entity value.</param>
        /// <param name="operationType">The single <see cref="Mapper.OperationTypes"/> being performed to enable selection.</param>
        /// <returns>The property value.</returns>
        object? GetDestValue(TDest entity, OperationTypes operationType);

        /// <summary>
        /// Sets the destination property value.
        /// </summary>
        /// <param name="entity">The entity value.</param>
        /// <param name="value">The property value.</param>
        /// <param name="operationType">The single <see cref="Mapper.OperationTypes"/> being performed to enable selection.</param>
        void SetDestValue(TDest entity, object? value, OperationTypes operationType);

        /// <summary>
        /// Invokes the underlying <b>when</b> clause to determine whether the destination mapping should occur.
        /// </summary>
        /// <param name="entity">The entity value.</param>
        /// <returns><c>true</c> indicates that the mapping should occur; otherwise, <c>false</c>.</returns>
        bool MapDestToSrceWhen(TDest entity);

        /// <summary>
        /// Maps the source property to the destination property.
        /// </summary>
        /// <param name="srceEntity">The source entity.</param>
        /// <param name="destEntity">The destination entity.</param>
        /// <param name="operationType">The single <see cref="Mapper.OperationTypes"/> being performed to enable selection.</param>
        void MapToDest(TSrce srceEntity, TDest destEntity, OperationTypes operationType);

        /// <summary>
        /// Maps the destination property to the source property.
        /// </summary>
        /// <param name="srceEntity">The source entity.</param>
        /// <param name="destEntity">The destination entity.</param>
        /// <param name="operationType">The single <see cref="Mapper.OperationTypes"/> being performed to enable selection.</param>
        void MapToSrce(TDest destEntity, TSrce srceEntity, OperationTypes operationType);

        /// <summary>
        /// Converts a source property value into the destination value.
        /// </summary>
        /// <param name="sourcePropertyValue">The source property value.</param>
        /// <param name="operationType">The single <see cref="Mapper.OperationTypes"/> being performed to enable selection.</param>
        /// <returns>The destination property value.</returns>
        object? ConvertToDestValue(object? sourcePropertyValue, OperationTypes operationType);

        /// <summary>
        /// Converts a destination property value into the source value.
        /// </summary>
        /// <param name="destinationPropertyValue">The destination property value.</param>
        /// <param name="operationType">The single <see cref="Mapper.OperationTypes"/> being performed to enable selection.</param>
        /// <returns>The destination property value.</returns>
        object? ConvertToSrceValue(object? destinationPropertyValue, OperationTypes operationType);

        /// <summary>
        /// Gets the <see cref="ComplexTypeReflector"/> (only set where the property <see cref="IsDestComplexType"/>).
        /// </summary>
        ComplexTypeReflector? DestComplexTypeReflector { get; }

        /// <summary>
        /// Indicates whether the property is a complex type or complex type collection (see <see cref="DestComplexTypeReflector"/>).
        /// </summary>
        bool IsDestComplexType { get; }
    }

    /// <summary>
    /// Provides property mapper capabilities for a source entity property and destination entity property.
    /// </summary>
    /// <typeparam name="TSrce">The source entity <see cref="Type"/>.</typeparam>
    /// <typeparam name="TSrceProperty">The source property <see cref="Type"/>.</typeparam>
    /// <typeparam name="TDest">The destination entity <see cref="Type"/>.</typeparam>
    /// <typeparam name="TDestProperty">The destination property <see cref="Type"/>.</typeparam>
    public class PropertyMapper<TSrce, TSrceProperty, TDest, TDestProperty> : IPropertyMapper<TSrce, TDest>
        where TSrce : class
        where TDest : class
    {
        private Func<TSrce, bool>? _mapSrceToDestWhen;
        private Func<TDest, bool>? _mapDestToSrceWhen;
        private Func<TSrce, TDest, OperationTypes, TDestProperty>? _mapSrceToDestOverride;
        private Func<TDest, TSrce, OperationTypes, TSrceProperty>? _mapDestToSrceOverride;

        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyMapper{TSrce, TSrceProperty, TDest, TDestProperty}"/> class.
        /// </summary>
        /// <param name="srcePropertyExpression">The <see cref="LambdaExpression"/> to reference the source entity property.</param>
        /// <param name="destPropertyExpression">The <see cref="LambdaExpression"/> to reference the destination entity property.</param>
        /// <param name="operationTypes">The <see cref="Mapper.OperationTypes"/> selection to enable inclusion or exclusion of property (default to <see cref="OperationTypes.Any"/>).</param>
        public PropertyMapper(Expression<Func<TSrce, TSrceProperty>> srcePropertyExpression, Expression<Func<TDest, TDestProperty>> destPropertyExpression, OperationTypes operationTypes = OperationTypes.Any)
            : this(PropertyExpression.Create(srcePropertyExpression ?? throw new ArgumentNullException(nameof(srcePropertyExpression))),
                   PropertyExpression.Create(destPropertyExpression ?? throw new ArgumentNullException(nameof(destPropertyExpression))),
                   operationTypes)
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyMapperCustomBase{TEntity, TProperty}"/> class with pre-compiled expressions.
        /// </summary>
        /// <param name="srcePropertyExpression">The <see cref="LambdaExpression"/> to reference the source entity property.</param>
        /// <param name="destPropertyExpression">The <see cref="LambdaExpression"/> to reference the destination entity property.</param>
        /// <param name="operationTypes">The <see cref="Mapper.OperationTypes"/> selection to enable inclusion or exclusion of property (default to <see cref="OperationTypes.Any"/>).</param>
        internal PropertyMapper(PropertyExpression<TSrce, TSrceProperty> srcePropertyExpression, PropertyExpression<TDest, TDestProperty> destPropertyExpression, OperationTypes operationTypes = OperationTypes.Any)
        {
            SrcePropertyExpression = srcePropertyExpression;
            DestPropertyExpression = destPropertyExpression;

            SrcePropertyInfo = typeof(TSrce).GetProperties().First(x => x.Name == SrcePropertyName && x.PropertyType == typeof(TSrceProperty));
            DestPropertyInfo = typeof(TDest).GetProperties().First(x => x.Name == DestPropertyName && x.PropertyType == typeof(TDestProperty));

            if (SrcePropertyInfo.PropertyType.IsClass && SrcePropertyInfo.PropertyType != typeof(string))
            {
                SrceComplexTypeReflector = ComplexTypeReflector.Create(SrcePropertyInfo);
                if (SrceComplexTypeReflector.IsCollection)
                    SrceUnderlyingPropertyType = SrceComplexTypeReflector.ItemType;
            }

            if (DestPropertyInfo.PropertyType.IsClass && DestPropertyInfo.PropertyType != typeof(string))
            {
                DestComplexTypeReflector = ComplexTypeReflector.Create(DestPropertyInfo);
                if (DestComplexTypeReflector.IsCollection)
                    DestUnderlyingPropertyType = DestComplexTypeReflector.ItemType;
            }

            OperationTypes = operationTypes;
        }

        /// <summary>
        /// Gets the compiled source <see cref="PropertyExpression{TEntity, TProperty}"/>.
        /// </summary>
        protected PropertyExpression<TSrce, TSrceProperty> SrcePropertyExpression { get; private set; }

        /// <summary>
        /// Gets the compiled destination <see cref="PropertyExpression{TEntity, TProperty}"/>.
        /// </summary>
        protected PropertyExpression<TDest, TDestProperty> DestPropertyExpression { get; private set; }

        /// <summary>
        /// Indicates whether the source and destination properties are the same <see cref="Type"/>.
        /// </summary>
        protected bool AreSameType { get => SrceUnderlyingPropertyType == DestUnderlyingPropertyType; }

        /// <summary>
        /// Gets the corresponding source <see cref="PropertyInfo"/>;
        /// </summary>
        protected PropertyInfo SrcePropertyInfo { get; private set; }

        /// <summary>
        /// Gets the corresponding destination <see cref="PropertyInfo"/>;
        /// </summary>
        protected PropertyInfo DestPropertyInfo { get; private set; }

        /// <summary>
        /// Gets the source property name.
        /// </summary>
        public string SrcePropertyName { get { return SrcePropertyExpression.Name; } }

        /// <summary>
        /// Gets the destination property name.
        /// </summary>
        public string DestPropertyName { get { return DestPropertyExpression.Name; } }

        /// <summary>
        /// Gets the source property <see cref="Type"/>.
        /// </summary>
        public Type SrcePropertyType => typeof(TSrceProperty);

        /// <summary>
        /// Gets the destination property <see cref="Type"/>.
        /// </summary>
        public Type DestPropertyType => typeof(TDestProperty);

        /// <summary>
        /// Gets the underlying source property <see cref="Type"/> allowing for nullables.
        /// </summary>
        protected Type SrceUnderlyingPropertyType { get; } = Nullable.GetUnderlyingType(typeof(TSrceProperty)) ?? typeof(TSrceProperty);

        /// <summary>
        /// Gets the underlying destination property <see cref="Type"/> allowing for nullables.
        /// </summary>
        protected Type DestUnderlyingPropertyType { get; } = Nullable.GetUnderlyingType(typeof(TDestProperty)) ?? typeof(TDestProperty);

        /// <summary>
        /// Gets the source <see cref="ComplexTypeReflector"/> (only set where the property <see cref="IsSrceComplexType"/>).
        /// </summary>
        public ComplexTypeReflector? SrceComplexTypeReflector { get; private set; }

        /// <summary>
        /// Indicates whether the source property is a complex type or complex type collection (see <see cref="SrceComplexTypeReflector"/>).
        /// </summary>
        public bool IsSrceComplexType => SrceComplexTypeReflector != null;

        /// <summary>
        /// Gets the destination <see cref="ComplexTypeReflector"/> (only set where the property <see cref="IsDestComplexType"/>).
        /// </summary>
        public ComplexTypeReflector? DestComplexTypeReflector { get; private set; }

        /// <summary>
        /// Indicates whether the destination property is a complex type or complex type collection (see <see cref="SrceComplexTypeReflector"/>).
        /// </summary>
        public bool IsDestComplexType => DestComplexTypeReflector != null;

        #region UniqueKey

        /// <summary>
        /// Indicates whether the property forms part of the unique (primary) key. 
        /// </summary>
        public bool IsUniqueKey { get; private set; }

        /// <summary>
        /// Indicates whether the destination property value is auto-generated on create (defaults to <c>true</c>). 
        /// </summary>
        public bool IsUniqueKeyAutoGeneratedOnCreate { get; protected set; } = true;

        /// <summary>
        /// Sets the unique key (<see cref="IsUniqueKey"/> and <see cref="IsUniqueKeyAutoGeneratedOnCreate"/>).
        /// </summary>
        /// <param name="autoGeneratedOnCreate">Indicates whether the destination property value is auto-generated on create (defaults to <c>true</c>).</param>
        void IPropertyMapperBase.SetUniqueKey(bool autoGeneratedOnCreate)
        {
            SetUniqueKey(autoGeneratedOnCreate);
        }

        #endregion

        /// <summary>
        /// Sets the unique key (enables fluent-style).
        /// </summary>
        /// <param name="autoGeneratedOnCreate">Indicates whether the destination property value is auto-generated on create (defaults to <c>true</c>).</param>
        /// <returns>The <see cref="PropertyMapper{TSrce, TSrceProperty, TDest, TDestProperty}"/>.</returns>
        public PropertyMapper<TSrce, TSrceProperty, TDest, TDestProperty> SetUniqueKey(bool autoGeneratedOnCreate = true)
        {
            IsUniqueKey = true;
            IsUniqueKeyAutoGeneratedOnCreate = autoGeneratedOnCreate;
            return this;
        }

        /// <summary>
        /// Gets the <see cref="IPropertyMapperConverter"/> (used where a specific source and destination type conversion is required).
        /// </summary>
        IPropertyMapperConverter? IPropertyMapperBase.Converter { get; }

        /// <summary>
        /// Gets the <see cref="IPropertyMapperConverter{TSrceProperty, TDestProperty}"/> (used where a specific source and destination type conversion is required).
        /// </summary>
        public IPropertyMapperConverter<TSrceProperty, TDestProperty>? Converter { get; private set; }

        /// <summary>
        /// Sets the <see cref="Converter"/>.
        /// </summary>
        /// <param name="converter">The <see cref="IPropertyMapperConverter"/>.</param>
        /// <remarks>The <see cref="Mapper"/> and <see cref="Converter"/> are mutually exclusive.</remarks>
        void IPropertyMapperBase.SetConverter(IPropertyMapperConverter converter)
        {
            SetConverter((IPropertyMapperConverter<TSrceProperty, TDestProperty>)converter);
        }

        /// <summary>
        /// Sets the <see cref="IPropertyMapperConverter{TSrceProperty, TDestProperty}"/> (used where a specific source and destination type conversion is required).
        /// </summary>
        /// <param name="converter">The <see cref="IPropertyMapperConverter{TSrceProperty, TDestProperty}"/>.</param>
        /// <returns>The <see cref="PropertyMapperCustomBase{TSrce, TSrceProperty}"/>.</returns>
        /// <remarks>The <see cref="Mapper"/> and <see cref="Converter"/> are mutually exclusive.</remarks>
        public PropertyMapper<TSrce, TSrceProperty, TDest, TDestProperty> SetConverter(IPropertyMapperConverter<TSrceProperty, TDestProperty> converter)
        {
            if (Mapper != null)
                throw new MapperException("The Mapper and Converter cannot be both set; only one is permissible.");

            Converter = Check.NotNull(converter, nameof(converter));
            return this;
        }

        /// <summary>
        /// Gets the <see cref="IEntityMapperBase"/> to map complex types.
        /// </summary>
        public IEntityMapperBase? Mapper { get; private set; }

        /// <summary>
        /// Set the <see cref="IEntityMapperBase"/> to map complex types.
        /// </summary>
        /// <param name="mapper">The <see cref="IEntityMapperBase"/>.</param>
        /// <remarks>The <see cref="Mapper"/> and <see cref="Converter"/> are mutually exclusive.</remarks>
        void IPropertyMapperBase.SetMapper(IEntityMapperBase mapper)
        {
            SetMapper(mapper);
        }

        /// <summary>
        /// Set the <see cref="IEntityMapper{TSrce, TDest}"/> to map two properties that are classes.
        /// </summary>
        /// <param name="mapper">The <see cref="PropertyMapper{TSrce, TSrceProperty, TDest, TDestProperty}"/>.</param>
        /// <returns>The <see cref="PropertyMapperCustomBase{TSrce, TSrceProperty}"/>.</returns>
        /// <remarks>The <see cref="Mapper"/> and <see cref="Converter"/> are mutually exclusive.</remarks>
        public PropertyMapper<TSrce, TSrceProperty, TDest, TDestProperty> SetMapper(IEntityMapperBase mapper)
        {
            if (Converter != null)
                throw new MapperException("The Mapper and Converter cannot be both set; only one is permissible.");

            Mapper = Check.NotNull(mapper, nameof(mapper));
            return this;
        }

        /// <summary>
        /// Create a Mapper automatically where complex type to type and not defined previously.
        /// </summary>
        private void CreateAutoMapperIfRequired()
        {
            if (Converter != null || Mapper != null)
                return;

            if (IsSrceComplexType && IsDestComplexType)
            {
                // This is auto-mapping between types as a default.
                if ((!SrceComplexTypeReflector!.IsCollection || SrceComplexTypeReflector.IsItemComplexType)
                    && (!DestComplexTypeReflector!.IsCollection || DestComplexTypeReflector.IsItemComplexType))
                {
                    var mapper = (IEntityMapperBase)typeof(EntityMapper)
                        .GetMethod(nameof(EntityMapper.CreateAuto))
                        .MakeGenericMethod(new Type[] { SrceUnderlyingPropertyType, DestUnderlyingPropertyType })
                        .Invoke(null, new object[] { Array.Empty<string>() });

                    SetMapper(mapper);
                }
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="Mapper.OperationTypes"/> selection to enable inclusion or exclusion of property (default to <see cref="OperationTypes.Any"/>).
        /// </summary>
        public OperationTypes OperationTypes { get; private set; } = OperationTypes.Any;

        /// <summary>
        /// Sets the <see cref="OperationTypes"/>.
        /// </summary>
        /// <param name="operationTypes">The <see cref="Mapper.OperationTypes"/> selection to enable inclusion or exclusion of property.</param>
        void IPropertyMapperBase.SetOperationTypes(OperationTypes operationTypes) => SetOperationTypes(operationTypes);

        /// <summary>
        /// Sets the <see cref="OperationTypes"/>.
        /// </summary>
        /// <param name="operationTypes"></param>
        /// <returns>The <see cref="Mapper.OperationTypes"/> selection to enable inclusion or exclusion of property.</returns>
        public PropertyMapper<TSrce, TSrceProperty, TDest, TDestProperty> SetOperationTypes(OperationTypes operationTypes)
        {
            OperationTypes = operationTypes;
            return this;
        }

        /// <summary>
        /// Defines a conditional clause which must be <c>true</c> when mapping from the source to the destination.
        /// </summary>
        /// <param name="predicate">A function to determine whether the property is to be mapped.</param>
        /// <returns>The <see cref="PropertyMapper{TEntity, TProperty, TDest, TDestProperty}"/>.</returns>
        public PropertyMapper<TSrce, TSrceProperty, TDest, TDestProperty> MapSrceToDestWhen(Func<TSrce, bool> predicate)
        {
            _mapSrceToDestWhen = predicate;
            return this;
        }

        /// <summary>
        /// Invokes the <see cref="MapSrceToDestWhen(Func{TSrce, bool})"/> clause to determine whether mapping from the source to the destination should occur.
        /// </summary>
        /// <param name="entity">The entity value.</param>
        /// <returns><c>true</c> indicates that the mapping should occur; otherwise, <c>false</c>.</returns>
        bool IPropertySrceMapper<TSrce>.MapSrceToDestWhen(TSrce entity)
        {
            return (_mapSrceToDestWhen == null) || _mapSrceToDestWhen.Invoke(entity);
        }

        /// <summary>
        /// Defines a condition clause which must be <c>true</c> when mapping from the destination to the source.
        /// </summary>
        /// <param name="predicate">A function to determine whether the property is to be mapped.</param>
        /// <returns>The <see cref="PropertyMapper{TEntity, TProperty, TDest, TDestProperty}"/>.</returns>
        public PropertyMapper<TSrce, TSrceProperty, TDest, TDestProperty> MapDestToSrceWhen(Func<TDest, bool> predicate)
        {
            _mapDestToSrceWhen = predicate;
            return this;
        }

        /// <summary>
        /// Invokes the <see cref="MapDestToSrceWhen(Func{TDest, bool})"/> clauses to determine whether mapping from the destination to the source should occur.
        /// </summary>
        /// <param name="entity">The entity value.</param>
        /// <returns><c>true</c> indicates that the mapping should occur; otherwise, <c>false</c>.</returns>
        public bool MapDestToSrceWhen(TDest entity)
        {
            return (_mapDestToSrceWhen == null) || _mapDestToSrceWhen.Invoke(entity);
        }

        /// <summary>
        /// Gets the source property value.
        /// </summary>
        /// <param name="entity">The entity value.</param>
        /// <param name="operationType">The single <see cref="Mapper.OperationTypes"/> being performed to enable selection.</param>
        /// <returns>The property value.</returns>
        protected TSrceProperty GetSrceValue(TSrce entity, OperationTypes operationType)
        {
            if (OperationTypes.HasFlag(operationType))
                return SrcePropertyExpression.GetValue(entity);
            else
                return default!;
        }

        /// <summary>
        /// Sets the source property value.
        /// </summary>
        /// <param name="entity">The entity value.</param>
        /// <param name="value">The property value.</param>
        /// <param name="operationType">The single <see cref="Mapper.OperationTypes"/> being performed to enable selection.</param>
        protected void SetSrceValue(TSrce entity, TSrceProperty value, OperationTypes operationType)
        {
            if (OperationTypes.HasFlag(operationType))
            {
                if (SrcePropertyInfo.CanWrite == false && IsSrceComplexType && SrceComplexTypeReflector!.IsCollection && SrceComplexTypeReflector.AddMethod != null)
                {
                    if (value != null)
                    {
                        var coll = SrcePropertyInfo.GetValue(entity);
                        foreach (var item in (System.Collections.IEnumerable)value)
                        {
                            SrceComplexTypeReflector.AddMethod.Invoke(coll, new object[] { item });
                        }
                    }

                    return;
                }

                if (SrcePropertyInfo.CanWrite == false)
                    throw new InvalidOperationException($"Srceination Property '{SrcePropertyName}' cannot be updated as it does not support write.");

                SrcePropertyInfo.SetValue(entity, value);
            }
        }

        /// <summary>
        /// Gets the source property value.
        /// </summary>
        /// <param name="entity">The entity value.</param>
        /// <param name="operationType">The single <see cref="Mapper.OperationTypes"/> being performed to enable selection.</param>
        /// <returns>The property value.</returns>
        object? IPropertySrceMapper<TSrce>.GetSrceValue(TSrce entity, OperationTypes operationType)
        {
            return GetSrceValue(entity, operationType);
        }

        /// <summary>
        /// Sets the source property value.
        /// </summary>
        /// <param name="entity">The entity value.</param>
        /// <param name="value">The property value.</param>
        /// <param name="operationType">The single <see cref="Mapper.OperationTypes"/> being performed to enable selection.</param>
        void IPropertySrceMapper<TSrce>.SetSrceValue(TSrce entity, object? value, OperationTypes operationType)
        {
            SetSrceValue(entity, (TSrceProperty)value!, operationType);
        }

#pragma warning disable IDE0060, CA1801 // Remove unused parameter; by-design to have consistent interface
        /// <summary>
        /// Gets the destination property value.
        /// </summary>
        /// <param name="entity">The entity value.</param>
        /// <param name="operationType">The single <see cref="Mapper.OperationTypes"/> being performed to enable selection.</param>
        /// <returns>The property value.</returns>
        protected TDestProperty GetDestValue(TDest entity, OperationTypes operationType)
#pragma warning restore IDE0060, CA1801
        {
            return DestPropertyExpression.GetValue(entity);
        }

        /// <summary>
        /// Sets the destination property value.
        /// </summary>
        /// <param name="entity">The entity value.</param>
        /// <param name="value">The property value.</param>
        /// <param name="operationType">The single <see cref="Mapper.OperationTypes"/> being performed to enable selection.</param>
        protected void SetDestValue(TDest entity, TDestProperty value, OperationTypes operationType)
        {
            if (OperationTypes.HasFlag(operationType))
            {
                if (DestPropertyInfo.CanWrite == false && IsDestComplexType && DestComplexTypeReflector!.IsCollection && DestComplexTypeReflector.AddMethod != null)
                {
                    if (value != null)
                    {
                        var coll = DestPropertyInfo.GetValue(entity);
                        foreach (var item in (System.Collections.IEnumerable)value)
                        {
                            DestComplexTypeReflector.AddMethod.Invoke(coll, new object[] { item });
                        }
                    }

                    return;
                }

                if (DestPropertyInfo.CanWrite == false)
                    throw new InvalidOperationException($"Destination Property '{DestPropertyName}' cannot be updated as it does not support write.");

                DestPropertyInfo.SetValue(entity, value);
            }
        }

        /// <summary>
        /// Gets the destination property value.
        /// </summary>
        /// <param name="entity">The entity value.</param>
        /// <param name="operationType">The single <see cref="Mapper.OperationTypes"/> being performed to enable selection.</param>
        /// <returns>The property value.</returns>
        object? IPropertyMapper<TSrce, TDest>.GetDestValue(TDest entity, OperationTypes operationType)
        {
            return GetDestValue(entity, operationType);
        }

        /// <summary>
        /// Sets the destination property value.
        /// </summary>
        /// <param name="entity">The entity value.</param>
        /// <param name="value">The property value.</param>
        /// <param name="operationType">The single <see cref="Mapper.OperationTypes"/> being performed to enable selection.</param>
        void IPropertyMapper<TSrce, TDest>.SetDestValue(TDest entity, object? value, OperationTypes operationType)
        {
            SetDestValue(entity, (TDestProperty)value!, operationType);
        }

        /// <summary>
        /// Overrides the source to destination mapping for this <see cref="PropertyMapper{TSrce, TSrceProperty, TDest, TDestProperty}"/>.
        /// </summary>
        /// <param name="func">A function to override the mapping.</param>
        /// <returns>The <see cref="PropertyMapper{TEntity, TProperty, TDest, TDestProperty}"/>.</returns>
        public PropertyMapper<TSrce, TSrceProperty, TDest, TDestProperty> MapToDest(Func<TSrce, TDest, OperationTypes, TDestProperty> func)
        {
            _mapSrceToDestOverride = func;
            return this;
        }

        /// <summary>
        /// Maps the source to the destination updating an existing object.
        /// </summary>
        /// <param name="sourceEntity">The source entity.</param>
        /// <param name="destinationEntity">The destination entity.</param>
        /// <param name="operationType">The single <see cref="Mapper.OperationTypes"/> being performed to enable selection.</param>
        void IPropertySrceMapper<TSrce>.MapToDest(TSrce sourceEntity, object destinationEntity, OperationTypes operationType)
        {
            MapToDest(sourceEntity, (TDest)destinationEntity, operationType);
        }

        /// <summary>
        /// Maps the source property to the destination property.
        /// </summary>
        /// <param name="sourceEntity">The source entity.</param>
        /// <param name="destinationEntity">The destination entity.</param>
        /// <param name="operationType">The single <see cref="Mapper.OperationTypes"/> being performed to enable selection.</param>
        public virtual void MapToDest(TSrce sourceEntity, TDest destinationEntity, OperationTypes operationType)
        {
            if (sourceEntity == null || destinationEntity == null)
                return;

            if (!OperationTypes.HasFlag(operationType))
                return;

            if (MapToDestOverride(sourceEntity, destinationEntity, operationType))
                return;

            TSrceProperty val = GetSrceValue(sourceEntity, operationType);

            CreateAutoMapperIfRequired();

            if (Converter == null && Mapper != null && (IsSrceComplexType && !SrceComplexTypeReflector!.IsCollection) && (IsDestComplexType && !DestComplexTypeReflector!.IsCollection))
            {
                TDestProperty dval = GetDestValue(destinationEntity, operationType);
                if (dval != null)
                {
                    ((IEntityMapper)Mapper).MapToDest(val!, dval, operationType);
                    return;
                }
            }

            SetDestValue(destinationEntity, ConvertToDestValue(val, operationType), operationType);
        }

        /// <summary>
        /// Converts a source property value into the destination value.
        /// </summary>
        /// <param name="sourcePropertyValue">The source property value.</param>
        /// <param name="operationType">The single <see cref="Mapper.OperationTypes"/> being performed to enable selection.</param>
        /// <returns>The destination property value.</returns>
        object? IPropertyMapper<TSrce, TDest>.ConvertToDestValue(object? sourcePropertyValue, OperationTypes operationType)
        {
            return ConvertToDestValue((TSrceProperty)sourcePropertyValue!, operationType);
        }

        /// <summary>
        /// Converts a source property value into the destination value.
        /// </summary>
        /// <param name="sourcePropertyValue">The source property value.</param>
        /// <param name="operationType">The single <see cref="Mapper.OperationTypes"/> being performed to enable selection.</param>
        /// <returns>The destination property value.</returns>
        public TDestProperty ConvertToDestValue(TSrceProperty sourcePropertyValue, OperationTypes operationType)
        {
            if (!OperationTypes.HasFlag(operationType))
                return default!;

            if (Converter != null)
                return Converter.ConvertToDest(sourcePropertyValue);

            if (sourcePropertyValue == null)
                return default!;

            CreateAutoMapperIfRequired();
            if ((!IsSrceComplexType && !IsDestComplexType)
                || ((IsSrceComplexType && !SrceComplexTypeReflector!.IsCollection) && (IsDestComplexType && !DestComplexTypeReflector!.IsCollection)))
            {
                if (Mapper != null)
                    return (TDestProperty)((IEntityMapper)Mapper).MapToDest(sourcePropertyValue, operationType)!;

                return (TDestProperty)Convert.ChangeType(sourcePropertyValue, DestUnderlyingPropertyType, System.Globalization.CultureInfo.InvariantCulture);
            }

            if (SrceComplexTypeReflector!.IsCollection)
            {
                var c = new List<object?>();
                foreach (var item in (System.Collections.IEnumerable)sourcePropertyValue)
                {
                    if (Mapper != null)
                        c.Add(((IEntityMapper)Mapper).MapToDest(item, operationType));
                    else
                        c.Add(Convert.ChangeType(item, DestUnderlyingPropertyType, System.Globalization.CultureInfo.InvariantCulture));
                }

                return (TDestProperty)DestComplexTypeReflector!.CreateValue(c);
            }

            try
            {
                return (TDestProperty)Convert.ChangeType(sourcePropertyValue, DestUnderlyingPropertyType, System.Globalization.CultureInfo.InvariantCulture);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Property mapping between types '{SrcePropertyInfo.Name}' and '{DestPropertyInfo.Name}' cannot be performed; consider using a Converter or Mapper.", ex);
            }
        }

        /// <summary>
        /// Performs the source to destination mapping override where specified.
        /// </summary>
        /// <param name="sourceEntity">The source entity.</param>
        /// <param name="destinationEntity">The destination entity.</param>
        /// <param name="operationType">The single <see cref="Mapper.OperationTypes"/> being performed to enable selection.</param>
        /// <returns><c>true</c> indicates that the mapping was performed (overridden); otherwise, <c>false</c>.</returns>
        protected bool MapToDestOverride(TSrce sourceEntity, TDest destinationEntity, OperationTypes operationType)
        {
            if (_mapSrceToDestOverride == null)
                return false;

            SetDestValue(destinationEntity, _mapSrceToDestOverride(sourceEntity, destinationEntity, operationType), operationType);
            return true;
        }

        /// <summary>
        /// Overrides the destination to source mapping for this <see cref="PropertyMapper{TSrce, TSrceProperty, TDest, TDestProperty}"/>.
        /// </summary>
        /// <param name="func">A function to override the mapping.</param>
        /// <returns>The <see cref="PropertyMapper{TEntity, TProperty, TDest, TDestProperty}"/>.</returns>
        public PropertyMapper<TSrce, TSrceProperty, TDest, TDestProperty> MapToSrce(Func<TDest, TSrce, OperationTypes, TSrceProperty> func)
        {
            _mapDestToSrceOverride = func;
            return this;
        }

        /// <summary>
        /// Maps the destination property to the source property.
        /// </summary>
        /// <param name="sourceEntity">The source entity.</param>
        /// <param name="destinationEntity">The destination entity.</param>
        /// <param name="operationType">The single <see cref="Mapper.OperationTypes"/> being performed to enable selection.</param>
        public virtual void MapToSrce(TDest destinationEntity, TSrce sourceEntity, OperationTypes operationType)
        {
            if (sourceEntity == null || destinationEntity == null)
                return;

            if (!OperationTypes.HasFlag(operationType))
                return;

            if (MapToSrceOverride(destinationEntity, sourceEntity, operationType))
                return;

            TDestProperty val = GetDestValue(destinationEntity, operationType);
            SetSrceValue(sourceEntity, ConvertToSrceValue(val, operationType), operationType);
        }

        /// <summary>
        /// Converts a source property value into the destination value.
        /// </summary>
        /// <param name="sourcePropertyValue">The source property value.</param>
        /// <param name="operationType">The single <see cref="Mapper.OperationTypes"/> being performed to enable selection.</param>
        /// <returns>The destination property value.</returns>
        object? IPropertyMapper<TSrce, TDest>.ConvertToSrceValue(object? sourcePropertyValue, OperationTypes operationType)
        {
            return ConvertToDestValue((TSrceProperty)sourcePropertyValue!, operationType);
        }

        /// <summary>
        /// Converts a destination property value into the source value.
        /// </summary>
        /// <param name="destinationPropertyValue">The destination property value.</param>
        /// <param name="operationType">The single <see cref="Mapper.OperationTypes"/> being performed to enable selection.</param>
        /// <returns>The source property value.</returns>
        public TSrceProperty ConvertToSrceValue(TDestProperty destinationPropertyValue, OperationTypes operationType)
        {
            if (!OperationTypes.HasFlag(operationType))
                return default!;

            if (Converter != null)
                return Converter.ConvertToSrce(destinationPropertyValue);

            if (destinationPropertyValue == null)
                return default!;

            CreateAutoMapperIfRequired();
            if ((!IsSrceComplexType && !IsDestComplexType)
                || ((IsSrceComplexType && !SrceComplexTypeReflector!.IsCollection) && (IsDestComplexType && !DestComplexTypeReflector!.IsCollection)))
            {
                if (Mapper != null)
                    return (TSrceProperty)((IEntityMapper)Mapper).MapToSrce(destinationPropertyValue, operationType)!;

                return (TSrceProperty)Convert.ChangeType(destinationPropertyValue, SrceUnderlyingPropertyType, System.Globalization.CultureInfo.InvariantCulture);
            }

            if (IsSrceComplexType && SrceComplexTypeReflector!.IsCollection)
            {
                var c = new List<object>();
                foreach (var item in (System.Collections.IEnumerable)destinationPropertyValue)
                {
                    if (Mapper != null)
                        c.Add(((IEntityMapper)Mapper).MapToSrce(item, operationType)!);
                    else
                        c.Add(Convert.ChangeType(item, SrceUnderlyingPropertyType, System.Globalization.CultureInfo.InvariantCulture));
                }

                return (TSrceProperty)SrceComplexTypeReflector.CreateValue(c);
            }

            try
            {
                return (TSrceProperty)Convert.ChangeType(destinationPropertyValue, SrceUnderlyingPropertyType, System.Globalization.CultureInfo.InvariantCulture);
            }
            catch (Exception ex)
            {
                throw new InvalidCastException($"Property mapping between types '{DestPropertyInfo.Name}' and '{SrcePropertyInfo.Name}' cannot be performed; consider using a Converter or Mapper.", ex);
            }
        }

        /// <summary>
        /// Performs the destination to source mapping override where specified.
        /// </summary>
        /// <param name="destinationEntity">The destination entity.</param>
        /// <param name="sourceEntity">The source entity.</param>
        /// <param name="operationType">The single <see cref="Mapper.OperationTypes"/> being performed to enable selection.</param>
        /// <returns><c>true</c> indicates that the mapping was performed (overridden); otherwise, <c>false</c>.</returns>
        protected bool MapToSrceOverride(TDest destinationEntity, TSrce sourceEntity, OperationTypes operationType)
        {
            if (_mapDestToSrceOverride == null)
                return false;

            SetSrceValue(sourceEntity, _mapDestToSrceOverride(destinationEntity, sourceEntity, operationType), operationType);
            return true;
        }
    }
}