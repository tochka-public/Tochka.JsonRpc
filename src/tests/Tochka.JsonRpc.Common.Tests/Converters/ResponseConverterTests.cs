using System;
using System.Text.Json;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Tochka.JsonRpc.Common.Models.Id;
using Tochka.JsonRpc.Common.Models.Response;
using Tochka.JsonRpc.Common.Models.Response.Errors;
using Tochka.JsonRpc.Common.Models.Response.Untyped;
using Tochka.JsonRpc.TestUtils;

namespace Tochka.JsonRpc.Common.Tests.Converters;

public class ResponseConverterTests
{
    [Test]
    public void Serialize_UntypedResponse()
    {
        const string id = "id";
        IResponse request = new UntypedResponse(new StringRpcId(id), null);

        var serialized = JsonSerializer.Serialize(request, JsonRpcSerializerOptions.Headers);

        var expected = $$"""
            {
                "id": "{{id}}",
                "result": null,
                "jsonrpc": "2.0"
            }
            """.TrimAllLines();
        serialized.TrimAllLines().Should().Be(expected);
    }

    [Test]
    public void Serialize_UntypedErrorResponse()
    {
        const string id = "id";
        const int errorCode = 123;
        const string errorMessage = "message";
        IResponse request = new UntypedErrorResponse(new StringRpcId(id), new Error<JsonDocument>(errorCode, errorMessage, null));

        var serialized = JsonSerializer.Serialize(request, JsonRpcSerializerOptions.Headers);

        var expected = $$"""
            {
                "id": "{{id}}",
                "error": {
                    "code": {{errorCode}},
                    "message": "{{errorMessage}}",
                    "data": null
                },
                "jsonrpc": "2.0"
            }
            """.TrimAllLines();
        serialized.TrimAllLines().Should().Be(expected);
    }

    [Test]
    public void Serialize_UnknownType_Throw()
    {
        var action = static () => JsonSerializer.Serialize(Mock.Of<IResponse>(), JsonRpcSerializerOptions.Headers);

        action.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Test]
    public void Deserialize_Response()
    {
        const string id = "id";
        var request = $$"""
            {
                "id": "{{id}}",
                "result": null,
                "jsonrpc": "2.0"
            }
            """;

        var deserialized = JsonSerializer.Deserialize<IResponse>(request, JsonRpcSerializerOptions.Headers);

        var expected = new UntypedResponse(new StringRpcId(id), null);
        deserialized.Should().BeOfType<UntypedResponse>().And.BeEquivalentTo(expected);
    }

    [Test]
    public void Deserialize_ErrorResponse()
    {
        const string id = "id";
        const int errorCode = 123;
        const string errorMessage = "message";
        var request = $$"""
            {
                "id": "{{id}}",
                "error": {
                    "code": {{errorCode}},
                    "message": "{{errorMessage}}",
                    "data": null
                },
                "jsonrpc": "2.0"
            }
            """;

        var deserialized = JsonSerializer.Deserialize<IResponse>(request, JsonRpcSerializerOptions.Headers);

        var expected = new UntypedErrorResponse(new StringRpcId(id), new Error<JsonDocument>(errorCode, errorMessage, null));
        deserialized.Should().BeOfType<UntypedErrorResponse>().And.BeEquivalentTo(expected);
    }

    [Test]
    public void Deserialize_NoId_Throw()
    {
        var request = """
            {
                "result": null,
                "jsonrpc": "2.0"
            }
            """;

        var action = () => JsonSerializer.Deserialize<IResponse>(request, JsonRpcSerializerOptions.Headers);

        action.Should().Throw<JsonRpcFormatException>();
    }

    [Test]
    public void Deserialize_NoVersion_Throw()
    {
        const string id = "id";
        var request = $$"""
            {
                "id": "{{id}}",
                "result": null
            }
            """;

        var action = () => JsonSerializer.Deserialize<IResponse>(request, JsonRpcSerializerOptions.Headers);

        action.Should().Throw<JsonRpcFormatException>();
    }

    [Test]
    public void Deserialize_NoResultAndError_Throw()
    {
        const string id = "id";
        var request = $$"""
            {
                "id": "{{id}}",
                "jsonrpc": "2.0"
            }
            """;

        var action = () => JsonSerializer.Deserialize<IResponse>(request, JsonRpcSerializerOptions.Headers);

        action.Should().Throw<JsonRpcFormatException>();
    }

    [Test]
    public void Deserialize_BothResultAndError_Throw()
    {
        const string id = "id";
        var request = $$"""
            {
                "id": "{{id}}",
                "result": null,
                "error": {
                    "code": 123,
                    "message": "message",
                    "data": null
                },
                "jsonrpc": "2.0"
            }
            """;

        var action = () => JsonSerializer.Deserialize<IResponse>(request, JsonRpcSerializerOptions.Headers);

        action.Should().Throw<JsonRpcFormatException>();
    }
}
