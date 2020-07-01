using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net.Http;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using Tochka.JsonRpc.Client.Services;
using Tochka.JsonRpc.Client.Settings;
using Tochka.JsonRpc.Common.Serializers;

namespace Tochka.JsonRpc.Client.Tests
{
    public class ExtensionsTests
    {
        [Test]
        public void Test_AddJsonRpcClient_RegistersServices()
        {
            var services = new ServiceCollection();

            services.AddJsonRpcClient<TestClient>();

            var result = services.Select(x => (x.ServiceType, x.Lifetime)).ToList();
            result.Remove((typeof(IHttpClientFactory), ServiceLifetime.Singleton)).Should().BeTrue();
            result.Remove((typeof(IJsonRpcIdGenerator), ServiceLifetime.Singleton)).Should().BeTrue();
            result.Remove((typeof(HeaderJsonRpcSerializer), ServiceLifetime.Singleton)).Should().BeTrue();
            result.Remove((typeof(TestClient), ServiceLifetime.Transient)).Should().BeTrue();
        }

        [Test]
        public void Test_AddJsonRpcClient_PreservesUserServices()
        {
            var services = new ServiceCollection();

            var mocks = new List<Mock>
            {
                RegisterMock<HeaderJsonRpcSerializer>(services),
                RegisterMock<IJsonRpcIdGenerator>(services),
            };

            services.AddJsonRpcClient<TestClient>();

            var result = services.Select(x => (x.ImplementationInstance, x.Lifetime)).ToList();
            foreach (var mock in mocks)
            {
                result.Should().Contain((mock.Object, ServiceLifetime.Singleton));
            }
        }

        [Test]
        public void Test_AddJsonRpcClient_CallsConfigure()
        {
            var services = new ServiceCollection();
            var actionMock = new Mock<Action<IServiceProvider, HttpClient>>();
            services.AddSingleton(Mock.Of<IJsonRpcSerializer>());
            services.Configure<TestOptions>(options =>
            {
                options.Url = "http://foo.bar/";
            });
            services.AddSingleton(Mock.Of<ILogger>());

            services.AddJsonRpcClient<TestClient>(actionMock.Object);
            services.BuildServiceProvider().GetRequiredService<TestClient>();

            actionMock.Verify(x => x(It.IsAny<IServiceProvider>(), It.IsAny<HttpClient>()));
        }

        [Test]
        public void Test_AddJsonRpcClient_ConfigureNullWorks()
        {
            var services = new ServiceCollection();
            services.AddSingleton(Mock.Of<IJsonRpcSerializer>());
            services.Configure<TestOptions>(options =>
            {
                options.Url = "http://foo.bar/";
            });
            services.AddSingleton(Mock.Of<ILogger>());

            services.AddJsonRpcClient<TestClient>();
            services.BuildServiceProvider().GetRequiredService<TestClient>();
        }

        [Test]
        public void Test_AddJsonRpcClientWithInterface_RegistersServices()
        {
            var services = new ServiceCollection();

            services.AddJsonRpcClient<ITestClient, TestClient>();

            var result = services.Select(x => (x.ServiceType, x.Lifetime)).ToList();
            result.Remove((typeof(IHttpClientFactory), ServiceLifetime.Singleton)).Should().BeTrue();
            result.Remove((typeof(IJsonRpcIdGenerator), ServiceLifetime.Singleton)).Should().BeTrue();
            result.Remove((typeof(HeaderJsonRpcSerializer), ServiceLifetime.Singleton)).Should().BeTrue();
            result.Remove((typeof(ITestClient), ServiceLifetime.Transient)).Should().BeTrue();
        }

        [Test]
        public void Test_AddJsonRpcClientWithInterface_PreservesUserServices()
        {
            var services = new ServiceCollection();

            var mocks = new List<Mock>
            {
                RegisterMock<HeaderJsonRpcSerializer>(services),
                RegisterMock<IJsonRpcIdGenerator>(services),
            };

            services.AddJsonRpcClient<ITestClient, TestClient>();

            var result = services.Select(x => (x.ImplementationInstance, x.Lifetime)).ToList();
            foreach (var mock in mocks)
            {
                result.Should().Contain((mock.Object, ServiceLifetime.Singleton));
            }
        }

        [Test]
        public void Test_AddJsonRpcClientWithInterface_CallsConfigure()
        {
            var services = new ServiceCollection();
            var actionMock = new Mock<Action<IServiceProvider, HttpClient>>();
            services.AddSingleton(Mock.Of<IJsonRpcSerializer>());
            services.Configure<TestOptions>(options =>
            {
                options.Url = "http://foo.bar/";
            });
            services.AddSingleton(Mock.Of<ILogger>());

            services.AddJsonRpcClient<ITestClient, TestClient>(actionMock.Object);
            services.BuildServiceProvider().GetRequiredService<ITestClient>();

            actionMock.Verify(x => x(It.IsAny<IServiceProvider>(), It.IsAny<HttpClient>()));
        }

        [Test]
        public void Test_AddJsonRpcClientWithInterface_ConfigureNullWorks()
        {
            var services = new ServiceCollection();
            services.AddSingleton(Mock.Of<IJsonRpcSerializer>());
            services.Configure<TestOptions>(options =>
            {
                options.Url = "http://foo.bar/";
            });
            services.AddSingleton(Mock.Of<ILogger>());

            services.AddJsonRpcClient<ITestClient,TestClient>();
            services.BuildServiceProvider().GetRequiredService<ITestClient>();
        }

        private static Mock<T> RegisterMock<T>(IServiceCollection services, params object[] args) where T : class
        {
            var mock = new Mock<T>(args);
            services.AddSingleton(mock.Object);
            return mock;
        }
    }
}