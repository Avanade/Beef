using CoreEx;
using CoreEx.Entities;
using CoreEx.Http;
using CoreEx.Json;
using Moq;
using Moq.Language;
using Moq.Language.Flow;
using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using UnitTestEx;
using UnitTestEx.Expectations;

namespace Beef.Test.NUnit
{
    /// <summary>
    /// Provides additional extension methods to support backwards compatibility to earlier <i>Beef</i> versions.
    /// </summary>
    public static class ExtensionMethods
    {
        /// <summary>
        /// Expect a <see cref="ValidationException"/> was thrown during execution with the specified <see cref="MessageType.Error"/> messages.
        /// </summary>
        /// <typeparam name="TSelf">The tester <see cref="Type"/>.</typeparam>
        /// <param name="tester">The <see cref="IHttpResponseExpectations{TSelf}"/>.</param>
        /// <param name="messages">The expected <see cref="MessageType.Error"/> message texts.</param>
        /// <returns>The <typeparamref name="TSelf"/> instance to support fluent-style method-chaining.</returns>
        /// <remarks>Provided to support backwards compatibility to earlier <i>Beef</i> versions. It is <b>recommended</b> that usage is upgraded to use <see cref="UnitTestEx.Expectations.ExpectationsExtensions.ExpectErrors{TSelf}(IExceptionSuccessExpectations{TSelf}, string[])"/> as this will eventually be deprecated.</remarks>
        public static TSelf ExpectMessages<TSelf>(this IExceptionSuccessExpectations<TSelf> tester, params string[] messages) where TSelf : IExceptionSuccessExpectations<TSelf>
        { 
            tester.ExceptionSuccessExpectations.SetExpectErrors(messages);
            return (TSelf)tester;
        }

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