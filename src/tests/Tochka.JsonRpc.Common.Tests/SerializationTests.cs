using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using FluentAssertions;
using NUnit.Framework;
using Tochka.JsonRpc.Common.Models.Id;
using Tochka.JsonRpc.Common.Models.Request.Untyped;
using Tochka.JsonRpc.TestUtils;

namespace Tochka.JsonRpc.Common.Tests;

[TestFixture]
internal class SerializationTests
{
    private readonly JsonSerializerOptions headersJsonSerializerOptions = JsonRpcSerializerOptions.Headers;
    private readonly JsonSerializerOptions snakeCaseSerializerOptions = JsonRpcSerializerOptions.SnakeCase;
    private readonly JsonSerializerOptions camelCaseSerializerOptions = JsonRpcSerializerOptions.CamelCase;

    #region Notification

    [Test]
    public void Notification_EmptyObjectParams()
    {
        var data = new { };
        var jsonData = JsonSerializer.SerializeToDocument(data, snakeCaseSerializerOptions);
        var notification = new UntypedNotification(Method, jsonData);

        var serialized = JsonSerializer.Serialize(notification, headersJsonSerializerOptions).TrimAllLines();

        var expected = $$"""
            {
                "method": "{{Method}}",
                "params": {},
                "jsonrpc": "2.0"
            }
            """.TrimAllLines();
        serialized.Should().Be(expected);
    }

    [Test]
    public void Notification_EmptyArrayParams()
    {
        var data = Array.Empty<TestData>();
        var jsonData = JsonSerializer.SerializeToDocument(data, snakeCaseSerializerOptions);
        var notification = new UntypedNotification(Method, jsonData);

        var serialized = JsonSerializer.Serialize(notification, headersJsonSerializerOptions).TrimAllLines();

        var expected = $$"""
            {
                "method": "{{Method}}",
                "params": [],
                "jsonrpc": "2.0"
            }
            """.TrimAllLines();
        serialized.Should().Be(expected);
    }

    [Test]
    public void Notification_EmptyListParams()
    {
        var data = new List<TestData>();
        var jsonData = JsonSerializer.SerializeToDocument(data, snakeCaseSerializerOptions);
        var notification = new UntypedNotification(Method, jsonData);

        var serialized = JsonSerializer.Serialize(notification, headersJsonSerializerOptions).TrimAllLines();

        var expected = $$"""
            {
                "method": "{{Method}}",
                "params": [],
                "jsonrpc": "2.0"
            }
            """.TrimAllLines();
        serialized.Should().Be(expected);
    }

    [Test]
    public void Notification_NullParams()
    {
        var notification = new UntypedNotification(Method, null);

        var serialized = JsonSerializer.Serialize(notification, headersJsonSerializerOptions).TrimAllLines();

        var expected = $$"""
            {
                "method": "{{Method}}",
                "params": null,
                "jsonrpc": "2.0"
            }
            """.TrimAllLines();
        serialized.Should().Be(expected);
    }

    [Test]
    public void Notification_PrimitiveTypeParams()
    {
        var data = 123;
        var jsonData = JsonSerializer.SerializeToDocument(data, snakeCaseSerializerOptions);
        var notification = new UntypedNotification(Method, jsonData);

        var serialized = JsonSerializer.Serialize(notification, headersJsonSerializerOptions).TrimAllLines();

        var expected = $$"""
            {
                "method": "{{Method}}",
                "params": 123,
                "jsonrpc": "2.0"
            }
            """.TrimAllLines();
        serialized.Should().Be(expected);
    }

    [Test]
    public void Notification_PrimitiveTypeArrayParams()
    {
        var data = new[] { 123 };
        var jsonData = JsonSerializer.SerializeToDocument(data, snakeCaseSerializerOptions);
        var notification = new UntypedNotification(Method, jsonData);

        var serialized = JsonSerializer.Serialize(notification, headersJsonSerializerOptions).TrimAllLines();

        var expected = $$"""
            {
                "method": "{{Method}}",
                "params": [
                    {{data[0]}}
                ],
                "jsonrpc": "2.0"
            }
            """.TrimAllLines();
        serialized.Should().Be(expected);
    }

    [Test]
    public void Notification_PrimitiveTypeListParams()
    {
        var data = new List<int> { 123 };
        var jsonData = JsonSerializer.SerializeToDocument(data, snakeCaseSerializerOptions);
        var notification = new UntypedNotification(Method, jsonData);

        var serialized = JsonSerializer.Serialize(notification, headersJsonSerializerOptions).TrimAllLines();

        var expected = $$"""
            {
                "method": "{{Method}}",
                "params": [
                    {{data[0]}}
                ],
                "jsonrpc": "2.0"
            }
            """.TrimAllLines();
        serialized.Should().Be(expected);
    }

    [Test]
    public void Notification_PlainSnakeCaseObjectParams()
    {
        var data = TestData.Plain;
        var jsonData = JsonSerializer.SerializeToDocument(data, snakeCaseSerializerOptions);
        var notification = new UntypedNotification(Method, jsonData);

        var serialized = JsonSerializer.Serialize(notification, headersJsonSerializerOptions).TrimAllLines();

        var expected = $$"""
            {
                "method": "{{Method}}",
                "params": {{TestData.PlainFullSnakeCaseJson}},
                "jsonrpc": "2.0"
            }
            """.TrimAllLines();
        serialized.Should().Be(expected);
    }

    [Test]
    public void Notification_PlainCamelCaseObjectParams()
    {
        var data = TestData.Plain;
        var jsonData = JsonSerializer.SerializeToDocument(data, camelCaseSerializerOptions);
        var notification = new UntypedNotification(Method, jsonData);

        var serialized = JsonSerializer.Serialize(notification, headersJsonSerializerOptions).TrimAllLines();

        var expected = $$"""
            {
                "method": "{{Method}}",
                "params": {{TestData.PlainFullCamelCaseJson}},
                "jsonrpc": "2.0"
            }
            """.TrimAllLines();
        serialized.Should().Be(expected);
    }

    [Test]
    public void Notification_PlainSnakeCaseArrayParams()
    {
        var data = new[] { TestData.Plain };
        var jsonData = JsonSerializer.SerializeToDocument(data, snakeCaseSerializerOptions);
        var notification = new UntypedNotification(Method, jsonData);

        var serialized = JsonSerializer.Serialize(notification, headersJsonSerializerOptions).TrimAllLines();

        var expected = $$"""
            {
                "method": "{{Method}}",
                "params": [
                    {{TestData.PlainFullSnakeCaseJson}}
                ],
                "jsonrpc": "2.0"
            }
            """.TrimAllLines();
        serialized.Should().Be(expected);
    }

    [Test]
    public void Notification_PlainCamelCaseArrayParams()
    {
        var data = new[] { TestData.Plain };
        var jsonData = JsonSerializer.SerializeToDocument(data, camelCaseSerializerOptions);
        var notification = new UntypedNotification(Method, jsonData);

        var serialized = JsonSerializer.Serialize(notification, headersJsonSerializerOptions).TrimAllLines();

        var expected = $$"""
            {
                "method": "{{Method}}",
                "params": [
                    {{TestData.PlainFullCamelCaseJson}}
                ],
                "jsonrpc": "2.0"
            }
            """.TrimAllLines();
        serialized.Should().Be(expected);
    }

    [Test]
    public void Notification_PlainSnakeCaseListParams()
    {
        var data = new List<TestData> { TestData.Plain };
        var jsonData = JsonSerializer.SerializeToDocument(data, snakeCaseSerializerOptions);
        var notification = new UntypedNotification(Method, jsonData);

        var serialized = JsonSerializer.Serialize(notification, headersJsonSerializerOptions).TrimAllLines();

        var expected = $$"""
            {
                "method": "{{Method}}",
                "params": [
                    {{TestData.PlainFullSnakeCaseJson}}
                ],
                "jsonrpc": "2.0"
            }
            """.TrimAllLines();
        serialized.Should().Be(expected);
    }

    [Test]
    public void Notification_PlainCamelCaseListParams()
    {
        var data = new List<TestData> { TestData.Plain };
        var jsonData = JsonSerializer.SerializeToDocument(data, camelCaseSerializerOptions);
        var notification = new UntypedNotification(Method, jsonData);

        var serialized = JsonSerializer.Serialize(notification, headersJsonSerializerOptions).TrimAllLines();

        var expected = $$"""
            {
                "method": "{{Method}}",
                "params": [
                    {{TestData.PlainFullCamelCaseJson}}
                ],
                "jsonrpc": "2.0"
            }
            """.TrimAllLines();
        serialized.Should().Be(expected);
    }

    [Test]
    public void Notification_NestedSnakeCaseObjectParams()
    {
        var data = TestData.Nested;
        var jsonData = JsonSerializer.SerializeToDocument(data, snakeCaseSerializerOptions);
        var notification = new UntypedNotification(Method, jsonData);

        var serialized = JsonSerializer.Serialize(notification, headersJsonSerializerOptions).TrimAllLines();

        var expected = $$"""
            {
                "method": "{{Method}}",
                "params": {{TestData.NestedFullSnakeCaseJson}},
                "jsonrpc": "2.0"
            }
            """.TrimAllLines();
        serialized.Should().Be(expected);
    }

    [Test]
    public void Notification_NestedCamelCaseObjectParams()
    {
        var data = TestData.Nested;
        var jsonData = JsonSerializer.SerializeToDocument(data, camelCaseSerializerOptions);
        var notification = new UntypedNotification(Method, jsonData);

        var serialized = JsonSerializer.Serialize(notification, headersJsonSerializerOptions).TrimAllLines();

        var expected = $$"""
            {
                "method": "{{Method}}",
                "params": {{TestData.NestedFullCamelCaseJson}},
                "jsonrpc": "2.0"
            }
            """.TrimAllLines();
        serialized.Should().Be(expected);
    }

    [Test]
    public void Notification_NestedSnakeCaseArrayParams()
    {
        var data = new[] { TestData.Nested };
        var jsonData = JsonSerializer.SerializeToDocument(data, snakeCaseSerializerOptions);
        var notification = new UntypedNotification(Method, jsonData);

        var serialized = JsonSerializer.Serialize(notification, headersJsonSerializerOptions).TrimAllLines();

        var expected = $$"""
            {
                "method": "{{Method}}",
                "params": [
                    {{TestData.NestedFullSnakeCaseJson}}
                ],
                "jsonrpc": "2.0"
            }
            """.TrimAllLines();
        serialized.Should().Be(expected);
    }

    [Test]
    public void Notification_NestedCamelCaseArrayParams()
    {
        var data = new[] { TestData.Nested };
        var jsonData = JsonSerializer.SerializeToDocument(data, camelCaseSerializerOptions);
        var notification = new UntypedNotification(Method, jsonData);

        var serialized = JsonSerializer.Serialize(notification, headersJsonSerializerOptions).TrimAllLines();

        var expected = $$"""
            {
                "method": "{{Method}}",
                "params": [
                    {{TestData.NestedFullCamelCaseJson}}
                ],
                "jsonrpc": "2.0"
            }
            """.TrimAllLines();
        serialized.Should().Be(expected);
    }

    [Test]
    public void Notification_NestedSnakeCaseListParams()
    {
        var data = new List<TestData> { TestData.Nested };
        var jsonData = JsonSerializer.SerializeToDocument(data, snakeCaseSerializerOptions);
        var notification = new UntypedNotification(Method, jsonData);

        var serialized = JsonSerializer.Serialize(notification, headersJsonSerializerOptions).TrimAllLines();

        var expected = $$"""
            {
                "method": "{{Method}}",
                "params": [
                    {{TestData.NestedFullSnakeCaseJson}}
                ],
                "jsonrpc": "2.0"
            }
            """.TrimAllLines();
        serialized.Should().Be(expected);
    }

    [Test]
    public void Notification_NestedCamelCaseListParams()
    {
        var data = new List<TestData> { TestData.Nested };
        var jsonData = JsonSerializer.SerializeToDocument(data, camelCaseSerializerOptions);
        var notification = new UntypedNotification(Method, jsonData);

        var serialized = JsonSerializer.Serialize(notification, headersJsonSerializerOptions).TrimAllLines();

        var expected = $$"""
            {
                "method": "{{Method}}",
                "params": [
                    {{TestData.NestedFullCamelCaseJson}}
                ],
                "jsonrpc": "2.0"
            }
            """.TrimAllLines();
        serialized.Should().Be(expected);
    }

    #endregion

    #region Request

    [Test]
    public void Request_EmptyObjectParams()
    {
        var id = "123";
        var data = new { };
        var jsonData = JsonSerializer.SerializeToDocument(data, snakeCaseSerializerOptions);
        var notification = new UntypedRequest(new StringRpcId(id), Method, jsonData);

        var serialized = JsonSerializer.Serialize(notification, headersJsonSerializerOptions).TrimAllLines();

        var expected = $$"""
            {
                "id": "{{id}}",
                "method": "{{Method}}",
                "params": {},
                "jsonrpc": "2.0"
            }
            """.TrimAllLines();
        serialized.Should().Be(expected);
    }

    [Test]
    public void Request_EmptyArrayParams()
    {
        var id = "123";
        var data = Array.Empty<TestData>();
        var jsonData = JsonSerializer.SerializeToDocument(data, snakeCaseSerializerOptions);
        var notification = new UntypedRequest(new StringRpcId(id), Method, jsonData);

        var serialized = JsonSerializer.Serialize(notification, headersJsonSerializerOptions).TrimAllLines();

        var expected = $$"""
            {
                "id": "{{id}}",
                "method": "{{Method}}",
                "params": [],
                "jsonrpc": "2.0"
            }
            """.TrimAllLines();
        serialized.Should().Be(expected);
    }

    [Test]
    public void Request_EmptyListParams()
    {
        var id = "123";
        var data = new List<TestData>();
        var jsonData = JsonSerializer.SerializeToDocument(data, snakeCaseSerializerOptions);
        var notification = new UntypedRequest(new StringRpcId(id), Method, jsonData);

        var serialized = JsonSerializer.Serialize(notification, headersJsonSerializerOptions).TrimAllLines();

        var expected = $$"""
            {
                "id": "{{id}}",
                "method": "{{Method}}",
                "params": [],
                "jsonrpc": "2.0"
            }
            """.TrimAllLines();
        serialized.Should().Be(expected);
    }

    [Test]
    public void Request_NullParams()
    {
        var id = "123";
        var notification = new UntypedRequest(new StringRpcId(id), Method, null);

        var serialized = JsonSerializer.Serialize(notification, headersJsonSerializerOptions).TrimAllLines();

        var expected = $$"""
            {
                "id": "{{id}}",
                "method": "{{Method}}",
                "params": null,
                "jsonrpc": "2.0"
            }
            """.TrimAllLines();
        serialized.Should().Be(expected);
    }

    [Test]
    public void Request_IntId()
    {
        var id = 123;
        var notification = new UntypedRequest(new NumberRpcId(id), Method, null);

        var serialized = JsonSerializer.Serialize(notification, headersJsonSerializerOptions).TrimAllLines();

        var expected = $$"""
            {
                "id": {{id}},
                "method": "{{Method}}",
                "params": null,
                "jsonrpc": "2.0"
            }
            """.TrimAllLines();
        serialized.Should().Be(expected);
    }

    [Test]
    public void Request_StringId()
    {
        var id = "123";
        var notification = new UntypedRequest(new StringRpcId(id), Method, null);

        var serialized = JsonSerializer.Serialize(notification, headersJsonSerializerOptions).TrimAllLines();

        var expected = $$"""
            {
                "id": "{{id}}",
                "method": "{{Method}}",
                "params": null,
                "jsonrpc": "2.0"
            }
            """.TrimAllLines();
        serialized.Should().Be(expected);
    }

    [Test]
    public void Request_NullId()
    {
        var notification = new UntypedRequest(new NullRpcId(), Method, null);

        var serialized = JsonSerializer.Serialize(notification, headersJsonSerializerOptions).TrimAllLines();

        var expected = $$"""
            {
                "id": null,
                "method": "{{Method}}",
                "params": null,
                "jsonrpc": "2.0"
            }
            """.TrimAllLines();
        serialized.Should().Be(expected);
    }

    [Test]
    public void Request_PrimitiveTypeParams()
    {
        var id = "123";
        var data = 123;
        var jsonData = JsonSerializer.SerializeToDocument(data, snakeCaseSerializerOptions);
        var notification = new UntypedRequest(new StringRpcId(id), Method, jsonData);

        var serialized = JsonSerializer.Serialize(notification, headersJsonSerializerOptions).TrimAllLines();

        var expected = $$"""
            {
                "id": "{{id}}",
                "method": "{{Method}}",
                "params": {{data}},
                "jsonrpc": "2.0"
            }
            """.TrimAllLines();
        serialized.Should().Be(expected);
    }

    [Test]
    public void Request_PrimitiveTypeArrayParams()
    {
        var id = "123";
        var data = new[] { 123 };
        var jsonData = JsonSerializer.SerializeToDocument(data, snakeCaseSerializerOptions);
        var notification = new UntypedRequest(new StringRpcId(id), Method, jsonData);

        var serialized = JsonSerializer.Serialize(notification, headersJsonSerializerOptions).TrimAllLines();

        var expected = $$"""
            {
                "id": "{{id}}",
                "method": "{{Method}}",
                "params": [
                    {{data[0]}}
                ],
                "jsonrpc": "2.0"
            }
            """.TrimAllLines();
        serialized.Should().Be(expected);
    }

    [Test]
    public void Request_PrimitiveTypeListParams()
    {
        var id = "123";
        var data = new List<int> { 123 };
        var jsonData = JsonSerializer.SerializeToDocument(data, snakeCaseSerializerOptions);
        var notification = new UntypedRequest(new StringRpcId(id), Method, jsonData);

        var serialized = JsonSerializer.Serialize(notification, headersJsonSerializerOptions).TrimAllLines();

        var expected = $$"""
            {
                "id": "{{id}}",
                "method": "{{Method}}",
                "params": [
                    {{data[0]}}
                ],
                "jsonrpc": "2.0"
            }
            """.TrimAllLines();
        serialized.Should().Be(expected);
    }

    [Test]
    public void Request_PlainSnakeCaseObjectParams()
    {
        var id = "123";
        var data = TestData.Plain;
        var jsonData = JsonSerializer.SerializeToDocument(data, snakeCaseSerializerOptions);
        var notification = new UntypedRequest(new StringRpcId(id), Method, jsonData);

        var serialized = JsonSerializer.Serialize(notification, headersJsonSerializerOptions).TrimAllLines();

        var expected = $$"""
            {
                "id": "{{id}}",
                "method": "{{Method}}",
                "params": {{TestData.PlainFullSnakeCaseJson}},
                "jsonrpc": "2.0"
            }
            """.TrimAllLines();
        serialized.Should().Be(expected);
    }

    [Test]
    public void Request_PlainCamelCaseObjectParams()
    {
        var id = "123";
        var data = TestData.Plain;
        var jsonData = JsonSerializer.SerializeToDocument(data, camelCaseSerializerOptions);
        var notification = new UntypedRequest(new StringRpcId(id), Method, jsonData);

        var serialized = JsonSerializer.Serialize(notification, headersJsonSerializerOptions).TrimAllLines();

        var expected = $$"""
            {
                "id": "{{id}}",
                "method": "{{Method}}",
                "params": {{TestData.PlainFullCamelCaseJson}},
                "jsonrpc": "2.0"
            }
            """.TrimAllLines();
        serialized.Should().Be(expected);
    }

    [Test]
    public void Request_PlainSnakeCaseArrayParams()
    {
        var id = "123";
        var data = new[] { TestData.Plain };
        var jsonData = JsonSerializer.SerializeToDocument(data, snakeCaseSerializerOptions);
        var notification = new UntypedRequest(new StringRpcId(id), Method, jsonData);

        var serialized = JsonSerializer.Serialize(notification, headersJsonSerializerOptions).TrimAllLines();

        var expected = $$"""
            {
                "id": "{{id}}",
                "method": "{{Method}}",
                "params": [
                    {{TestData.PlainFullSnakeCaseJson}}
                ],
                "jsonrpc": "2.0"
            }
            """.TrimAllLines();
        serialized.Should().Be(expected);
    }

    [Test]
    public void Request_PlainCamelCaseArrayParams()
    {
        var id = "123";
        var data = new[] { TestData.Plain };
        var jsonData = JsonSerializer.SerializeToDocument(data, camelCaseSerializerOptions);
        var notification = new UntypedRequest(new StringRpcId(id), Method, jsonData);

        var serialized = JsonSerializer.Serialize(notification, headersJsonSerializerOptions).TrimAllLines();

        var expected = $$"""
            {
                "id": "{{id}}",
                "method": "{{Method}}",
                "params": [
                    {{TestData.PlainFullCamelCaseJson}}
                ],
                "jsonrpc": "2.0"
            }
            """.TrimAllLines();
        serialized.Should().Be(expected);
    }

    [Test]
    public void Request_PlainSnakeCaseListParams()
    {
        var id = "123";
        var data = new List<TestData> { TestData.Plain };
        var jsonData = JsonSerializer.SerializeToDocument(data, snakeCaseSerializerOptions);
        var notification = new UntypedRequest(new StringRpcId(id), Method, jsonData);

        var serialized = JsonSerializer.Serialize(notification, headersJsonSerializerOptions).TrimAllLines();

        var expected = $$"""
            {
                "id": "{{id}}",
                "method": "{{Method}}",
                "params": [
                    {{TestData.PlainFullSnakeCaseJson}}
                ],
                "jsonrpc": "2.0"
            }
            """.TrimAllLines();
        serialized.Should().Be(expected);
    }

    [Test]
    public void Request_PlainCamelCaseListParams()
    {
        var id = "123";
        var data = new List<TestData> { TestData.Plain };
        var jsonData = JsonSerializer.SerializeToDocument(data, camelCaseSerializerOptions);
        var notification = new UntypedRequest(new StringRpcId(id), Method, jsonData);

        var serialized = JsonSerializer.Serialize(notification, headersJsonSerializerOptions).TrimAllLines();

        var expected = $$"""
            {
                "id": "{{id}}",
                "method": "{{Method}}",
                "params": [
                    {{TestData.PlainFullCamelCaseJson}}
                ],
                "jsonrpc": "2.0"
            }
            """.TrimAllLines();
        serialized.Should().Be(expected);
    }

    [Test]
    public void Request_NestedSnakeCaseObjectParams()
    {
        var id = "123";
        var data = TestData.Nested;
        var jsonData = JsonSerializer.SerializeToDocument(data, snakeCaseSerializerOptions);
        var notification = new UntypedRequest(new StringRpcId(id), Method, jsonData);

        var serialized = JsonSerializer.Serialize(notification, headersJsonSerializerOptions).TrimAllLines();

        var expected = $$"""
            {
                "id": "{{id}}",
                "method": "{{Method}}",
                "params": {{TestData.NestedFullSnakeCaseJson}},
                "jsonrpc": "2.0"
            }
            """.TrimAllLines();
        serialized.Should().Be(expected);
    }

    [Test]
    public void Request_NestedCamelCaseObjectParams()
    {
        var id = "123";
        var data = TestData.Nested;
        var jsonData = JsonSerializer.SerializeToDocument(data, camelCaseSerializerOptions);
        var notification = new UntypedRequest(new StringRpcId(id), Method, jsonData);

        var serialized = JsonSerializer.Serialize(notification, headersJsonSerializerOptions).TrimAllLines();

        var expected = $$"""
            {
                "id": "{{id}}",
                "method": "{{Method}}",
                "params": {{TestData.NestedFullCamelCaseJson}},
                "jsonrpc": "2.0"
            }
            """.TrimAllLines();
        serialized.Should().Be(expected);
    }

    [Test]
    public void Request_NestedSnakeCaseArrayParams()
    {
        var id = "123";
        var data = new[] { TestData.Nested };
        var jsonData = JsonSerializer.SerializeToDocument(data, snakeCaseSerializerOptions);
        var notification = new UntypedRequest(new StringRpcId(id), Method, jsonData);

        var serialized = JsonSerializer.Serialize(notification, headersJsonSerializerOptions).TrimAllLines();

        var expected = $$"""
            {
                "id": "{{id}}",
                "method": "{{Method}}",
                "params": [
                    {{TestData.NestedFullSnakeCaseJson}}
                ],
                "jsonrpc": "2.0"
            }
            """.TrimAllLines();
        serialized.Should().Be(expected);
    }

    [Test]
    public void Request_NestedCamelCaseArrayParams()
    {
        var id = "123";
        var data = new[] { TestData.Nested };
        var jsonData = JsonSerializer.SerializeToDocument(data, camelCaseSerializerOptions);
        var notification = new UntypedRequest(new StringRpcId(id), Method, jsonData);

        var serialized = JsonSerializer.Serialize(notification, headersJsonSerializerOptions).TrimAllLines();

        var expected = $$"""
            {
                "id": "{{id}}",
                "method": "{{Method}}",
                "params": [
                    {{TestData.NestedFullCamelCaseJson}}
                ],
                "jsonrpc": "2.0"
            }
            """.TrimAllLines();
        serialized.Should().Be(expected);
    }

    [Test]
    public void Request_NestedSnakeCaseListParams()
    {
        var id = "123";
        var data = new List<TestData> { TestData.Nested };
        var jsonData = JsonSerializer.SerializeToDocument(data, snakeCaseSerializerOptions);
        var notification = new UntypedRequest(new StringRpcId(id), Method, jsonData);

        var serialized = JsonSerializer.Serialize(notification, headersJsonSerializerOptions).TrimAllLines();

        var expected = $$"""
            {
                "id": "{{id}}",
                "method": "{{Method}}",
                "params": [
                    {{TestData.NestedFullSnakeCaseJson}}
                ],
                "jsonrpc": "2.0"
            }
            """.TrimAllLines();
        serialized.Should().Be(expected);
    }

    [Test]
    public void Request_NestedCamelCaseListParams()
    {
        var id = "123";
        var data = new List<TestData> { TestData.Nested };
        var jsonData = JsonSerializer.SerializeToDocument(data, camelCaseSerializerOptions);
        var notification = new UntypedRequest(new StringRpcId(id), Method, jsonData);

        var serialized = JsonSerializer.Serialize(notification, headersJsonSerializerOptions).TrimAllLines();

        var expected = $$"""
            {
                "id": "{{id}}",
                "method": "{{Method}}",
                "params": [
                    {{TestData.NestedFullCamelCaseJson}}
                ],
                "jsonrpc": "2.0"
            }
            """.TrimAllLines();
        serialized.Should().Be(expected);
    }

    #endregion

    #region RequestResponse

    [Test]
    public void RequestResponse_EmptyObjectResult()
    {
    }

    [Test]
    public void RequestResponse_EmptyArrayResult()
    {
    }

    [Test]
    public void RequestResponse_NullResult()
    {
    }

    [Test]
    public void RequestResponse_IntId()
    {
    }

    [Test]
    public void RequestResponse_StringId()
    {
    }

    [Test]
    public void RequestResponse_NullId()
    {
    }

    [Test]
    public void RequestResponse_PrimitiveTypeResult()
    {
    }

    [Test]
    public void RequestResponse_PrimitiveTypeArrayResult()
    {
    }

    [Test]
    public void RequestResponse_ResultWithErrorField()
    {
    }

    [Test]
    public void RequestResponse_PlainSnakeCaseObjectResult()
    {
    }

    [Test]
    public void RequestResponse_PlainCamelCaseObjectResult()
    {
    }

    [Test]
    public void RequestResponse_PlainSnakeCaseArrayResult()
    {
    }

    [Test]
    public void RequestResponse_PlainCamelCaseArrayResult()
    {
    }

    [Test]
    public void RequestResponse_NestedSnakeCaseObjectResult()
    {
    }

    [Test]
    public void RequestResponse_NestedCamelCaseObjectResult()
    {
    }

    [Test]
    public void RequestResponse_NestedSnakeCaseArrayResult()
    {
    }

    [Test]
    public void RequestResponse_NestedCamelCaseArrayResult()
    {
    }

    [Test]
    public void RequestResponse_EmptyObjectErrorData()
    {
    }

    [Test]
    public void RequestResponse_EmptyArrayErrorData()
    {
    }

    [Test]
    public void RequestResponse_NullErrorData()
    {
    }

    [Test]
    public void RequestResponse_NoErrorData()
    {
    }

    [Test]
    public void RequestResponse_PrimitiveTypeErrorData()
    {
    }

    [Test]
    public void RequestResponse_PrimitiveTypeArrayErrorData()
    {
    }

    [Test]
    public void RequestResponse_ErrorDataWithResultField()
    {
    }

    [Test]
    public void RequestResponse_PlainSnakeCaseObjectErrorData()
    {
    }

    [Test]
    public void RequestResponse_PlainCamelCaseObjectErrorData()
    {
    }

    [Test]
    public void RequestResponse_PlainSnakeCaseArrayErrorData()
    {
    }

    [Test]
    public void RequestResponse_PlainCamelCaseArrayErrorData()
    {
    }

    [Test]
    public void RequestResponse_NestedSnakeCaseObjectErrorData()
    {
    }

    [Test]
    public void RequestResponse_NestedCamelCaseObjectErrorData()
    {
    }

    [Test]
    public void RequestResponse_NestedSnakeCaseArrayErrorData()
    {
    }

    [Test]
    public void RequestResponse_NestedCamelCaseArrayErrorData()
    {
    }

    #endregion

    #region Batch

    [Test]
    public void Batch_IEnumerable()
    {
        var data = new { };
        var jsonData = JsonSerializer.SerializeToDocument(data, snakeCaseSerializerOptions);
        var batch = new List<IUntypedCall>
        {
            new UntypedNotification(Method, jsonData)
        }.AsEnumerable();

        var serialized = JsonSerializer.Serialize(batch, headersJsonSerializerOptions).TrimAllLines();

        var expected = $$"""
            [
                {
                    "method": "{{Method}}",
                    "params": {},
                    "jsonrpc": "2.0"
                }
            ]
            """.TrimAllLines();
        serialized.Should().Be(expected);
    }

    [Test]
    public void Batch_List()
    {
        var data = new { };
        var jsonData = JsonSerializer.SerializeToDocument(data, snakeCaseSerializerOptions);
        var batch = new List<IUntypedCall>
        {
            new UntypedNotification(Method, jsonData)
        };

        var serialized = JsonSerializer.Serialize(batch, headersJsonSerializerOptions).TrimAllLines();

        var expected = $$"""
            [
                {
                    "method": "{{Method}}",
                    "params": {},
                    "jsonrpc": "2.0"
                }
            ]
            """.TrimAllLines();
        serialized.Should().Be(expected);
    }

    [Test]
    public void Batch_Array()
    {
        var data = new { };
        var jsonData = JsonSerializer.SerializeToDocument(data, snakeCaseSerializerOptions);
        var batch = new IUntypedCall[]
        {
            new UntypedNotification(Method, jsonData)
        };

        var serialized = JsonSerializer.Serialize(batch, headersJsonSerializerOptions).TrimAllLines();

        var expected = $$"""
            [
                {
                    "method": "{{Method}}",
                    "params": {},
                    "jsonrpc": "2.0"
                }
            ]
            """.TrimAllLines();
        serialized.Should().Be(expected);
    }

    [Test]
    public void Batch_1Notification()
    {
        var data = new { };
        var jsonData = JsonSerializer.SerializeToDocument(data, snakeCaseSerializerOptions);
        var batch = new List<IUntypedCall>
        {
            new UntypedNotification(Method, jsonData)
        };

        var serialized = JsonSerializer.Serialize(batch, headersJsonSerializerOptions).TrimAllLines();

        var expected = $$"""
            [
                {
                    "method": "{{Method}}",
                    "params": {},
                    "jsonrpc": "2.0"
                }
            ]
            """.TrimAllLines();
        serialized.Should().Be(expected);
    }

    [Test]
    public void Batch_OnlyNotifications()
    {
        var data = new { };
        var jsonData = JsonSerializer.SerializeToDocument(data, snakeCaseSerializerOptions);
        var batch = new List<IUntypedCall>
        {
            new UntypedNotification(Method, jsonData),
            new UntypedNotification(Method, jsonData)
        };

        var serialized = JsonSerializer.Serialize(batch, headersJsonSerializerOptions).TrimAllLines();

        var expected = $$"""
            [
                {
                    "method": "{{Method}}",
                    "params": {},
                    "jsonrpc": "2.0"
                },
                {
                    "method": "{{Method}}",
                    "params": {},
                    "jsonrpc": "2.0"
                }
            ]
            """.TrimAllLines();
        serialized.Should().Be(expected);
    }

    [Test]
    public void Batch_1Request()
    {
        var id = "123";
        var data = new { };
        var jsonData = JsonSerializer.SerializeToDocument(data, snakeCaseSerializerOptions);
        var batch = new List<IUntypedCall>
        {
            new UntypedRequest(new StringRpcId(id), Method, jsonData)
        };

        var serialized = JsonSerializer.Serialize(batch, headersJsonSerializerOptions).TrimAllLines();

        var expected = $$"""
            [
                {
                    "id": "{{id}}",
                    "method": "{{Method}}",
                    "params": {},
                    "jsonrpc": "2.0"
                }
            ]
            """.TrimAllLines();
        serialized.Should().Be(expected);
    }

    [Test]
    public void Batch_OnlyRequests()
    {
        var id1 = "123";
        var id2 = "456";
        var data = new { };
        var jsonData = JsonSerializer.SerializeToDocument(data, snakeCaseSerializerOptions);
        var batch = new List<IUntypedCall>
        {
            new UntypedRequest(new StringRpcId(id1), Method, jsonData),
            new UntypedRequest(new StringRpcId(id2), Method, jsonData)
        };

        var serialized = JsonSerializer.Serialize(batch, headersJsonSerializerOptions).TrimAllLines();

        var expected = $$"""
            [
                {
                    "id": "{{id1}}",
                    "method": "{{Method}}",
                    "params": {},
                    "jsonrpc": "2.0"
                },
                {
                    "id": "{{id2}}",
                    "method": "{{Method}}",
                    "params": {},
                    "jsonrpc": "2.0"
                }
            ]
            """.TrimAllLines();
        serialized.Should().Be(expected);
    }

    [Test]
    public void Batch_NotificationAndRequest()
    {
        var id = "123";
        var data = new { };
        var jsonData = JsonSerializer.SerializeToDocument(data, snakeCaseSerializerOptions);
        var batch = new List<IUntypedCall>
        {
            new UntypedNotification(Method, jsonData),
            new UntypedRequest(new StringRpcId(id), Method, jsonData)
        };

        var serialized = JsonSerializer.Serialize(batch, headersJsonSerializerOptions).TrimAllLines();

        var expected = $$"""
            [
                {
                    "method": "{{Method}}",
                    "params": {},
                    "jsonrpc": "2.0"
                },
                {
                    "id": "{{id}}",
                    "method": "{{Method}}",
                    "params": {},
                    "jsonrpc": "2.0"
                }
            ]
            """.TrimAllLines();
        serialized.Should().Be(expected);
    }

    #endregion

    #region BatchResponse

    [Test]
    public void BatchResponse_1Response()
    {
    }

    [Test]
    public void BatchResponse_OnlyResponses()
    {
    }

    [Test]
    public void BatchResponse_1Error()
    {
    }

    [Test]
    public void BatchResponse_OnlyErrors()
    {
    }

    [Test]
    public void BatchResponse_ResponseAndError()
    {
    }

    #endregion

    private const string Method = "method";
}
