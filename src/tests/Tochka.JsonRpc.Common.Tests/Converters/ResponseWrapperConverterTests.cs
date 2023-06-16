using System;
using System.Collections.Generic;
using System.Text.Json;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Tochka.JsonRpc.Common.Models.Id;
using Tochka.JsonRpc.Common.Models.Response;
using Tochka.JsonRpc.Common.Models.Response.Untyped;
using Tochka.JsonRpc.Common.Models.Response.Wrappers;
using Tochka.JsonRpc.TestUtils;

namespace Tochka.JsonRpc.Common.Tests.Converters;

public class ResponseWrapperConverterTests
{
    [Test]
    public void Serialize_Single()
    {
        const string id = "id";
        IResponseWrapper wrapper = new SingleResponseWrapper(new UntypedResponse(new StringRpcId(id), null));

        var serialized = JsonSerializer.Serialize(wrapper, JsonRpcSerializerOptions.Headers);

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
    public void Serialize_Batch()
    {
        const string id = "id";
        IResponseWrapper request = new BatchResponseWrapper(new List<IResponse> { new UntypedResponse(new StringRpcId(id), null) });

        var serialized = JsonSerializer.Serialize(request, JsonRpcSerializerOptions.Headers);

        var expected = $$"""
            [
                {
                    "id": "{{id}}",
                    "result": null,
                    "jsonrpc": "2.0"
                }
            ]
            """.TrimAllLines();
        serialized.TrimAllLines().Should().Be(expected);
    }

    [Test]
    public void Serialize_UnknownType_Throw()
    {
        var action = static () => JsonSerializer.Serialize(Mock.Of<IResponseWrapper>(), JsonRpcSerializerOptions.Headers);

        action.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Test]
    public void Deserialize_Single()
    {
        var json = """
            {
                "id": "id",
                "result": null,
                "jsonrpc": "2.0"
            }
            """;

        var deserialized = JsonSerializer.Deserialize<IResponseWrapper>(json, JsonRpcSerializerOptions.Headers);

        deserialized.Should().BeOfType<SingleResponseWrapper>();
    }

    [Test]
    public void Deserialize_Batch()
    {
        var json = "[]";

        var deserialized = JsonSerializer.Deserialize<IResponseWrapper>(json, JsonRpcSerializerOptions.Headers);

        deserialized.Should().BeOfType<BatchResponseWrapper>();
    }

    // Other possible values from ECMA-404, Section 5:
    [TestCase("\"\"")]
    [TestCase("\"str\"")]
    [TestCase("0")]
    [TestCase("0.1")]
    [TestCase("true")]
    [TestCase("false")]
    public void Deserialize_UnknownType_Throw(string json)
    {
        var action = () => JsonSerializer.Deserialize<IResponseWrapper>(json, JsonRpcSerializerOptions.Headers);

        action.Should().Throw<JsonRpcFormatException>();
    }
}
