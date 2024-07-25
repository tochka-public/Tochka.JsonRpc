using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using FluentAssertions;
using JetBrains.Annotations;
using Json.Schema;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Namotion.Reflection;
using NUnit.Framework;
using Tochka.JsonRpc.ApiExplorer;
using Tochka.JsonRpc.Common;
using Tochka.JsonRpc.Common.Models.Id;
using Tochka.JsonRpc.Common.Models.Request;
using Tochka.JsonRpc.Common.Models.Response;
using Tochka.JsonRpc.OpenRpc.Models;
using Tochka.JsonRpc.OpenRpc.Services;
using Tochka.JsonRpc.Server.Attributes;
using Tochka.JsonRpc.Server.Metadata;
using Tochka.JsonRpc.Server.Serialization;
using Tochka.JsonRpc.Server.Settings;

namespace Tochka.JsonRpc.OpenRpc.Tests.Services;

[TestFixture]
public class OpenRpcDocumentGeneratorTests
{
    private Mock<IApiDescriptionGroupCollectionProvider> apiDescriptionsProviderMock;
    private Mock<IOpenRpcSchemaGenerator> schemaGeneratorMock;
    private Mock<IOpenRpcContentDescriptorGenerator> contentDescriptorGeneratorMock;
    private List<IJsonSerializerOptionsProvider> jsonSerializerOptionsProviders;
    private JsonRpcServerOptions serverOptions;
    private OpenRpcOptions openRpcOptions;

    private Mock<OpenRpcDocumentGenerator> documentGeneratorMock;

    [SetUp]
    public void Setup()
    {
        apiDescriptionsProviderMock = new Mock<IApiDescriptionGroupCollectionProvider>();
        schemaGeneratorMock = new Mock<IOpenRpcSchemaGenerator>();
        contentDescriptorGeneratorMock = new Mock<IOpenRpcContentDescriptorGenerator>();
        jsonSerializerOptionsProviders = new List<IJsonSerializerOptionsProvider>();
        serverOptions = new JsonRpcServerOptions();
        openRpcOptions = new OpenRpcOptions();

        documentGeneratorMock = new Mock<OpenRpcDocumentGenerator>(apiDescriptionsProviderMock.Object, schemaGeneratorMock.Object, contentDescriptorGeneratorMock.Object, jsonSerializerOptionsProviders, Options.Create(serverOptions), Options.Create(openRpcOptions), Mock.Of<ILogger<OpenRpcDocumentGenerator>>())
        {
            CallBase = true
        };
    }

    [Test]
    public void Generate_UseArgsAndInternalMethods()
    {
        var info = new OpenRpcInfo("title", "version");
        var host = new Uri(Host);
        var servers = new List<OpenRpcServer>();
        var methods = new List<OpenRpcMethod>();
        var schemas = new Dictionary<string, JsonSchema>();
        var tags = new Dictionary<string, OpenRpcTag>();
        documentGeneratorMock.Setup(g => g.GetServers(host, serverOptions.RoutePrefix))
            .Returns(servers)
            .Verifiable();
        documentGeneratorMock.Setup(g => g.GetMethods(DocumentName, host, tags))
            .Returns(methods)
            .Verifiable();
        schemaGeneratorMock.Setup(static g => g.GetAllSchemas())
            .Returns(schemas)
            .Verifiable();
        documentGeneratorMock.Setup(static g => g.GetControllersTags())
            .Returns(tags)
            .Verifiable();

        var result = documentGeneratorMock.Object.Generate(info, DocumentName, host);

        var expected = new Models.OpenRpc(info)
        {
            Servers = servers,
            Methods = methods,
            Components = new()
            {
                Schemas = schemas,
                Tags = tags
            }
        };
        result.Should().BeEquivalentTo(expected);
        documentGeneratorMock.Verify();
    }

    [Test]
    public void GetServers_ReturnsOneServerWithDefaultNameAndUrl()
    {
        var host = new Uri(Host);
        var route = "route";

        var result = documentGeneratorMock.Object.GetServers(host, route);

        var expected = new OpenRpcServer(openRpcOptions.DefaultServerName, new Uri($"{Host}/{route}"));
        result.Should().HaveCount(1);
        result.Single().Should().BeEquivalentTo(expected);
    }

    [Test]
    public void GetMethods_IgnoreObsoleteActionsTrueInOptions_ExcludeActionsWithObsoleteAttributeOrFromObsoleteType()
    {
        var host = new Uri(Host);
        var apiDescription1 = GetValidDescription();
#pragma warning disable CS0612
        var apiDescription2 = GetValidDescription(ObsoleteMethod);
        var apiDescription3 = GetValidDescription(ObsoleteType.ObsoleteTypeMethod);
#pragma warning restore CS0612
        apiDescriptionsProviderMock.Setup(static p => p.ApiDescriptionGroups)
            .Returns(new ApiDescriptionGroupCollection(new List<ApiDescriptionGroup>
                {
                    new(null, new[] { apiDescription1 }),
                    new(null, new[] { apiDescription2 }),
                    new(null, new[] { apiDescription3 })
                },
                0))
            .Verifiable();
        var method = new OpenRpcMethod("name");
        var tags = new Dictionary<string, OpenRpcTag>();
        documentGeneratorMock.Setup(g => g.GetMethod(apiDescription1, host, tags))
            .Returns(method)
            .Verifiable();
        openRpcOptions.DocInclusionPredicate = static (_, _) => true;
        openRpcOptions.IgnoreObsoleteActions = true;

        var result = documentGeneratorMock.Object.GetMethods(DocumentName, host, tags);

        var expected = new[]
        {
            method
        };
        result.Should().BeEquivalentTo(expected);
        apiDescriptionsProviderMock.Verify();
        documentGeneratorMock.Verify();
    }

    [Test]
    public void GetMethods_IgnoreObsoleteActionsFalseInOptions_IncludeActionsWithObsoleteAttributeAndFromObsoleteType()
    {
        var host = new Uri(Host);
        var apiDescription1 = GetValidDescription();
#pragma warning disable CS0612
        var apiDescription2 = GetValidDescription(ObsoleteMethod);
        var apiDescription3 = GetValidDescription(ObsoleteType.ObsoleteTypeMethod);
#pragma warning restore CS0612
        apiDescriptionsProviderMock.Setup(static p => p.ApiDescriptionGroups)
            .Returns(new ApiDescriptionGroupCollection(new List<ApiDescriptionGroup>
                {
                    new(null, new[] { apiDescription1 }),
                    new(null, new[] { apiDescription2 }),
                    new(null, new[] { apiDescription3 })
                },
                0))
            .Verifiable();
        var method1 = new OpenRpcMethod("name1");
        var method2 = new OpenRpcMethod("name2");
        var method3 = new OpenRpcMethod("name3");
        var tags = new Dictionary<string, OpenRpcTag>();
        documentGeneratorMock.Setup(g => g.GetMethod(apiDescription1, host, tags))
            .Returns(method1)
            .Verifiable();
        documentGeneratorMock.Setup(g => g.GetMethod(apiDescription2, host, tags))
            .Returns(method2)
            .Verifiable();
        documentGeneratorMock.Setup(g => g.GetMethod(apiDescription3, host, tags))
            .Returns(method3)
            .Verifiable();
        openRpcOptions.DocInclusionPredicate = static (_, _) => true;
        openRpcOptions.IgnoreObsoleteActions = false;

        var result = documentGeneratorMock.Object.GetMethods(DocumentName, host, tags);

        var expected = new[]
        {
            method1,
            method2,
            method3
        };
        result.Should().BeEquivalentTo(expected);
        apiDescriptionsProviderMock.Verify();
        documentGeneratorMock.Verify();
    }

    [Test]
    public void GetMethods_MethodNotFromJsonRpcController_ExcludeActions()
    {
        var host = new Uri(Host);
        var apiDescription1 = GetValidDescription();
        var apiDescription2 = GetValidDescription();
        apiDescription2.ActionDescriptor.EndpointMetadata = new List<object>();
        apiDescriptionsProviderMock.Setup(static p => p.ApiDescriptionGroups)
            .Returns(new ApiDescriptionGroupCollection(new List<ApiDescriptionGroup>
                {
                    new(null, new[] { apiDescription1 }),
                    new(null, new[] { apiDescription2 })
                },
                0))
            .Verifiable();
        var method = new OpenRpcMethod("name");
        var tags = new Dictionary<string, OpenRpcTag>();
        documentGeneratorMock.Setup(g => g.GetMethod(apiDescription1, host, tags))
            .Returns(method)
            .Verifiable();
        openRpcOptions.DocInclusionPredicate = static (_, _) => true;

        var result = documentGeneratorMock.Object.GetMethods(DocumentName, host, tags);

        var expected = new[]
        {
            method
        };
        result.Should().BeEquivalentTo(expected);
        apiDescriptionsProviderMock.Verify();
        documentGeneratorMock.Verify();
    }

    [Test]
    public void GetMethods_DocInclusionPredicateReturnsFalse_ExcludeActions()
    {
        var host = new Uri(Host);
        var apiDescription1 = GetValidDescription();
        var apiDescription2 = GetValidDescription();
        apiDescriptionsProviderMock.Setup(static p => p.ApiDescriptionGroups)
            .Returns(new ApiDescriptionGroupCollection(new List<ApiDescriptionGroup>
                {
                    new(null, new[] { apiDescription1 }),
                    new(null, new[] { apiDescription2 })
                },
                0))
            .Verifiable();
        var method = new OpenRpcMethod("name");
        var tags = new Dictionary<string, OpenRpcTag>();
        documentGeneratorMock.Setup(g => g.GetMethod(apiDescription1, host, tags))
            .Returns(method)
            .Verifiable();
        openRpcOptions.DocInclusionPredicate = (_, d) => d == apiDescription1;

        var result = documentGeneratorMock.Object.GetMethods(DocumentName, host, tags);

        var expected = new[]
        {
            method
        };
        result.Should().BeEquivalentTo(expected);
        apiDescriptionsProviderMock.Verify();
        documentGeneratorMock.Verify();
    }

    [Test]
    public void GetMethods_OrderMethodsByName()
    {
        var host = new Uri(Host);
        var apiDescription1 = GetValidDescription();
        var apiDescription2 = GetValidDescription();
        apiDescriptionsProviderMock.Setup(static p => p.ApiDescriptionGroups)
            .Returns(new ApiDescriptionGroupCollection(new List<ApiDescriptionGroup>
                {
                    new(null, new[] { apiDescription2 }),
                    new(null, new[] { apiDescription1 })
                },
                0))
            .Verifiable();
        var method1 = new OpenRpcMethod("a");
        var method2 = new OpenRpcMethod("b");
        var tags = new Dictionary<string, OpenRpcTag>();
        documentGeneratorMock.Setup(g => g.GetMethod(apiDescription1, host, tags))
            .Returns(method1)
            .Verifiable();
        documentGeneratorMock.Setup(g => g.GetMethod(apiDescription2, host, tags))
            .Returns(method2)
            .Verifiable();
        openRpcOptions.DocInclusionPredicate = static (_, _) => true;

        var result = documentGeneratorMock.Object.GetMethods(DocumentName, host, tags);

        var expected = new[]
        {
            method1,
            method2
        };
        result.Should().BeEquivalentTo(expected, static options => options.WithStrictOrdering());
        apiDescriptionsProviderMock.Verify();
        documentGeneratorMock.Verify();
    }

    [Test]
    public void GetMethod_UseInternalMethods()
    {
        var description = GetValidDescription();
        var host = new Uri(Host);
        var methodParams = new List<OpenRpcContentDescriptor>();
        var methodResult = new OpenRpcContentDescriptor("name", JsonSchema.Empty);
        var methodServers = new List<OpenRpcServer>();
        var methodParamsStructure = new OpenRpcParamStructure();
        var parametersMetadata = new JsonRpcActionParametersMetadata();
        var tags = new Dictionary<string, OpenRpcTag>();
        description.ActionDescriptor.EndpointMetadata.Add(parametersMetadata);
        documentGeneratorMock.Setup(g => g.GetMethodParams(description, MethodName, parametersMetadata, serverOptions.DefaultDataJsonSerializerOptions))
            .Returns(methodParams)
            .Verifiable();
        documentGeneratorMock.Setup(g => g.GetResultContentDescriptor(description, MethodName, serverOptions.DefaultDataJsonSerializerOptions))
            .Returns(methodResult)
            .Verifiable();
        documentGeneratorMock.Setup(g => g.GetMethodServers(description, host))
            .Returns(methodServers)
            .Verifiable();
        documentGeneratorMock.Setup(g => g.GetParamsStructure(parametersMetadata))
            .Returns(methodParamsStructure)
            .Verifiable();

        var result = documentGeneratorMock.Object.GetMethod(description, host, tags);

        var expected = new OpenRpcMethod(MethodName)
        {
            Summary = "",
            Description = "",
            Params = methodParams,
            Result = methodResult,
            Deprecated = false,
            Servers = methodServers,
            ParamStructure = methodParamsStructure
        };
        result.Should().BeEquivalentTo(expected);
        documentGeneratorMock.Verify();
    }

    [Test]
    public void GetMethod_ActionHasCustomSerializerOptions_UseSerializerOptionsFromAttribute()
    {
        var description = GetValidDescription();
        var host = new Uri(Host);
        var methodParams = new List<OpenRpcContentDescriptor>();
        var methodResult = new OpenRpcContentDescriptor("name", JsonSchema.Empty);
        var methodServers = new List<OpenRpcServer>();
        var methodParamsStructure = new OpenRpcParamStructure();
        var parametersMetadata = new JsonRpcActionParametersMetadata();
        var tags = new Dictionary<string, OpenRpcTag>();
        var serializerOptionsProvider = new SnakeCaseJsonSerializerOptionsProvider();
        description.ActionDescriptor.EndpointMetadata.Add(parametersMetadata);
        description.ActionDescriptor.EndpointMetadata.Add(new JsonRpcSerializerOptionsAttribute(serializerOptionsProvider.GetType()));
        jsonSerializerOptionsProviders.Add(serializerOptionsProvider);
        documentGeneratorMock.Setup(g => g.GetMethodParams(description, MethodName, parametersMetadata, serializerOptionsProvider.Options))
            .Returns(methodParams)
            .Verifiable();
        documentGeneratorMock.Setup(g => g.GetResultContentDescriptor(description, MethodName, serializerOptionsProvider.Options))
            .Returns(methodResult)
            .Verifiable();
        documentGeneratorMock.Setup(g => g.GetMethodServers(description, host))
            .Returns(methodServers)
            .Verifiable();
        documentGeneratorMock.Setup(g => g.GetParamsStructure(parametersMetadata))
            .Returns(methodParamsStructure)
            .Verifiable();

        var result = documentGeneratorMock.Object.GetMethod(description, host, tags);

        var expected = new OpenRpcMethod(MethodName)
        {
            Summary = "",
            Description = "",
            Params = methodParams,
            Result = methodResult,
            Deprecated = false,
            Servers = methodServers,
            ParamStructure = methodParamsStructure
        };
        result.Should().BeEquivalentTo(expected);
        documentGeneratorMock.Verify();
    }

    [Test]
    public void GetMethod_MethodHasXmlDocs_UseSummaryAndRemarks()
    {
        var description = GetValidDescription(MethodWithDocs);
        var host = new Uri(Host);
        var methodParams = new List<OpenRpcContentDescriptor>();
        var methodResult = new OpenRpcContentDescriptor("name", JsonSchema.Empty);
        var methodServers = new List<OpenRpcServer>();
        var methodParamsStructure = new OpenRpcParamStructure();
        var parametersMetadata = new JsonRpcActionParametersMetadata();
        var tags = new Dictionary<string, OpenRpcTag>();
        description.ActionDescriptor.EndpointMetadata.Add(parametersMetadata);
        documentGeneratorMock.Setup(g => g.GetMethodParams(description, MethodName, parametersMetadata, serverOptions.DefaultDataJsonSerializerOptions))
            .Returns(methodParams)
            .Verifiable();
        documentGeneratorMock.Setup(g => g.GetResultContentDescriptor(description, MethodName, serverOptions.DefaultDataJsonSerializerOptions))
            .Returns(methodResult)
            .Verifiable();
        documentGeneratorMock.Setup(g => g.GetMethodServers(description, host))
            .Returns(methodServers)
            .Verifiable();
        documentGeneratorMock.Setup(g => g.GetParamsStructure(parametersMetadata))
            .Returns(methodParamsStructure)
            .Verifiable();

        var result = documentGeneratorMock.Object.GetMethod(description, host, tags);

        var expected = new OpenRpcMethod(MethodName)
        {
            Summary = "summary",
            Description = "description",
            Params = methodParams,
            Result = methodResult,
            Deprecated = false,
            Servers = methodServers,
            ParamStructure = methodParamsStructure
        };
        result.Should().BeEquivalentTo(expected);
        documentGeneratorMock.Verify();
    }

    [Test]
    public void GetMethod_MethodHasObsoleteAttribute_MarkAsDeprecated()
    {
#pragma warning disable CS0612
        var description = GetValidDescription(ObsoleteMethod);
#pragma warning restore CS0612
        var host = new Uri(Host);
        var methodParams = new List<OpenRpcContentDescriptor>();
        var methodResult = new OpenRpcContentDescriptor("name", JsonSchema.Empty);
        var methodServers = new List<OpenRpcServer>();
        var methodParamsStructure = new OpenRpcParamStructure();
        var parametersMetadata = new JsonRpcActionParametersMetadata();
        var tags = new Dictionary<string, OpenRpcTag>();
        description.ActionDescriptor.EndpointMetadata.Add(parametersMetadata);
        documentGeneratorMock.Setup(g => g.GetMethodParams(description, MethodName, parametersMetadata, serverOptions.DefaultDataJsonSerializerOptions))
            .Returns(methodParams)
            .Verifiable();
        documentGeneratorMock.Setup(g => g.GetResultContentDescriptor(description, MethodName, serverOptions.DefaultDataJsonSerializerOptions))
            .Returns(methodResult)
            .Verifiable();
        documentGeneratorMock.Setup(g => g.GetMethodServers(description, host))
            .Returns(methodServers)
            .Verifiable();
        documentGeneratorMock.Setup(g => g.GetParamsStructure(parametersMetadata))
            .Returns(methodParamsStructure)
            .Verifiable();

        var result = documentGeneratorMock.Object.GetMethod(description, host, tags);

        var expected = new OpenRpcMethod(MethodName)
        {
            Summary = "",
            Description = "",
            Params = methodParams,
            Result = methodResult,
            Deprecated = true,
            Servers = methodServers,
            ParamStructure = methodParamsStructure
        };
        result.Should().BeEquivalentTo(expected);
        documentGeneratorMock.Verify();
    }

    [Test]
    public void GetMethod_MethodFromTypeWithObsoleteAttribute_MarkAsDeprecated()
    {
#pragma warning disable CS0612
        var description = GetValidDescription(ObsoleteType.ObsoleteTypeMethod);
#pragma warning restore CS0612
        var host = new Uri(Host);
        var methodParams = new List<OpenRpcContentDescriptor>();
        var methodResult = new OpenRpcContentDescriptor("name", JsonSchema.Empty);
        var methodServers = new List<OpenRpcServer>();
        var methodParamsStructure = new OpenRpcParamStructure();
        var parametersMetadata = new JsonRpcActionParametersMetadata();
        var tags = new Dictionary<string, OpenRpcTag>();
        description.ActionDescriptor.EndpointMetadata.Add(parametersMetadata);
        documentGeneratorMock.Setup(g => g.GetMethodParams(description, MethodName, parametersMetadata, serverOptions.DefaultDataJsonSerializerOptions))
            .Returns(methodParams)
            .Verifiable();
        documentGeneratorMock.Setup(g => g.GetResultContentDescriptor(description, MethodName, serverOptions.DefaultDataJsonSerializerOptions))
            .Returns(methodResult)
            .Verifiable();
        documentGeneratorMock.Setup(g => g.GetMethodServers(description, host))
            .Returns(methodServers)
            .Verifiable();
        documentGeneratorMock.Setup(g => g.GetParamsStructure(parametersMetadata))
            .Returns(methodParamsStructure)
            .Verifiable();

        var result = documentGeneratorMock.Object.GetMethod(description, host, tags);

        var expected = new OpenRpcMethod(MethodName)
        {
            Summary = "",
            Description = "",
            Params = methodParams,
            Result = methodResult,
            Deprecated = true,
            Servers = methodServers,
            ParamStructure = methodParamsStructure
        };
        result.Should().BeEquivalentTo(expected);
        documentGeneratorMock.Verify();
    }

    [Test]
    public void GetMethodParams_BodyIsRequestWithCollectionParams_GenerateForCollectionItemType()
    {
        var description = GetValidDescription();
        var parametersMetadata = new JsonRpcActionParametersMetadata();
        var jsonSerializerOptions = new JsonSerializerOptions();
        var type = typeof(Request<IEnumerable<TypeWithProperties>>);
        var itemType = typeof(TypeWithProperties).ToContextualType();
        var contentDescriptor = new OpenRpcContentDescriptor("name", JsonSchema.Empty);
        description.ParameterDescriptions.Add(new ApiParameterDescription { Source = BindingSource.Body, Type = type });
        contentDescriptorGeneratorMock.Setup(g => g.GenerateForType(itemType, MethodName, jsonSerializerOptions))
            .Returns(contentDescriptor)
            .Verifiable();

        var result = documentGeneratorMock.Object.GetMethodParams(description, MethodName, parametersMetadata, jsonSerializerOptions);

        var expected = new[]
        {
            contentDescriptor
        };
        result.Should().BeEquivalentTo(expected);
        contentDescriptorGeneratorMock.Verify();
        contentDescriptorGeneratorMock.VerifyNoOtherCalls();
    }

    [Test]
    public void GetMethodParams_BodyInheritedFromRequestWithCollectionParams_GenerateForCollectionItemType()
    {
        var description = GetValidDescription();
        var parametersMetadata = new JsonRpcActionParametersMetadata();
        var jsonSerializerOptions = new JsonSerializerOptions();
        var type = typeof(RequestChild<IEnumerable<TypeWithProperties>>);
        var itemType = typeof(TypeWithProperties).ToContextualType();
        var contentDescriptor = new OpenRpcContentDescriptor("name", JsonSchema.Empty);
        description.ParameterDescriptions.Add(new ApiParameterDescription { Source = BindingSource.Body, Type = type });
        contentDescriptorGeneratorMock.Setup(g => g.GenerateForType(itemType, MethodName, jsonSerializerOptions))
            .Returns(contentDescriptor)
            .Verifiable();

        var result = documentGeneratorMock.Object.GetMethodParams(description, MethodName, parametersMetadata, jsonSerializerOptions);

        var expected = new[]
        {
            contentDescriptor
        };
        result.Should().BeEquivalentTo(expected);
        contentDescriptorGeneratorMock.Verify();
        contentDescriptorGeneratorMock.VerifyNoOtherCalls();
    }

    [Test]
    public void GetMethodParams_BodyIsRequest_UseGeneratorBasedOnMetadata()
    {
        var description = GetValidDescription();
        var parameterMetadata = new JsonRpcParameterMetadata("propertyName", 0, BindingStyle.Default, false, "originalName", typeof(int));
        var parametersMetadata = new JsonRpcActionParametersMetadata { Parameters = { [nameof(TypeWithProperties.IntProperty)] = parameterMetadata } };
        var jsonSerializerOptions = new JsonSerializerOptions();
        var type = typeof(Request<TypeWithProperties>);
        var intPropertyInfo = typeof(TypeWithProperties).GetProperty(nameof(TypeWithProperties.IntProperty)).ToContextualProperty();
        var stringPropertyInfo = typeof(TypeWithProperties).GetProperty(nameof(TypeWithProperties.StringProperty)).ToContextualProperty();
        var intContentDescriptor = new OpenRpcContentDescriptor("int", JsonSchema.Empty);
        var stringContentDescriptor = new OpenRpcContentDescriptor("string", JsonSchema.Empty);
        description.ParameterDescriptions.Add(new ApiParameterDescription { Source = BindingSource.Body, Type = type });
        contentDescriptorGeneratorMock.Setup(g => g.GenerateForParameter(intPropertyInfo, MethodName, parameterMetadata, jsonSerializerOptions))
            .Returns(intContentDescriptor)
            .Verifiable();
        contentDescriptorGeneratorMock.Setup(g => g.GenerateForProperty(stringPropertyInfo, MethodName, jsonSerializerOptions))
            .Returns(stringContentDescriptor)
            .Verifiable();

        var result = documentGeneratorMock.Object.GetMethodParams(description, MethodName, parametersMetadata, jsonSerializerOptions);

        var expected = new[]
        {
            intContentDescriptor,
            stringContentDescriptor
        };
        result.Should().BeEquivalentTo(expected);
        contentDescriptorGeneratorMock.Verify();
        contentDescriptorGeneratorMock.VerifyNoOtherCalls();
    }

    [Test]
    public void GetMethodParams_BodyInheritedFromRequest_UseGeneratorBasedOnMetadata()
    {
        var description = GetValidDescription();
        var parameterMetadata = new JsonRpcParameterMetadata("propertyName", 0, BindingStyle.Default, false, "originalName", typeof(int));
        var parametersMetadata = new JsonRpcActionParametersMetadata { Parameters = { [nameof(TypeWithProperties.IntProperty)] = parameterMetadata } };
        var jsonSerializerOptions = new JsonSerializerOptions();
        var type = typeof(RequestChild<TypeWithProperties>);
        var intPropertyInfo = typeof(TypeWithProperties).GetProperty(nameof(TypeWithProperties.IntProperty)).ToContextualProperty();
        var stringPropertyInfo = typeof(TypeWithProperties).GetProperty(nameof(TypeWithProperties.StringProperty)).ToContextualProperty();
        var intContentDescriptor = new OpenRpcContentDescriptor("int", JsonSchema.Empty);
        var stringContentDescriptor = new OpenRpcContentDescriptor("string", JsonSchema.Empty);
        description.ParameterDescriptions.Add(new ApiParameterDescription { Source = BindingSource.Body, Type = type });
        contentDescriptorGeneratorMock.Setup(g => g.GenerateForParameter(intPropertyInfo, MethodName, parameterMetadata, jsonSerializerOptions))
            .Returns(intContentDescriptor)
            .Verifiable();
        contentDescriptorGeneratorMock.Setup(g => g.GenerateForProperty(stringPropertyInfo, MethodName, jsonSerializerOptions))
            .Returns(stringContentDescriptor)
            .Verifiable();

        var result = documentGeneratorMock.Object.GetMethodParams(description, MethodName, parametersMetadata, jsonSerializerOptions);

        var expected = new[]
        {
            intContentDescriptor,
            stringContentDescriptor
        };
        result.Should().BeEquivalentTo(expected);
        contentDescriptorGeneratorMock.Verify();
        contentDescriptorGeneratorMock.VerifyNoOtherCalls();
    }

    [Test]
    public void GetResultContentDescriptor_BodyIsResponse_GenerateForType()
    {
        var description = GetValidDescription();
        var jsonSerializerOptions = new JsonSerializerOptions();
        var type = typeof(Response<TypeWithProperties>);
        var resultType = typeof(TypeWithProperties).ToContextualType();
        var contentDescriptor = new OpenRpcContentDescriptor("name", JsonSchema.Empty);
        description.SupportedResponseTypes.Add(new ApiResponseType { Type = type });
        contentDescriptorGeneratorMock.Setup(g => g.GenerateForType(resultType, MethodName, jsonSerializerOptions))
            .Returns(contentDescriptor)
            .Verifiable();

        var result = documentGeneratorMock.Object.GetResultContentDescriptor(description, MethodName, jsonSerializerOptions);

        result.Should().Be(contentDescriptor);
    }

    [Test]
    public void GetResultContentDescriptor_BodyInheritedFromResponse_GenerateForType()
    {
        var description = GetValidDescription();
        var jsonSerializerOptions = new JsonSerializerOptions();
        var type = typeof(ResponseChild<TypeWithProperties>);
        var resultType = typeof(TypeWithProperties).ToContextualType();
        var contentDescriptor = new OpenRpcContentDescriptor("name", JsonSchema.Empty);
        description.SupportedResponseTypes.Add(new ApiResponseType { Type = type });
        contentDescriptorGeneratorMock.Setup(g => g.GenerateForType(resultType, MethodName, jsonSerializerOptions))
            .Returns(contentDescriptor)
            .Verifiable();

        var result = documentGeneratorMock.Object.GetResultContentDescriptor(description, MethodName, jsonSerializerOptions);

        result.Should().Be(contentDescriptor);
    }

    [TestCase("")]
    [TestCase(" ")]
    [TestCase(null)]
    public void GetMethodServers_RelativePathIsEmpty_ReturnNull(string path)
    {
        var description = GetValidDescription();
        var host = new Uri(Host);
        description.RelativePath = path;

        var result = documentGeneratorMock.Object.GetMethodServers(description, host);

        result.Should().BeNull();
    }

    [Test]
    public void GetMethodServers_RelativePathIsDefaultPrefix_ReturnNull()
    {
        var description = GetValidDescription();
        var host = new Uri(Host);
        var path = "default/path";
        serverOptions.RoutePrefix = $"/{path}";
        description.RelativePath = $"{path}#{MethodName}";

        var result = documentGeneratorMock.Object.GetMethodServers(description, host);

        result.Should().BeNull();
    }

    [Test]
    public void GetMethodServers_RelativePathIsDifferent_ReturnServerWithThisRoute()
    {
        var description = GetValidDescription();
        var host = new Uri(Host);
        var path = "different/path";
        var servers = new List<OpenRpcServer> { new("name", new Uri($"{Host}{path}")) };
        description.RelativePath = $"{path}#{MethodName}";
        documentGeneratorMock.Setup(g => g.GetServers(host, path))
            .Returns(servers)
            .Verifiable();

        var result = documentGeneratorMock.Object.GetMethodServers(description, host);

        result.Should().BeEquivalentTo(servers);
        documentGeneratorMock.Verify();
    }

    [Test]
    public void GetParamsStructure_MetadataIsNull_ReturnEither()
    {
        var result = documentGeneratorMock.Object.GetParamsStructure(null);

        result.Should().Be(OpenRpcParamStructure.Either);
    }

    [Test]
    public void GetParamsStructure_MetadataIsEmpty_ReturnEither()
    {
        var metadata = new JsonRpcActionParametersMetadata();

        var result = documentGeneratorMock.Object.GetParamsStructure(metadata);

        result.Should().Be(OpenRpcParamStructure.Either);
    }

    [TestCase(BindingStyle.Default, OpenRpcParamStructure.Either)]
    [TestCase(BindingStyle.Object, OpenRpcParamStructure.ByName)]
    [TestCase(BindingStyle.Array, OpenRpcParamStructure.ByPosition)]
    public void GetParamsStructure_SingleStyle_ConvertToParamsStructure(BindingStyle style, OpenRpcParamStructure structure)
    {
        var metadata = new JsonRpcActionParametersMetadata
        {
            Parameters =
            {
                ["param1"] = new JsonRpcParameterMetadata("propertyName1", 0, style, false, "originalName1", typeof(object)),
                ["param2"] = new JsonRpcParameterMetadata("propertyName2", 1, style, true, "originalName2", typeof(int))
            }
        };

        var result = documentGeneratorMock.Object.GetParamsStructure(metadata);

        result.Should().Be(structure);
    }

    [Test]
    public void GetParamsStructure_DifferentStyles_Combine()
    {
        var metadata = new JsonRpcActionParametersMetadata
        {
            Parameters =
            {
                ["param1"] = new JsonRpcParameterMetadata("propertyName1", 0, BindingStyle.Array, false, "originalName1", typeof(object)),
                ["param2"] = new JsonRpcParameterMetadata("propertyName2", 1, BindingStyle.Object, true, "originalName2", typeof(int)),
                ["param3"] = new JsonRpcParameterMetadata("propertyName3", 2, BindingStyle.Default, true, "originalName3", typeof(string))
            }
        };
        var structure = OpenRpcParamStructure.Either;
        documentGeneratorMock.Setup(static g => g.CombineBindingStyles(It.Is<IReadOnlySet<BindingStyle>>(static s =>
                s.Contains(BindingStyle.Array)
                && s.Contains(BindingStyle.Object)
                && s.Contains(BindingStyle.Default))))
            .Returns(structure)
            .Verifiable();

        var result = documentGeneratorMock.Object.GetParamsStructure(metadata);

        result.Should().Be(structure);
        documentGeneratorMock.Verify();
    }

    [Test]
    public void GetParamsStructure_InvalidStyle_Throw()
    {
        var metadata = new JsonRpcActionParametersMetadata
        {
            Parameters =
            {
                ["param"] = new JsonRpcParameterMetadata("propertyName", 0, (BindingStyle) 3, false, "originalName", typeof(object))
            }
        };

        var action = () => documentGeneratorMock.Object.GetParamsStructure(metadata);

        action.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Test]
    public void CombineBindingStyles_NoArrayAndNoObject_ReturnEither()
    {
        var bindingStyles = new HashSet<BindingStyle>
        {
            BindingStyle.Default
        };

        var result = documentGeneratorMock.Object.CombineBindingStyles(bindingStyles);

        result.Should().Be(OpenRpcParamStructure.Either);
    }

    [Test]
    public void CombineBindingStyles_ArrayAndNoObject_ReturnByPosition()
    {
        var bindingStyles = new HashSet<BindingStyle>
        {
            BindingStyle.Array,
            BindingStyle.Default
        };

        var result = documentGeneratorMock.Object.CombineBindingStyles(bindingStyles);

        result.Should().Be(OpenRpcParamStructure.ByPosition);
    }

    [Test]
    public void CombineBindingStyles_ObjectAndNoArray_ReturnByName()
    {
        var bindingStyles = new HashSet<BindingStyle>
        {
            BindingStyle.Object,
            BindingStyle.Default
        };

        var result = documentGeneratorMock.Object.CombineBindingStyles(bindingStyles);

        result.Should().Be(OpenRpcParamStructure.ByName);
    }

    [Test]
    public void CombineBindingStyles_BothObjectAndArray_ReturnEither()
    {
        var bindingStyles = new HashSet<BindingStyle>
        {
            BindingStyle.Object,
            BindingStyle.Array,
            BindingStyle.Default
        };

        var result = documentGeneratorMock.Object.CombineBindingStyles(bindingStyles);

        result.Should().Be(OpenRpcParamStructure.Either);
    }

    private static ApiDescription GetValidDescription(Action? action = null) => new()
    {
        ActionDescriptor = new ControllerActionDescriptor
        {
            EndpointMetadata = new List<object>
            {
                new JsonRpcControllerAttribute()
            },
            MethodInfo = action?.Method ?? ((Action) ValidMethod).Method
        },
        Properties =
        {
            [ApiExplorerConstants.MethodNameProperty] = MethodName
        }
    };

    [Obsolete]
    private static void ObsoleteMethod()
    {
    }

    private static void ValidMethod()
    {
    }

    /// <summary>summary</summary>
    /// <remarks>description</remarks>
    private static void MethodWithDocs()
    {
    }

    private const string Host = "https://localhost";
    private const string DocumentName = "documentName";
    private const string MethodName = "methodName";

    [Obsolete]
    [UsedImplicitly]
    private sealed class ObsoleteType
    {
        public static void ObsoleteTypeMethod()
        {
        }
    }

    private sealed record RequestChild<T>(IRpcId Id, string Method, T? Params, string Jsonrpc = JsonRpcConstants.Version) : Request<T>(Id, Method, Params, Jsonrpc)
        where T : class;

    private sealed record ResponseChild<T>(IRpcId Id, T? Result, string Jsonrpc = JsonRpcConstants.Version) : Response<T>(Id, Result, Jsonrpc);

    private sealed record TypeWithProperties(int IntProperty, string StringProperty);
}
