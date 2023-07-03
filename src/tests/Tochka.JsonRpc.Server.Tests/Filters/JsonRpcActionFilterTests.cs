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
using Tochka.JsonRpc.Common.Models.Request;
using Tochka.JsonRpc.Server.Features;
using Tochka.JsonRpc.Server.Filters;

namespace Tochka.JsonRpc.Server.Tests.Filters;

[TestFixture]
internal class JsonRpcActionFilterTests
{
    private JsonRpcActionFilter actionFilter;

    [SetUp]
    public void Setup() => actionFilter = new JsonRpcActionFilter();

    [Test]
    public void OnActionExecuting_NotJsonRpc_DoNothing()
    {
        var httpContext = new DefaultHttpContext();
        var actionContext = new ActionContext(httpContext, new RouteData(), new ActionDescriptor(), new ModelStateDictionary());
        var context = new ActionExecutingContext(actionContext, new List<IFilterMetadata>(), new Dictionary<string, object?>(), new object());
        context.ModelState.TryAddModelError("key", "errorMessage");

        actionFilter.OnActionExecuting(context);

        context.Result.Should().BeNull();
    }

    [Test]
    public void OnActionExecuting_ResultAlreadySet_DoNothing()
    {
        var httpContext = new DefaultHttpContext();
        httpContext.Features.Set<IJsonRpcFeature>(new JsonRpcFeature { Call = Mock.Of<ICall>() });
        var actionContext = new ActionContext(httpContext, new RouteData(), new ActionDescriptor(), new ModelStateDictionary());
        var context = new ActionExecutingContext(actionContext, new List<IFilterMetadata>(), new Dictionary<string, object?>(), new object());
        var result = new OkResult();
        context.ModelState.TryAddModelError("key", "errorMessage");
        context.Result = result;

        actionFilter.OnActionExecuting(context);

        context.Result.Should().Be(result);
    }

    [Test]
    public void OnActionExecuting_ModelStateIsValid_DoNothing()
    {
        var httpContext = new DefaultHttpContext();
        httpContext.Features.Set<IJsonRpcFeature>(new JsonRpcFeature { Call = Mock.Of<ICall>() });
        var actionContext = new ActionContext(httpContext, new RouteData(), new ActionDescriptor(), new ModelStateDictionary());
        var context = new ActionExecutingContext(actionContext, new List<IFilterMetadata>(), new Dictionary<string, object?>(), new object());

        actionFilter.OnActionExecuting(context);

        context.Result.Should().BeNull();
    }

    [Test]
    public void OnActionExecuting_ResultIsNullAndModelStateIsInvalid_SetBadRequestObjectResult()
    {
        var httpContext = new DefaultHttpContext();
        httpContext.Features.Set<IJsonRpcFeature>(new JsonRpcFeature { Call = Mock.Of<ICall>() });
        var actionContext = new ActionContext(httpContext, new RouteData(), new ActionDescriptor(), new ModelStateDictionary());
        var context = new ActionExecutingContext(actionContext, new List<IFilterMetadata>(), new Dictionary<string, object?>(), new object());
        context.ModelState.TryAddModelError("key", "errorMessage");

        actionFilter.OnActionExecuting(context);

        var expected = new BadRequestObjectResult(context.ModelState);
        context.Result.Should().BeEquivalentTo(expected);
    }
}
