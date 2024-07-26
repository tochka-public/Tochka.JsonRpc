using System;
using System.Linq;
using System.Net.Http;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using Tochka.JsonRpc.Client.HttpMessageHandlers;
using Tochka.JsonRpc.Client.Services;
using Tochka.JsonRpc.Client.Tests.TestHelpers;

namespace Tochka.JsonRpc.Client.Tests;

[TestFixture]
public class ExtensionsTests
{
    [Test]
    public void AddJsonRpcClientWithInterface_RegisterServices()
    {
        var services = new ServiceCollection();

        services.AddJsonRpcClient<ITestJsonRpcClient, TestJsonRpcClient>();

        var result = services.Select(static x => (x.ServiceType, x.Lifetime)).ToList();
        result.Remove((typeof(IHttpClientFactory), ServiceLifetime.Singleton)).Should().BeTrue();
        result.Remove((typeof(IJsonRpcIdGenerator), ServiceLifetime.Singleton)).Should().BeTrue();
        result.Remove((typeof(ITestJsonRpcClient), ServiceLifetime.Transient)).Should().BeTrue();
        result.Remove((typeof(JsonRpcRequestLoggingHandler), ServiceLifetime.Transient)).Should().BeTrue();
    }

    [Test]
    public void AddJsonRpcClientWithInterface_PreserveUserServices()
    {
        var services = new ServiceCollection();
        var idGeneratorMock = Mock.Of<IJsonRpcIdGenerator>();
        services.AddSingleton(idGeneratorMock);

        services.AddJsonRpcClient<ITestJsonRpcClient, TestJsonRpcClient>();

        var result = services.Select(static x => (x.ImplementationInstance, x.Lifetime)).ToList();
        result.Should().Contain((idGeneratorMock, ServiceLifetime.Singleton));
    }

    [Test]
    public void AddJsonRpcClientWithInterface_CallConfigure()
    {
        var services = new ServiceCollection();
        var actionMock = new Mock<Action<IServiceProvider, HttpClient>>();
        services.AddSingleton(Mock.Of<ILogger>());

        services.AddJsonRpcClient<ITestJsonRpcClient, TestJsonRpcClient>(actionMock.Object);
        services.BuildServiceProvider().GetRequiredService<ITestJsonRpcClient>();

        actionMock.Verify(static x => x(It.IsAny<IServiceProvider>(), It.IsAny<HttpClient>()));
    }

    [Test]
    public void AddJsonRpcClientWithInterface_WorkWithoutConfigure()
    {
        var services = new ServiceCollection();
        services.AddSingleton(Mock.Of<ILogger>());

        services.AddJsonRpcClient<ITestJsonRpcClient, TestJsonRpcClient>();
        services.BuildServiceProvider().GetRequiredService<ITestJsonRpcClient>();
    }

    [Test]
    public void AddJsonRpcClient_RegisterServices()
    {
        var services = new ServiceCollection();

        services.AddJsonRpcClient<TestJsonRpcClient>();

        var result = services.Select(static x => (x.ServiceType, x.Lifetime)).ToList();
        result.Remove((typeof(IHttpClientFactory), ServiceLifetime.Singleton)).Should().BeTrue();
        result.Remove((typeof(IJsonRpcIdGenerator), ServiceLifetime.Singleton)).Should().BeTrue();
        result.Remove((typeof(TestJsonRpcClient), ServiceLifetime.Transient)).Should().BeTrue();
        result.Remove((typeof(JsonRpcRequestLoggingHandler), ServiceLifetime.Transient)).Should().BeTrue();
    }

    [Test]
    public void AddJsonRpcClient_PreserveUserServices()
    {
        var services = new ServiceCollection();
        var idGeneratorMock = Mock.Of<IJsonRpcIdGenerator>();
        services.AddSingleton(idGeneratorMock);

        services.AddJsonRpcClient<TestJsonRpcClient>();

        var result = services.Select(static x => (x.ImplementationInstance, x.Lifetime)).ToList();
        result.Should().Contain((idGeneratorMock, ServiceLifetime.Singleton));
    }

    [Test]
    public void AddJsonRpcClient_CallConfigure()
    {
        var services = new ServiceCollection();
        var actionMock = new Mock<Action<IServiceProvider, HttpClient>>();
        services.AddSingleton(Mock.Of<ILogger>());

        services.AddJsonRpcClient<TestJsonRpcClient>(actionMock.Object);
        services.BuildServiceProvider().GetRequiredService<TestJsonRpcClient>();

        actionMock.Verify(static x => x(It.IsAny<IServiceProvider>(), It.IsAny<HttpClient>()));
    }

    [Test]
    public void AddJsonRpcClient_WorkWithoutConfigure()
    {
        var services = new ServiceCollection();
        services.AddSingleton(Mock.Of<ILogger>());

        services.AddJsonRpcClient<ITestJsonRpcClient, TestJsonRpcClient>();
        services.BuildServiceProvider().GetRequiredService<ITestJsonRpcClient>();
    }
}
