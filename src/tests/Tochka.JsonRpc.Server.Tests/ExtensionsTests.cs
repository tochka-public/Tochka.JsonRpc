using System;
using System.Buffers;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Moq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using NUnit.Framework;
using Tochka.JsonRpc.Common;
using Tochka.JsonRpc.Common.Models.Request;
using Tochka.JsonRpc.Common.Models.Request.Untyped;
using Tochka.JsonRpc.Common.Models.Response;
using Tochka.JsonRpc.Common.Models.Response.Errors;
using Tochka.JsonRpc.Common.Serializers;
using Tochka.JsonRpc.Server.Binding;
using Tochka.JsonRpc.Server.Conventions;
using Tochka.JsonRpc.Server.Exceptions;
using Tochka.JsonRpc.Server.Pipeline;
using Tochka.JsonRpc.Server.Services;
using Tochka.JsonRpc.Server.Settings;

namespace Tochka.JsonRpc.Server.Tests
{
    public class ExtensionsTests
    {
        [TestCase("testtest", "testtest")]
        [TestCase("testTest", "testTest")]
        [TestCase("TestTest", "testTest")]
        [TestCase("test_test", "test_test")]
        [TestCase("test_Test", "test_Test")]
        [TestCase("Test_test", "test_test")]
        [TestCase("Test_Test", "test_Test")]
        public void Test_GetJsonName_CamelCase(string original, string json)
        {
            var serializer = new CamelCaseJsonRpcSerializer();

            var result = serializer.GetJsonName(original);

            result.Original.Should().Be(original);
            result.Json.Should().Be(json);
        }

        [TestCase("testtest", "testtest")]
        [TestCase("testTest", "test_test")]
        [TestCase("TestTest", "test_test")]
        [TestCase("test_test", "test_test")]
        [TestCase("test_Test", "test_test")]
        [TestCase("Test_test", "test_test")]
        [TestCase("Test_Test", "test_test")]
        public void Test_GetJsonName_SnakeCase(string original, string json)
        {
            var serializer = new SnakeCaseJsonRpcSerializer();

            var result = serializer.GetJsonName(original);

            result.Original.Should().Be(original);
            result.Json.Should().Be(json);
        }

        [Test]
        public void Test_GetJsonName_ThrowsOnWrongResolver()
        {
            var serializerMock = new Mock<IJsonRpcSerializer>();
            serializerMock.Setup(x => x.Serializer)
                .Returns(new JsonSerializer()
                {
                    ContractResolver = Mock.Of<IContractResolver>()
                });
            Action action = () => serializerMock.Object.GetJsonName(string.Empty);

            action.Should().Throw<ArgumentException>();
        }


        [Test]
        public void Test_AddJsonRpcServer_RegistersServices()
        {
            var services = new ServiceCollection();
            var mvcBuilder = new Mock<IMvcBuilder>();
            mvcBuilder.SetupGet(x => x.Services).Returns(services);

            mvcBuilder.Object.AddJsonRpcServer();

            mvcBuilder.VerifyGet(x => x.Services);
            mvcBuilder.VerifyNoOtherCalls();
            var result = services.Select(x => (x.ServiceType, x.Lifetime)).ToList();
            result.Remove((typeof(IConfigureOptions<JsonRpcOptions>), ServiceLifetime.Singleton)).Should().BeTrue();
            result.Remove((typeof(ControllerConvention), ServiceLifetime.Singleton)).Should().BeTrue();
            result.Remove((typeof(ActionConvention), ServiceLifetime.Singleton)).Should().BeTrue();
            result.Remove((typeof(ParameterConvention), ServiceLifetime.Singleton)).Should().BeTrue();
            result.Remove((typeof(JsonRpcFormatter), ServiceLifetime.Singleton)).Should().BeTrue();
            result.Remove((typeof(JsonRpcModelBinder), ServiceLifetime.Singleton)).Should().BeTrue();
            result.Remove((typeof(JsonRpcFilter), ServiceLifetime.Scoped)).Should().BeTrue();
            result.Remove((typeof(IParameterBinder), ServiceLifetime.Singleton)).Should().BeTrue();
            result.Remove((typeof(IRequestHandler), ServiceLifetime.Singleton)).Should().BeTrue();
            result.Remove((typeof(IRequestReader), ServiceLifetime.Singleton)).Should().BeTrue();
            result.Remove((typeof(IResponseReader), ServiceLifetime.Singleton)).Should().BeTrue();
            result.Remove((typeof(IJsonRpcErrorFactory), ServiceLifetime.Singleton)).Should().BeTrue();
            result.Remove((typeof(IMethodMatcher), ServiceLifetime.Singleton)).Should().BeTrue();
            result.Remove((typeof(IParamsParser), ServiceLifetime.Singleton)).Should().BeTrue();
            result.Remove((typeof(IActionResultConverter), ServiceLifetime.Scoped)).Should().BeTrue();
            result.Remove((typeof(INestedContextFactory), ServiceLifetime.Transient)).Should().BeTrue();
            result.Remove((typeof(IJsonRpcSerializer), ServiceLifetime.Singleton)).Should().BeTrue("first serializer");
            result.Remove((typeof(IJsonRpcSerializer), ServiceLifetime.Singleton)).Should().BeTrue("second serializer");
            result.Remove((typeof(HeaderJsonRpcSerializer), ServiceLifetime.Singleton)).Should().BeTrue();
            result.Remove((typeof(SnakeCaseJsonRpcSerializer), ServiceLifetime.Singleton)).Should().BeTrue();
            result.Remove((typeof(IJsonRpcRoutes), ServiceLifetime.Singleton)).Should().BeTrue();

            // remove noise
            result.Remove((typeof(IOptions<>), ServiceLifetime.Singleton));
            result.Remove((typeof(IOptionsSnapshot<>), ServiceLifetime.Scoped));
            result.Remove((typeof(IOptionsMonitor<>), ServiceLifetime.Singleton));
            result.Remove((typeof(IOptionsFactory<>), ServiceLifetime.Transient));
            result.Remove((typeof(IOptionsMonitorCache<>), ServiceLifetime.Singleton));
            result.Remove((typeof(IConfigureOptions<MvcOptions>), ServiceLifetime.Singleton));
            result.Remove((typeof(IConfigureOptions<MvcOptions>), ServiceLifetime.Singleton));
            result.Remove((typeof(IConfigureOptions<MvcOptions>), ServiceLifetime.Singleton));

            result.Should().BeEmpty("no other services are expected");
        }

        [Test]
        public void Test_AddJsonRpcServer_ThrowsOnNull()
        {
            var builder = Mock.Of<IMvcBuilder>();
            Action action = () => builder.AddJsonRpcServer();

            action.Should().Throw<ArgumentNullException>();
        }

        [Test]
        public void Test_AddJsonRpcServer_ConfiguresOptions()
        {
            var services = new ServiceCollection();
            var mvcBuilder = new Mock<IMvcBuilder>();
            mvcBuilder.SetupGet(x => x.Services).Returns(services);
            var action = new Mock<Action<JsonRpcOptions>>();

            mvcBuilder.Object.AddJsonRpcServer(action.Object);
            var result = services.BuildServiceProvider().GetRequiredService<IOptions<JsonRpcOptions>>().Value;

            action.Verify(x => x(It.IsAny<JsonRpcOptions>()));
        }

        [Test]
        public void Test_AddJsonRpcServer_ConfiguresOptionsDefault()
        {
            var services = new ServiceCollection();
            var mvcBuilder = new Mock<IMvcBuilder>();
            mvcBuilder.SetupGet(x => x.Services).Returns(services);

            mvcBuilder.Object.AddJsonRpcServer(null);
            var result = services.BuildServiceProvider().GetRequiredService<IOptions<JsonRpcOptions>>().Value;

            result.Should().NotBeNull();
        }

        [Test]
        public void Test_AddJsonRpcServer_PreservesUserServices()
        {
            var services = new ServiceCollection();
            var mvcBuilder = new Mock<IMvcBuilder>();
            mvcBuilder.SetupGet(x => x.Services).Returns(services);

            var optionsMock = new Mock<IOptions<JsonRpcOptions>>();
            optionsMock.SetupGet(x => x.Value).Returns(new JsonRpcOptions());
            var mocks = new List<Mock>
            {
                RegisterMock<ControllerConvention>(services, null),
                RegisterMock<ActionConvention>(services, optionsMock.Object, null, null, null, null),
                RegisterMock<ParameterConvention>(services, null, null),
                RegisterMock<IStartupFilter>(services),
                RegisterMock<JsonRpcFormatter>(services, new HeaderJsonRpcSerializer(), Mock.Of<ArrayPool<char>>()),
                RegisterMock<JsonRpcModelBinder>(services),
                RegisterMock<JsonRpcFilter>(services, null, null),
                RegisterMock<IParameterBinder>(services),
                RegisterMock<IRequestHandler>(services),
                RegisterMock<IRequestReader>(services),
                RegisterMock<IResponseReader>(services),
                RegisterMock<IJsonRpcErrorFactory>(services),
                RegisterMock<IMethodMatcher>(services),
                RegisterMock<IParamsParser>(services),
                RegisterMock<IActionResultConverter>(services),
                RegisterMock<INestedContextFactory>(services),
                RegisterMock<IJsonRpcRoutes>(services),
            };

            mvcBuilder.Object.AddJsonRpcServer();

            mvcBuilder.VerifyGet(x => x.Services);
            mvcBuilder.VerifyNoOtherCalls();
            var result = services.Select(x => (x.ImplementationInstance, x.Lifetime)).ToList();
            foreach (var mock in mocks)
            {
                result.Should().Contain((mock.Object, ServiceLifetime.Singleton));
            }
        }

        [Test]
        public void Test_TryAddJsonRpcSerializer_RegistersServices()
        {
            var services = new ServiceCollection();

            services.TryAddJsonRpcSerializer<HeaderJsonRpcSerializer>();

            var result = services.Select(x => (x.ServiceType, x.Lifetime)).ToList();
            result.Remove((typeof(IJsonRpcSerializer), ServiceLifetime.Singleton)).Should().BeTrue();
            result.Remove((typeof(HeaderJsonRpcSerializer), ServiceLifetime.Singleton)).Should().BeTrue();
            result.Should().BeEmpty("no other services are expected");
        }

        [Test]
        public void Test_TryAddJsonRpcSerializer_DoesNotRegisterDuplicates()
        {
            var services = new ServiceCollection();

            services.TryAddJsonRpcSerializer<HeaderJsonRpcSerializer>();
            services.TryAddJsonRpcSerializer<HeaderJsonRpcSerializer>();
            services.TryAddJsonRpcSerializer<SnakeCaseJsonRpcSerializer>();
            services.TryAddJsonRpcSerializer<SnakeCaseJsonRpcSerializer>();

            var result = services.Select(x => (x.ServiceType, x.Lifetime)).ToList();
            result.Remove((typeof(IJsonRpcSerializer), ServiceLifetime.Singleton)).Should().BeTrue();
            result.Remove((typeof(IJsonRpcSerializer), ServiceLifetime.Singleton)).Should().BeTrue();
            result.Remove((typeof(HeaderJsonRpcSerializer), ServiceLifetime.Singleton)).Should().BeTrue();
            result.Remove((typeof(SnakeCaseJsonRpcSerializer), ServiceLifetime.Singleton)).Should().BeTrue();
            result.Should().BeEmpty("no other services are expected");
        }

        [Test]
        public void Test_GetJsonRpcCall_ReturnsValue()
        {
            var context = Mock.Of<HttpContext>();
            context.Items = new Dictionary<object, object>();
            var data = Mock.Of<IUntypedCall>();
            context.Items[JsonRpcConstants.RequestItemKey] = data;

            var result = context.GetJsonRpcCall();

            result.Should().Be(Mock.Get(result).Object);
        }

        [Test]
        public void Test_GetJsonRpcCall_ThrowsOnBadData()
        {
            var context = Mock.Of<HttpContext>();
            context.Items = new Dictionary<object, object>();
            var data = new object();
            context.Items[JsonRpcConstants.RequestItemKey] = data;

            Action action = () => context.GetJsonRpcCall();

            action.Should().Throw<InvalidCastException>();
        }

        [Test]
        public void Test_GetJsonRpcCall_ThrowsOnNoData()
        {
            var context = Mock.Of<HttpContext>();
            context.Items = new Dictionary<object, object>();

            Action action = () => context.GetJsonRpcCall();

            action.Should().Throw<KeyNotFoundException>();
        }

        [Test]
        public void Test_GetJsonRpcCall_ThrowsOnNoItems()
        {
            var context = Mock.Of<HttpContext>();
            context.Items = null;

            Action action = () => context.GetJsonRpcCall();

            action.Should().Throw<NullReferenceException>();
        }

        [Test]
        public void Test_GetJsonRpcCall_ThrowsOnNoContext()
        {
            HttpContext context = null;

            Action action = () => context.GetJsonRpcCall();

            action.Should().Throw<NullReferenceException>();
        }

        [Test]
        public void Test_TryAddTryAddConvention_RegistersServices()
        {
            var services = new ServiceCollection();

            services.TryAddConvention<ControllerConvention>();

            var result = services.Select(x => (x.ImplementationType, x.Lifetime)).ToList();
            result.Remove((typeof(ControllerConvention), ServiceLifetime.Singleton)).Should().BeTrue();
            result.Remove((typeof(ConventionConfigurator<ControllerConvention>), ServiceLifetime.Singleton)).Should().BeTrue();
            result.Should().BeEmpty("no other services are expected");
        }

        [Test]
        public void Test_TryAddTryAddConvention_DoesNotRegisterDuplicates()
        {
            var services = new ServiceCollection();

            services.TryAddConvention<ControllerConvention>();
            services.TryAddConvention<ControllerConvention>();

            var result = services.Select(x => (x.ImplementationType, x.Lifetime)).ToList();
            result.Remove((typeof(ControllerConvention), ServiceLifetime.Singleton)).Should().BeTrue();
            result.Remove((typeof(ConventionConfigurator<ControllerConvention>), ServiceLifetime.Singleton)).Should().BeTrue();
            result.Should().BeEmpty("no other services are expected");
        }

        [Test]
        public void Test_ConvertErrorToResponse_ReturnsJsonrpcError()
        {
            var errorFactory = Mock.Of<IJsonRpcErrorFactory>();
            var error = new Error<object>
            {
                Code = 0,
            };

            var result = errorFactory.ConvertErrorToResponse(error, new HeaderJsonRpcSerializer());
            result["error"].Should().NotBeNull();
            result["error"]["code"].Should().NotBeNull();
            result["error"]["message"].Should().NotBeNull();
            result["error"]["data"].Should().NotBeNull();
        }

        [Test]
        public void Test_ConvertExceptionToResponse_ConvertsGeneralExceptions()
        {
            var errorFactoryMock = new Mock<IJsonRpcErrorFactory>();
            errorFactoryMock
                .Setup(x => x.Exception(It.IsAny<Exception>()))
                .Returns(new Error<object>
                {
                    Code = 0,
                    Message = string.Empty,
                    Data = string.Empty
                });
            var exception = new Exception();

            var result = errorFactoryMock.Object.ConvertExceptionToResponse(exception, new HeaderJsonRpcSerializer());

            errorFactoryMock.Verify(x => x.Exception(It.IsAny<Exception>()), Times.Once);
            errorFactoryMock.VerifyNoOtherCalls();
            result["error"].Should().NotBeNull();
            result["error"]["code"].Should().NotBeNull();
            result["error"]["message"].Should().NotBeNull();
            result["error"]["data"].Should().NotBeNull();
        }

        [Test]
        public void Test_ConvertExceptionToResponse_ConvertsErrorResponseExceptions()
        {
            var errorFactoryMock = new Mock<IJsonRpcErrorFactory>();
            var errorData = new object();
            var exception = new JsonRpcErrorResponseException(new Error<object>
            {
                Code = 42,
                Data = errorData,
                Message = "test"
            });

            var result = errorFactoryMock.Object.ConvertExceptionToResponse(exception, new HeaderJsonRpcSerializer());

            errorFactoryMock.VerifyNoOtherCalls();
            result["error"].Should().NotBeNull();
            result["error"]["code"].Value<int>().Should().Be(42);
            result["error"]["message"].Value<string>().Should().Be("test");
            result["error"]["data"].Should().NotBeNull();
        }

        [Test]
        public void Test_ThrowAsResponseException_Throws()
        {
            var error = Mock.Of<IError>();
            Action action = () => error.ThrowAsResponseException();

            action.Should().ThrowExactly<JsonRpcErrorResponseException>().Which.Error.Should().BeSameAs(error);
        }

        private static Mock<T> RegisterMock<T>(IServiceCollection services, params object[] args) where T : class
        {
            var mock = new Mock<T>(args);
            services.AddSingleton(mock.Object);
            return mock;
        }
    }
}