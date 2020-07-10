using Beef.Demo.Api;
using Beef.Demo.Business;
using Beef.Demo.Common.Agents;
using Beef.Demo.Common.Entities;
using Beef.Test.NUnit;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Beef.Demo.Test
{
    [TestFixture, Parallelizable(ParallelScope.Children)]
    public class BlahTest
    {
        private WebApplicationFactory<Startup> _webAppFactory;
        private IServiceCollection _services;

        [OneTimeSetUp]
        public void Setup()
        {
            _webAppFactory = new WebApplicationFactory<Startup>()
                .WithWebHostBuilder(whb =>
                {
                    whb.ConfigureTestServices(sc =>
                    {
                        _services = sc;
                    });

                    whb.ConfigureAppConfiguration((ctx, builder) =>
                    {

                    });
                });
        }

        private HttpClient CreateHttpClient() => _webAppFactory.CreateClient();

        private WebApplicationFactory<TStartup> CreateWebApplicationFactory<TStartup>(Action<IServiceCollection> services) where TStartup : class
        {
            return new WebApplicationFactory<TStartup>()
                .WithWebHostBuilder(whb =>
                {
                    whb.ConfigureTestServices(sc =>
                    {
                        services?.Invoke(sc);
                    });

                    whb.ConfigureAppConfiguration((ctx, builder) =>
                    {

                    });
                });
        }

        [Test]
        public async Task TestA()
        {
            // Support the addition of the options.
            var client = CreateWebApplicationFactory<Startup>(services =>
            {
                var mockService = new Mock<IContactManager>();
                mockService.Setup(o => o.GetAllAsync()).Returns(Task.FromResult(new ContactCollectionResult() { Result = new ContactCollection { new Contact { Id = 8.ToGuid(), FirstName = "Test", LastName = "Er" } } }));
                services.AddScoped<IContactManager>(_ => mockService.Object);
            }).CreateClient();

            var ca = new ContactAgent(client);
            var x = await ca.GetAllAsync();
            Assert.Warn(x.Content);
        }

        [Test]
        public async Task TestB()
        {
            // Support the addition of the options.
            var client = _webAppFactory.CreateClient();

            var ca = new ContactAgent(client);
            var x = await ca.GetAllAsync();
            Assert.Warn(x.Content);
        }

        [Test]
        public async Task TestC()
        {
            // Support the addition of the options.
            var client = _webAppFactory.CreateClient();

            var ca = new ContactAgent(client);
            var x = await ca.GetAllAsync();
            Assert.Warn(x.Content);
        }

        [Test]
        public async Task TestD()
        {
            // Support the addition of the options.
            var client = CreateWebApplicationFactory<Startup>(services =>
            {
                var mockService = new Mock<IContactManager>();
                mockService.Setup(o => o.GetAllAsync()).Returns(Task.FromResult(new ContactCollectionResult() { Result = new ContactCollection { new Contact { Id = 8.ToGuid(), FirstName = "Test", LastName = "Er" } } }));
                services.AddScoped(_ => mockService.Object);
            }).CreateClient();

            var ca = new ContactAgent(client);
            var x = await ca.GetAllAsync();
            Assert.Warn(x.Content);
        }

        [Test]
        public async Task TestE()
        {
            // Support the addition of the options.
            var client = _webAppFactory.CreateClient();

            var ca = new ContactAgent(client);
            var x = await ca.GetAllAsync();
            Assert.Warn(x.Content);
        }

        [Test]
        public async Task TestF()
        {
            // Support the addition of the options.
            var client = CreateWebApplicationFactory<Startup>(services =>
            {
                var mockService = new Mock<IContactManager>();
                mockService.Setup(o => o.GetAllAsync()).Returns(Task.FromResult(new ContactCollectionResult() { Result = new ContactCollection { new Contact { Id = 8.ToGuid(), FirstName = "Test", LastName = "Er" } } }));
                services.AddScoped(_ => mockService.Object);
            }).CreateClient();

            var ca = new ContactAgent(client);
            var x = await ca.GetAllAsync();
            Assert.Warn(x.Content);
        }

        [Test]
        public async Task TestG()
        {
            // Support the addition of the options.
            var client = _webAppFactory.CreateClient();

            var ca = new ContactAgent(client);
            var x = await ca.GetAllAsync();
            Assert.Warn(x.Content);
        }

        [Test]
        public async Task TestH()
        {
            // Support the addition of the options.
            var client = CreateWebApplicationFactory<Startup>(services =>
            {
                var mockService = new Mock<IContactManager>();
                mockService.Setup(o => o.GetAllAsync()).Returns(Task.FromResult(new ContactCollectionResult() { Result = new ContactCollection { new Contact { Id = 8.ToGuid(), FirstName = "Test", LastName = "Er" } } }));
                services.AddScoped(_ => mockService.Object);
            }).CreateClient();

            var ca = new ContactAgent(client);
            var x = await ca.GetAllAsync();
            Assert.Warn(x.Content);
        }
    }
}