using System;
using System.Text.Json;
using FluentAssertions;
using Json.More;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using Tochka.JsonRpc.Common;
using Tochka.JsonRpc.Common.Models.Id;
using Tochka.JsonRpc.Common.Models.Response.Errors;
using Tochka.JsonRpc.Common.Models.Response.Untyped;
using Tochka.JsonRpc.Server.Services;
using Tochka.JsonRpc.Server.Settings;

namespace Tochka.JsonRpc.Server.Tests.Services;

[TestFixture]
public class JsonRpcExceptionWrapperTests
{
    private Mock<IJsonRpcErrorFactory> errorFactoryMock;
    private JsonRpcServerOptions options;
    private JsonRpcExceptionWrapper exceptionWrapper;

    [SetUp]
    public void Setup()
    {
        errorFactoryMock = new Mock<IJsonRpcErrorFactory>();
        options = new JsonRpcServerOptions();

        exceptionWrapper = new JsonRpcExceptionWrapper(errorFactoryMock.Object, Options.Create(options));
    }

    [Test]
    public void WrapGeneralException_IdIsNull_UseNullRpcId()
    {
        var exception = new ArgumentException();
        errorFactoryMock.Setup(f => f.Exception(exception))
            .Returns(Mock.Of<IError>())
            .Verifiable();

        var result = exceptionWrapper.WrapGeneralException(exception);

        result.Id.Should().BeOfType<NullRpcId>();
        errorFactoryMock.Verify();
    }

    [Test]
    public void WrapGeneralException_IdNotNull_UseId()
    {
        var exception = new ArgumentException();
        var id = new NumberRpcId(123);
        errorFactoryMock.Setup(f => f.Exception(exception))
            .Returns(Mock.Of<IError>())
            .Verifiable();

        var result = exceptionWrapper.WrapGeneralException(exception, id);

        result.Id.Should().Be(id);
        errorFactoryMock.Verify();
    }

    [Test]
    public void WrapGeneralException_ErrorDataIsNull_ReturnUntypedErrorWithNullData()
    {
        var exception = new ArgumentException();
        var errorCode = 123;
        var errorMessage = "errorMessage";
        var error = new Error<object>(errorCode, errorMessage, null);
        errorFactoryMock.Setup(f => f.Exception(exception))
            .Returns(error)
            .Verifiable();

        var result = exceptionWrapper.WrapGeneralException(exception);

        var expectedError = new Error<JsonDocument>(errorCode, errorMessage, null);
        result.Error.Should().BeEquivalentTo(expectedError);
        errorFactoryMock.Verify();
    }

    [Test]
    public void WrapGeneralException_ErrorDataNotNull_ReturnUntypedErrorWithSerializedData()
    {
        var exception = new ArgumentException();
        var errorCode = 123;
        var errorMessage = "errorMessage";
        var errorData = new { A = 456, B = "b" };
        var error = new Error<object>(errorCode, errorMessage, errorData);
        errorFactoryMock.Setup(f => f.Exception(exception))
            .Returns(error)
            .Verifiable();
        options.HeadersJsonSerializerOptions = JsonRpcSerializerOptions.SnakeCase;

        var result = exceptionWrapper.WrapGeneralException(exception);

        var expectedError = new Error<JsonDocument>(errorCode, errorMessage, JsonDocument.Parse("""{ "a": 456, "b": "b" }"""));
        result.Error.Should().BeEquivalentTo(expectedError, static options => options.Using(JsonElementEqualityComparer.Instance));
        errorFactoryMock.Verify();
    }

    [Test]
    public void WrapParseException_ErrorDataIsNull_ReturnUntypedErrorWithNullData()
    {
        var exception = new ArgumentException();
        var errorCode = 123;
        var errorMessage = "errorMessage";
        var error = new Error<object>(errorCode, errorMessage, null);
        errorFactoryMock.Setup(f => f.ParseError(exception))
            .Returns(error)
            .Verifiable();

        var result = exceptionWrapper.WrapParseException(exception);

        var expectedError = new Error<JsonDocument>(errorCode, errorMessage, null);
        var expectedResponse = new UntypedErrorResponse(new NullRpcId(), expectedError);
        result.Should().BeEquivalentTo(expectedResponse);
        errorFactoryMock.Verify();
    }

    [Test]
    public void WrapParseException_ErrorDataNotNull_ReturnUntypedErrorWithSerializedData()
    {
        var exception = new ArgumentException();
        var errorCode = 123;
        var errorMessage = "errorMessage";
        var errorData = new { A = 456, B = "b" };
        var error = new Error<object>(errorCode, errorMessage, errorData);
        errorFactoryMock.Setup(f => f.ParseError(exception))
            .Returns(error)
            .Verifiable();
        options.HeadersJsonSerializerOptions = JsonRpcSerializerOptions.SnakeCase;

        var result = exceptionWrapper.WrapParseException(exception);

        var expectedError = new Error<JsonDocument>(errorCode, errorMessage, JsonDocument.Parse("""{ "a": 456, "b": "b" }"""));
        var expectedResponse = new UntypedErrorResponse(new NullRpcId(), expectedError);
        result.Should().BeEquivalentTo(expectedResponse, static options => options.Using(JsonElementEqualityComparer.Instance));
        errorFactoryMock.Verify();
    }
}
