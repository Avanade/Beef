// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using System;
using System.Net.Http;

namespace Beef.Data.OData
{
    /// <summary>
    /// Represents the <see cref="OData"/> <see cref="RequestMessage"/> <see cref="EventArgs"/>.
    /// </summary>
    public class OdmRequestMessageEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OdmRequestMessageEventArgs"/> class.
        /// </summary>
        /// <param name="request">The <see cref="HttpRequestMessage"/>.</param>
        public OdmRequestMessageEventArgs(HttpRequestMessage request)
        {
            RequestMessage = request ?? throw new ArgumentNullException(nameof(request));
        }

        /// <summary>
        /// Gets the <see cref="HttpRequestMessage"/>.
        /// </summary>
        public HttpRequestMessage RequestMessage { get; private set; }
    }
}
