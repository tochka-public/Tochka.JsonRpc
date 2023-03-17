using System.Collections.Generic;
using System.Reflection;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;
using Tochka.JsonRpc.V1.Server.Conventions;
using Tochka.JsonRpc.V1.Server.Pipeline;
using Tochka.JsonRpc.V1.Server.Tests.Helpers;

namespace Tochka.JsonRpc.V1.Server.Tests.Conventions
{
    public class ControllerConventionTests
    {
        private TestEnvironment testEnvironment;
        private ControllerConvention controllerConvention;
        private IFilterMetadata someFilter;

        [SetUp]
        public void Setup()
        {
            testEnvironment = new TestEnvironment(services => { services.AddSingleton<ControllerConvention>(); });
            controllerConvention = testEnvironment.ServiceProvider.GetRequiredService<ControllerConvention>();
            someFilter = new AuthorizeFilter();
        }

        [Test]
        public void Test_Apply_IgnoresMvcControllers()
        {
            var model = new Mock<ControllerModel>(typeof(MvcTestController).GetTypeInfo(), new List<object>()).Object;
            model.Selectors.Add(new SelectorModel());
            model.Filters.Add(someFilter);

            controllerConvention.Apply(model);

            model.Selectors.Should().HaveCount(1);
            model.Filters.Should().HaveCount(1);
            model.Filters[0].Should().Be(someFilter);
        }

        [Test]
        public void Test_Apply_InsertsFirstFilter()
        {
            var model = new Mock<ControllerModel>(typeof(JsonRpcTestController).GetTypeInfo(), new List<object>()).Object;
            model.Filters.Add(someFilter);

            controllerConvention.Apply(model);

            model.Filters[0]
                .Should().BeOfType<ServiceFilterAttribute>()
                .Which.ServiceType
                .Should().Be<JsonRpcFilter>();
            model.Filters.Should().HaveElementAt(1, someFilter);
        }

        [Test]
        public void Test_Apply_ClearsSelectors()
        {
            var model = new Mock<ControllerModel>(typeof(JsonRpcTestController).GetTypeInfo(), new List<object>()).Object;
            model.Filters.Add(someFilter);

            controllerConvention.Apply(model);

            model.Selectors.Should().BeEmpty();
        }
    }
}