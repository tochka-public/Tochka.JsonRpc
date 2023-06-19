using System;
using System.Text.Json;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Tochka.JsonRpc.Common.Models.Id;
using Tochka.JsonRpc.Common.Models.Request.Untyped;
using Tochka.JsonRpc.TestUtils;

namespace Tochka.JsonRpc.Common.Tests.Converters;

[TestFixture]
internal class CallConverterTests
{
    [Test]
    public void Serialize_UntypedRequest()
    {
        const string id = "id";
        const string method = "method";
        IUntypedCall request = new UntypedRequest(new StringRpcId(id), method, null);

        var serialized = JsonSerializer.Serialize(request, JsonRpcSerializerOptions.Headers);

        var expected = $$"""
            {
                "id": "{{id}}",
                "method": "{{method}}",
                "params": null,
                "jsonrpc": "2.0"
            }
            """.TrimAllLines();
        serialized.TrimAllLines().Should().Be(expected);
    }

    [Test]
    public void Serialize_UntypedNotification()
    {
        const string method = "method";
        IUntypedCall request = new UntypedNotification(method, null);

        var serialized = JsonSerializer.Serialize(request, JsonRpcSerializerOptions.Headers);

        var expected = $$"""
            {
                "method": "{{method}}",
                "params": null,
                "jsonrpc": "2.0"
            }
            """.TrimAllLines();
        serialized.TrimAllLines().Should().Be(expected);
    }

    [Test]
    public void Serialize_UnknownType_Throw()
    {
        var action = static () => JsonSerializer.Serialize(Mock.Of<IUntypedCall>(), JsonRpcSerializerOptions.Headers);

        action.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Test]
    public void Deserialize_Request()
    {
        const string id = "id";
        const string method = "method";
        var request = $$"""
            {
                "id": "{{id}}",
                "method": "{{method}}",
                "params": null,
                "jsonrpc": "2.0"
            }
            """;

        var deserialized = JsonSerializer.Deserialize<IUntypedCall>(request, JsonRpcSerializerOptions.Headers);

        deserialized.Should().BeOfType<UntypedRequest>().And.BeEquivalentTo(new UntypedRequest(new StringRpcId(id), method, null));
    }

    [Test]
    public void Deserialize_Notification()
    {
        const string method = "method";
        var request = $$"""
            {
                "method": "{{method}}",
                "params": null,
                "jsonrpc": "2.0"
            }
            """;

        var deserialized = JsonSerializer.Deserialize<IUntypedCall>(request, JsonRpcSerializerOptions.Headers);

        deserialized.Should().BeOfType<UntypedNotification>().And.BeEquivalentTo(new UntypedNotification(method, null));
    }

    [Test]
    public void Deserialize_NoMethod_Throw()
    {
        const string id = "id";
        var request = $$"""
            {
                "id": "{{id}}",
                "params": null,
                "jsonrpc": "2.0"
            }
            """;

        var action = () => JsonSerializer.Deserialize<IUntypedCall>(request, JsonRpcSerializerOptions.Headers);

        action.Should().Throw<JsonRpcFormatException>();
    }

    [Test]
    public void Deserialize_NoVersion_Throw()
    {
        const string id = "id";
        const string method = "method";
        var request = $$"""
            {
                "id": "{{id}}",
                "method": "{{method}}",
                "params": null
            }
            """;

        var action = () => JsonSerializer.Deserialize<IUntypedCall>(request, JsonRpcSerializerOptions.Headers);

        action.Should().Throw<JsonRpcFormatException>();
    }
}
