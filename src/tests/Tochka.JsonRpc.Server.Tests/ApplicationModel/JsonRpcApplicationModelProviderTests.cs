using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using Tochka.JsonRpc.Common;
using Tochka.JsonRpc.Server.ApplicationModel;
using Tochka.JsonRpc.Server.Attributes;
using Tochka.JsonRpc.Server.Binding;
using Tochka.JsonRpc.Server.Metadata;
using Tochka.JsonRpc.Server.Settings;

namespace Tochka.JsonRpc.Server.Tests.ApplicationModel;

[TestFixture]
public class JsonRpcApplicationModelProviderTests
{
    private JsonOptions options;
    private JsonRpcServerOptions serverOptions;

    private JsonRpcApplicationModelProvider appModel;

    [SetUp]
    public void Setup()
    {
        options = new JsonOptions
        {
            JsonSerializerOptions =
            {
                PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
            }
        };
        serverOptions = new JsonRpcServerOptions();
        appModel = new JsonRpcApplicationModelProvider(Options.Create(options), Options.Create(serverOptions));
    }

    [Test]
    public void Parameters_ActionNotFromJsonRpcController_DoNothing()
    {
        var parameterInfo = new Mock<ParameterInfo>();
        parameterInfo.Setup(static i => i.ParameterType)
            .Returns(typeof(Foo));
        var context = InitContext(parameterInfo.Object, [], false);

        appModel.OnProvidersExecuting(context);

        context.Result.Controllers.Single().Actions.Single().Parameters.Single().BindingInfo.Should().BeNull();
        context.Result.Controllers.Single().Actions.Single().Selectors.Single().EndpointMetadata.Should().BeEmpty();
    }

    [Test]
    public void Parameters_BinderTypeNotJsonRpcModelBinder_DoNothing()
    {
        var parameterInfo = new Mock<ParameterInfo>();
        parameterInfo.Setup(static i => i.ParameterType)
            .Returns(typeof(Foo));
        var bindingInfo = new BindingInfo
        {
            BinderType = Mock.Of<IModelBinder>().GetType()
        };
        var context = InitContext(parameterInfo.Object, [], true);
        context.Result.Controllers.Single().Actions.Single().Parameters.Single().BindingInfo = bindingInfo;

        appModel.OnProvidersExecuting(context);

        context.Result.Controllers.Single().Actions.Single().Parameters.Single().BindingInfo.Should().Be(bindingInfo);
        new SelectorModel().EndpointMetadata.Should().BeEmpty();
    }

    [Test]
    public void Parameters_NoBindingInfo_SetDefaultFromParamsBindingInfo()
    {
        var parameterInfo = new Mock<ParameterInfo>();
        parameterInfo.Setup(static i => i.ParameterType)
            .Returns(typeof(Foo));
        var context = InitContext(parameterInfo.Object, [], true);
        context.Result.Controllers.Single().Actions.Single().Parameters.Single().BindingInfo = null;

        appModel.OnProvidersExecuting(context);

        var expected = new BindingInfo
        {
            BinderType = typeof(JsonRpcModelBinder),
            BindingSource = BindingSource.Custom
        };
        context.Result.Controllers.Single().Actions.Single().Parameters.Single().BindingInfo.Should().BeEquivalentTo(expected);
    }

    [Test]
    public void Parameters_NoParametersMetadata_AddMetadata()
    {
        var parameterInfo = new Mock<ParameterInfo>();
        parameterInfo.Setup(static i => i.ParameterType)
            .Returns(typeof(Foo));
        var context = InitContext(parameterInfo.Object, [], true);

        appModel.OnProvidersExecuting(context);

        context.Result.Controllers.Single().Actions.Single().Selectors.Single().EndpointMetadata.Should().Contain(static m => m is JsonRpcActionParametersMetadata);
    }

    [Test]
    public void Parameters_NoFromParamsAttribute_UseDefaultBindingStyle()
    {
        var parameterInfo = new Mock<ParameterInfo>();
        var position = 123;
        var type = typeof(Foo);
        parameterInfo.Setup(static i => i.ParameterType)
            .Returns(type);
        parameterInfo.Setup(static i => i.Position)
            .Returns(position);
        var context = InitContext(parameterInfo.Object, [], true);

        appModel.OnProvidersExecuting(context);

        var expected = new JsonRpcActionParametersMetadata
        {
            Parameters =
            {
                [ParameterName] = new JsonRpcParameterMetadata("test_parameter_name", position, BindingStyle.Default, false, ParameterName, type)
            }
        };
        context.Result.Controllers.Single().Actions.Single().Selectors.Single().EndpointMetadata.Should().ContainEquivalentOf(expected);
    }

    [TestCase(BindingStyle.Default)]
    [TestCase(BindingStyle.Object)]
    [TestCase(BindingStyle.Array)]
    public void Parameters_HasFromParamsAttribute_UseBindingStyleFromAttribute(BindingStyle bindingStyle)
    {
        var parameterInfo = new Mock<ParameterInfo>();
        var position = 123;
        var type = typeof(Foo);
        parameterInfo.Setup(static i => i.ParameterType)
            .Returns(type);
        parameterInfo.Setup(static i => i.Position)
            .Returns(position);
        var context = InitContext(parameterInfo.Object, [new FromParamsAttribute(bindingStyle)], true);

        appModel.OnProvidersExecuting(context);

        var expected = new JsonRpcActionParametersMetadata
        {
            Parameters =
            {
                [ParameterName] = new JsonRpcParameterMetadata("test_parameter_name", position, bindingStyle, false, ParameterName, type)
            }
        };
        context.Result.Controllers.Single().Actions.Single().Selectors.Single().EndpointMetadata.Should().ContainEquivalentOf(expected);
    }

    [Test]
    public void Parameters_ParameterIsOptional_MarkAsOptional()
    {
        var parameterInfo = new Mock<ParameterInfo>();
        var position = 123;
        var type = typeof(Foo);
        parameterInfo.Setup(static i => i.ParameterType)
            .Returns(type);
        parameterInfo.Setup(static i => i.Position)
            .Returns(position);
        parameterInfo.Setup(static i => i.Attributes)
            .Returns(ParameterAttributes.Optional);
        var context = InitContext(parameterInfo.Object, [], true);

        appModel.OnProvidersExecuting(context);

        var expected = new JsonRpcActionParametersMetadata
        {
            Parameters =
            {
                [ParameterName] = new JsonRpcParameterMetadata("test_parameter_name", position, BindingStyle.Default, true, ParameterName, type)
            }
        };
        context.Result.Controllers.Single().Actions.Single().Selectors.Single().EndpointMetadata.Should().ContainEquivalentOf(expected);
    }

    [Test]
    public void Apply_ActionNotFromJsonRpcController_DoNothing()
    {
        var context = InitDefaultContext(false);
        var someSelector = context.Result.Controllers.Single().Actions.Single().Selectors.Single();

        appModel.OnProvidersExecuting(context);

        var selector = context.Result.Controllers.Single().Actions.Single().Selectors.Single();
        selector.Should().BeEquivalentTo(someSelector);
        selector.EndpointMetadata.Should().BeEmpty();
    }

    [Test]
    public void Apply_NoControllerSelectors_DontAddNewSelectors()
    {
        var context = InitDefaultContext(true);
        context.Result.Controllers.Single().Selectors.Clear();

        appModel.OnProvidersExecuting(context);

        context.Result.Controllers.Single().Actions.Single().Selectors.Should().BeEmpty();
    }

    [Test]
    public void Apply_NoActionSelectors_DontAddNewSelectors()
    {
        var context = InitDefaultContext(true);
        context.Result.Controllers.Single().Actions.Single().Selectors.Clear();

        appModel.OnProvidersExecuting(context);

        context.Result.Controllers.Single().Actions.Single().Selectors.Should().BeEmpty();
    }

    [Test]
    public void Apply_NoRouteTemplate_AddRoutePrefixTemplate()
    {
        var context = InitDefaultContext(true);

        appModel.OnProvidersExecuting(context);

        var selector = context.Result.Controllers.Single().Actions.Single().Selectors.Single();
        selector.AttributeRouteModel.Template.Should().Be("/");
    }

    [TestCase("api/jsonrpc")]
    [TestCase("/")]
    [TestCase("some/route")]
    [TestCase("/some/route")]
    [TestCase("jsonrpc/route")]
    [TestCase("/jsonrpc/route")]
    public void Apply_OnlyControllerRouteTemplate_CombinePrefixWithTemplate(string controllerRoute)
    {
        var context = InitDefaultContext(true);
        context.Result.Controllers.Single().Selectors.Single().AttributeRouteModel = new AttributeRouteModel() { Template = controllerRoute };
        var expected = controllerRoute.StartsWith('/')
            ? controllerRoute
            : $"/{controllerRoute}";

        appModel.OnProvidersExecuting(context);

        var selector = context.Result.Controllers.Single().Actions.Single().Selectors.Single();
        selector.AttributeRouteModel.Template.Should().Be(expected);
    }

    [TestCase("api/jsonrpc")]
    [TestCase("/")]
    [TestCase("some/route")]
    [TestCase("/some/route")]
    [TestCase("jsonrpc/route")]
    [TestCase("/jsonrpc/route")]
    public void Apply_OnlyActionRouteTemplate_CombinePrefixWithTemplate(string actionRoute)
    {
        var context = InitDefaultContext(true);
        context.Result.Controllers.Single().Actions.Single().Selectors.Single().AttributeRouteModel = new AttributeRouteModel() { Template = actionRoute };
        var expected = actionRoute.StartsWith('/')
            ? actionRoute
            : $"/{actionRoute}";

        appModel.OnProvidersExecuting(context);

        var selector = context.Result.Controllers.Single().Actions.Single().Selectors.Single();
        selector.AttributeRouteModel.Template.Should().Be(expected);
    }

    [TestCase("/api", "jsonrpc", "/api/jsonrpc")]
    [TestCase("api", "jsonrpc", "/api/jsonrpc")]
    [TestCase("/api", "jsonrpc/route", "/api/jsonrpc/route")]
    [TestCase("api", "jsonrpc/route", "/api/jsonrpc/route")]
    [TestCase("/controller/route", "/action/route", "/action/route")]
    [TestCase("controller/route", "/action/route", "/action/route")]
    [TestCase("/controller/route", "action/route", "/controller/route/action/route")]
    [TestCase("controller/route", "action/route", "/controller/route/action/route")]
    [TestCase("/jsonrpc", "some/route", "/jsonrpc/some/route")]
    [TestCase("jsonrpc", "some/route", "/jsonrpc/some/route")]
    public void Apply_ControllerAndActionRouteTemplate_CombinePrefixWithTemplates(string controllerRoute, string actionRoute, string expectedRoute)
    {
        var context = InitDefaultContext(true);
        context.Result.Controllers.Single().Selectors.Single().AttributeRouteModel = new AttributeRouteModel() { Template = controllerRoute };
        context.Result.Controllers.Single().Actions.Single().Selectors.Single().AttributeRouteModel = new AttributeRouteModel() { Template = actionRoute };

        appModel.OnProvidersExecuting(context);

        var selector = context.Result.Controllers.Single().Actions.Single().Selectors.Single();
        selector.AttributeRouteModel.Template.Should().Be(expectedRoute);
    }

    [Test]
    public void Apply_SeveralRoutes_DistinctResultTemplates()
    {
        var context = InitDefaultContext(true);
        var cSelectors = context.Result.Controllers.Single().Selectors;
        cSelectors.Clear();
            cSelectors.Add(new SelectorModel { AttributeRouteModel = new AttributeRouteModel { Template = "/api" } });
            cSelectors.Add(new SelectorModel { AttributeRouteModel = new AttributeRouteModel { Template = "api" } });

            var aSelectors = context.Result.Controllers.Single().Actions.Single().Selectors;
            aSelectors.Clear();
            aSelectors.Add(new SelectorModel { AttributeRouteModel = new AttributeRouteModel { Template = "jsonrpc" } });
            aSelectors.Add(new SelectorModel { AttributeRouteModel = new AttributeRouteModel { Template = "/api/jsonrpc" } });

        appModel.OnProvidersExecuting(context);

        var selector = context.Result.Controllers.Single().Actions.Single().Selectors.Single();
        selector.AttributeRouteModel.Template.Should().Be("/api/jsonrpc");
    }

    [Test]
    public void Apply_HasMethodAttribute_DontAddAnother()
    {
        var methodName = "testMethodName";
        var context = InitDefaultContext(true);
        context.Result.Controllers.Single().Actions.Single().Selectors.Single().EndpointMetadata.Add(new JsonRpcMethodAttribute(methodName));

        appModel.OnProvidersExecuting(context);

        var expected = new object[] { new JsonRpcMethodAttribute(methodName) };
        var selector = context.Result.Controllers.Single().Actions.Single().Selectors.Single();
        selector.EndpointMetadata.OfType<JsonRpcMethodAttribute>().Should().BeEquivalentTo(expected);
    }

    [Test]
    public void Apply_DontHaveMethodStyleAttribute_UseDefaultMethodStyle()
    {
        var context = InitDefaultContext(true);
        serverOptions.DefaultMethodStyle = JsonRpcMethodStyle.ControllerAndAction;

        appModel.OnProvidersExecuting(context);

        var expected = new JsonRpcMethodAttribute($"test_controller_name{JsonRpcConstants.ControllerMethodSeparator}test_action_name");
        var selector = context.Result.Controllers.Single().Actions.Single().Selectors.Single();
        selector.EndpointMetadata.Should().ContainEquivalentOf(expected);
    }

    [TestCase(JsonRpcMethodStyle.ControllerAndAction, $"test_controller_name{JsonRpcConstants.ControllerMethodSeparator}test_action_name")]
    [TestCase(JsonRpcMethodStyle.ActionOnly, "test_action_name")]
    public void Apply_HasMethodStyleAttribute_UseMethodStyleFromAttribute(JsonRpcMethodStyle methodStyle, string expectedMethodName)
    {
        var context = InitDefaultContext(true);
        context.Result.Controllers.Single().Actions.Single().Selectors.Single().EndpointMetadata.Add(new JsonRpcMethodStyleAttribute(methodStyle));
        serverOptions.DefaultMethodStyle = JsonRpcMethodStyle.ControllerAndAction;

        appModel.OnProvidersExecuting(context);

        var expected = new JsonRpcMethodAttribute(expectedMethodName);
        var selector = context.Result.Controllers.Single().Actions.Single().Selectors.Single();
        selector.EndpointMetadata.Should().ContainEquivalentOf(expected);
    }

    [Test]
    public void Apply_UnknownMethodStyle_Throw()
    {
        var context = InitDefaultContext(true);
        context.Result.Controllers.Single().Actions.Single().Selectors.Single().EndpointMetadata.Add(new JsonRpcMethodStyleAttribute((JsonRpcMethodStyle) 2));
        serverOptions.DefaultMethodStyle = JsonRpcMethodStyle.ControllerAndAction;

        var action = () => appModel.OnProvidersExecuting(context);

        action.Should().Throw<ArgumentOutOfRangeException>();
    }

    private static ApplicationModelProviderContext InitDefaultContext(bool isJsonRpcController)
    {
        var parameterInfo = new Mock<ParameterInfo>();
        parameterInfo.Setup(static i => i.ParameterType)
            .Returns(typeof(Foo));
        return InitContext(parameterInfo.Object, [], isJsonRpcController);
    }

    private static ApplicationModelProviderContext InitContext(ParameterInfo parameterInfo, IReadOnlyList<object> parameterAttributes, bool isJsonRpcController)
    {
        var param = new ParameterModel(parameterInfo, parameterAttributes)
        {
            ParameterName = ParameterName
        };
        var action = new ActionModel(Mock.Of<MethodInfo>(), new List<object>())
        {
            Parameters = { param },
            ActionName = ActionName,
            Selectors = { new SelectorModel() }
        };
        var controllerAttributes = new List<object>();
        if (isJsonRpcController)
        {
            controllerAttributes.Add(new JsonRpcControllerAttribute());
        }

        var controller = new ControllerModel(Mock.Of<TypeInfo>(), controllerAttributes)
        {
            Actions = { action },
            Selectors = { new SelectorModel() },
            ControllerName = ControllerName
        };
        param.Action = action;
        action.Controller = controller;
        var context = new ApplicationModelProviderContext([]);
        context.Result.Controllers.Add(controller);
        return context;
    }

    private const string ParameterName = "testParameterName";
    private const string ControllerName = "testControllerName";
    private const string ActionName = "testActionName";

    private sealed class Foo;
}
