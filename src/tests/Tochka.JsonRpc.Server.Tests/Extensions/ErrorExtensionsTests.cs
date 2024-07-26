using System.Text.Json;
using FluentAssertions;
using Json.More;
using Moq;
using NUnit.Framework;
using Tochka.JsonRpc.Common;
using Tochka.JsonRpc.Common.Models.Response.Errors;
using Tochka.JsonRpc.Server.Exceptions;
using Tochka.JsonRpc.Server.Extensions;

namespace Tochka.JsonRpc.Server.Tests.Extensions;

[TestFixture]
public class ErrorExtensionsTests
{
    [Test]
    public void ThrowAsException_ThrowJsonRpcErrorExceptionWithError()
    {
        var error = Mock.Of<IError>();

        var action = () => error.ThrowAsException();

        action.Should().Throw<JsonRpcErrorException>().Which.Error.Should().Be(error);
    }

    [Test]
    public void AsException_ReturnJsonRpcErrorExceptionWithError()
    {
        var error = Mock.Of<IError>();

        var result = error.AsException();

        result.Error.Should().Be(error);
    }

    [Test]
    public void AsUntypedError_DataIsNull_ReturnUntypedErrorWithNullData()
    {
        var code = 123;
        var message = "message";
        var error = new Error<object>(code, message, null);

        var result = error.AsUntypedError(new JsonSerializerOptions());

        var expected = new Error<JsonDocument>(code, message, null);
        result.Should().BeEquivalentTo(expected);
    }

    [Test]
    public void AsUntypedError_DataNotNull_ReturnUntypedErrorWithSerializedData()
    {
        var code = 123;
        var message = "message";
        var data = new { A = 123, B = "b" };
        var error = new Error<object>(code, message, data);
        var jsonSerializerOptions = JsonRpcSerializerOptions.SnakeCase;

        var result = error.AsUntypedError(jsonSerializerOptions);

        var expected = new Error<JsonDocument>(code, message, JsonSerializer.SerializeToDocument(data, jsonSerializerOptions));
        result.Should().BeEquivalentTo(expected, static options => options.Using(JsonElementEqualityComparer.Instance));
    }
}
