// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.WebApi;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Moq.Language;
using Moq.Language.Flow;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace Beef.Test.NUnit
{
    /// <summary>
    /// Provides extension methods to support testing.
    /// </summary>
    public static class ExtensionMethods
    {
        /// <summary>
        /// Converts an <see cref="int"/> to a <see cref="Guid"/>; e.g. '1' will be '00000001-0000-0000-0000-000000000000'.
        /// </summary>
        /// <param name="value">The <see cref="int"/> value.</param>
        /// <returns>The corresponding <see cref="Guid"/>.</returns>
        /// <remarks>Sets the first argument with the <paramref name="value"/> and the remainder with zeroes using <see cref="Guid(int, short, short, byte[])"/>.</remarks>
        public static Guid ToGuid(this int value)
        {
            return new Guid(value, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0);
        }

        /// <summary>
        /// Creates a long string by repeating the character for the specified count (defaults to 250).
        /// </summary>
        /// <param name="value">The character value.</param>
        /// <param name="count">The repeating count.</param>
        /// <returns>The resulting string.</returns>
        public static string ToLongString(this char value, int count = 250)
        {
            return new string(value, count);
        }

        /// <summary>
        /// Extends <paramref name="mock"/> to simplify the return of a mocked <see cref="WebApiAgentResult{TEntity}"/> with no result.
        /// </summary>
        /// <typeparam name="TMock">The mock object <see cref="Type"/>.</typeparam>
        /// <typeparam name="TEntity">The resultant entity <see cref="Type"/>.</typeparam>
        /// <param name="mock">The mock object.</param>
        /// <param name="statusCode">The <see cref="HttpStatusCode"/>.</param>
        /// <returns>The <see cref="WebApiAgentResult{TEntity}"/> with no result.</returns>
        public static IReturnsResult<TMock> ReturnsWebApiAgentResultAsync<TMock, TEntity>(this IReturns<TMock, Task<WebApiAgentResult<TEntity>>> mock, HttpStatusCode statusCode = HttpStatusCode.NoContent) where TMock : class
        {
            return mock.ReturnsAsync(() => new WebApiAgentResult<TEntity>(new HttpResponseMessage() { StatusCode = statusCode }));
        }

        /// <summary>
        /// Extends <paramref name="mock"/> to simplify the return of a mocked <see cref="WebApiAgentResult{TEntity}"/> with an entity result.
        /// </summary>
        /// <typeparam name="TMock">The mock object <see cref="Type"/>.</typeparam>
        /// <typeparam name="TEntity">The resultant entity <see cref="Type"/>.</typeparam>
        /// <param name="mock">The mock object.</param>
        /// <param name="entity">The entity value.</param>
        /// <param name="statusCode">The <see cref="HttpStatusCode"/>.</param>
        /// <returns>The <see cref="WebApiAgentResult{TEntity}"/> with an entity result.</returns>
        public static IReturnsResult<TMock> ReturnsWebApiAgentResultAsync<TMock, TEntity>(this IReturns<TMock, Task<WebApiAgentResult<TEntity>>> mock, TEntity entity, HttpStatusCode statusCode = HttpStatusCode.OK) where TMock : class
        {
            return mock.ReturnsAsync(() => new WebApiAgentResult<TEntity>(new HttpResponseMessage(statusCode), entity));
        }

        /// <summary>
        /// Removes all items from the <see cref="IServiceCollection"/> for the specified <typeparamref name="TService"/>.
        /// </summary>
        /// <typeparam name="TService">The service <see cref="Type"/>.</typeparam>
        /// <param name="services">The <see cref="IServiceCollection"/>.</param>
        /// <returns><c>true</c> if item was successfully removed; otherwise, <c>false</c>. Also returns <c>false</c> if item was not found.</returns>
        public static bool Remove<TService>(this IServiceCollection services) where TService : class
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));

            var descriptor = services.FirstOrDefault(d => d.ServiceType == typeof(TService));
            return descriptor != null && services.Remove(descriptor);
        }
    }
}