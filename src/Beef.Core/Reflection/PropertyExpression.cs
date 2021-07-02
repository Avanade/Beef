// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;
using System.Reflection;

namespace Beef.Reflection
{
    /// <summary>
    /// Enables the property <see cref="Expression"/> capabilities.
    /// </summary>
    public interface IPropertyExpression
    {
        /// <summary>
        /// Gets the property name.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets the JSON property name.
        /// </summary>
        string JsonName { get; }

        /// <summary>
        /// Gets the property text.
        /// </summary>
        LText Text { get; }

        /// <summary>
        /// Gets the <see cref="JsonPropertyAttribute"/> where declared (see <see cref="HasJsonPropertyAttribute"/>);
        /// </summary>
        JsonPropertyAttribute? JsonPropertyAttribute { get; }

        /// <summary>
        /// Indicates whether the property has the <see cref="JsonPropertyAttribute"/> declared.
        /// </summary>
        bool HasJsonPropertyAttribute { get; }

        /// <summary>
        /// Gets the property value for the given entity.
        /// </summary>
        /// <param name="entity">The entity value.</param>
        /// <returns>The corresponding property value.</returns>
        object? GetValue(object entity);
    }

    /// <summary>
    /// Provides access to the common property expression capabilities.
    /// </summary>
    public static class PropertyExpression
    {
        /// <summary>
        /// Gets the property name from the property expression.
        /// </summary>
        /// <param name="propertyExpression">The <see cref="Expression"/> to reference the entity property.</param>
        /// <returns>The property name.</returns>
        public static string GetPropertyName<TEntity, TProperty>(Expression<Func<TEntity, TProperty>> propertyExpression)
        {
            Check.NotNull(propertyExpression, nameof(propertyExpression));

            if (propertyExpression.Body.NodeType != ExpressionType.MemberAccess)
                throw new InvalidOperationException("Only Member access expressions are supported.");

            return ((MemberExpression)propertyExpression.Body).Member.Name;
        }

        /// <summary>
        /// Validates, creates and compiles the property expression; whilst also determinig the property friendly <see cref="PropertyExpression{TEntity, TProperty}.Text"/>.
        /// </summary>
        /// <param name="propertyExpression">The <see cref="Expression"/> to reference the entity property.</param>
        /// <param name="probeForJsonRefDataSidProperties">Indicates whether to probe for the <see cref="JsonPropertyAttribute"/> via alternate <c>Sid</c> or <c>Sids</c> properties as implemented for reference data.</param>
        /// <returns>A <see cref="PropertyExpression{TEntity, TProperty}"/> which contains (in order) the compiled <see cref="System.Func{TEntity, TProperty}"/>, member name and resulting property text.</returns>
        public static PropertyExpression<TEntity, TProperty> Create<TEntity, TProperty>(Expression<Func<TEntity, TProperty>> propertyExpression, bool probeForJsonRefDataSidProperties = false)
        {
            return PropertyExpression<TEntity, TProperty>.CreateInternal(Check.NotNull(propertyExpression, nameof(propertyExpression)), probeForJsonRefDataSidProperties);
        }
    }

    /// <summary>
    /// Provides property <see cref="Expression"/> capability.
    /// </summary>
    /// <typeparam name="TEntity">The entity <see cref="Type"/>.</typeparam>
    /// <typeparam name="TProperty">The property <see cref="Type"/>.</typeparam>
    /// <remarks>The compiled expression is cached so all subsequent requests for the same <typeparamref name="TEntity"/> and
    /// <typeparamref name="TProperty"/> is optimised for performance.</remarks>
    public class PropertyExpression<TEntity, TProperty> : IPropertyExpression
    {
        private struct ExpressionKey
        {
            public Type Type;
            public string Name;
            public bool ProbeForJsonRefDataSidProperties;
        }

        private static readonly ConcurrentDictionary<ExpressionKey, PropertyExpression<TEntity, TProperty>> _expressions = new();

        private readonly Func<TEntity, TProperty> _func;

        /// <summary>
        /// Validates, creates and compiles the property expression; whilst also determinig the property friendly <see cref="Text"/>.
        /// </summary>
        /// <param name="propertyExpression">The <see cref="Expression"/> to reference the entity property.</param>
        /// <param name="probeForJsonRefDataSidProperties">Indicates whether to probe for the <see cref="JsonPropertyAttribute"/> via alternate <c>Sid</c> or <c>Sids</c> properties as implemented for reference data.</param>
        /// <returns>A <see cref="PropertyExpression{TEntity, TProperty}"/> which contains (in order) the compiled <see cref="System.Func{TEntity, TProperty}"/>, member name and resulting property text.</returns>
        internal static PropertyExpression<TEntity, TProperty> CreateInternal(Expression<Func<TEntity, TProperty>> propertyExpression, bool probeForJsonRefDataSidProperties = false)
        {
            Check.NotNull(propertyExpression, nameof(propertyExpression));

            if (propertyExpression.Body.NodeType != ExpressionType.MemberAccess)
                throw new InvalidOperationException("Only Member access expressions are supported.");

            var me = (MemberExpression)propertyExpression.Body;

            // Check cache and reuse as this is a *really* expensive operation.
            var key = new ExpressionKey { Type = me.Member.DeclaringType, Name = me.Member.Name, ProbeForJsonRefDataSidProperties = probeForJsonRefDataSidProperties };
            return _expressions.GetOrAdd(key, _ =>
            {
                if (me.Member.MemberType != MemberTypes.Property)
                    throw new InvalidOperationException("Expression results in a Member that is not a Property.");

                if (!me.Member.DeclaringType.GetTypeInfo().IsAssignableFrom(typeof(TEntity).GetTypeInfo()))
                    throw new InvalidOperationException("Expression results in a Member for a different Entity class.");

                string name = me.Member.Name;

                // Either get the friendly text from a corresponding DisplayTextAttribute or split the PascalCase member name into friendlier sentence case text.
                DisplayAttribute ca = me.Member.GetCustomAttribute<DisplayAttribute>(true);

                // Get the JSON property name.
                JsonPropertyAttribute jpa = me.Member.GetCustomAttribute<JsonPropertyAttribute>(true);
                if (jpa == null && probeForJsonRefDataSidProperties)
                {
                    // Probe corresponding Sid or Sids properties for value (using the standardised naming convention).
                    var pi = me.Member.DeclaringType.GetProperty($"{name}Sid");
                    if (pi == null)
                        pi = me.Member.DeclaringType.GetProperty($"{name}Sids");

                    if (pi != null)
                        jpa = pi.GetCustomAttribute<JsonPropertyAttribute>(true);
                }

                // Create expression (with compilation also).
                var pe = new PropertyExpression<TEntity, TProperty>(name, jpa == null ? name : jpa.PropertyName!, ca == null ? StringConversion.ToSentenceCase(me.Member.Name)! : ca.Name, propertyExpression.Compile())
                {
                    JsonPropertyAttribute = jpa
                };

                return pe;
            });
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyExpression"/> class.
        /// </summary>
        private PropertyExpression(string name, string jsonName, string text, Func<TEntity, TProperty> func)
        {
            Name = Check.NotEmpty(name, nameof(name));
            JsonName = Check.NotEmpty(jsonName, nameof(jsonName));
            Text = Check.NotEmpty(text, nameof(text));
            _func = Check.NotNull(func, nameof(func));
        }

        /// <summary>
        /// Gets the property name.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Gets the JSON property name.
        /// </summary>
        public string JsonName { get; private set; }

        /// <summary>
        /// Gets the property friendly text.
        /// </summary>
        public LText Text { get; private set; }

        /// <summary>
        /// Gets the <see cref="JsonPropertyAttribute"/> where declared (see <see cref="HasJsonPropertyAttribute"/>);
        /// </summary>
        public JsonPropertyAttribute? JsonPropertyAttribute { get; private set; }

        /// <summary>
        /// Indicates whether the property has the <see cref="JsonPropertyAttribute"/> declared.
        /// </summary>
        public bool HasJsonPropertyAttribute => JsonPropertyAttribute != null;

        /// <summary>
        /// Gets the property value for the given entity.
        /// </summary>
        /// <param name="entity">The entity value.</param>
        /// <returns>The corresponding property value.</returns>
        object? IPropertyExpression.GetValue(object entity)
        {
            return GetValue((TEntity)entity);
        }

        /// <summary>
        /// Gets the property value for the given entity.
        /// </summary>
        /// <param name="entity">The entity value.</param>
        /// <returns>The corresponding property value.</returns>
        public TProperty GetValue(TEntity entity)
        {
            if (entity == null)
                return default!;
            else
                return _func.Invoke(entity);
        }
    }
}