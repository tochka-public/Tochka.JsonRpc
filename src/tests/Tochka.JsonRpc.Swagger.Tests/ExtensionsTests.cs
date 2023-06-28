using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Moq;
using NUnit.Framework;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;
using Swashbuckle.AspNetCore.SwaggerUI;
using Tochka.JsonRpc.ApiExplorer;
using Tochka.JsonRpc.Server.Serialization;
using Tochka.JsonRpc.Tests.WebApplication;
using Yoh.Text.Json.NamingPolicies;

namespace Tochka.JsonRpc.Swagger.Tests;

[TestFixture]
internal class ExtensionsTests
{
    [Test]
    public void AddSwaggerWithJsonRpc_RegisterServices()
    {
        var services = new ServiceCollection();
        var info = new OpenApiInfo();
        var setup = new Mock<Action<SwaggerGenOptions>>();

        services.AddSwaggerWithJsonRpc(Mock.Of<Assembly>(), info, setup.Object);

        var result = services.Select(static x => (x.ServiceType, x.ImplementationType, x.Lifetime)).ToList();
        // services defined in library
        result.Remove((typeof(ISchemaGenerator), typeof(JsonRpcSchemaGenerator), ServiceLifetime.Transient)).Should().BeTrue();
        result.Remove((typeof(ITypeEmitter), typeof(TypeEmitter), ServiceLifetime.Singleton)).Should().BeTrue();
        result.Remove((typeof(IApiDescriptionProvider), typeof(JsonRpcDescriptionProvider), ServiceLifetime.Transient)).Should().BeTrue();
        // one of services registered by calling AddSwaggerGen
        result.Remove((typeof(ISwaggerProvider), typeof(SwaggerGenerator), ServiceLifetime.Transient)).Should().BeTrue();
    }

    [Test]
    public void AddSwaggerWithJsonRpc_PreserveUserServices()
    {
        var services = new ServiceCollection();
        var info = new OpenApiInfo();
        var setup = new Mock<Action<SwaggerGenOptions>>();
        var schemaGeneratorMock = Mock.Of<ISchemaGenerator>();
        var typeEmitterMock = Mock.Of<ITypeEmitter>();
        services.AddSingleton(schemaGeneratorMock);
        services.AddSingleton(typeEmitterMock);

        services.AddSwaggerWithJsonRpc(Mock.Of<Assembly>(), info, setup.Object);

        var result = services.Select(static x => (x.ImplementationInstance, x.Lifetime)).ToList();
        result.Should().Contain((schemaGeneratorMock, ServiceLifetime.Singleton));
        result.Should().Contain((typeEmitterMock, ServiceLifetime.Singleton));
    }

    [Test]
    public void AddSwaggerWithJsonRpc_OtherDescriptionProviderAlreadyRegistered_RegisterJsonRpcProvider()
    {
        var services = new ServiceCollection();
        var info = new OpenApiInfo();
        var setup = new Mock<Action<SwaggerGenOptions>>();
        services.AddTransient<IApiDescriptionProvider, DefaultApiDescriptionProvider>();

        services.AddSwaggerWithJsonRpc(Mock.Of<Assembly>(), info, setup.Object);

        services.Should().Contain(static s => s.ImplementationType == typeof(DefaultApiDescriptionProvider));
        services.Should().Contain(static s => s.ImplementationType == typeof(JsonRpcDescriptionProvider));
    }

    [Test]
    public void AddSwaggerWithJsonRpc_JsonRpcDescriptionProviderAlreadyRegistered_DontRegisterAgain()
    {
        var services = new ServiceCollection();
        var info = new OpenApiInfo();
        var setup = new Mock<Action<SwaggerGenOptions>>();
        services.AddTransient<IApiDescriptionProvider, JsonRpcDescriptionProvider>();

        services.AddSwaggerWithJsonRpc(Mock.Of<Assembly>(), info, setup.Object);

        services.Where(static s => s.ImplementationType == typeof(JsonRpcDescriptionProvider)).Should().HaveCount(1);
    }

    [Test]
    public void AddSwaggerWithJsonRpc_CallSetupAction()
    {
        var services = new ServiceCollection();
        var info = new OpenApiInfo();
        var setup = new Mock<Action<SwaggerGenOptions>>();
        var xmlDocAssembly = Assembly.GetAssembly(typeof(WebApplicationMarker));

        services.AddSwaggerWithJsonRpc(xmlDocAssembly, info, setup.Object);
        var swaggerOptions = services.BuildServiceProvider().GetRequiredService<IOptions<SwaggerGenOptions>>().Value;

        setup.Verify(static x => x(It.IsAny<SwaggerGenOptions>()));
    }

    [Test]
    public void AddSwaggerWithJsonRpc_SerializerOptionsProvidersRegistered_AddSwaggerDocForDefaultAndAllOfThem()
    {
        var services = new ServiceCollection();
        var info = new OpenApiInfo
        {
            Title = "title",
            Version = "version",
            Description = "description"
        };
        var setup = new Mock<Action<SwaggerGenOptions>>();
        var xmlDocAssembly = Assembly.GetAssembly(typeof(WebApplicationMarker));
        services.AddSingleton<IJsonSerializerOptionsProvider, SnakeCaseJsonSerializerOptionsProvider>();
        services.AddSingleton<IJsonSerializerOptionsProvider, CamelCaseJsonSerializerOptionsProvider>();

        services.AddSwaggerWithJsonRpc(xmlDocAssembly, info, setup.Object);
        var swaggerOptions = services.BuildServiceProvider().GetRequiredService<IOptions<SwaggerGenOptions>>().Value;

        var expected = new Dictionary<string, OpenApiInfo>
        {
            [ApiExplorerConstants.DefaultDocumentName] = info,
            [$"{ApiExplorerConstants.DefaultDocumentName}_snakecase"] = new()
            {
                Title = info.Title,
                Version = info.Version,
                Description = $"Serializer: {nameof(SnakeCaseJsonSerializerOptionsProvider)}\n{info.Description}"
            },
            [$"{ApiExplorerConstants.DefaultDocumentName}_camelcase"] = new()
            {
                Title = info.Title,
                Version = info.Version,
                Description = $"Serializer: {nameof(CamelCaseJsonSerializerOptionsProvider)}\n{info.Description}"
            }
        };
        swaggerOptions.SwaggerGeneratorOptions.SwaggerDocs.Should().BeEquivalentTo(expected);
    }

    [Test]
    public void AddSwaggerWithJsonRpc_AddCustomDocumentSelector()
    {
        var services = new ServiceCollection();
        var info = new OpenApiInfo();
        var setup = new Mock<Action<SwaggerGenOptions>>();
        var xmlDocAssembly = Assembly.GetAssembly(typeof(WebApplicationMarker));

        services.AddSwaggerWithJsonRpc(xmlDocAssembly, info, setup.Object);
        var swaggerOptions = services.BuildServiceProvider().GetRequiredService<IOptions<SwaggerGenOptions>>().Value;

        swaggerOptions.SwaggerGeneratorOptions.DocInclusionPredicate.Should().Be(Extensions.DocumentSelector);
    }

    [Test]
    public void AddSwaggerWithJsonRpc_AddPropertiesFilter()
    {
        var services = new ServiceCollection();
        var info = new OpenApiInfo();
        var setup = new Mock<Action<SwaggerGenOptions>>();
        var xmlDocAssembly = Assembly.GetAssembly(typeof(WebApplicationMarker));

        services.AddSwaggerWithJsonRpc(xmlDocAssembly, info, setup.Object);
        var swaggerOptions = services.BuildServiceProvider().GetRequiredService<IOptions<SwaggerGenOptions>>().Value;

        swaggerOptions.SchemaFilterDescriptors.Should().Contain(static f => f.Type == typeof(JsonRpcPropertiesFilter));
    }

    [Test]
    public void AddSwaggerWithJsonRpc_AddCustomSchemaIdSelector()
    {
        var services = new ServiceCollection();
        var info = new OpenApiInfo();
        var setup = new Mock<Action<SwaggerGenOptions>>();
        var xmlDocAssembly = Assembly.GetAssembly(typeof(WebApplicationMarker));

        services.AddSwaggerWithJsonRpc(xmlDocAssembly, info, setup.Object);
        var swaggerOptions = services.BuildServiceProvider().GetRequiredService<IOptions<SwaggerGenOptions>>().Value;

        swaggerOptions.SchemaGeneratorOptions.SchemaIdSelector.Should().Be(Extensions.SchemaIdSelector);
    }

    [Test]
    public void AddSwaggerWithJsonRpc_XmlDocDoesntExist_Throw()
    {
        var services = new ServiceCollection();
        var info = new OpenApiInfo();
        var setup = new Mock<Action<SwaggerGenOptions>>();
        var xmlDocAssembly = Assembly.GetExecutingAssembly();

        services.AddSwaggerWithJsonRpc(xmlDocAssembly, info, setup.Object);
        var action = () => services.BuildServiceProvider().GetRequiredService<IOptions<SwaggerGenOptions>>().Value;

        action.Should().Throw<FileNotFoundException>();
    }

    [Test]
    public void AddSwaggerWithJsonRpc_XmlDocExists_IncludeXmlComments()
    {
        var services = new ServiceCollection();
        var info = new OpenApiInfo();
        var setup = new Mock<Action<SwaggerGenOptions>>();
        var xmlDocAssembly = Assembly.GetAssembly(typeof(WebApplicationMarker));

        services.AddSwaggerWithJsonRpc(xmlDocAssembly, info, setup.Object);
        var swaggerOptions = services.BuildServiceProvider().GetRequiredService<IOptions<SwaggerGenOptions>>().Value;

        // one of filters registered by calling IncludeXmlComments
        swaggerOptions.ParameterFilterDescriptors.Should().Contain(static f => f.Type == typeof(XmlCommentsParameterFilter));
    }

    [Test]
    public void AddSwaggerWithJsonRpc_AddNRTSupport()
    {
        var services = new ServiceCollection();
        var info = new OpenApiInfo();
        var setup = new Mock<Action<SwaggerGenOptions>>();
        var xmlDocAssembly = Assembly.GetAssembly(typeof(WebApplicationMarker));

        services.AddSwaggerWithJsonRpc(xmlDocAssembly, info, setup.Object);
        var swaggerOptions = services.BuildServiceProvider().GetRequiredService<IOptions<SwaggerGenOptions>>().Value;

        swaggerOptions.SchemaGeneratorOptions.SupportNonNullableReferenceTypes.Should().BeTrue();
    }

    [Test]
    public void AddSwaggerWithJsonRpc_UseDefaultInfo_GetInfoFromAssembly()
    {
        var services = new ServiceCollection();
        var setup = new Mock<Action<SwaggerGenOptions>>();
        var xmlDocAssembly = Assembly.GetAssembly(typeof(WebApplicationMarker));

        services.AddSwaggerWithJsonRpc(xmlDocAssembly, setup.Object);
        var swaggerOptions = services.BuildServiceProvider().GetRequiredService<IOptions<SwaggerGenOptions>>().Value;

        var expected = new Dictionary<string, OpenApiInfo>
        {
            [ApiExplorerConstants.DefaultDocumentName] = new()
            {
                Title = $"Tochka.JsonRpc.Tests.WebApplication {ApiExplorerConstants.DefaultDocumentTitle}",
                Version = ApiExplorerConstants.DefaultDocumentVersion,
                Description = "description for tests"
            }
        };
        swaggerOptions.SwaggerGeneratorOptions.SwaggerDocs.Should().BeEquivalentTo(expected);
    }

    [Test]
    public void JsonRpcSwaggerEndpoints_SerializerOptionsProvidersRegistered_AddEndpointsForDefaultAndAllOfThem()
    {
        var options = new SwaggerUIOptions();
        var services = new ServiceCollection();
        services.AddSingleton<IJsonSerializerOptionsProvider, SnakeCaseJsonSerializerOptionsProvider>();
        services.AddSingleton<IJsonSerializerOptionsProvider, CamelCaseJsonSerializerOptionsProvider>();
        var name = "name";

        options.JsonRpcSwaggerEndpoints(services.BuildServiceProvider(), name);

        var expected = new[]
        {
            new UrlDescriptor
            {
                Url = $"/swagger/{ApiExplorerConstants.DefaultDocumentName}/swagger.json",
                Name = name
            },
            new UrlDescriptor
            {
                Url = $"/swagger/{ApiExplorerConstants.DefaultDocumentName}_snakecase/swagger.json",
                Name = $"{name} snake_case"
            },
            new UrlDescriptor
            {
                Url = $"/swagger/{ApiExplorerConstants.DefaultDocumentName}_camelcase/swagger.json",
                Name = $"{name} camelCase"
            }
        };
        options.ConfigObject.Urls.Should().BeEquivalentTo(expected);
    }

    [Test]
    public void JsonRpcSwaggerEndpoints_SerializerOptionsProvidersWithSpecialNamingRegistered_AddEndpointsForDefaultAndAllOfThem()
    {
        var options = new SwaggerUIOptions();
        var services = new ServiceCollection();
        services.AddSingleton<IJsonSerializerOptionsProvider, JsonSerializerOptionsProviderKebabCase>();
        services.AddSingleton<IJsonSerializerOptionsProvider, KebabCaseUpperProvider>();
        var name = "name";

        options.JsonRpcSwaggerEndpoints(services.BuildServiceProvider(), name);

        var expected = new[]
        {
            new UrlDescriptor
            {
                Url = $"/swagger/{ApiExplorerConstants.DefaultDocumentName}/swagger.json",
                Name = name
            },
            new UrlDescriptor
            {
                Url = $"/swagger/{ApiExplorerConstants.DefaultDocumentName}_kebabcase/swagger.json",
                Name = $"{name} kebab-case"
            },
            new UrlDescriptor
            {
                Url = $"/swagger/{ApiExplorerConstants.DefaultDocumentName}_kebabcaseupperprovider/swagger.json",
                Name = $"{name} KEBAB-CASE-UPPER-PROVIDER"
            }
        };
        options.ConfigObject.Urls.Should().BeEquivalentTo(expected);
    }

    [Test]
    public void DocumentSelector_DocNameStartsWithDefaultAndEqualToGroupName_ReturnTrue()
    {
        var docName = $"{ApiExplorerConstants.DefaultDocumentName} suffix";
        var description = new ApiDescription { GroupName = docName };

        var result = Extensions.DocumentSelector(docName, description);

        result.Should().BeTrue();
    }

    [Test]
    public void DocumentSelector_DocNameStartsWithDefaultAndNotEqualToGroupName_ReturnFalse()
    {
        var docName = $"{ApiExplorerConstants.DefaultDocumentName} suffix";
        var description = new ApiDescription { GroupName = "groupName" };

        var result = Extensions.DocumentSelector(docName, description);

        result.Should().BeFalse();
    }

    [Test]
    public void DocumentSelector_GroupNameIsNull_ReturnTrue()
    {
        var docName = "docName";
        var description = new ApiDescription { GroupName = null };

        var result = Extensions.DocumentSelector(docName, description);

        result.Should().BeTrue();
    }

    [Test]
    public void DocumentSelector_DocNameEqualToGroupName_ReturnTrue()
    {
        var docName = "docName";
        var description = new ApiDescription { GroupName = docName };

        var result = Extensions.DocumentSelector(docName, description);

        result.Should().BeTrue();
    }

    [Test]
    public void SchemaIdSelector_GeneratedType_ReturnFullName()
    {
        var typeMock = new Mock<Type>();
        var fullName = "full.name";
        typeMock.Setup(static t => t.Assembly.FullName)
            .Returns($"{ApiExplorerConstants.GeneratedModelsAssemblyId}.{fullName}")
            .Verifiable();
        typeMock.Setup(static t => t.FullName)
            .Returns(fullName)
            .Verifiable();

        var result = Extensions.SchemaIdSelector(typeMock.Object);

        result.Should().Be(fullName);
        typeMock.Verify();
    }

    [Test]
    public void SchemaIdSelector_NormalType_ReturnShortName()
    {
        var typeMock = new Mock<Type>();
        var name = "name";
        typeMock.Setup(static t => t.Assembly.FullName)
            .Returns($"Some.Assembly.{name}")
            .Verifiable();
        typeMock.Setup(static t => t.Name)
            .Returns(name)
            .Verifiable();

        var result = Extensions.SchemaIdSelector(typeMock.Object);

        result.Should().Be(name);
        typeMock.Verify();
    }

    private class JsonSerializerOptionsProviderKebabCase : IJsonSerializerOptionsProvider
    {
        public JsonSerializerOptions Options => new() { PropertyNamingPolicy = JsonNamingPolicies.KebabCaseLower };
    }

    private class KebabCaseUpperProvider : IJsonSerializerOptionsProvider
    {
        public JsonSerializerOptions Options => new() { PropertyNamingPolicy = JsonNamingPolicies.KebabCaseUpper };
    }
}
