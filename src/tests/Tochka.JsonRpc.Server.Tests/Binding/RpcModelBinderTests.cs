using System;
using System.Collections.Generic;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using Tochka.JsonRpc.Common;
using Tochka.JsonRpc.Common.Models.Request;
using Tochka.JsonRpc.Common.Models.Request.Untyped;
using Tochka.JsonRpc.Common.Serializers;
using Tochka.JsonRpc.Server.Binding;
using Tochka.JsonRpc.Server.Models;
using Tochka.JsonRpc.Server.Models.Binding;
using Tochka.JsonRpc.Server.Settings;
using Tochka.JsonRpc.Server.Tests.Helpers;

namespace Tochka.JsonRpc.Server.Tests.Binding
{
    public class RpcModelBinderTests
    {
        private TestEnvironment testEnvironment;

        [SetUp]
        public void Setup()
        {
            testEnvironment = new TestEnvironment(services =>
            {
                services.TryAddJsonRpcSerializer<HeaderRpcSerializer>();
                services.TryAddJsonRpcSerializer<SnakeCaseRpcSerializer>();
                services.AddSingleton(Mock.Of<IParamsParser>());
                services.AddSingleton(Mock.Of<IParameterBinder>());
                services.AddSingleton<RpcModelBinder>();
            });
        }

        [Test]
        public void Test_GetRpcBindingContext_ThrowsOnNoCall()
        {
            var bindingContextMock = MockContext(false);
            var binder = testEnvironment.ServiceProvider.GetRequiredService<RpcModelBinder>();

            Action action = () => binder.GetRpcBindingContext(bindingContextMock.Object, "test");

            action.Should().Throw<KeyNotFoundException>();
            bindingContextMock.VerifyGet(x => x.HttpContext);
        }

        [Test]
        public void Test_GetRpcBindingContext_ThrowsOnNoMetadata()
        {
            var bindingContextMock = MockContext();
            var binder = testEnvironment.ServiceProvider.GetRequiredService<RpcModelBinder>();

            Action action = () => binder.GetRpcBindingContext(bindingContextMock.Object, "test");

            action.Should().Throw<ArgumentNullException>();
            bindingContextMock.VerifyGet(x => x.HttpContext);
            bindingContextMock.VerifyGet(x => x.ActionContext);
        }

        [Test]
        public void Test_GetRpcBindingContext_ThrowsOnNoParameterMetadata()
        {
            var methodMetadata = new MethodMetadata(new JsonRpcMethodOptions(), new JsonName("test", "test"), new JsonName("test", "test"));
            var bindingContextMock = MockContext(methodMetadata: methodMetadata);
            var binder = testEnvironment.ServiceProvider.GetRequiredService<RpcModelBinder>();

            Action action = () => binder.GetRpcBindingContext(bindingContextMock.Object, "test");

            action.Should().Throw<ArgumentNullException>();
            bindingContextMock.VerifyGet(x => x.HttpContext);
            bindingContextMock.VerifyGet(x => x.ActionContext);
        }

        [Test]
        public void Test_GetRpcBindingContext_ThrowsOnNoSerializer()
        {
            var jsonRpcMethodOptions = new JsonRpcMethodOptions()
            {
                RequestSerializer = typeof(CamelCaseRpcSerializer)
            };
            var methodMetadata = new MethodMetadata(jsonRpcMethodOptions, new JsonName("test", "test"), new JsonName("test", "test"));
            methodMetadata.Add(new ParameterMetadata(new JsonName("test", "test"), 0, BindingStyle.Default, false));
            var bindingContextMock = MockContext(methodMetadata: methodMetadata);
            var binder = testEnvironment.ServiceProvider.GetRequiredService<RpcModelBinder>();

            Action action = () => binder.GetRpcBindingContext(bindingContextMock.Object, "test");

            action.Should().Throw<InvalidOperationException>();
            bindingContextMock.VerifyGet(x => x.HttpContext);
            bindingContextMock.VerifyGet(x => x.ActionContext);
            bindingContextMock.VerifyGet(x => x.HttpContext.RequestServices);
        }

        [Test]
        public void Test_GetRpcBindingContext_ReturnsValue()
        {
            var jsonRpcMethodOptions = new JsonRpcMethodOptions();
            var methodMetadata = new MethodMetadata(jsonRpcMethodOptions, new JsonName("test", "test"), new JsonName("test", "test"));
            var parameterMetadata = new ParameterMetadata(new JsonName("test", "test"), 0, BindingStyle.Default, false);
            methodMetadata.Add(parameterMetadata);
            var bindingContextMock = MockContext(methodMetadata: methodMetadata);
            var binder = testEnvironment.ServiceProvider.GetRequiredService<RpcModelBinder>();

            var result = binder.GetRpcBindingContext(bindingContextMock.Object, "test");

            bindingContextMock.VerifyGet(x => x.HttpContext);
            bindingContextMock.VerifyGet(x => x.ActionContext);
            bindingContextMock.VerifyGet(x => x.HttpContext.RequestServices);
            result.Should().NotBeNull();
            result.Call.Should().Be(Mock.Get(result.Call).Object);
            result.ParameterMetadata.Should().Be(parameterMetadata);
            result.Serializer.Should().BeOfType(jsonRpcMethodOptions.RequestSerializer);
        }

        [Test]
        public void Test_BindModelAsync_UsesParserAndBinder()
        {
            var jsonRpcMethodOptions = new JsonRpcMethodOptions();
            var methodMetadata = new MethodMetadata(jsonRpcMethodOptions, new JsonName("test", "test"), new JsonName("test", "test"));
            var parameterMetadata = new ParameterMetadata(new JsonName("test", "test"), 0, BindingStyle.Default, false);
            methodMetadata.Add(parameterMetadata);
            var name = "test";
            var bindingContextMock = MockContext(methodMetadata: methodMetadata, name:name);
            var binder = testEnvironment.ServiceProvider.GetRequiredService<RpcModelBinder>();

            binder.BindModelAsync(bindingContextMock.Object).GetAwaiter().GetResult();

            Mock.Get(testEnvironment.ServiceProvider.GetService<IParamsParser>())
                .Verify(x => x.ParseParams(It.IsAny<JToken>(), parameterMetadata), Times.Once);
            Mock.Get(testEnvironment.ServiceProvider.GetService<IParameterBinder>())
                .Verify(x => x.SetResult(bindingContextMock.Object, It.IsAny<IParseResult>(), name, It.IsAny<RpcBindingContext>()), Times.Once);
        }

        private Mock<ModelBindingContext> MockContext(bool withItem = true, MethodMetadata methodMetadata = null, string name=null)
        {
            var items = new Dictionary<object, object>();
            if (withItem)
            {
                items[JsonRpcConstants.RequestItemKey] = Mock.Of<IUntypedCall>();
            }

            var properties = new Dictionary<object, object>();
            if (methodMetadata != null)
            {
                properties[typeof(MethodMetadata)] = methodMetadata;
            }

            var httpContextMock = new Mock<HttpContext>();
            httpContextMock.SetupGet(x => x.RequestServices).Returns(testEnvironment.ServiceProvider);
            httpContextMock.SetupGet(x => x.Items).Returns(items);

            var actionDescriptorMock = new Mock<ActionDescriptor>();
            actionDescriptorMock.Object.Properties = properties;

            var actionContextMock = new Mock<ActionContext>();
            actionContextMock.Object.ActionDescriptor = actionDescriptorMock.Object;
            actionContextMock.Object.HttpContext = httpContextMock.Object;

            var modelMetadataIdentity = ModelMetadataIdentity.ForProperty(typeof(string), name ?? "null_name", typeof(object));
            var modelMetadataMock = new Mock<ModelMetadata>(modelMetadataIdentity);

            var bindingContextMock = new Mock<ModelBindingContext>();
            bindingContextMock.SetupGet(x => x.HttpContext).Returns(httpContextMock.Object);
            bindingContextMock.SetupGet(x => x.ActionContext).Returns(actionContextMock.Object);
            bindingContextMock.SetupGet(x => x.ModelMetadata).Returns(modelMetadataMock.Object);
            return bindingContextMock;
        }
    }
}