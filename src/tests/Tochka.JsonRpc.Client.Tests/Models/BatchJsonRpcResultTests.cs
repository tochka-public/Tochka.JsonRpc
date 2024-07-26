using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.Json;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Tochka.JsonRpc.Client.Models;
using Tochka.JsonRpc.Client.Models.BatchResult;
using Tochka.JsonRpc.Common;
using Tochka.JsonRpc.Common.Models.Id;
using Tochka.JsonRpc.Common.Models.Response;
using Tochka.JsonRpc.Common.Models.Response.Errors;
using Tochka.JsonRpc.Common.Models.Response.Untyped;
using Tochka.JsonRpc.TestUtils;

namespace Tochka.JsonRpc.Client.Tests.Models;

[TestFixture]
public class BatchJsonRpcResultTests
{
    private Mock<IJsonRpcCallContext> callContextMock;
    private JsonSerializerOptions headersJsonSerializerOptions;
    private JsonSerializerOptions dataJsonSerializerOptions;
    private BatchJsonRpcResult batchJsonRpcResult;

    [SetUp]
    public void Setup()
    {
        callContextMock = new Mock<IJsonRpcCallContext>();
        headersJsonSerializerOptions = JsonRpcSerializerOptions.Headers;
        dataJsonSerializerOptions = JsonRpcSerializerOptions.SnakeCase;
    }

    [Test]
    public void Ctor_SingleResponseNotNull_Throw()
    {
        callContextMock.Setup(static c => c.SingleResponse)
            .Returns(Mock.Of<IResponse>());

        var action = Initialize;

        action.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Test]
    public void GetResponseOrThrow_NoResponse_Throw()
    {
        var responses = new List<IResponse>();
        callContextMock.Setup(static c => c.BatchResponse)
            .Returns(responses);
        Initialize();

        var action = () => batchJsonRpcResult.GetResponseOrThrow<TestData>(new StringRpcId(StringId));

        action.Should().Throw<JsonRpcException>();
    }

    [Test]
    public void GetResponseOrThrow_ResultIsNull_ReturnNull()
    {
        var responses = new List<IResponse>
        {
            new UntypedResponse(new StringRpcId(StringId), null)
        };
        callContextMock.Setup(static c => c.BatchResponse)
            .Returns(responses);
        Initialize();

        var result = batchJsonRpcResult.GetResponseOrThrow<TestData>(new StringRpcId(StringId));

        result.Should().BeNull();
    }

    [Test]
    public void GetResponseOrThrow_ResultNotNull_ReturnDeserializedResult()
    {
        var responses = new List<IResponse>
        {
            new UntypedResponse(new StringRpcId(StringId), JsonDocument.Parse(TestData.PlainRequiredSnakeCaseJson))
        };
        callContextMock.Setup(static c => c.BatchResponse)
            .Returns(responses);
        Initialize();

        var result = batchJsonRpcResult.GetResponseOrThrow<TestData>(new StringRpcId(StringId));

        result.Should().BeEquivalentTo(TestData.Plain);
    }

    [Test]
    public void GetResponseOrThrow_ErrorResult_Throw()
    {
        var errorResponse = new UntypedErrorResponse(new StringRpcId(StringId), new Error<JsonDocument>(1, "message", null));
        var responses = new List<IResponse>
        {
            errorResponse
        };
        callContextMock.Setup(static c => c.BatchResponse)
            .Returns(responses);
        Initialize();

        var action = () => batchJsonRpcResult.GetResponseOrThrow<TestData>(new StringRpcId(StringId));

        action.Should().Throw<JsonRpcException>();
        callContextMock.Verify(c => c.WithError(errorResponse));
    }

    [Test]
    public void GetResponseOrThrow_UnknownType_Throw()
    {
        var responseMock = new Mock<IResponse>();
        responseMock.Setup(static r => r.Id)
            .Returns(new StringRpcId(StringId));
        var responses = new List<IResponse>
        {
            responseMock.Object
        };
        callContextMock.Setup(static c => c.BatchResponse)
            .Returns(responses);
        Initialize();

        var action = () => batchJsonRpcResult.GetResponseOrThrow<TestData>(new StringRpcId(StringId));

        action.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Test]
    public void GetResponseOrThrow_WorksForAnyId()
    {
        var stringIdResult = 1;
        var intIdResult = 2;
        var floatIdResult = 2.5;
        var nullIdResult = 3;
        var responses = new List<IResponse>
        {
            new UntypedResponse(new StringRpcId(StringId), JsonDocument.Parse($"{stringIdResult}")),
            new UntypedResponse(new NumberRpcId(IntId), JsonDocument.Parse($"{intIdResult}")),
            new UntypedResponse(new FloatNumberRpcId(FloatId), JsonDocument.Parse($"{floatIdResult.ToString(CultureInfo.InvariantCulture)}")),
            new UntypedResponse(new NullRpcId(), JsonDocument.Parse($"{nullIdResult}"))
        };
        callContextMock.Setup(static c => c.BatchResponse)
            .Returns(responses);
        Initialize();

        batchJsonRpcResult.GetResponseOrThrow<int>(new StringRpcId(StringId)).Should().Be(stringIdResult);
        batchJsonRpcResult.GetResponseOrThrow<int>(new NumberRpcId(IntId)).Should().Be(intIdResult);
        batchJsonRpcResult.GetResponseOrThrow<double>(new FloatNumberRpcId(FloatId)).Should().Be(floatIdResult);
        batchJsonRpcResult.GetResponseOrThrow<int>(new NullRpcId()).Should().Be(nullIdResult);
    }

    [Test]
    public void AsResponse_ResultNotNull_ReturnDeserializedResult()
    {
        var responses = new List<IResponse>
        {
            new UntypedResponse(new StringRpcId(StringId), JsonDocument.Parse(TestData.PlainRequiredSnakeCaseJson))
        };
        callContextMock.Setup(static c => c.BatchResponse)
            .Returns(responses);
        Initialize();

        var result = batchJsonRpcResult.AsResponse<TestData>(new StringRpcId(StringId));

        result.Should().BeEquivalentTo(TestData.Plain);
    }

    [Test]
    public void AsResponse_NoResponse_ReturnNull()
    {
        var responses = new List<IResponse>();
        callContextMock.Setup(static c => c.BatchResponse)
            .Returns(responses);
        Initialize();

        var result = batchJsonRpcResult.AsResponse<TestData>(new StringRpcId(StringId));

        result.Should().BeNull();
    }

    [Test]
    public void AsResponse_ResultIsNull_ReturnNull()
    {
        var responses = new List<IResponse>
        {
            new UntypedResponse(new StringRpcId(StringId), null)
        };
        callContextMock.Setup(static c => c.BatchResponse)
            .Returns(responses);
        Initialize();

        var result = batchJsonRpcResult.AsResponse<TestData>(new StringRpcId(StringId));

        result.Should().BeNull();
    }

    [Test]
    public void AsResponse_ErrorResponse_ReturnNull()
    {
        var responses = new List<IResponse>
        {
            new UntypedErrorResponse(new StringRpcId(StringId), new Error<JsonDocument>(1, "message", null))
        };
        callContextMock.Setup(static c => c.BatchResponse)
            .Returns(responses);
        Initialize();

        var result = batchJsonRpcResult.AsResponse<TestData>(new StringRpcId(StringId));

        result.Should().BeNull();
    }

    [Test]
    public void AsResponse_WorksForAnyId()
    {
        var stringIdResult = 1;
        var intIdResult = 2;
        var floatIdResult = 2.5;
        var nullIdResult = 3;
        var responses = new List<IResponse>
        {
            new UntypedResponse(new StringRpcId(StringId), JsonDocument.Parse($"{stringIdResult}")),
            new UntypedResponse(new NumberRpcId(IntId), JsonDocument.Parse($"{intIdResult}")),
            new UntypedResponse(new FloatNumberRpcId(FloatId), JsonDocument.Parse($"{floatIdResult.ToString(CultureInfo.InvariantCulture)}")),
            new UntypedResponse(new NullRpcId(), JsonDocument.Parse($"{nullIdResult}"))
        };
        callContextMock.Setup(static c => c.BatchResponse)
            .Returns(responses);
        Initialize();

        batchJsonRpcResult.AsResponse<int>(new StringRpcId(StringId)).Should().Be(stringIdResult);
        batchJsonRpcResult.AsResponse<int>(new NumberRpcId(IntId)).Should().Be(intIdResult);
        batchJsonRpcResult.AsResponse<double>(new FloatNumberRpcId(FloatId)).Should().Be(floatIdResult);
        batchJsonRpcResult.AsResponse<int>(new NullRpcId()).Should().Be(nullIdResult);
    }

    [Test]
    public void HasError_NoResponse_Throw()
    {
        var responses = new List<IResponse>();
        callContextMock.Setup(static c => c.BatchResponse)
            .Returns(responses);
        Initialize();

        var action = () => batchJsonRpcResult.HasError(new StringRpcId(StringId));

        action.Should().Throw<JsonRpcException>();
    }

    [Test]
    public void HasError_SuccessfulResponse_ReturnFalse()
    {
        var responses = new List<IResponse>
        {
            new UntypedResponse(new StringRpcId(StringId), null)
        };
        callContextMock.Setup(static c => c.BatchResponse)
            .Returns(responses);
        Initialize();

        var result = batchJsonRpcResult.HasError(new StringRpcId(StringId));

        result.Should().BeFalse();
    }

    [Test]
    public void HasError_ErrorResponse_ReturnTrue()
    {
        var responses = new List<IResponse>
        {
            new UntypedErrorResponse(new StringRpcId(StringId), new Error<JsonDocument>(1, "message", null))
        };
        callContextMock.Setup(static c => c.BatchResponse)
            .Returns(responses);
        Initialize();

        var result = batchJsonRpcResult.HasError(new StringRpcId(StringId));

        result.Should().BeTrue();
    }

    [Test]
    public void HasError_WorksForAnyId()
    {
        var error = new Error<JsonDocument>(1, "message", null);
        var responses = new List<IResponse>
        {
            new UntypedErrorResponse(new StringRpcId(StringId), error),
            new UntypedErrorResponse(new NumberRpcId(IntId), error),
            new UntypedErrorResponse(new FloatNumberRpcId(FloatId), error),
            new UntypedErrorResponse(new NullRpcId(), error)
        };
        callContextMock.Setup(static c => c.BatchResponse)
            .Returns(responses);
        Initialize();

        batchJsonRpcResult.HasError(new StringRpcId(StringId)).Should().BeTrue();
        batchJsonRpcResult.HasError(new NumberRpcId(IntId)).Should().BeTrue();
        batchJsonRpcResult.HasError(new FloatNumberRpcId(FloatId)).Should().BeTrue();
        batchJsonRpcResult.HasError(new NullRpcId()).Should().BeTrue();
    }

    [Test]
    public void AsAnyError_ErrorResponse_ReturnsError()
    {
        var error = new Error<JsonDocument>(1, "message", null);
        var responses = new List<IResponse>
        {
            new UntypedErrorResponse(new StringRpcId(StringId), error)
        };
        callContextMock.Setup(static c => c.BatchResponse)
            .Returns(responses);
        Initialize();

        var result = batchJsonRpcResult.Advanced.AsAnyError(new StringRpcId(StringId));

        result.Should().BeEquivalentTo(error);
    }

    [Test]
    public void AsAnyError_NoResponse_ReturnNull()
    {
        var responses = new List<IResponse>();
        callContextMock.Setup(static c => c.BatchResponse)
            .Returns(responses);
        Initialize();

        var result = batchJsonRpcResult.Advanced.AsAnyError(new StringRpcId(StringId));

        result.Should().BeNull();
    }

    [Test]
    public void AsAnyError_SuccessfulResponse_ReturnNull()
    {
        var responses = new List<IResponse>
        {
            new UntypedResponse(new StringRpcId(StringId), null)
        };
        callContextMock.Setup(static c => c.BatchResponse)
            .Returns(responses);
        Initialize();

        var result = batchJsonRpcResult.Advanced.AsAnyError(new StringRpcId(StringId));

        result.Should().BeNull();
    }

    [Test]
    public void AsAnyError_WorksForAnyId()
    {
        var stringIdError = new Error<JsonDocument>(1, "message", null);
        var intIdError = new Error<JsonDocument>(2, "message", null);
        var floatIdError = new Error<JsonDocument>(2, "message", null);
        var nullIdError = new Error<JsonDocument>(3, "message", null);
        var responses = new List<IResponse>
        {
            new UntypedErrorResponse(new StringRpcId(StringId), stringIdError),
            new UntypedErrorResponse(new NumberRpcId(IntId), intIdError),
            new UntypedErrorResponse(new FloatNumberRpcId(FloatId), floatIdError),
            new UntypedErrorResponse(new NullRpcId(), nullIdError)
        };
        callContextMock.Setup(static c => c.BatchResponse)
            .Returns(responses);
        Initialize();

        batchJsonRpcResult.Advanced.AsAnyError(new StringRpcId(StringId)).Should().BeEquivalentTo(stringIdError);
        batchJsonRpcResult.Advanced.AsAnyError(new NumberRpcId(IntId)).Should().BeEquivalentTo(intIdError);
        batchJsonRpcResult.Advanced.AsAnyError(new FloatNumberRpcId(FloatId)).Should().BeEquivalentTo(floatIdError);
        batchJsonRpcResult.Advanced.AsAnyError(new NullRpcId()).Should().BeEquivalentTo(nullIdError);
    }

    [Test]
    public void AsTypedError_ErrorResponse_ReturnErrorWithDeserializedData()
    {
        var error = new Error<JsonDocument>(1, "message", JsonDocument.Parse(TestData.PlainRequiredSnakeCaseJson));
        var responses = new List<IResponse>
        {
            new UntypedErrorResponse(new StringRpcId(StringId), error)
        };
        callContextMock.Setup(static c => c.BatchResponse)
            .Returns(responses);
        Initialize();

        var result = batchJsonRpcResult.Advanced.AsTypedError<TestData>(new StringRpcId(StringId));

        result.Code.Should().Be(error.Code);
        result.Message.Should().Be(error.Message);
        result.Data.Should().BeEquivalentTo(TestData.Plain);
    }

    [Test]
    public void AsTypedError_NoResponse_ReturnNull()
    {
        var responses = new List<IResponse>();
        callContextMock.Setup(static c => c.BatchResponse)
            .Returns(responses);
        Initialize();

        var result = batchJsonRpcResult.Advanced.AsTypedError<TestData>(new StringRpcId(StringId));

        result.Should().BeNull();
    }

    [Test]
    public void AsTypedError_SuccessfulResponse_ReturnNull()
    {
        var responses = new List<IResponse>
        {
            new UntypedResponse(new StringRpcId(StringId), null)
        };
        callContextMock.Setup(static c => c.BatchResponse)
            .Returns(responses);
        Initialize();

        var result = batchJsonRpcResult.Advanced.AsTypedError<TestData>(new StringRpcId(StringId));

        result.Should().BeNull();
    }

    [Test]
    public void AsTypedError_WorksForAnyId()
    {
        var stringIdErrorData = 1;
        var intIdErrorData = 2;
        var floatIdErrorData = 2.5;
        var nullIdErrorData = 3;
        var responses = new List<IResponse>
        {
            new UntypedErrorResponse(new StringRpcId(StringId), new Error<JsonDocument>(1, "message", JsonDocument.Parse($"{stringIdErrorData}"))),
            new UntypedErrorResponse(new NumberRpcId(IntId), new Error<JsonDocument>(1, "message", JsonDocument.Parse($"{intIdErrorData}"))),
            new UntypedErrorResponse(new FloatNumberRpcId(FloatId), new Error<JsonDocument>(1, "message", JsonDocument.Parse($"{floatIdErrorData.ToString(CultureInfo.InvariantCulture)}"))),
            new UntypedErrorResponse(new NullRpcId(), new Error<JsonDocument>(1, "message", JsonDocument.Parse($"{nullIdErrorData}")))
        };
        callContextMock.Setup(static c => c.BatchResponse)
            .Returns(responses);
        Initialize();

        batchJsonRpcResult.Advanced.AsTypedError<int>(new StringRpcId(StringId)).Data.Should().Be(stringIdErrorData);
        batchJsonRpcResult.Advanced.AsTypedError<int>(new NumberRpcId(IntId)).Data.Should().Be(intIdErrorData);
        batchJsonRpcResult.Advanced.AsTypedError<double>(new FloatNumberRpcId(FloatId)).Data.Should().Be(floatIdErrorData);
        batchJsonRpcResult.Advanced.AsTypedError<int>(new NullRpcId()).Data.Should().Be(nullIdErrorData);
    }

    // need to manually initialize it to update inner collections;
    private BatchJsonRpcResult Initialize() =>
        batchJsonRpcResult = new(callContextMock.Object, headersJsonSerializerOptions, dataJsonSerializerOptions);

    private const string StringId = "123";
    private const int IntId = 123;
    private const double FloatId = 123.5;
}
