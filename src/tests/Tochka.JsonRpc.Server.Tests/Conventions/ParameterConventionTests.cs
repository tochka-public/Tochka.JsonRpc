using System;
using System.Collections.Generic;
using System.Reflection;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;
using Tochka.JsonRpc.Common.Serializers;
using Tochka.JsonRpc.Server.Attributes;
using Tochka.JsonRpc.Server.Binding;
using Tochka.JsonRpc.Server.Conventions;
using Tochka.JsonRpc.Server.Models;
using Tochka.JsonRpc.Server.Settings;
using Tochka.JsonRpc.Server.Tests.Helpers;

namespace Tochka.JsonRpc.Server.Tests.Conventions
{
    public class ParameterConventionTests
    {
        private TestEnvironment testEnvironment;

        [SetUp]
        public void Setup()
        {
            testEnvironment = new TestEnvironment(services =>
            {
                services.TryAddJsonRpcSerializer<HeaderJsonRpcSerializer>();
                services.TryAddJsonRpcSerializer<SnakeCaseJsonRpcSerializer>();
                services.TryAddJsonRpcSerializer<CamelCaseJsonRpcSerializer>();
                services.AddSingleton<ParameterConvention>();
            });
        }

        [Test]
        public void Test_Apply_IgnoresMvcActionParameters()
        {
            var controllerModel = new Mock<ControllerModel>(typeof(MvcTestController).GetTypeInfo(), new List<object>()).Object;
            var actionModel = new Mock<ActionModel>(typeof(MvcTestController).GetMethod(nameof(MvcTestController.VoidAction)), new List<object>()).Object;
            var model = new Mock<ParameterModel>(typeof(MvcTestController).GetMethod(nameof(MvcTestController.VoidAction)).GetParameters()[0], new List<object>()).Object;
            actionModel.Controller = controllerModel;
            model.Action = actionModel;
            var binding = new BindingInfo();
            model.BindingInfo = binding;
            var parameterConvention = testEnvironment.ServiceProvider.GetRequiredService<ParameterConvention>();

            parameterConvention.Apply(model);

            model.BindingInfo.Should().Be(binding);
        }

        [Test]
        public void Test_Apply_ThrowsOnBadProperty()
        {
            var controllerModel = new Mock<ControllerModel>(typeof(JsonRpcTestController).GetTypeInfo(), new List<object>()).Object;
            var actionModel = new Mock<ActionModel>(typeof(JsonRpcTestController).GetMethod(nameof(MvcTestController.VoidAction)), new List<object>()).Object;
            actionModel.Properties[typeof(MethodMetadata)] = "test";
            var model = new Mock<ParameterModel>(typeof(JsonRpcTestController).GetMethod(nameof(MvcTestController.VoidAction)).GetParameters()[0], new List<object>()).Object;
            actionModel.Controller = controllerModel;
            model.Action = actionModel;
            var binding = new BindingInfo();
            model.BindingInfo = binding;
            var parameterConvention = testEnvironment.ServiceProvider.GetRequiredService<ParameterConvention>();
            Action action = () => parameterConvention.Apply(model);

            action.Should().Throw<ArgumentNullException>();
        }

        [Test]
        public void Test_Apply_AddsParameterMetadataSetsBinding()
        {
            var metadata = new MethodMetadata(new JsonRpcMethodOptions(), new JsonName("", ""), new JsonName("", ""));
            var controllerModel = new Mock<ControllerModel>(typeof(JsonRpcTestController).GetTypeInfo(), new List<object>()).Object;
            var actionModel = new Mock<ActionModel>(typeof(JsonRpcTestController).GetMethod(nameof(MvcTestController.VoidAction)), new List<object>()).Object;
            actionModel.Properties[typeof(MethodMetadata)] = metadata;
            var model = new Mock<ParameterModel>(typeof(JsonRpcTestController).GetMethod(nameof(MvcTestController.VoidAction)).GetParameters()[0], new List<object>()).Object;
            actionModel.Controller = controllerModel;
            model.Action = actionModel;
            var binding = new BindingInfo();
            model.BindingInfo = binding;
            model.ParameterName = "test";
            var parameterConvention = testEnvironment.ServiceProvider.GetRequiredService<ParameterConvention>();

            parameterConvention.Apply(model);

            metadata.ParametersInternal.Should().HaveCount(1);
            model.BindingInfo.Should().NotBeNull();
        }

        [TestCase(typeof(CamelCaseJsonRpcSerializer))]
        [TestCase(typeof(SnakeCaseJsonRpcSerializer))]
        public void Test_GetRpcParameterInfo_UsesSerializer(Type serializerType)
        {
            var serializer = testEnvironment.ServiceProvider.GetRequiredService(serializerType) as IJsonRpcSerializer;
            var controllerModel = new Mock<ControllerModel>(typeof(JsonRpcTestController).GetTypeInfo(), new List<object>()).Object;
            var actionModel = new Mock<ActionModel>(typeof(JsonRpcTestController).GetMethod(nameof(JsonRpcTestController.VoidAction)), new List<object>()).Object;
            var model = new Mock<ParameterModel>(typeof(JsonRpcTestController).GetMethod(nameof(JsonRpcTestController.VoidAction)).GetParameters()[0], new List<object>()).Object;
            actionModel.Controller = controllerModel;
            model.Action = actionModel;
            var parameterConvention = testEnvironment.ServiceProvider.GetRequiredService<ParameterConvention>();
            var name = serializer.GetJsonName(model.ParameterName);

            var result = parameterConvention.GetParameterMetadata(model, serializerType);

            result.Name.Should().BeEquivalentTo(name);
            result.BindingStyle.Should().Be(BindingStyle.Default);
            result.IsOptional.Should().BeFalse();
            result.Index.Should().Be(0);
        }

        [Test]
        public void Test_GetRpcParameterInfo_UsesOptional()
        {
            var serializerType = typeof(SnakeCaseJsonRpcSerializer);
            var controllerModel = new Mock<ControllerModel>(typeof(JsonRpcTestController).GetTypeInfo(), new List<object>()).Object;
            var actionModel = new Mock<ActionModel>(typeof(JsonRpcTestController).GetMethod(nameof(JsonRpcTestController.VoidAction)), new List<object>()).Object;
            var model = new Mock<ParameterModel>(typeof(JsonRpcTestController).GetMethod(nameof(JsonRpcTestController.VoidAction)).GetParameters()[1], new List<object>()).Object;
            actionModel.Controller = controllerModel;
            model.Action = actionModel;
            var parameterConvention = testEnvironment.ServiceProvider.GetRequiredService<ParameterConvention>();

            var result = parameterConvention.GetParameterMetadata(model, serializerType);

            result.IsOptional.Should().BeTrue();
            result.Index.Should().Be(1);
        }

        [TestCase(BindingStyle.Object)]
        [TestCase(BindingStyle.Array)]
        [TestCase(BindingStyle.Default)]
        public void Test_GetRpcParameterInfo_UsesAttributes(BindingStyle bindingStyle)
        {
            var attribute = new FromParamsAttribute(bindingStyle);
            var serializerType = typeof(SnakeCaseJsonRpcSerializer);
            var controllerModel = new Mock<ControllerModel>(typeof(JsonRpcTestController).GetTypeInfo(), new List<object>()).Object;
            var actionModel = new Mock<ActionModel>(typeof(JsonRpcTestController).GetMethod(nameof(JsonRpcTestController.VoidAction)), new List<object>()).Object;
            var model = new Mock<ParameterModel>(typeof(JsonRpcTestController).GetMethod(nameof(JsonRpcTestController.VoidAction)).GetParameters()[0], new List<object> {attribute}).Object;
            actionModel.Controller = controllerModel;
            model.Action = actionModel;
            var parameterConvention = testEnvironment.ServiceProvider.GetRequiredService<ParameterConvention>();

            var result = parameterConvention.GetParameterMetadata(model, serializerType);

            result.BindingStyle.Should().Be(bindingStyle);
        }

        [Test]
        public void Test_ValidateParameter_FailsWhenArrayBindingNotOnCollection()
        {
            var attribute = new FromParamsAttribute(BindingStyle.Array);
            var controllerModel = new Mock<ControllerModel>(typeof(JsonRpcTestController).GetTypeInfo(), new List<object>()).Object;
            var actionModel = new Mock<ActionModel>(typeof(JsonRpcTestController).GetMethod(nameof(JsonRpcTestController.VoidAction)), new List<object>()).Object;
            var model = new Mock<ParameterModel>(typeof(JsonRpcTestController).GetMethod(nameof(JsonRpcTestController.VoidAction)).GetParameters()[2], new List<object> {attribute}).Object;
            actionModel.Controller = controllerModel;
            model.Action = actionModel;
            var parameterConvention = testEnvironment.ServiceProvider.GetRequiredService<ParameterConvention>();

            Action action = () => parameterConvention.ValidateParameter(model, BindingStyle.Array);

            action.Should().Throw<ArgumentOutOfRangeException>();
        }

        [Test]
        public void Test_ValidateParameter_PassesWhenObject()
        {
            var attribute = new FromParamsAttribute(BindingStyle.Object);
            var controllerModel = new Mock<ControllerModel>(typeof(JsonRpcTestController).GetTypeInfo(), new List<object>()).Object;
            var actionModel = new Mock<ActionModel>(typeof(JsonRpcTestController).GetMethod(nameof(JsonRpcTestController.VoidAction)), new List<object>()).Object;
            var model = new Mock<ParameterModel>(typeof(JsonRpcTestController).GetMethod(nameof(JsonRpcTestController.VoidAction)).GetParameters()[2], new List<object> { attribute }).Object;
            actionModel.Controller = controllerModel;
            model.Action = actionModel;
            var parameterConvention = testEnvironment.ServiceProvider.GetRequiredService<ParameterConvention>();
            Action action = () => parameterConvention.ValidateParameter(model, BindingStyle.Object);

            action.Should().NotThrow();
        }

        [Test]
        public void Test_ValidateParameter_PassesWhenCollection()
        {
            var attribute = new FromParamsAttribute(BindingStyle.Array);
            var controllerModel = new Mock<ControllerModel>(typeof(JsonRpcTestController).GetTypeInfo(), new List<object>()).Object;
            var actionModel = new Mock<ActionModel>(typeof(JsonRpcTestController).GetMethod(nameof(JsonRpcTestController.VoidAction)), new List<object>()).Object;
            var model = new Mock<ParameterModel>(typeof(JsonRpcTestController).GetMethod(nameof(JsonRpcTestController.VoidAction)).GetParameters()[3], new List<object> { attribute }).Object;
            actionModel.Controller = controllerModel;
            model.Action = actionModel;
            var parameterConvention = testEnvironment.ServiceProvider.GetRequiredService<ParameterConvention>();
            Action action = () => parameterConvention.ValidateParameter(model, BindingStyle.Array);

            action.Should().NotThrow();
        }

        [Test]
        public void Test_ValidateParameter_ThrowsOnUnknown()
        {
            var attribute = new FromParamsAttribute(BindingStyle.Array);
            var controllerModel = new Mock<ControllerModel>(typeof(JsonRpcTestController).GetTypeInfo(), new List<object>()).Object;
            var actionModel = new Mock<ActionModel>(typeof(JsonRpcTestController).GetMethod(nameof(JsonRpcTestController.VoidAction)), new List<object>()).Object;
            var model = new Mock<ParameterModel>(typeof(JsonRpcTestController).GetMethod(nameof(JsonRpcTestController.VoidAction)).GetParameters()[3], new List<object> { attribute }).Object;
            actionModel.Controller = controllerModel;
            model.Action = actionModel;
            var parameterConvention = testEnvironment.ServiceProvider.GetRequiredService<ParameterConvention>();
            Action action = () => parameterConvention.ValidateParameter(model, (BindingStyle)(-1));

            action.Should().Throw<ArgumentOutOfRangeException>();
        }

        [Test]
        public void Test_SetBinding_SetsWhenNull()
        {
            var attribute = new FromParamsAttribute(BindingStyle.Array);
            var controllerModel = new Mock<ControllerModel>(typeof(JsonRpcTestController).GetTypeInfo(), new List<object>()).Object;
            var actionModel = new Mock<ActionModel>(typeof(JsonRpcTestController).GetMethod(nameof(JsonRpcTestController.VoidAction)), new List<object>()).Object;
            var model = new Mock<ParameterModel>(typeof(JsonRpcTestController).GetMethod(nameof(JsonRpcTestController.VoidAction)).GetParameters()[2], new List<object> {attribute}).Object;
            actionModel.Controller = controllerModel;
            model.Action = actionModel;
            var parameterConvention = testEnvironment.ServiceProvider.GetRequiredService<ParameterConvention>();

            model.BindingInfo.Should().BeNull();

            parameterConvention.SetBinding(model);

            model.BindingInfo.Should().NotBeNull();
            model.BindingInfo.BinderType.Should().Be<JsonRpcModelBinder>();
            model.BindingInfo.BindingSource.Should().Be(BindingSource.Custom);
        }

        [Test]
        public void Test_SetBinding_IgnoresWhenNotNull()
        {
            var attribute = new FromParamsAttribute(BindingStyle.Array);
            var controllerModel = new Mock<ControllerModel>(typeof(JsonRpcTestController).GetTypeInfo(), new List<object>()).Object;
            var actionModel = new Mock<ActionModel>(typeof(JsonRpcTestController).GetMethod(nameof(JsonRpcTestController.VoidAction)), new List<object>()).Object;
            var model = new Mock<ParameterModel>(typeof(JsonRpcTestController).GetMethod(nameof(JsonRpcTestController.VoidAction)).GetParameters()[2], new List<object> {attribute}).Object;
            actionModel.Controller = controllerModel;
            model.Action = actionModel;
            var bindingInfo = new BindingInfo();
            model.BindingInfo = bindingInfo;
            var parameterConvention = testEnvironment.ServiceProvider.GetRequiredService<ParameterConvention>();

            model.BindingInfo.Should().NotBeNull();

            parameterConvention.SetBinding(model);

            model.BindingInfo.Should().NotBeNull();
            model.BindingInfo.Should().Be(bindingInfo);
        }
    }
}