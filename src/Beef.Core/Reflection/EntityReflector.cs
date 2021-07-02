// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.RefData;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Beef.Reflection
{
    /// <summary>
    /// Provides the arguments passed to and through an <see cref="IEntityReflector"/>.
    /// </summary>
    public class EntityReflectorArgs
    {
        /// <summary>
        /// Initializes an instance of the <see cref="EntityReflectorArgs"/> class with an optional <paramref name="cache"/>.
        /// </summary>
        /// <param name="cache">The <b>cache</b> <see cref="ConcurrentDictionary{Type, IEntityReflector}"/> to use versus instantiating each <see cref="IEntityReflector"/> per use.</param>
        public EntityReflectorArgs(ConcurrentDictionary<Type, IEntityReflector>? cache = null)
        {
            Cache = cache ?? new ConcurrentDictionary<Type, IEntityReflector>();
        }

        /// <summary>
        /// Gets the <b>cache</b> <see cref="ConcurrentDictionary{Type, IEntityReflector}"/> to use versus instantiating each <see cref="IEntityReflector"/> per use.
        /// </summary>
        public ConcurrentDictionary<Type, IEntityReflector> Cache { get; private set; }

        /// <summary>
        /// Gets or sets the action to invoke to perform additional logic when reflecting/building the <b>entity</b> <see cref="Type"/>.
        /// </summary>
        public Action<IEntityReflector>? EntityBuilder { get; set; } = null;

        /// <summary>
        /// Indicates whether to automatically populate the entity properties using the optional <see cref="PropertyBuilder"/> (defaults to <c>true</c>).
        /// </summary>
        public bool AutoPopulateProperties { get; set; } = true;

        /// <summary>
        /// Indicates whether duplicate properties are ignored.
        /// </summary>
        public bool IgnoreDuplicateProperties { get; set; } = true;

        /// <summary>
        /// Indicates whether to evaluate the reference data properties and set the <see cref="IPropertyReflector.ReferenceDataPropertyType"/> accordingly; otherwise, all properties will default to
        /// <see cref="ReferenceDataPropertyType.NotEvaluated"/>.
        /// </summary>
        public bool EvaluateReferenceDataProperties { get; set; } = false;

        /// <summary>
        /// Gets or sets the function to invoke to perform additional logic when reflecting/building the <b>property</b> <see cref="Type"/>; the result determines whether the
        /// property should be included (<c>true</c>) or not (<c>false</c>) within the underlying properties collection.
        /// </summary>
        public Func<IPropertyReflector, bool>? PropertyBuilder { get; set; } = null;

        /// <summary>
        /// Defines the <see cref="StringComparer"/> for finding the property/JSON names (defaults to <see cref="StringComparer.Ordinal"/>).
        /// </summary>
        public StringComparer NameComparer { get; set; } = StringComparer.Ordinal;

        /// <summary>
        /// Gets the <see cref="EntityReflector{TEntity}"/>.
        /// </summary>
        /// <typeparam name="TEntity">The entity <see cref="Type"/>.</typeparam>
        /// <returns>An <see cref="EntityReflector{TEntity}"/>.</returns>
        public EntityReflector<TEntity> GetReflector<TEntity>() where TEntity : class, new()
        {
            return (EntityReflector<TEntity>)Cache.GetOrAdd(typeof(TEntity), (type) =>
            {
                var er = new EntityReflector<TEntity>(this);
                EntityBuilder?.Invoke(er);
                return er;
            });
        }

        /// <summary>
        /// Gets the <see cref="IEntityReflector"/> for the specified <paramref name="type"/>.
        /// </summary>
        /// <param name="type">The entity <see cref="Type"/>.</param>
        /// <returns>An <see cref="IEntityReflector"/>.</returns>
        public IEntityReflector GetReflector(Type type)
        {
            Check.NotNull(type, nameof(type));

            return Cache.GetOrAdd(type, _ =>
            {
                if (!type.IsClass || type == typeof(string) || type.GetConstructor(Type.EmptyTypes) == null)
                    throw new ArgumentException($"Type '{type.Name}' must be a class and have a default (parameter-less) constructor.", nameof(type));

                var er = (IEntityReflector)Activator.CreateInstance(typeof(EntityReflector<>).MakeGenericType(type), this);
                EntityBuilder?.Invoke(er);
                return er;
            });
        }
    }

    /// <summary>
    /// Enables a reflector for a given entity (<see cref="Type"/>).
    /// </summary>
    public interface IEntityReflector
    {
        /// <summary>
        /// Gets the entity <see cref="Type"/>.
        /// </summary>
        Type Type { get; }

        /// <summary>
        /// Gets the <see cref="EntityReflectorArgs"/>.
        /// </summary>
        EntityReflectorArgs Args { get; }

        /// <summary>
        /// Gets or sets the optional tag for storing a single value.
        /// </summary>
        object? Tag { get; set; }

        /// <summary>
        /// Gets the <see cref="Dictionary{TKey, TValue}"/> for storing additional data.
        /// </summary>
        Dictionary<string, object> Data { get; }

        /// <summary>
        /// Gets the <see cref="IPropertyReflector"/> for the specified property name.
        /// </summary>
        /// <param name="name">The property name.</param>
        /// <returns>The <see cref="IPropertyReflector"/>.</returns>
        IPropertyReflector GetProperty(string name);

        /// <summary>
        /// Gets the <see cref="IPropertyReflector"/> for the specified JSON name.
        /// </summary>
        /// <param name="jsonName">The JSON name.</param>
        /// <returns>The <see cref="IPropertyReflector"/>.</returns>
        IPropertyReflector GetJsonProperty(string jsonName);
    }

    /// <summary>
    /// Provides a reflector for a given entity (<see cref="Type"/>).
    /// </summary>
    /// <typeparam name="TEntity">The entity <see cref="Type"/>.</typeparam>
    public class EntityReflector<TEntity> : IEntityReflector where TEntity : class, new()
    {
        private readonly Dictionary<string, IPropertyReflector<TEntity>> _properties;
        private readonly Dictionary<string, IPropertyReflector<TEntity>> _jsonProperties;
        private readonly Lazy<Dictionary<string, object>> _data = new(true);

        /// <summary>
        /// Initializes a new instance of the <see cref="EntityReflector{TEntity}"/> class.
        /// </summary>
        /// <param name="args">The <see cref="EntityReflectorArgs"/>.</param>
        public EntityReflector(EntityReflectorArgs? args = null)
        {
            Args = args ?? new EntityReflectorArgs();
            _properties = new Dictionary<string, IPropertyReflector<TEntity>>(Args.NameComparer ?? StringComparer.Ordinal);
            _jsonProperties = new Dictionary<string, IPropertyReflector<TEntity>>(Args.NameComparer ?? StringComparer.Ordinal);

            if (!Args.AutoPopulateProperties)
                return;

            var pe = Expression.Parameter(typeof(TEntity), "x");

            foreach (var p in TypeReflector.GetProperties(typeof(TEntity)))
            {
                var lex = Expression.Lambda(Expression.Property(pe, p.Name), pe);
                var pr = (IPropertyReflector<TEntity>)Activator.CreateInstance(typeof(PropertyReflector<,>).MakeGenericType(typeof(TEntity), p.PropertyType), Args, lex);

                if (Args.PropertyBuilder != null && !Args.PropertyBuilder(pr))
                    continue;

                AddProperty(pr);
            }
        }

        /// <summary>
        /// Gets the <see cref="EntityReflectorArgs"/>.
        /// </summary>
        public EntityReflectorArgs Args { get; private set; }

        /// <summary>
        /// Gets the entity <see cref="Type"/>.
        /// </summary>
        public Type Type => typeof(TEntity);

        /// <summary>
        /// Gets or sets the optional tag for storing a single value.
        /// </summary>
        public object? Tag { get; set; }

        /// <summary>
        /// Gets the <see cref="Dictionary{TKey, TValue}"/> for storing additional data.
        /// </summary>
        public Dictionary<string, object> Data { get => _data.Value; }

        /// <summary>
        /// Adds a <see cref="PropertyReflector{TEntity, TProperty}"/> to the mapper.
        /// </summary>
        /// <typeparam name="TProperty">The property <see cref="Type"/>.</typeparam>
        /// <param name="propertyExpression">The <see cref="Expression"/> to reference the entity property.</param>
        /// <returns>The <see cref="PropertyReflector{TEntity, TProperty}"/>.</returns>
        public PropertyReflector<TEntity, TProperty> Property<TProperty>(Expression<Func<TEntity, TProperty>> propertyExpression)
        {
            if (propertyExpression == null)
                throw new ArgumentNullException(nameof(propertyExpression));

            var pr = new PropertyReflector<TEntity, TProperty>(Args, propertyExpression);
            AddProperty(pr);
            return pr;
        }

        /// <summary>
        /// Adds the <see cref="IPropertyReflector{TEntity}"/> to the underlying property collections.
        /// </summary>
        private void AddProperty(IPropertyReflector<TEntity> propertyReflector)
        {
            if (propertyReflector == null)
                throw new ArgumentNullException(nameof(propertyReflector));

            if (_properties.ContainsKey(propertyReflector.PropertyName))
                throw new ArgumentException($"Property with name '{propertyReflector.PropertyName}' can not be specified more than once.", nameof(propertyReflector));

            if (propertyReflector.JsonName != null && _jsonProperties.ContainsKey(propertyReflector.JsonName))
                throw new ArgumentException($"Property with name '{propertyReflector.JsonName}' can not be specified more than once.", nameof(propertyReflector));

            _properties.Add(propertyReflector.PropertyName, propertyReflector);
            if (propertyReflector.JsonName != null)
                _jsonProperties.Add(propertyReflector.JsonName, propertyReflector);
        }

        /// <summary>
        /// Gets the <see cref="IPropertyReflector"/> for the specified name.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns>The <see cref="IPropertyReflector"/>.</returns>
        IPropertyReflector IEntityReflector.GetProperty(string name) => GetProperty(name);

        /// <summary>
        /// Gets the <see cref="IPropertyReflector"/> for the specified name.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns>The <see cref="IPropertyReflector"/>.</returns>
        public IPropertyReflector<TEntity> GetProperty(string name)
        {
            _properties.TryGetValue(name, out var value);
            return value;
        }

        /// <summary>
        /// Gets the <see cref="IPropertyReflector"/> for the specified JSON name.
        /// </summary>
        /// <param name="jsonName">The JSON name.</param>
        /// <returns>The <see cref="IPropertyReflector"/>.</returns>
        IPropertyReflector IEntityReflector.GetJsonProperty(string jsonName) => GetJsonProperty(jsonName);

        /// <summary>
        /// Gets the <see cref="IPropertyReflector"/> for the specified JSON name.
        /// </summary>
        /// <param name="jsonName">The JSON name.</param>
        /// <returns>The <see cref="IPropertyReflector"/>.</returns>
        public IPropertyReflector<TEntity> GetJsonProperty(string jsonName)
        {
            _jsonProperties.TryGetValue(jsonName, out var value);
            return value;
        }

        /// <summary>
        /// Gets all the properties.
        /// </summary>
        public IReadOnlyCollection<IPropertyReflector> GetProperties() => new ReadOnlyCollection<IPropertyReflector>(_properties.Values.OfType<IPropertyReflector>().ToList());
    }

    /// <summary>
    /// Enables a reflector for a given entity property.
    /// </summary>
    public interface IPropertyReflector
    {
        /// <summary>
        /// Gets the <see cref="EntityReflectorArgs"/>.
        /// </summary>
        EntityReflectorArgs Args { get; }

        /// <summary>
        /// Gets or sets the optional tag for storing a single value.
        /// </summary>
        object? Tag { get; set; }

        /// <summary>
        /// Gets the <see cref="Dictionary{TKey, TValue}"/> for storing additional data.
        /// </summary>
        Dictionary<string, object> Data { get; }

        /// <summary>
        /// Gets the compiled <see cref="IPropertyExpression"/>.
        /// </summary>
        IPropertyExpression PropertyExpression { get; }

        /// <summary>
        /// Gets the <see cref="ComplexTypeReflector"/> (only set where the property <see cref="IsComplexType"/>).
        /// </summary>
        ComplexTypeReflector? ComplexTypeReflector { get; }

        /// <summary>
        /// Indicates whether the property is a complex type or complex type collection (see <see cref="ComplexTypeReflector"/>).
        /// </summary>
        bool IsComplexType { get; }

        /// <summary>
        /// Gets the corresponding <see cref="PropertyInfo"/>;
        /// </summary>
        PropertyInfo PropertyInfo { get; }

        /// <summary>
        /// Gets the JSON property name.
        /// </summary>
        string JsonName { get; }

        /// <summary>
        /// Gets the property name.
        /// </summary>
        string PropertyName { get; }

        /// <summary>
        /// Gets the parent entity <see cref="Type"/>.
        /// </summary>
        Type EntityType { get; }

        /// <summary>
        /// Gets the property <see cref="Type"/>.
        /// </summary>
        Type PropertyType { get; }

        /// <summary>
        /// Gets the <see cref="Reflection.ReferenceDataPropertyType"/>.
        /// </summary>
        ReferenceDataPropertyType ReferenceDataPropertyType { get; }

        /// <summary>
        /// Gets the <see cref="IEntityReflector"/> for the property; will return <c>null</c> where <see cref="PropertyType"/> is not a class.
        /// </summary>
        /// <returns>An <see cref="IEntityReflector"/>.</returns>
        IEntityReflector? GetEntityReflector();

        /// <summary>
        /// Gets the <see cref="IEntityReflector"/> for the collection item property; will return <c>null</c> where <see cref="ComplexTypeReflector.ItemType"/> is not a class.
        /// </summary>
        /// <returns>An <see cref="IEntityReflector"/>.</returns>
        IEntityReflector? GetItemEntityReflector();

        /// <summary>
        /// Gets the specified <see cref="Reflection.ReferenceDataPropertyType"/> <see cref="PropertyInfo"/> for this <see cref="ReferenceDataPropertyType.ReferenceData"/> property.
        /// </summary>
        /// <returns>The corresponding <see cref="PropertyInfo"/>; otherwise, an <see cref="InvalidOperationException"/> will be thrown.</returns>
        PropertyInfo GetReferenceDataProperty(ReferenceDataPropertyType type);

        /// <summary>
        /// Gets the value from a <see cref="JToken"/>.
        /// </summary>
        /// <param name="jtoken">The <see cref="JToken"/>.</param>
        /// <returns>The value.</returns>
        object? GetJTokenValue(JToken jtoken);

        /// <summary>
        /// Sets the property value from a <see cref="JToken"/>.
        /// </summary>
        /// <param name="entity">The entity whose value is to be set.</param>
        /// <param name="jtoken">The <see cref="JToken"/>.</param>
        /// <returns><c>true</c> where the value was changed; otherwise, <c>false</c> (i.e. same value).</returns>
        bool SetValueFromJToken(object entity, JToken jtoken);

        /// <summary>
        /// Sets the property value.
        /// </summary>
        /// <param name="entity">The entity whose value is to be set.</param>
        /// <param name="value">The value.</param>
        /// <returns><c>true</c> where the value was changed; otherwise, <c>false</c> (i.e. same value).</returns>
        bool SetValue(object entity, object? value);

        /// <summary>
        /// Creates a new instance (value) and sets the property value.
        /// </summary>
        /// <param name="entity">The entity whose value is to be set.</param>
        /// <returns><c>true</c> where the value was changed; otherwise, <c>false</c> (i.e. same value).</returns>
        (bool changed, object? value) NewValue(object entity);

        /// <summary>
        /// Creates a new instance (value).
        /// </summary>
        /// <returns>The value.</returns>
        object? NewValue();
    }

    /// <summary>
    /// Enables a reflector for a given entity property.
    /// </summary>
    /// <typeparam name="TEntity">The entity <see cref="Type"/>.</typeparam>
    public interface IPropertyReflector<TEntity> : IPropertyReflector where TEntity : class, new()
    {
        /// <summary>
        /// Sets the property value from a <see cref="JToken"/>.
        /// </summary>
        /// <param name="entity">The entity whose value is to be set.</param>
        /// <param name="jtoken">The <see cref="JToken"/>.</param>
        /// <returns><c>true</c> where the value was changed; otherwise, <c>false</c> (i.e. same value).</returns>
        bool SetValueFromJToken(TEntity entity, JToken jtoken);

        /// <summary>
        /// Sets the property value.
        /// </summary>
        /// <param name="entity">The entity whose value is to be set.</param>
        /// <param name="value">The value.</param>
        /// <returns><c>true</c> where the value was changed; otherwise, <c>false</c> (i.e. same value).</returns>
        bool SetValue(TEntity entity, object value);

        /// <summary>
        /// Creates a new instance and sets the property value.
        /// </summary>
        /// <param name="entity">The entity whose value is to be set.</param>
        /// <returns><c>true</c> where the value was changed; otherwise, <c>false</c> (i.e. same value).</returns>
        (bool changed, object? value) NewValue(TEntity entity);
    }

    /// <summary>
    /// Provides a reflector for a given entity property.
    /// </summary>
    /// <typeparam name="TEntity">The entity <see cref="Type"/>.</typeparam>
    /// <typeparam name="TProperty">The property <see cref="Type"/>.</typeparam>
    public class PropertyReflector<TEntity, TProperty> : IPropertyReflector<TEntity> where TEntity : class, new()
    {
        private readonly Lazy<Dictionary<string, object>> _data = new(true);
        private string? _rootRefDataName;

        private const string _refDataSidSuffix = "Sid";
        private const string _refDataTextSuffix = "Text";

        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyReflector{TEntity, TProperty}"/> class.
        /// </summary>
        /// <param name="args">The <see cref="EntityReflectorArgs"/>.</param>
        /// <param name="propertyExpression">The <see cref="LambdaExpression"/> to reference the source entity property.</param>
        public PropertyReflector(EntityReflectorArgs args, Expression<Func<TEntity, TProperty>> propertyExpression)
        {
            Args = args ?? throw new ArgumentNullException(nameof(args));
            PropertyExpression = Reflection.PropertyExpression.Create(propertyExpression ?? throw new ArgumentNullException(nameof(propertyExpression)));
            PropertyInfo = TypeReflector.GetPropertyInfo(typeof(TEntity), PropertyName) ?? throw new ArgumentException($"Propery '{PropertyName}' does not exist for Type.", nameof(propertyExpression));

            if (PropertyInfo.PropertyType.IsClass && PropertyInfo.PropertyType != typeof(string))
                ComplexTypeReflector = ComplexTypeReflector.Create(PropertyInfo);

            if (args.EvaluateReferenceDataProperties)
            {
                if (PropertyInfo.PropertyType.IsClass && typeof(ReferenceDataBase).IsAssignableFrom(PropertyInfo.PropertyType))
                {
                    ReferenceDataPropertyType = ReferenceDataPropertyType.Value;
                    _rootRefDataName = PropertyName;
                }
                else if (IsReferenceDataProperty(_refDataSidSuffix))
                    ReferenceDataPropertyType = ReferenceDataPropertyType.Sid;
                else if (IsReferenceDataProperty(_refDataTextSuffix))
                    ReferenceDataPropertyType = ReferenceDataPropertyType.Text;
                else
                    ReferenceDataPropertyType = ReferenceDataPropertyType.None;
            }
        }

        /// <summary>
        /// Check whether property has a name that ends with the specified suffix and determine whether that property is reference data.
        /// </summary>
        private bool IsReferenceDataProperty(string suffix)
        {
            if (PropertyName.Length < 4 || !PropertyName.EndsWith(suffix, StringComparison.InvariantCulture))
                return false;

            var rdp = EntityType.GetProperty(PropertyName[0..^suffix.Length]);
            if (rdp == null || !typeof(ReferenceDataBase).IsAssignableFrom(rdp.PropertyType))
                return false;

            _rootRefDataName = rdp.Name;
            return true;
        }

        /// <summary>
        /// Gets the <see cref="EntityReflectorArgs"/>.
        /// </summary>
        public EntityReflectorArgs Args { get; private set; }

        /// <summary>
        /// Gets or sets the optional tag for storing a single value.
        /// </summary>
        public object? Tag { get; set; }

        /// <summary>
        /// Gets the <see cref="Dictionary{TKey, TValue}"/> for storing additional data.
        /// </summary>
        public Dictionary<string, object> Data { get => _data.Value; }

        /// <summary>
        /// Gets the compiled <see cref="IPropertyExpression"/>.
        /// </summary>
        IPropertyExpression IPropertyReflector.PropertyExpression => PropertyExpression;

        /// <summary>
        /// Gets the compiled <see cref="PropertyExpression{TEntity, TProperty}"/>.
        /// </summary>
        public PropertyExpression<TEntity, TProperty> PropertyExpression { get; private set; }

        /// <summary>
        /// Gets the <see cref="ComplexTypeReflector"/> (only set where the property <see cref="IsComplexType"/>).
        /// </summary>
        public ComplexTypeReflector? ComplexTypeReflector { get; private set; }

        /// <summary>
        /// Indicates whether the property is a complex type or complex type collection (see <see cref="ComplexTypeReflector"/>).
        /// </summary>
        public bool IsComplexType => ComplexTypeReflector != null;

        /// <summary>
        /// Gets the corresponding <see cref="PropertyInfo"/>;
        /// </summary>
        public PropertyInfo PropertyInfo { get; private set; }

        /// <summary>
        /// Gets the JSON property name.
        /// </summary>
        public string JsonName => PropertyExpression.JsonName;

        /// <summary>
        /// Gets the property name.
        /// </summary>
        public string PropertyName => PropertyExpression.Name;

        /// <summary>
        /// Gets the parent entity <see cref="Type"/>.
        /// </summary>
        public Type EntityType => typeof(TEntity);

        /// <summary>
        /// Gets the property <see cref="Type"/>.
        /// </summary>
        public Type PropertyType => typeof(TProperty);

        /// <summary>
        /// Gets the <see cref="Reflection.ReferenceDataPropertyType"/>.
        /// </summary>
        public ReferenceDataPropertyType ReferenceDataPropertyType { get; private set; }

        /// <summary>
        /// Gets the <see cref="IEntityReflector"/> for the property; will return <c>null</c> where <see cref="PropertyType"/> is not a class.
        /// </summary>
        /// <returns>An <see cref="IEntityReflector"/>.</returns>
        public IEntityReflector? GetEntityReflector()
        {
            if (!IsComplexType || ComplexTypeReflector!.ComplexTypeCode != ComplexTypeCode.Object)
                return null;

            return Args.GetReflector(PropertyType);
        }

        /// <summary>
        /// Gets the <see cref="IEntityReflector"/> for the collection item property; will return <c>null</c> where <see cref="ComplexTypeReflector.ItemType"/> is not a class.
        /// </summary>
        /// <returns>An <see cref="IEntityReflector"/>.</returns>
        public IEntityReflector? GetItemEntityReflector()
        {
            if (!IsComplexType || ComplexTypeReflector!.ComplexTypeCode == ComplexTypeCode.Object)
                return null;

            if (!ComplexTypeReflector.ItemType.IsClass && ComplexTypeReflector.ItemType == typeof(string))
                return null;

            return Args.GetReflector(ComplexTypeReflector.ItemType);
        }

        /// <summary>
        /// Gets the specified <see cref="Reflection.ReferenceDataPropertyType"/> <see cref="PropertyInfo"/> for this <see cref="ReferenceDataPropertyType.ReferenceData"/> property.
        /// </summary>
        /// <returns>The corresponding <see cref="PropertyInfo"/>; otherwise, an <see cref="InvalidOperationException"/> will be thrown.</returns>
        public PropertyInfo GetReferenceDataProperty(ReferenceDataPropertyType type) => type switch
        {
            ReferenceDataPropertyType.Value => ReferenceDataPropertyType == ReferenceDataPropertyType.Value ? PropertyInfo : EntityType.GetProperty($"{_rootRefDataName}")!,
            ReferenceDataPropertyType.Sid => ReferenceDataPropertyType == ReferenceDataPropertyType.Sid ? PropertyInfo : EntityType.GetProperty($"{_rootRefDataName}{_refDataSidSuffix}")!,
            ReferenceDataPropertyType.Text => ReferenceDataPropertyType == ReferenceDataPropertyType.Text ? PropertyInfo : EntityType.GetProperty($"{_rootRefDataName}{_refDataTextSuffix}")!,
            ReferenceDataPropertyType.None => throw new InvalidOperationException($"The {nameof(ReferenceDataPropertyType)} is '{nameof(ReferenceDataPropertyType.None)}'; as such no corresponding property exists."),
            _ => throw new InvalidOperationException($"The {nameof(ReferenceDataPropertyType)} is '{nameof(ReferenceDataPropertyType.NotEvaluated)}'; as such not determination can be made."),
        };

        /// <summary>
        /// Gets the value from a <see cref="JToken"/>.
        /// </summary>
        /// <param name="jtoken">The <see cref="JToken"/>.</param>
        /// <returns>The value.</returns>
        object? IPropertyReflector.GetJTokenValue(JToken jtoken) => GetJTokenValue(jtoken);

        /// <summary>
        /// Gets the value from a <see cref="JToken"/>.
        /// </summary>
        /// <param name="jtoken">The <see cref="JToken"/>.</param>
        /// <returns>The value.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "Needs to be a non-static as matches interface.")]
        TProperty GetJTokenValue(JToken jtoken) => jtoken.ToObject<TProperty>()!;

        /// <summary>
        /// Sets the property value from a <see cref="JToken"/>.
        /// </summary>
        /// <param name="entity">The entity whose value is to be set.</param>
        /// <param name="jtoken">The <see cref="JToken"/>.</param>
        /// <returns><c>true</c> where the value was changed; otherwise, <c>false</c> (i.e. same value).</returns>
        bool IPropertyReflector.SetValueFromJToken(object entity, JToken jtoken) => SetValueFromJToken((TEntity)entity, jtoken);

        /// <summary>
        /// Sets the property value from a <see cref="JToken"/>.
        /// </summary>
        /// <param name="entity">The entity whose value is to be set.</param>
        /// <param name="jtoken">The <see cref="JToken"/>.</param>
        /// <returns><c>true</c> where the value was changed; otherwise, <c>false</c> (i.e. same value).</returns>
        public bool SetValueFromJToken(TEntity entity, JToken jtoken)
        {
            Check.NotNull(entity, nameof(entity));
            Check.NotNull(jtoken, nameof(jtoken));
            return SetValue(entity, GetJTokenValue(jtoken));
        }

        /// <summary>
        /// Sets the property value.
        /// </summary>
        /// <param name="entity">The entity whose value is to be set.</param>
        /// <param name="value">The value.</param>
        /// <returns><c>true</c> where the value was changed; otherwise, <c>false</c> (i.e. same value).</returns>
        bool IPropertyReflector.SetValue(object entity, object? value) => SetValue((TEntity)entity, (TProperty)value!);

        /// <summary>
        /// Sets the property value.
        /// </summary>
        /// <param name="entity">The entity whose value is to be set.</param>
        /// <param name="value">The value.</param>
        /// <returns><c>true</c> where the value was changed; otherwise, <c>false</c> (i.e. same value).</returns>
        bool IPropertyReflector<TEntity>.SetValue(TEntity entity, object? value) => SetValue(entity, (TProperty)value!);

        /// <summary>
        /// Sets the property value.
        /// </summary>
        /// <param name="entity">The entity whose value is to be set.</param>
        /// <param name="value">The value.</param>
        /// <returns><c>true</c> where the value was changed; otherwise, <c>false</c> (i.e. same value).</returns>
        public bool SetValue(TEntity entity, TProperty value)
        {
            var existing = PropertyExpression.GetValue(entity);
            if (Comparer<TProperty>.Default.Compare(existing, value) == 0)
                return false;

            PropertyInfo.SetValue(entity, value);
            return true;
        }

        /// <summary>
        /// Creates a new instance and sets the property value.
        /// </summary>
        /// <param name="entity">The entity whose value is to be set.</param>
        /// <returns><c>true</c> where the value was changed; otherwise, <c>false</c> (i.e. same value).</returns>
        (bool changed, object? value) IPropertyReflector.NewValue(object entity) => NewValue((TEntity)entity);

        /// <summary>
        /// Creates a new instance and sets the property value.
        /// </summary>
        /// <param name="entity">The entity whose value is to be set.</param>
        /// <returns><c>true</c> where the value was changed; otherwise, <c>false</c> (i.e. same value).</returns>
        public (bool changed, object? value) NewValue(TEntity entity)
        {
            var val = NewValue();
            return (SetValue(entity, val), val);
        }

        /// <summary>
        /// Creates a new instance.
        /// </summary>
        /// <returns>The value.</returns>
        object? IPropertyReflector.NewValue() => NewValue();

        /// <summary>
        /// Creates a new instance.
        /// </summary>
        /// <returns>The value.</returns>
        public TProperty NewValue() => Activator.CreateInstance<TProperty>();
    }

    /// <summary>
    /// Indicates whether the <see cref="IPropertyReflector"/> is referencing a <see cref="ReferenceDataBase"/> and which of the possible code-generated property types it is based on naming convention.
    /// </summary>
    public enum ReferenceDataPropertyType
    {
        /// <summary>
        /// Indicates that the property has not been evaluated.
        /// </summary>
        NotEvaluated = 0,

        /// <summary>
        /// Indicates that the property is not considered related to reference data.
        /// </summary>
        None = 1,

        /// <summary>
        /// Indicates that the property is <i>the</i> <see cref="ReferenceDataBase"/> property value itsef.
        /// </summary>
        Value = 2,

        /// <summary>
        /// Inidicates that the property is the corresponding Serialization Identifier (SID) property (typically <see cref="ReferenceDataBase.Code"/>); this property is named with the <c>Sid</c> suffix.
        /// </summary>
        Sid = 4,

        /// <summary>
        /// Inidicates that the property is the corresponding <see cref="ReferenceDataBase.Text"/> property; this property is named with the <c>Text</c> suffix.
        /// </summary>
        Text = 8,

        /// <summary>
        /// Indicates that the property is reference data in orientation, in that it is either <see cref="Value"/>, <see cref="Sid"/> or <see cref="Text"/>.
        /// </summary>
        ReferenceData = Value | Sid | Text
    }
}