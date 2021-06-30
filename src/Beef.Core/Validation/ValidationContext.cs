// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.Entities;
using Beef.Reflection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Beef.Validation
{
    /// <summary>
    /// Provides the base validation context properties.
    /// </summary>
    public interface IValidationContextBase
    {
        /// <summary>
        /// Gets the entity value.
        /// </summary>
        object? Value { get; }

        /// <summary>
        /// Gets the <see cref="MessageItemCollection"/>.
        /// </summary>
        MessageItemCollection Messages { get; }

        /// <summary>
        /// Indicates whether there has been a validation error.
        /// </summary>
        bool HasErrors { get; }
    }

    /// <summary>
    /// Provides the validation context properties for an entity.
    /// </summary>
    public interface IValidationContext : IValidationContextBase
    {
        /// <summary>
        /// Gets the entity prefix used for fully qualified <i>entity.property</i> naming (<c>null</c> represents the root).
        /// </summary>
        string? FullyQualifiedEntityName { get; }

        /// <summary>
        /// Gets the entity prefix used for fully qualified JSON <i>entity.property</i> naming (<c>null</c> represents the root).
        /// </summary>
        string? FullyQualifiedJsonEntityName { get; }

        /// <summary>
        /// Indicates whether to use the JSON name for the <see cref="MessageItem"/> <see cref="MessageItem.Property"/>; by default (<c>false</c>) uses the .NET name.
        /// </summary>
        bool UseJsonNames { get; }

        /// <summary>
        /// Gets the configuration parameters.
        /// </summary>
        /// <remarks>Configuration parameters provide a means to pass values down through the validation stack.</remarks>
        Dictionary<string, object> Config { get; }

        /// <summary>
        /// Determines whether one of the specified fully qualified property name has an error.
        /// </summary>
        /// <param name="fullyQualifiedPropertyName">The fully qualified property name.</param>
        /// <returns><c>true</c> where an error exists for at least one of the specified properties; otherwise, <c>false</c>.</returns>
        bool HasError(string fullyQualifiedPropertyName);
    }

    /// <summary>
    /// Provides a validation context for an entity.
    /// </summary>
    /// <typeparam name="TEntity">The entity <see cref="Type"/>.</typeparam>
    public class ValidationContext<TEntity> : IValidationContext 
    {
        private readonly Dictionary<string, MessageItem> _propertyErrors = new();

        /// <summary>
        /// Initializes a new instance of the <see cref="ValidationContext{TEntity}"/> class.
        /// </summary>
        /// <param name="value">The entity value.</param>
        /// <param name="args">The <see cref="ValidationArgs"/>.</param>
        /// <param name="fullyQualifiedEntityNameOverride">Optional <see cref="ValidationArgs.FullyQualifiedEntityName"/> override.</param>
        /// <param name="fullyQualifiedJsonEntityNameOverride">Optional <see cref="ValidationArgs.FullyQualifiedJsonEntityName"/> override.</param>
        public ValidationContext(TEntity value, ValidationArgs args, string? fullyQualifiedEntityNameOverride = null, string? fullyQualifiedJsonEntityNameOverride = null)
        {
            Beef.Check.NotNull(args, nameof(args));

            Value = value;
            FullyQualifiedEntityName = fullyQualifiedEntityNameOverride ?? args.FullyQualifiedEntityName;
            FullyQualifiedJsonEntityName = fullyQualifiedJsonEntityNameOverride ?? args.FullyQualifiedJsonEntityName ?? FullyQualifiedEntityName;
            UseJsonNames = args.UseJsonNamesSelection;
            Config = args.Config;
            SelectedPropertyName = args.SelectedPropertyName;
            ShallowValidation = args.ShallowValidation;

            Messages.CollectionChanged += Messages_CollectionChanged;
        }

        /// <summary>
        /// Handle the add of a message.
        /// </summary>
        private void Messages_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case System.Collections.Specialized.NotifyCollectionChangedAction.Add:
                    foreach (var m in e.NewItems)
                    {
                        MessageItem mi = (MessageItem)m;
                        if (mi.Type == MessageType.Error)
                            HasErrors = true;
                    }

                    break;

                default:
                    throw new InvalidOperationException("Operation invalid for Messages; only add supported.");
            }
        }

        /// <summary>
        /// Gets the entity value.
        /// </summary>
        object? IValidationContextBase.Value => Value;

        /// <summary>
        /// Gets the entity value.
        /// </summary>
        public TEntity Value { get; }

        /// <summary>
        /// Gets the entity prefix used for fully qualified <i>entity.property</i> naming (<c>null</c> represents the root).
        /// </summary>
        public string? FullyQualifiedEntityName { get; }

        /// <summary>
        /// Gets the entity prefix used for fully qualified JSON <i>entity.property</i> naming (<c>null</c> represents the root).
        /// </summary>
        public string? FullyQualifiedJsonEntityName { get; }

        /// <summary>
        /// Gets the optional name of a selected (specific) property to validate for the entity (<c>null</c> indicates to validate all).
        /// </summary>
        public string? SelectedPropertyName { get; }

        /// <summary>
        /// Indicates whether to use the JSON name for the <see cref="MessageItem"/> <see cref="MessageItem.Property"/>; by default (<c>false</c>) uses the .NET name.
        /// </summary>
        public bool UseJsonNames { get; }

        /// <summary>
        /// Gets the <see cref="MessageItemCollection"/>.
        /// </summary>
        public MessageItemCollection Messages { get; } = new MessageItemCollection();

        /// <summary>
        /// Indicates whether there has been a validation error.
        /// </summary>
        public bool HasErrors { get; private set; }

        /// <summary>
        /// Indicates that a shallow validation is required; i.e. will only validate the top level properties.
        /// </summary>
        public bool ShallowValidation { get; }

        /// <summary>
        /// Gets the <see cref="IServiceProvider"/> from the <see cref="ExecutionContext.ServiceProvider"/>.
        /// </summary>
        public IServiceProvider ServiceProvider => (ExecutionContext.HasCurrent ? ExecutionContext.Current.ServiceProvider : null) ?? throw new InvalidOperationException("There is either no ExecutionContext.Current or the ExecutionContext.ServiceProvider has not been configured.");

        /// <summary>
        /// Gets service of type <typeparamref name="TService"/> from the <see cref="ServiceProvider"/>.
        /// </summary>
        /// <typeparam name="TService">The service <see cref="Type"/>.</typeparam>
        /// <param name="throwExceptionOnNull">Indicates whether to throw an <see cref="InvalidOperationException"/> where the underlying <see cref="IServiceProvider.GetService(Type)"/> returns <c>null</c>.</param>
        /// <returns>The specified service where found; </returns>
        public TService GetService<TService>(bool throwExceptionOnNull = true) where TService : class => ExecutionContext.GetService<TService>(throwExceptionOnNull)!;

        /// <summary>
        /// Throws a <see cref="ValidationException"/> where an error was found (and optionally if warnings).
        /// </summary>
        public void ThrowOnError(bool includeWarnings = false)
        {
            if (HasErrors)
                throw new ValidationException(Messages);

            if (includeWarnings && Messages.Any(x => x.Type == MessageType.Warning))
                throw new ValidationException(Messages);
        }

        /// <summary>
        /// Merges a validation result into this.
        /// </summary>
        /// <param name="context">The <see cref="IValidationContext"/> to merge.</param>
        public void MergeResult(IValidationContext context)
        {
            MergeResult(context?.Messages);
        }

        /// <summary>
        /// Merges a <see cref="MessageItemCollection"/> into this.
        /// </summary>
        /// <param name="messages">The <see cref="MessageItemCollection"/> to merge.</param>
        public void MergeResult(MessageItemCollection? messages)
        {
            if (messages == null || messages.Count == 0)
                return;

            foreach (var mi in messages)
            {
                if (!string.IsNullOrEmpty(mi.Property) && mi.Type == MessageType.Error)
                {
                    if (!HasError(mi.Property))
                        _propertyErrors.Add(mi.Property, mi);
                }

                Messages.Add(mi);
            }
        }

        #region HasError

        /// <summary>
        /// Determines whether the specified property has an error.
        /// </summary>
        /// <typeparam name="TProperty">The property <see cref="Type"/>.</typeparam>
        /// <param name="propertyExpression">The property expression.</param>
        /// <returns><c>true</c> where an error exists for the specified property; otherwise, <c>false</c>.</returns>
        public bool HasError<TProperty>(Expression<Func<TEntity, TProperty>> propertyExpression) => HasError(CreateFullyQualifiedName(propertyExpression));

        /// <summary>
        /// Determines whether the specified property has an error.
        /// </summary>
        /// <typeparam name="TProperty">The property <see cref="Type"/>.</typeparam>
        /// <param name="propertyExpression">The property expression.</param>
        /// <returns><c>true</c> where an error exists for the specified property; otherwise, <c>false</c>.</returns>
        internal bool HasError<TProperty>(PropertyExpression<TEntity, TProperty> propertyExpression) => HasError(CreateFullyQualifiedName(propertyExpression));

        /// <summary>
        /// Determines whether one of the specified fully qualified property name has an error.
        /// </summary>
        /// <param name="fullyQualifiedPropertyName">The fully qualified property name.</param>
        /// <returns><c>true</c> where an error exists for at least one of the specified properties; otherwise, <c>false</c>.</returns>
        public bool HasError(string fullyQualifiedPropertyName) => _propertyErrors.ContainsKey(fullyQualifiedPropertyName);

        /// <summary>
        /// Gets the error <see cref="MessageItem"/> for the specified property.
        /// </summary>
        /// <typeparam name="TProperty">The property <see cref="Type"/>.</typeparam>
        /// <param name="propertyExpression">The property expression.</param>
        /// <returns>The corresponding <see cref="MessageItem"/>; otherwise, <c>null</c>.</returns>
        public MessageItem? GetError<TProperty>(Expression<Func<TEntity, TProperty>> propertyExpression)
        {
            if (_propertyErrors.TryGetValue(CreateFullyQualifiedName(propertyExpression), out MessageItem mi))
                return null;
            else
                return mi;
        }

        #endregion

        #region Message

        /// <summary>
        /// Adds an <see cref="MessageType.Error"/> <see cref="MessageItem"/> to the <see cref="Messages"/> for the specified property and explicit text. 
        /// </summary>
        /// <typeparam name="TProperty">The property <see cref="Type"/>.</typeparam>
        /// <param name="propertyExpression">The property expression.</param>
        /// <param name="text">The text <see cref="LText"/>.</param>
        /// <returns>The <see cref="MessageItem"/>.</returns>
        public MessageItem AddError<TProperty>(Expression<Func<TEntity, TProperty>> propertyExpression, LText text)
        {
            var pe = CreatePropertyExpression(propertyExpression);
            return AddMessage(pe.Name, pe.JsonName, MessageType.Error, text, GetTextAndValue(pe));
        }

        /// <summary>
        /// Adds an <see cref="MessageType.Error"/> <see cref="MessageItem"/> to the <see cref="Messages"/> for the specified property, explicit text format and and additional values included in the text. The property
        /// friendly text and value are automatically passed as the first two arguments to the string formatter.
        /// </summary>
        /// <typeparam name="TProperty">The property <see cref="Type"/>.</typeparam>
        /// <param name="propertyExpression">The property expression.</param>
        /// <param name="format">The composite format string.</param>
        /// <param name="values">The values that form part of the message text.</param>
        /// <returns>The <see cref="MessageItem"/>.</returns>
        public MessageItem AddError<TProperty>(Expression<Func<TEntity, TProperty>> propertyExpression, LText format, params object[] values)
        {
            var pe = CreatePropertyExpression(propertyExpression);
            return AddMessage(pe.Name, pe.JsonName, MessageType.Error, format, GetTextAndValue(pe).Concat(values).ToArray());
        }

        /// <summary>
        /// Adds an <see cref="MessageType.Warning"/> <see cref="MessageItem"/> to the <see cref="Messages"/> for the specified property and explicit text. The property friendly text and value are automatically passed as the
        /// first two arguments to the string formatter.
        /// </summary>
        /// <typeparam name="TProperty">The property <see cref="Type"/>.</typeparam>
        /// <param name="propertyExpression">The property expression.</param>
        /// <param name="text">The text <see cref="LText"/>.</param>
        /// <returns>The <see cref="MessageItem"/>.</returns>
        public MessageItem AddWarning<TProperty>(Expression<Func<TEntity, TProperty>> propertyExpression, LText text)
        {
            var pe = CreatePropertyExpression(propertyExpression);
            return AddMessage(pe.Name, pe.JsonName, MessageType.Warning, text, GetTextAndValue(pe));
        }

        /// <summary>
        /// Adds an <see cref="MessageType.Warning"/> <see cref="MessageItem"/> to the <see cref="Messages"/> for the specified property, explicit text format and and additional values included in the text. The property
        /// friendly text and value are automatically passed as the first two arguments to the string formatter.
        /// </summary>
        /// <typeparam name="TProperty">The property <see cref="Type"/>.</typeparam>
        /// <param name="propertyExpression">The property expression.</param>
        /// <param name="format">The composite format string.</param>
        /// <param name="values">The values that form part of the message text.</param>
        /// <returns>The <see cref="MessageItem"/>.</returns>
        public MessageItem AddWarning<TProperty>(Expression<Func<TEntity, TProperty>> propertyExpression, LText format, params object[] values)
        {
            var pe = CreatePropertyExpression(propertyExpression);
            return AddMessage(pe.Name, pe.JsonName, MessageType.Warning, format, GetTextAndValue(pe).Concat(values).ToArray());
        }

        /// <summary>
        /// Adds an <see cref="MessageType.Info"/> <see cref="MessageItem"/> to the <see cref="Messages"/> for the specified property and explicit text. The property friendly text and value are automatically passed as the
        /// first two arguments to the string formatter.
        /// </summary>
        /// <typeparam name="TProperty">The property <see cref="Type"/>.</typeparam>
        /// <param name="propertyExpression">The property expression.</param>
        /// <param name="text">The text <see cref="LText"/>.</param>
        /// <returns>The <see cref="MessageItem"/>.</returns>
        public MessageItem AddInfo<TProperty>(Expression<Func<TEntity, TProperty>> propertyExpression, LText text)
        {
            var pe = CreatePropertyExpression(propertyExpression);
            return AddMessage(pe.Name, pe.JsonName, MessageType.Info, text, GetTextAndValue(pe));
        }

        /// <summary>
        /// Adds an <see cref="MessageType.Info"/> <see cref="MessageItem"/> to the <see cref="Messages"/> for the specified property, explicit text format and and additional values included in the text. The property
        /// friendly text and value are automatically passed as the first two arguments to the string formatter.
        /// </summary>
        /// <typeparam name="TProperty">The property <see cref="Type"/>.</typeparam>
        /// <param name="propertyExpression">The property expression.</param>
        /// <param name="format">The composite format string.</param>
        /// <param name="values">The values that form part of the message text.</param>
        /// <returns>The <see cref="MessageItem"/>.</returns>
        public MessageItem AddInfo<TProperty>(Expression<Func<TEntity, TProperty>> propertyExpression, LText format, params object[] values)
        {
            var pe = CreatePropertyExpression(propertyExpression);
            return AddMessage(pe.Name, pe.JsonName, MessageType.Info, format, GetTextAndValue(pe).Concat(values).ToArray());
        }

        /// <summary>
        /// Adds a <see cref="MessageItem"/> to the <see cref="Messages"/> for the specified property and explicit text.
        /// </summary>
        /// <param name="propertyName">The property name.</param>
        /// <param name="jsonPropertyName">The JSON property name.</param>
        /// <param name="type">The <see cref="MessageType"/>.</param>
        /// <param name="text">The text <see cref="LText"/>.</param>
        /// <returns>The <see cref="MessageItem"/>.</returns>
        internal MessageItem AddMessage(string propertyName, string jsonPropertyName, MessageType type, LText text)
        {
            var mi = Messages.Add(CreateFullyQualifiedName(propertyName, jsonPropertyName), type, text);
            if (type == MessageType.Error && !HasError(mi.Property!))
                _propertyErrors.Add(mi.Property!, mi);

            return mi;
        }

        /// <summary>
        /// Adds a <see cref="MessageItem"/> to the <see cref="Messages"/> for the specified property, explicit text format and additional values included in the text.
        /// </summary>
        /// <param name="propertyName">The property name.</param>
        /// <param name="jsonPropertyName">The JSON property name.</param>
        /// <param name="type">The <see cref="MessageType"/>.</param>
        /// <param name="format">The composite format string.</param>
        /// <param name="values">The values that form part of the message text.</param>
        /// <returns>The <see cref="MessageItem"/>.</returns>
        internal MessageItem AddMessage(string propertyName, string jsonPropertyName, MessageType type, LText format, params object?[] values)
        {
            Beef.Check.NotNull(propertyName, nameof(propertyName));
            var mi = Messages.Add(CreateFullyQualifiedName(propertyName, jsonPropertyName ?? propertyName), type, format, values);
            if (type == MessageType.Error && !HasError(mi.Property!))
                _propertyErrors.Add(mi.Property!, mi);

            return mi;
        }

        /// <summary>
        /// Adds a <see cref="MessageItem"/> to the <see cref="Messages"/> for the specified text.
        /// </summary>
        /// <param name="type">The <see cref="MessageType"/>.</param>
        /// <param name="text">The text <see cref="LText"/>.</param>
        /// <returns>The <see cref="MessageItem"/>.</returns>
        public MessageItem AddMessage(MessageType type, LText text)
        {
            var mi = Messages.Add(UseJsonNames ? FullyQualifiedJsonEntityName : FullyQualifiedEntityName, type, text);
            if (type == MessageType.Error && !HasError(mi.Property!))
                _propertyErrors.Add(mi.Property!, mi);

            return mi;
        }

        /// <summary>
        /// Adds a <see cref="MessageItem"/> to the <see cref="Messages"/> for the specified text format and additional values included in the text.
        /// </summary>
        /// <param name="type">The <see cref="MessageType"/>.</param>
        /// <param name="format">The composite format string.</param>
        /// <param name="values">The values that form part of the message text.</param>
        /// <returns>The <see cref="MessageItem"/>.</returns>
        public MessageItem AddMessage(MessageType type, LText format, params object?[] values)
        {
            var mi = Messages.Add(UseJsonNames ? FullyQualifiedJsonEntityName : FullyQualifiedEntityName, type, format, values);
            if (type == MessageType.Error && !HasError(mi.Property!))
                _propertyErrors.Add(mi.Property!, mi);

            return mi;
        }

        #endregion

        #region Check

        /// <summary>
        /// Checks whether a specified property has not had an error, then executes a predicate to determine whether an error has occurred (returns <c>true</c>) adding an error <see cref="MessageItem"/> for the
        /// specified property and text. The property friendly text and value are automatically passed as the first two arguments to the string formatter.
        /// </summary>
        /// <typeparam name="TProperty">The property <see cref="Type"/>.</typeparam>
        /// <param name="propertyExpression">The property expression.</param>
        /// <param name="predicate">The error checking predicate; a <c>true</c> result indicates an error.</param>
        /// <param name="text">The error text <see cref="LText"/>.</param>
        /// <returns><c>true</c> indicates that the specified property has had an error, or is now considered in error; otherwise, <c>false</c> for no error.</returns>
        public bool Check<TProperty>(Expression<Func<TEntity, TProperty>> propertyExpression, Func<TProperty, bool> predicate, LText text)
        {
            var pe = CreatePropertyExpression(propertyExpression);
            Beef.Check.NotNull(predicate, nameof(predicate));
            if (HasError(pe))
                return true;

            var tv = GetTextAndValue(pe);
            if (!predicate((TProperty)tv[1]))
                return false;

            AddMessage(pe.Name, pe.JsonName, MessageType.Error, text, tv);
            return true;
        }

        /// <summary>
        /// Checks whether a specified property has not had an error, then where <paramref name="when"/> is <c>true</c> adds an error <see cref="MessageItem"/> for the
        /// specified property and text. The property friendly text and value are automatically passed as the first two arguments to the string formatter.
        /// </summary>
        /// <typeparam name="TProperty">The property <see cref="Type"/>.</typeparam>
        /// <param name="propertyExpression">The property expression.</param>
        /// <param name="when"><c>true</c> indicates an error; otherwise, <c>false</c>.</param>
        /// <param name="text">The error text <see cref="LText"/>.</param>
        /// <returns><c>true</c> indicates that the specified property has had an error, or is now considered in error; otherwise, <c>false</c> for no error.</returns>
        public bool Check<TProperty>(Expression<Func<TEntity, TProperty>> propertyExpression, bool when, LText text)
        {
            var pe = CreatePropertyExpression(propertyExpression);
            if (HasError(pe))
                return true;

            var tv = GetTextAndValue(pe);
            if (!when)
                return false;

            AddMessage(pe.Name, pe.JsonName, MessageType.Error, text, tv);
            return true;
        }

        /// <summary>
        /// Checks whether a specified property has not had an error, then executes a predicate to determine whether an error has occurred (returns <c>true</c>) adding an error <see cref="MessageItem"/> for the
        /// specified property, text format and and additional values included in the text. The property friendly text and value are automatically passed as the first two arguments to the string formatter.
        /// </summary>
        /// <typeparam name="TProperty">The property <see cref="Type"/>.</typeparam>
        /// <param name="propertyExpression">The property expression.</param>
        /// <param name="predicate">The error checking predicate; a <c>false</c> result indicates an error.</param>
        /// <param name="format">The error composite format string.</param>
        /// <param name="values">The values that form part of the message text.</param>
        /// <returns><c>true</c> indicates that the specified property has had an error, or is now considered in error; otherwise, <c>false</c> for no error.</returns>
        public bool Check<TProperty>(Expression<Func<TEntity, TProperty>> propertyExpression, Func<TProperty, bool> predicate, LText format, params object[] values)
        {
            var pe = CreatePropertyExpression(propertyExpression);
            Beef.Check.NotNull(predicate, nameof(predicate));
            if (HasError(pe))
                return true;

            var tv = GetTextAndValue(pe);
            if (!predicate((TProperty)tv[1]))
                return false;

            AddMessage(pe.Name, pe.JsonName, MessageType.Error, format, tv.Concat(values).ToArray());
            return true;
        }

        /// <summary>
        /// Checks whether a specified property has not had an error, then where <paramref name="when"/> is <c>true</c> adds an error <see cref="MessageItem"/> for the specified property, 
        /// text format and and additional values included in the text. The property friendly text and value are automatically passed as the first two arguments to the string formatter.
        /// </summary>
        /// <typeparam name="TProperty">The property <see cref="Type"/>.</typeparam>
        /// <param name="propertyExpression">The property expression.</param>
        /// <param name="when"><c>true</c> indicates an error; otherwise, <c>false</c>.</param>
        /// <param name="format">The error composite format string.</param>
        /// <param name="values">The values that form part of the message text.</param>
        /// <returns><c>true</c> indicates that the specified property has had an error, or is now considered in error; otherwise, <c>false</c> for no error.</returns>
        public bool Check<TProperty>(Expression<Func<TEntity, TProperty>> propertyExpression, bool when, LText format, params object[] values)
        {
            var pe = CreatePropertyExpression(propertyExpression);
            if (HasError(pe))
                return true;

            var tv = GetTextAndValue(pe);
            if (!when)
                return false;

            AddMessage(pe.Name, pe.JsonName, MessageType.Error, format, tv.Concat(values).ToArray());
            return true;
        }

        #endregion

        #region Utility

        /// <summary>
        /// Gets the friendly text and value for a property expression.
        /// </summary>
        private object[] GetTextAndValue<TProperty>(PropertyExpression<TEntity, TProperty> propertyExpression)
        {
            return new object[] { propertyExpression.Text, propertyExpression.GetValue(Value)! };
        }

        /// <summary>
        /// Creates the property expression from the expression.
        /// </summary>
        private static PropertyExpression<TEntity, TProperty> CreatePropertyExpression<TProperty>(Expression<Func<TEntity, TProperty>> propertyExpression)
        {
            return PropertyExpression.Create(propertyExpression, true);
        }

        /// <summary>
        /// Creates the fully qualified name from an expression.
        /// </summary>
        private string CreateFullyQualifiedName<TProperty>(Expression<Func<TEntity, TProperty>> propertyExpression)
        {
            var pi = CreatePropertyExpression(propertyExpression);
            return CreateFullyQualifiedName(pi.Name, pi.JsonName);
        }

        /// <summary>
        /// Creates the fully qualified name from a property expression.
        /// </summary>
        private string CreateFullyQualifiedName<TProperty>(PropertyExpression<TEntity, TProperty> propertyExpression)
        {
            Beef.Check.NotNull(propertyExpression, nameof(propertyExpression));
            return CreateFullyQualifiedName(propertyExpression.Name, propertyExpression.JsonName);
        }

        /// <summary>
        /// Creates the fully qualified name using the property and json property names.
        /// </summary>
        private string CreateFullyQualifiedName(string propertyName, string jsonPropertyName)
            => UseJsonNames ? CreateFullyQualifiedJsonPropertyName(jsonPropertyName) : CreateFullyQualifiedPropertyName(propertyName);

        /// <summary>
        /// Creates a fully qualified property name for the specified name.
        /// </summary>
        /// <param name="name">The property name.</param>
        /// <returns>The fully qualified property name.</returns>
        internal string CreateFullyQualifiedPropertyName(string name)
        {
            if (FullyQualifiedEntityName == null)
                return name;
            else
                return name.StartsWith("[") ? FullyQualifiedEntityName + name : FullyQualifiedEntityName + "." + name;
        }

        /// <summary>
        /// Creates a fully qualified JSON property name for the specified name.
        /// </summary>
        /// <param name="name">The property name.</param>
        /// <returns>The fully qualified property name.</returns>
        internal string CreateFullyQualifiedJsonPropertyName(string name)
        {
            if (FullyQualifiedJsonEntityName == null)
                return name;
            else
                return name.StartsWith("[") ? FullyQualifiedJsonEntityName + name : FullyQualifiedJsonEntityName + "." + name;
        }

        #endregion

        #region Config

        /// <summary>
        /// Gets the configuration parameters.
        /// </summary>
        /// <remarks>Configuration parameters provide a means to pass values down through the validation stack.</remarks>
        public Dictionary<string, object> Config { get; internal set; }

        /// <summary>
        /// Sets a configuration parameter value.
        /// </summary>
        /// <param name="name">The configuration parameter name.</param>
        /// <param name="value">The configuration parameter value.</param>
        /// <remarks>Configuration parameters provide a means to pass values down through the validation stack.</remarks>
        public void ConfigSet(string name, object value)
        {
            if (Config.ContainsKey(name))
                Config[name] = value;
            else
                Config.Add(name, value);
        }

        /// <summary>
        /// Gets a typed configuration parameter value.
        /// </summary>
        /// <typeparam name="T">The configuration parameter value <see cref="Type"/>.</typeparam>
        /// <param name="name">The configuration parameter name.</param>
        /// <returns>The configuration parameter value where found; otherwise, the default value for the type.</returns>
        public T ConfigGet<T>(string name)
        {
            if (Config.ContainsKey(name))
                return (T)Config[name];

            return default!;
        }

        /// <summary>
        /// Gets a configuration parameter value.
        /// </summary>
        /// <param name="name">The configuration parameter name.</param>
        /// <returns>The configuration parameter value where found; otherwise, <c>null</c>.</returns>
        public object ConfigGet(string name)
        {
            return ConfigGet<object>(name);
        }

        #endregion
    }
}