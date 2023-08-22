using System.Collections.Generic;
using System.Reflection;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Moq;
using NUnit.Framework;
using Tochka.JsonRpc.Server.Attributes;
using Tochka.JsonRpc.Server.Routing;

namespace Tochka.JsonRpc.Server.Tests.Routing;

[TestFixture]
internal class JsonRpcControllerSpecificationTests
{
    private JsonRpcControllerSpecification specification;

    [SetUp]
    public void Setup() => specification = new JsonRpcControllerSpecification();

    [Test]
    public void IsSatisfiedBy_ControllerWithJsonRpcControllerAttribute_ReturnTrue()
    {
        var attributes = new List<object>
        {
            new JsonRpcControllerAttribute()
        };
        var controllerModel = new ControllerModel(Mock.Of<TypeInfo>(), attributes);

        var result = specification.IsSatisfiedBy(controllerModel);

        result.Should().BeTrue();
    }

    [Test]
    public void IsSatisfiedBy_NoJsonRpcControllerAttribute_ReturnFalse()
    {
        var controllerModel = new ControllerModel(Mock.Of<TypeInfo>(), new List<object>());

        var result = specification.IsSatisfiedBy(controllerModel);

        result.Should().BeFalse();
    }
}
