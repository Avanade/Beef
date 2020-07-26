// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Microsoft.Extensions.DependencyInjection;

namespace Beef.Grpc
{
    /// <summary>
    /// Enables the <b>Beef</b> gRPC extension(s).
    /// </summary>
    public static class GrpcExtensions
    {
        /// <summary>
        /// Adds the required <b>Service</b> (server-side) services.
        /// </summary>
        /// <param name="serviceCollection">The <see cref="IServiceCollection"/>.</param>
        /// <returns>The <see cref="IServiceCollection"/>.</returns>
        public static IServiceCollection AddBeefGrpcServiceServices(this IServiceCollection serviceCollection)
            => serviceCollection.AddSingleton(_ => new GrpcInvoker());

        /// <summary>
        /// Adds the required <b>Agent</b> (client-side) services.
        /// </summary>
        /// <param name="serviceCollection">The <see cref="IServiceCollection"/>.</param>
        /// <returns>The <see cref="IServiceCollection"/>.</returns>
        public static IServiceCollection AddBeefGrpcAgentServices(this IServiceCollection serviceCollection)
            => serviceCollection.AddSingleton(_ => new GrpcAgentInvoker());
    }
}