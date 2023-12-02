// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using CoreEx.Http;
using CoreEx.Json;
using Moq;
using Moq.Language;
using Moq.Language.Flow;
using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace Beef.Test.NUnit
{
    /// <summary>
    /// Provides additional extension methods to support backwards compatibility to earlier <i>Beef</i> versions.
    /// </summary>
    public static class ExtensionMethods
    {
        /// <summary>
        /// Extends <paramref name="mock"/> to simplify the return of a mocked <see cref="HttpResult"/> with the specified <paramref name="statusCode"/> (defaults to <see cref="HttpStatusCode.OK"/>).
        /// </summary>
        /// <typeparam name="TMock">The mock object <see cref="Type"/>.</typeparam>
        /// <param name="mock">The mock object.</param>
        /// <param name="statusCode">The <see cref="HttpStatusCode"/>.</param>
        /// <returns>The <see cref="HttpResult"/> with no result.</returns>
        public static IReturnsResult<TMock> ReturnsHttpResultAsync<TMock>(this IReturns<TMock, Task<HttpResult>> mock, HttpStatusCode statusCode = HttpStatusCode.OK) where TMock : class =>
            mock.ReturnsAsync(() => HttpResult.CreateAsync(new HttpResponseMessage() { StatusCode = statusCode }).GetAwaiter().GetResult());

        /// <summary>
        /// Extends <paramref name="mock"/> to simplify the return of a mocked <see cref="HttpResult{T}"/> with the specified <paramref name="statusCode"/> (defaults to <see cref="HttpStatusCode.NoContent"/>).
        /// </summary>
        /// <typeparam name="TMock">The mock object <see cref="Type"/>.</typeparam>
        /// <typeparam name="T">The resultant <see cref="Type"/>.</typeparam>
        /// <param name="mock">The mock object.</param>
        /// <param name="statusCode">The <see cref="HttpStatusCode"/>.</param>
        /// <returns>The <see cref="HttpResult{T}"/> with no result.</returns>
        public static IReturnsResult<TMock> ReturnsHttpResultAsync<TMock, T>(this IReturns<TMock, Task<HttpResult<T>>> mock, HttpStatusCode statusCode = HttpStatusCode.NoContent) where TMock : class =>
            mock.ReturnsAsync(() => HttpResult.CreateAsync<T>(new HttpResponseMessage() { StatusCode = statusCode }, JsonSerializer.Default).GetAwaiter().GetResult());

        /// <summary>
        /// Extends <paramref name="mock"/> to simplify the return of a mocked <see cref="HttpResult{T}"/> with the specified <paramref name="statusCode"/> (defaults to <see cref="HttpStatusCode.NoContent"/>).
        /// </summary>
        /// <typeparam name="TMock">The mock object <see cref="Type"/>.</typeparam>
        /// <typeparam name="T">The resultant <see cref="Type"/>.</typeparam>
        /// <param name="mock">The mock object.</param>
        /// <param name="value">The value.</param>
        /// <param name="statusCode">The <see cref="HttpStatusCode"/>.</param>
        /// <param name="jsonSerializer">The <see cref="IJsonSerializer"/>. Defaults to <see cref="JsonSerializer.Default"/>.</param>
        /// <returns>The <see cref="HttpResult{T}"/> with no result.</returns>
        public static IReturnsResult<TMock> ReturnsHttpResultAsync<TMock, T>(this IReturns<TMock, Task<HttpResult<T>>> mock, T value, HttpStatusCode statusCode = HttpStatusCode.OK, IJsonSerializer? jsonSerializer = null) where TMock : class =>
            mock.ReturnsAsync(() =>
            {
                var js = jsonSerializer ?? JsonSerializer.Default;
                return HttpResult.CreateAsync<T>(new HttpResponseMessage() { StatusCode = statusCode, Content = new StringContent(js.Serialize(value)) }, js).GetAwaiter().GetResult();
            });
    }
}