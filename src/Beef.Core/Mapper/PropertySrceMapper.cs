using Beef.Mapper.Converters;
using Beef.Reflection;
using System;
using System.Linq.Expressions;
using System.Reflection;

namespace Beef.Mapper
{
    /// <summary>
    /// Provides property mapper capabilities for a source entity property and destination entity.
    /// </summary>
    /// <typeparam name="TSrce">The source entity <see cref="Type"/>.</typeparam>
    /// <typeparam name="TSrceProperty">The source property <see cref="Type"/>.</typeparam>
    /// <typeparam name="TDest">The destination entity <see cref="Type"/>.</typeparam>
    public class PropertySrceMapper<TSrce, TSrceProperty, TDest> : IPropertyMapper<TSrce, TDest>
        where TSrce : class
        where TSrceProperty : class
        where TDest : class
    {
        private Func<TSrce, bool> _mapSrceToDestWhen;
        private Func<TDest, bool> _mapDestToSrceWhen;
        private Action<TSrce, TDest, OperationTypes> _mapSrceToDestOverride;
        private Func<TDest, TSrce, OperationTypes, TSrceProperty> _mapDestToSrceOverride;

        /// <summary>
        /// Initializes a new instance of the <see cref="PropertySrceMapper{TEntity, TProperty, TDest}"/> class.
        /// </summary>
        /// <param name="srcePropertyExpression">The <see cref="LambdaExpression"/> to reference the source entity property.</param>
        /// <param name="operationTypes">The <see cref="Mapper.OperationTypes"/> selection to enable inclusion or exclusion of property (default to <see cref="OperationTypes.Any"/>).</param>
        public PropertySrceMapper(Expression<Func<TSrce, TSrceProperty>> srcePropertyExpression, OperationTypes operationTypes = OperationTypes.Any)
            : this(PropertyExpression<TSrce, TSrceProperty>.Create(srcePropertyExpression ?? throw new ArgumentNullException(nameof(srcePropertyExpression))), operationTypes)
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="PropertySrceMapper{TEntity, TProperty, TDest}"/> class with pre-compiled expressions.
        /// </summary>
        /// <param name="srcePropertyExpression">The <see cref="LambdaExpression"/> to reference the source entity property.</param>
        /// <param name="operationTypes">The <see cref="Mapper.OperationTypes"/> selection to enable inclusion or exclusion of property (default to <see cref="OperationTypes.Any"/>).</param>
        internal PropertySrceMapper(PropertyExpression<TSrce, TSrceProperty> srcePropertyExpression, OperationTypes operationTypes = OperationTypes.Any)
        {
            SrcePropertyExpression = srcePropertyExpression;
            SrcePropertyInfo = typeof(TSrce).GetProperty(SrcePropertyName);

            if (SrcePropertyInfo.PropertyType.IsClass && SrcePropertyInfo.PropertyType != typeof(string))
                SrceComplexTypeReflector = ComplexTypeReflector.Create(SrcePropertyInfo);

            OperationTypes = operationTypes;
        }

        /// <summary>
        /// Gets the compiled source <see cref="PropertyExpression{TEntity, TProperty}"/>.
        /// </summary>
        protected PropertyExpression<TSrce, TSrceProperty> SrcePropertyExpression { get; private set; }

        /// <summary>
        /// Gets the <see cref="T:CollectionTypeReflector"/> (only set where the property <see cref="IsSrceComplexType"/>).
        /// </summary>
        public ComplexTypeReflector SrceComplexTypeReflector { get; private set; }

        /// <summary>
        /// Indicates whether the property is a complex type or complex type collection (see <see cref="SrceComplexTypeReflector"/>).
        /// </summary>
        public bool IsSrceComplexType => true;

        /// <summary>
        /// Gets the corresponding source <see cref="PropertyInfo"/>;
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
        /// Gets the destination property name.
        /// </summary>
        string IPropertyMapperBase.DestPropertyName => null;

        /// <summary>
        /// Gets the <see cref="T:CollectionTypeReflector"/> (only set where the property <see cref="IPropertyMapper{TSrce, TDest}.IsDestComplexType"/>).
        /// </summary>
        ComplexTypeReflector IPropertyMapper<TSrce, TDest>.DestComplexTypeReflector { get; }

        /// <summary>
        /// Indicates whether the property is a complex type or complex type collection (see <see cref="IPropertyMapper{TSrce, TDest}.DestComplexTypeReflector"/>).
        /// </summary>
        bool IPropertyMapper<TSrce, TDest>.IsDestComplexType { get; } = false;

        #region UniqueKey

        /// <summary>
        /// Indicates whether the property forms part of the unique (primary) key. 
        /// </summary>
        bool IPropertyMapperBase.IsUniqueKey => false;

        /// <summary>
        /// Indicates whether the destination property value is auto-generated on create. 
        /// </summary>
        bool IPropertyMapperBase.IsUniqueKeyAutoGeneratedOnCreate => throw new NotSupportedException();

        /// <summary>
        /// Sets the unique key (<see cref="IPropertyMapperBase.IsUniqueKey"/> and <see cref="IPropertyMapperBase.IsUniqueKeyAutoGeneratedOnCreate"/>).
        /// </summary>
        /// <param name="autoGeneratedOnCreate">Indicates whether the destination property value is auto-generated on create (defaults to <c>true</c>).</param>
        void IPropertyMapperBase.SetUniqueKey(bool autoGeneratedOnCreate) => throw new NotSupportedException();

        #endregion

        #region Converter

        /// <summary>
        /// Gets the <see cref="IPropertyMapperConverter{TSrceProperty, TDestProperty}"/>.
        /// </summary>
        IPropertyMapperConverter IPropertyMapperBase.Converter => throw new NotSupportedException();

        /// <summary>
        /// Sets the <see cref="IPropertyMapperBase.Converter"/>.
        /// </summary>
        /// <param name="converter">The <see cref="IPropertyMapperConverter"/>.</param>
        /// <remarks>The <see cref="IPropertyMapperBase.Converter"/> and <see cref="Mapper"/> are mutually exclusive.</remarks>
        void IPropertyMapperBase.SetConverter(IPropertyMapperConverter converter) => throw new NotSupportedException();

        #endregion

        #region Mapper

        /// <summary>
        /// Gets the <see cref="IEntityMapperBase"/> to map complex types.
        /// </summary>
        IEntityMapperBase IPropertyMapperBase.Mapper { get => Mapper; }

        /// <summary>
        /// Gets the <see cref="IEntityMapperBase"/> to map two properties that are classes.
        /// </summary>
        public IEntityMapper<TSrceProperty, TDest> Mapper { get; private set; }

        /// <summary>
        /// Set the <see cref="IEntityMapperBase"/> to map complex types.
        /// </summary>
        /// <param name="mapper">The <see cref="IEntityMapperBase"/>.</param>
        /// <remarks>The <see cref="Mapper"/> and <see cref="IPropertyMapperBase.Converter"/> are mutually exclusive.</remarks>
        void IPropertyMapperBase.SetMapper(IEntityMapperBase mapper) => SetMapper((IEntityMapper<TSrceProperty, TDest>)mapper);

        /// <summary>
        /// Set the <see cref="IEntityMapper{TSrceProperty, TDest}"/> to map two properties that are classes.
        /// </summary>
        /// <param name="mapper">The <see cref="PropertySrceMapper{TSrce, TSrceProperty, TDest}"/>.</param>
        /// <returns>The <see cref="PropertyMapperCustomBase{TSrce, TSrceProperty}"/>.</returns>
        public PropertySrceMapper<TSrce, TSrceProperty, TDest> SetMapper(IEntityMapper<TSrceProperty, TDest> mapper)
        {
            if (mapper != null)
            {
                if (!IsSrceComplexType)
                    throw new MapperException($"The PropertyMapper SrceType '{typeof(TSrceProperty).Name}' must be a complex type to set a Mapper.");

                if (mapper.SrceType != SrceComplexTypeReflector.ItemType)
                    throw new MapperException($"The PropertyMapper SrceType '{typeof(TSrceProperty).Name}' has an ItemType of '{SrceComplexTypeReflector.ItemType.Name}' which must be the same as the underlying EntityMapper SrceType '{mapper.SrceType.Name}'.");
            }

            Mapper = mapper;
            return this;
        }

        #endregion

        #region OperationTypes

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
        public PropertySrceMapper<TSrce, TSrceProperty, TDest> SetOperationTypes(OperationTypes operationTypes)
        {
            OperationTypes = operationTypes;
            return this;
        }

        #endregion

        #region Srce

        /// <summary>
        /// Gets the source property value.
        /// </summary>
        /// <param name="entity">The entity value.</param>
        /// <param name="operationType">The single <see cref="Mapper.OperationTypes"/> being performed to enable selection.</param>
        /// <returns>The property value.</returns>
        object IPropertySrceMapper<TSrce>.GetSrceValue(TSrce entity, OperationTypes operationType)
        {
            return GetSrceValue(entity, operationType);
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
                return default(TSrceProperty);
        }

        /// <summary>
        /// Sets the source property value.
        /// </summary>
        /// <param name="entity">The entity value.</param>
        /// <param name="value">The property value.</param>
        /// <param name="operationType">The single <see cref="Mapper.OperationTypes"/> being performed to enable selection.</param>
        void IPropertySrceMapper<TSrce>.SetSrceValue(TSrce entity, object value, OperationTypes operationType)
        {
            SetSrceValue(entity, (TSrceProperty)value, operationType);
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
                SrcePropertyInfo.SetValue(entity, value);
        }

        /// <summary>
        /// Maps the destination property to the source property.
        /// </summary>
        /// <param name="sourceEntity">The source entity.</param>
        /// <param name="destinationEntity">The destination entity.</param>
        /// <param name="operationType">The single <see cref="Mapper.OperationTypes"/> being performed to enable selection.</param>
        public void MapToSrce(TDest destinationEntity, TSrce sourceEntity, OperationTypes operationType)
        {
            if (MapToSrceOverride(destinationEntity, sourceEntity, operationType))
                return;

            if (Mapper != null)
                SetSrceValue(sourceEntity, Mapper.MapToSrce(destinationEntity, operationType), operationType);
        }

        /// <summary>
        /// Overrides the destination to source mapping for this <see cref="PropertyMapper{TSrce, TSrceProperty, TDest, TDestProperty}"/>.
        /// </summary>
        /// <param name="func">A function to override the mapping.</param>
        /// <returns>The <see cref="PropertyMapper{TEntity, TProperty, TDest, TDestProperty}"/>.</returns>
        public PropertySrceMapper<TSrce, TSrceProperty, TDest> MapToSrce(Func<TDest, TSrce, OperationTypes, TSrceProperty> func)
        {
            _mapDestToSrceOverride = func;
            return this;
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

        /// <summary>
        /// Converts a destination property value into the source value.
        /// </summary>
        /// <param name="destinationPropertyValue">The destination property value.</param>
        /// <param name="operationType">The single <see cref="Mapper.OperationTypes"/> being performed to enable selection.</param>
        /// <returns>The source property value.</returns>
        object IPropertyMapper<TSrce, TDest>.ConvertToSrceValue(object destinationPropertyValue, OperationTypes operationType) => throw new NotImplementedException();

        /// <summary>
        /// Defines a condition clause which must be <c>true</c> when mapping from the destination to the source.
        /// </summary>
        /// <param name="predicate">A function to determine whether the property is to be mapped.</param>
        /// <returns>The <see cref="PropertySrceMapper{TEntity, TProperty, TDest}"/>.</returns>
        public PropertySrceMapper<TSrce, TSrceProperty, TDest> MapDestToSrceWhen(Func<TDest, bool> predicate)
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
            return (_mapDestToSrceWhen == null) ? true : _mapDestToSrceWhen.Invoke(entity);
        }

        #endregion

        #region Dest

        /// <summary>
        /// Gets the destination property value.
        /// </summary>
        /// <param name="entity">The entity value.</param>
        /// <param name="operationType">The single <see cref="Mapper.OperationTypes"/> being performed to enable selection.</param>
        /// <returns>The property value.</returns>
        object IPropertyMapper<TSrce, TDest>.GetDestValue(TDest entity, OperationTypes operationType)
        {
            return GetDestValue(entity, operationType);
        }

        /// <summary>
        /// Gets the destination property value.
        /// </summary>
        /// <param name="entity">The entity value.</param>
        /// <param name="operationType">The single <see cref="Mapper.OperationTypes"/> being performed to enable selection.</param>
        /// <returns>The property value.</returns>
        protected TDest GetDestValue(TDest entity, OperationTypes operationType)
        {
            if (OperationTypes.HasFlag(operationType))
                return entity;
            else
                return default(TDest);
        }

        /// <summary>
        /// Sets the destination property value.
        /// </summary>
        /// <param name="entity">The entity value.</param>
        /// <param name="value">The property value.</param>
        /// <param name="operationType">The single <see cref="Mapper.OperationTypes"/> being performed to enable selection.</param>
        void IPropertyMapper<TSrce, TDest>.SetDestValue(TDest entity, object value, OperationTypes operationType)
        {
            SetDestValue(entity, (TDest)value, operationType);
        }

        /// <summary>
        /// Sets the destination property value.
        /// </summary>
        /// <param name="entity">The entity value.</param>
        /// <param name="value">The property value.</param>
        /// <param name="operationType">The single <see cref="Mapper.OperationTypes"/> being performed to enable selection.</param>
        public void SetDestValue(TDest entity, TDest value, OperationTypes operationType) => throw new NotSupportedException();

        /// <summary>
        /// Maps the source property to the destination property.
        /// </summary>
        /// <param name="sourceEntity">The source entity.</param>
        /// <param name="destinationEntity">The destination entity.</param>
        /// <param name="operationType">The single <see cref="Mapper.OperationTypes"/> being performed to enable selection.</param>
        public void MapToDest(TSrce sourceEntity, TDest destinationEntity, OperationTypes operationType)
        {
            if (sourceEntity == null || destinationEntity == null)
                return;

            if (!OperationTypes.HasFlag(operationType))
                return;

            if (MapToDestOverride(sourceEntity, destinationEntity, operationType))
                return;

            if (Mapper == null)
                return;

            TSrceProperty val = GetSrceValue(sourceEntity, operationType);
            if (val != null)
            {
                foreach (var pm in Mapper.Mappings)
                {
                    if (pm.OperationTypes.HasFlag(operationType) && pm.MapSrceToDestWhen(val))
                        pm.MapToDest(val, destinationEntity, operationType);
                }
            }
        }

        /// <summary>
        /// Overrides the source to destination mapping for this <see cref="PropertyMapper{TSrce, TSrceProperty, TDest, TDestProperty}"/>.
        /// </summary>
        /// <param name="action">An action to override the mapping.</param>
        /// <returns>The <see cref="PropertyMapper{TEntity, TProperty, TDest, TDestProperty}"/>.</returns>
        public PropertySrceMapper<TSrce, TSrceProperty, TDest> MapToDest(Action<TSrce, TDest, OperationTypes> action)
        {
            _mapSrceToDestOverride = action;
            return this;
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

            _mapSrceToDestOverride(sourceEntity, destinationEntity, operationType);
            return true;
        }

        /// <summary>
        /// Converts a source property value into the destination value.
        /// </summary>
        /// <param name="sourcePropertyValue">The source property value.</param>
        /// <param name="operationType">The single <see cref="Mapper.OperationTypes"/> being performed to enable selection.</param>
        /// <returns>The destination property value.</returns>
        object IPropertyMapper<TSrce, TDest>.ConvertToDestValue(object sourcePropertyValue, OperationTypes operationType) => throw new NotImplementedException();

        /// <summary>
        /// Invokes the <see cref="MapSrceToDestWhen(Func{TSrce, bool})"/> clause to determine whether mapping from the source to the destination should occur.
        /// </summary>
        /// <param name="entity">The entity value.</param>
        /// <returns><c>true</c> indicates that the mapping should occur; otherwise, <c>false</c>.</returns>
        bool IPropertySrceMapper<TSrce>.MapSrceToDestWhen(TSrce entity)
        {
            return (_mapSrceToDestWhen == null) ? true : _mapSrceToDestWhen.Invoke(entity);
        }

        /// <summary>
        /// Defines a conditional clause which must be <c>true</c> when mapping from the source to the destination.
        /// </summary>
        /// <param name="predicate">A function to determine whether the property is to be mapped.</param>
        /// <returns>The <see cref="PropertySrceMapper{TEntity, TProperty, TDest}"/>.</returns>
        public PropertySrceMapper<TSrce, TSrceProperty, TDest> MapSrceToDestWhen(Func<TSrce, bool> predicate)
        {
            _mapSrceToDestWhen = predicate;
            return this;
        }

        #endregion
    }
}