using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using Tochka.JsonRpc.Common;
using Tochka.JsonRpc.Server.Attributes;
using Tochka.JsonRpc.Server.Metadata;
using Tochka.JsonRpc.Server.Serialization;
using Tochka.JsonRpc.Server.Settings;

namespace Tochka.JsonRpc.ApiExplorer.Tests;

[TestFixture]
public class JsonRpcDescriptionProviderTests
{
    private JsonRpcDescriptionProvider descriptionProvider;
    private Mock<ITypeEmitter> typeEmitterMock;

    [SetUp]
    public void Setup()
    {
        typeEmitterMock = new Mock<ITypeEmitter>();

        descriptionProvider = new JsonRpcDescriptionProvider(typeEmitterMock.Object, Mock.Of<ILogger<JsonRpcDescriptionProvider>>());
    }

    [Test]
    public void Order_GreaterThanDefaultProviderOrder()
    {
        var defaultProvider = new DefaultApiDescriptionProvider(Mock.Of<IOptions<MvcOptions>>(), Mock.Of<IInlineConstraintResolver>(), Mock.Of<IModelMetadataProvider>(), Mock.Of<IActionResultTypeMapper>(), Mock.Of<IOptions<RouteOptions>>());

        descriptionProvider.Order.Should().BeGreaterThan(defaultProvider.Order);
    }

    [Test]
    public void OnProvidersExecuting_NoResults_DoNothing()
    {
        var context = GetContext();
        context.Results.Clear();

        descriptionProvider.OnProvidersExecuting(context);

        context.Actions.Should().BeEmpty();
        context.Results.Should().BeEmpty();
    }

    [Test]
    public void OnProvidersExecuting_NotJsonRpcDescriptorsExist_DontChangeThem()
    {
        var context = GetContext();
        context.Results.First().ActionDescriptor.EndpointMetadata.Clear();

        descriptionProvider.OnProvidersExecuting(context);

        context.Results.Should().HaveCount(1);
        context.Results.Single()
            .Should()
            .BeEquivalentTo(new ApiDescription { ActionDescriptor = new ControllerActionDescriptor(), RelativePath = Route },
                static options => options.Excluding(static d => d.ActionDescriptor.Id));
    }

    [Test]
    public void OnProvidersExecuting_NotControllerActionDescriptor_RemoveFromResults()
    {
        var context = GetContext();
        context.Results.First().ActionDescriptor = new PageActionDescriptor
        {
            EndpointMetadata = new List<object> { new JsonRpcControllerAttribute() }
        };

        descriptionProvider.OnProvidersExecuting(context);

        context.Results.Should().BeEmpty();
    }

    [Test]
    public void OnProvidersExecuting_NoMethodAttribute_RemoveFromResults()
    {
        var context = GetContext();

        descriptionProvider.OnProvidersExecuting(context);

        context.Results.Should().BeEmpty();
    }

    [Test]
    public void OnProvidersExecuting_ValidDescriptor_UpdateDescriptionProperties()
    {
        var context = GetContext();
        context.Results.First().ActionDescriptor.EndpointMetadata.Add(new JsonRpcMethodAttribute(Method));
        typeEmitterMock.Setup(static e => e.CreateRequestType(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Type>(), It.IsAny<IReadOnlyDictionary<string, Type>>(), It.IsAny<Type?>()))
            .Returns(Mock.Of<Type>);
        typeEmitterMock.Setup(static e => e.CreateResponseType(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Type>(), It.IsAny<Type?>()))
            .Returns(Mock.Of<Type>);

        descriptionProvider.OnProvidersExecuting(context);

        context.Results.Should().HaveCount(1);
        var description = context.Results.Single();
        description.GroupName.Should().Be(ApiExplorerConstants.DefaultDocumentName);
        description.HttpMethod.Should().Be(HttpMethods.Post);
        description.RelativePath = $"{Route}#{Method}";
        description.Properties[ApiExplorerConstants.MethodNameProperty] = Method;
    }

    [Test]
    public void OnProvidersExecuting_GroupNameAlreadySpecified_DontOverrideGroupName()
    {
        var groupName = "group-name";
        var context = GetContext();
        context.Results.First().ActionDescriptor.EndpointMetadata.Add(new JsonRpcMethodAttribute(Method));
        context.Results.First().GroupName = groupName;
        typeEmitterMock.Setup(static e => e.CreateRequestType(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Type>(), It.IsAny<IReadOnlyDictionary<string, Type>>(), It.IsAny<Type?>()))
            .Returns(Mock.Of<Type>);
        typeEmitterMock.Setup(static e => e.CreateResponseType(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Type>(), It.IsAny<Type?>()))
            .Returns(Mock.Of<Type>);

        descriptionProvider.OnProvidersExecuting(context);

        context.Results.Should().HaveCount(1);
        var description = context.Results.Single();
        description.GroupName.Should().Be(groupName);
    }

    [Test]
    public void OnProvidersExecuting_HasCustomSerializer_UseSerializer()
    {
        var context = GetContext();
        context.Results.First().ActionDescriptor.EndpointMetadata.Add(new JsonRpcMethodAttribute(Method));
        var serializerOptionsProviderType = typeof(SnakeCaseJsonSerializerOptionsProvider);
        context.Results.First().ActionDescriptor.EndpointMetadata.Add(new JsonRpcSerializerOptionsAttribute(serializerOptionsProviderType));
        typeEmitterMock.Setup(e => e.CreateRequestType(It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<Type>(),
                It.IsAny<IReadOnlyDictionary<string, Type>>(),
                serializerOptionsProviderType))
            .Returns(Mock.Of<Type>)
            .Verifiable();
        typeEmitterMock.Setup(e => e.CreateResponseType(It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<Type>(),
                serializerOptionsProviderType))
            .Returns(Mock.Of<Type>)
            .Verifiable();

        descriptionProvider.OnProvidersExecuting(context);

        context.Results.Should().HaveCount(1);
        var description = context.Results.Single();
        description.GroupName.Should().Be($"{ApiExplorerConstants.DefaultDocumentName}_snakecase");
        typeEmitterMock.Verify();
    }

    [Test]
    public void OnProvidersExecuting_ValidDescriptor_WrapRequest()
    {
        var context = GetContext();
        context.Results.First().ActionDescriptor.EndpointMetadata.Add(new JsonRpcMethodAttribute(Method));
        var requestType = Mock.Of<Type>();
        typeEmitterMock.Setup(static e => e.CreateRequestType(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Type>(), It.IsAny<IReadOnlyDictionary<string, Type>>(), It.IsAny<Type?>()))
            .Returns(requestType);
        typeEmitterMock.Setup(static e => e.CreateResponseType(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Type>(), It.IsAny<Type?>()))
            .Returns(Mock.Of<Type>);

        descriptionProvider.OnProvidersExecuting(context);

        context.Results.Should().HaveCount(1);
        var description = context.Results.Single();
        description.SupportedRequestFormats.Should().HaveCount(1);
        description.SupportedRequestFormats.Single().MediaType.Should().Be(JsonRpcConstants.ContentType);
        description.ParameterDescriptions.Should().HaveCount(1);
        description.ParameterDescriptions.Single().Name.Should().Be(JsonRpcConstants.ParamsProperty);
        description.ParameterDescriptions.Single().Source.Should().Be(BindingSource.Body);
        description.ParameterDescriptions.Single().IsRequired.Should().BeTrue();
        description.ParameterDescriptions.Single().Type.Should().Be(requestType);
    }

    [Test]
    public void OnProvidersExecuting_HasBodyParameters_RemoveThem()
    {
        var context = GetContext();
        context.Results.First().ActionDescriptor.EndpointMetadata.Add(new JsonRpcMethodAttribute(Method));
        var parameter1 = new ApiParameterDescription { Name = "1", Source = BindingSource.Body };
        var parameter2 = new ApiParameterDescription { Name = "2", Source = BindingSource.Body };
        var parameter3 = new ApiParameterDescription { Name = "3", Source = BindingSource.Body };
        context.Results.First().ParameterDescriptions.Add(parameter1);
        context.Results.First().ParameterDescriptions.Add(parameter2);
        context.Results.First().ParameterDescriptions.Add(parameter3);
        typeEmitterMock.Setup(static e => e.CreateRequestType(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Type>(), It.IsAny<IReadOnlyDictionary<string, Type>>(), It.IsAny<Type?>()))
            .Returns(Mock.Of<Type>());
        typeEmitterMock.Setup(static e => e.CreateResponseType(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Type>(), It.IsAny<Type?>()))
            .Returns(Mock.Of<Type>);

        descriptionProvider.OnProvidersExecuting(context);

        context.Results.Should().HaveCount(1);
        var description = context.Results.Single();
        description.ParameterDescriptions.Should().NotContain(parameter1);
        description.ParameterDescriptions.Should().NotContain(parameter2);
        description.ParameterDescriptions.Should().NotContain(parameter3);
    }

    [Test]
    public void OnProvidersExecuting_HasNotBodyParameters_LeaveThem()
    {
        var context = GetContext();
        context.Results.First().ActionDescriptor.EndpointMetadata.Add(new JsonRpcMethodAttribute(Method));
        var parameter1 = new ApiParameterDescription { Name = "1", Source = BindingSource.Header };
        var parameter2 = new ApiParameterDescription { Name = "2", Source = BindingSource.Path };
        var parameter3 = new ApiParameterDescription { Name = "3", Source = BindingSource.Query };
        var parameter4 = new ApiParameterDescription { Name = "4", Source = BindingSource.Services };
        var parameter5 = new ApiParameterDescription { Name = "5", Source = BindingSource.Special };
        var parameter6 = new ApiParameterDescription { Name = "6", Source = BindingSource.Custom };
        context.Results.First().ParameterDescriptions.Add(parameter1);
        context.Results.First().ParameterDescriptions.Add(parameter2);
        context.Results.First().ParameterDescriptions.Add(parameter3);
        context.Results.First().ParameterDescriptions.Add(parameter4);
        context.Results.First().ParameterDescriptions.Add(parameter5);
        context.Results.First().ParameterDescriptions.Add(parameter6);
        typeEmitterMock.Setup(static e => e.CreateRequestType(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Type>(), It.IsAny<IReadOnlyDictionary<string, Type>>(), It.IsAny<Type?>()))
            .Returns(Mock.Of<Type>());
        typeEmitterMock.Setup(static e => e.CreateResponseType(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Type>(), It.IsAny<Type?>()))
            .Returns(Mock.Of<Type>);

        descriptionProvider.OnProvidersExecuting(context);

        context.Results.Should().HaveCount(1);
        var description = context.Results.Single();
        description.ParameterDescriptions.Should().Contain(parameter1);
        description.ParameterDescriptions.Should().Contain(parameter2);
        description.ParameterDescriptions.Should().Contain(parameter3);
        description.ParameterDescriptions.Should().Contain(parameter4);
        description.ParameterDescriptions.Should().Contain(parameter5);
        description.ParameterDescriptions.Should().Contain(parameter6);
    }

    [Test]
    public void OnProvidersExecuting_HasArrayAndDefaultParameters_RemoveThemAndUseAsBaseTypeWithoutDefault()
    {
        var context = GetContext();
        context.Results.First().ActionDescriptor.EndpointMetadata.Add(new JsonRpcMethodAttribute(Method));
        var arrayParameter = new ApiParameterDescription { Name = "array", Source = BindingSource.Custom };
        var defaultParameter = new ApiParameterDescription { Name = "default", Source = BindingSource.Custom };
        var arrayParameterType = Mock.Of<Type>();
        var defaultParameterType = Mock.Of<Type>();
        var defaultParameterOriginalName = "original";
        var parametersMetadata = new JsonRpcActionParametersMetadata
        {
            Parameters =
            {
                [arrayParameter.Name] = new JsonRpcParameterMetadata(arrayParameter.Name, 0, BindingStyle.Array, false, arrayParameter.Name, arrayParameterType),
                [defaultParameter.Name] = new JsonRpcParameterMetadata(defaultParameter.Name, 1, BindingStyle.Default, false, defaultParameterOriginalName, defaultParameterType)
            }
        };
        context.Results.First().ParameterDescriptions.Add(arrayParameter);
        context.Results.First().ParameterDescriptions.Add(defaultParameter);
        context.Results.First().ActionDescriptor.EndpointMetadata.Add(parametersMetadata);
        typeEmitterMock.Setup(e => e.CreateRequestType(It.IsAny<string>(),
                It.IsAny<string>(),
                arrayParameterType,
                It.Is<IReadOnlyDictionary<string, Type>>(static d => d.Count == 0),
                It.IsAny<Type?>()))
            .Returns(Mock.Of<Type>)
            .Verifiable();
        typeEmitterMock.Setup(static e => e.CreateResponseType(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Type>(), It.IsAny<Type?>()))
            .Returns(Mock.Of<Type>);

        descriptionProvider.OnProvidersExecuting(context);

        context.Results.Should().HaveCount(1);
        var description = context.Results.Single();
        description.ParameterDescriptions.Should().NotContain(arrayParameter);
        description.ParameterDescriptions.Should().NotContain(defaultParameter);
        typeEmitterMock.Verify();
    }

    [Test]
    public void OnProvidersExecuting_HasObjectAndDefaultParameters_RemoveThemAndUseAsBaseTypeWithDefault()
    {
        var context = GetContext();
        context.Results.First().ActionDescriptor.EndpointMetadata.Add(new JsonRpcMethodAttribute(Method));
        var objectParameter = new ApiParameterDescription { Name = "object", Source = BindingSource.Custom };
        var defaultParameter = new ApiParameterDescription { Name = "default", Source = BindingSource.Custom };
        var objectParameterType = Mock.Of<Type>();
        var defaultParameterType = Mock.Of<Type>();
        var defaultParameterOriginalName = "original";
        var parametersMetadata = new JsonRpcActionParametersMetadata
        {
            Parameters =
            {
                [objectParameter.Name] = new JsonRpcParameterMetadata(objectParameter.Name, 0, BindingStyle.Object, false, objectParameter.Name, objectParameterType),
                [defaultParameter.Name] = new JsonRpcParameterMetadata(defaultParameter.Name, 1, BindingStyle.Default, false, defaultParameterOriginalName, defaultParameterType)
            }
        };
        context.Results.First().ParameterDescriptions.Add(objectParameter);
        context.Results.First().ParameterDescriptions.Add(defaultParameter);
        context.Results.First().ActionDescriptor.EndpointMetadata.Add(parametersMetadata);
        typeEmitterMock.Setup(e => e.CreateRequestType(It.IsAny<string>(),
                It.IsAny<string>(),
                objectParameterType,
                It.Is<IReadOnlyDictionary<string, Type>>(d =>
                    d.ContainsKey(defaultParameterOriginalName)
                    && d[defaultParameterOriginalName] == defaultParameterType),
                It.IsAny<Type?>()))
            .Returns(Mock.Of<Type>)
            .Verifiable();
        typeEmitterMock.Setup(static e => e.CreateResponseType(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Type>(), It.IsAny<Type?>()))
            .Returns(Mock.Of<Type>);

        descriptionProvider.OnProvidersExecuting(context);

        context.Results.Should().HaveCount(1);
        var description = context.Results.Single();
        description.ParameterDescriptions.Should().NotContain(objectParameter);
        description.ParameterDescriptions.Should().NotContain(defaultParameter);
        typeEmitterMock.Verify();
    }

    [Test]
    public void OnProvidersExecuting_HasOnlyDefaultParameters_RemoveThemAndUseObjectAsBaseTypeWithDefault()
    {
        var context = GetContext();
        context.Results.First().ActionDescriptor.EndpointMetadata.Add(new JsonRpcMethodAttribute(Method));
        var defaultParameter = new ApiParameterDescription { Name = "default", Source = BindingSource.Custom };
        var defaultParameterType = Mock.Of<Type>();
        var defaultParameterOriginalName = "original";
        var parametersMetadata = new JsonRpcActionParametersMetadata
        {
            Parameters =
            {
                [defaultParameter.Name] = new JsonRpcParameterMetadata(defaultParameter.Name, 1, BindingStyle.Default, false, defaultParameterOriginalName, defaultParameterType)
            }
        };
        context.Results.First().ActionDescriptor.EndpointMetadata.Add(parametersMetadata);
        typeEmitterMock.Setup(e => e.CreateRequestType(It.IsAny<string>(),
                It.IsAny<string>(),
                typeof(object),
                It.Is<IReadOnlyDictionary<string, Type>>(d =>
                    d.ContainsKey(defaultParameterOriginalName)
                    && d[defaultParameterOriginalName] == defaultParameterType),
                It.IsAny<Type?>()))
            .Returns(Mock.Of<Type>)
            .Verifiable();
        typeEmitterMock.Setup(static e => e.CreateResponseType(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Type>(), It.IsAny<Type?>()))
            .Returns(Mock.Of<Type>);

        descriptionProvider.OnProvidersExecuting(context);

        context.Results.Should().HaveCount(1);
        var description = context.Results.Single();
        description.ParameterDescriptions.Should().NotContain(defaultParameter);
        typeEmitterMock.Verify();
    }

    [Test]
    public void OnProvidersExecuting_HasResponseType_RemoveItAndWrapResponseWithBaseType()
    {
        var context = GetContext();
        context.Results.First().ActionDescriptor.EndpointMetadata.Add(new JsonRpcMethodAttribute(Method));
        var initialResponseType = new ApiResponseType { Type = Mock.Of<Type>() };
        context.Results.First().SupportedResponseTypes.Add(initialResponseType);
        var responseType = Mock.Of<Type>();
        typeEmitterMock.Setup(static e => e.CreateRequestType(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Type>(), It.IsAny<IReadOnlyDictionary<string, Type>>(), It.IsAny<Type?>()))
            .Returns(Mock.Of<Type>);
        typeEmitterMock.Setup(e => e.CreateResponseType(It.IsAny<string>(),
                It.IsAny<string>(),
                initialResponseType.Type,
                It.IsAny<Type?>()))
            .Returns(responseType)
            .Verifiable();

        descriptionProvider.OnProvidersExecuting(context);

        context.Results.Should().HaveCount(1);
        var description = context.Results.Single();
        description.SupportedResponseTypes.Should().HaveCount(1);
        description.SupportedResponseTypes.Should().NotContain(initialResponseType);
        description.SupportedResponseTypes.Single().ApiResponseFormats.Should().HaveCount(1);
        description.SupportedResponseTypes.Single().ApiResponseFormats.Single().MediaType.Should().Be(JsonRpcConstants.ContentType);
        description.SupportedResponseTypes.Single().IsDefaultResponse.Should().BeFalse();
        description.SupportedResponseTypes.Single().StatusCode.Should().Be(200);
        description.SupportedResponseTypes.Single().Type.Should().Be(responseType);
    }

    [Test]
    public void OnProvidersExecuting_DoesntHaveResponseType_WrapResponseWithObjectBaseType()
    {
        var context = GetContext();
        context.Results.First().ActionDescriptor.EndpointMetadata.Add(new JsonRpcMethodAttribute(Method));
        var responseType = Mock.Of<Type>();
        typeEmitterMock.Setup(static e => e.CreateRequestType(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Type>(), It.IsAny<IReadOnlyDictionary<string, Type>>(), It.IsAny<Type?>()))
            .Returns(Mock.Of<Type>);
        typeEmitterMock.Setup(e => e.CreateResponseType(It.IsAny<string>(),
                It.IsAny<string>(),
                typeof(object),
                It.IsAny<Type?>()))
            .Returns(responseType)
            .Verifiable();

        descriptionProvider.OnProvidersExecuting(context);

        context.Results.Should().HaveCount(1);
        var description = context.Results.Single();
        description.SupportedResponseTypes.Should().HaveCount(1);
        description.SupportedResponseTypes.Single().Type.Should().Be(responseType);
    }

    private static ApiDescriptionProviderContext GetContext() => new(new List<ActionDescriptor>())
    {
        Results =
        {
            new ApiDescription
            {
                RelativePath = Route,
                ActionDescriptor = new ControllerActionDescriptor
                {
                    EndpointMetadata = new List<object>
                    {
                        new JsonRpcControllerAttribute()
                    },
                    ControllerTypeInfo = Mock.Of<TypeInfo>()
                }
            }
        }
    };

    private const string Method = "method";
    private const string Route = "/api/jsonrpc";
}
