using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using Tochka.JsonRpc.Common;
using Tochka.JsonRpc.Server.Attributes;
using Tochka.JsonRpc.Server.Routing;
using Tochka.JsonRpc.Server.Serialization;
using Tochka.JsonRpc.Server.Settings;

namespace Tochka.JsonRpc.Server.Tests.Routing;

[TestFixture]
internal class JsonRpcActionModelConventionTests
{
    private List<IJsonSerializerOptionsProvider> serializerOptionsProviders;
    private JsonRpcServerOptions options;
    private JsonRpcActionModelConvention actionModelConvention;

    [SetUp]
    public void Setup()
    {
        serializerOptionsProviders = new List<IJsonSerializerOptionsProvider>();
        options = new JsonRpcServerOptions();

        actionModelConvention = new JsonRpcActionModelConvention(serializerOptionsProviders, Options.Create(options));
    }

    [Test]
    public void Apply_ActionNotFromJsonRpcController_DoNothing()
    {
        var actionSelector = new SelectorModel();
        var controllerSelector = new SelectorModel();
        var actionModel = new ActionModel(Mock.Of<MethodInfo>(), new List<object>())
        {
            Controller = new ControllerModel(Mock.Of<TypeInfo>(), new List<object>())
            {
                Selectors = { controllerSelector }
            },
            Selectors = { actionSelector }
        };

        actionModelConvention.Apply(actionModel);

        actionModel.Selectors.Should().BeEquivalentTo(new[] { actionSelector });
        actionSelector.EndpointMetadata.Should().BeEmpty();
    }

    [Test]
    public void Apply_NoControllerSelectors_DontAddNewSelectors()
    {
        var controllerAttributes = new List<object> { new JsonRpcControllerAttribute() };
        var actionSelector = new SelectorModel();
        var actionModel = new ActionModel(Mock.Of<MethodInfo>(), new List<object>())
        {
            Controller = new ControllerModel(Mock.Of<TypeInfo>(), controllerAttributes),
            Selectors = { actionSelector }
        };

        actionModelConvention.Apply(actionModel);

        actionModel.Selectors.Should().BeEmpty();
    }

    [Test]
    public void Apply_NoActionSelectors_DontAddNewSelectors()
    {
        var controllerAttributes = new List<object> { new JsonRpcControllerAttribute() };
        var controllerSelector = new SelectorModel();
        var actionModel = new ActionModel(Mock.Of<MethodInfo>(), new List<object>())
        {
            Controller = new ControllerModel(Mock.Of<TypeInfo>(), controllerAttributes)
            {
                Selectors = { controllerSelector }
            }
        };

        actionModelConvention.Apply(actionModel);

        actionModel.Selectors.Should().BeEmpty();
    }

    [Test]
    public void Apply_NoRouteTemplate_AddRoutePrefixTemplate()
    {
        var controllerAttributes = new List<object> { new JsonRpcControllerAttribute() };
        var controllerSelector = new SelectorModel();
        var actionSelector = new SelectorModel();
        var actionModel = new ActionModel(Mock.Of<MethodInfo>(), new List<object>())
        {
            Controller = new ControllerModel(Mock.Of<TypeInfo>(), controllerAttributes)
            {
                Selectors = { controllerSelector },
                ControllerName = ControllerName
            },
            Selectors = { actionSelector },
            ActionName = ActionName
        };
        var prefix = "/prefix/route";
        options.RoutePrefix = prefix;

        actionModelConvention.Apply(actionModel);

        actionModel.Selectors.Should().HaveCount(1);
        actionModel.Selectors.Single().AttributeRouteModel.Template.Should().Be(prefix);
    }

    [TestCase("api/jsonrpc", JsonRpcConstants.DefaultRoutePrefix)]
    [TestCase(JsonRpcConstants.DefaultRoutePrefix, JsonRpcConstants.DefaultRoutePrefix)]
    [TestCase("some/route", $"{JsonRpcConstants.DefaultRoutePrefix}/some/route")]
    [TestCase("/some/route", $"{JsonRpcConstants.DefaultRoutePrefix}/some/route")]
    [TestCase("jsonrpc/route", $"{JsonRpcConstants.DefaultRoutePrefix}/jsonrpc/route")]
    [TestCase("/jsonrpc/route", $"{JsonRpcConstants.DefaultRoutePrefix}/jsonrpc/route")]
    public void Apply_OnlyControllerRouteTemplate_CombinePrefixWithTemplate(string controllerRoute, string expectedRoute)
    {
        var controllerAttributes = new List<object> { new JsonRpcControllerAttribute() };
        var controllerSelector = new SelectorModel
        {
            AttributeRouteModel = new AttributeRouteModel { Template = controllerRoute }
        };
        var actionSelector = new SelectorModel();
        var actionModel = new ActionModel(Mock.Of<MethodInfo>(), new List<object>())
        {
            Controller = new ControllerModel(Mock.Of<TypeInfo>(), controllerAttributes)
            {
                Selectors = { controllerSelector },
                ControllerName = ControllerName
            },
            Selectors = { actionSelector },
            ActionName = ActionName
        };

        actionModelConvention.Apply(actionModel);

        actionModel.Selectors.Should().HaveCount(1);
        actionModel.Selectors.Single().AttributeRouteModel.Template.Should().Be(expectedRoute);
    }

    [TestCase("api/jsonrpc", JsonRpcConstants.DefaultRoutePrefix)]
    [TestCase(JsonRpcConstants.DefaultRoutePrefix, JsonRpcConstants.DefaultRoutePrefix)]
    [TestCase("some/route", $"{JsonRpcConstants.DefaultRoutePrefix}/some/route")]
    [TestCase("/some/route", $"{JsonRpcConstants.DefaultRoutePrefix}/some/route")]
    [TestCase("jsonrpc/route", $"{JsonRpcConstants.DefaultRoutePrefix}/jsonrpc/route")]
    [TestCase("/jsonrpc/route", $"{JsonRpcConstants.DefaultRoutePrefix}/jsonrpc/route")]
    public void Apply_OnlyActionRouteTemplate_CombinePrefixWithTemplate(string actionRoute, string expectedRoute)
    {
        var controllerAttributes = new List<object> { new JsonRpcControllerAttribute() };
        var controllerSelector = new SelectorModel();
        var actionSelector = new SelectorModel
        {
            AttributeRouteModel = new AttributeRouteModel { Template = actionRoute }
        };
        var actionModel = new ActionModel(Mock.Of<MethodInfo>(), new List<object>())
        {
            Controller = new ControllerModel(Mock.Of<TypeInfo>(), controllerAttributes)
            {
                Selectors = { controllerSelector },
                ControllerName = ControllerName
            },
            Selectors = { actionSelector },
            ActionName = ActionName
        };

        actionModelConvention.Apply(actionModel);

        actionModel.Selectors.Should().HaveCount(1);
        actionModel.Selectors.Single().AttributeRouteModel.Template.Should().Be(expectedRoute);
    }

    [TestCase("/api", "jsonrpc", JsonRpcConstants.DefaultRoutePrefix)]
    [TestCase("api", "jsonrpc", JsonRpcConstants.DefaultRoutePrefix)]
    [TestCase("/api", "jsonrpc/route", $"{JsonRpcConstants.DefaultRoutePrefix}/route")]
    [TestCase("api", "jsonrpc/route", $"{JsonRpcConstants.DefaultRoutePrefix}/route")]
    [TestCase("/controller/route", "/action/route", $"{JsonRpcConstants.DefaultRoutePrefix}/action/route")]
    [TestCase("controller/route", "/action/route", $"{JsonRpcConstants.DefaultRoutePrefix}/action/route")]
    [TestCase("/controller/route", "action/route", $"{JsonRpcConstants.DefaultRoutePrefix}/controller/route/action/route")]
    [TestCase("controller/route", "action/route", $"{JsonRpcConstants.DefaultRoutePrefix}/controller/route/action/route")]
    [TestCase("/jsonrpc", "some/route", $"{JsonRpcConstants.DefaultRoutePrefix}/jsonrpc/some/route")]
    [TestCase("jsonrpc", "some/route", $"{JsonRpcConstants.DefaultRoutePrefix}/jsonrpc/some/route")]
    public void Apply_ControllerAndActionRouteTemplate_CombinePrefixWithTemplates(string controllerRoute, string actionRoute, string expectedRoute)
    {
        var controllerAttributes = new List<object> { new JsonRpcControllerAttribute() };
        var controllerSelector = new SelectorModel
        {
            AttributeRouteModel = new AttributeRouteModel { Template = controllerRoute }
        };
        var actionSelector = new SelectorModel
        {
            AttributeRouteModel = new AttributeRouteModel { Template = actionRoute }
        };
        var actionModel = new ActionModel(Mock.Of<MethodInfo>(), new List<object>())
        {
            Controller = new ControllerModel(Mock.Of<TypeInfo>(), controllerAttributes)
            {
                Selectors = { controllerSelector },
                ControllerName = ControllerName
            },
            Selectors = { actionSelector },
            ActionName = ActionName
        };

        actionModelConvention.Apply(actionModel);

        actionModel.Selectors.Should().HaveCount(1);
        actionModel.Selectors.Single().AttributeRouteModel.Template.Should().Be(expectedRoute);
    }

    [Test]
    public void Apply_SeveralRoutes_DistinctResultTemplates()
    {
        var controllerAttributes = new List<object> { new JsonRpcControllerAttribute() };
        var actionModel = new ActionModel(Mock.Of<MethodInfo>(), new List<object>())
        {
            Controller = new ControllerModel(Mock.Of<TypeInfo>(), controllerAttributes)
            {
                Selectors =
                {
                    new SelectorModel { AttributeRouteModel = new AttributeRouteModel { Template = "/api" } },
                    new SelectorModel { AttributeRouteModel = new AttributeRouteModel { Template = "api" } }
                },
                ControllerName = ControllerName
            },
            Selectors =
            {
                new SelectorModel { AttributeRouteModel = new AttributeRouteModel { Template = "jsonrpc" } },
                new SelectorModel { AttributeRouteModel = new AttributeRouteModel { Template = "/api/jsonrpc" } }
            },
            ActionName = ActionName
        };

        actionModelConvention.Apply(actionModel);

        actionModel.Selectors.Should().HaveCount(1);
        actionModel.Selectors.Single().AttributeRouteModel.Template.Should().Be(JsonRpcConstants.DefaultRoutePrefix);
    }

    [Test]
    public void Apply_HasMethodAttribute_DontAddAnother()
    {
        var controllerAttributes = new List<object> { new JsonRpcControllerAttribute() };
        var controllerSelector = new SelectorModel();
        var methodName = "methodName";
        var actionSelector = new SelectorModel
        {
            EndpointMetadata = { new JsonRpcMethodAttribute(methodName) }
        };
        var actionModel = new ActionModel(Mock.Of<MethodInfo>(), new List<object>())
        {
            Controller = new ControllerModel(Mock.Of<TypeInfo>(), controllerAttributes)
            {
                Selectors = { controllerSelector },
                ControllerName = ControllerName
            },
            Selectors = { actionSelector },
            ActionName = ActionName
        };

        actionModelConvention.Apply(actionModel);

        var expected = new object[] { new JsonRpcMethodAttribute(methodName) };
        actionModel.Selectors.Should().HaveCount(1);
        actionModel.Selectors.Single().EndpointMetadata.Should().BeEquivalentTo(expected);
    }

    [Test]
    public void Apply_NoCustomSerializer_UseDefaultDataSerializerOptions()
    {
        var controllerAttributes = new List<object> { new JsonRpcControllerAttribute() };
        var controllerSelector = new SelectorModel();
        var actionSelector = new SelectorModel();
        var actionModel = new ActionModel(Mock.Of<MethodInfo>(), new List<object>())
        {
            Controller = new ControllerModel(Mock.Of<TypeInfo>(), controllerAttributes)
            {
                Selectors = { controllerSelector },
                ControllerName = ControllerName
            },
            Selectors = { actionSelector },
            ActionName = ActionName
        };

        actionModelConvention.Apply(actionModel);

        var expected = new JsonRpcMethodAttribute($"controller_name{JsonRpcConstants.ControllerMethodSeparator}action_name");
        actionModel.Selectors.Should().HaveCount(1);
        actionModel.Selectors.Single().EndpointMetadata.Should().ContainEquivalentOf(expected);
    }

    [Test]
    public void Apply_CustomSerializerNotRegistered_Throw()
    {
        var controllerAttributes = new List<object> { new JsonRpcControllerAttribute() };
        var controllerSelector = new SelectorModel();
        var actionSelector = new SelectorModel
        {
            EndpointMetadata = { new JsonRpcSerializerOptionsAttribute(typeof(SnakeCaseJsonSerializerOptionsProvider)) }
        };
        var actionModel = new ActionModel(Mock.Of<MethodInfo>(), new List<object>())
        {
            Controller = new ControllerModel(Mock.Of<TypeInfo>(), controllerAttributes)
            {
                Selectors = { controllerSelector },
                ControllerName = ControllerName
            },
            Selectors = { actionSelector },
            ActionName = ActionName
        };

        var action = () => actionModelConvention.Apply(actionModel);

        action.Should().Throw<ArgumentException>();
    }

    [Test]
    public void Apply_HasCustomSerializer_UseCustomSerializer()
    {
        var controllerAttributes = new List<object> { new JsonRpcControllerAttribute() };
        var controllerSelector = new SelectorModel();
        var actionSelector = new SelectorModel
        {
            EndpointMetadata = { new JsonRpcSerializerOptionsAttribute(typeof(CamelCaseJsonSerializerOptionsProvider)) }
        };
        var actionModel = new ActionModel(Mock.Of<MethodInfo>(), new List<object>())
        {
            Controller = new ControllerModel(Mock.Of<TypeInfo>(), controllerAttributes)
            {
                Selectors = { controllerSelector },
                ControllerName = ControllerName
            },
            Selectors = { actionSelector },
            ActionName = ActionName
        };
        serializerOptionsProviders.Add(new CamelCaseJsonSerializerOptionsProvider());

        actionModelConvention.Apply(actionModel);

        var expected = new JsonRpcMethodAttribute($"controllerName{JsonRpcConstants.ControllerMethodSeparator}actionName");
        actionModel.Selectors.Should().HaveCount(1);
        actionModel.Selectors.Single().EndpointMetadata.Should().ContainEquivalentOf(expected);
    }

    [Test]
    public void Apply_DontHaveMethodStyleAttribute_UseDefaultMethodStyle()
    {
        var controllerAttributes = new List<object> { new JsonRpcControllerAttribute() };
        var controllerSelector = new SelectorModel();
        var actionSelector = new SelectorModel();
        var actionModel = new ActionModel(Mock.Of<MethodInfo>(), new List<object>())
        {
            Controller = new ControllerModel(Mock.Of<TypeInfo>(), controllerAttributes)
            {
                Selectors = { controllerSelector },
                ControllerName = ControllerName
            },
            Selectors = { actionSelector },
            ActionName = ActionName
        };
        options.DefaultMethodStyle = JsonRpcMethodStyle.ControllerAndAction;

        actionModelConvention.Apply(actionModel);

        var expected = new JsonRpcMethodAttribute($"controller_name{JsonRpcConstants.ControllerMethodSeparator}action_name");
        actionModel.Selectors.Should().HaveCount(1);
        actionModel.Selectors.Single().EndpointMetadata.Should().ContainEquivalentOf(expected);
    }

    [TestCase(JsonRpcMethodStyle.ControllerAndAction, $"controller_name{JsonRpcConstants.ControllerMethodSeparator}action_name")]
    [TestCase(JsonRpcMethodStyle.ActionOnly, "action_name")]
    public void Apply_HasMethodStyleAttribute_UseMethodStyleFromAttribute(JsonRpcMethodStyle methodStyle, string expectedMethodName)
    {
        var controllerAttributes = new List<object> { new JsonRpcControllerAttribute() };
        var controllerSelector = new SelectorModel();
        var actionSelector = new SelectorModel
        {
            EndpointMetadata = { new JsonRpcMethodStyleAttribute(methodStyle) }
        };
        var actionModel = new ActionModel(Mock.Of<MethodInfo>(), new List<object>())
        {
            Controller = new ControllerModel(Mock.Of<TypeInfo>(), controllerAttributes)
            {
                Selectors = { controllerSelector },
                ControllerName = ControllerName
            },
            Selectors = { actionSelector },
            ActionName = ActionName
        };
        options.DefaultMethodStyle = JsonRpcMethodStyle.ControllerAndAction;

        actionModelConvention.Apply(actionModel);

        var expected = new JsonRpcMethodAttribute(expectedMethodName);
        actionModel.Selectors.Should().HaveCount(1);
        actionModel.Selectors.Single().EndpointMetadata.Should().ContainEquivalentOf(expected);
    }

    [Test]
    public void Apply_UnknownMethodStyle_Throw()
    {
        var controllerAttributes = new List<object> { new JsonRpcControllerAttribute() };
        var controllerSelector = new SelectorModel();
        var actionSelector = new SelectorModel
        {
            EndpointMetadata = { new JsonRpcMethodStyleAttribute((JsonRpcMethodStyle) 2) }
        };
        var actionModel = new ActionModel(Mock.Of<MethodInfo>(), new List<object>())
        {
            Controller = new ControllerModel(Mock.Of<TypeInfo>(), controllerAttributes)
            {
                Selectors = { controllerSelector },
                ControllerName = ControllerName
            },
            Selectors = { actionSelector },
            ActionName = ActionName
        };
        options.DefaultMethodStyle = JsonRpcMethodStyle.ControllerAndAction;

        var action = () => actionModelConvention.Apply(actionModel);

        action.Should().Throw<ArgumentOutOfRangeException>();
    }

    private const string ControllerName = "ControllerName";
    private const string ActionName = "ActionName";
}
