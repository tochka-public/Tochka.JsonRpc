using System;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using Tochka.JsonRpc.Common;
using Tochka.JsonRpc.Common.Models.Response;
using Tochka.JsonRpc.Common.Models.Response.Errors;
using Tochka.JsonRpc.Server.Exceptions;
using Tochka.JsonRpc.Server.Services;
using Tochka.JsonRpc.Server.Settings;
using Tochka.JsonRpc.Server.Tests.Helpers;

namespace Tochka.JsonRpc.Server.Tests.Services
{
    public class JsonRpcErrorFactoryTests
    {
        private TestEnvironment testEnvironment;
        private JsonRpcOptions jsonRpcOptions;

        [SetUp]
        public void Setup()
        {
            jsonRpcOptions = new JsonRpcOptions();
            var optionsMock = new Mock<IOptions<JsonRpcOptions>>();
            optionsMock.Setup(x => x.Value)
                .Returns(jsonRpcOptions);
            testEnvironment = new TestEnvironment(services =>
            {
                services.AddSingleton(optionsMock.Object);
                services.AddSingleton<JsonRpcErrorFactory>();
            });
        }

        [Test]
        public void Test_WrapExceptions_AddsDetails()
        {
            var e = new DivideByZeroException("test");
            jsonRpcOptions.DetailedResponseExceptions = true;
            var errorFactory = testEnvironment.ServiceProvider.GetRequiredService<JsonRpcErrorFactory>();

            var result = errorFactory.WrapExceptions(e);

            result.Should().BeOfType<ExceptionInfo>();
            var info = result as ExceptionInfo;
            info.Type.Should().Be(e.GetType().FullName);
            info.Message.Should().Be(e.Message);
            info.InternalHttpCode.Should().BeNull();
            info.Details.Should().Be(e.ToString());
        }

        [Test]
        public void Test_WrapExceptions_HidesDetails()
        {
            var e = new DivideByZeroException("test");
            jsonRpcOptions.DetailedResponseExceptions = false;
            var errorFactory = testEnvironment.ServiceProvider.GetRequiredService<JsonRpcErrorFactory>();

            var result = errorFactory.WrapExceptions(e);

            result.Should().BeOfType<ExceptionInfo>();
            var info = result as ExceptionInfo;
            info.Type.Should().Be(e.GetType().FullName);
            info.Message.Should().Be(e.Message);
            info.InternalHttpCode.Should().BeNull();
            info.Details.Should().BeNull();
        }

        [TestCase(true)]
        [TestCase(false)]
        public void Test_WrapExceptions_AddsHttpCode(bool addDetails)
        {
            var e = new DivideByZeroException("test");
            jsonRpcOptions.DetailedResponseExceptions = addDetails;
            var errorFactory = testEnvironment.ServiceProvider.GetRequiredService<JsonRpcErrorFactory>();

            var result = errorFactory.WrapExceptions(e, 500);

            result.Should().BeOfType<ExceptionInfo>();
            var info = result as ExceptionInfo;
            info.InternalHttpCode.Should().Be(500);
        }

        [Test]
        public void Test_WrapExceptions_PassesNonException()
        {
            var data = "test";
            var errorFactory = testEnvironment.ServiceProvider.GetRequiredService<JsonRpcErrorFactory>();

            var result = errorFactory.WrapExceptions(data);

            result.Should().BeOfType<string>();
            var value = result as string;
            value.Should().Be(data);
        }

        [Test]
        public void Test_InternalException_CallsHttpErrorAndWraps()
        {
            var data = "test";
            var errorFactory = GetFactoryMock();
            var expected = Mock.Of<IError>();
            errorFactory.Setup(x => x.HttpError(It.IsAny<int?>(), It.IsAny<object>()))
                .Returns(expected);

            var result = errorFactory.Object.InternalException(new JsonRpcInternalException(data));

            result.Should().Be(expected);
            errorFactory.Verify(x => x.HttpError(It.IsAny<int?>(), It.IsAny<object>()));
            errorFactory.Verify(x => x.WrapExceptions(It.IsAny<object>(), It.IsAny<int?>()));
            errorFactory.Verify(x => x.InternalException(It.IsAny<JsonRpcInternalException>()));
            errorFactory.VerifyNoOtherCalls();
        }

        [TestCase(400)]
        [TestCase(422)]
        public void Test_HttpError_ReturnsInvalidParams(int httpCode)
        {
            var data = "test";
            var errorFactory = GetFactoryMock();
            var expected = Mock.Of<IError>();
            errorFactory.Setup(x => x.InvalidParams(It.IsAny<object>()))
                .Returns(expected);

            var result = errorFactory.Object.HttpError(httpCode, data);

            result.Should().Be(expected);
            errorFactory.Verify(x => x.InvalidParams(It.IsAny<object>()));
            errorFactory.Verify(x => x.HttpError(It.IsAny<int?>(), It.IsAny<object>()));
            errorFactory.VerifyNoOtherCalls();
        }

        [TestCase(401)]
        [TestCase(403)]
        public void Test_HttpError_ReturnsMethodNotFound(int httpCode)
        {
            var data = "test";
            var errorFactory = GetFactoryMock();
            var expected = Mock.Of<IError>();
            errorFactory.Setup(x => x.MethodNotFound(It.IsAny<object>()))
                .Returns(expected);

            var result = errorFactory.Object.HttpError(httpCode, data);

            result.Should().Be(expected);
            errorFactory.Verify(x => x.MethodNotFound(It.IsAny<object>()));
            errorFactory.Verify(x => x.HttpError(It.IsAny<int?>(), It.IsAny<object>()));
            errorFactory.VerifyNoOtherCalls();
        }

        [TestCase(404)]
        public void Test_HttpError_ReturnsNotFound(int httpCode)
        {
            var data = "test";
            var errorFactory = GetFactoryMock();
            var expected = Mock.Of<IError>();
            errorFactory.Setup(x => x.NotFound(It.IsAny<object>()))
                .Returns(expected);

            var result = errorFactory.Object.HttpError(httpCode, data);

            result.Should().Be(expected);
            errorFactory.Verify(x => x.NotFound(It.IsAny<object>()));
            errorFactory.Verify(x => x.HttpError(It.IsAny<int?>(), It.IsAny<object>()));
            errorFactory.VerifyNoOtherCalls();
        }

        [TestCase(415)]
        public void Test_HttpError_ReturnsParseError(int httpCode)
        {
            var data = "test";
            var errorFactory = GetFactoryMock();
            var expected = Mock.Of<IError>();
            errorFactory.Setup(x => x.ParseError(It.IsAny<object>()))
                .Returns(expected);

            var result = errorFactory.Object.HttpError(httpCode, data);

            result.Should().Be(expected);
            errorFactory.Verify(x => x.ParseError(It.IsAny<object>()));
            errorFactory.Verify(x => x.HttpError(It.IsAny<int?>(), It.IsAny<object>()));
            errorFactory.VerifyNoOtherCalls();
        }

        [TestCase(500)]
        public void Test_HttpError_ReturnsInternalError(int httpCode)
        {
            var data = "test";
            var errorFactory = GetFactoryMock();
            var expected = Mock.Of<IError>();
            errorFactory.Setup(x => x.InternalError(It.IsAny<object>()))
                .Returns(expected);

            var result = errorFactory.Object.HttpError(httpCode, data);

            result.Should().Be(expected);
            errorFactory.Verify(x => x.InternalError(It.IsAny<object>()));
            errorFactory.Verify(x => x.HttpError(It.IsAny<int?>(), It.IsAny<object>()));
            errorFactory.VerifyNoOtherCalls();
        }

        [Test]
        public void Test_HttpError_ReturnsServerError()
        {
            var data = "test";
            var errorFactory = GetFactoryMock();
            var expected = Mock.Of<IError>();
            errorFactory.Setup(x => x.ServerError(JsonRpcConstants.InternalExceptionCode, It.IsAny<object>()))
                .Returns(expected);

            var result = errorFactory.Object.HttpError(null, data);

            result.Should().Be(expected);
            errorFactory.Verify(x => x.ServerError(JsonRpcConstants.InternalExceptionCode, It.IsAny<object>()));
            errorFactory.Verify(x => x.HttpError(It.IsAny<int?>(), It.IsAny<object>()));
            errorFactory.VerifyNoOtherCalls();
        }

        [Test]
        public void Test_ServerError_ThrowsIfNotServerCode()
        {
            var data = "test";
            var errorFactory = GetFactoryMock();
            Action action = () => errorFactory.Object.ServerError(0, data);

            action.Should().Throw<ArgumentOutOfRangeException>();

            errorFactory.Verify(x => x.ServerError(0, It.IsAny<object>()));
            errorFactory.VerifyNoOtherCalls();
        }

        [Test]
        public void Test_ServerError_WrapsAndReturnsError()
        {
            var data = "test";
            var errorFactory = GetFactoryMock();
            errorFactory.Setup(x => x.WrapExceptions(data, It.IsAny<int?>()))
                .Returns(data);

            var result = errorFactory.Object.ServerError(-32000, data);

            result.Should().BeAssignableTo<IError>();
            result.Code.Should().Be(-32000);
            result.Message.Should().Be("Server error");
            errorFactory.Verify(x => x.ServerError(-32000, It.IsAny<object>()));
            errorFactory.Verify(x => x.WrapExceptions(data, It.IsAny<int?>()));
            errorFactory.VerifyNoOtherCalls();
        }

        [Test]
        public void Test_Exception_ReturnsInternalException()
        {
            var expected = Mock.Of<IError>();
            var errorFactory = GetFactoryMock();
            errorFactory.Setup(x => x.InternalException(It.IsAny<JsonRpcInternalException>()))
                .Returns(expected);

            var result = errorFactory.Object.Exception(new JsonRpcInternalException("test"));

            result.Should().Be(expected);
            errorFactory.Verify(x => x.InternalException(It.IsAny<JsonRpcInternalException>()));
            errorFactory.Verify(x => x.Exception(It.IsAny<JsonRpcInternalException>()));
            errorFactory.VerifyNoOtherCalls();
        }

        [Test]
        public void Test_Exception_WrapsAndReturnsServerError()
        {
            var expected = Mock.Of<IError>();
            var errorFactory = GetFactoryMock();
            errorFactory.Setup(x => x.ServerError(JsonRpcConstants.ExceptionCode, It.IsAny<object>()))
                .Returns(expected);
            errorFactory.Setup(x => x.WrapExceptions(It.IsAny<object>(), It.IsAny<int?>()))
                .Returns(Mock.Of<object>());

            var result = errorFactory.Object.Exception(new DivideByZeroException());

            result.Should().Be(expected);
            errorFactory.Verify(x => x.WrapExceptions(It.IsAny<object>(), It.IsAny<int?>()));
            errorFactory.Verify(x => x.ServerError(JsonRpcConstants.ExceptionCode, It.IsAny<object>()));
            errorFactory.Verify(x => x.Exception(It.IsAny<DivideByZeroException>()));
            errorFactory.VerifyNoOtherCalls();
        }

        [Test]
        public void Test_Error_ThrowsIfReserved()
        {
            var data = "test";
            var errorFactory = GetFactoryMock();
            Action action = () => errorFactory.Object.Error(-32000, data, null);

            action.Should().Throw<ArgumentOutOfRangeException>();

            errorFactory.Verify(x => x.Error(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<object>()));
            errorFactory.VerifyNoOtherCalls();
        }

        [Test]
        public void Test_Error_WrapsAndReturnsError()
        {
            var data = "test";
            var errorFactory = GetFactoryMock();
            errorFactory.Setup(x => x.WrapExceptions(It.IsAny<object>(), It.IsAny<int?>()))
                .Returns(Mock.Of<object>());

            var result = errorFactory.Object.Error(0, data, null);

            result.Code.Should().Be(0);
            result.Message.Should().Be(data);
            errorFactory.Verify(x => x.WrapExceptions(It.IsAny<object>(), It.IsAny<int?>()));
            errorFactory.Verify(x => x.Error(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<object>()));
            errorFactory.VerifyNoOtherCalls();
        }

        [Test]
        public void Test_ParseError_WrapsAndReturnsError()
        {
            var data = "test";
            var errorFactory = GetFactoryMock();
            errorFactory.Setup(x => x.WrapExceptions(It.IsAny<object>(), It.IsAny<int?>()))
                .Returns(Mock.Of<object>());

            var result = errorFactory.Object.ParseError(data);

            result.Code.Should().Be(-32700);
            result.Message.Should().Be("Parse error");
            errorFactory.Verify(x => x.WrapExceptions(It.IsAny<object>(), It.IsAny<int?>()));
            errorFactory.Verify(x => x.ParseError(It.IsAny<object>()));
            errorFactory.VerifyNoOtherCalls();
        }

        [Test]
        public void Test_InternalError_WrapsAndReturnsError()
        {
            var data = "test";
            var errorFactory = GetFactoryMock();
            errorFactory.Setup(x => x.WrapExceptions(It.IsAny<object>(), It.IsAny<int?>()))
                .Returns(Mock.Of<object>());

            var result = errorFactory.Object.InternalError(data);

            result.Code.Should().Be(-32603);
            result.Message.Should().Be("Internal error");
            errorFactory.Verify(x => x.WrapExceptions(It.IsAny<object>(), It.IsAny<int?>()));
            errorFactory.Verify(x => x.InternalError(It.IsAny<object>()));
            errorFactory.VerifyNoOtherCalls();
        }

        [Test]
        public void Test_InvalidParams_WrapsAndReturnsError()
        {
            var data = "test";
            var errorFactory = GetFactoryMock();
            errorFactory.Setup(x => x.WrapExceptions(It.IsAny<object>(), It.IsAny<int?>()))
                .Returns(Mock.Of<object>());

            var result = errorFactory.Object.InvalidParams(data);

            result.Code.Should().Be(-32602);
            result.Message.Should().Be("Invalid params");
            errorFactory.Verify(x => x.WrapExceptions(It.IsAny<object>(), It.IsAny<int?>()));
            errorFactory.Verify(x => x.InvalidParams(It.IsAny<object>()));
            errorFactory.VerifyNoOtherCalls();
        }

        [Test]
        public void Test_MethodNotFound_WrapsAndReturnsError()
        {
            var data = "test";
            var errorFactory = GetFactoryMock();
            errorFactory.Setup(x => x.WrapExceptions(It.IsAny<object>(), It.IsAny<int?>()))
                .Returns(Mock.Of<object>());

            var result = errorFactory.Object.MethodNotFound(data);

            result.Code.Should().Be(-32601);
            result.Message.Should().Be("Method not found");
            errorFactory.Verify(x => x.WrapExceptions(It.IsAny<object>(), It.IsAny<int?>()));
            errorFactory.Verify(x => x.MethodNotFound(It.IsAny<object>()));
            errorFactory.VerifyNoOtherCalls();
        }

        [Test]
        public void Test_InvalidRequest_WrapsAndReturnsError()
        {
            var data = "test";
            var errorFactory = GetFactoryMock();
            errorFactory.Setup(x => x.WrapExceptions(It.IsAny<object>(), It.IsAny<int?>()))
                .Returns(Mock.Of<object>());

            var result = errorFactory.Object.InvalidRequest(data);

            result.Code.Should().Be(-32600);
            result.Message.Should().Be("Invalid Request");
            errorFactory.Verify(x => x.WrapExceptions(It.IsAny<object>(), It.IsAny<int?>()));
            errorFactory.Verify(x => x.InvalidRequest(It.IsAny<object>()));
            errorFactory.VerifyNoOtherCalls();
        }

        [Test]
        public void Test_NotFound_WrapsAndReturnsError()
        {
            var data = "test";
            var errorFactory = GetFactoryMock();
            errorFactory.Setup(x => x.WrapExceptions(It.IsAny<object>(), It.IsAny<int?>()))
                .Returns(Mock.Of<object>());

            var result = errorFactory.Object.NotFound(data);

            result.Code.Should().Be(-32004);
            result.Message.Should().Be("Not found");
            errorFactory.Verify(x => x.WrapExceptions(It.IsAny<object>(), It.IsAny<int?>()));
            errorFactory.Verify(x => x.NotFound(It.IsAny<object>()));
            errorFactory.VerifyNoOtherCalls();
        }

        private Mock<JsonRpcErrorFactory> GetFactoryMock()
        {
            return new Mock<JsonRpcErrorFactory>(testEnvironment.ServiceProvider.GetRequiredService<IOptions<JsonRpcOptions>>(), testEnvironment.ServiceProvider.GetRequiredService<ILogger<JsonRpcErrorFactory>>())
            {
                CallBase = true
            };
        }
    }
}