using System;
using System.Collections.Generic;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;
using Tochka.JsonRpc.V1.Common;
using Tochka.JsonRpc.V1.Common.Serializers;
using Tochka.JsonRpc.V1.Server.Models;
using Tochka.JsonRpc.V1.Server.Pipeline;
using Tochka.JsonRpc.V1.Server.Services;
using Tochka.JsonRpc.V1.Server.Settings;
using Tochka.JsonRpc.V1.Server.Tests.Helpers;

namespace Tochka.JsonRpc.V1.Server.Tests.Pipeline
{
    public class JsonRpcFilterTests
    {
        private TestEnvironment testEnvironment;
        private JsonRpcFilter filter;
        private Mock<IActionResultConverter> converterMock;

        [SetUp]
        public void Setup()
        {
            converterMock = new Mock<IActionResultConverter>(MockBehavior.Strict);
            testEnvironment = new TestEnvironment(services =>
            {
                services.AddSingleton(converterMock.Object);
                services.AddSingleton<SnakeCaseJsonRpcSerializer>();
                services.AddSingleton<JsonRpcFilter>();
            });

            filter = testEnvironment.ServiceProvider.GetRequiredService<JsonRpcFilter>();
        }

        [Test]
        public void Test_OnActionExecuted_DoesNothing()
        {
            var mocks = new[]
            {
                Mock.Of<IList<IFilterMetadata>>(MockBehavior.Strict),
                Mock.Of<object>(MockBehavior.Strict)
            };
            var contextMock = GetContextMock<ActionExecutedContext>(mocks);

            filter.OnActionExecuted(contextMock.Object);

            contextMock.Verify();
            Mock.Get(testEnvironment.ServiceProvider.GetRequiredService<IActionResultConverter>()).VerifyNoOtherCalls();
        }

        [Test]
        public void Test_OnResultExecuted_DoesNothing()
        {
            var mocks = new[]
            {
                Mock.Of<IList<IFilterMetadata>>(MockBehavior.Strict),
                Mock.Of<IActionResult>(MockBehavior.Strict),
                Mock.Of<object>(MockBehavior.Strict)
            };
            var contextMock = GetContextMock<ResultExecutedContext>(mocks);

            filter.OnResultExecuted(contextMock.Object);

            contextMock.Verify();
            Mock.Get(testEnvironment.ServiceProvider.GetRequiredService<IActionResultConverter>()).VerifyNoOtherCalls();
        }

        [TestCase(true)]
        [TestCase(false)]
        public void Test_OnActionExecuing_IgnoresResults(bool modelIsValid)
        {
            var mocks = new[]
            {
                Mock.Of<IList<IFilterMetadata>>(MockBehavior.Strict),
                Mock.Of<IDictionary<string, object>>(MockBehavior.Strict),
                Mock.Of<object>(MockBehavior.Strict)
            };
            var contextMock = GetContextMock<ActionExecutingContext>(mocks);
            contextMock.SetupGet(x => x.Result).Returns(Mock.Of<IActionResult>()).Verifiable();
            if (!modelIsValid)
            {
                contextMock.Object.ModelState.AddModelError("test", "fail");
            }

            filter.OnActionExecuting(contextMock.Object);

            contextMock.Verify();
            contextMock.Object.ModelState.ValidationState
                .Should().Be(modelIsValid ? ModelValidationState.Valid : ModelValidationState.Invalid);
            Mock.Get(testEnvironment.ServiceProvider.GetRequiredService<IActionResultConverter>()).VerifyNoOtherCalls();
        }

        [Test]
        public void Test_OnActionExecuing_IgnoresNullResultWithValidModelState()
        {
            var mocks = new[]
            {
                Mock.Of<IList<IFilterMetadata>>(MockBehavior.Strict),
                Mock.Of<IDictionary<string, object>>(MockBehavior.Strict),
                Mock.Of<object>(MockBehavior.Strict)
            };
            var contextMock = GetContextMock<ActionExecutingContext>(mocks);
            contextMock.SetupGet(x => x.Result).Returns((IActionResult)null).Verifiable();

            filter.OnActionExecuting(contextMock.Object);

            contextMock.Verify();
            contextMock.Object.ModelState.Should().BeEmpty();
            contextMock.Object.ModelState.ValidationState.Should().Be(ModelValidationState.Valid);
            Mock.Get(testEnvironment.ServiceProvider.GetRequiredService<IActionResultConverter>()).VerifyNoOtherCalls();
        }

        [Test]
        public void Test_OnActionExecuing_SetsResultOnNullResultAndInvalidModelState()
        {
            converterMock.Setup(x => x.GetFailedBindingResult(It.IsAny<ModelStateDictionary>()))
                .Returns(Mock.Of<IActionResult>).Verifiable();
            var itemsMock = new Mock<IDictionary<object, object>>(MockBehavior.Strict);
            itemsMock.SetupSet(x => x[JsonRpcConstants.ActionResultTypeItemKey] = It.IsAny<Type>()).Verifiable();
            itemsMock.SetupSet(x => x[JsonRpcConstants.ActionDescriptorItemKey] = It.IsAny<object>()).Verifiable();
            var httpContextMock = new Mock<HttpContext>(MockBehavior.Strict);
            httpContextMock.SetupGet(x => x.Items).Returns(itemsMock.Object).Verifiable();
            var mocks = new[]
            {
                Mock.Of<IList<IFilterMetadata>>(MockBehavior.Strict),
                Mock.Of<IDictionary<string, object>>(MockBehavior.Strict),
                Mock.Of<object>(MockBehavior.Strict)
            };
            var contextMock = GetContextMock<ActionExecutingContext>(mocks);
            contextMock.SetupGet(x => x.Result).Returns((IActionResult)null).Verifiable();
            contextMock.SetupSet(x => x.Result = It.IsAny<IActionResult>()).Verifiable();
            contextMock.Object.HttpContext = httpContextMock.Object;
            contextMock.Object.ModelState.AddModelError("test", "fail");

            filter.OnActionExecuting(contextMock.Object);

            contextMock.Verify();
            contextMock.Object.ModelState.ValidationState.Should().Be(ModelValidationState.Invalid);
            itemsMock.Verify();
            httpContextMock.Verify();
            converterMock.Verify();
        }

        [Test]
        public void Test_OnResultExecuting_ThrowsOnNoMetadata()
        {
            var properties = new Dictionary<object, object>
            {
                //[typeof(MethodMetadata)] = methodMetadata
            };
            var actionDescriptorMock = new Mock<ActionDescriptor>();
            actionDescriptorMock.Object.Properties = properties;
            var itemsMock = new Mock<IDictionary<object, object>>(MockBehavior.Strict);
            var httpContextMock = new Mock<HttpContext>(MockBehavior.Strict);
            var mocks = new[]
            {
                Mock.Of<IList<IFilterMetadata>>(MockBehavior.Strict),
                Mock.Of<IActionResult>(MockBehavior.Strict),
                Mock.Of<object>(MockBehavior.Strict)
            };
            var contextMock = GetContextMock<ResultExecutingContext>(mocks);
            contextMock.SetupSet(x => x.Result = It.IsAny<IActionResult>()).Verifiable();
            contextMock.Object.HttpContext = httpContextMock.Object;
            contextMock.Object.ActionDescriptor = actionDescriptorMock.Object;

            Action action = () => filter.OnResultExecuting(contextMock.Object);

            action.Should().Throw<ArgumentNullException>();
            contextMock.Verify();
            itemsMock.Verify();
            httpContextMock.Verify();
            converterMock.Verify();
        }

        [Test]
        public void Test_OnResultExecuting_ThrowsOnNoSerializer()
        {
            var methodMetadata = new MethodMetadata(new JsonRpcMethodOptions()
            {
                RequestSerializer = typeof(CamelCaseJsonRpcSerializer)
            }, new JsonName("test", "test"), new JsonName("test", "test"));
            var properties = new Dictionary<object, object>
            {
                [typeof(MethodMetadata)] = methodMetadata
            };
            var actionDescriptorMock = new Mock<ActionDescriptor>();
            actionDescriptorMock.Object.Properties = properties;
            var itemsMock = new Mock<IDictionary<object, object>>(MockBehavior.Strict);
            var httpContextMock = new Mock<HttpContext>(MockBehavior.Strict);
            httpContextMock.SetupGet(x => x.RequestServices).Returns(testEnvironment.ServiceProvider).Verifiable();
            var mocks = new[]
            {
                Mock.Of<IList<IFilterMetadata>>(MockBehavior.Strict),
                Mock.Of<IActionResult>(MockBehavior.Strict),
                Mock.Of<object>(MockBehavior.Strict)
            };
            var contextMock = GetContextMock<ResultExecutingContext>(mocks);
            contextMock.SetupSet(x => x.Result = It.IsAny<IActionResult>()).Verifiable();
            contextMock.Object.HttpContext = httpContextMock.Object;
            contextMock.Object.ActionDescriptor = actionDescriptorMock.Object;

            Action action = () => filter.OnResultExecuting(contextMock.Object);

            action.Should().Throw<InvalidOperationException>();
            contextMock.Verify();
            itemsMock.Verify();
            httpContextMock.Verify();
            converterMock.Verify();
        }

        [Test]
        public void Test_OnResultExecuting_UsesConverterAndSetsResult()
        {
            var expectedResult = Mock.Of<IActionResult>(MockBehavior.Strict);
            converterMock.Setup(x => x.ConvertActionResult(It.IsAny<IActionResult>(), It.IsAny<MethodMetadata>(), It.IsAny<IJsonRpcSerializer>()))
                .Returns(expectedResult).Verifiable();
            var methodMetadata = new MethodMetadata(new JsonRpcMethodOptions(), new JsonName("test", "test"), new JsonName("test", "test"));
            var properties = new Dictionary<object, object>
            {
                [typeof(MethodMetadata)] = methodMetadata
            };
            var actionDescriptorMock = new Mock<ActionDescriptor>();
            actionDescriptorMock.Object.Properties = properties;
            var itemsMock = new Mock<IDictionary<object, object>>(MockBehavior.Strict);
            itemsMock.SetupSet(x => x[JsonRpcConstants.ActionResultTypeItemKey] = It.IsAny<object>()).Verifiable();
            itemsMock.SetupSet(x => x[JsonRpcConstants.ActionDescriptorItemKey] = It.IsAny<object>()).Verifiable();
            var httpContextMock = new Mock<HttpContext>(MockBehavior.Strict);
            httpContextMock.SetupGet(x => x.RequestServices).Returns(testEnvironment.ServiceProvider).Verifiable();
            httpContextMock.SetupGet(x => x.Items).Returns(itemsMock.Object).Verifiable();
            var mocks = new[]
            {
                Mock.Of<IList<IFilterMetadata>>(MockBehavior.Strict),
                Mock.Of<IActionResult>(MockBehavior.Strict),
                Mock.Of<object>(MockBehavior.Strict)
            };
            var contextMock = GetContextMock<ResultExecutingContext>(mocks);
            contextMock.SetupProperty(x => x.Result);
            contextMock.Object.Result = Mock.Of<IActionResult>();
            contextMock.Object.HttpContext = httpContextMock.Object;
            contextMock.Object.ActionDescriptor = actionDescriptorMock.Object;

            filter.OnResultExecuting(contextMock.Object);

            contextMock.Verify();
            contextMock.Object.Result.Should().Be(expectedResult);
            itemsMock.Verify();
            httpContextMock.Verify();
            converterMock.Verify();
        }

        private Mock<T> GetContextMock<T>(object[] args)
            where T : class
        {
            var actionContextMock = GetActionContextMock();
            var allArgs = new List<object>
            {
                actionContextMock.Object
            };
            allArgs.AddRange(args);
            return new Mock<T>(MockBehavior.Strict, allArgs.ToArray());
        }

        private Mock<ActionContext> GetActionContextMock()
        {
            var actionContextMock = new Mock<ActionContext>(MockBehavior.Strict);
            actionContextMock.Object.HttpContext = Mock.Of<HttpContext>(MockBehavior.Strict);
            actionContextMock.Object.RouteData = Mock.Of<RouteData>(MockBehavior.Strict);
            actionContextMock.Object.ActionDescriptor = Mock.Of<ActionDescriptor>(MockBehavior.Strict);
            return actionContextMock;
        }
    }
}
