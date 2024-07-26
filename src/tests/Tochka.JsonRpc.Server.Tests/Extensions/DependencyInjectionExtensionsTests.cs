using System;
using System.Linq;
using Asp.Versioning;
using Asp.Versioning.ApiExplorer;
using Asp.Versioning.ApplicationModels;
using Asp.Versioning.Conventions;
using FluentAssertions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using Tochka.JsonRpc.Server.Binding;
using Tochka.JsonRpc.Server.DependencyInjection;
using Tochka.JsonRpc.Server.Extensions;
using Tochka.JsonRpc.Server.Filters;
using Tochka.JsonRpc.Server.Routing;
using Tochka.JsonRpc.Server.Services;
using Tochka.JsonRpc.Server.Settings;

namespace Tochka.JsonRpc.Server.Tests.Extensions;

[TestFixture]
public class DependencyInjectionExtensionsTests
{
    [Test]
    public void AddJsonRpcServer_RegisterServices()
    {
        var services = new ServiceCollection();
        var configureOptions = Mock.Of<Action<JsonRpcServerOptions>>();

        services.AddJsonRpcServer(configureOptions);

        var result = services.Select(static x => (x.ServiceType, x.ImplementationType, x.Lifetime)).ToList();
        // services defined in library
        result.Remove((typeof(JsonRpcActionModelConvention), typeof(JsonRpcActionModelConvention), ServiceLifetime.Singleton)).Should().BeTrue();
        result.Remove((typeof(IConfigureOptions<MvcOptions>), typeof(ModelConventionConfigurator<JsonRpcActionModelConvention>), ServiceLifetime.Singleton)).Should().BeTrue();
        result.Remove((typeof(JsonRpcParameterModelConvention), typeof(JsonRpcParameterModelConvention), ServiceLifetime.Singleton)).Should().BeTrue();
        result.Remove((typeof(IConfigureOptions<MvcOptions>), typeof(ModelConventionConfigurator<JsonRpcParameterModelConvention>), ServiceLifetime.Singleton)).Should().BeTrue();
        result.Remove((typeof(MatcherPolicy), typeof(JsonRpcMatcherPolicy), ServiceLifetime.Singleton)).Should().BeTrue();
        result.Remove((typeof(IJsonRpcParamsParser), typeof(JsonRpcParamsParser), ServiceLifetime.Singleton)).Should().BeTrue();
        result.Remove((typeof(IJsonRpcParameterBinder), typeof(JsonRpcParameterBinder), ServiceLifetime.Singleton)).Should().BeTrue();
        result.Remove((typeof(IJsonRpcRequestHandler), typeof(JsonRpcRequestHandler), ServiceLifetime.Singleton)).Should().BeTrue();
        result.Remove((typeof(IJsonRpcExceptionWrapper), typeof(JsonRpcExceptionWrapper), ServiceLifetime.Singleton)).Should().BeTrue();
        result.Remove((typeof(IJsonRpcRequestValidator), typeof(JsonRpcRequestValidator), ServiceLifetime.Singleton)).Should().BeTrue();
        result.Remove((typeof(IJsonRpcErrorFactory), typeof(JsonRpcErrorFactory), ServiceLifetime.Singleton)).Should().BeTrue();
        result.Remove((typeof(IApiControllerSpecification), typeof(JsonRpcControllerSpecification), ServiceLifetime.Singleton)).Should().BeTrue();
        result.Remove((typeof(IApiVersionDescriptionProvider), typeof(DefaultApiVersionDescriptionProvider), ServiceLifetime.Singleton)).Should().BeTrue();
        result.Remove((typeof(JsonRpcMarkerService), typeof(JsonRpcMarkerService), ServiceLifetime.Singleton)).Should().BeTrue();
        // one of services registered by calling AddApiVersioning
        result.Remove((typeof(IApiVersionSelector), null, ServiceLifetime.Singleton)).Should().BeTrue();
        // one of services registered by calling AddApiVersioning.AddMvc
        result.Remove((typeof(IControllerNameConvention), typeof(DefaultControllerNameConvention), ServiceLifetime.Singleton)).Should().BeTrue();
        // one of services registered by calling AddApiVersioning.AddApiExplorer
        var x = result.Single(x => x.ServiceType == typeof(IApiVersionDescriptionProviderFactory));
        x.ImplementationType.Should().NotBeNull("can't check type directly because it is internal");
        x.Lifetime.Should().Be(ServiceLifetime.Transient);
        result.Remove(x).Should().BeTrue();
    }

    [Test]
    public void AddJsonRpcServer_PreserveUserServices()
    {
        var services = new ServiceCollection();
        var configureOptions = Mock.Of<Action<JsonRpcServerOptions>>();
        var mvcOptionsConfiguratorMock = Mock.Of<IConfigureOptions<MvcOptions>>();
        var matcherPolicyMock = Mock.Of<MatcherPolicy>();
        var paramsParserMock = Mock.Of<IJsonRpcParamsParser>();
        var parameterBinderMock = Mock.Of<IJsonRpcParameterBinder>();
        var requestHandlerMock = Mock.Of<IJsonRpcRequestHandler>();
        var exceptionWrapperMock = Mock.Of<IJsonRpcExceptionWrapper>();
        var requestValidatorMock = Mock.Of<IJsonRpcRequestValidator>();
        var errorFactoryMock = Mock.Of<IJsonRpcErrorFactory>();
        services.AddSingleton(mvcOptionsConfiguratorMock);
        services.AddSingleton(matcherPolicyMock);
        services.AddSingleton(paramsParserMock);
        services.AddSingleton(parameterBinderMock);
        services.AddSingleton(requestHandlerMock);
        services.AddSingleton(exceptionWrapperMock);
        services.AddSingleton(errorFactoryMock);
        services.AddSingleton(requestValidatorMock);

        services.AddJsonRpcServer(configureOptions);

        var result = services.Select(static x => (x.ImplementationInstance, x.Lifetime)).ToList();
        result.Should().Contain((mvcOptionsConfiguratorMock, ServiceLifetime.Singleton));
        result.Should().Contain((matcherPolicyMock, ServiceLifetime.Singleton));
        result.Should().Contain((paramsParserMock, ServiceLifetime.Singleton));
        result.Should().Contain((parameterBinderMock, ServiceLifetime.Singleton));
        result.Should().Contain((requestHandlerMock, ServiceLifetime.Singleton));
        result.Should().Contain((exceptionWrapperMock, ServiceLifetime.Singleton));
        result.Should().Contain((requestValidatorMock, ServiceLifetime.Singleton));
        result.Should().Contain((errorFactoryMock, ServiceLifetime.Singleton));
    }

    [Test]
    public void AddJsonRpcServer_ConventionsAlreadyRegistered_DontRegisterAgain()
    {
        var services = new ServiceCollection();
        var configureOptions = Mock.Of<Action<JsonRpcServerOptions>>();
        services.AddSingleton<JsonRpcActionModelConvention>();
        services.AddSingleton<IConfigureOptions<MvcOptions>, ModelConventionConfigurator<JsonRpcActionModelConvention>>();
        services.AddSingleton<JsonRpcParameterModelConvention>();
        services.AddSingleton<IConfigureOptions<MvcOptions>, ModelConventionConfigurator<JsonRpcParameterModelConvention>>();

        services.AddJsonRpcServer(configureOptions);

        services.Where(static s => s.ImplementationType == typeof(JsonRpcActionModelConvention)).Should().HaveCount(1);
        services.Where(static s => s.ImplementationType == typeof(ModelConventionConfigurator<JsonRpcActionModelConvention>)).Should().HaveCount(1);
        services.Where(static s => s.ImplementationType == typeof(JsonRpcParameterModelConvention)).Should().HaveCount(1);
        services.Where(static s => s.ImplementationType == typeof(ModelConventionConfigurator<JsonRpcParameterModelConvention>)).Should().HaveCount(1);
    }

    [Test]
    public void AddJsonRpcServer_CallConfigure()
    {
        var services = new ServiceCollection();
        var configureOptionsMock = new Mock<Action<JsonRpcServerOptions>>();

        services.AddJsonRpcServer(configureOptionsMock.Object);
        _ = services.BuildServiceProvider().GetRequiredService<IOptions<JsonRpcServerOptions>>().Value;

        configureOptionsMock.Verify(static x => x(It.IsAny<JsonRpcServerOptions>()));
    }

    [Test]
    public void AddJsonRpcServer_ConfigureApiVersioning()
    {
        var services = new ServiceCollection();
        var configureOptions = Mock.Of<Action<JsonRpcServerOptions>>();

        services.AddJsonRpcServer(configureOptions);
        var apiVersioningOptions = services.BuildServiceProvider().GetRequiredService<IOptions<ApiVersioningOptions>>().Value;

        apiVersioningOptions.DefaultApiVersion.Should().BeEquivalentTo(new ApiVersion(1, 0));
        apiVersioningOptions.AssumeDefaultVersionWhenUnspecified.Should().BeTrue();
    }

    [Test]
    public void AddJsonRpcServer_ConfigureApiVersioningExplorer()
    {
        var services = new ServiceCollection();
        var configureOptions = Mock.Of<Action<JsonRpcServerOptions>>();

        services.AddJsonRpcServer(configureOptions);
        var apiExplorerOptions = services.BuildServiceProvider().GetRequiredService<IOptions<ApiExplorerOptions>>().Value;

        apiExplorerOptions.SubstituteApiVersionInUrl.Should().BeTrue();
        apiExplorerOptions.GroupNameFormat.Should().Be("'v'VVV");
        var name = "name";
        var version = "version";
        apiExplorerOptions.FormatGroupName(name, version).Should().Be($"{name}_{version}");
    }

    [Test]
    public void AddJsonRpcServer_AddFilters()
    {
        var services = new ServiceCollection();
        var configureOptions = Mock.Of<Action<JsonRpcServerOptions>>();

        services.AddJsonRpcServer(configureOptions);
        var options = services.BuildServiceProvider().GetRequiredService<IOptions<MvcOptions>>().Value;

        options.Filters.Should().ContainEquivalentOf(new TypeFilterAttribute(typeof(JsonRpcActionFilter)) { Order = int.MaxValue });
        options.Filters.Should().ContainEquivalentOf(new TypeFilterAttribute(typeof(JsonRpcExceptionLoggingFilter)) { Order = int.MinValue });
        options.Filters.Should().ContainEquivalentOf(new TypeFilterAttribute(typeof(JsonRpcExceptionWrappingFilter)) { Order = int.MaxValue });
        options.Filters.Should().ContainEquivalentOf(new TypeFilterAttribute(typeof(JsonRpcResultFilter)) { Order = int.MaxValue });
    }

    [Test]
    public void UseJsonRpc_AddWasNotCalled_Throw()
    {
        var services = new ServiceCollection();
        var app = new Mock<IApplicationBuilder>();
        app.Setup(static a => a.ApplicationServices)
            .Returns(services.BuildServiceProvider)
            .Verifiable();

        var action = () => app.Object.UseJsonRpc();

        action.Should().Throw<InvalidOperationException>();
        app.Verify();
    }

    [Test]
    public void UseJsonRpc_AddWasCalled_DontThrow()
    {
        var services = new ServiceCollection();
        var configureOptions = Mock.Of<Action<JsonRpcServerOptions>>();
        services.AddJsonRpcServer(configureOptions);
        var app = new Mock<IApplicationBuilder>();
        app.Setup(static a => a.ApplicationServices)
            .Returns(services.BuildServiceProvider)
            .Verifiable();

        var action = () => app.Object.UseJsonRpc();

        action.Should().NotThrow();
        app.Verify();
    }
}
