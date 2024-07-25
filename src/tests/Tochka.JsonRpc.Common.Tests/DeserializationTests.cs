using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.Json;
using FluentAssertions;
using FluentAssertions.Equivalency;
using NUnit.Framework;
using Tochka.JsonRpc.Common.Models.Id;
using Tochka.JsonRpc.Common.Models.Request.Untyped;
using Tochka.JsonRpc.Common.Models.Request.Wrappers;
using Tochka.JsonRpc.Common.Models.Response;
using Tochka.JsonRpc.Common.Models.Response.Errors;
using Tochka.JsonRpc.Common.Models.Response.Untyped;
using Tochka.JsonRpc.Common.Models.Response.Wrappers;
using Tochka.JsonRpc.TestUtils;

namespace Tochka.JsonRpc.Common.Tests;

[TestFixture]
public class DeserializationTests
{
    private readonly JsonSerializerOptions headersJsonSerializerOptions = JsonRpcSerializerOptions.Headers;
    private readonly JsonSerializerOptions snakeCaseSerializerOptions = JsonRpcSerializerOptions.SnakeCase;
    private readonly JsonSerializerOptions camelCaseSerializerOptions = JsonRpcSerializerOptions.CamelCase;

    #region Notification

    [Test]
    public void Notification_EmptyObjectParams()
    {
        var method = "method";
        var json = $$"""
            {
                "method": "{{method}}",
                "params": {},
                "jsonrpc": "2.0"
            }
            """;

        var deserialized = JsonSerializer.Deserialize<IRequestWrapper>(json, headersJsonSerializerOptions);

        deserialized.Should().BeOfType<SingleRequestWrapper>();
        var notification = ((SingleRequestWrapper) deserialized).Call.Deserialize<IUntypedCall>(headersJsonSerializerOptions);
        notification.Method.Should().Be(method);
        var deserializedParams = notification.Params.Deserialize<TestData>(snakeCaseSerializerOptions);
        deserializedParams.Should().NotBeNull();
    }

    [Test]
    public void Notification_EmptyArrayParams()
    {
        var method = "method";
        var json = $$"""
            {
                "method": "{{method}}",
                "params": [],
                "jsonrpc": "2.0"
            }
            """;

        var deserialized = JsonSerializer.Deserialize<IRequestWrapper>(json, headersJsonSerializerOptions);

        deserialized.Should().BeOfType<SingleRequestWrapper>();
        var notification = ((SingleRequestWrapper) deserialized).Call.Deserialize<IUntypedCall>(headersJsonSerializerOptions);
        notification.Method.Should().Be(method);
        var deserializedArray = notification.Params.Deserialize<TestData[]>(snakeCaseSerializerOptions);
        deserializedArray.Should().BeEquivalentTo(Array.Empty<TestData>());
        var deserializedList = notification.Params.Deserialize<List<TestData>>(snakeCaseSerializerOptions);
        deserializedList.Should().BeEquivalentTo(new List<TestData>());
    }

    [Test]
    public void Notification_NullParams()
    {
        var method = "method";
        var json = $$"""
            {
                "method": "{{method}}",
                "params": null,
                "jsonrpc": "2.0"
            }
            """;

        var deserialized = JsonSerializer.Deserialize<IRequestWrapper>(json, headersJsonSerializerOptions);

        deserialized.Should().BeOfType<SingleRequestWrapper>();
        var notification = ((SingleRequestWrapper) deserialized).Call.Deserialize<IUntypedCall>(headersJsonSerializerOptions);
        var expected = new UntypedNotification(method, null);
        notification.Should().BeEquivalentTo(expected);
    }

    [Test]
    public void Notification_NoParams()
    {
        var method = "method";
        var json = $$"""
            {
                "method": "{{method}}",
                "jsonrpc": "2.0"
            }
            """;

        var deserialized = JsonSerializer.Deserialize<IRequestWrapper>(json, headersJsonSerializerOptions);

        deserialized.Should().BeOfType<SingleRequestWrapper>();
        var notification = ((SingleRequestWrapper) deserialized).Call.Deserialize<IUntypedCall>(headersJsonSerializerOptions);
        var expected = new UntypedNotification(method, null);
        notification.Should().BeEquivalentTo(expected);
    }

    [Test]
    public void Notification_PrimitiveTypeParams()
    {
        var method = "method";
        var parameter = 123;
        var json = $$"""
            {
                "method": "{{method}}",
                "params": {{parameter}},
                "jsonrpc": "2.0"
            }
            """;

        var deserialized = JsonSerializer.Deserialize<IRequestWrapper>(json, headersJsonSerializerOptions);

        deserialized.Should().BeOfType<SingleRequestWrapper>();
        var notification = ((SingleRequestWrapper) deserialized).Call.Deserialize<IUntypedCall>(headersJsonSerializerOptions);
        notification.Method.Should().Be(method);
        var deserializedParams = notification.Params.Deserialize<int>(snakeCaseSerializerOptions);
        deserializedParams.Should().Be(parameter);
    }

    [Test]
    public void Notification_PrimitiveTypeArrayParams()
    {
        var method = "method";
        var parameter = 123;
        var json = $$"""
            {
                "method": "{{method}}",
                "params": [
                    {{parameter}}
                ],
                "jsonrpc": "2.0"
            }
            """;

        var deserialized = JsonSerializer.Deserialize<IRequestWrapper>(json, headersJsonSerializerOptions);

        deserialized.Should().BeOfType<SingleRequestWrapper>();
        var notification = ((SingleRequestWrapper) deserialized).Call.Deserialize<IUntypedCall>(headersJsonSerializerOptions);
        notification.Method.Should().Be(method);
        var deserializedArray = notification.Params.Deserialize<int[]>(snakeCaseSerializerOptions);
        deserializedArray.Should().BeEquivalentTo(new[] { parameter });
        var deserializedList = notification.Params.Deserialize<List<int>>(snakeCaseSerializerOptions);
        deserializedList.Should().BeEquivalentTo(new List<int> { parameter });
    }

    [Test]
    public void Notification_PlainSnakeCaseObjectParams()
    {
        var method = "method";
        var json = $$"""
            {
                "method": "{{method}}",
                "params": {{TestData.PlainRequiredSnakeCaseJson}},
                "jsonrpc": "2.0"
            }
            """;

        var deserialized = JsonSerializer.Deserialize<IRequestWrapper>(json, headersJsonSerializerOptions);

        deserialized.Should().BeOfType<SingleRequestWrapper>();
        var notification = ((SingleRequestWrapper) deserialized).Call.Deserialize<IUntypedCall>(headersJsonSerializerOptions);
        notification.Method.Should().Be(method);
        var deserializedParams = notification.Params.Deserialize<TestData>(snakeCaseSerializerOptions);
        deserializedParams.Should().BeEquivalentTo(TestData.Plain);
    }

    [Test]
    public void Notification_PlainCamelCaseObjectParams()
    {
        var method = "method";
        var json = $$"""
            {
                "method": "{{method}}",
                "params": {{TestData.PlainRequiredCamelCaseJson}},
                "jsonrpc": "2.0"
            }
            """;

        var deserialized = JsonSerializer.Deserialize<IRequestWrapper>(json, headersJsonSerializerOptions);

        deserialized.Should().BeOfType<SingleRequestWrapper>();
        var notification = ((SingleRequestWrapper) deserialized).Call.Deserialize<IUntypedCall>(headersJsonSerializerOptions);
        notification.Method.Should().Be(method);
        var deserializedParams = notification.Params.Deserialize<TestData>(camelCaseSerializerOptions);
        deserializedParams.Should().BeEquivalentTo(TestData.Plain);
    }

    [Test]
    public void Notification_PlainSnakeCaseArrayParams()
    {
        var method = "method";
        var json = $$"""
            {
                "method": "{{method}}",
                "params": [
                    {{TestData.PlainRequiredSnakeCaseJson}}
                ],
                "jsonrpc": "2.0"
            }
            """;

        var deserialized = JsonSerializer.Deserialize<IRequestWrapper>(json, headersJsonSerializerOptions);

        deserialized.Should().BeOfType<SingleRequestWrapper>();
        var notification = ((SingleRequestWrapper) deserialized).Call.Deserialize<IUntypedCall>(headersJsonSerializerOptions);
        notification.Method.Should().Be(method);
        var expectedResult = TestData.Plain;
        var deserializedArray = notification.Params.Deserialize<TestData[]>(snakeCaseSerializerOptions);
        deserializedArray.Should().BeEquivalentTo(new[] { expectedResult });
        var deserializedList = notification.Params.Deserialize<List<TestData>>(snakeCaseSerializerOptions);
        deserializedList.Should().BeEquivalentTo(new List<TestData> { expectedResult });
    }

    [Test]
    public void Notification_PlainCamelCaseArrayParams()
    {
        var method = "method";
        var json = $$"""
            {
                "method": "{{method}}",
                "params": [
                    {{TestData.PlainRequiredCamelCaseJson}}
                ],
                "jsonrpc": "2.0"
            }
            """;

        var deserialized = JsonSerializer.Deserialize<IRequestWrapper>(json, headersJsonSerializerOptions);

        deserialized.Should().BeOfType<SingleRequestWrapper>();
        var notification = ((SingleRequestWrapper) deserialized).Call.Deserialize<IUntypedCall>(headersJsonSerializerOptions);
        notification.Method.Should().Be(method);
        var expectedResult = TestData.Plain;
        var deserializedArray = notification.Params.Deserialize<TestData[]>(camelCaseSerializerOptions);
        deserializedArray.Should().BeEquivalentTo(new[] { expectedResult });
        var deserializedList = notification.Params.Deserialize<List<TestData>>(camelCaseSerializerOptions);
        deserializedList.Should().BeEquivalentTo(new List<TestData> { expectedResult });
    }

    [Test]
    public void Notification_NestedSnakeCaseObjectParams()
    {
        var method = "method";
        var json = $$"""
            {
                "method": "{{method}}",
                "params": {{TestData.NestedRequiredSnakeCaseJson}},
                "jsonrpc": "2.0"
            }
            """;

        var deserialized = JsonSerializer.Deserialize<IRequestWrapper>(json, headersJsonSerializerOptions);

        deserialized.Should().BeOfType<SingleRequestWrapper>();
        var notification = ((SingleRequestWrapper) deserialized).Call.Deserialize<IUntypedCall>(headersJsonSerializerOptions);
        notification.Method.Should().Be(method);
        var deserializedParams = notification.Params.Deserialize<TestData>(snakeCaseSerializerOptions);
        deserializedParams.Should().BeEquivalentTo(TestData.Nested);
    }

    [Test]
    public void Notification_NestedCamelCaseObjectParams()
    {
        var method = "method";
        var json = $$"""
            {
                "method": "{{method}}",
                "params": {{TestData.NestedRequiredCamelCaseJson}},
                "jsonrpc": "2.0"
            }
            """;

        var deserialized = JsonSerializer.Deserialize<IRequestWrapper>(json, headersJsonSerializerOptions);

        deserialized.Should().BeOfType<SingleRequestWrapper>();
        var notification = ((SingleRequestWrapper) deserialized).Call.Deserialize<IUntypedCall>(headersJsonSerializerOptions);
        notification.Method.Should().Be(method);
        var deserializedParams = notification.Params.Deserialize<TestData>(camelCaseSerializerOptions);
        deserializedParams.Should().BeEquivalentTo(TestData.Nested);
    }

    [Test]
    public void Notification_NestedSnakeCaseArrayParams()
    {
        var method = "method";
        var json = $$"""
            {
                "method": "{{method}}",
                "params": [
                    {{TestData.NestedRequiredSnakeCaseJson}}
                ],
                "jsonrpc": "2.0"
            }
            """;

        var deserialized = JsonSerializer.Deserialize<IRequestWrapper>(json, headersJsonSerializerOptions);

        deserialized.Should().BeOfType<SingleRequestWrapper>();
        var notification = ((SingleRequestWrapper) deserialized).Call.Deserialize<IUntypedCall>(headersJsonSerializerOptions);
        notification.Method.Should().Be(method);
        var expectedResult = TestData.Nested;
        var deserializedArray = notification.Params.Deserialize<TestData[]>(snakeCaseSerializerOptions);
        deserializedArray.Should().BeEquivalentTo(new[] { expectedResult });
        var deserializedList = notification.Params.Deserialize<List<TestData>>(snakeCaseSerializerOptions);
        deserializedList.Should().BeEquivalentTo(new List<TestData> { expectedResult });
    }

    [Test]
    public void Notification_NestedCamelCaseArrayParams()
    {
        var method = "method";
        var json = $$"""
            {
                "method": "{{method}}",
                "params": [
                    {{TestData.NestedRequiredCamelCaseJson}}
                ],
                "jsonrpc": "2.0"
            }
            """;

        var deserialized = JsonSerializer.Deserialize<IRequestWrapper>(json, headersJsonSerializerOptions);

        deserialized.Should().BeOfType<SingleRequestWrapper>();
        var notification = ((SingleRequestWrapper) deserialized).Call.Deserialize<IUntypedCall>(headersJsonSerializerOptions);
        notification.Method.Should().Be(method);
        var expectedResult = TestData.Nested;
        var deserializedArray = notification.Params.Deserialize<TestData[]>(camelCaseSerializerOptions);
        deserializedArray.Should().BeEquivalentTo(new[] { expectedResult });
        var deserializedList = notification.Params.Deserialize<List<TestData>>(camelCaseSerializerOptions);
        deserializedList.Should().BeEquivalentTo(new List<TestData> { expectedResult });
    }

    #endregion

    #region Request

    [Test]
    public void Request_EmptyObjectParams()
    {
        var method = "method";
        var id = "123";
        var json = $$"""
            {
                "id": "{{id}}",
                "method": "{{method}}",
                "params": {},
                "jsonrpc": "2.0"
            }
            """;

        var deserialized = JsonSerializer.Deserialize<IRequestWrapper>(json, headersJsonSerializerOptions);

        deserialized.Should().BeOfType<SingleRequestWrapper>();
        var request = ((SingleRequestWrapper) deserialized).Call.Deserialize<IUntypedCall>(headersJsonSerializerOptions);
        var expected = new UntypedRequest(new StringRpcId(id), method, AnyJsonDocument);
        request.Should().BeEquivalentTo(expected, AssertionOptions);
        var deserializedParams = request.Params.Deserialize<TestData>(snakeCaseSerializerOptions);
        deserializedParams.Should().NotBeNull();
    }

    [Test]
    public void Request_EmptyArrayParams()
    {
        var method = "method";
        var id = "123";
        var json = $$"""
            {
                "id": "{{id}}",
                "method": "{{method}}",
                "params": [],
                "jsonrpc": "2.0"
            }
            """;

        var deserialized = JsonSerializer.Deserialize<IRequestWrapper>(json, headersJsonSerializerOptions);

        deserialized.Should().BeOfType<SingleRequestWrapper>();
        var request = ((SingleRequestWrapper) deserialized).Call.Deserialize<IUntypedCall>(headersJsonSerializerOptions);
        var expected = new UntypedRequest(new StringRpcId(id), method, AnyJsonDocument);
        request.Should().BeEquivalentTo(expected, AssertionOptions);
        var deserializedArray = request.Params.Deserialize<TestData[]>(snakeCaseSerializerOptions);
        deserializedArray.Should().BeEquivalentTo(Array.Empty<TestData>());
        var deserializedList = request.Params.Deserialize<List<TestData>>(snakeCaseSerializerOptions);
        deserializedList.Should().BeEquivalentTo(new List<TestData>());
    }

    [Test]
    public void Request_NullParams()
    {
        var method = "method";
        var id = "123";
        var json = $$"""
            {
                "id": "{{id}}",
                "method": "{{method}}",
                "params": null,
                "jsonrpc": "2.0"
            }
            """;

        var deserialized = JsonSerializer.Deserialize<IRequestWrapper>(json, headersJsonSerializerOptions);

        deserialized.Should().BeOfType<SingleRequestWrapper>();
        var request = ((SingleRequestWrapper) deserialized).Call.Deserialize<IUntypedCall>(headersJsonSerializerOptions);
        var expected = new UntypedRequest(new StringRpcId(id), method, null);
        request.Should().BeEquivalentTo(expected);
    }

    [Test]
    public void Request_NoParams()
    {
        var method = "method";
        var id = "123";
        var json = $$"""
            {
                "id": "{{id}}",
                "method": "{{method}}",
                "jsonrpc": "2.0"
            }
            """;

        var deserialized = JsonSerializer.Deserialize<IRequestWrapper>(json, headersJsonSerializerOptions);

        deserialized.Should().BeOfType<SingleRequestWrapper>();
        var request = ((SingleRequestWrapper) deserialized).Call.Deserialize<IUntypedCall>(headersJsonSerializerOptions);
        var expected = new UntypedRequest(new StringRpcId(id), method, null);
        request.Should().BeEquivalentTo(expected);
    }

    [Test]
    public void Request_IntId()
    {
        var method = "method";
        var id = 123;
        var json = $$"""
            {
                "id": {{id}},
                "method": "{{method}}",
                "jsonrpc": "2.0"
            }
            """;

        var deserialized = JsonSerializer.Deserialize<IRequestWrapper>(json, headersJsonSerializerOptions);

        deserialized.Should().BeOfType<SingleRequestWrapper>();
        var request = ((SingleRequestWrapper) deserialized).Call.Deserialize<IUntypedCall>(headersJsonSerializerOptions);
        var expected = new UntypedRequest(new NumberRpcId(id), method, null);
        request.Should().BeEquivalentTo(expected);
    }

    [Test]
    public void Request_FloatId()
    {
        var method = "method";
        var id = 123.565;
        var json = $$"""
                     {
                         "id": {{id.ToString(CultureInfo.InvariantCulture)}},
                         "method": "{{method}}",
                         "jsonrpc": "2.0"
                     }
                     """;

        var deserialized = JsonSerializer.Deserialize<IRequestWrapper>(json, headersJsonSerializerOptions);

        deserialized.Should().BeOfType<SingleRequestWrapper>();
        var request = ((SingleRequestWrapper) deserialized).Call.Deserialize<IUntypedCall>(headersJsonSerializerOptions);
        var expected = new UntypedRequest(new FloatNumberRpcId(id), method, null);


        request.Should().BeEquivalentTo(expected);
    }

    [Test]
    public void Request_StringId()
    {
        var method = "method";
        var id = "123";
        var json = $$"""
            {
                "id": "{{id}}",
                "method": "{{method}}",
                "jsonrpc": "2.0"
            }
            """;

        var deserialized = JsonSerializer.Deserialize<IRequestWrapper>(json, headersJsonSerializerOptions);

        deserialized.Should().BeOfType<SingleRequestWrapper>();
        var request = ((SingleRequestWrapper) deserialized).Call.Deserialize<IUntypedCall>(headersJsonSerializerOptions);
        var expected = new UntypedRequest(new StringRpcId(id), method, null);
        request.Should().BeEquivalentTo(expected);
    }

    [Test]
    public void Request_NullId()
    {
        var method = "method";
        var json = $$"""
            {
                "id": null,
                "method": "{{method}}",
                "jsonrpc": "2.0"
            }
            """;

        var deserialized = JsonSerializer.Deserialize<IRequestWrapper>(json, headersJsonSerializerOptions);

        deserialized.Should().BeOfType<SingleRequestWrapper>();
        var request = ((SingleRequestWrapper) deserialized).Call.Deserialize<IUntypedCall>(headersJsonSerializerOptions);
        var expected = new UntypedRequest(new NullRpcId(), method, null);
        request.Should().BeEquivalentTo(expected);
    }

    [Test]
    public void Request_PrimitiveTypeParams()
    {
        var method = "method";
        var id = "123";
        var parameter = 123;
        var json = $$"""
            {
                "id": "{{id}}",
                "method": "{{method}}",
                "params": {{parameter}},
                "jsonrpc": "2.0"
            }
            """;

        var deserialized = JsonSerializer.Deserialize<IRequestWrapper>(json, headersJsonSerializerOptions);

        deserialized.Should().BeOfType<SingleRequestWrapper>();
        var request = ((SingleRequestWrapper) deserialized).Call.Deserialize<IUntypedCall>(headersJsonSerializerOptions);
        var expected = new UntypedRequest(new StringRpcId(id), method, AnyJsonDocument);
        request.Should().BeEquivalentTo(expected, AssertionOptions);
        var deserializedParams = request.Params.Deserialize<int>(snakeCaseSerializerOptions);
        deserializedParams.Should().Be(parameter);
    }

    [Test]
    public void Request_PrimitiveTypeArrayParams()
    {
        var method = "method";
        var id = "123";
        var parameter = 123;
        var json = $$"""
            {
                "id": "{{id}}",
                "method": "{{method}}",
                "params": [
                    {{parameter}}
                ],
                "jsonrpc": "2.0"
            }
            """;

        var deserialized = JsonSerializer.Deserialize<IRequestWrapper>(json, headersJsonSerializerOptions);

        deserialized.Should().BeOfType<SingleRequestWrapper>();
        var request = ((SingleRequestWrapper) deserialized).Call.Deserialize<IUntypedCall>(headersJsonSerializerOptions);
        var expected = new UntypedRequest(new StringRpcId(id), method, AnyJsonDocument);
        request.Should().BeEquivalentTo(expected, AssertionOptions);
        var deserializedArray = request.Params.Deserialize<int[]>(snakeCaseSerializerOptions);
        deserializedArray.Should().BeEquivalentTo(new[] { parameter });
        var deserializedList = request.Params.Deserialize<List<int>>(snakeCaseSerializerOptions);
        deserializedList.Should().BeEquivalentTo(new List<int> { parameter });
    }

    [Test]
    public void Request_PlainSnakeCaseObjectParams()
    {
        var method = "method";
        var id = "123";
        var json = $$"""
            {
                "id": "{{id}}",
                "method": "{{method}}",
                "params": {{TestData.PlainRequiredSnakeCaseJson}},
                "jsonrpc": "2.0"
            }
            """;

        var deserialized = JsonSerializer.Deserialize<IRequestWrapper>(json, headersJsonSerializerOptions);

        deserialized.Should().BeOfType<SingleRequestWrapper>();
        var request = ((SingleRequestWrapper) deserialized).Call.Deserialize<IUntypedCall>(headersJsonSerializerOptions);
        var expected = new UntypedRequest(new StringRpcId(id), method, AnyJsonDocument);
        request.Should().BeEquivalentTo(expected, AssertionOptions);
        var deserializedParams = request.Params.Deserialize<TestData>(snakeCaseSerializerOptions);
        deserializedParams.Should().BeEquivalentTo(TestData.Plain);
    }

    [Test]
    public void Request_PlainCamelCaseObjectParams()
    {
        var method = "method";
        var id = "123";
        var json = $$"""
            {
                "id": "{{id}}",
                "method": "{{method}}",
                "params": {{TestData.PlainRequiredCamelCaseJson}},
                "jsonrpc": "2.0"
            }
            """;

        var deserialized = JsonSerializer.Deserialize<IRequestWrapper>(json, headersJsonSerializerOptions);

        deserialized.Should().BeOfType<SingleRequestWrapper>();
        var request = ((SingleRequestWrapper) deserialized).Call.Deserialize<IUntypedCall>(headersJsonSerializerOptions);
        var expected = new UntypedRequest(new StringRpcId(id), method, AnyJsonDocument);
        request.Should().BeEquivalentTo(expected, AssertionOptions);
        var deserializedParams = request.Params.Deserialize<TestData>(camelCaseSerializerOptions);
        deserializedParams.Should().BeEquivalentTo(TestData.Plain);
    }

    [Test]
    public void Request_PlainSnakeCaseArrayParams()
    {
        var method = "method";
        var id = "123";
        var json = $$"""
            {
                "id": "{{id}}",
                "method": "{{method}}",
                "params": [
                    {{TestData.PlainRequiredSnakeCaseJson}}
                ],
                "jsonrpc": "2.0"
            }
            """;

        var deserialized = JsonSerializer.Deserialize<IRequestWrapper>(json, headersJsonSerializerOptions);

        deserialized.Should().BeOfType<SingleRequestWrapper>();
        var request = ((SingleRequestWrapper) deserialized).Call.Deserialize<IUntypedCall>(headersJsonSerializerOptions);
        var expected = new UntypedRequest(new StringRpcId(id), method, AnyJsonDocument);
        request.Should().BeEquivalentTo(expected, AssertionOptions);
        var expectedResult = TestData.Plain;
        var deserializedArray = request.Params.Deserialize<TestData[]>(snakeCaseSerializerOptions);
        deserializedArray.Should().BeEquivalentTo(new[] { expectedResult });
        var deserializedList = request.Params.Deserialize<List<TestData>>(snakeCaseSerializerOptions);
        deserializedList.Should().BeEquivalentTo(new List<TestData> { expectedResult });
    }

    [Test]
    public void Request_PlainCamelCaseArrayParams()
    {
        var method = "method";
        var id = "123";
        var json = $$"""
            {
                "id": "{{id}}",
                "method": "{{method}}",
                "params": [
                    {{TestData.PlainRequiredCamelCaseJson}}
                ],
                "jsonrpc": "2.0"
            }
            """;

        var deserialized = JsonSerializer.Deserialize<IRequestWrapper>(json, headersJsonSerializerOptions);

        deserialized.Should().BeOfType<SingleRequestWrapper>();
        var request = ((SingleRequestWrapper) deserialized).Call.Deserialize<IUntypedCall>(headersJsonSerializerOptions);
        var expected = new UntypedRequest(new StringRpcId(id), method, AnyJsonDocument);
        request.Should().BeEquivalentTo(expected, AssertionOptions);
        var expectedResult = TestData.Plain;
        var deserializedArray = request.Params.Deserialize<TestData[]>(camelCaseSerializerOptions);
        deserializedArray.Should().BeEquivalentTo(new[] { expectedResult });
        var deserializedList = request.Params.Deserialize<List<TestData>>(camelCaseSerializerOptions);
        deserializedList.Should().BeEquivalentTo(new List<TestData> { expectedResult });
    }

    [Test]
    public void Request_NestedSnakeCaseObjectParams()
    {
        var method = "method";
        var id = "123";
        var json = $$"""
            {
                "id": "{{id}}",
                "method": "{{method}}",
                "params": {{TestData.NestedRequiredSnakeCaseJson}},
                "jsonrpc": "2.0"
            }
            """;

        var deserialized = JsonSerializer.Deserialize<IRequestWrapper>(json, headersJsonSerializerOptions);

        deserialized.Should().BeOfType<SingleRequestWrapper>();
        var request = ((SingleRequestWrapper) deserialized).Call.Deserialize<IUntypedCall>(headersJsonSerializerOptions);
        var expected = new UntypedRequest(new StringRpcId(id), method, AnyJsonDocument);
        request.Should().BeEquivalentTo(expected, AssertionOptions);
        var deserializedParams = request.Params.Deserialize<TestData>(snakeCaseSerializerOptions);
        deserializedParams.Should().BeEquivalentTo(TestData.Nested);
    }

    [Test]
    public void Request_NestedCamelCaseObjectParams()
    {
        var method = "method";
        var id = "123";
        var json = $$"""
            {
                "id": "{{id}}",
                "method": "{{method}}",
                "params": {{TestData.NestedRequiredCamelCaseJson}},
                "jsonrpc": "2.0"
            }
            """;

        var deserialized = JsonSerializer.Deserialize<IRequestWrapper>(json, headersJsonSerializerOptions);

        deserialized.Should().BeOfType<SingleRequestWrapper>();
        var request = ((SingleRequestWrapper) deserialized).Call.Deserialize<IUntypedCall>(headersJsonSerializerOptions);
        var expected = new UntypedRequest(new StringRpcId(id), method, AnyJsonDocument);
        request.Should().BeEquivalentTo(expected, AssertionOptions);
        var deserializedParams = request.Params.Deserialize<TestData>(camelCaseSerializerOptions);
        deserializedParams.Should().BeEquivalentTo(TestData.Nested);
    }

    [Test]
    public void Request_NestedSnakeCaseArrayParams()
    {
        var method = "method";
        var id = "123";
        var json = $$"""
            {
                "id": "{{id}}",
                "method": "{{method}}",
                "params": [
                    {{TestData.NestedRequiredSnakeCaseJson}}
                ],
                "jsonrpc": "2.0"
            }
            """;

        var deserialized = JsonSerializer.Deserialize<IRequestWrapper>(json, headersJsonSerializerOptions);

        deserialized.Should().BeOfType<SingleRequestWrapper>();
        var request = ((SingleRequestWrapper) deserialized).Call.Deserialize<IUntypedCall>(headersJsonSerializerOptions);
        var expected = new UntypedRequest(new StringRpcId(id), method, AnyJsonDocument);
        request.Should().BeEquivalentTo(expected, AssertionOptions);
        var expectedResult = TestData.Nested;
        var deserializedArray = request.Params.Deserialize<TestData[]>(snakeCaseSerializerOptions);
        deserializedArray.Should().BeEquivalentTo(new[] { expectedResult });
        var deserializedList = request.Params.Deserialize<List<TestData>>(snakeCaseSerializerOptions);
        deserializedList.Should().BeEquivalentTo(new List<TestData> { expectedResult });
    }

    [Test]
    public void Request_NestedCamelCaseArrayParams()
    {
        var method = "method";
        var id = "123";
        var json = $$"""
            {
                "id": "{{id}}",
                "method": "{{method}}",
                "params": [
                    {{TestData.NestedRequiredCamelCaseJson}}
                ],
                "jsonrpc": "2.0"
            }
            """;

        var deserialized = JsonSerializer.Deserialize<IRequestWrapper>(json, headersJsonSerializerOptions);

        deserialized.Should().BeOfType<SingleRequestWrapper>();
        var request = ((SingleRequestWrapper) deserialized).Call.Deserialize<IUntypedCall>(headersJsonSerializerOptions);
        var expected = new UntypedRequest(new StringRpcId(id), method, AnyJsonDocument);
        request.Should().BeEquivalentTo(expected, AssertionOptions);
        var expectedResult = TestData.Nested;
        var deserializedArray = request.Params.Deserialize<TestData[]>(camelCaseSerializerOptions);
        deserializedArray.Should().BeEquivalentTo(new[] { expectedResult });
        var deserializedList = request.Params.Deserialize<List<TestData>>(camelCaseSerializerOptions);
        deserializedList.Should().BeEquivalentTo(new List<TestData> { expectedResult });
    }

    #endregion

    #region RequestResponse

    [Test]
    public void RequestResponse_EmptyObjectResult()
    {
        var id = "123";
        var json = $$"""
            {
                "id": "{{id}}",
                "result": {},
                "jsonrpc": "2.0"
            }
            """;

        var deserialized = JsonSerializer.Deserialize<IResponseWrapper>(json, headersJsonSerializerOptions);

        var expected = new SingleResponseWrapper(new UntypedResponse(new StringRpcId(id), AnyJsonDocument));
        deserialized.Should().BeEquivalentTo(expected, AssertionOptions);
        var response = (UntypedResponse) ((SingleResponseWrapper) deserialized).Response;
        var deserializedResult = response.Result.Deserialize<TestData>(snakeCaseSerializerOptions);
        deserializedResult.Should().NotBeNull();
    }

    [Test]
    public void RequestResponse_EmptyArrayResult()
    {
        var id = "123";
        var json = $$"""
            {
                "id": "{{id}}",
                "result": [],
                "jsonrpc": "2.0"
            }
            """;

        var deserialized = JsonSerializer.Deserialize<IResponseWrapper>(json, headersJsonSerializerOptions);

        var expected = new SingleResponseWrapper(new UntypedResponse(new StringRpcId(id), AnyJsonDocument));
        deserialized.Should().BeEquivalentTo(expected, AssertionOptions);
        var response = (UntypedResponse) ((SingleResponseWrapper) deserialized).Response;
        var deserializedArray = response.Result.Deserialize<TestData[]>(snakeCaseSerializerOptions);
        deserializedArray.Should().BeEquivalentTo(Array.Empty<TestData>());
        var deserializedList = response.Result.Deserialize<List<TestData>>(snakeCaseSerializerOptions);
        deserializedList.Should().BeEquivalentTo(new List<TestData>());
    }

    [Test]
    public void RequestResponse_NullResult()
    {
        var id = "123";
        var json = $$"""
            {
                "id": "{{id}}",
                "result": null,
                "jsonrpc": "2.0"
            }
            """;

        var deserialized = JsonSerializer.Deserialize<IResponseWrapper>(json, headersJsonSerializerOptions);

        var expected = new SingleResponseWrapper(new UntypedResponse(new StringRpcId(id), null));
        deserialized.Should().BeEquivalentTo(expected);
    }

    [Test]
    public void RequestResponse_IntId()
    {
        var id = 123;
        var json = $$"""
            {
                "id": {{id}},
                "result": {},
                "jsonrpc": "2.0"
            }
            """;

        var deserialized = JsonSerializer.Deserialize<IResponseWrapper>(json, headersJsonSerializerOptions);

        var expected = new SingleResponseWrapper(new UntypedResponse(new NumberRpcId(id), AnyJsonDocument));
        deserialized.Should().BeEquivalentTo(expected, AssertionOptions);
    }

    [Test]
    public void RequestResponse_FloatId()
    {
        var id = 123.565;
        var json = $$"""
                     {
                         "id": {{id.ToString(CultureInfo.InvariantCulture)}},
                         "result": {},
                         "jsonrpc": "2.0"
                     }
                     """;

        var deserialized = JsonSerializer.Deserialize<IResponseWrapper>(json, headersJsonSerializerOptions);

        var expected = new SingleResponseWrapper(new UntypedResponse(new FloatNumberRpcId(id), AnyJsonDocument));
        deserialized.Should().BeEquivalentTo(expected, AssertionOptions);
    }

    [Test]
    public void RequestResponse_StringId()
    {
        var id = "123";
        var json = $$"""
            {
                "id": "{{id}}",
                "result": {},
                "jsonrpc": "2.0"
            }
            """;

        var deserialized = JsonSerializer.Deserialize<IResponseWrapper>(json, headersJsonSerializerOptions);

        var expected = new SingleResponseWrapper(new UntypedResponse(new StringRpcId(id), AnyJsonDocument));
        deserialized.Should().BeEquivalentTo(expected, AssertionOptions);
    }

    [Test]
    public void RequestResponse_NullId()
    {
        var json = """
            {
                "id": null,
                "result": {},
                "jsonrpc": "2.0"
            }
            """;

        var deserialized = JsonSerializer.Deserialize<IResponseWrapper>(json, headersJsonSerializerOptions);

        var expected = new SingleResponseWrapper(new UntypedResponse(new NullRpcId(), AnyJsonDocument));
        deserialized.Should().BeEquivalentTo(expected, AssertionOptions);
    }

    [Test]
    public void RequestResponse_PrimitiveTypeResult()
    {
        var id = "123";
        var result = 123;
        var json = $$"""
            {
                "id": "{{id}}",
                "result": {{result}},
                "jsonrpc": "2.0"
            }
            """;

        var deserialized = JsonSerializer.Deserialize<IResponseWrapper>(json, headersJsonSerializerOptions);

        var expected = new SingleResponseWrapper(new UntypedResponse(new StringRpcId(id), AnyJsonDocument));
        deserialized.Should().BeEquivalentTo(expected, AssertionOptions);
        var response = (UntypedResponse) ((SingleResponseWrapper) deserialized).Response;
        var deserializedResult = response.Result.Deserialize<int>(snakeCaseSerializerOptions);
        deserializedResult.Should().Be(result);
    }

    [Test]
    public void RequestResponse_PrimitiveTypeArrayResult()
    {
        var id = "123";
        var result = 123;
        var json = $$"""
            {
                "id": "{{id}}",
                "result": [
                    {{result}}
                ],
                "jsonrpc": "2.0"
            }
            """;

        var deserialized = JsonSerializer.Deserialize<IResponseWrapper>(json, headersJsonSerializerOptions);

        var expected = new SingleResponseWrapper(new UntypedResponse(new StringRpcId(id), AnyJsonDocument));
        deserialized.Should().BeEquivalentTo(expected, AssertionOptions);
        var response = (UntypedResponse) ((SingleResponseWrapper) deserialized).Response;
        var deserializedArray = response.Result.Deserialize<int[]>(snakeCaseSerializerOptions);
        deserializedArray.Should().BeEquivalentTo(new[] { result });
        var deserializedList = response.Result.Deserialize<List<int>>(snakeCaseSerializerOptions);
        deserializedList.Should().BeEquivalentTo(new List<int> { result });
    }

    [Test]
    public void RequestResponse_ResultWithErrorField()
    {
        var id = "123";
        var error = "errorValue";
        var json = $$"""
            {
                "id": "{{id}}",
                "result": {
                    "error": "{{error}}"
                },
                "jsonrpc": "2.0"
            }
            """;

        var deserialized = JsonSerializer.Deserialize<IResponseWrapper>(json, headersJsonSerializerOptions);

        var expected = new SingleResponseWrapper(new UntypedResponse(new StringRpcId(id), AnyJsonDocument));
        deserialized.Should().BeEquivalentTo(expected, AssertionOptions);
        var expectedResult = new ResultWithError(error);
        var response = (UntypedResponse) ((SingleResponseWrapper) deserialized).Response;
        var deserializedResult = response.Result.Deserialize<ResultWithError>(snakeCaseSerializerOptions);
        deserializedResult.Should().BeEquivalentTo(expectedResult);
    }

    [Test]
    public void RequestResponse_PlainSnakeCaseObjectResult()
    {
        var id = "123";
        var json = $$"""
            {
                "id": "{{id}}",
                "result": {{TestData.PlainRequiredSnakeCaseJson}},
                "jsonrpc": "2.0"
            }
            """;

        var deserialized = JsonSerializer.Deserialize<IResponseWrapper>(json, headersJsonSerializerOptions);

        var expected = new SingleResponseWrapper(new UntypedResponse(new StringRpcId(id), AnyJsonDocument));
        deserialized.Should().BeEquivalentTo(expected, AssertionOptions);
        var expectedResult = TestData.Plain;
        var response = (UntypedResponse) ((SingleResponseWrapper) deserialized).Response;
        var deserializedResult = response.Result.Deserialize<TestData>(snakeCaseSerializerOptions);
        deserializedResult.Should().BeEquivalentTo(expectedResult);
    }

    [Test]
    public void RequestResponse_PlainCamelCaseObjectResult()
    {
        var id = "123";
        var json = $$"""
            {
                "id": "{{id}}",
                "result": {{TestData.PlainRequiredCamelCaseJson}},
                "jsonrpc": "2.0"
            }
            """;

        var deserialized = JsonSerializer.Deserialize<IResponseWrapper>(json, headersJsonSerializerOptions);

        var expected = new SingleResponseWrapper(new UntypedResponse(new StringRpcId(id), AnyJsonDocument));
        deserialized.Should().BeEquivalentTo(expected, AssertionOptions);
        var expectedResult = TestData.Plain;
        var response = (UntypedResponse) ((SingleResponseWrapper) deserialized).Response;
        var deserializedResult = response.Result.Deserialize<TestData>(camelCaseSerializerOptions);
        deserializedResult.Should().BeEquivalentTo(expectedResult);
    }

    [Test]
    public void RequestResponse_PlainSnakeCaseArrayResult()
    {
        var id = "123";
        var json = $$"""
            {
                "id": "{{id}}",
                "result": [
                    {{TestData.PlainRequiredSnakeCaseJson}}
                ],
                "jsonrpc": "2.0"
            }
            """;

        var deserialized = JsonSerializer.Deserialize<IResponseWrapper>(json, headersJsonSerializerOptions);

        var expected = new SingleResponseWrapper(new UntypedResponse(new StringRpcId(id), AnyJsonDocument));
        deserialized.Should().BeEquivalentTo(expected, AssertionOptions);
        var expectedResult = TestData.Plain;
        var response = (UntypedResponse) ((SingleResponseWrapper) deserialized).Response;
        var deserializedArray = response.Result.Deserialize<TestData[]>(snakeCaseSerializerOptions);
        deserializedArray.Should().BeEquivalentTo(new[] { expectedResult });
        var deserializedList = response.Result.Deserialize<List<TestData>>(snakeCaseSerializerOptions);
        deserializedList.Should().BeEquivalentTo(new List<TestData> { expectedResult });
    }

    [Test]
    public void RequestResponse_PlainCamelCaseArrayResult()
    {
        var id = "123";
        var json = $$"""
            {
                "id": "{{id}}",
                "result": [
                    {{TestData.PlainRequiredCamelCaseJson}}
                ],
                "jsonrpc": "2.0"
            }
            """;

        var deserialized = JsonSerializer.Deserialize<IResponseWrapper>(json, headersJsonSerializerOptions);

        var expected = new SingleResponseWrapper(new UntypedResponse(new StringRpcId(id), AnyJsonDocument));
        deserialized.Should().BeEquivalentTo(expected, AssertionOptions);
        var expectedResult = TestData.Plain;
        var response = (UntypedResponse) ((SingleResponseWrapper) deserialized).Response;
        var deserializedArray = response.Result.Deserialize<TestData[]>(camelCaseSerializerOptions);
        deserializedArray.Should().BeEquivalentTo(new[] { expectedResult });
        var deserializedList = response.Result.Deserialize<List<TestData>>(camelCaseSerializerOptions);
        deserializedList.Should().BeEquivalentTo(new List<TestData> { expectedResult });
    }

    [Test]
    public void RequestResponse_NestedSnakeCaseObjectResult()
    {
        var id = "123";
        var json = $$"""
            {
                "id": "{{id}}",
                "result": {{TestData.NestedRequiredSnakeCaseJson}},
                "jsonrpc": "2.0"
            }
            """;

        var deserialized = JsonSerializer.Deserialize<IResponseWrapper>(json, headersJsonSerializerOptions);

        var expected = new SingleResponseWrapper(new UntypedResponse(new StringRpcId(id), AnyJsonDocument));
        deserialized.Should().BeEquivalentTo(expected, AssertionOptions);
        var expectedResult = TestData.Nested;
        var response = (UntypedResponse) ((SingleResponseWrapper) deserialized).Response;
        var deserializedResult = response.Result.Deserialize<TestData>(snakeCaseSerializerOptions);
        deserializedResult.Should().BeEquivalentTo(expectedResult);
    }

    [Test]
    public void RequestResponse_NestedCamelCaseObjectResult()
    {
        var id = "123";
        var json = $$"""
            {
                "id": "{{id}}",
                "result": {{TestData.NestedRequiredCamelCaseJson}},
                "jsonrpc": "2.0"
            }
            """;

        var deserialized = JsonSerializer.Deserialize<IResponseWrapper>(json, headersJsonSerializerOptions);

        var expected = new SingleResponseWrapper(new UntypedResponse(new StringRpcId(id), AnyJsonDocument));
        deserialized.Should().BeEquivalentTo(expected, AssertionOptions);
        var expectedResult = TestData.Nested;
        var response = (UntypedResponse) ((SingleResponseWrapper) deserialized).Response;
        var deserializedResult = response.Result.Deserialize<TestData>(camelCaseSerializerOptions);
        deserializedResult.Should().BeEquivalentTo(expectedResult);
    }

    [Test]
    public void RequestResponse_NestedSnakeCaseArrayResult()
    {
        var id = "123";
        var json = $$"""
            {
                "id": "{{id}}",
                "result": [
                    {{TestData.NestedRequiredSnakeCaseJson}}
                ],
                "jsonrpc": "2.0"
            }
            """;

        var deserialized = JsonSerializer.Deserialize<IResponseWrapper>(json, headersJsonSerializerOptions);

        var expected = new SingleResponseWrapper(new UntypedResponse(new StringRpcId(id), AnyJsonDocument));
        deserialized.Should().BeEquivalentTo(expected, AssertionOptions);
        var expectedResult = TestData.Nested;
        var response = (UntypedResponse) ((SingleResponseWrapper) deserialized).Response;
        var deserializedArray = response.Result.Deserialize<TestData[]>(snakeCaseSerializerOptions);
        deserializedArray.Should().BeEquivalentTo(new[] { expectedResult });
        var deserializedList = response.Result.Deserialize<List<TestData>>(snakeCaseSerializerOptions);
        deserializedList.Should().BeEquivalentTo(new List<TestData> { expectedResult });
    }

    [Test]
    public void RequestResponse_NestedCamelCaseArrayResult()
    {
        var id = "123";
        var json = $$"""
            {
                "id": "{{id}}",
                "result": [
                    {{TestData.NestedRequiredCamelCaseJson}}
                ],
                "jsonrpc": "2.0"
            }
            """;

        var deserialized = JsonSerializer.Deserialize<IResponseWrapper>(json, headersJsonSerializerOptions);

        var expected = new SingleResponseWrapper(new UntypedResponse(new StringRpcId(id), AnyJsonDocument));
        deserialized.Should().BeEquivalentTo(expected, AssertionOptions);
        var expectedResult = TestData.Nested;
        var response = (UntypedResponse) ((SingleResponseWrapper) deserialized).Response;
        var deserializedArray = response.Result.Deserialize<TestData[]>(camelCaseSerializerOptions);
        deserializedArray.Should().BeEquivalentTo(new[] { expectedResult });
        var deserializedList = response.Result.Deserialize<List<TestData>>(camelCaseSerializerOptions);
        deserializedList.Should().BeEquivalentTo(new List<TestData> { expectedResult });
    }

    [Test]
    public void RequestResponse_EmptyObjectErrorData()
    {
        var id = "123";
        var errorCode = 123;
        var errorMessage = "errorMessage";
        var json = $$"""
            {
                "id": "{{id}}",
                "error": {
                    "code": {{errorCode}},
                    "message": "{{errorMessage}}",
                    "data": {}
                },
                "jsonrpc": "2.0"
            }
            """;

        var deserialized = JsonSerializer.Deserialize<IResponseWrapper>(json, headersJsonSerializerOptions);

        var expectedError = new Error<JsonDocument>(errorCode, errorMessage, AnyJsonDocument);
        var expected = new SingleResponseWrapper(new UntypedErrorResponse(new StringRpcId(id), expectedError));
        deserialized.Should().BeEquivalentTo(expected, AssertionOptions);
        var response = (UntypedErrorResponse) ((SingleResponseWrapper) deserialized).Response;
        var deserializedErrorData = response.Error.Data.Deserialize<TestData>(snakeCaseSerializerOptions);
        deserializedErrorData.Should().NotBeNull();
    }

    [Test]
    public void RequestResponse_EmptyArrayErrorData()
    {
        var id = "123";
        var errorCode = 123;
        var errorMessage = "errorMessage";
        var json = $$"""
            {
                "id": "{{id}}",
                "error": {
                    "code": {{errorCode}},
                    "message": "{{errorMessage}}",
                    "data": []
                },
                "jsonrpc": "2.0"
            }
            """;

        var deserialized = JsonSerializer.Deserialize<IResponseWrapper>(json, headersJsonSerializerOptions);

        var expectedError = new Error<JsonDocument>(errorCode, errorMessage, AnyJsonDocument);
        var expected = new SingleResponseWrapper(new UntypedErrorResponse(new StringRpcId(id), expectedError));
        deserialized.Should().BeEquivalentTo(expected, AssertionOptions);
        var response = (UntypedErrorResponse) ((SingleResponseWrapper) deserialized).Response;
        var deserializedArray = response.Error.Data.Deserialize<TestData[]>(snakeCaseSerializerOptions);
        deserializedArray.Should().BeEquivalentTo(Array.Empty<TestData>());
        var deserializedList = response.Error.Data.Deserialize<List<TestData>>(snakeCaseSerializerOptions);
        deserializedList.Should().BeEquivalentTo(new List<TestData>());
    }

    [Test]
    public void RequestResponse_NullErrorData()
    {
        var id = "123";
        var errorCode = 123;
        var errorMessage = "errorMessage";
        var json = $$"""
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

        var deserialized = JsonSerializer.Deserialize<IResponseWrapper>(json, headersJsonSerializerOptions);

        var expectedError = new Error<JsonDocument>(errorCode, errorMessage, AnyJsonDocument);
        var expected = new SingleResponseWrapper(new UntypedErrorResponse(new StringRpcId(id), expectedError));
        deserialized.Should().BeEquivalentTo(expected, AssertionOptions);
        var response = (UntypedErrorResponse) ((SingleResponseWrapper) deserialized).Response;
        response.Error.Data.Should().BeNull();
    }

    [Test]
    public void RequestResponse_NoErrorData()
    {
        var id = "123";
        var errorCode = 123;
        var errorMessage = "errorMessage";
        var json = $$"""
            {
                "id": "{{id}}",
                "error": {
                    "code": {{errorCode}},
                    "message": "{{errorMessage}}"
                },
                "jsonrpc": "2.0"
            }
            """;

        var deserialized = JsonSerializer.Deserialize<IResponseWrapper>(json, headersJsonSerializerOptions);

        var expectedError = new Error<JsonDocument>(errorCode, errorMessage, AnyJsonDocument);
        var expected = new SingleResponseWrapper(new UntypedErrorResponse(new StringRpcId(id), expectedError));
        deserialized.Should().BeEquivalentTo(expected, AssertionOptions);
        var response = (UntypedErrorResponse) ((SingleResponseWrapper) deserialized).Response;
        response.Error.Data.Should().BeNull();
    }

    [Test]
    public void RequestResponse_PrimitiveTypeErrorData()
    {
        var id = "123";
        var errorCode = 123;
        var errorMessage = "errorMessage";
        var errorData = 123;
        var json = $$"""
            {
                "id": "{{id}}",
                "error": {
                    "code": {{errorCode}},
                    "message": "{{errorMessage}}",
                    "data": {{errorData}}
                },
                "jsonrpc": "2.0"
            }
            """;

        var deserialized = JsonSerializer.Deserialize<IResponseWrapper>(json, headersJsonSerializerOptions);

        var expectedError = new Error<JsonDocument>(errorCode, errorMessage, AnyJsonDocument);
        var expected = new SingleResponseWrapper(new UntypedErrorResponse(new StringRpcId(id), expectedError));
        deserialized.Should().BeEquivalentTo(expected, AssertionOptions);
        var response = (UntypedErrorResponse) ((SingleResponseWrapper) deserialized).Response;
        var deserializedErrorData = response.Error.Data.Deserialize<int>(snakeCaseSerializerOptions);
        deserializedErrorData.Should().Be(errorData);
    }

    [Test]
    public void RequestResponse_PrimitiveTypeArrayErrorData()
    {
        var id = "123";
        var errorCode = 123;
        var errorMessage = "errorMessage";
        var errorData = 123;
        var json = $$"""
            {
                "id": "{{id}}",
                "error": {
                    "code": {{errorCode}},
                    "message": "{{errorMessage}}",
                    "data": [
                        {{errorData}}
                    ]
                },
                "jsonrpc": "2.0"
            }
            """;

        var deserialized = JsonSerializer.Deserialize<IResponseWrapper>(json, headersJsonSerializerOptions);

        var expectedError = new Error<JsonDocument>(errorCode, errorMessage, AnyJsonDocument);
        var expected = new SingleResponseWrapper(new UntypedErrorResponse(new StringRpcId(id), expectedError));
        deserialized.Should().BeEquivalentTo(expected, AssertionOptions);
        var response = (UntypedErrorResponse) ((SingleResponseWrapper) deserialized).Response;
        var deserializedArray = response.Error.Data.Deserialize<int[]>(snakeCaseSerializerOptions);
        deserializedArray.Should().BeEquivalentTo(new[] { errorData });
        var deserializedList = response.Error.Data.Deserialize<List<int>>(snakeCaseSerializerOptions);
        deserializedList.Should().BeEquivalentTo(new List<int> { errorData });
    }

    [Test]
    public void RequestResponse_ErrorDataWithResultField()
    {
        var id = "123";
        var errorCode = 123;
        var errorMessage = "errorMessage";
        var result = "resultValue";
        var json = $$"""
            {
                "id": "{{id}}",
                "error": {
                    "code": {{errorCode}},
                    "message": "{{errorMessage}}",
                    "data": {
                        "result": "{{result}}"
                    }
                },
                "jsonrpc": "2.0"
            }
            """;

        var deserialized = JsonSerializer.Deserialize<IResponseWrapper>(json, headersJsonSerializerOptions);

        var expectedError = new Error<JsonDocument>(errorCode, errorMessage, AnyJsonDocument);
        var expected = new SingleResponseWrapper(new UntypedErrorResponse(new StringRpcId(id), expectedError));
        deserialized.Should().BeEquivalentTo(expected, AssertionOptions);
        var expectedErrorData = new ErrorWithResult(result);
        var response = (UntypedErrorResponse) ((SingleResponseWrapper) deserialized).Response;
        var deserializedErrorData = response.Error.Data.Deserialize<ErrorWithResult>(snakeCaseSerializerOptions);
        deserializedErrorData.Should().BeEquivalentTo(expectedErrorData);
    }

    [Test]
    public void RequestResponse_PlainSnakeCaseObjectErrorData()
    {
        var id = "123";
        var errorCode = 123;
        var errorMessage = "errorMessage";
        var json = $$"""
            {
                "id": "{{id}}",
                "error": {
                    "code": {{errorCode}},
                    "message": "{{errorMessage}}",
                    "data": {{TestData.PlainRequiredSnakeCaseJson}}
                },
                "jsonrpc": "2.0"
            }
            """;

        var deserialized = JsonSerializer.Deserialize<IResponseWrapper>(json, headersJsonSerializerOptions);

        var expectedError = new Error<JsonDocument>(errorCode, errorMessage, AnyJsonDocument);
        var expected = new SingleResponseWrapper(new UntypedErrorResponse(new StringRpcId(id), expectedError));
        deserialized.Should().BeEquivalentTo(expected, AssertionOptions);
        var expectedErrorData = TestData.Plain;
        var response = (UntypedErrorResponse) ((SingleResponseWrapper) deserialized).Response;
        var deserializedErrorData = response.Error.Data.Deserialize<TestData>(snakeCaseSerializerOptions);
        deserializedErrorData.Should().BeEquivalentTo(expectedErrorData);
    }

    [Test]
    public void RequestResponse_PlainCamelCaseObjectErrorData()
    {
        var id = "123";
        var errorCode = 123;
        var errorMessage = "errorMessage";
        var json = $$"""
            {
                "id": "{{id}}",
                "error": {
                    "code": {{errorCode}},
                    "message": "{{errorMessage}}",
                    "data": {{TestData.PlainRequiredCamelCaseJson}}
                },
                "jsonrpc": "2.0"
            }
            """;

        var deserialized = JsonSerializer.Deserialize<IResponseWrapper>(json, headersJsonSerializerOptions);

        var expectedError = new Error<JsonDocument>(errorCode, errorMessage, AnyJsonDocument);
        var expected = new SingleResponseWrapper(new UntypedErrorResponse(new StringRpcId(id), expectedError));
        deserialized.Should().BeEquivalentTo(expected, AssertionOptions);
        var expectedErrorData = TestData.Plain;
        var response = (UntypedErrorResponse) ((SingleResponseWrapper) deserialized).Response;
        var deserializedErrorData = response.Error.Data.Deserialize<TestData>(camelCaseSerializerOptions);
        deserializedErrorData.Should().BeEquivalentTo(expectedErrorData);
    }

    [Test]
    public void RequestResponse_PlainSnakeCaseArrayErrorData()
    {
        var id = "123";
        var errorCode = 123;
        var errorMessage = "errorMessage";
        var json = $$"""
            {
                "id": "{{id}}",
                "error": {
                    "code": {{errorCode}},
                    "message": "{{errorMessage}}",
                    "data": [
                        {{TestData.PlainRequiredSnakeCaseJson}}
                    ]
                },
                "jsonrpc": "2.0"
            }
            """;

        var deserialized = JsonSerializer.Deserialize<IResponseWrapper>(json, headersJsonSerializerOptions);

        var expectedError = new Error<JsonDocument>(errorCode, errorMessage, AnyJsonDocument);
        var expected = new SingleResponseWrapper(new UntypedErrorResponse(new StringRpcId(id), expectedError));
        deserialized.Should().BeEquivalentTo(expected, AssertionOptions);
        var expectedErrorData = TestData.Plain;
        var response = (UntypedErrorResponse) ((SingleResponseWrapper) deserialized).Response;
        var deserializedArray = response.Error.Data.Deserialize<TestData[]>(snakeCaseSerializerOptions);
        deserializedArray.Should().BeEquivalentTo(new[] { expectedErrorData });
        var deserializedList = response.Error.Data.Deserialize<List<TestData>>(snakeCaseSerializerOptions);
        deserializedList.Should().BeEquivalentTo(new List<TestData> { expectedErrorData });
    }

    [Test]
    public void RequestResponse_PlainCamelCaseArrayErrorData()
    {
        var id = "123";
        var errorCode = 123;
        var errorMessage = "errorMessage";
        var json = $$"""
            {
                "id": "{{id}}",
                "error": {
                    "code": {{errorCode}},
                    "message": "{{errorMessage}}",
                    "data": [
                        {{TestData.PlainRequiredCamelCaseJson}}
                    ]
                },
                "jsonrpc": "2.0"
            }
            """;

        var deserialized = JsonSerializer.Deserialize<IResponseWrapper>(json, headersJsonSerializerOptions);

        var expectedError = new Error<JsonDocument>(errorCode, errorMessage, AnyJsonDocument);
        var expected = new SingleResponseWrapper(new UntypedErrorResponse(new StringRpcId(id), expectedError));
        deserialized.Should().BeEquivalentTo(expected, AssertionOptions);
        var expectedErrorData = TestData.Plain;
        var response = (UntypedErrorResponse) ((SingleResponseWrapper) deserialized).Response;
        var deserializedArray = response.Error.Data.Deserialize<TestData[]>(camelCaseSerializerOptions);
        deserializedArray.Should().BeEquivalentTo(new[] { expectedErrorData });
        var deserializedList = response.Error.Data.Deserialize<List<TestData>>(camelCaseSerializerOptions);
        deserializedList.Should().BeEquivalentTo(new List<TestData> { expectedErrorData });
    }

    [Test]
    public void RequestResponse_NestedSnakeCaseObjectErrorData()
    {
        var id = "123";
        var errorCode = 123;
        var errorMessage = "errorMessage";
        var json = $$"""
            {
                "id": "{{id}}",
                "error": {
                    "code": {{errorCode}},
                    "message": "{{errorMessage}}",
                    "data": {{TestData.NestedRequiredSnakeCaseJson}}
                },
                "jsonrpc": "2.0"
            }
            """;

        var deserialized = JsonSerializer.Deserialize<IResponseWrapper>(json, headersJsonSerializerOptions);

        var expectedError = new Error<JsonDocument>(errorCode, errorMessage, AnyJsonDocument);
        var expected = new SingleResponseWrapper(new UntypedErrorResponse(new StringRpcId(id), expectedError));
        deserialized.Should().BeEquivalentTo(expected, AssertionOptions);
        var expectedErrorData = TestData.Nested;
        var response = (UntypedErrorResponse) ((SingleResponseWrapper) deserialized).Response;
        var deserializedErrorData = response.Error.Data.Deserialize<TestData>(snakeCaseSerializerOptions);
        deserializedErrorData.Should().BeEquivalentTo(expectedErrorData);
    }

    [Test]
    public void RequestResponse_NestedCamelCaseObjectErrorData()
    {
        var id = "123";
        var errorCode = 123;
        var errorMessage = "errorMessage";
        var json = $$"""
            {
                "id": "{{id}}",
                "error": {
                    "code": {{errorCode}},
                    "message": "{{errorMessage}}",
                    "data": {{TestData.NestedRequiredCamelCaseJson}}
                },
                "jsonrpc": "2.0"
            }
            """;

        var deserialized = JsonSerializer.Deserialize<IResponseWrapper>(json, headersJsonSerializerOptions);

        var expectedError = new Error<JsonDocument>(errorCode, errorMessage, AnyJsonDocument);
        var expected = new SingleResponseWrapper(new UntypedErrorResponse(new StringRpcId(id), expectedError));
        deserialized.Should().BeEquivalentTo(expected, AssertionOptions);
        var expectedErrorData = TestData.Nested;
        var response = (UntypedErrorResponse) ((SingleResponseWrapper) deserialized).Response;
        var deserializedErrorData = response.Error.Data.Deserialize<TestData>(camelCaseSerializerOptions);
        deserializedErrorData.Should().BeEquivalentTo(expectedErrorData);
    }

    [Test]
    public void RequestResponse_NestedSnakeCaseArrayErrorData()
    {
        var id = "123";
        var errorCode = 123;
        var errorMessage = "errorMessage";
        var json = $$"""
            {
                "id": "{{id}}",
                "error": {
                    "code": {{errorCode}},
                    "message": "{{errorMessage}}",
                    "data": [
                        {{TestData.NestedRequiredSnakeCaseJson}}
                    ]
                },
                "jsonrpc": "2.0"
            }
            """;

        var deserialized = JsonSerializer.Deserialize<IResponseWrapper>(json, headersJsonSerializerOptions);

        var expectedError = new Error<JsonDocument>(errorCode, errorMessage, AnyJsonDocument);
        var expected = new SingleResponseWrapper(new UntypedErrorResponse(new StringRpcId(id), expectedError));
        deserialized.Should().BeEquivalentTo(expected, AssertionOptions);
        var expectedErrorData = TestData.Nested;
        var response = (UntypedErrorResponse) ((SingleResponseWrapper) deserialized).Response;
        var deserializedArray = response.Error.Data.Deserialize<TestData[]>(snakeCaseSerializerOptions);
        deserializedArray.Should().BeEquivalentTo(new[] { expectedErrorData });
        var deserializedList = response.Error.Data.Deserialize<List<TestData>>(snakeCaseSerializerOptions);
        deserializedList.Should().BeEquivalentTo(new List<TestData> { expectedErrorData });
    }

    [Test]
    public void RequestResponse_NestedCamelCaseArrayErrorData()
    {
        var id = "123";
        var errorCode = 123;
        var errorMessage = "errorMessage";
        var json = $$"""
            {
                "id": "{{id}}",
                "error": {
                    "code": {{errorCode}},
                    "message": "{{errorMessage}}",
                    "data": [
                        {{TestData.NestedRequiredCamelCaseJson}}
                    ]
                },
                "jsonrpc": "2.0"
            }
            """;

        var deserialized = JsonSerializer.Deserialize<IResponseWrapper>(json, headersJsonSerializerOptions);

        var expectedError = new Error<JsonDocument>(errorCode, errorMessage, AnyJsonDocument);
        var expected = new SingleResponseWrapper(new UntypedErrorResponse(new StringRpcId(id), expectedError));
        deserialized.Should().BeEquivalentTo(expected, AssertionOptions);
        var expectedErrorData = TestData.Nested;
        var response = (UntypedErrorResponse) ((SingleResponseWrapper) deserialized).Response;
        var deserializedArray = response.Error.Data.Deserialize<TestData[]>(camelCaseSerializerOptions);
        deserializedArray.Should().BeEquivalentTo(new[] { expectedErrorData });
        var deserializedList = response.Error.Data.Deserialize<List<TestData>>(camelCaseSerializerOptions);
        deserializedList.Should().BeEquivalentTo(new List<TestData> { expectedErrorData });
    }

    [Test]
    public void RequestResponse_NeitherResultNorError_ThrowJsonRpcFormatException()
    {
        var id = "123";
        var json = $$"""
            {
                "id": "{{id}}",
                "jsonrpc": "2.0"
            }
            """;

        var act = () => JsonSerializer.Deserialize<IResponseWrapper>(json, headersJsonSerializerOptions);

        act.Should().Throw<JsonRpcFormatException>();
    }

    [Test]
    public void RequestResponse_NoId_ThrowJsonRpcFormatException()
    {
        var json = """
            {
                "result": {},
                "jsonrpc": "2.0"
            }
            """;

        var act = () => JsonSerializer.Deserialize<IResponseWrapper>(json, headersJsonSerializerOptions);

        act.Should().Throw<JsonRpcFormatException>();
    }

    [Test]
    public void RequestResponse_RootElementNotObject_ThrowJsonRpcFormatException()
    {
        var json = """
            123
            """;

        var act = () => JsonSerializer.Deserialize<IResponseWrapper>(json, headersJsonSerializerOptions);

        act.Should().Throw<JsonRpcFormatException>();
    }

    [Test]
    public void RequestResponse_UnsupportedIdType_ThrowJsonRpcFormatException()
    {
        var json = """
            {
                "id": {},
                "jsonrpc": "2.0",
                "result": {}
            }
            """;

        var act = () => JsonSerializer.Deserialize<IResponseWrapper>(json, headersJsonSerializerOptions);

        act.Should().Throw<JsonRpcFormatException>();
    }

    #endregion

    #region Batch

    [Test]
    public void Batch_1Notification()
    {
        var method = "method";
        var json = $$"""
            [
                {
                    "method": "{{method}}",
                    "params": {},
                    "jsonrpc": "2.0"
                }
            ]
            """;

        var deserialized = JsonSerializer.Deserialize<IRequestWrapper>(json, headersJsonSerializerOptions);

        deserialized.Should().BeOfType<BatchRequestWrapper>();
        var calls = ((BatchRequestWrapper) deserialized).Calls
            .Select(c => c.Deserialize<IUntypedCall>(headersJsonSerializerOptions));
        var expectedCalls = new List<IUntypedCall>
        {
            new UntypedNotification(method, AnyJsonDocument)
        };
        calls.Should().BeEquivalentTo(expectedCalls, AssertionOptions);
    }

    [Test]
    public void Batch_OnlyNotifications()
    {
        var method1 = "method1";
        var method2 = "method2";
        var json = $$"""
            [
                {
                    "method": "{{method1}}",
                    "params": {},
                    "jsonrpc": "2.0"
                },
                {
                    "method": "{{method2}}",
                    "params": {},
                    "jsonrpc": "2.0"
                }
            ]
            """;

        var deserialized = JsonSerializer.Deserialize<IRequestWrapper>(json, headersJsonSerializerOptions);

        deserialized.Should().BeOfType<BatchRequestWrapper>();
        var calls = ((BatchRequestWrapper) deserialized).Calls
            .Select(c => c.Deserialize<IUntypedCall>(headersJsonSerializerOptions));
        var expectedCalls = new List<IUntypedCall>
        {
            new UntypedNotification(method1, AnyJsonDocument),
            new UntypedNotification(method2, AnyJsonDocument)
        };
        calls.Should().BeEquivalentTo(expectedCalls, AssertionOptions);
    }

    [Test]
    public void Batch_1Request()
    {
        var id = "123";
        var method = "method";
        var json = $$"""
            [
                {
                    "id": "{{id}}",
                    "method": "{{method}}",
                    "params": {},
                    "jsonrpc": "2.0"
                }
            ]
            """;

        var deserialized = JsonSerializer.Deserialize<IRequestWrapper>(json, headersJsonSerializerOptions);

        deserialized.Should().BeOfType<BatchRequestWrapper>();
        var calls = ((BatchRequestWrapper) deserialized).Calls
            .Select(c => c.Deserialize<IUntypedCall>(headersJsonSerializerOptions));
        var expectedCalls = new List<IUntypedCall>
        {
            new UntypedRequest(new StringRpcId(id), method, AnyJsonDocument)
        };
        calls.Should().BeEquivalentTo(expectedCalls, AssertionOptions);
    }

    [Test]
    public void Batch_OnlyRequests()
    {
        var id1 = "123";
        var id2 = "456";
        var method1 = "method1";
        var method2 = "method2";
        var json = $$"""
            [
                {
                    "id": "{{id1}}",
                    "method": "{{method1}}",
                    "params": {},
                    "jsonrpc": "2.0"
                },
                {
                    "id": "{{id2}}",
                    "method": "{{method2}}",
                    "params": {},
                    "jsonrpc": "2.0"
                }
            ]
            """;

        var deserialized = JsonSerializer.Deserialize<IRequestWrapper>(json, headersJsonSerializerOptions);

        deserialized.Should().BeOfType<BatchRequestWrapper>();
        var calls = ((BatchRequestWrapper) deserialized).Calls
            .Select(c => c.Deserialize<IUntypedCall>(headersJsonSerializerOptions));
        var expectedCalls = new List<IUntypedCall>
        {
            new UntypedRequest(new StringRpcId(id1), method1, AnyJsonDocument),
            new UntypedRequest(new StringRpcId(id2), method2, AnyJsonDocument)
        };
        calls.Should().BeEquivalentTo(expectedCalls, AssertionOptions);
    }

    [Test]
    public void Batch_NotificationsAndRequests()
    {
        var id1 = "123";
        var method1 = "method1";
        var method2 = "method2";
        var json = $$"""
            [
                {
                    "id": "{{id1}}",
                    "method": "{{method1}}",
                    "params": {},
                    "jsonrpc": "2.0"
                },
                {
                    "method": "{{method2}}",
                    "params": {},
                    "jsonrpc": "2.0"
                }
            ]
            """;

        var deserialized = JsonSerializer.Deserialize<IRequestWrapper>(json, headersJsonSerializerOptions);

        deserialized.Should().BeOfType<BatchRequestWrapper>();
        var calls = ((BatchRequestWrapper) deserialized).Calls
            .Select(c => c.Deserialize<IUntypedCall>(headersJsonSerializerOptions));
        var expectedCalls = new List<IUntypedCall>
        {
            new UntypedRequest(new StringRpcId(id1), method1, AnyJsonDocument),
            new UntypedNotification(method2, AnyJsonDocument)
        };
        calls.Should().BeEquivalentTo(expectedCalls, AssertionOptions);
    }

    #endregion

    #region BatchResponse

    [Test]
    public void BatchResponse_1Response()
    {
        var id = "123";
        var json = $$"""
            [
                {
                    "id": "{{id}}",
                    "result": {},
                    "jsonrpc": "2.0"
                }
            ]
            """;

        var deserialized = JsonSerializer.Deserialize<IResponseWrapper>(json, headersJsonSerializerOptions);

        var expectedResponses = new List<IResponse>
        {
            new UntypedResponse(new StringRpcId(id), AnyJsonDocument)
        };
        var expected = new BatchResponseWrapper(expectedResponses);
        deserialized.Should().BeEquivalentTo(expected, AssertionOptions);
    }

    [Test]
    public void BatchResponse_OnlyResponses()
    {
        var id1 = "123";
        var id2 = "546";
        var json = $$"""
            [
                {
                    "id": "{{id1}}",
                    "result": {},
                    "jsonrpc": "2.0"
                },
                {
                    "id": "{{id2}}",
                    "result": {},
                    "jsonrpc": "2.0"
                }
            ]
            """;

        var deserialized = JsonSerializer.Deserialize<IResponseWrapper>(json, headersJsonSerializerOptions);

        var expectedResponses = new List<IResponse>
        {
            new UntypedResponse(new StringRpcId(id1), AnyJsonDocument),
            new UntypedResponse(new StringRpcId(id2), AnyJsonDocument)
        };
        var expected = new BatchResponseWrapper(expectedResponses);
        deserialized.Should().BeEquivalentTo(expected, AssertionOptions);
    }

    [Test]
    public void BatchResponse_1Error()
    {
        var id = "123";
        var errorCode = 123;
        var errorMessage = "errorMessage";
        var json = $$"""
            [
                {
                    "id": "{{id}}",
                    "error": {
                        "code": {{errorCode}},
                        "message": "{{errorMessage}}"
                    },
                    "jsonrpc": "2.0"
                }
            ]
            """;

        var deserialized = JsonSerializer.Deserialize<IResponseWrapper>(json, headersJsonSerializerOptions);

        var expectedResponses = new List<IResponse>
        {
            new UntypedErrorResponse(new StringRpcId(id), new Error<JsonDocument>(errorCode, errorMessage, AnyJsonDocument))
        };
        var expected = new BatchResponseWrapper(expectedResponses);
        deserialized.Should().BeEquivalentTo(expected, AssertionOptions);
    }

    [Test]
    public void BatchResponse_OnlyErrors()
    {
        var id1 = "123";
        var id2 = "456";
        var errorCode = 123;
        var errorMessage = "errorMessage";
        var json = $$"""
            [
                {
                    "id": "{{id1}}",
                    "error": {
                        "code": {{errorCode}},
                        "message": "{{errorMessage}}"
                    },
                    "jsonrpc": "2.0"
                },
                {
                    "id": "{{id2}}",
                    "error": {
                        "code": {{errorCode}},
                        "message": "{{errorMessage}}"
                    },
                    "jsonrpc": "2.0"
                }
            ]
            """;

        var deserialized = JsonSerializer.Deserialize<IResponseWrapper>(json, headersJsonSerializerOptions);

        var expectedResponses = new List<IResponse>
        {
            new UntypedErrorResponse(new StringRpcId(id1), new Error<JsonDocument>(errorCode, errorMessage, AnyJsonDocument)),
            new UntypedErrorResponse(new StringRpcId(id2), new Error<JsonDocument>(errorCode, errorMessage, AnyJsonDocument))
        };
        var expected = new BatchResponseWrapper(expectedResponses);
        deserialized.Should().BeEquivalentTo(expected, AssertionOptions);
    }

    [Test]
    public void BatchResponse_ResponseAndError()
    {
        var id1 = "123";
        var id2 = "456";
        var errorCode = 123;
        var errorMessage = "errorMessage";
        var json = $$"""
            [
                {
                    "id": "{{id1}}",
                    "result": {},
                    "jsonrpc": "2.0"
                },
                {
                    "id": "{{id2}}",
                    "error": {
                        "code": {{errorCode}},
                        "message": "{{errorMessage}}"
                    },
                    "jsonrpc": "2.0"
                }
            ]
            """;

        var deserialized = JsonSerializer.Deserialize<IResponseWrapper>(json, headersJsonSerializerOptions);

        var expectedResponses = new List<IResponse>
        {
            new UntypedResponse(new StringRpcId(id1), AnyJsonDocument),
            new UntypedErrorResponse(new StringRpcId(id2), new Error<JsonDocument>(errorCode, errorMessage, AnyJsonDocument))
        };
        var expected = new BatchResponseWrapper(expectedResponses);
        deserialized.Should().BeEquivalentTo(expected, AssertionOptions);
    }

    #endregion

    #region Common

    [Test]
    public void JsonRpcSnakeCaseSerializerOptions_DeserializedWithHonorToSpecifiedPolicy()
    {
        var expectedEnum = TestEnum.ThreeFour;

        var data = TestData.Plain with { EnumField = expectedEnum };
        var json = JsonSerializer.Serialize(data, snakeCaseSerializerOptions);

        var deserialized = JsonSerializer.Deserialize<TestData>(json, snakeCaseSerializerOptions);
        deserialized.EnumField.Should().Be(expectedEnum);
    }

    [Test]
    public void JsonRpcCamelCaseSerializerOptions_DeserializedWithHonorToSpecifiedPolicy()
    {
        var expectedEnum = TestEnum.ThreeFour;

        var data = TestData.Plain with { EnumField = expectedEnum };
        var json = JsonSerializer.Serialize(data, camelCaseSerializerOptions);

        var deserialized = JsonSerializer.Deserialize<TestData>(json, camelCaseSerializerOptions);
        deserialized.EnumField.Should().Be(expectedEnum);
    }

    #endregion

    private static EquivalencyAssertionOptions<T> AssertionOptions<T>(EquivalencyAssertionOptions<T> options) => options
        .RespectingRuntimeTypes()
        .Excluding(static info => info.Type == typeof(JsonDocument));

    private static readonly JsonDocument AnyJsonDocument = JsonDocument.Parse("{}");
}
