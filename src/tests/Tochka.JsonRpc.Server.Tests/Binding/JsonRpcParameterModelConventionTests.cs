using System;
using System.Collections.Generic;
using System.Reflection;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using Tochka.JsonRpc.Server.Attributes;
using Tochka.JsonRpc.Server.Binding;
using Tochka.JsonRpc.Server.Metadata;
using Tochka.JsonRpc.Server.Serialization;
using Tochka.JsonRpc.Server.Settings;

namespace Tochka.JsonRpc.Server.Tests.Binding;

[TestFixture]
public class JsonRpcParameterModelConventionTests
{
    private List<IJsonSerializerOptionsProvider> serializerOptionsProviders;
    private JsonRpcServerOptions options;
    private JsonRpcParameterModelConvention parameterModelConvention;

    [SetUp]
    public void Setup()
    {
        serializerOptionsProviders = new List<IJsonSerializerOptionsProvider>();
        options = new JsonRpcServerOptions();

        parameterModelConvention = new JsonRpcParameterModelConvention(serializerOptionsProviders, Options.Create(options));
    }

    private sealed class Foo;

    [Test]
    public void Apply_ActionNotFromJsonRpcController_DoNothing()
    {
        var selector = new SelectorModel();
        var parameterInfo = new Mock<ParameterInfo>();
        parameterInfo.Setup(static i => i.ParameterType)
            .Returns(typeof(Foo));
        var parameterModel = new ParameterModel(parameterInfo.Object, new List<object>())
        {
            Action = new ActionModel(Mock.Of<MethodInfo>(), new List<object>())
            {
                Controller = new ControllerModel(Mock.Of<TypeInfo>(), new List<object>()),
                Selectors = { selector }
            }
        };

        parameterModelConvention.Apply(parameterModel);

        parameterModel.BindingInfo.Should().BeNull();
        selector.EndpointMetadata.Should().BeEmpty();
    }

    [Test]
    public void Apply_BinderTypeNotJsonRpcModelBinder_DoNothing()
    {
        var selector = new SelectorModel();
        var parameterInfo = new Mock<ParameterInfo>();
        parameterInfo.Setup(static i => i.ParameterType)
            .Returns(typeof(Foo));
        var controllerAttributes = new List<object>
        {
            new JsonRpcControllerAttribute()
        };
        var bindingInfo = new BindingInfo
        {
            BinderType = Mock.Of<IModelBinder>().GetType()
        };
        var parameterModel = new ParameterModel(parameterInfo.Object, new List<object>())
        {
            Action = new ActionModel(Mock.Of<MethodInfo>(), new List<object>())
            {
                Controller = new ControllerModel(Mock.Of<TypeInfo>(), controllerAttributes),
                Selectors = { selector }
            },
            BindingInfo = bindingInfo
        };

        parameterModelConvention.Apply(parameterModel);

        parameterModel.BindingInfo.Should().Be(bindingInfo);
        selector.EndpointMetadata.Should().BeEmpty();
    }

    [Test]
    public void Apply_NoBindingInfo_SetDefaultFromParamsBindingInfo()
    {
        var selector = new SelectorModel();
        var parameterInfo = new Mock<ParameterInfo>();
        parameterInfo.Setup(static i => i.ParameterType)
            .Returns(typeof(Foo));
        var controllerAttributes = new List<object>
        {
            new JsonRpcControllerAttribute()
        };
        var parameterModel = new ParameterModel(parameterInfo.Object, new List<object>())
        {
            Action = new ActionModel(Mock.Of<MethodInfo>(), new List<object>())
            {
                Controller = new ControllerModel(Mock.Of<TypeInfo>(), controllerAttributes),
                Selectors = { selector }
            },
            BindingInfo = null,
            ParameterName = ParameterName
        };

        parameterModelConvention.Apply(parameterModel);

        var expected = new BindingInfo
        {
            BinderType = typeof(JsonRpcModelBinder),
            BindingSource = BindingSource.Custom
        };
        parameterModel.BindingInfo.Should().BeEquivalentTo(expected);
    }

    [Test]
    public void Apply_NoParametersMetadata_AddMetadata()
    {
        var selector = new SelectorModel();
        var parameterInfo = new Mock<ParameterInfo>();
        parameterInfo.Setup(static i => i.ParameterType)
            .Returns(typeof(Foo));
        var controllerAttributes = new List<object>
        {
            new JsonRpcControllerAttribute()
        };
        var parameterModel = new ParameterModel(parameterInfo.Object, new List<object>())
        {
            Action = new ActionModel(Mock.Of<MethodInfo>(), new List<object>())
            {
                Controller = new ControllerModel(Mock.Of<TypeInfo>(), controllerAttributes),
                Selectors = { selector }
            },
            ParameterName = ParameterName
        };

        parameterModelConvention.Apply(parameterModel);

        selector.EndpointMetadata.Should().Contain(static m => m is JsonRpcActionParametersMetadata);
    }

    [Test]
    public void Apply_NoFromParamsAttribute_UseDefaultBindingStyle()
    {
        var selector = new SelectorModel();
        var parameterInfo = new Mock<ParameterInfo>();
        var position = 123;
        var type = typeof(Foo);
        parameterInfo.Setup(static i => i.ParameterType)
            .Returns(type);
        parameterInfo.Setup(static i => i.Position)
            .Returns(position);
        var controllerAttributes = new List<object>
        {
            new JsonRpcControllerAttribute()
        };
        var parameterModel = new ParameterModel(parameterInfo.Object, new List<object>())
        {
            Action = new ActionModel(Mock.Of<MethodInfo>(), new List<object>())
            {
                Controller = new ControllerModel(Mock.Of<TypeInfo>(), controllerAttributes),
                Selectors = { selector }
            },
            ParameterName = ParameterName
        };

        parameterModelConvention.Apply(parameterModel);

        var expected = new JsonRpcActionParametersMetadata
        {
            Parameters =
            {
                [ParameterName] = new JsonRpcParameterMetadata("parameter_name", position, BindingStyle.Default, false, ParameterName, type)
            }
        };
        selector.EndpointMetadata.Should().ContainEquivalentOf(expected);
    }

    [TestCase(BindingStyle.Default)]
    [TestCase(BindingStyle.Object)]
    [TestCase(BindingStyle.Array)]
    public void Apply_HasFromParamsAttribute_UseBindingStyleFromAttribute(BindingStyle bindingStyle)
    {
        var selector = new SelectorModel();
        var parameterInfo = new Mock<ParameterInfo>();
        var position = 123;
        var type = typeof(Foo);
        parameterInfo.Setup(static i => i.ParameterType)
            .Returns(type);
        parameterInfo.Setup(static i => i.Position)
            .Returns(position);
        var controllerAttributes = new List<object>
        {
            new JsonRpcControllerAttribute()
        };
        var parameterAttributes = new List<object>
        {
            new FromParamsAttribute(bindingStyle)
        };
        var parameterModel = new ParameterModel(parameterInfo.Object, parameterAttributes)
        {
            Action = new ActionModel(Mock.Of<MethodInfo>(), new List<object>())
            {
                Controller = new ControllerModel(Mock.Of<TypeInfo>(), controllerAttributes),
                Selectors = { selector }
            },
            ParameterName = ParameterName
        };

        parameterModelConvention.Apply(parameterModel);

        var expected = new JsonRpcActionParametersMetadata
        {
            Parameters =
            {
                [ParameterName] = new JsonRpcParameterMetadata("parameter_name", position, bindingStyle, false, ParameterName, type)
            }
        };
        selector.EndpointMetadata.Should().ContainEquivalentOf(expected);
    }

    [Test]
    public void Apply_ParameterIsOptional_MarkAsOptional()
    {
        var selector = new SelectorModel();
        var parameterInfo = new Mock<ParameterInfo>();
        var position = 123;
        var type = typeof(Foo);
        parameterInfo.Setup(static i => i.ParameterType)
            .Returns(type);
        parameterInfo.Setup(static i => i.Position)
            .Returns(position);
        parameterInfo.Setup(static i => i.Attributes)
            .Returns(ParameterAttributes.Optional);
        var controllerAttributes = new List<object>
        {
            new JsonRpcControllerAttribute()
        };
        var parameterModel = new ParameterModel(parameterInfo.Object, new List<object>())
        {
            Action = new ActionModel(Mock.Of<MethodInfo>(), new List<object>())
            {
                Controller = new ControllerModel(Mock.Of<TypeInfo>(), controllerAttributes),
                Selectors = { selector }
            },
            ParameterName = ParameterName
        };

        parameterModelConvention.Apply(parameterModel);

        var expected = new JsonRpcActionParametersMetadata
        {
            Parameters =
            {
                [ParameterName] = new JsonRpcParameterMetadata("parameter_name", position, BindingStyle.Default, true, ParameterName, type)
            }
        };
        selector.EndpointMetadata.Should().ContainEquivalentOf(expected);
    }

    [Test]
    public void Apply_NoCustomSerializer_UseDefaultDataJsonSerializerOptions()
    {
        var selector = new SelectorModel();
        var parameterInfo = new Mock<ParameterInfo>();
        var position = 123;
        var type = typeof(Foo);
        parameterInfo.Setup(static i => i.ParameterType)
            .Returns(type);
        parameterInfo.Setup(static i => i.Position)
            .Returns(position);
        var controllerAttributes = new List<object>
        {
            new JsonRpcControllerAttribute()
        };
        var parameterModel = new ParameterModel(parameterInfo.Object, new List<object>())
        {
            Action = new ActionModel(Mock.Of<MethodInfo>(), new List<object>())
            {
                Controller = new ControllerModel(Mock.Of<TypeInfo>(), controllerAttributes),
                Selectors = { selector }
            },
            ParameterName = ParameterName
        };

        parameterModelConvention.Apply(parameterModel);

        var expected = new JsonRpcActionParametersMetadata
        {
            Parameters =
            {
                [ParameterName] = new JsonRpcParameterMetadata("parameter_name", position, BindingStyle.Default, false, ParameterName, type)
            }
        };
        selector.EndpointMetadata.Should().ContainEquivalentOf(expected);
    }

    [Test]
    public void Apply_CustomSerializerNotRegistered_Throw()
    {
        var selector = new SelectorModel
        {
            EndpointMetadata =
            {
                new JsonRpcSerializerOptionsAttribute(typeof(SnakeCaseJsonSerializerOptionsProvider))
            }
        };
        var parameterInfo = new Mock<ParameterInfo>();
        var position = 123;
        var type = typeof(Foo);
        parameterInfo.Setup(static i => i.ParameterType)
            .Returns(type);
        parameterInfo.Setup(static i => i.Position)
            .Returns(position);
        var controllerAttributes = new List<object>
        {
            new JsonRpcControllerAttribute()
        };
        var parameterModel = new ParameterModel(parameterInfo.Object, new List<object>())
        {
            Action = new ActionModel(Mock.Of<MethodInfo>(), new List<object>())
            {
                Controller = new ControllerModel(Mock.Of<TypeInfo>(), controllerAttributes),
                Selectors = { selector }
            },
            ParameterName = ParameterName
        };

        var action = () => parameterModelConvention.Apply(parameterModel);

        action.Should().Throw<ArgumentException>();
    }

    [Test]
    public void Apply_CustomSerializer_UseDefaultDataJsonSerializerOptions()
    {
        var selector = new SelectorModel
        {
            EndpointMetadata =
            {
                new JsonRpcSerializerOptionsAttribute(typeof(CamelCaseJsonSerializerOptionsProvider))
            }
        };
        var parameterInfo = new Mock<ParameterInfo>();
        var position = 123;
        var type = typeof(Foo);
        parameterInfo.Setup(static i => i.ParameterType)
            .Returns(type);
        parameterInfo.Setup(static i => i.Position)
            .Returns(position);
        var controllerAttributes = new List<object>
        {
            new JsonRpcControllerAttribute()
        };
        serializerOptionsProviders.Add(new CamelCaseJsonSerializerOptionsProvider());
        var parameterModel = new ParameterModel(parameterInfo.Object, new List<object>())
        {
            Action = new ActionModel(Mock.Of<MethodInfo>(), new List<object>())
            {
                Controller = new ControllerModel(Mock.Of<TypeInfo>(), controllerAttributes),
                Selectors = { selector }
            },
            ParameterName = ParameterName
        };

        parameterModelConvention.Apply(parameterModel);

        var expected = new JsonRpcActionParametersMetadata
        {
            Parameters =
            {
                [ParameterName] = new JsonRpcParameterMetadata("parameterName", position, BindingStyle.Default, false, ParameterName, type)
            }
        };
        selector.EndpointMetadata.Should().ContainEquivalentOf(expected);
    }

    private const string ParameterName = "parameterName";
}
