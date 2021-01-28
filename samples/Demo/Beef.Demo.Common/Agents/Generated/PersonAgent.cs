/*
 * This file is automatically generated; any changes will be lost.
 */

#nullable enable
#pragma warning disable IDE0079, IDE0001, IDE0005, IDE0044, CA1034, CA1052, CA1056, CA1819, CA2227, CS0649

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Beef.Entities;
using Beef.WebApi;
using Newtonsoft.Json.Linq;
using Beef.Demo.Common.Entities;
using RefDataNamespace = Beef.Demo.Common.Entities;

namespace Beef.Demo.Common.Agents
{
    /// <summary>
    /// Defines the <see cref="Person"/> Web API agent.
    /// </summary>
    public partial interface IPersonAgent
    {
        /// <summary>
        /// Creates a new <see cref="Person"/>.
        /// </summary>
        /// <param name="value">The <see cref="Person"/>.</param>
        /// <param name="requestOptions">The optional <see cref="WebApiRequestOptions"/>.</param>
        /// <returns>A <see cref="WebApiAgentResult"/>.</returns>
        Task<WebApiAgentResult<Person>> CreateAsync(Person value, WebApiRequestOptions? requestOptions = null);

        /// <summary>
        /// Deletes the specified <see cref="Person"/>.
        /// </summary>
        /// <param name="id">The <see cref="Person"/> identifier.</param>
        /// <param name="requestOptions">The optional <see cref="WebApiRequestOptions"/>.</param>
        /// <returns>A <see cref="WebApiAgentResult"/>.</returns>
        Task<WebApiAgentResult> DeleteAsync(Guid id, WebApiRequestOptions? requestOptions = null);

        /// <summary>
        /// Gets the specified <see cref="Person"/>.
        /// </summary>
        /// <param name="id">The <see cref="Person"/> identifier.</param>
        /// <param name="requestOptions">The optional <see cref="WebApiRequestOptions"/>.</param>
        /// <returns>A <see cref="WebApiAgentResult"/>.</returns>
        Task<WebApiAgentResult<Person?>> GetAsync(Guid id, WebApiRequestOptions? requestOptions = null);

        /// <summary>
        /// Updates an existing <see cref="Person"/>.
        /// </summary>
        /// <param name="value">The <see cref="Person"/>.</param>
        /// <param name="id">The <see cref="Person"/> identifier.</param>
        /// <param name="requestOptions">The optional <see cref="WebApiRequestOptions"/>.</param>
        /// <returns>A <see cref="WebApiAgentResult"/>.</returns>
        Task<WebApiAgentResult<Person>> UpdateAsync(Person value, Guid id, WebApiRequestOptions? requestOptions = null);

        /// <summary>
        /// Updates an existing <see cref="Person"/>.
        /// </summary>
        /// <param name="value">The <see cref="Person"/>.</param>
        /// <param name="id">The <see cref="Person"/> identifier.</param>
        /// <param name="requestOptions">The optional <see cref="WebApiRequestOptions"/>.</param>
        /// <returns>A <see cref="WebApiAgentResult"/>.</returns>
        Task<WebApiAgentResult<Person>> UpdateWithRollbackAsync(Person value, Guid id, WebApiRequestOptions? requestOptions = null);

        /// <summary>
        /// Patches an existing <see cref="Person"/>.
        /// </summary>
        /// <param name="patchOption">The <see cref="WebApiPatchOption"/>.</param>
        /// <param name="value">The <see cref="JToken"/> that contains the patch content for the <see cref="Person"/>.</param>
        /// <param name="id">The <see cref="Person"/> identifier.</param>
        /// <param name="requestOptions">The optional <see cref="WebApiRequestOptions"/>.</param>
        /// <returns>A <see cref="WebApiAgentResult"/>.</returns>
        Task<WebApiAgentResult<Person>> PatchAsync(WebApiPatchOption patchOption, JToken value, Guid id, WebApiRequestOptions? requestOptions = null);

        /// <summary>
        /// Gets the <see cref="PersonCollectionResult"/> that contains the items that match the selection criteria.
        /// </summary>
        /// <param name="paging">The <see cref="PagingArgs"/>.</param>
        /// <param name="requestOptions">The optional <see cref="WebApiRequestOptions"/>.</param>
        /// <returns>A <see cref="WebApiAgentResult"/>.</returns>
        Task<WebApiAgentResult<PersonCollectionResult>> GetAllAsync(PagingArgs? paging = null, WebApiRequestOptions? requestOptions = null);

        /// <summary>
        /// Gets the <see cref="PersonCollectionResult"/> that contains the items that match the selection criteria.
        /// </summary>
        /// <param name="requestOptions">The optional <see cref="WebApiRequestOptions"/>.</param>
        /// <returns>A <see cref="WebApiAgentResult"/>.</returns>
        Task<WebApiAgentResult<PersonCollectionResult>> GetAll2Async(WebApiRequestOptions? requestOptions = null);

        /// <summary>
        /// Gets the <see cref="PersonCollectionResult"/> that contains the items that match the selection criteria.
        /// </summary>
        /// <param name="args">The Args (see <see cref="Common.Entities.PersonArgs"/>).</param>
        /// <param name="paging">The <see cref="PagingArgs"/>.</param>
        /// <param name="requestOptions">The optional <see cref="WebApiRequestOptions"/>.</param>
        /// <returns>A <see cref="WebApiAgentResult"/>.</returns>
        Task<WebApiAgentResult<PersonCollectionResult>> GetByArgsAsync(PersonArgs? args, PagingArgs? paging = null, WebApiRequestOptions? requestOptions = null);

        /// <summary>
        /// Gets the <see cref="PersonDetailCollectionResult"/> that contains the items that match the selection criteria.
        /// </summary>
        /// <param name="args">The Args (see <see cref="Common.Entities.PersonArgs"/>).</param>
        /// <param name="paging">The <see cref="PagingArgs"/>.</param>
        /// <param name="requestOptions">The optional <see cref="WebApiRequestOptions"/>.</param>
        /// <returns>A <see cref="WebApiAgentResult"/>.</returns>
        Task<WebApiAgentResult<PersonDetailCollectionResult>> GetDetailByArgsAsync(PersonArgs? args, PagingArgs? paging = null, WebApiRequestOptions? requestOptions = null);

        /// <summary>
        /// Merge first <see cref="Person"/> into second.
        /// </summary>
        /// <param name="fromId">The from <see cref="Person"/> identifier.</param>
        /// <param name="toId">The to <see cref="Person"/> identifier.</param>
        /// <param name="requestOptions">The optional <see cref="WebApiRequestOptions"/>.</param>
        /// <returns>A <see cref="WebApiAgentResult"/>.</returns>
        Task<WebApiAgentResult<Person>> MergeAsync(Guid fromId, Guid toId, WebApiRequestOptions? requestOptions = null);

        /// <summary>
        /// Mark <see cref="Person"/>.
        /// </summary>
        /// <param name="requestOptions">The optional <see cref="WebApiRequestOptions"/>.</param>
        /// <returns>A <see cref="WebApiAgentResult"/>.</returns>
        Task<WebApiAgentResult> MarkAsync(WebApiRequestOptions? requestOptions = null);

        /// <summary>
        /// Get <see cref="Person"/> at specified <see cref="MapCoordinates"/>.
        /// </summary>
        /// <param name="args">The Args (see <see cref="Common.Entities.MapArgs"/>).</param>
        /// <param name="requestOptions">The optional <see cref="WebApiRequestOptions"/>.</param>
        /// <returns>A <see cref="WebApiAgentResult"/>.</returns>
        Task<WebApiAgentResult<MapCoordinates>> MapAsync(MapArgs? args, WebApiRequestOptions? requestOptions = null);

        /// <summary>
        /// Get no arguments.
        /// </summary>
        /// <param name="requestOptions">The optional <see cref="WebApiRequestOptions"/>.</param>
        /// <returns>A <see cref="WebApiAgentResult"/>.</returns>
        Task<WebApiAgentResult<Person?>> GetNoArgsAsync(WebApiRequestOptions? requestOptions = null);

        /// <summary>
        /// Gets the specified <see cref="PersonDetail"/>.
        /// </summary>
        /// <param name="id">The <see cref="Person"/> identifier.</param>
        /// <param name="requestOptions">The optional <see cref="WebApiRequestOptions"/>.</param>
        /// <returns>A <see cref="WebApiAgentResult"/>.</returns>
        Task<WebApiAgentResult<PersonDetail?>> GetDetailAsync(Guid id, WebApiRequestOptions? requestOptions = null);

        /// <summary>
        /// Updates an existing <see cref="PersonDetail"/>.
        /// </summary>
        /// <param name="value">The <see cref="PersonDetail"/>.</param>
        /// <param name="id">The <see cref="Person"/> identifier.</param>
        /// <param name="requestOptions">The optional <see cref="WebApiRequestOptions"/>.</param>
        /// <returns>A <see cref="WebApiAgentResult"/>.</returns>
        Task<WebApiAgentResult<PersonDetail>> UpdateDetailAsync(PersonDetail value, Guid id, WebApiRequestOptions? requestOptions = null);

        /// <summary>
        /// Patches an existing <see cref="PersonDetail"/>.
        /// </summary>
        /// <param name="patchOption">The <see cref="WebApiPatchOption"/>.</param>
        /// <param name="value">The <see cref="JToken"/> that contains the patch content for the <see cref="PersonDetail"/>.</param>
        /// <param name="id">The <see cref="Person"/> identifier.</param>
        /// <param name="requestOptions">The optional <see cref="WebApiRequestOptions"/>.</param>
        /// <returns>A <see cref="WebApiAgentResult"/>.</returns>
        Task<WebApiAgentResult<PersonDetail>> PatchDetailAsync(WebApiPatchOption patchOption, JToken value, Guid id, WebApiRequestOptions? requestOptions = null);

        /// <summary>
        /// Actually validating the FromBody parameter generation.
        /// </summary>
        /// <param name="person">The Person (see <see cref="Common.Entities.Person"/>).</param>
        /// <param name="requestOptions">The optional <see cref="WebApiRequestOptions"/>.</param>
        /// <returns>A <see cref="WebApiAgentResult"/>.</returns>
        Task<WebApiAgentResult> AddAsync(Person person, WebApiRequestOptions? requestOptions = null);

        /// <summary>
        /// Get Null.
        /// </summary>
        /// <param name="name">The Name.</param>
        /// <param name="names">The Names.</param>
        /// <param name="requestOptions">The optional <see cref="WebApiRequestOptions"/>.</param>
        /// <returns>A <see cref="WebApiAgentResult"/>.</returns>
        Task<WebApiAgentResult<Person?>> GetNullAsync(string? name, List<string>? names, WebApiRequestOptions? requestOptions = null);

        /// <summary>
        /// Gets the <see cref="PersonCollectionResult"/> that contains the items that match the selection criteria.
        /// </summary>
        /// <param name="args">The Args (see <see cref="Common.Entities.PersonArgs"/>).</param>
        /// <param name="paging">The <see cref="PagingArgs"/>.</param>
        /// <param name="requestOptions">The optional <see cref="WebApiRequestOptions"/>.</param>
        /// <returns>A <see cref="WebApiAgentResult"/>.</returns>
        Task<WebApiAgentResult<PersonCollectionResult>> GetByArgsWithEfAsync(PersonArgs? args, PagingArgs? paging = null, WebApiRequestOptions? requestOptions = null);

        /// <summary>
        /// Throw Error.
        /// </summary>
        /// <param name="requestOptions">The optional <see cref="WebApiRequestOptions"/>.</param>
        /// <returns>A <see cref="WebApiAgentResult"/>.</returns>
        Task<WebApiAgentResult> ThrowErrorAsync(WebApiRequestOptions? requestOptions = null);

        /// <summary>
        /// Invoke Api Via Agent.
        /// </summary>
        /// <param name="id">The <see cref="Person"/> identifier.</param>
        /// <param name="requestOptions">The optional <see cref="WebApiRequestOptions"/>.</param>
        /// <returns>A <see cref="WebApiAgentResult"/>.</returns>
        Task<WebApiAgentResult<string?>> InvokeApiViaAgentAsync(Guid id, WebApiRequestOptions? requestOptions = null);

        /// <summary>
        /// Gets the specified <see cref="Person"/>.
        /// </summary>
        /// <param name="id">The <see cref="Person"/> identifier.</param>
        /// <param name="requestOptions">The optional <see cref="WebApiRequestOptions"/>.</param>
        /// <returns>A <see cref="WebApiAgentResult"/>.</returns>
        Task<WebApiAgentResult<Person?>> GetWithEfAsync(Guid id, WebApiRequestOptions? requestOptions = null);

        /// <summary>
        /// Creates a new <see cref="Person"/>.
        /// </summary>
        /// <param name="value">The <see cref="Person"/>.</param>
        /// <param name="requestOptions">The optional <see cref="WebApiRequestOptions"/>.</param>
        /// <returns>A <see cref="WebApiAgentResult"/>.</returns>
        Task<WebApiAgentResult<Person>> CreateWithEfAsync(Person value, WebApiRequestOptions? requestOptions = null);

        /// <summary>
        /// Updates an existing <see cref="Person"/>.
        /// </summary>
        /// <param name="value">The <see cref="Person"/>.</param>
        /// <param name="id">The <see cref="Person"/> identifier.</param>
        /// <param name="requestOptions">The optional <see cref="WebApiRequestOptions"/>.</param>
        /// <returns>A <see cref="WebApiAgentResult"/>.</returns>
        Task<WebApiAgentResult<Person>> UpdateWithEfAsync(Person value, Guid id, WebApiRequestOptions? requestOptions = null);

        /// <summary>
        /// Deletes the specified <see cref="Person"/>.
        /// </summary>
        /// <param name="id">The <see cref="Person"/> identifier.</param>
        /// <param name="requestOptions">The optional <see cref="WebApiRequestOptions"/>.</param>
        /// <returns>A <see cref="WebApiAgentResult"/>.</returns>
        Task<WebApiAgentResult> DeleteWithEfAsync(Guid id, WebApiRequestOptions? requestOptions = null);

        /// <summary>
        /// Patches an existing <see cref="Person"/>.
        /// </summary>
        /// <param name="patchOption">The <see cref="WebApiPatchOption"/>.</param>
        /// <param name="value">The <see cref="JToken"/> that contains the patch content for the <see cref="Person"/>.</param>
        /// <param name="id">The <see cref="Person"/> identifier.</param>
        /// <param name="requestOptions">The optional <see cref="WebApiRequestOptions"/>.</param>
        /// <returns>A <see cref="WebApiAgentResult"/>.</returns>
        Task<WebApiAgentResult<Person>> PatchWithEfAsync(WebApiPatchOption patchOption, JToken value, Guid id, WebApiRequestOptions? requestOptions = null);
    }

    /// <summary>
    /// Provides the <see cref="Person"/> Web API agent.
    /// </summary>
    public partial class PersonAgent : WebApiAgentBase, IPersonAgent
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PersonAgent"/> class.
        /// </summary>
        /// <param name="args">The <see cref="IDemoWebApiAgentArgs"/>.</param>
        public PersonAgent(IDemoWebApiAgentArgs args) : base(args) { }

        /// <summary>
        /// Creates a new <see cref="Person"/>.
        /// </summary>
        /// <param name="value">The <see cref="Person"/>.</param>
        /// <param name="requestOptions">The optional <see cref="WebApiRequestOptions"/>.</param>
        /// <returns>A <see cref="WebApiAgentResult"/>.</returns>
        public Task<WebApiAgentResult<Person>> CreateAsync(Person value, WebApiRequestOptions? requestOptions = null) =>
            PostAsync<Person>("api/v1/persons", Beef.Check.NotNull(value, nameof(value)), requestOptions: requestOptions,
                args: Array.Empty<WebApiArg>());

        /// <summary>
        /// Deletes the specified <see cref="Person"/>.
        /// </summary>
        /// <param name="id">The <see cref="Person"/> identifier.</param>
        /// <param name="requestOptions">The optional <see cref="WebApiRequestOptions"/>.</param>
        /// <returns>A <see cref="WebApiAgentResult"/>.</returns>
        public Task<WebApiAgentResult> DeleteAsync(Guid id, WebApiRequestOptions? requestOptions = null) =>
            DeleteAsync("api/v1/persons/{id}", requestOptions: requestOptions,
                args: new WebApiArg[] { new WebApiArg<Guid>("id", id) });

        /// <summary>
        /// Gets the specified <see cref="Person"/>.
        /// </summary>
        /// <param name="id">The <see cref="Person"/> identifier.</param>
        /// <param name="requestOptions">The optional <see cref="WebApiRequestOptions"/>.</param>
        /// <returns>A <see cref="WebApiAgentResult"/>.</returns>
        public Task<WebApiAgentResult<Person?>> GetAsync(Guid id, WebApiRequestOptions? requestOptions = null) =>
            GetAsync<Person?>("api/v1/persons/{id}", requestOptions: requestOptions,
                args: new WebApiArg[] { new WebApiArg<Guid>("id", id) });

        /// <summary>
        /// Updates an existing <see cref="Person"/>.
        /// </summary>
        /// <param name="value">The <see cref="Person"/>.</param>
        /// <param name="id">The <see cref="Person"/> identifier.</param>
        /// <param name="requestOptions">The optional <see cref="WebApiRequestOptions"/>.</param>
        /// <returns>A <see cref="WebApiAgentResult"/>.</returns>
        public Task<WebApiAgentResult<Person>> UpdateAsync(Person value, Guid id, WebApiRequestOptions? requestOptions = null) =>
            PutAsync<Person>("api/v1/persons/{id}", Beef.Check.NotNull(value, nameof(value)), requestOptions: requestOptions,
                args: new WebApiArg[] { new WebApiArg<Guid>("id", id) });

        /// <summary>
        /// Updates an existing <see cref="Person"/>.
        /// </summary>
        /// <param name="value">The <see cref="Person"/>.</param>
        /// <param name="id">The <see cref="Person"/> identifier.</param>
        /// <param name="requestOptions">The optional <see cref="WebApiRequestOptions"/>.</param>
        /// <returns>A <see cref="WebApiAgentResult"/>.</returns>
        public Task<WebApiAgentResult<Person>> UpdateWithRollbackAsync(Person value, Guid id, WebApiRequestOptions? requestOptions = null) =>
            PutAsync<Person>("api/v1/persons/withRollback/{id}", Beef.Check.NotNull(value, nameof(value)), requestOptions: requestOptions,
                args: new WebApiArg[] { new WebApiArg<Guid>("id", id) });

        /// <summary>
        /// Patches an existing <see cref="Person"/>.
        /// </summary>
        /// <param name="patchOption">The <see cref="WebApiPatchOption"/>.</param>
        /// <param name="value">The <see cref="JToken"/> that contains the patch content for the <see cref="Person"/>.</param>
        /// <param name="id">The <see cref="Person"/> identifier.</param>
        /// <param name="requestOptions">The optional <see cref="WebApiRequestOptions"/>.</param>
        /// <returns>A <see cref="WebApiAgentResult"/>.</returns>
        public Task<WebApiAgentResult<Person>> PatchAsync(WebApiPatchOption patchOption, JToken value, Guid id, WebApiRequestOptions? requestOptions = null) =>
            PatchAsync<Person>("api/v1/persons/{id}", patchOption, Beef.Check.NotNull(value, nameof(value)), requestOptions: requestOptions,
                args: new WebApiArg[] { new WebApiArg<Guid>("id", id) });

        /// <summary>
        /// Gets the <see cref="PersonCollectionResult"/> that contains the items that match the selection criteria.
        /// </summary>
        /// <param name="paging">The <see cref="PagingArgs"/>.</param>
        /// <param name="requestOptions">The optional <see cref="WebApiRequestOptions"/>.</param>
        /// <returns>A <see cref="WebApiAgentResult"/>.</returns>
        public Task<WebApiAgentResult<PersonCollectionResult>> GetAllAsync(PagingArgs? paging = null, WebApiRequestOptions? requestOptions = null) =>
            GetCollectionResultAsync<PersonCollectionResult, PersonCollection, Person>("api/v1/persons/all", requestOptions: requestOptions,
                args: new WebApiArg[] { new WebApiPagingArgsArg("paging", paging) });

        /// <summary>
        /// Gets the <see cref="PersonCollectionResult"/> that contains the items that match the selection criteria.
        /// </summary>
        /// <param name="requestOptions">The optional <see cref="WebApiRequestOptions"/>.</param>
        /// <returns>A <see cref="WebApiAgentResult"/>.</returns>
        public Task<WebApiAgentResult<PersonCollectionResult>> GetAll2Async(WebApiRequestOptions? requestOptions = null) =>
            GetCollectionResultAsync<PersonCollectionResult, PersonCollection, Person>("api/v1/persons/allnopaging", requestOptions: requestOptions,
                args: Array.Empty<WebApiArg>());

        /// <summary>
        /// Gets the <see cref="PersonCollectionResult"/> that contains the items that match the selection criteria.
        /// </summary>
        /// <param name="args">The Args (see <see cref="Common.Entities.PersonArgs"/>).</param>
        /// <param name="paging">The <see cref="PagingArgs"/>.</param>
        /// <param name="requestOptions">The optional <see cref="WebApiRequestOptions"/>.</param>
        /// <returns>A <see cref="WebApiAgentResult"/>.</returns>
        public Task<WebApiAgentResult<PersonCollectionResult>> GetByArgsAsync(PersonArgs? args, PagingArgs? paging = null, WebApiRequestOptions? requestOptions = null) =>
            GetCollectionResultAsync<PersonCollectionResult, PersonCollection, Person>("api/v1/persons", requestOptions: requestOptions,
                args: new WebApiArg[] { new WebApiArg<PersonArgs?>("args", args, WebApiArgType.FromUriUseProperties), new WebApiPagingArgsArg("paging", paging) });

        /// <summary>
        /// Gets the <see cref="PersonDetailCollectionResult"/> that contains the items that match the selection criteria.
        /// </summary>
        /// <param name="args">The Args (see <see cref="Common.Entities.PersonArgs"/>).</param>
        /// <param name="paging">The <see cref="PagingArgs"/>.</param>
        /// <param name="requestOptions">The optional <see cref="WebApiRequestOptions"/>.</param>
        /// <returns>A <see cref="WebApiAgentResult"/>.</returns>
        public Task<WebApiAgentResult<PersonDetailCollectionResult>> GetDetailByArgsAsync(PersonArgs? args, PagingArgs? paging = null, WebApiRequestOptions? requestOptions = null) =>
            GetCollectionResultAsync<PersonDetailCollectionResult, PersonDetailCollection, PersonDetail>("api/v1/persons/argsdetail", requestOptions: requestOptions,
                args: new WebApiArg[] { new WebApiArg<PersonArgs?>("args", args, WebApiArgType.FromUriUseProperties), new WebApiPagingArgsArg("paging", paging) });

        /// <summary>
        /// Merge first <see cref="Person"/> into second.
        /// </summary>
        /// <param name="fromId">The from <see cref="Person"/> identifier.</param>
        /// <param name="toId">The to <see cref="Person"/> identifier.</param>
        /// <param name="requestOptions">The optional <see cref="WebApiRequestOptions"/>.</param>
        /// <returns>A <see cref="WebApiAgentResult"/>.</returns>
        public Task<WebApiAgentResult<Person>> MergeAsync(Guid fromId, Guid toId, WebApiRequestOptions? requestOptions = null) =>
            PostAsync<Person>("api/v1/persons/merge", requestOptions: requestOptions,
                args: new WebApiArg[] { new WebApiArg<Guid>("fromId", fromId), new WebApiArg<Guid>("toId", toId) });

        /// <summary>
        /// Mark <see cref="Person"/>.
        /// </summary>
        /// <param name="requestOptions">The optional <see cref="WebApiRequestOptions"/>.</param>
        /// <returns>A <see cref="WebApiAgentResult"/>.</returns>
        public Task<WebApiAgentResult> MarkAsync(WebApiRequestOptions? requestOptions = null) =>
            PostAsync("api/v1/persons/mark", requestOptions: requestOptions,
                args: Array.Empty<WebApiArg>());

        /// <summary>
        /// Get <see cref="Person"/> at specified <see cref="MapCoordinates"/>.
        /// </summary>
        /// <param name="args">The Args (see <see cref="Common.Entities.MapArgs"/>).</param>
        /// <param name="requestOptions">The optional <see cref="WebApiRequestOptions"/>.</param>
        /// <returns>A <see cref="WebApiAgentResult"/>.</returns>
        public Task<WebApiAgentResult<MapCoordinates>> MapAsync(MapArgs? args, WebApiRequestOptions? requestOptions = null) =>
            PostAsync<MapCoordinates>("api/v1/persons/map", requestOptions: requestOptions,
                args: new WebApiArg[] { new WebApiArg<MapArgs?>("args", args, WebApiArgType.FromUriUseProperties) });

        /// <summary>
        /// Get no arguments.
        /// </summary>
        /// <param name="requestOptions">The optional <see cref="WebApiRequestOptions"/>.</param>
        /// <returns>A <see cref="WebApiAgentResult"/>.</returns>
        public Task<WebApiAgentResult<Person?>> GetNoArgsAsync(WebApiRequestOptions? requestOptions = null) =>
            GetAsync<Person?>("api/v1/persons/noargsforme", requestOptions: requestOptions,
                args: Array.Empty<WebApiArg>());

        /// <summary>
        /// Gets the specified <see cref="PersonDetail"/>.
        /// </summary>
        /// <param name="id">The <see cref="Person"/> identifier.</param>
        /// <param name="requestOptions">The optional <see cref="WebApiRequestOptions"/>.</param>
        /// <returns>A <see cref="WebApiAgentResult"/>.</returns>
        public Task<WebApiAgentResult<PersonDetail?>> GetDetailAsync(Guid id, WebApiRequestOptions? requestOptions = null) =>
            GetAsync<PersonDetail?>("api/v1/persons/{id}/detail", requestOptions: requestOptions,
                args: new WebApiArg[] { new WebApiArg<Guid>("id", id) });

        /// <summary>
        /// Updates an existing <see cref="PersonDetail"/>.
        /// </summary>
        /// <param name="value">The <see cref="PersonDetail"/>.</param>
        /// <param name="id">The <see cref="Person"/> identifier.</param>
        /// <param name="requestOptions">The optional <see cref="WebApiRequestOptions"/>.</param>
        /// <returns>A <see cref="WebApiAgentResult"/>.</returns>
        public Task<WebApiAgentResult<PersonDetail>> UpdateDetailAsync(PersonDetail value, Guid id, WebApiRequestOptions? requestOptions = null) =>
            PutAsync<PersonDetail>("api/v1/persons/{id}/detail", Beef.Check.NotNull(value, nameof(value)), requestOptions: requestOptions,
                args: new WebApiArg[] { new WebApiArg<Guid>("id", id) });

        /// <summary>
        /// Patches an existing <see cref="PersonDetail"/>.
        /// </summary>
        /// <param name="patchOption">The <see cref="WebApiPatchOption"/>.</param>
        /// <param name="value">The <see cref="JToken"/> that contains the patch content for the <see cref="PersonDetail"/>.</param>
        /// <param name="id">The <see cref="Person"/> identifier.</param>
        /// <param name="requestOptions">The optional <see cref="WebApiRequestOptions"/>.</param>
        /// <returns>A <see cref="WebApiAgentResult"/>.</returns>
        public Task<WebApiAgentResult<PersonDetail>> PatchDetailAsync(WebApiPatchOption patchOption, JToken value, Guid id, WebApiRequestOptions? requestOptions = null) =>
            PatchAsync<PersonDetail>("api/v1/persons/{id}/detail", patchOption, Beef.Check.NotNull(value, nameof(value)), requestOptions: requestOptions,
                args: new WebApiArg[] { new WebApiArg<Guid>("id", id) });

        /// <summary>
        /// Actually validating the FromBody parameter generation.
        /// </summary>
        /// <param name="person">The Person (see <see cref="Common.Entities.Person"/>).</param>
        /// <param name="requestOptions">The optional <see cref="WebApiRequestOptions"/>.</param>
        /// <returns>A <see cref="WebApiAgentResult"/>.</returns>
        public Task<WebApiAgentResult> AddAsync(Person person, WebApiRequestOptions? requestOptions = null) =>
            PostAsync("api/v1/persons/fromBody", requestOptions: requestOptions,
                args: new WebApiArg[] { new WebApiArg<Person>("person", person, WebApiArgType.FromBody) });

        /// <summary>
        /// Get Null.
        /// </summary>
        /// <param name="name">The Name.</param>
        /// <param name="names">The Names.</param>
        /// <param name="requestOptions">The optional <see cref="WebApiRequestOptions"/>.</param>
        /// <returns>A <see cref="WebApiAgentResult"/>.</returns>
        public Task<WebApiAgentResult<Person?>> GetNullAsync(string? name, List<string>? names, WebApiRequestOptions? requestOptions = null) =>
            GetAsync<Person?>("api/v1/persons/null", requestOptions: requestOptions,
                args: new WebApiArg[] { new WebApiArg<string?>("name", name), new WebApiArg<List<string>?>("names", names) });

        /// <summary>
        /// Gets the <see cref="PersonCollectionResult"/> that contains the items that match the selection criteria.
        /// </summary>
        /// <param name="args">The Args (see <see cref="Common.Entities.PersonArgs"/>).</param>
        /// <param name="paging">The <see cref="PagingArgs"/>.</param>
        /// <param name="requestOptions">The optional <see cref="WebApiRequestOptions"/>.</param>
        /// <returns>A <see cref="WebApiAgentResult"/>.</returns>
        public Task<WebApiAgentResult<PersonCollectionResult>> GetByArgsWithEfAsync(PersonArgs? args, PagingArgs? paging = null, WebApiRequestOptions? requestOptions = null) =>
            GetCollectionResultAsync<PersonCollectionResult, PersonCollection, Person>("api/v1/persons/args", requestOptions: requestOptions,
                args: new WebApiArg[] { new WebApiArg<PersonArgs?>("args", args, WebApiArgType.FromUriUseProperties), new WebApiPagingArgsArg("paging", paging) });

        /// <summary>
        /// Throw Error.
        /// </summary>
        /// <param name="requestOptions">The optional <see cref="WebApiRequestOptions"/>.</param>
        /// <returns>A <see cref="WebApiAgentResult"/>.</returns>
        public Task<WebApiAgentResult> ThrowErrorAsync(WebApiRequestOptions? requestOptions = null) =>
            PostAsync("api/v1/persons/error", requestOptions: requestOptions,
                args: Array.Empty<WebApiArg>());

        /// <summary>
        /// Invoke Api Via Agent.
        /// </summary>
        /// <param name="id">The <see cref="Person"/> identifier.</param>
        /// <param name="requestOptions">The optional <see cref="WebApiRequestOptions"/>.</param>
        /// <returns>A <see cref="WebApiAgentResult"/>.</returns>
        public Task<WebApiAgentResult<string?>> InvokeApiViaAgentAsync(Guid id, WebApiRequestOptions? requestOptions = null) =>
            PostAsync<string?>("api/v1/persons/invokeApi", requestOptions: requestOptions,
                args: new WebApiArg[] { new WebApiArg<Guid>("id", id) });

        /// <summary>
        /// Gets the specified <see cref="Person"/>.
        /// </summary>
        /// <param name="id">The <see cref="Person"/> identifier.</param>
        /// <param name="requestOptions">The optional <see cref="WebApiRequestOptions"/>.</param>
        /// <returns>A <see cref="WebApiAgentResult"/>.</returns>
        public Task<WebApiAgentResult<Person?>> GetWithEfAsync(Guid id, WebApiRequestOptions? requestOptions = null) =>
            GetAsync<Person?>("api/v1/persons/ef/{id}", requestOptions: requestOptions,
                args: new WebApiArg[] { new WebApiArg<Guid>("id", id) });

        /// <summary>
        /// Creates a new <see cref="Person"/>.
        /// </summary>
        /// <param name="value">The <see cref="Person"/>.</param>
        /// <param name="requestOptions">The optional <see cref="WebApiRequestOptions"/>.</param>
        /// <returns>A <see cref="WebApiAgentResult"/>.</returns>
        public Task<WebApiAgentResult<Person>> CreateWithEfAsync(Person value, WebApiRequestOptions? requestOptions = null) =>
            PostAsync<Person>("api/v1/persons/ef", Beef.Check.NotNull(value, nameof(value)), requestOptions: requestOptions,
                args: Array.Empty<WebApiArg>());

        /// <summary>
        /// Updates an existing <see cref="Person"/>.
        /// </summary>
        /// <param name="value">The <see cref="Person"/>.</param>
        /// <param name="id">The <see cref="Person"/> identifier.</param>
        /// <param name="requestOptions">The optional <see cref="WebApiRequestOptions"/>.</param>
        /// <returns>A <see cref="WebApiAgentResult"/>.</returns>
        public Task<WebApiAgentResult<Person>> UpdateWithEfAsync(Person value, Guid id, WebApiRequestOptions? requestOptions = null) =>
            PutAsync<Person>("api/v1/persons/ef/{id}", Beef.Check.NotNull(value, nameof(value)), requestOptions: requestOptions,
                args: new WebApiArg[] { new WebApiArg<Guid>("id", id) });

        /// <summary>
        /// Deletes the specified <see cref="Person"/>.
        /// </summary>
        /// <param name="id">The <see cref="Person"/> identifier.</param>
        /// <param name="requestOptions">The optional <see cref="WebApiRequestOptions"/>.</param>
        /// <returns>A <see cref="WebApiAgentResult"/>.</returns>
        public Task<WebApiAgentResult> DeleteWithEfAsync(Guid id, WebApiRequestOptions? requestOptions = null) =>
            DeleteAsync("api/v1/persons/ef/{id}", requestOptions: requestOptions,
                args: new WebApiArg[] { new WebApiArg<Guid>("id", id) });

        /// <summary>
        /// Patches an existing <see cref="Person"/>.
        /// </summary>
        /// <param name="patchOption">The <see cref="WebApiPatchOption"/>.</param>
        /// <param name="value">The <see cref="JToken"/> that contains the patch content for the <see cref="Person"/>.</param>
        /// <param name="id">The <see cref="Person"/> identifier.</param>
        /// <param name="requestOptions">The optional <see cref="WebApiRequestOptions"/>.</param>
        /// <returns>A <see cref="WebApiAgentResult"/>.</returns>
        public Task<WebApiAgentResult<Person>> PatchWithEfAsync(WebApiPatchOption patchOption, JToken value, Guid id, WebApiRequestOptions? requestOptions = null) =>
            PatchAsync<Person>("api/v1/persons/ef/{id}", patchOption, Beef.Check.NotNull(value, nameof(value)), requestOptions: requestOptions,
                args: new WebApiArg[] { new WebApiArg<Guid>("id", id) });
    }
}

#pragma warning restore IDE0079, IDE0001, IDE0005, IDE0044, CA1034, CA1052, CA1056, CA1819, CA2227, CS0649
#nullable restore