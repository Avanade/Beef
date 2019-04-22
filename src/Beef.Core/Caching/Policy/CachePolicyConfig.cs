// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using System;

namespace Beef.Caching.Policy
{
    /// <summary>
    /// Provides JSON-based cache policy configuration.
    /// </summary>
    public class CachePolicyConfig
    {
        #region Static

        /// <summary>
        /// Sets (configures) the <see cref="CachePolicyManager"/> using the <paramref name="config"/>.
        /// </summary>
        /// <param name="config">The <see cref="CachePolicyConfig"/>.</param>
        internal static void SetCachePolicyManager(CachePolicyConfig config)
        {
            if (config == null)
                return;

            // Load the policies.
            var isDefaultSet = false;
            foreach (var pol in config.Policies)
            {
                if (string.IsNullOrEmpty(pol.Name))
                    throw new CachePolicyConfigException("A Policy has been defined with no Name.");

                if (string.IsNullOrEmpty(pol.Policy))
                    throw new CachePolicyConfigException($"Policy '{pol.Name}' has been defined with no Type.");

                try
                {
                    var type = Type.GetType(pol.Policy) ?? throw new CachePolicyConfigException($"Policy '{pol.Name}' Type '{pol.Policy}' could not be loaded/instantiated.");
                    var policy = (ICachePolicy)Activator.CreateInstance(type);
                    LoadPolicyProperties(pol, type, policy);
                    LoadCaches(pol, policy);

                    if (pol.IsDefault)
                    {
                        if (isDefaultSet)
                            throw new CachePolicyConfigException($"Policy '{pol.Name}' can not set DefaultPolicy where already set.");

                        isDefaultSet = true;
                        CachePolicyManager.DefaultPolicy = policy;
                    }
                }
                catch (CachePolicyConfigException) { throw; }
                catch (Exception ex) { throw new CachePolicyConfigException($"Policy '{pol.Name}' Type '{pol.Policy}' could not be loaded/instantiated: {ex.Message}", ex); }
            }
        }

        /// <summary>
        /// Loads the properties for the policy.
        /// </summary>
        private static void LoadPolicyProperties(CachePolicyConfigPolicy config, Type type, ICachePolicy policy)
        {
            if (config.Properties == null)
                return;

            foreach (var prop in config.Properties)
            {
                if (string.IsNullOrEmpty(prop.Name))
                    throw new CachePolicyConfigException($"Policy '{config.Name}' has a Property with no Name.");

                if (prop == null)
                    continue;

                try
                {
                    var pi = type.GetProperty(prop.Name) ?? throw new CachePolicyConfigException($"Policy '{config.Name}' Property '{prop.Name}' does not exist.");
                    if (prop.Value is string)
                    {
                        if (pi.PropertyType == typeof(string))
                            pi.SetValue(policy, prop.Value);
                        else
                        {
                            var mi = pi.PropertyType.GetMethod("TryParse", new Type[] { typeof(string), pi.PropertyType.MakeByRefType() }) ?? throw new CachePolicyConfigException($"Policy '{config.Name}' Property '{prop.Name}' type must support TryParse.");
                            var args = new object[] { prop.Value, null };
                            if (!(bool)mi.Invoke(null, args))
                                throw new CachePolicyConfigException($"Policy '{config.Name}' Property '{prop.Name}' value is not valid.");

                            pi.SetValue(policy, args[1]);
                        }
                    }
                    else
                        pi.SetValue(policy, prop.Value);
                }
                catch (CachePolicyConfigException) { throw; }
                catch (Exception ex) { throw new CachePolicyConfigException($"Policy '{config.Name}' Property '{prop.Name}' is unable to be set: {ex.Message}", ex); }
            }
        }

        /// <summary>
        /// Loads the cache type and policy configurations.
        /// </summary>
        private static void LoadCaches(CachePolicyConfigPolicy config, ICachePolicy policy)
        {
            if (config.Caches == null)
                return;

            foreach (var cache in config.Caches)
            {
                if (!string.IsNullOrEmpty(cache))
                    CachePolicyManager.Set(cache, policy);
            }
        }

        #endregion

        /// <summary>
        /// Gets or sets the <see cref="CachePolicyConfigPolicy"/> array.
        /// </summary>
        public CachePolicyConfigPolicy[] Policies { get; set; }

        /// <summary>
        /// Represents the <see cref="CachePolicyConfig"/> policy definition.
        /// </summary>
        public class CachePolicyConfigPolicy
        {
            /// <summary>
            /// Gets or sets the policy name.
            /// </summary>
            public string Name { get; set; }

            /// <summary>
            /// Indicates whether the policy is the <see cref="CachePolicyManager.DefaultPolicy"/>.
            /// </summary>
            public bool IsDefault { get; set; }

            /// <summary>
            /// Gets or sets the policy <see cref="Type"/> name.
            /// </summary>
            public string Policy { get; set; }

            /// <summary>
            /// Gets or sets the <see cref="CachePolicyConfigPolicyProperty"/> array.
            /// </summary>
            public CachePolicyConfigPolicyProperty[] Properties { get; set; }

            /// <summary>
            /// Gets or sets the related cache <see cref="Type"/> names.
            /// </summary>
            public string[] Caches { get; set; }
        }

        /// <summary>
        /// Represents the <see cref="CachePolicyConfigPolicy"/> property.
        /// </summary>
        public class CachePolicyConfigPolicyProperty
        {
            /// <summary>
            /// Gets or sets the property name.
            /// </summary>
            public string Name { get; set; }

            /// <summary>
            /// Gets or sets the property value.
            /// </summary>
            public object Value { get; set; }
        }
    }

    /// <summary>
    /// Represents a <see cref="CachePolicyConfig"/> exception.
    /// </summary>
    public class CachePolicyConfigException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CachePolicyConfigException"/> class with a <paramref name="message"/>.
        /// </summary>
        /// <param name="message">The exception message.</param>
        public CachePolicyConfigException(string message) : base(message) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="CachePolicyConfigException"/> class with a <paramref name="message"/> and <paramref name="innerException"/>.
        /// </summary>
        /// <param name="message">The exception message.</param>
        /// <param name="innerException">The inner <see cref="Exception"/>.</param>
        public CachePolicyConfigException(string message, Exception innerException) : base(message, innerException) { }
    }
}