using System;
using System.Collections.Generic;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Routing;
using Moq;
using NUnit.Framework;
using Tochka.JsonRpc.Common.Features;
using Tochka.JsonRpc.Common.Models.Request;
using Tochka.JsonRpc.Common.Models.Response.Errors;
using Tochka.JsonRpc.Server.Filters;
using Tochka.JsonRpc.Server.Services;

namespace Tochka.JsonRpc.Server.Tests.Filters;

[TestFixture]
public class JsonRpcExceptionWrappingFilterTests
{
    private Mock<IJsonRpcErrorFactory> errorFactoryMock;
    private JsonRpcExceptionWrappingFilter exceptionWrappingFilter;

    [SetUp]
    public void Setup()
    {
        errorFactoryMock = new Mock<IJsonRpcErrorFactory>();

        exceptionWrappingFilter = new JsonRpcExceptionWrappingFilter(errorFactoryMock.Object);
    }

    [Test]
    public void OnException_NotJsonRpc_DoNothing()
    {
        var exception = new ArgumentException();
        var httpContext = new DefaultHttpContext();
        var actionContext = new ActionContext(httpContext, new RouteData(), new ActionDescriptor(), new ModelStateDictionary());
        var context = new ExceptionContext(actionContext, new List<IFilterMetadata>())
        {
            Exception = exception
        };

        exceptionWrappingFilter.OnException(context);

        context.Result.Should().BeNull();
    }

    [Test]
    public void OnException_JsonRpcCall_SetErrorResult()
    {
        var exception = new ArgumentException();
        var httpContext = new DefaultHttpContext();
        httpContext.Features.Set<IJsonRpcFeature>(new JsonRpcFeature { Call = Mock.Of<ICall>() });
        var actionContext = new ActionContext(httpContext, new RouteData(), new ActionDescriptor(), new ModelStateDictionary());
        var context = new ExceptionContext(actionContext, new List<IFilterMetadata>())
        {
            Exception = exception
        };
        var error = Mock.Of<IError>();
        errorFactoryMock.Setup(f => f.Exception(exception))
            .Returns(error)
            .Verifiable();

        exceptionWrappingFilter.OnException(context);

        var expected = new ObjectResult(error);
        context.Result.Should().BeEquivalentTo(expected);
        errorFactoryMock.Verify();
    }
}
