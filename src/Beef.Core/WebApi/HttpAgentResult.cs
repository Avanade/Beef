// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using AutoMapper;
using Beef.Entities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Net.Http;

namespace Beef.WebApi
{
    /// <summary>
    /// Provides the common <b>HTTP Agent</b> result.
    /// </summary>
    public interface IHttpAgentResult : IWebApiAgentResult 
    {
        /// <summary>
        /// Gets the <see cref="HttpSendArgs"/>.
        /// </summary>
        HttpSendArgs SendArgs { get; }

        /// <summary>
        /// Indicates whether the <see cref="GetValue"/> method is supported.
        /// </summary>
        bool IsValueSupported { get; }

        /// <summary>
        /// Indicates whether to convert the <see cref="HttpStatusCode"/> to the equivalent <see cref="Beef.IBusinessException"/>.
        /// </summary>
        bool ConvertHttpStatusToBeefException { get; }

        /// <summary>
        /// Gets the value where <see cref="IsValueSupported"/>; otherwise should throw a <see cref="NotSupportedException"/>.
        /// </summary>
        /// <returns>The value.</returns>
        object? GetValue();
    }

    /// <summary>
    /// Provides the common <b>HTTP Agent</b> result with a <see cref="IWebApiAgentResult{T}.Value"/>.
    /// </summary>
    public interface IHttpAgentResult<T> : IWebApiAgentResult<T>, IHttpAgentResult { }

    /// <summary>
    /// Represents a result for the <see cref="HttpAgentBase"/>.
    /// </summary>
    public class HttpAgentResult : WebApiAgentResult, IHttpAgentResult
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WebApiAgentResult"/> class.
        /// </summary>
        /// <param name="sendArgs">The corresponding <see cref="HttpSendArgs"/>.</param>
        /// <param name="response">The <see cref="HttpResponseMessage"/>.</param>
        public HttpAgentResult(HttpSendArgs sendArgs, HttpResponseMessage response) : base(response) => SendArgs = Check.NotNull(sendArgs, nameof(sendArgs));

        /// <summary>
        /// Gets the corresponding <see cref="HttpSendArgs"/>.
        /// </summary>
        public HttpSendArgs SendArgs { get; }

        /// <summary>
        /// Indicates whether the <see cref="IHttpAgentResult.GetValue"/> method is supported; which it is not.
        /// </summary>
        public bool IsValueSupported => false;

        /// <summary>
        /// This methiod is not supported and will throw a <see cref="NotSupportedException"/>.
        /// </summary>
        object? IHttpAgentResult.GetValue() => throw new NotImplementedException();

        /// <summary>
        /// Indicates whether to convert the <see cref="HttpStatusCode"/> to the equivalent <see cref="Beef.IBusinessException"/>.
        /// </summary>
        public bool ConvertHttpStatusToBeefException { get; set; } = true;

        /// <summary>
        /// Throws an exception if the <see cref="WebApiAgentResult.Response"/> <see cref="HttpResponseMessage.IsSuccessStatusCode"/> for the HTTP response is false 
        /// (see <see cref="HttpResponseMessage.EnsureSuccessStatusCode"/>).
        /// </summary>
        /// <returns>The <see cref="HttpAgentResult"/> instance to support fluent/chaining usage.</returns>
        public new HttpAgentResult ThrowOnError()
        {
            ThrowBeefExceptionOnError(this);
            base.ThrowOnError();
            return this;
        }

        /// <summary>
        /// Throws the equivalent <see cref="Beef.IBusinessException"/> for the <see cref="HttpStatusCode"/> (where applicable).
        /// </summary>
        internal static void ThrowBeefExceptionOnError(IHttpAgentResult result)
        {
            if (!result.IsSuccess && result.ConvertHttpStatusToBeefException)
            {
                switch (result.StatusCode)
                {
                    case HttpStatusCode.NotFound: throw new NotFoundException(result.ErrorMessage);
                    case HttpStatusCode.PreconditionFailed: throw new ConcurrencyException(result.ErrorMessage);
                    case HttpStatusCode.Conflict: throw new ConflictException(result.ErrorMessage);
                    case HttpStatusCode.BadRequest: throw new ValidationException(result.ErrorMessage, result.Messages ?? new MessageItemCollection());
                    case HttpStatusCode.Forbidden: throw new AuthorizationException(result.ErrorMessage);
                    case HttpStatusCode.Unauthorized: throw new AuthenticationException(result.ErrorMessage);
                }
            }
        }
    }

    /// <summary>
    /// Represents a result for the <see cref="HttpAgentBase"/> with a deserialized (JSON) response <see cref="IWebApiAgentResult{T}.Value"/>.
    /// </summary>
    public class HttpAgentResult<T> : WebApiAgentResult, IHttpAgentResult<T>
    {
        private bool _isValueSet = false;
        private T _value = default!;

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpAgentResult{T}"/> class.
        /// </summary>
        /// <param name="sendArgs">The corresponding <see cref="HttpSendArgs"/>.</param>
        /// <param name="response">The <see cref="HttpResponseMessage"/>.</param>
        /// <param name="overrideValue">The value overriding the internal content deserialization.</param>
        public HttpAgentResult(HttpSendArgs sendArgs, HttpResponseMessage response, T overrideValue = default!) : base(response)
        {
            SendArgs = Check.NotNull(sendArgs, nameof(sendArgs));
            if (Comparer<T>.Default.Compare(overrideValue, default!) != 0)
            {
                _value = overrideValue;
                _isValueSet = true;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpAgentResult{T}"/> class using an existing <see cref="WebApiAgentResult"/>.
        /// </summary>
        /// <param name="result">The result containing the <see cref="WebApiAgentResult.Content"/> to deserialize.</param>
        /// <param name="overrideValue">The value overriding the internal content deserialization.</param>
        public HttpAgentResult(HttpAgentResult result, T overrideValue = default!) : this(Check.NotNull(result, nameof(result)).SendArgs, result.Response, overrideValue)
        {
            Content = result.Content;
            Messages = result.Messages;
            ErrorType = result.ErrorType;
            ErrorMessage = result.ErrorMessage;
        }

        /// <summary>
        /// Gets the corresponding <see cref="HttpSendArgs"/>.
        /// </summary>
        public HttpSendArgs SendArgs { get; }

        /// <summary>
        /// Gets the deserialized (JSON) response value.
        /// </summary>
        /// <remarks>Performs a <see cref="ThrowOnError"/> before attempting to deserialize the content value.</remarks>
        public T Value
        {
            get
            {
                if (StatusCode == HttpStatusCode.NotFound && SendArgs.NullOnNotFoundResponse)
                    return _value;

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
        /// Indicates whether to convert the <see cref="HttpStatusCode"/> to the equivalent <see cref="Beef.IBusinessException"/>.
        /// </summary>
        public bool ConvertHttpStatusToBeefException { get; set; } = true;

        /// <summary>
        /// Throws an exception if the request was not successful (see <see cref="IWebApiAgentResult.IsSuccess"/>).
        /// </summary>
        /// <returns>The <see cref="WebApiAgentResult"/> instance to support fluent/chaining usage.</returns>
        IWebApiAgentResult IWebApiAgentResult.ThrowOnError() => ThrowOnError();

        /// <summary>
        /// Throws an exception if the <see cref="WebApiAgentResult.Response"/> <see cref="HttpResponseMessage.IsSuccessStatusCode"/> for the HTTP response is false 
        /// (see <see cref="HttpResponseMessage.EnsureSuccessStatusCode"/>).
        /// </summary>
        /// <returns>The <see cref="HttpAgentResult{T}"/> instance to support fluent/chaining usage.</returns>
        public new HttpAgentResult<T> ThrowOnError()
        {
            if (StatusCode == HttpStatusCode.NotFound && SendArgs.NullOnNotFoundResponse)
                return this;

            HttpAgentResult.ThrowBeefExceptionOnError(this);
            base.ThrowOnError();
            return this;
        }

        /// <summary>
        /// Indicates whether the <see cref="IHttpAgentResult.GetValue"/> is supported; which it is.
        /// </summary>
        public bool IsValueSupported => true;

        /// <summary>
        /// Gets the value.
        /// </summary>
        object? IHttpAgentResult.GetValue() => Value;
    }

    /// <summary>
    /// Represents a result for the <see cref="HttpAgentBase"/> with a deserialized (JSON) and mapped response <see cref="IWebApiAgentResult{T}.Value"/>.
    /// </summary>
    public class HttpAgentResult<T, TModel> : WebApiAgentResult, IHttpAgentResult<T>
    {
        private readonly IMapper _mapper;
        private bool _isValueSet = false;
        private T _value = default!;

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpAgentResult{T, TModel}"/> class.
        /// </summary>
        /// <param name="sendArgs">The corresponding <see cref="HttpSendArgs"/>.</param>
        /// <param name="mapper">The <see cref="IMapper"/> to map the <typeparamref name="TModel"/> to <typeparamref name="T"/>.</param>
        /// <param name="response">The <see cref="HttpResponseMessage"/>.</param>
        /// <param name="overrideValue">The value overriding the internal content deserialization.</param>
        public HttpAgentResult(HttpSendArgs sendArgs, IMapper mapper, HttpResponseMessage response, T overrideValue = default!) : base(response)
        {
            SendArgs = Check.NotNull(sendArgs, nameof(sendArgs));
            _mapper = Check.NotNull(mapper, nameof(mapper));
            if (Comparer<T>.Default.Compare(overrideValue, default!) != 0)
            {
                _value = overrideValue;
                _isValueSet = true;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpAgentResult{T, TModel}"/> class using an existing <see cref="WebApiAgentResult"/>.
        /// </summary>
        /// <param name="mapper">The <see cref="IMapper"/> to map the <typeparamref name="TModel"/> to <typeparamref name="T"/>.</param>
        /// <param name="result">The result containing the <see cref="WebApiAgentResult.Content"/> to deserialize.</param>
        /// <param name="overrideValue">The value overriding the internal content deserialization.</param>
        public HttpAgentResult(IMapper mapper, HttpAgentResult result, T overrideValue = default!) : this(Check.NotNull(result, nameof(result)).SendArgs, mapper, result.Response, overrideValue)
        {
            Content = result.Content;
            Messages = result.Messages;
            ErrorType = result.ErrorType;
            ErrorMessage = result.ErrorMessage;
        }

        /// <summary>
        /// Gets the corresponding <see cref="HttpSendArgs"/>.
        /// </summary>
        public HttpSendArgs SendArgs { get; }

        /// <summary>
        /// Gets the deserialized (JSON) response value.
        /// </summary>
        /// <remarks>Performs a <see cref="ThrowOnError"/> before attempting to deserialize the content value.</remarks>
        public T Value
        {
            get
            {
                if (StatusCode == HttpStatusCode.NotFound && SendArgs.NullOnNotFoundResponse)
                    return _value;

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
                        var model = JsonConvert.DeserializeObject<TModel>(Content)!;
                        if (model != null && model is IETag eTag)
                        {
                            if (eTag.ETag == null && Response.Headers.ETag != null)
                                eTag.ETag = Response.Headers.ETag.Tag;
                        }

                        _value = _mapper.Map<TModel, T>(model);
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
        /// Indicates whether to convert the <see cref="HttpStatusCode"/> to the equivalent <see cref="Beef.IBusinessException"/>.
        /// </summary>
        public bool ConvertHttpStatusToBeefException { get; set; } = true;

        /// <summary>
        /// Throws an exception if the request was not successful (see <see cref="IWebApiAgentResult.IsSuccess"/>).
        /// </summary>
        /// <returns>The <see cref="WebApiAgentResult"/> instance to support fluent/chaining usage.</returns>
        IWebApiAgentResult IWebApiAgentResult.ThrowOnError() => ThrowOnError();

        /// <summary>
        /// Throws an exception if the <see cref="WebApiAgentResult.Response"/> <see cref="HttpResponseMessage.IsSuccessStatusCode"/> for the HTTP response is false 
        /// (see <see cref="HttpResponseMessage.EnsureSuccessStatusCode"/>).
        /// </summary>
        /// <returns>The <see cref="HttpAgentResult{T, TModel}"/> instance to support fluent/chaining usage.</returns>
        public new HttpAgentResult<T, TModel> ThrowOnError()
        {
            if (StatusCode == HttpStatusCode.NotFound && SendArgs.NullOnNotFoundResponse)
                return this;

            HttpAgentResult.ThrowBeefExceptionOnError(this);
            base.ThrowOnError();
            return this;
        }

        /// <summary>
        /// Indicates whether the <see cref="IHttpAgentResult.GetValue"/> is supported; which it is.
        /// </summary>
        public bool IsValueSupported => true;

        /// <summary>
        /// Gets the value.
        /// </summary>
        object? IHttpAgentResult.GetValue() => Value;
    }
}