using System;
using System.Collections.Generic;
using System.Text.Json;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Tochka.JsonRpc.Client.Models;
using Tochka.JsonRpc.Client.Models.SingleResult;
using Tochka.JsonRpc.Common;
using Tochka.JsonRpc.Common.Models.Id;
using Tochka.JsonRpc.Common.Models.Response;
using Tochka.JsonRpc.Common.Models.Response.Errors;
using Tochka.JsonRpc.Common.Models.Response.Untyped;
using Tochka.JsonRpc.TestUtils;

namespace Tochka.JsonRpc.Client.Tests.Models;

[TestFixture]
public class SingleJsonRpcResultTests
{
    private Mock<IJsonRpcCallContext> callContextMock;
    private JsonSerializerOptions headersJsonSerializerOptions;
    private JsonSerializerOptions dataJsonSerializerOptions;
    private SingleJsonRpcResult singleJsonRpcResult;

    [SetUp]
    public void Setup()
    {
        callContextMock = new Mock<IJsonRpcCallContext>();
        headersJsonSerializerOptions = JsonRpcSerializerOptions.Headers;
        dataJsonSerializerOptions = JsonRpcSerializerOptions.SnakeCase;
    }

    [Test]
    public void Ctor_BatchResponseNotNull_Throw()
    {
        callContextMock.Setup(static c => c.BatchResponse)
            .Returns(new List<IResponse>());

        var action = Initialize;

        action.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Test]
    public void GetResponseOrThrow_NoResponse_Throw()
    {
        callContextMock.Setup(static c => c.SingleResponse)
            .Returns((IResponse?) null);
        Initialize();

        var action = () => singleJsonRpcResult.GetResponseOrThrow<TestData>();

        action.Should().Throw<JsonRpcException>();
    }

    [Test]
    public void GetResponseOrThrow_ResultIsNull_ReturnNull()
    {
        callContextMock.Setup(static c => c.SingleResponse)
            .Returns(new UntypedResponse(new NullRpcId(), null));
        Initialize();

        var result = singleJsonRpcResult.GetResponseOrThrow<TestData>();

        result.Should().BeNull();
    }

    [Test]
    public void GetResponseOrThrow_ResultNotNull_ReturnDeserializedResult()
    {
        callContextMock.Setup(static c => c.SingleResponse)
            .Returns(new UntypedResponse(new NullRpcId(), JsonDocument.Parse(TestData.PlainRequiredSnakeCaseJson)));
        Initialize();

        var result = singleJsonRpcResult.GetResponseOrThrow<TestData>();

        result.Should().BeEquivalentTo(TestData.Plain);
    }

    [Test]
    public void GetResponseOrThrow_ErrorResult_Throw()
    {
        var errorResponse = new UntypedErrorResponse(new NullRpcId(), new Error<JsonDocument>(1, "message", null));
        callContextMock.Setup(static c => c.SingleResponse)
            .Returns(errorResponse);
        Initialize();

        var action = () => singleJsonRpcResult.GetResponseOrThrow<TestData>();

        action.Should().Throw<JsonRpcException>();
        callContextMock.Verify(c => c.WithError(errorResponse));
    }

    [Test]
    public void GetResponseOrThrow_UnknownType_Throw()
    {
        callContextMock.Setup(static c => c.SingleResponse)
            .Returns(Mock.Of<IResponse>());
        Initialize();

        var action = () => singleJsonRpcResult.GetResponseOrThrow<TestData>();

        action.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Test]
    public void AsResponse_ResultNotNull_ReturnDeserializedResult()
    {
        callContextMock.Setup(static c => c.SingleResponse)
            .Returns(new UntypedResponse(new NullRpcId(), JsonDocument.Parse(TestData.PlainRequiredSnakeCaseJson)));
        Initialize();

        var result = singleJsonRpcResult.AsResponse<TestData>();

        result.Should().BeEquivalentTo(TestData.Plain);
    }

    [Test]
    public void AsResponse_NoResponse_ReturnNull()
    {
        callContextMock.Setup(static c => c.SingleResponse)
            .Returns((IResponse?) null);
        Initialize();

        var result = singleJsonRpcResult.AsResponse<TestData>();

        result.Should().BeNull();
    }

    [Test]
    public void AsResponse_ResultIsNull_ReturnNull()
    {
        callContextMock.Setup(static c => c.SingleResponse)
            .Returns(new UntypedResponse(new NullRpcId(), null));
        Initialize();

        var result = singleJsonRpcResult.AsResponse<TestData>();

        result.Should().BeNull();
    }

    [Test]
    public void AsResponse_ErrorResponse_ReturnNull()
    {
        callContextMock.Setup(static c => c.SingleResponse)
            .Returns(new UntypedErrorResponse(new NullRpcId(), new Error<JsonDocument>(1, "message", null)));
        Initialize();

        var result = singleJsonRpcResult.AsResponse<TestData>();

        result.Should().BeNull();
    }

    [Test]
    public void HasError_NoResponse_ReturnFalse()
    {
        callContextMock.Setup(static c => c.SingleResponse)
            .Returns((IResponse?) null);
        Initialize();

        var result = singleJsonRpcResult.HasError();

        result.Should().BeFalse();
    }

    [Test]
    public void HasError_SuccessfulResponse_ReturnFalse()
    {
        callContextMock.Setup(static c => c.SingleResponse)
            .Returns(new UntypedResponse(new NullRpcId(), null));
        Initialize();

        var result = singleJsonRpcResult.HasError();

        result.Should().BeFalse();
    }

    [Test]
    public void HasError_ErrorResponse_ReturnTrue()
    {
        callContextMock.Setup(static c => c.SingleResponse)
            .Returns(new UntypedErrorResponse(new NullRpcId(), new Error<JsonDocument>(1, "message", null)));
        Initialize();

        var result = singleJsonRpcResult.HasError();

        result.Should().BeTrue();
    }

    [Test]
    public void AsAnyError_ErrorResponse_ReturnsError()
    {
        var error = new Error<JsonDocument>(1, "message", null);
        callContextMock.Setup(static c => c.SingleResponse)
            .Returns(new UntypedErrorResponse(new NullRpcId(), error));
        Initialize();

        var result = singleJsonRpcResult.Advanced.AsAnyError();

        result.Should().BeEquivalentTo(error);
    }

    [Test]
    public void AsAnyError_NoResponse_ReturnNull()
    {
        callContextMock.Setup(static c => c.SingleResponse)
            .Returns((IResponse?) null);
        Initialize();

        var result = singleJsonRpcResult.Advanced.AsAnyError();

        result.Should().BeNull();
    }

    [Test]
    public void AsAnyError_SuccessfulResponse_ReturnNull()
    {
        callContextMock.Setup(static c => c.SingleResponse)
            .Returns(new UntypedResponse(new NullRpcId(), null));
        Initialize();

        var result = singleJsonRpcResult.Advanced.AsAnyError();

        result.Should().BeNull();
    }

    [Test]
    public void AsTypedError_ErrorResponse_ReturnErrorWithDeserializedData()
    {
        var error = new Error<JsonDocument>(1, "message", JsonDocument.Parse(TestData.PlainRequiredSnakeCaseJson));
        callContextMock.Setup(static c => c.SingleResponse)
            .Returns(new UntypedErrorResponse(new NullRpcId(), error));
        Initialize();

        var result = singleJsonRpcResult.Advanced.AsTypedError<TestData>();

        result.Code.Should().Be(error.Code);
        result.Message.Should().Be(error.Message);
        result.Data.Should().BeEquivalentTo(TestData.Plain);
    }

    [Test]
    public void AsTypedError_NoResponse_ReturnNull()
    {
        callContextMock.Setup(static c => c.SingleResponse)
            .Returns((IResponse?) null);
        Initialize();

        var result = singleJsonRpcResult.Advanced.AsTypedError<TestData>();

        result.Should().BeNull();
    }

    [Test]
    public void AsTypedError_SuccessfulResponse_ReturnNull()
    {
        callContextMock.Setup(static c => c.SingleResponse)
            .Returns(new UntypedResponse(new NullRpcId(), null));
        Initialize();

        var result = singleJsonRpcResult.Advanced.AsTypedError<TestData>();

        result.Should().BeNull();
    }

    // need to manually initialize it to update inner properties;
    private SingleJsonRpcResult Initialize() =>
        singleJsonRpcResult = new(callContextMock.Object, headersJsonSerializerOptions, dataJsonSerializerOptions);
}
