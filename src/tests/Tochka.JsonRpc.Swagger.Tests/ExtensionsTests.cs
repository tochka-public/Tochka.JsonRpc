using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using Asp.Versioning;
using Asp.Versioning.ApiExplorer;
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

namespace Tochka.JsonRpc.Swagger.Tests;

[TestFixture]
public class ExtensionsTests
{
    [Test]
    public void AddSwaggerWithJsonRpc_RegisterServices()
    {
        var services = new ServiceCollection();
        var setup = new Mock<Action<SwaggerGenOptions>>();

        services.AddSwaggerWithJsonRpc(Mock.Of<Assembly>(), setup.Object);

        var result = services.Select(static x => (x.ServiceType, x.ImplementationType, x.Lifetime)).ToList();
        // services defined in library
        result.Remove((typeof(ISchemaGenerator), typeof(JsonRpcSchemaGenerator), ServiceLifetime.Transient)).Should().BeTrue();
        result.Remove((typeof(ITypeEmitter), typeof(TypeEmitter), ServiceLifetime.Singleton)).Should().BeTrue();
        result.Remove((typeof(IApiDescriptionProvider), typeof(JsonRpcDescriptionProvider), ServiceLifetime.Transient)).Should().BeTrue();
        // one of services registered by calling AddSwaggerGen
        result.Remove((typeof(SwaggerGenerator), typeof(SwaggerGenerator), ServiceLifetime.Transient)).Should().BeTrue();
    }

    [Test]
    public void AddSwaggerWithJsonRpc_PreserveUserServices()
    {
        var services = new ServiceCollection();
        var setup = new Mock<Action<SwaggerGenOptions>>();
        var schemaGeneratorMock = Mock.Of<ISchemaGenerator>();
        var typeEmitterMock = Mock.Of<ITypeEmitter>();
        services.AddSingleton(schemaGeneratorMock);
        services.AddSingleton(typeEmitterMock);

        services.AddSwaggerWithJsonRpc(Mock.Of<Assembly>(), setup.Object);

        var result = services.Select(static x => (x.ImplementationInstance, x.Lifetime)).ToList();
        result.Should().Contain((schemaGeneratorMock, ServiceLifetime.Singleton));
        result.Should().Contain((typeEmitterMock, ServiceLifetime.Singleton));
    }

    [Test]
    public void AddSwaggerWithJsonRpc_OtherDescriptionProviderAlreadyRegistered_RegisterJsonRpcProvider()
    {
        var services = new ServiceCollection();
        var setup = new Mock<Action<SwaggerGenOptions>>();
        services.AddTransient<IApiDescriptionProvider, DefaultApiDescriptionProvider>();

        services.AddSwaggerWithJsonRpc(Mock.Of<Assembly>(), setup.Object);

        services.Should().Contain(static s => s.ImplementationType == typeof(DefaultApiDescriptionProvider));
        services.Should().Contain(static s => s.ImplementationType == typeof(JsonRpcDescriptionProvider));
    }

    [Test]
    public void AddSwaggerWithJsonRpc_JsonRpcDescriptionProviderAlreadyRegistered_DontRegisterAgain()
    {
        var services = new ServiceCollection();
        var setup = new Mock<Action<SwaggerGenOptions>>();
        services.AddTransient<IApiDescriptionProvider, JsonRpcDescriptionProvider>();

        services.AddSwaggerWithJsonRpc(Mock.Of<Assembly>(), setup.Object);

        services.Where(static s => s.ImplementationType == typeof(JsonRpcDescriptionProvider)).Should().HaveCount(1);
    }

    [Test]
    public void AddSwaggerWithJsonRpc_CallSetupAction()
    {
        var services = new ServiceCollection();
        var setup = new Mock<Action<SwaggerGenOptions>>();
        var xmlDocAssembly = Assembly.GetAssembly(typeof(WebApplicationMarker));

        services.AddSwaggerWithJsonRpc(xmlDocAssembly, setup.Object);
        var swaggerOptions = services.BuildServiceProvider().GetRequiredService<IOptions<SwaggerGenOptions>>().Value;

        setup.Verify(static x => x(It.IsAny<SwaggerGenOptions>()));
    }

    [Test]
    public void AddSwaggerWithJsonRpc_AddCustomDocumentSelector()
    {
        var services = new ServiceCollection();
        var setup = new Mock<Action<SwaggerGenOptions>>();
        var xmlDocAssembly = Assembly.GetAssembly(typeof(WebApplicationMarker));

        services.AddSwaggerWithJsonRpc(xmlDocAssembly, setup.Object);
        var swaggerOptions = services.BuildServiceProvider().GetRequiredService<IOptions<SwaggerGenOptions>>().Value;

#pragma warning disable CS8974 // Converting method group to non-delegate type
        swaggerOptions.SwaggerGeneratorOptions.DocInclusionPredicate.Should().Be(Extensions.DocumentSelector);
#pragma warning restore CS8974 // Converting method group to non-delegate type
    }

    [Test]
    public void AddSwaggerWithJsonRpc_AddPropertiesFilter()
    {
        var services = new ServiceCollection();
        var setup = new Mock<Action<SwaggerGenOptions>>();
        var xmlDocAssembly = Assembly.GetAssembly(typeof(WebApplicationMarker));

        services.AddSwaggerWithJsonRpc(xmlDocAssembly, setup.Object);
        var swaggerOptions = services.BuildServiceProvider().GetRequiredService<IOptions<SwaggerGenOptions>>().Value;

        swaggerOptions.SchemaFilterDescriptors.Should().Contain(static f => f.Type == typeof(JsonRpcPropertiesFilter));
    }

    [Test]
    public void AddSwaggerWithJsonRpc_AddCustomSchemaIdSelector()
    {
        var services = new ServiceCollection();
        var setup = new Mock<Action<SwaggerGenOptions>>();
        var xmlDocAssembly = Assembly.GetAssembly(typeof(WebApplicationMarker));

        services.AddSwaggerWithJsonRpc(xmlDocAssembly, setup.Object);
        var swaggerOptions = services.BuildServiceProvider().GetRequiredService<IOptions<SwaggerGenOptions>>().Value;

#pragma warning disable CS8974 // Converting method group to non-delegate type
        swaggerOptions.SchemaGeneratorOptions.SchemaIdSelector.Should().Be(Extensions.SchemaIdSelector);
#pragma warning restore CS8974 // Converting method group to non-delegate type
    }

    [Test]
    public void AddSwaggerWithJsonRpc_XmlDocDoesntExist_Throw()
    {
        var services = new ServiceCollection();
        var setup = new Mock<Action<SwaggerGenOptions>>();
        var xmlDocAssembly = Assembly.GetExecutingAssembly();

        services.AddSwaggerWithJsonRpc(xmlDocAssembly, setup.Object);
        var action = () => services.BuildServiceProvider().GetRequiredService<IOptions<SwaggerGenOptions>>().Value;

        action.Should().Throw<FileNotFoundException>();
    }

    [Test]
    public void AddSwaggerWithJsonRpc_XmlDocExists_IncludeXmlComments()
    {
        var services = new ServiceCollection();
        var setup = new Mock<Action<SwaggerGenOptions>>();
        var xmlDocAssembly = Assembly.GetAssembly(typeof(WebApplicationMarker));

        services.AddSwaggerWithJsonRpc(xmlDocAssembly, setup.Object);
        var swaggerOptions = services.BuildServiceProvider().GetRequiredService<IOptions<SwaggerGenOptions>>().Value;

        // one of filters registered by calling IncludeXmlComments
        swaggerOptions.ParameterFilterDescriptors.Should().Contain(static f => f.Type == typeof(XmlCommentsParameterFilter));
    }

    [Test]
    public void AddSwaggerWithJsonRpc_AddNRTSupport()
    {
        var services = new ServiceCollection();
        var setup = new Mock<Action<SwaggerGenOptions>>();
        var xmlDocAssembly = Assembly.GetAssembly(typeof(WebApplicationMarker));

        services.AddSwaggerWithJsonRpc(xmlDocAssembly, setup.Object);
        var swaggerOptions = services.BuildServiceProvider().GetRequiredService<IOptions<SwaggerGenOptions>>().Value;

        swaggerOptions.SchemaGeneratorOptions.SupportNonNullableReferenceTypes.Should().BeTrue();
    }

    [Test]
    public void AddSwaggerWithJsonRpc_OverloadWithoutSetup_AddSwaggerDocForDefaultAndAllVersionsAndSerializersWithInfoFromAssembly()
    {
        var services = new ServiceCollection();
        var xmlDocAssembly = Assembly.GetAssembly(typeof(WebApplicationMarker));
        services.AddSingleton<IJsonSerializerOptionsProvider, SnakeCaseJsonSerializerOptionsProvider>();
        services.AddSingleton<IJsonSerializerOptionsProvider, CamelCaseJsonSerializerOptionsProvider>();
        var versionDescriptions = new List<ApiVersionDescription>
        {
            new(new ApiVersion(1, 0), "v1"),
            new(new ApiVersion(2, 0), "v2")
        };
        var apiVersionDescriptionProviderMock = new Mock<IApiVersionDescriptionProvider>();
        apiVersionDescriptionProviderMock.Setup(static p => p.ApiVersionDescriptions)
            .Returns(versionDescriptions)
            .Verifiable();
        services.AddSingleton(apiVersionDescriptionProviderMock.Object);

        services.AddSwaggerWithJsonRpc(xmlDocAssembly);
        var swaggerOptions = services.BuildServiceProvider().GetRequiredService<IOptions<SwaggerGenOptions>>().Value;

        var expectedTitle = $"Tochka.JsonRpc.Tests.WebApplication {ApiExplorerConstants.DefaultDocumentTitle}";
        var expectedDescription = "description for tests";
        var expected = new Dictionary<string, OpenApiInfo>
        {
            [$"{ApiExplorerConstants.DefaultDocumentName}_{versionDescriptions[0].GroupName}"] = new()
            {
                Title = expectedTitle,
                Description = expectedDescription,
                Version = versionDescriptions[0].ApiVersion.ToString()
            },
            [$"{ApiExplorerConstants.DefaultDocumentName}_snakecase_{versionDescriptions[0].GroupName}"] = new()
            {
                Title = expectedTitle,
                Description = $"Serializer: {nameof(SnakeCaseJsonSerializerOptionsProvider)}\n{expectedDescription}",
                Version = versionDescriptions[0].ApiVersion.ToString()
            },
            [$"{ApiExplorerConstants.DefaultDocumentName}_camelcase_{versionDescriptions[0].GroupName}"] = new()
            {
                Title = expectedTitle,
                Description = $"Serializer: {nameof(CamelCaseJsonSerializerOptionsProvider)}\n{expectedDescription}",
                Version = versionDescriptions[0].ApiVersion.ToString()
            },
            [$"{ApiExplorerConstants.DefaultDocumentName}_{versionDescriptions[1].GroupName}"] = new()
            {
                Title = expectedTitle,
                Description = expectedDescription,
                Version = versionDescriptions[1].ApiVersion.ToString()
            },
            [$"{ApiExplorerConstants.DefaultDocumentName}_snakecase_{versionDescriptions[1].GroupName}"] = new()
            {
                Title = expectedTitle,
                Description = $"Serializer: {nameof(SnakeCaseJsonSerializerOptionsProvider)}\n{expectedDescription}",
                Version = versionDescriptions[1].ApiVersion.ToString()
            },
            [$"{ApiExplorerConstants.DefaultDocumentName}_camelcase_{versionDescriptions[1].GroupName}"] = new()
            {
                Title = expectedTitle,
                Description = $"Serializer: {nameof(CamelCaseJsonSerializerOptionsProvider)}\n{expectedDescription}",
                Version = versionDescriptions[1].ApiVersion.ToString()
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
        var versionDescriptions = new List<ApiVersionDescription>
        {
            new(new ApiVersion(1, 0), "v1"),
            new(new ApiVersion(2, 0), "v2")
        };
        var apiVersionDescriptionProviderMock = new Mock<IApiVersionDescriptionProvider>();
        apiVersionDescriptionProviderMock.Setup(static p => p.ApiVersionDescriptions)
            .Returns(versionDescriptions)
            .Verifiable();
        services.AddSingleton(apiVersionDescriptionProviderMock.Object);

        options.JsonRpcSwaggerEndpoints(services.BuildServiceProvider(), name);

        var expected = new[]
        {
            new UrlDescriptor
            {
                Url = $"/swagger/{ApiExplorerConstants.DefaultDocumentName}_{versionDescriptions[0].GroupName}/swagger.json",
                Name = $"{name} {versionDescriptions[0].ApiVersion.ToString()}"
            },
            new UrlDescriptor
            {
                Url = $"/swagger/{ApiExplorerConstants.DefaultDocumentName}_snakecase_{versionDescriptions[0].GroupName}/swagger.json",
                Name = $"{name} snake_case {versionDescriptions[0].ApiVersion.ToString()}"
            },
            new UrlDescriptor
            {
                Url = $"/swagger/{ApiExplorerConstants.DefaultDocumentName}_camelcase_{versionDescriptions[0].GroupName}/swagger.json",
                Name = $"{name} camelCase {versionDescriptions[0].ApiVersion.ToString()}"
            },
            new UrlDescriptor
            {
                Url = $"/swagger/{ApiExplorerConstants.DefaultDocumentName}_{versionDescriptions[1].GroupName}/swagger.json",
                Name = $"{name} {versionDescriptions[1].ApiVersion.ToString()}"
            },
            new UrlDescriptor
            {
                Url = $"/swagger/{ApiExplorerConstants.DefaultDocumentName}_snakecase_{versionDescriptions[1].GroupName}/swagger.json",
                Name = $"{name} snake_case {versionDescriptions[1].ApiVersion.ToString()}"
            },
            new UrlDescriptor
            {
                Url = $"/swagger/{ApiExplorerConstants.DefaultDocumentName}_camelcase_{versionDescriptions[1].GroupName}/swagger.json",
                Name = $"{name} camelCase {versionDescriptions[1].ApiVersion.ToString()}"
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
        var versionDescriptions = new List<ApiVersionDescription>
        {
            new(new ApiVersion(1, 0), "v1"),
            new(new ApiVersion(2, 0), "v2")
        };
        var apiVersionDescriptionProviderMock = new Mock<IApiVersionDescriptionProvider>();
        apiVersionDescriptionProviderMock.Setup(static p => p.ApiVersionDescriptions)
            .Returns(versionDescriptions)
            .Verifiable();
        services.AddSingleton(apiVersionDescriptionProviderMock.Object);

        options.JsonRpcSwaggerEndpoints(services.BuildServiceProvider(), name);

        var expected = new[]
        {
            new UrlDescriptor
            {
                Url = $"/swagger/{ApiExplorerConstants.DefaultDocumentName}_{versionDescriptions[0].GroupName}/swagger.json",
                Name = $"{name} {versionDescriptions[0].ApiVersion.ToString()}"
            },
            new UrlDescriptor
            {
                Url = $"/swagger/{ApiExplorerConstants.DefaultDocumentName}_kebabcase_{versionDescriptions[0].GroupName}/swagger.json",
                Name = $"{name} kebab-case {versionDescriptions[0].ApiVersion.ToString()}"
            },
            new UrlDescriptor
            {
                Url = $"/swagger/{ApiExplorerConstants.DefaultDocumentName}_kebabcaseupperprovider_{versionDescriptions[0].GroupName}/swagger.json",
                Name = $"{name} KEBAB-CASE-UPPER-PROVIDER {versionDescriptions[0].ApiVersion.ToString()}"
            },
            new UrlDescriptor
            {
                Url = $"/swagger/{ApiExplorerConstants.DefaultDocumentName}_{versionDescriptions[1].GroupName}/swagger.json",
                Name = $"{name} {versionDescriptions[1].ApiVersion.ToString()}"
            },
            new UrlDescriptor
            {
                Url = $"/swagger/{ApiExplorerConstants.DefaultDocumentName}_kebabcase_{versionDescriptions[1].GroupName}/swagger.json",
                Name = $"{name} kebab-case {versionDescriptions[1].ApiVersion.ToString()}"
            },
            new UrlDescriptor
            {
                Url = $"/swagger/{ApiExplorerConstants.DefaultDocumentName}_kebabcaseupperprovider_{versionDescriptions[1].GroupName}/swagger.json",
                Name = $"{name} KEBAB-CASE-UPPER-PROVIDER {versionDescriptions[1].ApiVersion.ToString()}"
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
            .Returns($"{ApiExplorerConstants.GeneratedModelsAssemblyName}.{fullName}")
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

    private sealed class JsonSerializerOptionsProviderKebabCase : IJsonSerializerOptionsProvider
    {
        public JsonSerializerOptions Options => new() { PropertyNamingPolicy = JsonNamingPolicy.KebabCaseLower };
    }

    private sealed class KebabCaseUpperProvider : IJsonSerializerOptionsProvider
    {
        public JsonSerializerOptions Options => new() { PropertyNamingPolicy = JsonNamingPolicy.KebabCaseUpper };
    }
}
