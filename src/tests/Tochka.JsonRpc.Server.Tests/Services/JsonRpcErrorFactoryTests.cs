using System;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using Tochka.JsonRpc.Common;
using Tochka.JsonRpc.Common.Models.Response.Errors;
using Tochka.JsonRpc.Server.Exceptions;
using Tochka.JsonRpc.Server.Services;
using Tochka.JsonRpc.Server.Settings;

namespace Tochka.JsonRpc.Server.Tests.Services;

[TestFixture]
public class JsonRpcErrorFactoryTests
{
    private JsonRpcServerOptions options;
    private Mock<JsonRpcErrorFactory> errorFactoryMock;

    [SetUp]
    public void Setup()
    {
        options = new JsonRpcServerOptions();

        errorFactoryMock = new Mock<JsonRpcErrorFactory>(Options.Create(options), Mock.Of<ILogger<JsonRpcErrorFactory>>())
        {
            CallBase = true
        };
    }

    [Test]
    public void ParseError_WrapException()
    {
        var errorData = 123;
        var wrappedData = 456;
        errorFactoryMock.Setup(f => f.WrapExceptions(errorData))
            .Returns(wrappedData)
            .Verifiable();

        var result = errorFactoryMock.Object.ParseError(errorData);

        var expected = new Error<object>(-32700, "Parse error", wrappedData);
        result.Should().BeEquivalentTo(expected);
        errorFactoryMock.Verify();
    }

    [Test]
    public void InvalidRequest_WrapException()
    {
        var errorData = 123;
        var wrappedData = 456;
        errorFactoryMock.Setup(f => f.WrapExceptions(errorData))
            .Returns(wrappedData)
            .Verifiable();

        var result = errorFactoryMock.Object.InvalidRequest(errorData);

        var expected = new Error<object>(-32600, "Invalid Request", wrappedData);
        result.Should().BeEquivalentTo(expected);
        errorFactoryMock.Verify();
    }

    [Test]
    public void MethodNotFound_WrapException()
    {
        var errorData = 123;
        var wrappedData = 456;
        errorFactoryMock.Setup(f => f.WrapExceptions(errorData))
            .Returns(wrappedData)
            .Verifiable();

        var result = errorFactoryMock.Object.MethodNotFound(errorData);

        var expected = new Error<object>(-32601, "Method not found", wrappedData);
        result.Should().BeEquivalentTo(expected);
        errorFactoryMock.Verify();
    }

    [Test]
    public void InvalidParams_WrapException()
    {
        var errorData = 123;
        var wrappedData = 456;
        errorFactoryMock.Setup(f => f.WrapExceptions(errorData))
            .Returns(wrappedData)
            .Verifiable();

        var result = errorFactoryMock.Object.InvalidParams(errorData);

        var expected = new Error<object>(-32602, "Invalid params", wrappedData);
        result.Should().BeEquivalentTo(expected);
        errorFactoryMock.Verify();
    }

    [Test]
    public void InternalError_WrapException()
    {
        var errorData = 123;
        var wrappedData = 456;
        errorFactoryMock.Setup(f => f.WrapExceptions(errorData))
            .Returns(wrappedData)
            .Verifiable();

        var result = errorFactoryMock.Object.InternalError(errorData);

        var expected = new Error<object>(-32603, "Internal error", wrappedData);
        result.Should().BeEquivalentTo(expected);
        errorFactoryMock.Verify();
    }

    [TestCase(-33000)]
    [TestCase(-32100)]
    [TestCase(-31999)]
    [TestCase(-31000)]
    [TestCase(32012)]
    public void ServerError_NotServerCode_Throw(int code)
    {
        var errorData = 123;

        var action = () => errorFactoryMock.Object.ServerError(code, errorData);

        action.Should().Throw<ArgumentOutOfRangeException>();
    }

    [TestCase(-32099)]
    [TestCase(-32012)]
    [TestCase(-32000)]
    public void ServerError_ServerCode_WrapException(int code)
    {
        var errorData = 123;
        var wrappedData = 456;
        errorFactoryMock.Setup(f => f.WrapExceptions(errorData))
            .Returns(wrappedData)
            .Verifiable();

        var result = errorFactoryMock.Object.ServerError(code, errorData);

        var expected = new Error<object>(code, "Server error", wrappedData);
        result.Should().BeEquivalentTo(expected);
        errorFactoryMock.Verify();
    }

    [Test]
    public void NotFound_WrapException()
    {
        var errorData = 123;
        var wrappedData = 456;
        errorFactoryMock.Setup(f => f.WrapExceptions(errorData))
            .Returns(wrappedData)
            .Verifiable();

        var result = errorFactoryMock.Object.NotFound(errorData);

        var expected = new Error<object>(-32004, "Not found", wrappedData);
        result.Should().BeEquivalentTo(expected);
        errorFactoryMock.Verify();
    }

    [TestCase(-32768)]
    [TestCase(-32123)]
    [TestCase(-32000)]
    public void Error_ReservedCode_Throw(int code)
    {
        var errorData = 123;
        var errorMessage = "errorMessage";

        var action = () => errorFactoryMock.Object.Error(code, errorMessage, errorData);

        action.Should().Throw<ArgumentOutOfRangeException>();
    }

    [TestCase(-33000)]
    [TestCase(-32769)]
    [TestCase(-31999)]
    [TestCase(-31000)]
    [TestCase(32123)]
    public void Error_NotReservedCode_WrapException(int code)
    {
        var errorData = 123;
        var wrappedData = 456;
        var errorMessage = "errorMessage";
        errorFactoryMock.Setup(f => f.WrapExceptions(errorData))
            .Returns(wrappedData)
            .Verifiable();

        var result = errorFactoryMock.Object.Error(code, errorMessage, errorData);

        var expected = new Error<object>(code, errorMessage, wrappedData);
        result.Should().BeEquivalentTo(expected);
        errorFactoryMock.Verify();
    }

    [Test]
    public void Exception_JsonRpcServerException_ReturnServerError()
    {
        var exception = new JsonRpcServerException();
        var wrappedData = 456;
        errorFactoryMock.Setup(f => f.WrapExceptions(exception))
            .Returns(wrappedData)
            .Verifiable();

        var result = errorFactoryMock.Object.Exception(exception);

        var expected = new Error<object>(JsonRpcConstants.InternalExceptionCode, "Server error", wrappedData);
        result.Should().BeEquivalentTo(expected);
        errorFactoryMock.Verify();
    }

    [Test]
    public void Exception_JsonRpcMethodNotFoundException_ReturnMethodNotFoundError()
    {
        var methodName = "methodName";
        var exception = new JsonRpcMethodNotFoundException(methodName);

        var result = errorFactoryMock.Object.Exception(exception);

        var expected = new Error<object>(-32601, "Method not found", new { Method = methodName });
        result.Should().BeEquivalentTo(expected);
    }

    [Test]
    public void Exception_JsonRpcErrorException_ReturnErrorFromException()
    {
        var error = new Error<object>(0, "errorMessage", null);
        var exception = new JsonRpcErrorException(error);

        var result = errorFactoryMock.Object.Exception(exception);

        result.Should().Be(error);
    }

    [Test]
    public void Exception_JsonRpcFormatException_ReturnInvalidRequestError()
    {
        var exception = new JsonRpcFormatException();
        var wrappedData = 456;
        errorFactoryMock.Setup(f => f.WrapExceptions(exception))
            .Returns(wrappedData)
            .Verifiable();

        var result = errorFactoryMock.Object.Exception(exception);

        var expected = new Error<object>(-32600, "Invalid Request", wrappedData);
        result.Should().BeEquivalentTo(expected);
        errorFactoryMock.Verify();
    }

    [Test]
    public void Exception_UnknownException_ReturnServerError()
    {
        var exception = new ArgumentException();
        var wrappedData = 456;
        errorFactoryMock.Setup(f => f.WrapExceptions(exception))
            .Returns(wrappedData)
            .Verifiable();

        var result = errorFactoryMock.Object.Exception(exception);

        var expected = new Error<object>(JsonRpcConstants.ExceptionCode, "Server error", wrappedData);
        result.Should().BeEquivalentTo(expected);
        errorFactoryMock.Verify();
    }

    [TestCase(400)]
    [TestCase(422)]
    public void HttpError_HttpCode400Or422_ReturnInvalidParamsError(int httpCode)
    {
        var errorData = 123;
        var wrappedData = 456;
        errorFactoryMock.Setup(f => f.WrapExceptions(errorData))
            .Returns(wrappedData)
            .Verifiable();

        var result = errorFactoryMock.Object.HttpError(httpCode, errorData);

        var expected = new Error<object>(-32602, "Invalid params", wrappedData);
        result.Should().BeEquivalentTo(expected);
        errorFactoryMock.Verify();
    }

    [TestCase(401)]
    [TestCase(403)]
    public void HttpError_HttpCode401Or403_ReturnMethodNotFoundError(int httpCode)
    {
        var errorData = 123;
        var wrappedData = 456;
        errorFactoryMock.Setup(f => f.WrapExceptions(errorData))
            .Returns(wrappedData)
            .Verifiable();

        var result = errorFactoryMock.Object.HttpError(httpCode, errorData);

        var expected = new Error<object>(-32601, "Method not found", wrappedData);
        result.Should().BeEquivalentTo(expected);
        errorFactoryMock.Verify();
    }

    [Test]
    public void HttpError_HttpCode404_ReturnNotFoundError()
    {
        var errorData = 123;
        var wrappedData = 456;
        errorFactoryMock.Setup(f => f.WrapExceptions(errorData))
            .Returns(wrappedData)
            .Verifiable();

        var result = errorFactoryMock.Object.HttpError(404, errorData);

        var expected = new Error<object>(-32004, "Not found", wrappedData);
        result.Should().BeEquivalentTo(expected);
        errorFactoryMock.Verify();
    }

    [Test]
    public void HttpError_HttpCode415_ReturnParseError()
    {
        var errorData = 123;
        var wrappedData = 456;
        errorFactoryMock.Setup(f => f.WrapExceptions(errorData))
            .Returns(wrappedData)
            .Verifiable();

        var result = errorFactoryMock.Object.HttpError(415, errorData);

        var expected = new Error<object>(-32700, "Parse error", wrappedData);
        result.Should().BeEquivalentTo(expected);
        errorFactoryMock.Verify();
    }

    [Test]
    public void HttpError_HttpCode500_ReturnInternalError()
    {
        var errorData = 123;
        var wrappedData = 456;
        errorFactoryMock.Setup(f => f.WrapExceptions(errorData))
            .Returns(wrappedData)
            .Verifiable();

        var result = errorFactoryMock.Object.HttpError(500, errorData);

        var expected = new Error<object>(-32603, "Internal error", wrappedData);
        result.Should().BeEquivalentTo(expected);
        errorFactoryMock.Verify();
    }

    [TestCase(100)]
    [TestCase(200)]
    [TestCase(300)]
    [TestCase(402)]
    [TestCase(501)]
    public void HttpError_OtherHttpCodes_ReturnServerError(int httpCode)
    {
        var errorData = 123;
        var wrappedData = 456;
        errorFactoryMock.Setup(f => f.WrapExceptions(errorData))
            .Returns(wrappedData)
            .Verifiable();

        var result = errorFactoryMock.Object.HttpError(httpCode, errorData);

        var expected = new Error<object>(JsonRpcConstants.InternalExceptionCode, "Server error", wrappedData);
        result.Should().BeEquivalentTo(expected);
        errorFactoryMock.Verify();
    }

    [TestCase(123)]
    [TestCase("123")]
    [TestCase(true)]
    [TestCase(1.23)]
    [TestCase(null)]
    public void WrapExceptions_ErrorDataNotException_ReturnDataItself(object? errorData)
    {
        var result = errorFactoryMock.Object.WrapExceptions(errorData);

        result.Should().Be(errorData);
    }

    [Test]
    public void WrapExceptions_DetailedResponseExceptionsEnabledInOptions_ReturnErrorWithDetails()
    {
        var exceptionMessage = "exceptionMessage";
        var exception = new ArgumentException(exceptionMessage);
        options.DetailedResponseExceptions = true;

        var result = errorFactoryMock.Object.WrapExceptions(exception);

        var expected = new ExceptionInfo(typeof(ArgumentException).FullName, exceptionMessage, exception.ToString());
        result.Should().BeEquivalentTo(expected);
    }

    [Test]
    public void WrapExceptions_DetailedResponseExceptionsDisabledInOptions_ReturnErrorWithoutDetails()
    {
        var exceptionMessage = "exceptionMessage";
        var exception = new ArgumentException(exceptionMessage);
        options.DetailedResponseExceptions = false;

        var result = errorFactoryMock.Object.WrapExceptions(exception);

        var expected = new ExceptionInfo(typeof(ArgumentException).FullName, exceptionMessage, null);
        result.Should().BeEquivalentTo(expected);
    }
}
