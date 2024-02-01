using System;
using System.Collections.Generic;
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
using Tochka.JsonRpc.Server.Filters;
using Tochka.JsonRpc.Server.Settings;

namespace Tochka.JsonRpc.Server.Tests.Filters;

[TestFixture]
public class JsonRpcExceptionLoggingFilterTests
{
    private JsonRpcServerOptions options;
    private Mock<ILogger<JsonRpcExceptionLoggingFilter>> logMock;
    private JsonRpcExceptionLoggingFilter exceptionLoggingFilter;

    [SetUp]
    public void Setup()
    {
        options = new JsonRpcServerOptions();
        logMock = new Mock<ILogger<JsonRpcExceptionLoggingFilter>>();

        exceptionLoggingFilter = new JsonRpcExceptionLoggingFilter(Options.Create(options), logMock.Object);
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

        exceptionLoggingFilter.OnException(context);

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

        exceptionLoggingFilter.OnException(context);

        logMock.VerifyNoOtherCalls();
    }
}
