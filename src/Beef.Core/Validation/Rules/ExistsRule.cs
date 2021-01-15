// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.WebApi;
using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace Beef.Validation.Rules
{
    /// <summary>
    /// Provides validation where the rule predicate <b>must</b> return <c>true</c> or a value to verify it exists.
    /// </summary>
    /// <typeparam name="TEntity">The entity <see cref="Type"/>.</typeparam>
    /// <typeparam name="TProperty">The property <see cref="Type"/>.</typeparam>
    public class ExistsRule<TEntity, TProperty> : ValueRuleBase<TEntity, TProperty> where TEntity : class
    {
        private readonly Predicate<TEntity>? _predicate;
        private readonly Func<TEntity, Task<bool>>? _exists;
        private readonly Func<TEntity, Task<object>>? _existsNotNull;
        private readonly Func<TEntity, Task<WebApiAgentResult>>? _agentResult;

        /// <summary>
        /// Initializes a new instance of the <see cref="ExistsRule{TEntity, TProperty}"/> class with a <paramref name="predicate"/>.
        /// </summary>
        /// <param name="predicate">The must predicate.</param>
        public ExistsRule(Predicate<TEntity> predicate)
        {
            _predicate = predicate ?? throw new ArgumentNullException(nameof(predicate));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExistsRule{TEntity, TProperty}"/> class with an <paramref name="exists"/> function that must return true.
        /// </summary>
        /// <param name="exists">The exists function.</param>
        public ExistsRule(Func<TEntity, Task<bool>> exists)
        {
            _exists = exists ?? throw new ArgumentNullException(nameof(exists));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExistsRule{TEntity, TProperty}"/> class with an <paramref name="exists"/> function that must return a value.
        /// </summary>
        /// <param name="exists">The exists function.</param>
        public ExistsRule(Func<TEntity, Task<object>> exists)
        {
            _existsNotNull = exists ?? throw new ArgumentNullException(nameof(exists));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExistsRule{TEntity, TProperty}"/> class with an <paramref name="agentResult"/> function that must return a successful <see cref="WebApiAgentResult"/>.
        /// </summary>
        /// <param name="agentResult">The <see cref="WebApiAgentResult"/> function.</param>
        /// <remarks>A result of <see cref="WebApiAgentResult.IsSuccess"/> implies exists, whilst a <see cref="WebApiAgentResult.StatusCode"/> of <see cref="HttpStatusCode.NotFound"/> does not.
        /// Any other status code will result in the underlying <see cref="WebApiAgentResult.Response"/> <see cref="HttpResponseMessage.EnsureSuccessStatusCode"/> being invoked resulting in an
        /// appropriate exception being thrown.</remarks>
        public ExistsRule(Func<TEntity, Task<WebApiAgentResult>> agentResult)
        {
            _agentResult = agentResult ?? throw new ArgumentNullException(nameof(agentResult));
        }

        /// <summary>
        /// Validate the property value.
        /// </summary>
        /// <param name="context">The <see cref="PropertyContext{TEntity, TProperty}"/>.</param>
        public override async Task ValidateAsync(PropertyContext<TEntity, TProperty> context)
        {
            Beef.Check.NotNull(context, nameof(context));

            if (_predicate != null)
            {
                if (!_predicate(context.Parent.Value))
                    CreateErrorMessage(context);
            }
            else if (_exists != null)
            {
                if (!await _exists(context.Parent.Value).ConfigureAwait(false))
                    CreateErrorMessage(context);
            }
            else if (_agentResult != null)
            {
                var r = await _agentResult(context.Parent.Value).ConfigureAwait(false);
                if (r == null || r.Response == null)
                    throw new InvalidOperationException("The WebApiAgentResult value is in an invalid state; the underlying Response property must not be null.");

                if (!r.IsSuccess)
                {
                    if (r.StatusCode == HttpStatusCode.NotFound)
                        CreateErrorMessage(context);
                    else
                        r.Response.EnsureSuccessStatusCode();
                }
            }
            else
            {
                if (await _existsNotNull!(context.Parent.Value).ConfigureAwait(false) == null)
                    CreateErrorMessage(context);
            }
        }

        /// <summary>
        /// Create the error message.
        /// </summary>
        private void CreateErrorMessage(PropertyContext<TEntity, TProperty> context)
        {
            context.CreateErrorMessage(ErrorText ?? ValidatorStrings.ExistsFormat);
        }
    }
}