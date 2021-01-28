/*
 * This file is automatically generated; any changes will be lost.
 */

#nullable enable
#pragma warning disable IDE0079, IDE0001, IDE0005, IDE0044, CA1034, CA1052, CA1056, CA1819, CA2227, CS0649

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Beef.Entities;
using Beef.Grpc;
using Beef.WebApi;
using Beef.Demo.Common.Entities;
using proto = Beef.Demo.Common.Grpc.Proto;
using RefDataNamespace = Beef.Demo.Common.Entities;

namespace Beef.Demo.Common.Grpc
{
    /// <summary>
    /// Defines the <see cref="Robot"/> gRPC agent.
    /// </summary>
    public partial interface IRobotAgent
    {
        /// <summary>
        /// Gets the specified <see cref="Robot"/>.
        /// </summary>
        /// <param name="id">The <see cref="Robot"/> identifier.</param>
        /// <param name="requestOptions">The optional <see cref="GrpcRequestOptions"/>.</param>
        /// <returns>A <see cref="GrpcAgentResult"/>.</returns>
        Task<GrpcAgentResult<Robot>> GetAsync(Guid id, GrpcRequestOptions? requestOptions = null);

        /// <summary>
        /// Creates a new <see cref="Robot"/>.
        /// </summary>
        /// <param name="value">The <see cref="Robot"/>.</param>
        /// <param name="requestOptions">The optional <see cref="GrpcRequestOptions"/>.</param>
        /// <returns>A <see cref="GrpcAgentResult"/>.</returns>
        Task<GrpcAgentResult<Robot>> CreateAsync(Robot value, GrpcRequestOptions? requestOptions = null);

        /// <summary>
        /// Updates an existing <see cref="Robot"/>.
        /// </summary>
        /// <param name="value">The <see cref="Robot"/>.</param>
        /// <param name="id">The <see cref="Robot"/> identifier.</param>
        /// <param name="requestOptions">The optional <see cref="GrpcRequestOptions"/>.</param>
        /// <returns>A <see cref="GrpcAgentResult"/>.</returns>
        Task<GrpcAgentResult<Robot>> UpdateAsync(Robot value, Guid id, GrpcRequestOptions? requestOptions = null);

        /// <summary>
        /// Deletes the specified <see cref="Robot"/>.
        /// </summary>
        /// <param name="id">The <see cref="Robot"/> identifier.</param>
        /// <param name="requestOptions">The optional <see cref="GrpcRequestOptions"/>.</param>
        /// <returns>A <see cref="GrpcAgentResult"/>.</returns>
        Task<GrpcAgentResult> DeleteAsync(Guid id, GrpcRequestOptions? requestOptions = null);

        /// <summary>
        /// Gets the <see cref="RobotCollectionResult"/> that contains the items that match the selection criteria.
        /// </summary>
        /// <param name="args">The Args (see <see cref="Common.Entities.RobotArgs"/>).</param>
        /// <param name="paging">The <see cref="PagingArgs"/>.</param>
        /// <param name="requestOptions">The optional <see cref="GrpcRequestOptions"/>.</param>
        /// <returns>A <see cref="GrpcAgentResult"/>.</returns>
        Task<GrpcAgentResult<RobotCollectionResult>> GetByArgsAsync(RobotArgs? args, PagingArgs? paging = null, GrpcRequestOptions? requestOptions = null);
    }

    /// <summary>
    /// Provides the <see cref="Robot"/> gRPC agent.
    /// </summary>
    public partial class RobotAgent : GrpcAgentBase<proto.RobotGrpcService.RobotGrpcServiceClient>, IRobotAgent
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RobotAgent"/> class.
        /// </summary>
        /// <param name="args">The <see cref="Common.Agents.IDemoWebApiAgentArgs"/>.</param>
        public RobotAgent(Common.Agents.IDemoWebApiAgentArgs args) : base(args) { }

        /// <summary>
        /// Gets the specified <see cref="Robot"/>.
        /// </summary>
        /// <param name="id">The <see cref="Robot"/> identifier.</param>
        /// <param name="requestOptions">The optional <see cref="GrpcRequestOptions"/>.</param>
        /// <returns>A <see cref="GrpcAgentResult"/>.</returns>
        public Task<GrpcAgentResult<Robot>> GetAsync(Guid id, GrpcRequestOptions? requestOptions = null)
        {
            var __req = new proto.RobotGetRequest { Id = Transformers.GuidToStringConverter.ConvertToDest(id) };
            return InvokeAsync((c, o) => c.GetAsync(__req, o), __req, Transformers.Robot, requestOptions);
        }

        /// <summary>
        /// Creates a new <see cref="Robot"/>.
        /// </summary>
        /// <param name="value">The <see cref="Robot"/>.</param>
        /// <param name="requestOptions">The optional <see cref="GrpcRequestOptions"/>.</param>
        /// <returns>A <see cref="GrpcAgentResult"/>.</returns>
        public Task<GrpcAgentResult<Robot>> CreateAsync(Robot value, GrpcRequestOptions? requestOptions = null)
        {
            var __req = new proto.RobotCreateRequest { Value = Transformers.Robot.MapToDest(Check.NotNull(value, nameof(value))) };
            return InvokeAsync((c, o) => c.CreateAsync(__req, o), __req, Transformers.Robot, requestOptions);
        }

        /// <summary>
        /// Updates an existing <see cref="Robot"/>.
        /// </summary>
        /// <param name="value">The <see cref="Robot"/>.</param>
        /// <param name="id">The <see cref="Robot"/> identifier.</param>
        /// <param name="requestOptions">The optional <see cref="GrpcRequestOptions"/>.</param>
        /// <returns>A <see cref="GrpcAgentResult"/>.</returns>
        public Task<GrpcAgentResult<Robot>> UpdateAsync(Robot value, Guid id, GrpcRequestOptions? requestOptions = null)
        {
            var __req = new proto.RobotUpdateRequest { Value = Transformers.Robot.MapToDest(Check.NotNull(value, nameof(value))), Id = Transformers.GuidToStringConverter.ConvertToDest(id) };
            return InvokeAsync((c, o) => c.UpdateAsync(__req, o), __req, Transformers.Robot, requestOptions);
        }

        /// <summary>
        /// Deletes the specified <see cref="Robot"/>.
        /// </summary>
        /// <param name="id">The <see cref="Robot"/> identifier.</param>
        /// <param name="requestOptions">The optional <see cref="GrpcRequestOptions"/>.</param>
        /// <returns>A <see cref="GrpcAgentResult"/>.</returns>
        public Task<GrpcAgentResult> DeleteAsync(Guid id, GrpcRequestOptions? requestOptions = null)
        {
            var __req = new proto.RobotDeleteRequest { Id = Transformers.GuidToStringConverter.ConvertToDest(id) };
            return InvokeAsync((c, o) => c.DeleteAsync(__req, o), __req, requestOptions);
        }

        /// <summary>
        /// Gets the <see cref="RobotCollectionResult"/> that contains the items that match the selection criteria.
        /// </summary>
        /// <param name="args">The Args (see <see cref="Common.Entities.RobotArgs"/>).</param>
        /// <param name="paging">The <see cref="PagingArgs"/>.</param>
        /// <param name="requestOptions">The optional <see cref="GrpcRequestOptions"/>.</param>
        /// <returns>A <see cref="GrpcAgentResult"/>.</returns>
        public Task<GrpcAgentResult<RobotCollectionResult>> GetByArgsAsync(RobotArgs? args, PagingArgs? paging = null, GrpcRequestOptions? requestOptions = null)
        {
            var __req = new proto.RobotGetByArgsRequest { Args = Transformers.RobotArgs.MapToDest(args), Paging = Transformers.PagingArgsToPagingArgsConverter.ConvertToDest(paging!) };
            return InvokeAsync((c, o) => c.GetByArgsAsync(__req, o), __req, Transformers.RobotCollectionResult, requestOptions);
        }
    }
}

#pragma warning restore IDE0079, IDE0001, IDE0005, IDE0044, CA1034, CA1052, CA1056, CA1819, CA2227, CS0649
#nullable restore