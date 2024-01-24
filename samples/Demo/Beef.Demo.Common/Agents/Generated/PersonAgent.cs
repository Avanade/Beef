/*
 * This file is automatically generated; any changes will be lost.
 */

#nullable enable
#pragma warning disable

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using CoreEx.Configuration;
using CoreEx.Entities;
using CoreEx.Http;
using CoreEx.Json;
using Microsoft.Extensions.Logging;
using Beef.Demo.Common.Entities;
using RefDataNamespace = Beef.Demo.Common.Entities;

namespace Beef.Demo.Common.Agents
{
    /// <summary>
    /// Provides the <see cref="Person"/> HTTP agent.
    /// </summary>
    public partial class PersonAgent : TypedHttpClientBase<PersonAgent>, IPersonAgent
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PersonAgent"/> class.
        /// </summary>
        /// <param name="client">The underlying <see cref="HttpClient"/>.</param>
        /// <param name="jsonSerializer">The <see cref="IJsonSerializer"/>.</param>
        /// <param name="executionContext">The <see cref="CoreEx.ExecutionContext"/>.</param>
        /// <param name="settings">The <see cref="SettingsBase"/>.</param>
        /// <param name="logger">The <see cref="ILogger"/>.</param>
        public PersonAgent(HttpClient client, IJsonSerializer jsonSerializer, CoreEx.ExecutionContext executionContext, SettingsBase settings, ILogger<PersonAgent> logger) 
            : base(client, jsonSerializer, executionContext, settings, logger) { }

        /// <inheritdoc/>
        public Task<HttpResult<Person>> CreateAsync(Person value, HttpRequestOptions? requestOptions = null, CancellationToken cancellationToken = default)
            => PostAsync<Person, Person>("api/v1/persons", value, requestOptions: requestOptions, cancellationToken: cancellationToken);

        /// <inheritdoc/>
        public Task<HttpResult> DeleteAsync(Guid id, HttpRequestOptions? requestOptions = null, CancellationToken cancellationToken = default)
            => DeleteAsync("api/v1/persons/{id}", requestOptions: requestOptions, args: HttpArgs.Create(new HttpArg<Guid>("id", id)), cancellationToken: cancellationToken);

        /// <inheritdoc/>
        public Task<HttpResult<Person?>> GetAsync(Guid id, HttpRequestOptions? requestOptions = null, CancellationToken cancellationToken = default)
            => GetAsync<Person?>("api/v1/persons/{id}", requestOptions: requestOptions, args: HttpArgs.Create(new HttpArg<Guid>("id", id)), cancellationToken: cancellationToken);

        /// <inheritdoc/>
        public Task<HttpResult<Person?>> GetExAsync(Guid id, HttpRequestOptions? requestOptions = null, CancellationToken cancellationToken = default)
            => GetAsync<Person?>("api/v1/persons/ex/{id}", requestOptions: requestOptions, args: HttpArgs.Create(new HttpArg<Guid>("id", id)), cancellationToken: cancellationToken);

        /// <inheritdoc/>
        public Task<HttpResult<Person>> UpdateAsync(Person value, Guid id, HttpRequestOptions? requestOptions = null, CancellationToken cancellationToken = default)
            => PutAsync<Person, Person>("api/v1/persons/{id}", value, requestOptions: requestOptions, args: HttpArgs.Create(new HttpArg<Guid>("id", id)), cancellationToken: cancellationToken);

        /// <inheritdoc/>
        public Task<HttpResult<Person>> UpdateWithRollbackAsync(Person value, Guid id, HttpRequestOptions? requestOptions = null, CancellationToken cancellationToken = default)
            => PutAsync<Person, Person>("api/v1/persons/withRollback/{id}", value, requestOptions: requestOptions, args: HttpArgs.Create(new HttpArg<Guid>("id", id)), cancellationToken: cancellationToken);

        /// <inheritdoc/>
        public Task<HttpResult<Person>> PatchAsync(HttpPatchOption patchOption, string value, Guid id, HttpRequestOptions? requestOptions = null, CancellationToken cancellationToken = default)
            => PatchAsync<Person>("api/v1/persons/{id}", patchOption, value, requestOptions: requestOptions, args: HttpArgs.Create(new HttpArg<Guid>("id", id)), cancellationToken: cancellationToken);

        /// <inheritdoc/>
        public Task<HttpResult<PersonCollectionResult>> GetAllAsync(PagingArgs? paging = null, HttpRequestOptions? requestOptions = null, CancellationToken cancellationToken = default)
            => GetAsync<PersonCollectionResult>("api/v1/persons/all", requestOptions: requestOptions.IncludePaging(paging), args: HttpArgs.Create(), cancellationToken: cancellationToken);

        /// <inheritdoc/>
        public Task<HttpResult<PersonCollectionResult>> GetAll2Async(HttpRequestOptions? requestOptions = null, CancellationToken cancellationToken = default)
            => GetAsync<PersonCollectionResult>("api/v1/persons/allnopaging", requestOptions: requestOptions, cancellationToken: cancellationToken);

        /// <inheritdoc/>
        public Task<HttpResult<PersonCollectionResult>> GetByArgsAsync(PersonArgs? args, PagingArgs? paging = null, HttpRequestOptions? requestOptions = null, CancellationToken cancellationToken = default)
            => GetAsync<PersonCollectionResult>("api/v1/persons", requestOptions: requestOptions.IncludePaging(paging), args: HttpArgs.Create(new HttpArg<PersonArgs?>("args", args, HttpArgType.FromUriUseProperties)), cancellationToken: cancellationToken);

        /// <inheritdoc/>
        public Task<HttpResult<PersonDetailCollectionResult>> GetDetailByArgsAsync(PersonArgs? args, PagingArgs? paging = null, HttpRequestOptions? requestOptions = null, CancellationToken cancellationToken = default)
            => GetAsync<PersonDetailCollectionResult>("api/v1/persons/argsdetail", requestOptions: requestOptions.IncludePaging(paging), args: HttpArgs.Create(new HttpArg<PersonArgs?>("args", args, HttpArgType.FromUriUseProperties)), cancellationToken: cancellationToken);

        /// <inheritdoc/>
        public Task<HttpResult<Person>> MergeAsync(Guid fromId, Guid toId, HttpRequestOptions? requestOptions = null, CancellationToken cancellationToken = default)
            => PostAsync<Person>("api/v1/persons/merge", requestOptions: requestOptions, args: HttpArgs.Create(new HttpArg<Guid>("fromId", fromId), new HttpArg<Guid>("toId", toId)), cancellationToken: cancellationToken);

        /// <inheritdoc/>
        public Task<HttpResult> MarkAsync(HttpRequestOptions? requestOptions = null, CancellationToken cancellationToken = default)
            => PostAsync("api/v1/persons/mark", requestOptions: requestOptions, cancellationToken: cancellationToken);

        /// <inheritdoc/>
        public Task<HttpResult<MapCoordinates>> MapAsync(MapArgs? args, HttpRequestOptions? requestOptions = null, CancellationToken cancellationToken = default)
            => PostAsync<MapCoordinates>("api/v1/persons/map", requestOptions: requestOptions, args: HttpArgs.Create(new HttpArg<MapArgs?>("args", args, HttpArgType.FromUriUseProperties)), cancellationToken: cancellationToken);

        /// <inheritdoc/>
        public Task<HttpResult<Person?>> GetNoArgsAsync(HttpRequestOptions? requestOptions = null, CancellationToken cancellationToken = default)
            => GetAsync<Person?>("api/v1/persons/noargsforme", requestOptions: requestOptions, cancellationToken: cancellationToken);

        /// <inheritdoc/>
        public Task<HttpResult<PersonDetail?>> GetDetailAsync(Guid id, HttpRequestOptions? requestOptions = null, CancellationToken cancellationToken = default)
            => GetAsync<PersonDetail?>("api/v1/persons/{id}/detail", requestOptions: requestOptions, args: HttpArgs.Create(new HttpArg<Guid>("id", id)), cancellationToken: cancellationToken);

        /// <inheritdoc/>
        public Task<HttpResult<PersonDetail>> UpdateDetailAsync(PersonDetail value, Guid id, HttpRequestOptions? requestOptions = null, CancellationToken cancellationToken = default)
            => PutAsync<PersonDetail, PersonDetail>("api/v1/persons/{id}/detail", value, requestOptions: requestOptions, args: HttpArgs.Create(new HttpArg<Guid>("id", id)), cancellationToken: cancellationToken);

        /// <inheritdoc/>
        public Task<HttpResult<PersonDetail>> PatchDetailAsync(HttpPatchOption patchOption, string value, Guid id, HttpRequestOptions? requestOptions = null, CancellationToken cancellationToken = default)
            => PatchAsync<PersonDetail>("api/v1/persons/{id}/detail", patchOption, value, requestOptions: requestOptions, args: HttpArgs.Create(new HttpArg<Guid>("id", id)), cancellationToken: cancellationToken);

        /// <inheritdoc/>
        public Task<HttpResult> AddAsync(Person person, HttpRequestOptions? requestOptions = null, CancellationToken cancellationToken = default)
            => PostAsync("api/v1/persons/fromBody", requestOptions: requestOptions, args: HttpArgs.Create(new HttpArg<Person>("person", person, HttpArgType.FromBody)), cancellationToken: cancellationToken);

        /// <inheritdoc/>
        public Task<HttpResult> CustomManagerOnlyAsync(HttpRequestOptions? requestOptions = null, CancellationToken cancellationToken = default)
            => PostAsync("api/v1/persons/cmo", requestOptions: requestOptions, cancellationToken: cancellationToken);

        /// <inheritdoc/>
        public Task<HttpResult<Person?>> GetNullAsync(string? name, List<string>? names, HttpRequestOptions? requestOptions = null, CancellationToken cancellationToken = default)
            => GetAsync<Person?>("api/v1/persons/null", requestOptions: requestOptions, args: HttpArgs.Create(new HttpArg<string?>("name", name), new HttpArg<List<string>?>("names", names)), cancellationToken: cancellationToken);

        /// <inheritdoc/>
        public Task<HttpResult<Person>> EventPublishNoSendAsync(Person value, HttpRequestOptions? requestOptions = null, CancellationToken cancellationToken = default)
            => PutAsync<Person, Person>("api/v1/persons/publishnosend", value, requestOptions: requestOptions, cancellationToken: cancellationToken);

        /// <inheritdoc/>
        public Task<HttpResult<PersonCollectionResult>> GetByArgsWithEfAsync(PersonArgs? args, PagingArgs? paging = null, HttpRequestOptions? requestOptions = null, CancellationToken cancellationToken = default)
            => GetAsync<PersonCollectionResult>("api/v1/persons/args", requestOptions: requestOptions.IncludePaging(paging), args: HttpArgs.Create(new HttpArg<PersonArgs?>("args", args, HttpArgType.FromUriUseProperties)), cancellationToken: cancellationToken);

        /// <inheritdoc/>
        public Task<HttpResult> ThrowErrorAsync(HttpRequestOptions? requestOptions = null, CancellationToken cancellationToken = default)
            => PostAsync("api/v1/persons/error", requestOptions: requestOptions, cancellationToken: cancellationToken);

        /// <inheritdoc/>
        public Task<HttpResult<string?>> InvokeApiViaAgentAsync(Guid id, HttpRequestOptions? requestOptions = null, CancellationToken cancellationToken = default)
            => PostAsync<string?>("api/v1/persons/invokeApi", requestOptions: requestOptions, args: HttpArgs.Create(new HttpArg<Guid>("id", id)), cancellationToken: cancellationToken);

        /// <inheritdoc/>
        public Task<HttpResult> ParamCollAsync(AddressCollection? addresses, HttpRequestOptions? requestOptions = null, CancellationToken cancellationToken = default)
            => PostAsync("api/v1/persons/paramcoll", requestOptions: requestOptions, args: HttpArgs.Create(new HttpArg<AddressCollection?>("addresses", addresses, HttpArgType.FromBody)), cancellationToken: cancellationToken);

        /// <inheritdoc/>
        public Task<HttpResult<Person?>> GetWithEfAsync(Guid id, HttpRequestOptions? requestOptions = null, CancellationToken cancellationToken = default)
            => GetAsync<Person?>("api/v1/persons/ef/{id}", requestOptions: requestOptions, args: HttpArgs.Create(new HttpArg<Guid>("id", id)), cancellationToken: cancellationToken);

        /// <inheritdoc/>
        public Task<HttpResult<Person>> CreateWithEfAsync(Person value, HttpRequestOptions? requestOptions = null, CancellationToken cancellationToken = default)
            => PostAsync<Person, Person>("api/v1/persons/ef", value, requestOptions: requestOptions, cancellationToken: cancellationToken);

        /// <inheritdoc/>
        public Task<HttpResult<Person>> UpdateWithEfAsync(Person value, Guid id, HttpRequestOptions? requestOptions = null, CancellationToken cancellationToken = default)
            => PutAsync<Person, Person>("api/v1/persons/ef/{id}", value, requestOptions: requestOptions, args: HttpArgs.Create(new HttpArg<Guid>("id", id)), cancellationToken: cancellationToken);

        /// <inheritdoc/>
        public Task<HttpResult> DeleteWithEfAsync(Guid id, HttpRequestOptions? requestOptions = null, CancellationToken cancellationToken = default)
            => DeleteAsync("api/v1/persons/ef/{id}", requestOptions: requestOptions, args: HttpArgs.Create(new HttpArg<Guid>("id", id)), cancellationToken: cancellationToken);

        /// <inheritdoc/>
        public Task<HttpResult<Person>> PatchWithEfAsync(HttpPatchOption patchOption, string value, Guid id, HttpRequestOptions? requestOptions = null, CancellationToken cancellationToken = default)
            => PatchAsync<Person>("api/v1/persons/ef/{id}", patchOption, value, requestOptions: requestOptions, args: HttpArgs.Create(new HttpArg<Guid>("id", id)), cancellationToken: cancellationToken);

        /// <inheritdoc/>
        public Task<HttpResult> GetDocumentationAsync(Guid id, HttpRequestOptions? requestOptions = null, CancellationToken cancellationToken = default)
            => GetAsync("api/v1/persons/{id}/documentation", requestOptions: requestOptions, args: HttpArgs.Create(new HttpArg<Guid>("id", id)), cancellationToken: cancellationToken);
    }
}

#pragma warning restore
#nullable restore