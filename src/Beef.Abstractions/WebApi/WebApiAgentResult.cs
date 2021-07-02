// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.Entities;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Net.Http;

namespace Beef.WebApi
{
    /// <summary>
    /// Represents a result for the <see cref="WebApiAgentBase"/>.
    /// </summary>
    public class WebApiAgentResult : IWebApiAgentResult
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WebApiAgentResult"/> class.
        /// </summary>
        /// <param name="response">The <see cref="HttpResponseMessage"/>.</param>
        public WebApiAgentResult(HttpResponseMessage response)
        {
            Response = Check.NotNull(response, nameof(response));
        }

        /// <summary>
        /// Gets the underlying <see cref="HttpRequestMessage"/>.
        /// </summary>
        public HttpRequestMessage Request => Response.RequestMessage;

        /// <summary>
        /// Gets the <see cref="HttpResponseMessage"/>.
        /// </summary>
        public HttpResponseMessage Response { get; private set; }

#pragma warning disable CA2227 // Collection properties should be read only; by-design, can be updated.
        /// <summary>
        /// Gets or sets the <see cref="MessageItemCollection"/>.
        /// </summary>
        public MessageItemCollection? Messages { get; set; }
#pragma warning restore CA2227

        /// <summary>
        /// Gets or sets the <see cref="HttpStatusCode"/>.
        /// </summary>
        public HttpStatusCode StatusCode => Response.StatusCode;

        /// <summary>
        /// Gets or sets the known <see cref="ErrorType"/>; otherwise, <c>null</c> indicates an unknown error type.
        /// </summary>
        public ErrorType? ErrorType { get; set; }

        /// <summary>
        /// Gets or sets the error message for the corresponding <see cref="ErrorType"/>.
        /// </summary>
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// Gets or sets the response body content.
        /// </summary>
        public string? Content { get; set; }

        /// <summary>
        /// Indicates whether the request was successful (i.e. <see cref="Response"/> <see cref="HttpResponseMessage.IsSuccessStatusCode"/>.
        /// </summary>
        public bool IsSuccess => Response.IsSuccessStatusCode;

        /// <summary>
        /// Throws an exception if the request was not successful (see <see cref="IsSuccess"/>).
        /// </summary>
        /// <returns>The <see cref="WebApiAgentResult"/> instance to support fluent/chaining usage.</returns>
        IWebApiAgentResult IWebApiAgentResult.ThrowOnError() => ThrowOnError();

        /// <summary>
        /// Throws an exception if the <see cref="Response"/> <see cref="HttpResponseMessage.IsSuccessStatusCode"/> for the HTTP response is false 
        /// (see <see cref="HttpResponseMessage.EnsureSuccessStatusCode"/>). Where the <see cref="ErrorType"/> is known then the corresponding <b>BEEF</b> exception will be thrown.
        /// </summary>
        /// <returns>The <see cref="WebApiAgentResult"/> instance to support fluent/chaining usage.</returns>
        public WebApiAgentResult ThrowOnError()
        {
            if (Response.IsSuccessStatusCode)
                return this;

            // Throw the beef exception if a known error type.
            if (ErrorType.HasValue)
            {
                switch (ErrorType.Value)
                {
                    case Beef.ErrorType.AuthorizationError:
                        throw new AuthorizationException(ErrorMessage);

                    case Beef.ErrorType.BusinessError:
                        throw new BusinessException(ErrorMessage);

                    case Beef.ErrorType.ConcurrencyError:
                        throw new ConcurrencyException(ErrorMessage);

                    case Beef.ErrorType.ConflictError:
                        throw new ConflictException(ErrorMessage);

                    case Beef.ErrorType.NotFoundError:
                        throw new NotFoundException(ErrorMessage);

                    case Beef.ErrorType.ValidationError:
                        throw new ValidationException(ErrorMessage, Messages ?? new MessageItemCollection());

                    case Beef.ErrorType.DuplicateError:
                        throw new DuplicateException(ErrorMessage);

                    case Beef.ErrorType.AuthenticationError:
                        throw new AuthenticationException(ErrorMessage);
                }
            }

            // Throw the catch-all http operation exception.
            Response.EnsureSuccessStatusCode();

            return this;
        }
    }

    /// <summary>
    /// Represents a result for the <see cref="WebApiAgentBase"/> with a deserialized (JSON) response <see cref="Value"/>.
    /// </summary>
    public class WebApiAgentResult<T> : WebApiAgentResult, IWebApiAgentResult<T>
    {
        private bool _isValueSet = false;
        private T _value = default!;

        /// <summary>
        /// Initializes a new instance of the <see cref="WebApiAgentResult{T}"/> class.
        /// </summary>
        /// <param name="response">The <see cref="HttpResponseMessage"/>.</param>
        /// <param name="overrideValue">The value overriding the internal content deserialization.</param>
        public WebApiAgentResult(HttpResponseMessage response, T overrideValue = default!) : base(response)
        {
            if (Comparer<T>.Default.Compare(overrideValue, default!) != 0)
            {
                _value = overrideValue;
                _isValueSet = true;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WebApiAgentResult{T}"/> class using an existing <see cref="WebApiAgentResult"/>.
        /// </summary>
        /// <param name="result">The result containing the <see cref="WebApiAgentResult.Content"/> to deserialize.</param>
        /// <param name="overrideValue">The value overriding the internal content deserialization.</param>
        public WebApiAgentResult(WebApiAgentResult result, T overrideValue = default!) : this(Check.NotNull(result, nameof(result)).Response, overrideValue)
        {
            Content = result.Content;
            Messages = result.Messages;
            ErrorType = result.ErrorType;
            ErrorMessage = result.ErrorMessage;
        }

        /// <summary>
        /// Gets the deserialized (JSON) response value.
        /// </summary>
        /// <remarks>Performs a <see cref="ThrowOnError"/> before attempting to deserialize the content value.</remarks>
        public T Value
        {
            get
            {
                ThrowOnError();

                if (_isValueSet)
                    return _value;

                if (Content != null)
                {
                    if (typeof(T) == typeof(string) && Response.Content.Headers.ContentType.MediaType == "text/plain")
                    {
                        _value = (T)System.Convert.ChangeType(Content, typeof(T), CultureInfo.CurrentCulture);
                    }
                    else
                    {
                        _value = JsonConvert.DeserializeObject<T>(Content)!;
                        if (_value != null && _value is IETag eTag)
                        {
                            if (eTag.ETag == null && Response.Headers.ETag != null)
                                eTag.ETag = Response.Headers.ETag.Tag;
                        }
                    }
                }

                _isValueSet = true;
                return _value;
            }
        }

        /// <summary>
        /// Indicates whether a <see cref="Value"/> was returned as <see cref="WebApiAgentResult.Content"/>.
        /// </summary>
        public bool HasValue => _isValueSet || !string.IsNullOrEmpty(Content);

        /// <summary>
        /// Throws an exception if the request was not successful (see <see cref="IWebApiAgentResult.IsSuccess"/>).
        /// </summary>
        /// <returns>The <see cref="WebApiAgentResult"/> instance to support fluent/chaining usage.</returns>
        IWebApiAgentResult IWebApiAgentResult.ThrowOnError() => ThrowOnError();

        /// <summary>
        /// Throws an exception if the <see cref="WebApiAgentResult.Response"/> <see cref="HttpResponseMessage.IsSuccessStatusCode"/> for the HTTP response is false 
        /// (see <see cref="HttpResponseMessage.EnsureSuccessStatusCode"/>).
        /// </summary>
        /// <returns>The <see cref="WebApiAgentResult"/> instance to support fluent/chaining usage.</returns>
        public new WebApiAgentResult<T> ThrowOnError()
        {
            base.ThrowOnError();
            return this;
        }
    }
}