using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Internal;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using Tochka.JsonRpc.Common;
using Tochka.JsonRpc.Common.Serializers;
using Tochka.JsonRpc.Server.Attributes;
using Tochka.JsonRpc.Server.Conventions;
using Tochka.JsonRpc.Server.Models;
using Tochka.JsonRpc.Server.Services;
using Tochka.JsonRpc.Server.Settings;
using Tochka.JsonRpc.Server.Tests.Helpers;

namespace Tochka.JsonRpc.Server.Tests.Conventions
{
    public class ActionConventionTests
    {
        private TestEnvironment testEnvironment;
        private IFilterMetadata someFilter;

        [SetUp]
        public void Setup()
        {
            var routesMock = new Mock<IJsonRpcRoutes>();
            routesMock.Setup(x => x.IsJsonRpcRoute(It.IsAny<string>()))
                .Returns(true);

            testEnvironment = new TestEnvironment(services =>
            {
                services.TryAddJsonRpcSerializer<HeaderJsonRpcSerializer>();
                services.TryAddJsonRpcSerializer<SnakeCaseJsonRpcSerializer>();
                services.TryAddJsonRpcSerializer<CamelCaseJsonRpcSerializer>();
                var matcher = Mock.Of<IMethodMatcher>(x => x.GetActionName(It.IsAny<MethodMetadata>()) == string.Empty);
                services.AddSingleton(matcher);
                services.AddSingleton<ActionConvention>();
                services.AddSingleton(routesMock.Object);
            });
            someFilter = new AuthorizeFilter();
        }

        [Test]
        public void Test_Apply_IgnoresMvcActions()
        {
            var controllerModel = new Mock<ControllerModel>(typeof(MvcTestController).GetTypeInfo(), new List<object>()).Object;
            var model = new Mock<ActionModel>(typeof(MvcTestController).GetMethod(nameof(MvcTestController.VoidAction)), new List<object>()).Object;
            model.Controller = controllerModel;
            model.Selectors.Add(new SelectorModel());
            model.Filters.Add(someFilter);
            var options = new JsonRpcOptions();
            var actionConvention = GetActionConvention(options);

            actionConvention.Apply(model);

            model.Selectors.Should().HaveCount(1);
            model.Filters.Should().HaveCount(1);
            model.Filters[0].Should().Be(someFilter);
        }

        [Test]
        public void Test_SetAttributes_ClearsSelectorsAndAddsCustomSelector()
        {
            var controllerModel = new Mock<ControllerModel>(typeof(JsonRpcTestController).GetTypeInfo(), new List<object>()).Object;
            var model = new Mock<ActionModel>(typeof(JsonRpcTestController).GetMethod(nameof(JsonRpcTestController.VoidAction)), new List<object>()).Object;
            model.Controller = controllerModel;
            var selectorToRemove = new SelectorModel();
            model.Selectors.Add(selectorToRemove);
            var options = new JsonRpcOptions();
            var route = new PathString("/test");
            options.DefaultMethodOptions.Route = route;
            var actionConvention = GetActionConvention(options);

            actionConvention.SetAttributes(model, options.DefaultMethodOptions);

            model.Selectors.Should().HaveCount(1);
            model.Selectors[0].Should().NotBe(selectorToRemove);
            model.Selectors[0].AttributeRouteModel.Template.Should().Be(route);
            model.Selectors[0].ActionConstraints.Should().HaveCount(2);
            model.Selectors[0].ActionConstraints[0].Should().BeOfType<JsonRpcAttribute>();
            model.Selectors[0].ActionConstraints[1].Should().BeOfType<HttpMethodActionConstraint>();
            model.Selectors[0].ActionConstraints[1].As<HttpMethodActionConstraint>().HttpMethods.Should().HaveCount(1);
            model.Selectors[0].ActionConstraints[1].As<HttpMethodActionConstraint>().HttpMethods.First().Should().Be(HttpMethods.Post);
        }

        [Test]
        public void Test_MakeOptions_UsesDefaults()
        {
            var controllerModel = new Mock<ControllerModel>(typeof(JsonRpcTestController).GetTypeInfo(), new List<object>()).Object;
            var model = new Mock<ActionModel>(typeof(JsonRpcTestController).GetMethod(nameof(JsonRpcTestController.VoidAction)), new List<object>()).Object;
            model.Controller = controllerModel;
            var options = new JsonRpcOptions()
            {
                DefaultMethodOptions = new JsonRpcMethodOptions
                {
                    MethodStyle = MethodStyle.ActionOnly,
                    Route = new PathString("/test"),
                    RequestSerializer = typeof(CamelCaseJsonRpcSerializer)
                }
            };
            var actionConvention = GetActionConvention(options);

            var result = actionConvention.MakeOptions(model);
            result.Should().NotBe(options.DefaultMethodOptions);
            result.Should().BeEquivalentTo(options.DefaultMethodOptions);
        }

        [Test]
        public void Test_MakeOptions_UsesActionAttributes()
        {
            var serializerType = typeof(CamelCaseJsonRpcSerializer);
            var route = new PathString("/test");
            var methodStyle = MethodStyle.ActionOnly;
            var attributes = new List<object>
            {
                new Mock<JsonRpcSerializerAttribute>(serializerType).Object,
                new Mock<RouteAttribute>(route.Value).Object,
                new Mock<JsonRpcMethodStyleAttribute>(methodStyle).Object,
            };
            var controllerModel = new Mock<ControllerModel>(typeof(JsonRpcTestController).GetTypeInfo(), new List<object>()).Object;
            var model = new Mock<ActionModel>(typeof(JsonRpcTestController).GetMethod(nameof(JsonRpcTestController.VoidAction)), attributes).Object;
            model.Controller = controllerModel;
            var options = new JsonRpcOptions();
            var actionConvention = GetActionConvention(options);

            var result = actionConvention.MakeOptions(model);
            result.Should().NotBe(options.DefaultMethodOptions);
            result.Should().NotBeEquivalentTo(options.DefaultMethodOptions);
            result.Route.Should().Be(route);
            result.MethodStyle.Should().Be(methodStyle);
            result.RequestSerializer.Should().Be(serializerType);
        }

        [Test]
        public void Test_MakeOptions_UsesControllerAttributes()
        {
            var serializerType = typeof(CamelCaseJsonRpcSerializer);
            var route = new PathString("/test");
            var methodStyle = MethodStyle.ActionOnly;
            var attributes = new List<object>
            {
                new Mock<JsonRpcSerializerAttribute>(serializerType).Object,
                new Mock<RouteAttribute>(route.Value).Object,
                new Mock<JsonRpcMethodStyleAttribute>(methodStyle).Object,
            };
            var controllerModel = new Mock<ControllerModel>(typeof(JsonRpcTestController).GetTypeInfo(), attributes).Object;
            var model = new Mock<ActionModel>(typeof(JsonRpcTestController).GetMethod(nameof(JsonRpcTestController.VoidAction)), new List<object>()).Object;
            model.Controller = controllerModel;
            var options = new JsonRpcOptions();
            var actionConvention = GetActionConvention(options);

            var result = actionConvention.MakeOptions(model);
            result.Should().NotBe(options.DefaultMethodOptions);
            result.Should().NotBeEquivalentTo(options.DefaultMethodOptions);
            result.Route.Should().Be(route);
            result.MethodStyle.Should().Be(methodStyle);
            result.RequestSerializer.Should().Be(serializerType);
        }

        [TestCase(typeof(CamelCaseJsonRpcSerializer))]
        [TestCase(typeof(SnakeCaseJsonRpcSerializer))]
        public void Test_GetMethodMetadata_UsesSerializerAndMethodOptions(Type serializerType)
        {
            var actionName = "actionName";
            var controllerName = "controllerName";
            var serializer = testEnvironment.ServiceProvider.GetRequiredService(serializerType) as IJsonRpcSerializer;
            var controllerModel = new Mock<ControllerModel>(typeof(JsonRpcTestController).GetTypeInfo(), new List<object> { }).Object;
            var model = new Mock<ActionModel>(typeof(JsonRpcTestController).GetMethod(nameof(JsonRpcTestController.VoidAction)), new List<object>()).Object;
            model.Controller = controllerModel;
            model.ActionName = actionName;
            model.Controller.ControllerName = controllerName;
            var options = new JsonRpcOptions();
            var actionConvention = GetActionConvention(options);
            var methodOptions = new JsonRpcMethodOptions()
            {
                RequestSerializer = serializerType
            };
            var result = actionConvention.GetMethodMetadata(model, methodOptions);
            result.Action.Should().BeEquivalentTo(serializer.GetJsonName(actionName));
            result.Controller.Should().BeEquivalentTo(serializer.GetJsonName(controllerName));
            result.MethodOptions.Should().NotBe(options.DefaultMethodOptions);
            result.MethodOptions.Should().Be(methodOptions);
        }

        [Test]
        public void Test_ValidateRouting_AddsAndFailsOnDuplicates()
        {
            var options = new JsonRpcOptions();
            var data1 = new MethodMetadata(options.DefaultMethodOptions, new JsonName("", ""), new JsonName("", "a"));
            var data2 = new MethodMetadata(options.DefaultMethodOptions, new JsonName("", ""), new JsonName("", "b"));

            var matcherMock = new Mock<IMethodMatcher>();
            matcherMock.Setup(x => x.GetActionName(It.IsAny<MethodMetadata>())).Returns<MethodMetadata>(x => x.Action.Json);
            var actionConvention = GetActionConvention(options, matcherMock.Object);

            actionConvention.KnownRoutes.Should().HaveCount(0);

            Action addDiffertentRoutes = () =>
            {
                actionConvention.ValidateRouting(data1);
                actionConvention.ValidateRouting(data2);
            };

            addDiffertentRoutes.Should().NotThrow();
            actionConvention.KnownRoutes.Should().HaveCount(2);

            Action addDuplicate1 = () => actionConvention.ValidateRouting(data1);
            addDuplicate1.Should().Throw<InvalidOperationException>();

            Action addDuplicate2 = () => actionConvention.ValidateRouting(data2);
            addDuplicate2.Should().Throw<InvalidOperationException>();

            actionConvention.KnownRoutes.Should().HaveCount(2);
        }

        [Test]
        public void Test_ValidateRouting_FailsOnReservedName()
        {
            var options = new JsonRpcOptions();
            options.DefaultMethodOptions.MethodStyle = MethodStyle.ActionOnly;
            var data = new MethodMetadata(options.DefaultMethodOptions, new JsonName("", ""), new JsonName("", $"{JsonRpcConstants.ReservedMethodPrefix}a"));

            var matcherMock = new Mock<IMethodMatcher>();
            matcherMock.Setup(x => x.GetActionName(It.IsAny<MethodMetadata>())).Returns<MethodMetadata>(x => x.Action.Json);
            var actionConvention = GetActionConvention(options, matcherMock.Object);

            actionConvention.KnownRoutes.Should().HaveCount(0);

            Action addDuplicate1 = () => actionConvention.ValidateRouting(data);
            addDuplicate1.Should().Throw<InvalidOperationException>();

            actionConvention.KnownRoutes.Should().HaveCount(0);
        }

        [Test]
        public void Test_Apply_AddsProperty()
        {
            var controllerModel = new Mock<ControllerModel>(typeof(JsonRpcTestController).GetTypeInfo(), new List<object>()).Object;
            var model = new Mock<ActionModel>(typeof(JsonRpcTestController).GetMethod(nameof(JsonRpcTestController.VoidAction)), new List<object>()).Object;
            model.Controller = controllerModel;
            var options = new JsonRpcOptions();
            var actionConvention = GetActionConvention(options);

            actionConvention.KnownRoutes.Should().HaveCount(0);
            model.Properties.Should().HaveCount(0);

            actionConvention.Apply(model);

            model.Properties.Should().HaveCount(1);
            model.Properties.First().Key.Should().Be(typeof(MethodMetadata));
            model.Properties.First().Value.Should().BeOfType<MethodMetadata>();
            actionConvention.KnownRoutes.Should().HaveCount(1);
        }

        /// <summary>
        /// Resolve ActionConvention with options and overriding services instead of registered in DI
        /// </summary>
        /// <param name="jsonRpcOptions"></param>
        /// <param name="services"></param>
        /// <returns></returns>
        private ActionConvention GetActionConvention(JsonRpcOptions jsonRpcOptions, params object[] services)
        {
            var optionsWrapper = new OptionsWrapper<JsonRpcOptions>(jsonRpcOptions);
            var allServices = new List<object> {optionsWrapper};
            allServices.AddRange(services);
            return ActivatorUtilities.CreateInstance<ActionConvention>(testEnvironment.ServiceProvider, allServices.ToArray());
        }
    }
}