using System;
using System.Collections.Generic;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using Tochka.JsonRpc.Common.Features;
using Tochka.JsonRpc.Common.Models.Request;
using Tochka.JsonRpc.Common.Models.Response.Errors;
using Tochka.JsonRpc.Server.Filters;
using Tochka.JsonRpc.Server.Services;
using Tochka.JsonRpc.Server.Settings;

namespace Tochka.JsonRpc.Server.Tests.Filters;

[TestFixture]
internal class JsonRpcExceptionFilterTests
{
    private Mock<IJsonRpcErrorFactory> errorFactoryMock;
    private JsonRpcServerOptions options;
    private Mock<ILogger<JsonRpcExceptionFilter>> logMock;
    private JsonRpcExceptionFilter exceptionFilter;

    [SetUp]
    public void Setup()
    {
        errorFactoryMock = new Mock<IJsonRpcErrorFactory>();
        logMock = new Mock<ILogger<JsonRpcExceptionFilter>>();
        options = new JsonRpcServerOptions();

        exceptionFilter = new JsonRpcExceptionFilter(errorFactoryMock.Object, Options.Create(options), logMock.Object);
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

        exceptionFilter.OnException(context);

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

        exceptionFilter.OnException(context);

        var expected = new ObjectResult(error);
        context.Result.Should().BeEquivalentTo(expected);
        errorFactoryMock.Verify();
    }

    [Test]
    public void OnException_LogExceptionsTrue_LogError()
    {
        var exception = new ArgumentException();
        var httpContext = new DefaultHttpContext();
        httpContext.Features.Set<IJsonRpcFeature>(new JsonRpcFeature { Call = Mock.Of<ICall>() });
        var actionContext = new ActionContext(httpContext, new RouteData(), new ActionDescriptor(), new ModelStateDictionary());
        var context = new ExceptionContext(actionContext, new List<IFilterMetadata>())
        {
            Exception = exception
        };
        options.LogExceptions = true;
        logMock.Setup(l => l.Log(LogLevel.Error, It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), exception, It.IsAny<Func<It.IsAnyType, Exception?, string>>()))
            .Verifiable();

        exceptionFilter.OnException(context);

        logMock.Verify();
    }

    [Test]
    public void OnException_LogExceptionsFalse_DontLogAnything()
    {
        var exception = new ArgumentException();
        var httpContext = new DefaultHttpContext();
        httpContext.Features.Set<IJsonRpcFeature>(new JsonRpcFeature { Call = Mock.Of<ICall>() });
        var actionContext = new ActionContext(httpContext, new RouteData(), new ActionDescriptor(), new ModelStateDictionary());
        var context = new ExceptionContext(actionContext, new List<IFilterMetadata>())
        {
            Exception = exception
        };
        options.LogExceptions = false;

        exceptionFilter.OnException(context);

        logMock.VerifyNoOtherCalls();
    }
}
