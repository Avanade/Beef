﻿// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using System;
using System.Collections.Generic;
using System.Net.Http;

namespace Beef.WebApi
{
    /// <summary>
    /// Provides the underlying <b>ServiceAgent</b> configuration.
    /// </summary>
    public class WebApiServiceAgentConfig
    {
#pragma warning disable CA2227 // Collection properties should be read only; by-design, can be overridden.
        /// <summary>
        /// Gets or sets the <see cref="WebApiServiceAgentConfigItem"/> array.
        /// </summary>
        public List<WebApiServiceAgentConfigItem> ServiceAgents { get; set; }
#pragma warning restore CA2227

        /// <summary>
        /// Performs the <see cref="WebApiServiceAgentManager.Register(string, Uri, Action{System.Net.Http.HttpRequestMessage})"/> for all <see cref="ServiceAgents"/>.
        /// </summary>
        /// <param name="beforeRequestOverride">An <see cref="Action{HttpRequestMessage, ServiceAgentConfigItem}"/> to invoke before the <see cref="HttpRequestMessage"/> is
        /// sent (overriding the default behaviour).</param>
        public void RegisterAll(Action<HttpRequestMessage, WebApiServiceAgentConfigItem> beforeRequestOverride = null)
        {
            if (ServiceAgents != null)
            {
                foreach (var sa in ServiceAgents)
                {
                    sa.Register(beforeRequestOverride);
                }
            }
        }
    }

    /// <summary>
    /// Represents the <b>ServiceAgent</b> configuration item.
    /// </summary>
    public class WebApiServiceAgentConfigItem
    {
        private Action<HttpRequestMessage, WebApiServiceAgentConfigItem> _beforeRequestOverride;

        /// <summary>
        /// Gets or sets the namespace required for the <see cref="WebApiServiceAgentManager"/>.
        /// </summary>
        public string Namespace { get; set; }

#pragma warning disable CA1056 // Uri properties should not be strings; by-design, comes from configuration string.
        /// <summary>
        /// Gets or sets the base URL endpoint.
        /// </summary>
        public string BaseUrl { get; set; }
#pragma warning restore CA1056

        /// <summary>
        /// Performs the <see cref="WebApiServiceAgentManager.Register(string, Uri, Action{System.Net.Http.HttpRequestMessage})"/>.
        /// </summary>
        /// <param name="beforeRequestOverride">An <see cref="Action{HttpRequestMessage, ServiceAgentConfigItem}"/> to invoke before the <see cref="HttpRequestMessage"/> is
        ///  sent (overriding the default behaviour).</param>
        public void Register(Action<HttpRequestMessage, WebApiServiceAgentConfigItem> beforeRequestOverride = null)
        {
            _beforeRequestOverride = beforeRequestOverride;
            if (_beforeRequestOverride == null)
                WebApiServiceAgentManager.Register(Namespace, new Uri(BaseUrl), null);
            else
                WebApiServiceAgentManager.Register(Namespace, new Uri(BaseUrl), BeforeHttpRequestOverride);
        }

        /// <summary>
        /// Updates the <see cref="HttpRequestMessage"/> before the send using the specified override.
        /// </summary>
        private void BeforeHttpRequestOverride(HttpRequestMessage requestMessage)
        {
            _beforeRequestOverride(requestMessage, this);
        }
    }
}