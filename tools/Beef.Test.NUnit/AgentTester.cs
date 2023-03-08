using Microsoft.Extensions.DependencyInjection;
using System;
using UnitTestEx.NUnit;
using UnitTestEx.NUnit.Internal;

namespace Beef.Test.NUnit
{
    /// <summary>
    /// Provides existing <c>Test</c> methods to support backwards compatibility.
    /// </summary>
    /// <remarks>It is <b>recommended</b> that usage is upgraded to the new <see cref="ApiTester.Create{TEntryPoint}"/> as this will eventually be deprecated.</remarks>
    public static class AgentTester
    {
        /// <summary>
        /// Creates an <see cref="AgentTester{TEntryPoint}"/> to support agent-initiated API testing.
        /// </summary>
        /// <typeparam name="TEntryPoint">The API startup <see cref="Type"/>.</typeparam>
        /// <param name="configureServices">Provides an opportunity to configure the <see cref="IServiceCollection"/>.</param>
        /// <param name="configureTester">Provides an opportunity to configure the owning <see cref="ApiTester{TEntryPoint}"/></param>
        /// <returns>The <see cref="AgentTester{TEntryPoint}"/>.</returns>
        /// <remarks>It is <b>recommended</b> that usage is upgraded to the new <see cref="ApiTester.Create{TEntryPoint}"/> as this will eventually be deprecated.</remarks>
        public static AgentTester<TEntryPoint> CreateWaf<TEntryPoint>(Action<IServiceCollection>? configureServices = null, Action<ApiTester<TEntryPoint>>? configureTester = null) where TEntryPoint : class
        {
            var tester = ApiTester.Create<TEntryPoint>();
            configureTester?.Invoke(tester);

            if (configureServices != null)
                tester.ConfigureServices(configureServices);

            return new AgentTester<TEntryPoint>(tester);
        }
    }
}