// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;

namespace Beef
{
    /// <summary>
    /// Provides the concrete implementation (<see cref="Create(object[])"/>) for a given interface. 
    /// </summary>
    /// <remarks>The process and order in which the <see cref="Factory"/> determines the concrete version of an given <b>interface</b> is as follows
    /// (where not found an exception will be thrown):
    /// <list type="number">
    /// <item><description>The concrete instance as set by <see cref="Set{Type}"/>.</description></item>
    /// <item><description>The specified <see cref="Type"/> as set by <see cref="Set(Type, Type)"/>. The instance is created at runtime.</description></item>
    /// <item><description>Find concrete <see cref="Type"/> in same <see cref="Assembly"/> as <b>interface</b> by assuming the concrete name has the same name
    /// with the leading "I" removed (e.g. IFoo is Foo) as specified by the <see cref="Regex"/> pattern <see cref="DefaultNameSubstitutionPattern"/>.
    /// The instance is created at runtime.</description></item>
    /// <item><description>Find concrete <see cref="Type"/> in substituted <see cref="Assembly"/> as set by <see cref="SetSubstitution"/> (uses
    /// <see cref="DefaultNameSubstitutionPattern"/> where not specified). The instance is created at runtime.</description></item>
    /// </list>
    /// </remarks>
    public static class Factory
    {
        #region Substitution

        /// <summary>
        /// Represents a <see cref="Type"/> Substitution configuration.
        /// </summary>
        private class Substitution
        {
            /// <summary>
            /// Create key from an existing <see cref="Type"/>.
            /// </summary>
            /// <param name="type">The <see cref="Type"/>.</param>
            /// <returns>The corresponding key.</returns>
            public static string CreateKey(Type type)
            {
                return type.Namespace + ", " + type.GetTypeInfo().Assembly.GetName().Name;
            }

            /// <summary>
            /// Initializes an instance of the <see cref="Substitution"/> class from a <paramref name="concreteExample"/>.
            /// </summary>
            /// <param name="interfaceExample">An interface example <see cref="Type"/>.</param>
            /// <param name="concreteExample">A concrete example <see cref="Type"/>.</param>
            /// <param name="regex">The name substitution <see cref="Regex"/>.</param>
            public Substitution(Type interfaceExample, Type concreteExample, Regex regex)
            {
                Key = CreateKey(interfaceExample);
                Interface = interfaceExample;
                Concrete = concreteExample;
                Regex = regex;
            }

            /// <summary>
            /// Gets or sets the key.
            /// </summary>
            public string Key { get; set; }

            /// <summary>
            /// Gets or sets the interface type.
            /// </summary>
            public Type Interface { get; set; }

            /// <summary>
            /// Gets or sets the concrete type.
            /// </summary>
            public Type Concrete { get; set; }

            /// <summary>
            /// Gets or sets the name substitution <see cref="Regex"/>.
            /// </summary>
            public Regex Regex { get; set; }
        }

        #endregion

        /// <summary>
        /// Default name substitution <see cref="Regex"/> patten.
        /// </summary>
        /// <remarks>Assumes that the concrete name has the same name with the leading "I" removed (e.g. IFoo is Foo) from the interface name;
        /// where there is no leading "I" the interface name will be used as-is.</remarks>
        public const string DefaultNameSubstitutionPattern = "(?(^[I][A-Z])^[I])";

        private static readonly Regex SubstitutionRegex = new Regex(DefaultNameSubstitutionPattern);
        private static readonly object _lock = new object();
        private static readonly Dictionary<string, string> _typeProviders = new Dictionary<string, string>();
        private static readonly List<Substitution> _substitutions = new List<Substitution>();
        private static readonly Dictionary<string, object> _providerValues = new Dictionary<string, object>();
        private static readonly Dictionary<Type, Type> _typeCache = new Dictionary<Type, Type>();
        private static AsyncLocal<Dictionary<string, object>> _localProviderValues = new AsyncLocal<Dictionary<string, object>>();

        /// <summary>
        /// Sets the interface and type relationship.
        /// </summary>
        /// <param name="interfaceType">The interface <see cref="Type"/>.</param>
        /// <param name="concreteType">The concrete <see cref="Type"/>.</param>
        public static void Set(Type interfaceType, Type concreteType)
        {
            if (interfaceType == null)
                throw new ArgumentNullException("interfaceType");

            if (!interfaceType.GetTypeInfo().IsInterface)
                throw new ArgumentException(string.Format("Type '{0}' is not an interface.", interfaceType.AssemblyQualifiedName), "interfaceType");

            if (concreteType == null)
                throw new ArgumentNullException("concreteType");

            TypeInfo ti = concreteType.GetTypeInfo();
            if (!ti.IsClass && ti.IsAssignableFrom(interfaceType.GetTypeInfo()))
                throw new ArgumentException(string.Format("Type '{0}' must implement the interface.", concreteType.AssemblyQualifiedName), "concreteType");

            lock (_lock)
            {
                if (_typeProviders.ContainsKey(interfaceType.AssemblyQualifiedName))
                    _typeProviders[interfaceType.AssemblyQualifiedName] = concreteType.AssemblyQualifiedName;
                else
                    _typeProviders.Add(interfaceType.AssemblyQualifiedName, concreteType.AssemblyQualifiedName);
            }
        }

        /// <summary>
        /// Sets a provider interface and concrete type substitution relationship based on an example interface and concrete type
        /// that is leveraged to provide a pattern for where to look for any classes within the same assembly and namespace.
        /// </summary>
        /// <param name="interfaceTypeExample">An interface <see cref="Type"/> example.</param>
        /// <param name="concreteTypeExample">A concrete <see cref="Type"/> example.</param>
        /// <param name="nameSubstitutionRegex">Optional substitution <see cref="Regex"/>. Where <c>null</c> a default <see cref="Regex"/> will be used with the
        /// pattern set to <see cref="DefaultNameSubstitutionPattern"/>.</param>
        /// <remarks>This can be called multiple times to define all required substitution relationships.</remarks>
        public static void SetSubstitution(Type interfaceTypeExample, Type concreteTypeExample, Regex nameSubstitutionRegex = null)
        {
            if (interfaceTypeExample == null)
                throw new ArgumentNullException("interfaceTypeExample");

            if (!interfaceTypeExample.GetTypeInfo().IsInterface)
                throw new ArgumentException(string.Format("Type '{0}' is not an interface.", interfaceTypeExample.AssemblyQualifiedName), "interfaceTypeExample");

            if (concreteTypeExample == null)
                throw new ArgumentNullException("concreteTypeExample");

            TypeInfo ti = concreteTypeExample.GetTypeInfo();
            if (!ti.IsClass && ti.IsAssignableFrom(interfaceTypeExample.GetTypeInfo()))
                throw new ArgumentException(string.Format("Type '{0}' must implement the interface.", concreteTypeExample.AssemblyQualifiedName), "concreteTypeExample");

            _substitutions.Add(new Substitution(interfaceTypeExample, concreteTypeExample, nameSubstitutionRegex ?? SubstitutionRegex));
        }

        /// <summary>
        /// Sets the provider instance value.
        /// </summary>
        /// <typeparam name="T">The interface <see cref="Type"/> of the value.</typeparam>
        /// <param name="value">The value.</param>
        public static void Set<T>(T value) where T : class
        {
            Type typeKey = typeof(T);
            if (!typeKey.GetTypeInfo().IsInterface)
                throw new InvalidOperationException(string.Format("Factory does not support Type '{0}' as this is not an Interface.", typeKey.AssemblyQualifiedName));

            if (value == null)
                throw new ArgumentNullException("value");

            lock (_lock)
            {
                if (_providerValues.ContainsKey(typeKey.AssemblyQualifiedName))
                    _providerValues[typeKey.AssemblyQualifiedName] = value;
                else
                    _providerValues.Add(typeKey.AssemblyQualifiedName, value);
            }
        }

        /// <summary>
        /// Sets the provider instance value for the current thread (see <see cref="ThreadLocal{T}"/>; primary usage is to support unit testing.
        /// </summary>
        /// <typeparam name="T">The interface <see cref="Type"/> of the value.</typeparam>
        /// <param name="value">The value.</param>
        public static void SetLocal<T>(T value) where T : class
        {
            Type typeKey = typeof(T);
            if (!typeKey.GetTypeInfo().IsInterface)
                throw new InvalidOperationException(string.Format("Factory does not support Type '{0}' as this is not an Interface.", typeKey.AssemblyQualifiedName));

            if (value == null)
                throw new ArgumentNullException("value");

            lock (_lock)
            {
                if (_localProviderValues.Value == null)
                    _localProviderValues.Value = new Dictionary<string, object>();

                if (_localProviderValues.Value.ContainsKey(typeKey.AssemblyQualifiedName))
                    _localProviderValues.Value[typeKey.AssemblyQualifiedName] = value;
                else
                    _localProviderValues.Value.Add(typeKey.AssemblyQualifiedName, value);
            }
        }

        /// <summary>
        /// Resets (remove) any local provider instances. 
        /// </summary>
        public static void ResetLocal()
        {
            _localProviderValues = new AsyncLocal<Dictionary<string, object>>();
        }

        /// <summary>
        /// Create an instance of the requested <see cref="Type"/>.
        /// </summary>
        /// <typeparam name="T">The interface <see cref="Type"/> of the instance to create.</typeparam>
        /// <returns>An instance of the requested <see cref="Type"/>.</returns>
        public static T Create<T>()
        {
            return Create<T>(null);
        }

        /// <summary>
        /// Create an instance of the requested <see cref="Type"/> using the constructor that best matches the specified parameters.
        /// </summary>
        /// <typeparam name="T">The interface <see cref="Type"/> of the instance to create.</typeparam>
        /// <param name="args">An array of arguments that match in number, order, and type the parameters of the constructor to invoke. If <paramref name="args"/> 
        /// is an empty array or <c>null</c>, the constructor that takes no parameters (the default constructor) is invoked.</param>
        /// <returns>An instance of the requested <see cref="Type"/>.</returns>
        public static T Create<T>(params object[] args)
        {
            Type typeKey = typeof(T);
            Type typeVal = null;

            if (_localProviderValues.Value != null && _localProviderValues.Value.ContainsKey(typeKey.AssemblyQualifiedName))
                return (T)_localProviderValues.Value[typeKey.AssemblyQualifiedName];

            if (_providerValues.ContainsKey(typeKey.AssemblyQualifiedName))
                return (T)_providerValues[typeKey.AssemblyQualifiedName];

            if (_typeCache.ContainsKey(typeKey))
                return (T)Activator.CreateInstance(_typeCache[typeKey], args);

            if (!typeKey.GetTypeInfo().IsInterface)
                throw new InvalidOperationException(string.Format("Factory can not create an instance of Type '{0}' as this is not an Interface.", typeKey.AssemblyQualifiedName));

            if (_typeProviders.ContainsKey(typeKey.AssemblyQualifiedName))
                typeVal = Type.GetType(_typeProviders[typeKey.AssemblyQualifiedName], false);
            else
            {
                // Assume concrete is the same name with the leading "I" removed or itself if not following that naming convention; e.g. IFoo is Foo.
                string concreteName = SubstitutionRegex.Replace(typeKey.Name, string.Empty);

                // Look in same Assembly as Interface for implementation (primary).
                if (concreteName != typeKey.Name)
                    typeVal = Type.GetType(MakeTypeName(typeKey, concreteName));

                // Look at namespace substitutions as it may reside in another assembly (secondary).
                if (typeVal == null)
                {
                    foreach (var substitution in _substitutions.Where(x => x.Key == Substitution.CreateKey(typeKey)))
                    {
                        typeVal = Type.GetType(MakeTypeName(substitution.Concrete, SubstitutionRegex.Replace(typeKey.Name, string.Empty)));
                        if (typeVal != null)
                            break;
                    }
                }
            }

            if (typeVal == null)
                throw new InvalidOperationException(string.Format("Factory can not create an instance of Type '{0}'; please check the naming convention and substitution rules.", typeKey.AssemblyQualifiedName));

            lock (_lock)
            {
                if (_typeCache.ContainsKey(typeKey))
                    return (T)Activator.CreateInstance(_typeCache[typeKey], args);

                _typeCache.Add(typeKey, typeVal);
                return (T)Activator.CreateInstance(typeVal, args);
            }
        }

        /// <summary>
        /// Makes a <see cref="Type"/> qualified name.
        /// </summary>
        /// <param name="type">The base <see cref="Type"/>.</param>
        /// <param name="className">The class name.</param>
        /// <returns>The <see cref="Type"/> qualified name.</returns>
        private static string MakeTypeName(Type type, string className)
        {
            return type.AssemblyQualifiedName.Replace(type.FullName, type.Namespace + "." + className);
        }
    }
}