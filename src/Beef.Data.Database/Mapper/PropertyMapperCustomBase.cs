﻿// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.Mapper;
using Beef.Mapper.Converters;
using Beef.Reflection;
using System;
using System.Linq.Expressions;
using System.Reflection;

namespace Beef.Data.Database.Mapper
{
    /// <summary>
    /// Provides property mapper capabilities for a source entity property and <b>custom</b> destination (for use where the destination is not a .NET Type).
    /// </summary>
    /// <typeparam name="TSrce">The source entity <see cref="Type"/>.</typeparam>
    /// <typeparam name="TSrceProperty">The source property <see cref="Type"/>.</typeparam>
    public abstract class PropertyMapperCustomBase<TSrce, TSrceProperty> : IPropertySrceMapper<TSrce>
    {
        private Func<TSrce, bool>? _mapSrceToDestWhen;

        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyMapperCustomBase{TEntity, TProperty}"/> class.
        /// </summary>
        /// <param name="srcePropertyExpression">The <see cref="LambdaExpression"/> to reference the source entity property.</param>
        /// <param name="destPropertyName">The name of the destination property (defaults to <see cref="SrcePropertyName"/> where null).</param>
        /// <param name="operationTypes">The <see cref="Beef.Mapper.OperationTypes"/> selection to enable inclusion or exclusion of property (default to <see cref="OperationTypes.Any"/>).</param>
        protected PropertyMapperCustomBase(Expression<Func<TSrce, TSrceProperty>> srcePropertyExpression, string? destPropertyName = null, OperationTypes operationTypes = OperationTypes.Any)
        {
            SrcePropertyExpression = PropertyExpression.Create(srcePropertyExpression ?? throw new ArgumentNullException(nameof(srcePropertyExpression)));
            SrcePropertyInfo = TypeReflector.GetPropertyInfo(typeof(TSrce), SrcePropertyName) ?? throw new ArgumentException($"Property '{SrcePropertyName}' does not exist for Type.", nameof(destPropertyName));
            DestPropertyName = string.IsNullOrEmpty(destPropertyName) ? SrcePropertyExpression.Name : destPropertyName;
            OperationTypes = operationTypes;

            if (SrcePropertyInfo.PropertyType.IsClass && SrcePropertyInfo.PropertyType != typeof(string))
                SrceComplexTypeReflector = ComplexTypeReflector.Create(SrcePropertyInfo);
        }

        /// <summary>
        /// Gets the compiled source <see cref="PropertyExpression{TEntity, TProperty}"/>.
        /// </summary>
        protected PropertyExpression<TSrce, TSrceProperty> SrcePropertyExpression { get; private set; }

        /// <summary>
        /// Gets the <see cref="ComplexTypeReflector"/> (only set where the property <see cref="IsSrceComplexType"/>).
        /// </summary>
        public ComplexTypeReflector? SrceComplexTypeReflector { get; private set; }

        /// <summary>
        /// Indicates whether the property is a complex type or complex type collection (see <see cref="SrceComplexTypeReflector"/>).
        /// </summary>
        public bool IsSrceComplexType => SrceComplexTypeReflector != null;

        /// <summary>
        /// Gets the corresponding <see cref="PropertyInfo"/>;
        /// </summary>
        protected PropertyInfo SrcePropertyInfo { get; private set; }

        /// <summary>
        /// Gets the source property name.
        /// </summary>
        public string SrcePropertyName { get { return SrcePropertyExpression.Name; } }

        /// <summary>
        /// Gets the source property <see cref="Type"/>.
        /// </summary>
        public Type SrcePropertyType => typeof(TSrceProperty);

        /// <summary>
        /// Gets the destination property <see cref="Type"/>.
        /// </summary>
        public virtual Type DestPropertyType { get => throw new InvalidOperationException("DestPropertyType has not been implemented."); }

        /// <summary>
        /// Gets the destination property name.
        /// </summary>
        public string DestPropertyName { get; protected set; }

        /// <summary>
        /// Indicates whether the property forms part of the unique (primary) key. 
        /// </summary>
        public bool IsUniqueKey { get; protected set; }

        /// <summary>
        /// Indicates whether the destination (unique key) property value is auto-generated on create (defaults to <c>true</c>). 
        /// </summary>
        public bool IsUniqueKeyAutoGeneratedOnCreate { get; protected set; } = true;

        /// <summary>
        /// Sets the unique key (<see cref="IsUniqueKey"/> and <see cref="IsUniqueKeyAutoGeneratedOnCreate"/>).
        /// </summary>
        /// <param name="autoGeneratedOnCreate">Indicates whether the destination property value is auto-generated on create (defaults to <c>true</c>).</param>
        void IPropertyMapperBase.SetUniqueKey(bool autoGeneratedOnCreate) => SetUniqueKey(autoGeneratedOnCreate);

        /// <summary>
        /// Sets the unique key (enables fluent-style).
        /// </summary>
        /// <param name="autoGeneratedOnCreate">Indicates whether the destination property value is auto-generated on create (defaults to <c>true</c>).</param>
        /// <returns>The <see cref="PropertyMapperCustomBase{TSrce, TSrceProperty}"/>.</returns>
        public PropertyMapperCustomBase<TSrce, TSrceProperty> SetUniqueKey(bool autoGeneratedOnCreate = true)
        {
            if (IsSrceComplexType && autoGeneratedOnCreate)
                throw new InvalidOperationException("A Unique Key with AutoGeneratedOnCreate cannot be set for a Property where IsSrceComplexType is true.");

            IsUniqueKey = true;
            IsUniqueKeyAutoGeneratedOnCreate = autoGeneratedOnCreate;
            return this;
        }

        /// <summary>
        /// Gets or sets the <see cref="IPropertyMapperConverter"/> (used where a specific source and destination type conversion is required).
        /// </summary>
        public IPropertyMapperConverter? Converter { get; private set; }

        /// <summary>
        /// Sets the <see cref="Converter"/>.
        /// </summary>
        /// <param name="converter">The <see cref="IPropertyMapperConverter"/>.</param>
        void IPropertyMapperBase.SetConverter(IPropertyMapperConverter converter)
        {
            if (converter == null)
                throw new ArgumentNullException(nameof(converter));

            if (Mapper != null)
                throw new MapperException("The Mapper and Converter cannot be both set; only one is permissible.");

            if (converter.SrceType != typeof(TSrceProperty))
                throw new MapperException($"The PropertyMapper SrceType '{typeof(TSrceProperty).Name}' and Converter SrceType '{converter.SrceType.Name}' must match.");

            Converter = converter;
        }

        /// <summary>
        /// Sets the <see cref="IPropertyMapperConverter{TSrceProperty, TDestProperty}"/> (used where a specific source and destination type conversion is required).
        /// </summary>
        /// <typeparam name="TDestProperty">The destination property <see cref="Type"/>.</typeparam>
        /// <param name="converter">The <see cref="IPropertyMapperConverter{TSrceProperty, TDestProperty}"/>.</param>
        /// <returns>The <see cref="PropertyMapperCustomBase{TSrce, TSrceProperty}"/>.</returns>
        /// <remarks>The <see cref="Mapper"/> and <see cref="Converter"/> are mutually exclusive.</remarks>
        public PropertyMapperCustomBase<TSrce, TSrceProperty> SetConverter<TDestProperty>(IPropertyMapperConverter<TSrceProperty, TDestProperty> converter)
        {
            if (Mapper != null && converter != null)
                throw new MapperException("The Mapper and Converter cannot be both set; only one is permissible.");

            Converter = converter;
            return this;
        }

        /// <summary>
        /// Gets the <see cref="IEntityMapperBase"/> to map complex types.
        /// </summary>
        public IEntityMapperBase? Mapper { get; private set; }

        /// <summary>
        /// Sets the <see cref="IEntityMapperBase"/> to map complex types.
        /// </summary>
        /// <param name="mapper">The <see cref="IEntityMapperBase"/>.</param>
        /// <remarks>The <see cref="Mapper"/> and <see cref="Converter"/> are mutually exclusive.</remarks>
        void IPropertyMapperBase.SetMapper(IEntityMapperBase mapper) => SetMapper(mapper);

        /// <summary>
        /// Sets the <see cref="IEntityMapperBase"/> to map complex types.
        /// </summary>
        /// <param name="mapper">The <see cref="IEntityMapperBase"/>.</param>
        /// <returns>The <see cref="PropertyMapperCustomBase{TSrce, TSrceProperty}"/>.</returns>
        /// <remarks>The <see cref="Mapper"/> and <see cref="Converter"/> are mutually exclusive.</remarks>
        public PropertyMapperCustomBase<TSrce, TSrceProperty> SetMapper(IEntityMapperBase mapper)
        {
            if (mapper == null)
                throw new ArgumentNullException(nameof(mapper));

            if (Converter != null)
                throw new MapperException("The Mapper and Converter cannot be both set; only one is permissible.");

            if (!IsSrceComplexType)
                throw new MapperException($"The PropertyMapper SrceType '{typeof(TSrceProperty).Name}' must be a complex type to set a Mapper.");

            if (mapper.SrceType != SrceComplexTypeReflector!.ItemType)
                throw new MapperException($"The PropertyMapper SrceType '{typeof(TSrceProperty).Name}' has an ItemType of '{SrceComplexTypeReflector.ItemType.Name}' which must be the same as the underlying EntityMapper SrceType '{mapper.SrceType.Name}'.");

            Mapper = mapper;
            return this;
        }

        /// <summary>
        /// Gets or sets the <see cref="Beef.Mapper.OperationTypes"/> selection to enable inclusion or exclusion of property (default to <see cref="OperationTypes.Any"/>).
        /// </summary>
        public OperationTypes OperationTypes { get; set; } = OperationTypes.Any;

        /// <summary>
        /// Sets the <see cref="OperationTypes"/>.
        /// </summary>
        /// <param name="operationTypes">The <see cref="Beef.Mapper.OperationTypes"/> selection to enable inclusion or exclusion of property.</param>
        void IPropertyMapperBase.SetOperationTypes(OperationTypes operationTypes) => SetOperationTypes(operationTypes);

        /// <summary>
        /// Sets the <see cref="OperationTypes"/>.
        /// </summary>
        /// <param name="operationTypes"></param>
        /// <returns>The <see cref="Beef.Mapper.OperationTypes"/> selection to enable inclusion or exclusion of property.</returns>
        public PropertyMapperCustomBase<TSrce, TSrceProperty> SetOperationTypes(OperationTypes operationTypes)
        {
            OperationTypes = operationTypes;
            return this;
        }

        /// <summary>
        /// Defines a conditional clause which must be <c>true</c> when mapping from the source to the destination.
        /// </summary>
        /// <param name="predicate">A function to determine whether the property is to be mapped.</param>
        /// <returns>The <see cref="PropertyMapperCustomBase{TSrce, TSrceProperty}"/>.</returns>
        public PropertyMapperCustomBase<TSrce, TSrceProperty> MapSrceToDestWhen(Func<TSrce, bool> predicate)
        {
            _mapSrceToDestWhen = predicate;
            return this;
        }

        /// <summary>
        /// Invokes the <see cref="MapSrceToDestWhen(Func{TSrce, bool})"/> clause to determine whether mapping from the source to the destination should occur.
        /// </summary>
        /// <param name="entity">The entity value.</param>
        /// <returns><c>true</c> indicates that the mapping should occur; otherwise, <c>false</c>.</returns>
        bool IPropertySrceMapper<TSrce>.MapSrceToDestWhen(TSrce entity) => (_mapSrceToDestWhen == null) || _mapSrceToDestWhen.Invoke(entity);

        /// <summary>
        /// Gets the source property value.
        /// </summary>
        /// <param name="entity">The entity value.</param>
        /// <param name="operationType">The single <see cref="Beef.Mapper.OperationTypes"/> being performed to enable selection.</param>
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
        /// <param name="operationType">The single <see cref="Beef.Mapper.OperationTypes"/> being performed to enable selection.</param>
        protected void SetSrceValue(TSrce entity, TSrceProperty value, OperationTypes operationType)
        {
            if (OperationTypes.HasFlag(operationType))
                SrcePropertyInfo.SetValue(entity, value);
        }

        /// <summary>
        /// Gets the source property value.
        /// </summary>
        /// <param name="entity">The entity value.</param>
        /// <param name="operationType">The single <see cref="Beef.Mapper.OperationTypes"/> being performed to enable selection.</param>
        /// <returns>The property value.</returns>
        object? IPropertySrceMapper<TSrce>.GetSrceValue(TSrce entity, OperationTypes operationType) => GetSrceValue(entity, operationType);

        /// <summary>
        /// Sets the source property value.
        /// </summary>
        /// <param name="entity">The entity value.</param>
        /// <param name="value">The property value.</param>
        /// <param name="operationType">The single <see cref="Beef.Mapper.OperationTypes"/> being performed to enable selection.</param>
        void IPropertySrceMapper<TSrce>.SetSrceValue(TSrce entity, object? value, OperationTypes operationType) => SetSrceValue(entity, (TSrceProperty)value!, operationType);

        /// <summary>
        /// Maps the source to the destination updating an existing object.
        /// </summary>
        /// <param name="sourceEntity">The source entity.</param>
        /// <param name="destinationEntity">The destination entity.</param>
        /// <param name="operationType">The single <see cref="Beef.Mapper.OperationTypes"/> being performed to enable selection.</param>
        void IPropertySrceMapper<TSrce>.MapToDest(TSrce sourceEntity, object destinationEntity, OperationTypes operationType) => throw new NotSupportedException();
    }
}