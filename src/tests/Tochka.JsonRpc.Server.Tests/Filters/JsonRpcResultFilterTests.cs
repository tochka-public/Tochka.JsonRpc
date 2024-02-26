using System.Collections.Generic;
using System.Text.Json;
using FluentAssertions;
using Json.More;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using Tochka.JsonRpc.Common.Features;
using Tochka.JsonRpc.Common.Models.Id;
using Tochka.JsonRpc.Common.Models.Request.Untyped;
using Tochka.JsonRpc.Common.Models.Response.Errors;
using Tochka.JsonRpc.Common.Models.Response.Untyped;
using Tochka.JsonRpc.Server.Exceptions;
using Tochka.JsonRpc.Server.Extensions;
using Tochka.JsonRpc.Server.Filters;
using Tochka.JsonRpc.Server.Serialization;
using Tochka.JsonRpc.Server.Services;
using Tochka.JsonRpc.Server.Settings;

namespace Tochka.JsonRpc.Server.Tests.Filters;

[TestFixture]
internal class JsonRpcResultFilterTests
{
    private List<IJsonSerializerOptionsProvider> serializerOptionsProviders;
    private JsonRpcServerOptions options;
    private Mock<IJsonRpcErrorFactory> errorFactoryMock;
    private JsonRpcResultFilter resultFilter;

    [SetUp]
    public void Setup()
    {
        serializerOptionsProviders = new List<IJsonSerializerOptionsProvider>();
        options = new JsonRpcServerOptions();
        errorFactoryMock = new Mock<IJsonRpcErrorFactory>();

        resultFilter = new JsonRpcResultFilter(serializerOptionsProviders, Options.Create(options), errorFactoryMock.Object);
    }

    [Test]
    public void OnResultExecuting_NotJsonRpc_DoNothing()
    {
        var httpContext = new DefaultHttpContext();
        var actionContext = new ActionContext(httpContext, new RouteData(), new ActionDescriptor(), new ModelStateDictionary());
        var result = Mock.Of<IActionResult>();
        var context = new ResultExecutingContext(actionContext, new List<IFilterMetadata>(), result, new object());

        resultFilter.OnResultExecuting(context);

        context.Result.Should().Be(result);
        httpContext.Features.Get<IJsonRpcFeature>().Should().BeNull();
    }

    [Test]
    public void OnResultExecuting_CallIsNotification_SetStatusResult200()
    {
        var httpContext = new DefaultHttpContext();
        var jsonRpcFeature = new JsonRpcFeature { Call = new UntypedNotification("method", null) };
        httpContext.Features.Set<IJsonRpcFeature>(jsonRpcFeature);
        var actionContext = new ActionContext(httpContext, new RouteData(), new ActionDescriptor(), new ModelStateDictionary());
        var context = new ResultExecutingContext(actionContext, new List<IFilterMetadata>(), Mock.Of<IActionResult>(), new object());

        resultFilter.OnResultExecuting(context);

        var expected = new StatusCodeResult(StatusCodes.Status200OK);
        context.Result.Should().BeEquivalentTo(expected);
        jsonRpcFeature.Response.Should().BeNull();
    }

    [Test]
    public void OnResultExecuting_RawResponseForbiddenByOptions_Throw()
    {
        var httpContext = new DefaultHttpContext();
        var jsonRpcFeature = new JsonRpcFeature { Call = new UntypedRequest(new NullRpcId(), "method", null) };
        httpContext.Features.Set<IJsonRpcFeature>(jsonRpcFeature);
        var actionContext = new ActionContext(httpContext, new RouteData(), new ActionDescriptor(), new ModelStateDictionary());
        var result = Mock.Of<IActionResult>();
        var context = new ResultExecutingContext(actionContext, new List<IFilterMetadata>(), result, new object());
        options.AllowRawResponses = false;

        var action = () => resultFilter.OnResultExecuting(context);

        action.Should().Throw<JsonRpcServerException>();
    }

    [Test]
    public void OnResultExecuting_RawResponseForBatchRequest_Throw()
    {
        var httpContext = new DefaultHttpContext();
        var jsonRpcFeature = new JsonRpcFeature
        {
            Call = new UntypedRequest(new NullRpcId(), "method", null),
            IsBatch = true
        };
        httpContext.Features.Set<IJsonRpcFeature>(jsonRpcFeature);
        var actionContext = new ActionContext(httpContext, new RouteData(), new ActionDescriptor(), new ModelStateDictionary());
        var result = Mock.Of<IActionResult>();
        var context = new ResultExecutingContext(actionContext, new List<IFilterMetadata>(), result, new object());
        options.AllowRawResponses = true;

        var action = () => resultFilter.OnResultExecuting(context);

        action.Should().Throw<JsonRpcServerException>();
    }

    [Test]
    public void OnResultExecuting_RawResponse_DoNothing()
    {
        var httpContext = new DefaultHttpContext();
        var jsonRpcFeature = new JsonRpcFeature { Call = new UntypedRequest(new NullRpcId(), "method", null) };
        httpContext.Features.Set<IJsonRpcFeature>(jsonRpcFeature);
        var actionContext = new ActionContext(httpContext, new RouteData(), new ActionDescriptor(), new ModelStateDictionary());
        var result = Mock.Of<IActionResult>();
        var context = new ResultExecutingContext(actionContext, new List<IFilterMetadata>(), result, new object());
        options.AllowRawResponses = true;

        resultFilter.OnResultExecuting(context);

        context.Result.Should().Be(result);
        jsonRpcFeature.Response.Should().BeNull();
    }

    [Test]
    public void OnResultExecuting_ObjectResultWithError_SetErrorResponseAndStatusResult200()
    {
        var httpContext = new DefaultHttpContext();
        var id = new NumberRpcId(123);
        var jsonRpcFeature = new JsonRpcFeature { Call = new UntypedRequest(id, "method", null) };
        httpContext.Features.Set<IJsonRpcFeature>(jsonRpcFeature);
        var actionContext = new ActionContext(httpContext, new RouteData(), new ActionDescriptor(), new ModelStateDictionary());
        var errorCode = 123;
        var errorMessage = "errorMessage";
        var error = new Error<object>(errorCode, errorMessage, null);
        var result = new ObjectResult(error);
        var context = new ResultExecutingContext(actionContext, new List<IFilterMetadata>(), result, new object());

        resultFilter.OnResultExecuting(context);

        var expectedResult = new StatusCodeResult(StatusCodes.Status200OK);
        var expectedResponse = new UntypedErrorResponse(id, error.AsUntypedError(options.DefaultDataJsonSerializerOptions));
        context.Result.Should().BeEquivalentTo(expectedResult);
        jsonRpcFeature.Response.Should().BeEquivalentTo(expectedResponse);
    }

    [TestCase(StatusCodes.Status400BadRequest)]
    [TestCase(StatusCodes.Status404NotFound)]
    [TestCase(StatusCodes.Status500InternalServerError)]
    public void OnResultExecuting_ObjectResultWithErrorStatus_SetErrorResponseAndStatusResult200(int statusCode)
    {
        var httpContext = new DefaultHttpContext();
        var id = new NumberRpcId(123);
        var jsonRpcFeature = new JsonRpcFeature { Call = new UntypedRequest(id, "method", null) };
        httpContext.Features.Set<IJsonRpcFeature>(jsonRpcFeature);
        var actionContext = new ActionContext(httpContext, new RouteData(), new ActionDescriptor(), new ModelStateDictionary());
        var errorData = 123;
        var result = new ObjectResult(errorData) { StatusCode = statusCode };
        var error = Mock.Of<IError>();
        errorFactoryMock.Setup(f => f.HttpError(statusCode, errorData))
            .Returns(error)
            .Verifiable();
        var context = new ResultExecutingContext(actionContext, new List<IFilterMetadata>(), result, new object());

        resultFilter.OnResultExecuting(context);

        var expectedResult = new StatusCodeResult(StatusCodes.Status200OK);
        var expectedResponse = new UntypedErrorResponse(id, error.AsUntypedError(options.DefaultDataJsonSerializerOptions));
        context.Result.Should().BeEquivalentTo(expectedResult);
        jsonRpcFeature.Response.Should().BeEquivalentTo(expectedResponse);
        errorFactoryMock.Verify();
    }

    [Test]
    public void OnResultExecuting_ObjectResultWithNull_SetResponseAndStatusResult200()
    {
        var httpContext = new DefaultHttpContext();
        var id = new NumberRpcId(123);
        var jsonRpcFeature = new JsonRpcFeature { Call = new UntypedRequest(id, "method", null) };
        httpContext.Features.Set<IJsonRpcFeature>(jsonRpcFeature);
        var actionContext = new ActionContext(httpContext, new RouteData(), new ActionDescriptor(), new ModelStateDictionary());
        var result = new ObjectResult(null);
        var context = new ResultExecutingContext(actionContext, new List<IFilterMetadata>(), result, new object());

        resultFilter.OnResultExecuting(context);

        var expectedResult = new StatusCodeResult(StatusCodes.Status200OK);
        var expectedResponse = new UntypedResponse(id, null);
        context.Result.Should().BeEquivalentTo(expectedResult);
        jsonRpcFeature.Response.Should().BeEquivalentTo(expectedResponse);
    }

    [Test]
    public void OnResultExecuting_ObjectResult_SetResponseAndStatusResult200()
    {
        var httpContext = new DefaultHttpContext();
        var id = new NumberRpcId(123);
        var jsonRpcFeature = new JsonRpcFeature { Call = new UntypedRequest(id, "method", null) };
        httpContext.Features.Set<IJsonRpcFeature>(jsonRpcFeature);
        var actionContext = new ActionContext(httpContext, new RouteData(), new ActionDescriptor(), new ModelStateDictionary());
        var responseData = new { A = 123, B = "b" };
        var result = new ObjectResult(responseData);
        var context = new ResultExecutingContext(actionContext, new List<IFilterMetadata>(), result, new object());

        resultFilter.OnResultExecuting(context);

        var expectedResult = new StatusCodeResult(StatusCodes.Status200OK);
        var expectedResponse = new UntypedResponse(id, JsonSerializer.SerializeToDocument(responseData, options.DefaultDataJsonSerializerOptions));
        context.Result.Should().BeEquivalentTo(expectedResult);
        jsonRpcFeature.Response.Should().BeEquivalentTo(expectedResponse, static options => options.Using(JsonElementEqualityComparer.Instance));
    }

    [TestCase(StatusCodes.Status400BadRequest)]
    [TestCase(StatusCodes.Status404NotFound)]
    [TestCase(StatusCodes.Status500InternalServerError)]
    public void OnResultExecuting_ErrorStatusResult_SetErrorResponseAndStatusResult200(int statusCode)
    {
        var httpContext = new DefaultHttpContext();
        var id = new NumberRpcId(123);
        var jsonRpcFeature = new JsonRpcFeature { Call = new UntypedRequest(id, "method", null) };
        httpContext.Features.Set<IJsonRpcFeature>(jsonRpcFeature);
        var actionContext = new ActionContext(httpContext, new RouteData(), new ActionDescriptor(), new ModelStateDictionary());
        var result = new StatusCodeResult(statusCode);
        var error = Mock.Of<IError>();
        errorFactoryMock.Setup(f => f.HttpError(statusCode, null))
            .Returns(error)
            .Verifiable();
        var context = new ResultExecutingContext(actionContext, new List<IFilterMetadata>(), result, new object());

        resultFilter.OnResultExecuting(context);

        var expectedResult = new StatusCodeResult(StatusCodes.Status200OK);
        var expectedResponse = new UntypedErrorResponse(id, error.AsUntypedError(options.DefaultDataJsonSerializerOptions));
        context.Result.Should().BeEquivalentTo(expectedResult);
        jsonRpcFeature.Response.Should().BeEquivalentTo(expectedResponse);
        errorFactoryMock.Verify();
    }

    [TestCase(StatusCodes.Status200OK)]
    [TestCase(StatusCodes.Status201Created)]
    [TestCase(StatusCodes.Status204NoContent)]
    public void OnResultExecuting_SuccessStatusResult_SetNullResponseAndStatusResult200(int statusCode)
    {
        var httpContext = new DefaultHttpContext();
        var id = new NumberRpcId(123);
        var jsonRpcFeature = new JsonRpcFeature { Call = new UntypedRequest(id, "method", null) };
        httpContext.Features.Set<IJsonRpcFeature>(jsonRpcFeature);
        var actionContext = new ActionContext(httpContext, new RouteData(), new ActionDescriptor(), new ModelStateDictionary());
        var result = new StatusCodeResult(statusCode);
        var context = new ResultExecutingContext(actionContext, new List<IFilterMetadata>(), result, new object());

        resultFilter.OnResultExecuting(context);

        var expectedResult = new StatusCodeResult(StatusCodes.Status200OK);
        var expectedResponse = new UntypedResponse(id, null);
        context.Result.Should().BeEquivalentTo(expectedResult);
        jsonRpcFeature.Response.Should().BeEquivalentTo(expectedResponse);
    }

    [TestCase(StatusCodes.Status100Continue)]
    [TestCase(StatusCodes.Status300MultipleChoices)]
    public void OnResultExecuting_UnknownStatusResponseAndRawResponsesForbiddenByOptions_Throw(int statusCode)
    {
        var httpContext = new DefaultHttpContext();
        var jsonRpcFeature = new JsonRpcFeature { Call = new UntypedRequest(new NullRpcId(), "method", null) };
        httpContext.Features.Set<IJsonRpcFeature>(jsonRpcFeature);
        var actionContext = new ActionContext(httpContext, new RouteData(), new ActionDescriptor(), new ModelStateDictionary());
        var result = new StatusCodeResult(statusCode);
        var context = new ResultExecutingContext(actionContext, new List<IFilterMetadata>(), result, new object());
        options.AllowRawResponses = false;

        var action = () => resultFilter.OnResultExecuting(context);

        action.Should().Throw<JsonRpcServerException>();
    }

    [Test]
    public void OnResultExecuting_EmptyResult_SetResponseAndStatusResult200()
    {
        var httpContext = new DefaultHttpContext();
        var id = new NumberRpcId(123);
        var jsonRpcFeature = new JsonRpcFeature { Call = new UntypedRequest(id, "method", null) };
        httpContext.Features.Set<IJsonRpcFeature>(jsonRpcFeature);
        var actionContext = new ActionContext(httpContext, new RouteData(), new ActionDescriptor(), new ModelStateDictionary());
        var result = new EmptyResult();
        var context = new ResultExecutingContext(actionContext, new List<IFilterMetadata>(), result, new object());

        resultFilter.OnResultExecuting(context);

        var expectedResult = new StatusCodeResult(StatusCodes.Status200OK);
        var expectedResponse = new UntypedResponse(id, null);
        context.Result.Should().BeEquivalentTo(expectedResult);
        jsonRpcFeature.Response.Should().BeEquivalentTo(expectedResponse);
    }
}
