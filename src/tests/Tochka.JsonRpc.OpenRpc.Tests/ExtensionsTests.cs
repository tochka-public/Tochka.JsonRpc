using System;
using System.IO;
using System.Linq;
using System.Reflection;
using FluentAssertions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using Tochka.JsonRpc.ApiExplorer;
using Tochka.JsonRpc.OpenRpc.Models;
using Tochka.JsonRpc.OpenRpc.Services;
using Tochka.JsonRpc.Tests.WebApplication;
using Tochka.JsonRpc.TestUtils;

namespace Tochka.JsonRpc.OpenRpc.Tests;

[TestFixture]
internal class ExtensionsTests
{
    [Test]
    public void AddOpenRpc_RegisterServices()
    {
        var services = new ServiceCollection();
        var xmlDocAssembly = Assembly.GetAssembly(typeof(WebApplicationMarker));
        var info = new OpenRpcInfo("title", "version");
        var setup = Mock.Of<Action<OpenRpcOptions>>();

        services.AddOpenRpc(xmlDocAssembly, info, setup);

        var result = services.Select(static x => (x.ServiceType, x.ImplementationType, x.Lifetime)).ToList();
        result.Remove((typeof(IOpenRpcDocumentGenerator), typeof(OpenRpcDocumentGenerator), ServiceLifetime.Scoped)).Should().BeTrue();
        result.Remove((typeof(IOpenRpcSchemaGenerator), typeof(OpenRpcSchemaGenerator), ServiceLifetime.Scoped)).Should().BeTrue();
        result.Remove((typeof(IOpenRpcContentDescriptorGenerator), typeof(OpenRpcContentDescriptorGenerator), ServiceLifetime.Scoped)).Should().BeTrue();
        result.Remove((typeof(ITypeEmitter), typeof(TypeEmitter), ServiceLifetime.Singleton)).Should().BeTrue();
        result.Remove((typeof(IApiDescriptionProvider), typeof(JsonRpcDescriptionProvider), ServiceLifetime.Transient)).Should().BeTrue();
        result.Remove((typeof(OpenRpcMarkerService), typeof(OpenRpcMarkerService), ServiceLifetime.Singleton)).Should().BeTrue();
    }

    [Test]
    public void AddOpenRpc_PreserveUserServices()
    {
        var services = new ServiceCollection();
        var xmlDocAssembly = Assembly.GetAssembly(typeof(WebApplicationMarker));
        var info = new OpenRpcInfo("title", "version");
        var setup = Mock.Of<Action<OpenRpcOptions>>();
        var documentGeneratorMock = Mock.Of<IOpenRpcDocumentGenerator>();
        var schemaGeneratorMock = Mock.Of<IOpenRpcSchemaGenerator>();
        var contentDescriptorGeneratorMock = Mock.Of<IOpenRpcContentDescriptorGenerator>();
        var typeEmitterMock = Mock.Of<ITypeEmitter>();
        services.AddSingleton(documentGeneratorMock);
        services.AddSingleton(schemaGeneratorMock);
        services.AddSingleton(contentDescriptorGeneratorMock);
        services.AddSingleton(typeEmitterMock);

        services.AddOpenRpc(xmlDocAssembly, info, setup);

        var result = services.Select(static x => (x.ImplementationInstance, x.Lifetime)).ToList();
        result.Should().Contain((documentGeneratorMock, ServiceLifetime.Singleton));
        result.Should().Contain((schemaGeneratorMock, ServiceLifetime.Singleton));
        result.Should().Contain((contentDescriptorGeneratorMock, ServiceLifetime.Singleton));
        result.Should().Contain((typeEmitterMock, ServiceLifetime.Singleton));
    }

    [Test]
    public void AddOpenRpc_OtherDescriptionProviderAlreadyRegistered_RegisterJsonRpcProvider()
    {
        var services = new ServiceCollection();
        var xmlDocAssembly = Assembly.GetAssembly(typeof(WebApplicationMarker));
        var info = new OpenRpcInfo("title", "version");
        var setup = Mock.Of<Action<OpenRpcOptions>>();
        services.AddTransient<IApiDescriptionProvider, DefaultApiDescriptionProvider>();

        services.AddOpenRpc(xmlDocAssembly, info, setup);

        services.Should().Contain(static s => s.ImplementationType == typeof(DefaultApiDescriptionProvider));
        services.Should().Contain(static s => s.ImplementationType == typeof(JsonRpcDescriptionProvider));
    }

    [Test]
    public void AddOpenRpc_JsonRpcDescriptionProviderAlreadyRegistered_DontRegisterAgain()
    {
        var services = new ServiceCollection();
        var xmlDocAssembly = Assembly.GetAssembly(typeof(WebApplicationMarker));
        var info = new OpenRpcInfo("title", "version");
        var setup = Mock.Of<Action<OpenRpcOptions>>();
        services.AddTransient<IApiDescriptionProvider, JsonRpcDescriptionProvider>();

        services.AddOpenRpc(xmlDocAssembly, info, setup);

        services.Where(static s => s.ImplementationType == typeof(JsonRpcDescriptionProvider)).Should().HaveCount(1);
    }

    [Test]
    public void AddOpenRpc_CallSetupAction()
    {
        var services = new ServiceCollection();
        var xmlDocAssembly = Assembly.GetAssembly(typeof(WebApplicationMarker));
        var info = new OpenRpcInfo("title", "version");
        var setup = new Mock<Action<OpenRpcOptions>>();

        services.AddOpenRpc(xmlDocAssembly, info, setup.Object);
        var _ = services.BuildServiceProvider().GetRequiredService<IOptions<OpenRpcOptions>>().Value;

        setup.Verify(static x => x(It.IsAny<OpenRpcOptions>()));
    }

    [Test]
    public void AddOpenRpc_AddOpenRpcDoc()
    {
        var services = new ServiceCollection();
        var xmlDocAssembly = Assembly.GetAssembly(typeof(WebApplicationMarker));
        var info = new OpenRpcInfo("title", "version");
        var setup = Mock.Of<Action<OpenRpcOptions>>();

        services.AddOpenRpc(xmlDocAssembly, info, setup);
        var options = services.BuildServiceProvider().GetRequiredService<IOptions<OpenRpcOptions>>().Value;

        options.Docs.Should().Contain(ApiExplorerConstants.DefaultDocumentName, info);
    }

    [Test]
    public void AddOpenRpc_XmlDocDoesntExist_Throw()
    {
        var services = new ServiceCollection();
        var xmlDocAssembly = Assembly.GetAssembly(typeof(TestUtilsMarker));
        var info = new OpenRpcInfo("title", "version");
        var setup = Mock.Of<Action<OpenRpcOptions>>();

        var action = () => services.AddOpenRpc(xmlDocAssembly, info, setup);

        action.Should().Throw<FileNotFoundException>();
    }

    [Test]
    public void AddOpenRpc_UseDefaultInfo_GetInfoFromAssembly()
    {
        var services = new ServiceCollection();
        var setup = Mock.Of<Action<OpenRpcOptions>>();
        var xmlDocAssembly = Assembly.GetAssembly(typeof(WebApplicationMarker));

        services.AddOpenRpc(xmlDocAssembly, setup);
        var options = services.BuildServiceProvider().GetRequiredService<IOptions<OpenRpcOptions>>().Value;

        var expectedTitle = $"Tochka.JsonRpc.Tests.WebApplication {ApiExplorerConstants.DefaultDocumentTitle}";
        var expected = new OpenRpcInfo(expectedTitle, ApiExplorerConstants.DefaultDocumentVersion)
        {
            Description = "description for tests"
        };
        options.Docs.Should().Contain(ApiExplorerConstants.DefaultDocumentName, expected);
    }

    [Test]
    public void OpenRpcDoc_AddsDocToOptions()
    {
        var options = new OpenRpcOptions();
        var info = new OpenRpcInfo("title", "version");
        var name = "name";

        options.OpenRpcDoc(name, info);

        options.Docs.Should().Contain(name, info);
    }

    [Test]
    public void UseOpenRpc_AddWasNotCalled_Throw()
    {
        var services = new ServiceCollection();
        var app = new Mock<IApplicationBuilder>();
        app.Setup(static a => a.ApplicationServices)
            .Returns(services.BuildServiceProvider)
            .Verifiable();

        var action = () => app.Object.UseOpenRpc();

        action.Should().Throw<InvalidOperationException>();
        app.Verify();
    }

    [Test]
    public void UseOpenRpc_AddWasCalled_DontThrow()
    {
        var services = new ServiceCollection();
        var xmlDocAssembly = Assembly.GetAssembly(typeof(WebApplicationMarker));
        var info = new OpenRpcInfo("title", "version");
        var setup = Mock.Of<Action<OpenRpcOptions>>();
        services.AddOpenRpc(xmlDocAssembly, info, setup);
        var app = new Mock<IApplicationBuilder>();
        app.Setup(static a => a.ApplicationServices)
            .Returns(services.BuildServiceProvider)
            .Verifiable();

        var action = () => app.Object.UseOpenRpc();

        action.Should().NotThrow();
        app.Verify();
    }
}
