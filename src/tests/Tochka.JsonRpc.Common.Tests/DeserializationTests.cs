using System.Text.Json;
using FluentAssertions;
using FluentAssertions.Equivalency;
using NUnit.Framework;
using Tochka.JsonRpc.Common.Models.Id;
using Tochka.JsonRpc.Common.Models.Response.Untyped;
using Tochka.JsonRpc.Common.Models.Response.Wrappers;

namespace Tochka.JsonRpc.Common.Tests;

[TestFixture]
internal class DeserializationTests
{
    private readonly JsonSerializerOptions headersJsonSerializerOptions = JsonRpcSerializerOptions.Headers;
    // private readonly JsonSerializerOptions snakeCaseSerializerOptions = JsonRpcSerializerOptions.SnakeCase;
    // private readonly JsonSerializerOptions camelCaseSerializerOptions = JsonRpcSerializerOptions.CamelCase;

    #region Notification

    [Test]
    public void Notification_EmptyObjectParams()
    {
    }

    [Test]
    public void Notification_EmptyArrayParams()
    {
    }

    [Test]
    public void Notification_NullParams()
    {
    }

    [Test]
    public void Notification_NoParams()
    {
    }

    [Test]
    public void Notification_SnakeCaseMethod()
    {
    }

    [Test]
    public void Notification_CamelCaseMethod()
    {
    }

    [Test]
    public void Notification_PrimitiveTypeParams()
    {
    }

    [Test]
    public void Notification_PrimitiveTypeArrayParams()
    {
    }

    [Test]
    public void Notification_PlainSnakeSnakeCaseObjectParams()
    {
    }

    [Test]
    public void Notification_PlainCamelSnakeCaseObjectParams()
    {
    }

    [Test]
    public void Notification_PlainSnakeSnakeCaseArrayParams()
    {
    }

    [Test]
    public void Notification_PlainCamelSnakeCaseArrayParams()
    {
    }

    [Test]
    public void Notification_NestedSnakeSnakeCaseObjectParams()
    {
    }

    [Test]
    public void Notification_NestedCamelSnakeCaseObjectParams()
    {
    }

    [Test]
    public void Notification_NestedSnakeSnakeCaseArrayParams()
    {
    }

    [Test]
    public void Notification_NestedCamelSnakeCaseArrayParams()
    {
    }

    #endregion

    #region Request

    [Test]
    public void Request_EmptyObjectParams()
    {
    }

    [Test]
    public void Request_EmptyArrayParams()
    {
    }

    [Test]
    public void Request_NullParams()
    {
    }

    [Test]
    public void Request_NoParams()
    {
    }

    [Test]
    public void Request_IntId()
    {
    }

    [Test]
    public void Request_StringId()
    {
    }

    [Test]
    public void Request_NullId()
    {
    }

    [Test]
    public void Request_SnakeCaseMethod()
    {
    }

    [Test]
    public void Request_CamelCaseMethod()
    {
    }

    [Test]
    public void Request_PrimitiveTypeParams()
    {
    }

    [Test]
    public void Request_PrimitiveTypeArrayParams()
    {
    }

    [Test]
    public void Request_PlainSnakeSnakeCaseObjectParams()
    {
    }

    [Test]
    public void Request_PlainCamelSnakeCaseObjectParams()
    {
    }

    [Test]
    public void Request_PlainSnakeSnakeCaseArrayParams()
    {
    }

    [Test]
    public void Request_PlainCamelSnakeCaseArrayParams()
    {
    }

    [Test]
    public void Request_NestedSnakeSnakeCaseObjectParams()
    {
    }

    [Test]
    public void Request_NestedCamelSnakeCaseObjectParams()
    {
    }

    [Test]
    public void Request_NestedSnakeSnakeCaseArrayParams()
    {
    }

    [Test]
    public void Request_NestedCamelSnakeCaseArrayParams()
    {
    }

    #endregion

    #region RequestResponse

    [Test]
    public void RequestResponse_EmptyObjectResult()
    {
        var id = "123";
        var json = $@"{{
    ""id"": ""{id}"",
    ""result"": {{}},
    ""jsonrpc"": ""2.0""
}}";

        var deserialized = JsonSerializer.Deserialize<IResponseWrapper>(json, headersJsonSerializerOptions);

        var expected = new SingleResponseWrapper(new UntypedResponse(new StringRpcId(id), AnyJsonDocument));
        deserialized.Should().BeEquivalentTo(expected, AssertionOptions);
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
    public void RequestResponse_NoResult()
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
    public void RequestResponse_PlainSnakeSnakeCaseObjectResult()
    {
    }

    [Test]
    public void RequestResponse_PlainCamelSnakeCaseObjectResult()
    {
    }

    [Test]
    public void RequestResponse_PlainSnakeSnakeCaseArrayResult()
    {
    }

    [Test]
    public void RequestResponse_PlainCamelSnakeCaseArrayResult()
    {
    }

    [Test]
    public void RequestResponse_NestedSnakeSnakeCaseObjectResult()
    {
    }

    [Test]
    public void RequestResponse_NestedCamelSnakeCaseObjectResult()
    {
    }

    [Test]
    public void RequestResponse_NestedSnakeSnakeCaseArrayResult()
    {
    }

    [Test]
    public void RequestResponse_NestedCamelSnakeCaseArrayResult()
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
    public void RequestResponse_PlainSnakeSnakeCaseObjectErrorData()
    {
    }

    [Test]
    public void RequestResponse_PlainCamelSnakeCaseObjectErrorData()
    {
    }

    [Test]
    public void RequestResponse_PlainSnakeSnakeCaseArrayErrorData()
    {
    }

    [Test]
    public void RequestResponse_PlainCamelSnakeCaseArrayErrorData()
    {
    }

    [Test]
    public void RequestResponse_NestedSnakeSnakeCaseObjectErrorData()
    {
    }

    [Test]
    public void RequestResponse_NestedCamelSnakeCaseObjectErrorData()
    {
    }

    [Test]
    public void RequestResponse_NestedSnakeSnakeCaseArrayErrorData()
    {
    }

    [Test]
    public void RequestResponse_NestedCamelSnakeCaseArrayErrorData()
    {
    }

    #endregion

    #region Batch

    [Test]
    public void Batch_1Notification()
    {
    }

    [Test]
    public void Batch_OnlyNotifications()
    {
    }

    [Test]
    public void Batch_1Request()
    {
    }

    [Test]
    public void Batch_OnlyRequests()
    {
    }

    [Test]
    public void Batch_NotificationsAndRequests()
    {
    }

    #endregion

    #region BatchResponse

    [Test]
    public void Batch_1Response()
    {
    }

    [Test]
    public void Batch_OnlyResponses()
    {
    }

    [Test]
    public void Batch_1Error()
    {
    }

    [Test]
    public void Batch_OnlyErrors()
    {
    }

    [Test]
    public void Batch_ResponsesAndErrors()
    {
    }

    #endregion

    private static EquivalencyAssertionOptions<T> AssertionOptions<T>(EquivalencyAssertionOptions<T> options) => options
        .RespectingRuntimeTypes()
        .Excluding(static info => info.Type == typeof(JsonDocument));

    private static readonly JsonDocument AnyJsonDocument = JsonDocument.Parse("{}");

    // private const string Method = "method";
    // private const string SnakeCaseMethod = "snake_case_method";
    // private const string CamelCaseMethod = "camel_case_method";
}
