// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.Events.Triggers.Config;
using Beef.Events.Triggers.Listener;
using EventHubs = Microsoft.Azure.EventHubs;
using Microsoft.Azure.EventHubs.Processor;
using Microsoft.Azure.WebJobs.Host.Bindings;
using Microsoft.Azure.WebJobs.Host.Listeners;
using Microsoft.Azure.WebJobs.Host.Protocols;
using Microsoft.Azure.WebJobs.Host.Triggers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Beef.Events.Triggers.Bindings
{
    /// <summary>
    /// Performs the "resilient event hub" binding.
    /// </summary>
    internal class ResilientEventHubBinding : ITriggerBinding
    {
        private readonly EventProcessorHost _host;
        private readonly ParameterInfo _parameter;
        private readonly ResilientEventHubOptions _options;
        private readonly IConfiguration _config;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="ResilientEventHubBinding"/> class.
        /// </summary>
        /// <param name="host">The <see cref="EventProcessorHost"/>.</param>
        /// <param name="parameter">The originating function <see cref="ParameterInfo"/>.</param>
        /// <param name="options">The <see cref="ResilientEventHubOptions"/>.</param>
        /// <param name="config">The <see cref="IConfiguration"/>.</param>
        /// <param name="logger">The <see cref="ILogger"/>.</param>
        public ResilientEventHubBinding(EventProcessorHost host, ParameterInfo parameter, ResilientEventHubOptions options, IConfiguration config, ILogger logger)
        {
            _host = host;
            _parameter = parameter;
            _options = options;
            _config = config;
            _logger = logger;
        }

        /// <summary>
        /// Gets the binding data contract.
        /// </summary>
        public IReadOnlyDictionary<string, Type> BindingDataContract { get; } = new Dictionary<string, Type>(StringComparer.OrdinalIgnoreCase) { { "ResilientEventHubTrigger", typeof(string) } };

        /// <summary>
        /// Gets the trigger value <see cref="Type"/>.
        /// </summary>
        public Type TriggerValueType => typeof(ResilientEventHubData);

        /// <summary>
        /// Bind the <paramref name="value"/> creating the <see cref="ITriggerData"/>.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="context">The <see cref="ValueBindingContext"/>.</param>
        /// <returns>The <see cref="ITriggerData"/>.</returns>
        public Task<ITriggerData> BindAsync(object value, ValueBindingContext context)
        {
            IValueProvider valueProvider;
            switch (value)
            {
                case ResilientEventHubData mehi:
                    valueProvider = new ValueProvider(mehi.EventData);
                    break;

                case string str:
                    valueProvider = new ValueProvider(new EventHubs.EventData(Encoding.UTF8.GetBytes(str)));
                    break;

                default:
                    throw new InvalidOperationException($"Unable to bind as the value is not a 'ResilientEventHubInfo' Type; is Type {value.GetType()}.");
            }

            IReadOnlyDictionary<string, object> bindingData = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase) { { "ResilientEventHubTrigger", DateTime.Now.ToString(System.Globalization.CultureInfo.InvariantCulture) } };
            return Task.FromResult((ITriggerData)new TriggerData(valueProvider, bindingData));
        }

        /// <summary>
        /// Create the <see cref="ResilientEventHubListener"/>.
        /// </summary>
        /// <param name="context">The <see cref="ListenerFactoryContext"/>.</param>
        /// <returns>The <see cref="ResilientEventHubListener"/>.</returns>
        public Task<IListener> CreateListenerAsync(ListenerFactoryContext context)
        {
            return Task.FromResult<IListener>(new ResilientEventHubListener(_host, context.Executor, _options, _config, _logger));
        }

        /// <summary>
        /// Creates a <see cref="TriggerParameterDescriptor"/>.
        /// </summary>
        /// <returns>The <see cref="TriggerParameterDescriptor"/>.</returns>
        public ParameterDescriptor ToParameterDescriptor()
        {
            return new TriggerParameterDescriptor
            {
                 Name = _parameter.Name,
                 DisplayHints = new ParameterDisplayHints { Description = "Resilient EventHub Trigger" }
            };
        }

        /// <summary>
        /// Represents an <see cref="IValueProvider"/> for the <see cref="EventData"/>.
        /// </summary>
        private class ValueProvider : IValueProvider
        {
            private readonly object _value;

            /// <summary>
            /// Initializes a new instance of the <see cref="ValueProvider"/> class.
            /// </summary>
            /// <param name="value">The value.</param>
            public ValueProvider(object value)
            {
                _value = value;
            }

            /// <summary>
            /// Gets the value <see cref="Type"/>.
            /// </summary>
            public Type Type => typeof(EventData);

            /// <summary>
            /// Gets the value.
            /// </summary>
            /// <returns>The value.</returns>
            public Task<object> GetValueAsync()
            {
                return Task.FromResult(_value);
            }

            /// <summary>
            /// Gets a string representation.
            /// </summary>
            public string ToInvokeString()
            {
                return DateTime.Now.ToString("o", System.Globalization.CultureInfo.InvariantCulture);
            }
        }
    }
}