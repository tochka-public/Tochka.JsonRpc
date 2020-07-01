using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using FluentAssertions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.Net.Http.Headers;
using Moq;
using NUnit.Framework;
using Tochka.JsonRpc.Common;
using Tochka.JsonRpc.Common.Serializers;
using Tochka.JsonRpc.Server.Tests.Helpers;

namespace Tochka.JsonRpc.Server.Tests
{
    public class UtilsTests
    {
        [Test]
        public void Test_IsJsonRpcController_True()
        {
            Utils.IsJsonRpcController(typeof(JsonRpcTestController).GetTypeInfo()).Should().BeTrue();
        }

        [Test]
        public void Test_IsJsonRpcController_False()
        {
            Utils.IsJsonRpcController(typeof(MvcTestController).GetTypeInfo()).Should().BeFalse();
        }

        [Test]
        public void Test_GetAttributeTransitive_ReturnsActionWhenHasBothAttribute()
        {
            var actionAttribute = new AuthorizeAttribute("action");
            var controllerAttribute = new AuthorizeAttribute("controller");
            var actionMock = GetActionModelMock(new List<object> {actionAttribute}, new List<object> {controllerAttribute});

            Utils.GetAttributeTransitive<AuthorizeAttribute>(actionMock.Object).Should().Be(actionAttribute);
        }

        [Test]
        public void Test_GetAttributeTransitive_ReturnsActionAttribute()
        {
            var actionAttribute = new AuthorizeAttribute("action");
            var actionMock = GetActionModelMock(new List<object> {actionAttribute}, new List<object> { });

            Utils.GetAttributeTransitive<AuthorizeAttribute>(actionMock.Object).Should().Be(actionAttribute);
        }

        [Test]
        public void Test_GetAttributeTransitive_ReturnsControllerAttribute()
        {
            var controllerAttribute = new AuthorizeAttribute("controller");
            var actionMock = GetActionModelMock(new List<object> { }, new List<object> {controllerAttribute});

            Utils.GetAttributeTransitive<AuthorizeAttribute>(actionMock.Object).Should().Be(controllerAttribute);
        }

        [Test]
        public void Test_GetAttributeTransitive_ReturnsNull()
        {
            var actionMock = GetActionModelMock(new List<object> { }, new List<object> { });

            Utils.GetAttributeTransitive<AuthorizeAttribute>(actionMock.Object).Should().BeNull();
        }

        [Test]
        public void Test_GetAttributeTransitive_ThrowsOnMultipleActionAttributes()
        {
            var actionAttribute = new AuthorizeAttribute("action");
            var actionMock = GetActionModelMock(new List<object> {actionAttribute, actionAttribute}, new List<object> { });

            Action action = () => Utils.GetAttributeTransitive<AuthorizeAttribute>(actionMock.Object);

            action.Should().Throw<InvalidOperationException>();
        }

        [Test]
        public void Test_GetAttributeTransitive_ThrowsOnMultipleControllerAttributes()
        {
            var controllerAttribute = new AuthorizeAttribute("controller");
            var actionMock = GetActionModelMock(new List<object> { }, new List<object> {controllerAttribute, controllerAttribute});

            Action action = () => Utils.GetAttributeTransitive<AuthorizeAttribute>(actionMock.Object);

            action.Should().Throw<InvalidOperationException>();
        }

        [Test]
        public void Test_GetAttribute_ReturnsValue()
        {
            var parameterAttribute = new AuthorizeAttribute("parameter");
            var parameterMock = GetParameterModelMock(new List<object> {parameterAttribute});

            Utils.GetAttribute<AuthorizeAttribute>(parameterMock.Object).Should().Be(parameterAttribute);
        }

        [Test]
        public void Test_GetAttribute_ReturnsNull()
        {
            var parameterMock = GetParameterModelMock(new List<object> { });

            Utils.GetAttribute<AuthorizeAttribute>(parameterMock.Object).Should().BeNull();
        }

        [Test]
        public void Test_GetAttribute_ThrowsOnMultipmeAttributes()
        {
            var parameterAttribute = new AuthorizeAttribute("parameter");
            var parameterMock = GetParameterModelMock(new List<object> {parameterAttribute, parameterAttribute});

            Action action = () => Utils.GetAttribute<AuthorizeAttribute>(parameterMock.Object);

            action.Should().Throw<InvalidOperationException>();
        }

        [TestCaseSource(typeof(UtilsTests), nameof(GoodHttpCodeCases))]
        public void Test_IsGoodHttpCode(int input, bool expected)
        {
            Utils.IsGoodHttpCode(input).Should().Be(expected);
        }

        [Test]
        public void Test_ProbablyIsJsonRpc_ReturnsTrue()
        {
            var request = new DefaultHttpRequest(new DefaultHttpContext())
            {
                Method = HttpMethods.Post
            };
            var contentType = MediaTypeHeaderValue.Parse(JsonRpcConstants.ContentType);
            Utils.ProbablyIsJsonRpc(request, contentType).Should().BeTrue();
        }

        [TestCaseSource(typeof(UtilsTests), nameof(BadHttpMethods))]
        public void Test_ProbablyIsJsonRpc_ReturnsFalseOnMethod(string httpMethod)
        {
            var request = new DefaultHttpRequest(new DefaultHttpContext())
            {
                Method = httpMethod
            };
            var contentType = MediaTypeHeaderValue.Parse(JsonRpcConstants.ContentType);

            Utils.ProbablyIsJsonRpc(request, contentType).Should().BeFalse();
        }

        [Test]
        public void Test_ProbablyIsJsonRpc_ReturnsFalseOnContentType()
        {
            var request = new DefaultHttpRequest(new DefaultHttpContext())
            {
                Method = HttpMethods.Post
            };
            var contentType = MediaTypeHeaderValue.Parse("text/json");

            Utils.ProbablyIsJsonRpc(request, contentType).Should().BeFalse();
        }

        [Test]
        public void Test_ProbablyIsJsonRpc_ReturnsFalseOnNull()
        {
            var request = new DefaultHttpRequest(new DefaultHttpContext())
            {
                Method = HttpMethods.Post
            };

            Utils.ProbablyIsJsonRpc(request, null).Should().BeFalse();
        }

        [Test]
        public void Test_GetActionResultType_ReturnsData()
        {
            var context = new DefaultHttpContext();
            var dataType = typeof(int);
            context.Items.Add(JsonRpcConstants.ActionResultTypeItemKey, dataType);

            Utils.GetActionResultType(context).Should().Be(dataType);
        }

        [Test]
        public void Test_GetActionResultType_ReturnsNull()
        {
            var context = new DefaultHttpContext();

            Utils.GetActionResultType(context).Should().BeNull();
        }

        [Test]
        public void Test_GetSerializer_ReturnsValue()
        {
            var expected = new CamelCaseJsonRpcSerializer();
            var serializers = new List<IJsonRpcSerializer>
            {
                new SnakeCaseJsonRpcSerializer(),
                expected
            };

            Utils.GetSerializer(serializers, expected.GetType()).Should().Be(expected);
        }

        [Test]
        public void Test_GetSerializer_ThrowsOnNotFound()
        {
            var serializers = new List<IJsonRpcSerializer>
            {
                new SnakeCaseJsonRpcSerializer()
            };

            Action action = () => Utils.GetSerializer(serializers, typeof(CamelCaseJsonRpcSerializer));
            action.Should().Throw<InvalidOperationException>();
        }

        private static Mock<ControllerModel> GetControllerModelMock(List<object> controllerAttributes)
        {
            return new Mock<ControllerModel>(Mock.Of<TypeInfo>(), controllerAttributes);
        }

        private static Mock<ActionModel> GetActionModelMock(List<object> actionAttributes, List<object> controllerAttributes)
        {
            var actionMock = new Mock<ActionModel>(Mock.Of<MethodInfo>(), actionAttributes);
            actionMock.Object.Controller = GetControllerModelMock(controllerAttributes).Object;
            return actionMock;
        }

        private static (Mock<ActionModel>, Mock<ControllerModel>) GetActionControllerModelMocks(List<object> actionAttributes, List<object> controllerAttributes)
        {
            var actionMock = new Mock<ActionModel>(Mock.Of<MethodInfo>(), actionAttributes);
            var controllerMock = GetControllerModelMock(controllerAttributes);
            actionMock.Object.Controller = controllerMock.Object;
            return (actionMock, controllerMock);
        }

        private static Mock<ParameterModel> GetParameterModelMock(List<object> parameterAttributes)
        {
            var parameterMock = Mock.Of<ParameterInfo>(x => x.ParameterType == typeof(int));
            return new Mock<ParameterModel>(parameterMock, parameterAttributes);
        }

        private static IEnumerable GoodHttpCodeCases => GoodHttpCodes.Select(data => new TestCaseData(data.input, data.expected));

        private static IEnumerable<(int input, bool expected)> GoodHttpCodes
        {
            get
            {
                foreach (var x in Enumerable.Range(0, 200))
                {
                    yield return (x, false);
                }

                foreach (var x in Enumerable.Range(200, 100))
                {
                    yield return (x, true);
                }

                foreach (var x in Enumerable.Range(300, 700))
                {
                    yield return (x, false);
                }
            }
        }

        private static IEnumerable BadHttpMethods => new List<string>
        {
            HttpMethods.Options,
            HttpMethods.Connect,
            HttpMethods.Delete,
            HttpMethods.Get,
            HttpMethods.Head,
            HttpMethods.Patch,
            HttpMethods.Put,
            HttpMethods.Trace,
        };
    }
}