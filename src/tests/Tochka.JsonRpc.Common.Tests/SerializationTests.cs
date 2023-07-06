using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using FluentAssertions;
using NUnit.Framework;
using Tochka.JsonRpc.Common.Models.Id;
using Tochka.JsonRpc.Common.Models.Request.Untyped;
using Tochka.JsonRpc.Common.Models.Response;
using Tochka.JsonRpc.Common.Models.Response.Errors;
using Tochka.JsonRpc.Common.Models.Response.Untyped;
using Tochka.JsonRpc.Common.Models.Response.Wrappers;
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
        var data = new { };
        var jsonData = JsonSerializer.SerializeToDocument(data, snakeCaseSerializerOptions);
        IResponseWrapper response = new SingleResponseWrapper(new UntypedResponse(new StringRpcId(Id), jsonData));

        var serialized = JsonSerializer.Serialize(response, headersJsonSerializerOptions).TrimAllLines();

        var expected = $$"""
            {
                "id": "{{Id}}",
                "result": {},
                "jsonrpc": "2.0"
            }
            """.TrimAllLines();
        serialized.Should().Be(expected);
    }

    [Test]
    public void RequestResponse_EmptyArrayResult()
    {
        var data = Array.Empty<TestData>();
        var jsonData = JsonSerializer.SerializeToDocument(data, snakeCaseSerializerOptions);
        IResponseWrapper response = new SingleResponseWrapper(new UntypedResponse(new StringRpcId(Id), jsonData));

        var serialized = JsonSerializer.Serialize(response, headersJsonSerializerOptions).TrimAllLines();

        var expected = $$"""
            {
                "id": "{{Id}}",
                "result": [],
                "jsonrpc": "2.0"
            }
            """.TrimAllLines();
        serialized.Should().Be(expected);
    }

    [Test]
    public void RequestResponse_NullResult()
    {
        IResponseWrapper response = new SingleResponseWrapper(new UntypedResponse(new StringRpcId(Id), null));

        var serialized = JsonSerializer.Serialize(response, headersJsonSerializerOptions).TrimAllLines();

        var expected = $$"""
            {
                "id": "{{Id}}",
                "result": null,
                "jsonrpc": "2.0"
            }
            """.TrimAllLines();
        serialized.Should().Be(expected);
    }

    [Test]
    public void RequestResponse_IntId()
    {
        var id = 123;
        var data = new { };
        var jsonData = JsonSerializer.SerializeToDocument(data, snakeCaseSerializerOptions);
        IResponseWrapper response = new SingleResponseWrapper(new UntypedResponse(new NumberRpcId(id), jsonData));

        var serialized = JsonSerializer.Serialize(response, headersJsonSerializerOptions).TrimAllLines();

        var expected = $$"""
            {
                "id": {{id}},
                "result": {},
                "jsonrpc": "2.0"
            }
            """.TrimAllLines();
        serialized.Should().Be(expected);
    }

    [Test]
    public void RequestResponse_StringId()
    {
        var id = "123";
        var data = new { };
        var jsonData = JsonSerializer.SerializeToDocument(data, snakeCaseSerializerOptions);
        IResponseWrapper response = new SingleResponseWrapper(new UntypedResponse(new StringRpcId(id), jsonData));

        var serialized = JsonSerializer.Serialize(response, headersJsonSerializerOptions).TrimAllLines();

        var expected = $$"""
            {
                "id": "{{id}}",
                "result": {},
                "jsonrpc": "2.0"
            }
            """.TrimAllLines();
        serialized.Should().Be(expected);
    }

    [Test]
    public void RequestResponse_NullId()
    {
        var data = new { };
        var jsonData = JsonSerializer.SerializeToDocument(data, snakeCaseSerializerOptions);
        IResponseWrapper response = new SingleResponseWrapper(new UntypedResponse(new NullRpcId(), jsonData));

        var serialized = JsonSerializer.Serialize(response, headersJsonSerializerOptions).TrimAllLines();

        var expected = """
            {
                "id": null,
                "result": {},
                "jsonrpc": "2.0"
            }
            """.TrimAllLines();
        serialized.Should().Be(expected);
    }

    [Test]
    public void RequestResponse_PrimitiveTypeResult()
    {
        var data = 123;
        var jsonData = JsonSerializer.SerializeToDocument(data, snakeCaseSerializerOptions);
        IResponseWrapper response = new SingleResponseWrapper(new UntypedResponse(new StringRpcId(Id), jsonData));

        var serialized = JsonSerializer.Serialize(response, headersJsonSerializerOptions).TrimAllLines();

        var expected = $$"""
            {
                "id": "{{Id}}",
                "result": {{data}},
                "jsonrpc": "2.0"
            }
            """.TrimAllLines();
        serialized.Should().Be(expected);
    }

    [Test]
    public void RequestResponse_PrimitiveTypeArrayResult()
    {
        var data = new[] { 123 };
        var jsonData = JsonSerializer.SerializeToDocument(data, snakeCaseSerializerOptions);
        IResponseWrapper response = new SingleResponseWrapper(new UntypedResponse(new StringRpcId(Id), jsonData));

        var serialized = JsonSerializer.Serialize(response, headersJsonSerializerOptions).TrimAllLines();

        var expected = $$"""
            {
                "id": "{{Id}}",
                "result": [
                    {{data[0]}}
                ],
                "jsonrpc": "2.0"
            }
            """.TrimAllLines();
        serialized.Should().Be(expected);
    }

    [Test]
    public void RequestResponse_ResultWithErrorField()
    {
        var error = "errorValue";
        var data = new ResultWithError(error);
        var jsonData = JsonSerializer.SerializeToDocument(data, snakeCaseSerializerOptions);
        IResponseWrapper response = new SingleResponseWrapper(new UntypedResponse(new StringRpcId(Id), jsonData));

        var serialized = JsonSerializer.Serialize(response, headersJsonSerializerOptions).TrimAllLines();

        var expected = $$"""
            {
                "id": "{{Id}}",
                "result": {
                    "error": "{{error}}"
                },
                "jsonrpc": "2.0"
            }
            """.TrimAllLines();
        serialized.Should().Be(expected);
    }

    [Test]
    public void RequestResponse_PlainSnakeCaseObjectResult()
    {
        var data = TestData.Plain;
        var jsonData = JsonSerializer.SerializeToDocument(data, snakeCaseSerializerOptions);
        IResponseWrapper response = new SingleResponseWrapper(new UntypedResponse(new StringRpcId(Id), jsonData));

        var serialized = JsonSerializer.Serialize(response, headersJsonSerializerOptions).TrimAllLines();

        var expected = $$"""
            {
                "id": "{{Id}}",
                "result": {{TestData.PlainFullSnakeCaseJson}},
                "jsonrpc": "2.0"
            }
            """.TrimAllLines();
        serialized.Should().Be(expected);
    }

    [Test]
    public void RequestResponse_PlainCamelCaseObjectResult()
    {
        var data = TestData.Plain;
        var jsonData = JsonSerializer.SerializeToDocument(data, camelCaseSerializerOptions);
        IResponseWrapper response = new SingleResponseWrapper(new UntypedResponse(new StringRpcId(Id), jsonData));

        var serialized = JsonSerializer.Serialize(response, headersJsonSerializerOptions).TrimAllLines();

        var expected = $$"""
            {
                "id": "{{Id}}",
                "result": {{TestData.PlainFullCamelCaseJson}},
                "jsonrpc": "2.0"
            }
            """.TrimAllLines();
        serialized.Should().Be(expected);
    }

    [Test]
    public void RequestResponse_PlainSnakeCaseArrayResult()
    {
        var data = new[] { TestData.Plain };
        var jsonData = JsonSerializer.SerializeToDocument(data, snakeCaseSerializerOptions);
        IResponseWrapper response = new SingleResponseWrapper(new UntypedResponse(new StringRpcId(Id), jsonData));

        var serialized = JsonSerializer.Serialize(response, headersJsonSerializerOptions).TrimAllLines();

        var expected = $$"""
            {
                "id": "{{Id}}",
                "result": [
                    {{TestData.PlainFullSnakeCaseJson}}
                ],
                "jsonrpc": "2.0"
            }
            """.TrimAllLines();
        serialized.Should().Be(expected);
    }

    [Test]
    public void RequestResponse_PlainCamelCaseArrayResult()
    {
        var data = new[] { TestData.Plain };
        var jsonData = JsonSerializer.SerializeToDocument(data, camelCaseSerializerOptions);
        IResponseWrapper response = new SingleResponseWrapper(new UntypedResponse(new StringRpcId(Id), jsonData));

        var serialized = JsonSerializer.Serialize(response, headersJsonSerializerOptions).TrimAllLines();

        var expected = $$"""
            {
                "id": "{{Id}}",
                "result": [
                    {{TestData.PlainFullCamelCaseJson}}
                ],
                "jsonrpc": "2.0"
            }
            """.TrimAllLines();
        serialized.Should().Be(expected);
    }

    [Test]
    public void RequestResponse_NestedSnakeCaseObjectResult()
    {
        var data = TestData.Nested;
        var jsonData = JsonSerializer.SerializeToDocument(data, snakeCaseSerializerOptions);
        IResponseWrapper response = new SingleResponseWrapper(new UntypedResponse(new StringRpcId(Id), jsonData));

        var serialized = JsonSerializer.Serialize(response, headersJsonSerializerOptions).TrimAllLines();

        var expected = $$"""
            {
                "id": "{{Id}}",
                "result": {{TestData.NestedFullSnakeCaseJson}},
                "jsonrpc": "2.0"
            }
            """.TrimAllLines();
        serialized.Should().Be(expected);
    }

    [Test]
    public void RequestResponse_NestedCamelCaseObjectResult()
    {
        var data = TestData.Nested;
        var jsonData = JsonSerializer.SerializeToDocument(data, camelCaseSerializerOptions);
        IResponseWrapper response = new SingleResponseWrapper(new UntypedResponse(new StringRpcId(Id), jsonData));

        var serialized = JsonSerializer.Serialize(response, headersJsonSerializerOptions).TrimAllLines();

        var expected = $$"""
            {
                "id": "{{Id}}",
                "result": {{TestData.NestedFullCamelCaseJson}},
                "jsonrpc": "2.0"
            }
            """.TrimAllLines();
        serialized.Should().Be(expected);
    }

    [Test]
    public void RequestResponse_NestedSnakeCaseArrayResult()
    {
        var data = new[] { TestData.Nested };
        var jsonData = JsonSerializer.SerializeToDocument(data, snakeCaseSerializerOptions);
        IResponseWrapper response = new SingleResponseWrapper(new UntypedResponse(new StringRpcId(Id), jsonData));

        var serialized = JsonSerializer.Serialize(response, headersJsonSerializerOptions).TrimAllLines();

        var expected = $$"""
            {
                "id": "{{Id}}",
                "result": [
                    {{TestData.NestedFullSnakeCaseJson}}
                ],
                "jsonrpc": "2.0"
            }
            """.TrimAllLines();
        serialized.Should().Be(expected);
    }

    [Test]
    public void RequestResponse_NestedCamelCaseArrayResult()
    {
        var data = new[] { TestData.Nested };
        var jsonData = JsonSerializer.SerializeToDocument(data, camelCaseSerializerOptions);
        IResponseWrapper response = new SingleResponseWrapper(new UntypedResponse(new StringRpcId(Id), jsonData));

        var serialized = JsonSerializer.Serialize(response, headersJsonSerializerOptions).TrimAllLines();

        var expected = $$"""
            {
                "id": "{{Id}}",
                "result": [
                    {{TestData.NestedFullCamelCaseJson}}
                ],
                "jsonrpc": "2.0"
            }
            """.TrimAllLines();
        serialized.Should().Be(expected);
    }

    [Test]
    public void RequestResponse_EmptyObjectErrorData()
    {
        var errorCode = 123;
        var errorMessage = "errorMessage";
        var data = new { };
        var jsonData = JsonSerializer.SerializeToDocument(data, snakeCaseSerializerOptions);
        var error = new Error<JsonDocument>(errorCode, errorMessage, jsonData);
        IResponseWrapper response = new SingleResponseWrapper(new UntypedErrorResponse(new StringRpcId(Id), error));

        var serialized = JsonSerializer.Serialize(response, headersJsonSerializerOptions).TrimAllLines();

        var expected = $$"""
            {
                "id": "{{Id}}",
                "error": {
                    "code": {{errorCode}},
                    "message": "{{errorMessage}}",
                    "data": {}
                },
                "jsonrpc": "2.0"
            }
            """.TrimAllLines();
        serialized.Should().Be(expected);
    }

    [Test]
    public void RequestResponse_EmptyArrayErrorData()
    {
        var errorCode = 123;
        var errorMessage = "errorMessage";
        var data = Array.Empty<TestData>();
        var jsonData = JsonSerializer.SerializeToDocument(data, snakeCaseSerializerOptions);
        var error = new Error<JsonDocument>(errorCode, errorMessage, jsonData);
        IResponseWrapper response = new SingleResponseWrapper(new UntypedErrorResponse(new StringRpcId(Id), error));

        var serialized = JsonSerializer.Serialize(response, headersJsonSerializerOptions).TrimAllLines();

        var expected = $$"""
            {
                "id": "{{Id}}",
                "error": {
                    "code": {{errorCode}},
                    "message": "{{errorMessage}}",
                    "data": []
                },
                "jsonrpc": "2.0"
            }
            """.TrimAllLines();
        serialized.Should().Be(expected);
    }

    [Test]
    public void RequestResponse_NullErrorData()
    {
        var errorCode = 123;
        var errorMessage = "errorMessage";
        var error = new Error<JsonDocument>(errorCode, errorMessage, null);
        IResponseWrapper response = new SingleResponseWrapper(new UntypedErrorResponse(new StringRpcId(Id), error));

        var serialized = JsonSerializer.Serialize(response, headersJsonSerializerOptions).TrimAllLines();

        var expected = $$"""
            {
                "id": "{{Id}}",
                "error": {
                    "code": {{errorCode}},
                    "message": "{{errorMessage}}",
                    "data": null
                },
                "jsonrpc": "2.0"
            }
            """.TrimAllLines();
        serialized.Should().Be(expected);
    }

    [Test]
    public void RequestResponse_PrimitiveTypeErrorData()
    {
        var errorCode = 123;
        var errorMessage = "errorMessage";
        var data = 123;
        var jsonData = JsonSerializer.SerializeToDocument(data, snakeCaseSerializerOptions);
        var error = new Error<JsonDocument>(errorCode, errorMessage, jsonData);
        IResponseWrapper response = new SingleResponseWrapper(new UntypedErrorResponse(new StringRpcId(Id), error));

        var serialized = JsonSerializer.Serialize(response, headersJsonSerializerOptions).TrimAllLines();

        var expected = $$"""
            {
                "id": "{{Id}}",
                "error": {
                    "code": {{errorCode}},
                    "message": "{{errorMessage}}",
                    "data": {{data}}
                },
                "jsonrpc": "2.0"
            }
            """.TrimAllLines();
        serialized.Should().Be(expected);
    }

    [Test]
    public void RequestResponse_PrimitiveTypeArrayErrorData()
    {
        var errorCode = 123;
        var errorMessage = "errorMessage";
        var data = new[] { 123 };
        var jsonData = JsonSerializer.SerializeToDocument(data, snakeCaseSerializerOptions);
        var error = new Error<JsonDocument>(errorCode, errorMessage, jsonData);
        IResponseWrapper response = new SingleResponseWrapper(new UntypedErrorResponse(new StringRpcId(Id), error));

        var serialized = JsonSerializer.Serialize(response, headersJsonSerializerOptions).TrimAllLines();

        var expected = $$"""
            {
                "id": "{{Id}}",
                "error": {
                    "code": {{errorCode}},
                    "message": "{{errorMessage}}",
                    "data": [
                        {{data[0]}}
                    ]
                },
                "jsonrpc": "2.0"
            }
            """.TrimAllLines();
        serialized.Should().Be(expected);
    }

    [Test]
    public void RequestResponse_ErrorDataWithResultField()
    {
        var errorCode = 123;
        var errorMessage = "errorMessage";
        var result = "resultValue";
        var data = new ErrorWithResult(result);
        var jsonData = JsonSerializer.SerializeToDocument(data, snakeCaseSerializerOptions);
        var error = new Error<JsonDocument>(errorCode, errorMessage, jsonData);
        IResponseWrapper response = new SingleResponseWrapper(new UntypedErrorResponse(new StringRpcId(Id), error));

        var serialized = JsonSerializer.Serialize(response, headersJsonSerializerOptions).TrimAllLines();

        var expected = $$"""
            {
                "id": "{{Id}}",
                "error": {
                    "code": {{errorCode}},
                    "message": "{{errorMessage}}",
                    "data": {
                        "result": "{{result}}"
                    }
                },
                "jsonrpc": "2.0"
            }
            """.TrimAllLines();
        serialized.Should().Be(expected);
    }

    [Test]
    public void RequestResponse_PlainSnakeCaseObjectErrorData()
    {
        var errorCode = 123;
        var errorMessage = "errorMessage";
        var data = TestData.Plain;
        var jsonData = JsonSerializer.SerializeToDocument(data, snakeCaseSerializerOptions);
        var error = new Error<JsonDocument>(errorCode, errorMessage, jsonData);
        IResponseWrapper response = new SingleResponseWrapper(new UntypedErrorResponse(new StringRpcId(Id), error));

        var serialized = JsonSerializer.Serialize(response, headersJsonSerializerOptions).TrimAllLines();

        var expected = $$"""
            {
                "id": "{{Id}}",
                "error": {
                    "code": {{errorCode}},
                    "message": "{{errorMessage}}",
                    "data": {{TestData.PlainFullSnakeCaseJson}}
                },
                "jsonrpc": "2.0"
            }
            """.TrimAllLines();
        serialized.Should().Be(expected);
    }

    [Test]
    public void RequestResponse_PlainCamelCaseObjectErrorData()
    {
        var errorCode = 123;
        var errorMessage = "errorMessage";
        var data = TestData.Plain;
        var jsonData = JsonSerializer.SerializeToDocument(data, camelCaseSerializerOptions);
        var error = new Error<JsonDocument>(errorCode, errorMessage, jsonData);
        IResponseWrapper response = new SingleResponseWrapper(new UntypedErrorResponse(new StringRpcId(Id), error));

        var serialized = JsonSerializer.Serialize(response, headersJsonSerializerOptions).TrimAllLines();

        var expected = $$"""
            {
                "id": "{{Id}}",
                "error": {
                    "code": {{errorCode}},
                    "message": "{{errorMessage}}",
                    "data": {{TestData.PlainFullCamelCaseJson}}
                },
                "jsonrpc": "2.0"
            }
            """.TrimAllLines();
        serialized.Should().Be(expected);
    }

    [Test]
    public void RequestResponse_PlainSnakeCaseArrayErrorData()
    {
        var errorCode = 123;
        var errorMessage = "errorMessage";
        var data = new[] { TestData.Plain };
        var jsonData = JsonSerializer.SerializeToDocument(data, snakeCaseSerializerOptions);
        var error = new Error<JsonDocument>(errorCode, errorMessage, jsonData);
        IResponseWrapper response = new SingleResponseWrapper(new UntypedErrorResponse(new StringRpcId(Id), error));

        var serialized = JsonSerializer.Serialize(response, headersJsonSerializerOptions).TrimAllLines();

        var expected = $$"""
            {
                "id": "{{Id}}",
                "error": {
                    "code": {{errorCode}},
                    "message": "{{errorMessage}}",
                    "data": [
                        {{TestData.PlainFullSnakeCaseJson}}
                    ]
                },
                "jsonrpc": "2.0"
            }
            """.TrimAllLines();
        serialized.Should().Be(expected);
    }

    [Test]
    public void RequestResponse_PlainCamelCaseArrayErrorData()
    {
        var errorCode = 123;
        var errorMessage = "errorMessage";
        var data = new[] { TestData.Plain };
        var jsonData = JsonSerializer.SerializeToDocument(data, camelCaseSerializerOptions);
        var error = new Error<JsonDocument>(errorCode, errorMessage, jsonData);
        IResponseWrapper response = new SingleResponseWrapper(new UntypedErrorResponse(new StringRpcId(Id), error));

        var serialized = JsonSerializer.Serialize(response, headersJsonSerializerOptions).TrimAllLines();

        var expected = $$"""
            {
                "id": "{{Id}}",
                "error": {
                    "code": {{errorCode}},
                    "message": "{{errorMessage}}",
                    "data": [
                        {{TestData.PlainFullCamelCaseJson}}
                    ]
                },
                "jsonrpc": "2.0"
            }
            """.TrimAllLines();
        serialized.Should().Be(expected);
    }

    [Test]
    public void RequestResponse_NestedSnakeCaseObjectErrorData()
    {
        var errorCode = 123;
        var errorMessage = "errorMessage";
        var data = TestData.Nested;
        var jsonData = JsonSerializer.SerializeToDocument(data, snakeCaseSerializerOptions);
        var error = new Error<JsonDocument>(errorCode, errorMessage, jsonData);
        IResponseWrapper response = new SingleResponseWrapper(new UntypedErrorResponse(new StringRpcId(Id), error));

        var serialized = JsonSerializer.Serialize(response, headersJsonSerializerOptions).TrimAllLines();

        var expected = $$"""
            {
                "id": "{{Id}}",
                "error": {
                    "code": {{errorCode}},
                    "message": "{{errorMessage}}",
                    "data": {{TestData.NestedFullSnakeCaseJson}}
                },
                "jsonrpc": "2.0"
            }
            """.TrimAllLines();
        serialized.Should().Be(expected);
    }

    [Test]
    public void RequestResponse_NestedCamelCaseObjectErrorData()
    {
        var errorCode = 123;
        var errorMessage = "errorMessage";
        var data = TestData.Nested;
        var jsonData = JsonSerializer.SerializeToDocument(data, camelCaseSerializerOptions);
        var error = new Error<JsonDocument>(errorCode, errorMessage, jsonData);
        IResponseWrapper response = new SingleResponseWrapper(new UntypedErrorResponse(new StringRpcId(Id), error));

        var serialized = JsonSerializer.Serialize(response, headersJsonSerializerOptions).TrimAllLines();

        var expected = $$"""
            {
                "id": "{{Id}}",
                "error": {
                    "code": {{errorCode}},
                    "message": "{{errorMessage}}",
                    "data": {{TestData.NestedFullCamelCaseJson}}
                },
                "jsonrpc": "2.0"
            }
            """.TrimAllLines();
        serialized.Should().Be(expected);
    }

    [Test]
    public void RequestResponse_NestedSnakeCaseArrayErrorData()
    {
        var errorCode = 123;
        var errorMessage = "errorMessage";
        var data = new[] { TestData.Nested };
        var jsonData = JsonSerializer.SerializeToDocument(data, snakeCaseSerializerOptions);
        var error = new Error<JsonDocument>(errorCode, errorMessage, jsonData);
        IResponseWrapper response = new SingleResponseWrapper(new UntypedErrorResponse(new StringRpcId(Id), error));

        var serialized = JsonSerializer.Serialize(response, headersJsonSerializerOptions).TrimAllLines();

        var expected = $$"""
            {
                "id": "{{Id}}",
                "error": {
                    "code": {{errorCode}},
                    "message": "{{errorMessage}}",
                    "data": [
                        {{TestData.NestedFullSnakeCaseJson}}
                    ]
                },
                "jsonrpc": "2.0"
            }
            """.TrimAllLines();
        serialized.Should().Be(expected);
    }

    [Test]
    public void RequestResponse_NestedCamelCaseArrayErrorData()
    {
        var errorCode = 123;
        var errorMessage = "errorMessage";
        var data = new[] { TestData.Nested };
        var jsonData = JsonSerializer.SerializeToDocument(data, camelCaseSerializerOptions);
        var error = new Error<JsonDocument>(errorCode, errorMessage, jsonData);
        IResponseWrapper response = new SingleResponseWrapper(new UntypedErrorResponse(new StringRpcId(Id), error));

        var serialized = JsonSerializer.Serialize(response, headersJsonSerializerOptions).TrimAllLines();

        var expected = $$"""
            {
                "id": "{{Id}}",
                "error": {
                    "code": {{errorCode}},
                    "message": "{{errorMessage}}",
                    "data": [
                        {{TestData.NestedFullCamelCaseJson}}
                    ]
                },
                "jsonrpc": "2.0"
            }
            """.TrimAllLines();
        serialized.Should().Be(expected);
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
        var data = new { };
        var jsonData = JsonSerializer.SerializeToDocument(data, snakeCaseSerializerOptions);
        var responses = new List<IResponse>
        {
            new UntypedResponse(new StringRpcId(Id), jsonData)
        };
        IResponseWrapper response = new BatchResponseWrapper(responses);

        var serialized = JsonSerializer.Serialize(response, headersJsonSerializerOptions).TrimAllLines();

        var expected = $$"""
            [
                {
                    "id": "{{Id}}",
                    "result": {},
                    "jsonrpc": "2.0"
                }
            ]
            """.TrimAllLines();
        serialized.Should().Be(expected);
    }

    [Test]
    public void BatchResponse_OnlyResponses()
    {
        var id1 = "123";
        var id2 = "456";
        var data = new { };
        var jsonData = JsonSerializer.SerializeToDocument(data, snakeCaseSerializerOptions);
        var responses = new List<IResponse>
        {
            new UntypedResponse(new StringRpcId(id1), jsonData),
            new UntypedResponse(new StringRpcId(id2), jsonData)
        };
        IResponseWrapper response = new BatchResponseWrapper(responses);

        var serialized = JsonSerializer.Serialize(response, headersJsonSerializerOptions).TrimAllLines();

        var expected = $$"""
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
            """.TrimAllLines();
        serialized.Should().Be(expected);
    }

    [Test]
    public void BatchResponse_1Error()
    {
        var errorCode = 123;
        var errorMessage = "errorMessage";
        var data = new { };
        var jsonData = JsonSerializer.SerializeToDocument(data, snakeCaseSerializerOptions);
        var error = new Error<JsonDocument>(errorCode, errorMessage, jsonData);
        var responses = new List<IResponse>
        {
            new UntypedErrorResponse(new StringRpcId(Id), error)
        };
        IResponseWrapper response = new BatchResponseWrapper(responses);

        var serialized = JsonSerializer.Serialize(response, headersJsonSerializerOptions).TrimAllLines();

        var expected = $$"""
            [
                {
                    "id": "{{Id}}",
                    "error": {
                        "code": {{errorCode}},
                        "message": "{{errorMessage}}",
                        "data": {}
                    },
                    "jsonrpc": "2.0"
                }
            ]
            """.TrimAllLines();
        serialized.Should().Be(expected);
    }

    [Test]
    public void BatchResponse_OnlyErrors()
    {
        var id1 = "123";
        var id2 = "456";
        var errorCode = 123;
        var errorMessage = "errorMessage";
        var data = new { };
        var jsonData = JsonSerializer.SerializeToDocument(data, snakeCaseSerializerOptions);
        var error = new Error<JsonDocument>(errorCode, errorMessage, jsonData);
        var responses = new List<IResponse>
        {
            new UntypedErrorResponse(new StringRpcId(id1), error),
            new UntypedErrorResponse(new StringRpcId(id2), error)
        };
        IResponseWrapper response = new BatchResponseWrapper(responses);

        var serialized = JsonSerializer.Serialize(response, headersJsonSerializerOptions).TrimAllLines();

        var expected = $$"""
            [
                {
                    "id": "{{id1}}",
                    "error": {
                        "code": {{errorCode}},
                        "message": "{{errorMessage}}",
                        "data": {}
                    },
                    "jsonrpc": "2.0"
                },
                {
                    "id": "{{id2}}",
                    "error": {
                        "code": {{errorCode}},
                        "message": "{{errorMessage}}",
                        "data": {}
                    },
                    "jsonrpc": "2.0"
                }
            ]
            """.TrimAllLines();
        serialized.Should().Be(expected);
    }

    [Test]
    public void BatchResponse_ResponseAndError()
    {
        var id1 = "123";
        var id2 = "456";
        var errorCode = 123;
        var errorMessage = "errorMessage";
        var data = new { };
        var jsonData = JsonSerializer.SerializeToDocument(data, snakeCaseSerializerOptions);
        var error = new Error<JsonDocument>(errorCode, errorMessage, jsonData);
        var responses = new List<IResponse>
        {
            new UntypedResponse(new StringRpcId(id1), jsonData),
            new UntypedErrorResponse(new StringRpcId(id2), error)
        };
        IResponseWrapper response = new BatchResponseWrapper(responses);

        var serialized = JsonSerializer.Serialize(response, headersJsonSerializerOptions).TrimAllLines();

        var expected = $$"""
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
                        "message": "{{errorMessage}}",
                        "data": {}
                    },
                    "jsonrpc": "2.0"
                }
            ]
            """.TrimAllLines();
        serialized.Should().Be(expected);
    }

    #endregion

    private const string Method = "method";
    private const string Id = "id";
}
