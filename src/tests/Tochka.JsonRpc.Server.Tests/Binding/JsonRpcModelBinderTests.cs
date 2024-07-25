using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using Tochka.JsonRpc.Common.Features;
using Tochka.JsonRpc.Common.Models.Request.Untyped;
using Tochka.JsonRpc.Server.Attributes;
using Tochka.JsonRpc.Server.Binding;
using Tochka.JsonRpc.Server.Binding.ParseResults;
using Tochka.JsonRpc.Server.Exceptions;
using Tochka.JsonRpc.Server.Metadata;
using Tochka.JsonRpc.Server.Serialization;
using Tochka.JsonRpc.Server.Settings;

namespace Tochka.JsonRpc.Server.Tests.Binding;

[TestFixture]
public class JsonRpcModelBinderTests
{
    private Mock<IJsonRpcParamsParser> paramsParserMock;
    private Mock<IJsonRpcParameterBinder> parameterBinderMock;
    private List<IJsonSerializerOptionsProvider> serializerOptionsProviders;
    private JsonRpcServerOptions options;
    private Mock<JsonRpcModelBinder> modelBinderMock;

    [SetUp]
    public void Setup()
    {
        paramsParserMock = new Mock<IJsonRpcParamsParser>();
        parameterBinderMock = new Mock<IJsonRpcParameterBinder>();
        serializerOptionsProviders = new List<IJsonSerializerOptionsProvider>();
        options = new JsonRpcServerOptions();
        modelBinderMock = new Mock<JsonRpcModelBinder>(paramsParserMock.Object, parameterBinderMock.Object, serializerOptionsProviders, Options.Create(options))
        {
            CallBase = true
        };
    }

    [Test]
    public async Task BindModelAsync_NoParametersMetadata_Throw()
    {
        var bindingContext = new DefaultModelBindingContext
        {
            ActionContext = new ActionContext
            {
                ActionDescriptor = new ControllerActionDescriptor
                {
                    EndpointMetadata = new List<object>()
                }
            }
        };

        var action = () => modelBinderMock.Object.BindModelAsync(bindingContext);

        await action.Should().ThrowAsync<JsonRpcServerException>();
    }

    [Test]
    public async Task BindModelAsync_NoParameterInMetadata_Throw()
    {
        var bindingContext = new DefaultModelBindingContext
        {
            ActionContext = new ActionContext
            {
                ActionDescriptor = new ControllerActionDescriptor
                {
                    EndpointMetadata = new List<object>
                    {
                        new JsonRpcActionParametersMetadata()
                    }
                }
            },
            FieldName = FieldName
        };

        var action = () => modelBinderMock.Object.BindModelAsync(bindingContext);

        await action.Should().ThrowAsync<JsonRpcServerException>();
    }

    [Test]
    public async Task BindModelAsync_HasParameterMetadata_ParseAndSetResult()
    {
        var parameterMetadata = new JsonRpcParameterMetadata("propertyName", 0, BindingStyle.Default, false, "originalName", typeof(string));
        var bindingContext = new DefaultModelBindingContext
        {
            ActionContext = new ActionContext
            {
                ActionDescriptor = new ControllerActionDescriptor
                {
                    EndpointMetadata = new List<object>
                    {
                        new JsonRpcActionParametersMetadata
                        {
                            Parameters =
                            {
                                [FieldName] = parameterMetadata
                            }
                        }
                    }
                }
            },
            FieldName = FieldName
        };
        var parseResult = Mock.Of<IParseResult>();
        modelBinderMock.Setup(b => b.Parse(bindingContext, parameterMetadata))
            .ReturnsAsync(parseResult)
            .Verifiable();
        modelBinderMock.Setup(b => b.SetResult(parseResult, bindingContext, parameterMetadata))
            .Returns(Task.CompletedTask)
            .Verifiable();

        await modelBinderMock.Object.BindModelAsync(bindingContext);

        modelBinderMock.Verify();
    }

    [Test]
    public async Task Parse_RawCallIsNull_Throw()
    {
        var parameterMetadata = new JsonRpcParameterMetadata("propertyName", 0, BindingStyle.Default, false, "originalName", typeof(string));
        var bindingContext = new DefaultModelBindingContext
        {
            ActionContext = new ActionContext
            {
                HttpContext = new DefaultHttpContext()
            }
        };
        bindingContext.HttpContext.Features.Set<IJsonRpcFeature>(new JsonRpcFeature
        {
            Call = new UntypedNotification("method", null)
        });

        var action = () => modelBinderMock.Object.Parse(bindingContext, parameterMetadata);

        await action.Should().ThrowAsync<JsonRpcServerException>();
    }

    [Test]
    public async Task Parse_CallIsNull_Throw()
    {
        var parameterMetadata = new JsonRpcParameterMetadata("propertyName", 0, BindingStyle.Default, false, "originalName", typeof(string));
        var bindingContext = new DefaultModelBindingContext
        {
            ActionContext = new ActionContext
            {
                HttpContext = new DefaultHttpContext()
            }
        };
        bindingContext.HttpContext.Features.Set<IJsonRpcFeature>(new JsonRpcFeature
        {
            RawCall = JsonDocument.Parse("{}")
        });

        var action = () => modelBinderMock.Object.Parse(bindingContext, parameterMetadata);

        await action.Should().ThrowAsync<JsonRpcServerException>();
    }

    [Test]
    public async Task Parse_BothCallsPresent_UseParser()
    {
        var parameterMetadata = new JsonRpcParameterMetadata("propertyName", 0, BindingStyle.Default, false, "originalName", typeof(string));
        var bindingContext = new DefaultModelBindingContext
        {
            ActionContext = new ActionContext
            {
                HttpContext = new DefaultHttpContext()
            }
        };
        var rawCall = JsonDocument.Parse("{}");
        var parameters = JsonDocument.Parse("{}");
        bindingContext.HttpContext.Features.Set<IJsonRpcFeature>(new JsonRpcFeature
        {
            RawCall = rawCall,
            Call = new UntypedNotification("method", parameters)
        });
        var parseResult = Mock.Of<IParseResult>();
        paramsParserMock.Setup(p => p.Parse(rawCall, parameters, parameterMetadata))
            .Returns(parseResult)
            .Verifiable();

        var result = await modelBinderMock.Object.Parse(bindingContext, parameterMetadata);

        result.Should().Be(parseResult);
        paramsParserMock.Verify();
    }

    [Test]
    public async Task SetResult_NoCustomSerializer_UseDefaultDataJsonSerializerOptions()
    {
        var parseResult = Mock.Of<IParseResult>();
        var parameterMetadata = new JsonRpcParameterMetadata("propertyName", 0, BindingStyle.Default, false, "originalName", typeof(string));
        var bindingContext = new DefaultModelBindingContext
        {
            ActionContext = new ActionContext
            {
                ActionDescriptor = new ControllerActionDescriptor
                {
                    EndpointMetadata = new List<object>()
                }
            }
        };
        parameterBinderMock.Setup(b => b.SetResult(bindingContext, parameterMetadata, parseResult, options.DefaultDataJsonSerializerOptions))
            .Verifiable();

        await modelBinderMock.Object.SetResult(parseResult, bindingContext, parameterMetadata);

        paramsParserMock.Verify();
    }

    [Test]
    public async Task SetResult_CustomSerializerNotRegistered_Throw()
    {
        var parseResult = Mock.Of<IParseResult>();
        var parameterMetadata = new JsonRpcParameterMetadata("propertyName", 0, BindingStyle.Default, false, "originalName", typeof(string));
        var bindingContext = new DefaultModelBindingContext
        {
            ActionContext = new ActionContext
            {
                ActionDescriptor = new ControllerActionDescriptor
                {
                    EndpointMetadata = new List<object>
                    {
                        new JsonRpcSerializerOptionsAttribute(typeof(SnakeCaseJsonSerializerOptionsProvider))
                    }
                }
            }
        };

        var action = () => modelBinderMock.Object.SetResult(parseResult, bindingContext, parameterMetadata);

        await action.Should().ThrowAsync<ArgumentException>();
    }

    [Test]
    public async Task SetResult_CustomSerializer_UseCustomSerializerOptions()
    {
        var parseResult = Mock.Of<IParseResult>();
        var parameterMetadata = new JsonRpcParameterMetadata("propertyName", 0, BindingStyle.Default, false, "originalName", typeof(string));
        var bindingContext = new DefaultModelBindingContext
        {
            ActionContext = new ActionContext
            {
                ActionDescriptor = new ControllerActionDescriptor
                {
                    EndpointMetadata = new List<object>
                    {
                        new JsonRpcSerializerOptionsAttribute(typeof(SnakeCaseJsonSerializerOptionsProvider))
                    }
                }
            }
        };
        var serializerOptionsProvider = new SnakeCaseJsonSerializerOptionsProvider();
        serializerOptionsProviders.Add(serializerOptionsProvider);
        parameterBinderMock.Setup(b => b.SetResult(bindingContext, parameterMetadata, parseResult, serializerOptionsProvider.Options))
            .Verifiable();

        await modelBinderMock.Object.SetResult(parseResult, bindingContext, parameterMetadata);

        parameterBinderMock.Verify();
    }

    private const string FieldName = "fieldName";
}
