using System;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using Tochka.JsonRpc.Common.Serializers;
using Tochka.JsonRpc.Server.Models;
using Tochka.JsonRpc.Server.Models.Binding;
using Tochka.JsonRpc.Server.Settings;
using Tochka.JsonRpc.Server.Tests.Helpers;
using ParameterBinder = Tochka.JsonRpc.Server.Binding.ParameterBinder;

namespace Tochka.JsonRpc.Server.Tests.Binding
{
    public class ParameterBinderTests
    {
        private TestEnvironment testEnvironment;
        private ParameterBinder parameterBinder;

        [SetUp]
        public void Setup()
        {
            testEnvironment = new TestEnvironment(services => { services.AddSingleton<ParameterBinder>(); });

            parameterBinder = testEnvironment.ServiceProvider.GetRequiredService<ParameterBinder>();
        }

        [Test]
        public void Test_SetNoResult_DoesNothing()
        {
            var resultMock = new Mock<NoParseResult>(MockBehavior.Strict, "test");
            resultMock.Setup(x => x.ToString()).Returns("test");
            var bindingContextMock = new Mock<ModelBindingContext>(MockBehavior.Strict);

            parameterBinder.SetNoResult(bindingContextMock.Object, "test", resultMock.Object);

            bindingContextMock.Verify();
            resultMock.Verify();
        }

        [Test]
        public void Test_SetNullResult_SetsSuccessWithNull()
        {
            var resultMock = new Mock<NullParseResult>(MockBehavior.Strict, "test");
            resultMock.Setup(x => x.ToString()).Returns("test");
            var bindingContextMock = new Mock<ModelBindingContext>(MockBehavior.Strict);
            bindingContextMock.SetupSet(x => x.Result = ModelBindingResult.Success(null));

            parameterBinder.SetNullResult(bindingContextMock.Object, "test", resultMock.Object);

            bindingContextMock.Verify();
            resultMock.Verify();
        }

        [Test]
        public void Test_SetError_AddsModelError()
        {
            var resultMock = new Mock<IParseResult>(MockBehavior.Strict);
            resultMock.Setup(x => x.ToString()).Returns("test_ToString");
            var modelStateMock = new Mock<ModelStateDictionary>(MockBehavior.Strict);
            var bindingContextMock = new Mock<ModelBindingContext>(MockBehavior.Strict);
            bindingContextMock.SetupGet(x => x.ModelState).Returns(modelStateMock.Object);

            parameterBinder.SetError(bindingContextMock.Object, "test", resultMock.Object);

            bindingContextMock.Verify();
            modelStateMock.Object.ValidationState.Should().Be(ModelValidationState.Invalid);
            resultMock.Verify();
        }

        [Test]
        public void Test_SetDeserializedResult_SetsSuccess()
        {
            var jToken = JValue.CreateString("value");
            var serializer = new SnakeCaseJsonRpcSerializer();
            var modelMetadataIdentity = ModelMetadataIdentity.ForProperty(typeof(string), "null_name", typeof(object));
            var modelMetadataMock = new Mock<ModelMetadata>(MockBehavior.Strict, modelMetadataIdentity);
            var bindingContextMock = new Mock<ModelBindingContext>(MockBehavior.Strict);
            bindingContextMock.SetupProperty(x => x.Result);
            bindingContextMock.SetupGet(x => x.ModelMetadata).Returns(modelMetadataMock.Object);

            parameterBinder.SetDeserializedResult(bindingContextMock.Object, jToken, serializer.Serializer);

            bindingContextMock.Verify();
            bindingContextMock.Object.Result.Should().BeEquivalentTo(ModelBindingResult.Success("value"));
        }

        [Test]
        public void Test_SetResultSafe_SetsDeserializedResult()
        {
            var jToken = JValue.CreateString("value");
            var resultMock = new Mock<SuccessParseResult>(MockBehavior.Strict, jToken, "test");
            resultMock.Setup(x => x.ToString()).Returns("test_ToString");
            var serializer = Mock.Of<JsonSerializer>(MockBehavior.Strict);
            var bindingContextMock = new Mock<ModelBindingContext>(MockBehavior.Strict);
            var binderMock = new Mock<ParameterBinder>(testEnvironment.ServiceProvider.GetRequiredService<ILogger<ParameterBinder>>());
            binderMock.Setup(x => x.SetResultSafe(It.IsAny<ModelBindingContext>(), It.IsAny<string>(), It.IsAny<SuccessParseResult>(), It.IsAny<JsonSerializer>()))
                .CallBase();

            binderMock.Object.SetResultSafe(bindingContextMock.Object, "test", resultMock.Object, serializer);

            binderMock.Verify(x => x.SetResultSafe(It.IsAny<ModelBindingContext>(), It.IsAny<string>(), It.IsAny<SuccessParseResult>(), It.IsAny<JsonSerializer>()));
            binderMock.Verify(x => x.SetDeserializedResult(It.IsAny<ModelBindingContext>(), It.IsAny<JToken>(), It.IsAny<JsonSerializer>()));
            binderMock.VerifyNoOtherCalls();
            bindingContextMock.Verify();
        }

        [Test]
        public void Test_SetResultSafe_AddsModelErrorOnException()
        {
            var jToken = JValue.CreateString("value");
            var resultMock = new Mock<SuccessParseResult>(MockBehavior.Strict, jToken, "test");
            resultMock.Setup(x => x.ToString()).Returns("test_ToString");
            var serializer = Mock.Of<JsonSerializer>(MockBehavior.Strict);
            var modelStateMock = new Mock<ModelStateDictionary>(MockBehavior.Strict);
            var bindingContextMock = new Mock<ModelBindingContext>(MockBehavior.Strict);
            bindingContextMock.SetupGet(x => x.ModelState).Returns(modelStateMock.Object);
            var binderMock = new Mock<ParameterBinder>(testEnvironment.ServiceProvider.GetRequiredService<ILogger<ParameterBinder>>());
            binderMock.Setup(x => x.SetDeserializedResult(It.IsAny<ModelBindingContext>(), It.IsAny<JToken>(), It.IsAny<JsonSerializer>()))
                .Throws<Exception>();
            binderMock.Setup(x => x.SetResultSafe(It.IsAny<ModelBindingContext>(), It.IsAny<string>(), It.IsAny<SuccessParseResult>(), It.IsAny<JsonSerializer>()))
                .CallBase();

            binderMock.Object.SetResultSafe(bindingContextMock.Object, "test", resultMock.Object, serializer);

            binderMock.Verify(x => x.SetResultSafe(It.IsAny<ModelBindingContext>(), It.IsAny<string>(), It.IsAny<SuccessParseResult>(), It.IsAny<JsonSerializer>()));
            binderMock.Verify(x => x.SetDeserializedResult(It.IsAny<ModelBindingContext>(), It.IsAny<JToken>(), It.IsAny<JsonSerializer>()));
            binderMock.VerifyNoOtherCalls();
            bindingContextMock.Verify();
            modelStateMock.Object.ValidationState.Should().Be(ModelValidationState.Invalid);
        }

        [Test]
        public void Test_SetResult_SetsSuccess()
        {
            var jToken = JValue.CreateString("value");
            var resultMock = new Mock<SuccessParseResult>(MockBehavior.Strict, jToken, "test");
            var bindingContextMock = new Mock<ModelBindingContext>(MockBehavior.Strict);
            var serializerMock = new Mock<IJsonRpcSerializer>(MockBehavior.Strict);
            serializerMock.SetupGet(x => x.Serializer).Returns(Mock.Of<JsonSerializer>(MockBehavior.Strict));
            var rpcContext = new JsonRpcBindingContext()
            {
                Serializer = serializerMock.Object
            };
            var binderMock = new Mock<ParameterBinder>(testEnvironment.ServiceProvider.GetRequiredService<ILogger<ParameterBinder>>());
            binderMock.Setup(x => x.SetResultSafe(It.IsAny<ModelBindingContext>(), It.IsAny<string>(), It.IsAny<SuccessParseResult>(), It.IsAny<JsonSerializer>()))
                .Returns((Task) null);
            binderMock.Setup(x => x.SetResult(It.IsAny<ModelBindingContext>(), It.IsAny<IParseResult>(), It.IsAny<string>(), It.IsAny<JsonRpcBindingContext>()))
                .CallBase();

            binderMock.Object.SetResult(bindingContextMock.Object, resultMock.Object, "test", rpcContext);

            binderMock.Verify(x => x.SetResult(It.IsAny<ModelBindingContext>(), It.IsAny<IParseResult>(), It.IsAny<string>(), It.IsAny<JsonRpcBindingContext>()));
            binderMock.Verify(x => x.SetResultSafe(It.IsAny<ModelBindingContext>(), It.IsAny<string>(), It.IsAny<SuccessParseResult>(), It.IsAny<JsonSerializer>()));
            binderMock.VerifyNoOtherCalls();
            serializerMock.Verify();
            bindingContextMock.Verify();
        }

        [Test]
        public void Test_SetResult_SetsError()
        {
            var resultMock = new Mock<ErrorParseResult>(MockBehavior.Strict, "test", "test");
            var bindingContextMock = new Mock<ModelBindingContext>(MockBehavior.Strict);
            var rpcContext = new JsonRpcBindingContext();
            var binderMock = new Mock<ParameterBinder>(testEnvironment.ServiceProvider.GetRequiredService<ILogger<ParameterBinder>>());
            binderMock.Setup(x => x.SetError(It.IsAny<ModelBindingContext>(), It.IsAny<string>(), It.IsAny<IParseResult>()))
                .Returns((Task) null);
            binderMock.Setup(x => x.SetResult(It.IsAny<ModelBindingContext>(), It.IsAny<IParseResult>(), It.IsAny<string>(), It.IsAny<JsonRpcBindingContext>()))
                .CallBase();

            binderMock.Object.SetResult(bindingContextMock.Object, resultMock.Object, "test", rpcContext);

            binderMock.Verify(x => x.SetResult(It.IsAny<ModelBindingContext>(), It.IsAny<IParseResult>(), It.IsAny<string>(), It.IsAny<JsonRpcBindingContext>()));
            binderMock.Verify(x => x.SetError(It.IsAny<ModelBindingContext>(), It.IsAny<string>(), It.IsAny<IParseResult>()));
            binderMock.VerifyNoOtherCalls();
            bindingContextMock.Verify();
        }

        [Test]
        public void Test_SetResult_SetsNoResultWhenOptional()
        {
            var resultMock = new Mock<NoParseResult>(MockBehavior.Strict, "test");
            var bindingContextMock = new Mock<ModelBindingContext>(MockBehavior.Strict);
            var rpcContext = new JsonRpcBindingContext
            {
                ParameterMetadata = new ParameterMetadata(new JsonName("test", "test"), 0, BindingStyle.Default, true)
            };
            var binderMock = new Mock<ParameterBinder>(testEnvironment.ServiceProvider.GetRequiredService<ILogger<ParameterBinder>>());
            binderMock.Setup(x => x.SetNoResult(It.IsAny<ModelBindingContext>(), It.IsAny<string>(), It.IsAny<NoParseResult>()))
                .Returns((Task) null);
            binderMock.Setup(x => x.SetResult(It.IsAny<ModelBindingContext>(), It.IsAny<IParseResult>(), It.IsAny<string>(), It.IsAny<JsonRpcBindingContext>()))
                .CallBase();

            binderMock.Object.SetResult(bindingContextMock.Object, resultMock.Object, "test", rpcContext);

            binderMock.Verify(x => x.SetResult(It.IsAny<ModelBindingContext>(), It.IsAny<IParseResult>(), It.IsAny<string>(), It.IsAny<JsonRpcBindingContext>()));
            binderMock.Verify(x => x.SetNoResult(It.IsAny<ModelBindingContext>(), It.IsAny<string>(), It.IsAny<NoParseResult>()));
            binderMock.VerifyNoOtherCalls();
            bindingContextMock.Verify();
        }

        [Test]
        public void Test_SetResult_SetsErrorWhenRequired()
        {
            var resultMock = new Mock<NoParseResult>(MockBehavior.Strict, "test");
            var bindingContextMock = new Mock<ModelBindingContext>(MockBehavior.Strict);
            var rpcContext = new JsonRpcBindingContext
            {
                ParameterMetadata = new ParameterMetadata(new JsonName("test", "test"), 0, BindingStyle.Default, false)
            };
            var binderMock = new Mock<ParameterBinder>(testEnvironment.ServiceProvider.GetRequiredService<ILogger<ParameterBinder>>());
            binderMock.Setup(x => x.SetError(It.IsAny<ModelBindingContext>(), It.IsAny<string>(), It.IsAny<IParseResult>()))
                .Returns((Task) null)
                .Verifiable();
            binderMock.Setup(x => x.SetResult(It.IsAny<ModelBindingContext>(), It.IsAny<IParseResult>(), It.IsAny<string>(), It.IsAny<JsonRpcBindingContext>()))
                .CallBase();

            binderMock.Object.SetResult(bindingContextMock.Object, resultMock.Object, "test", rpcContext);

            binderMock.Verify(x => x.SetResult(It.IsAny<ModelBindingContext>(), It.IsAny<IParseResult>(), It.IsAny<string>(), It.IsAny<JsonRpcBindingContext>()));
            binderMock.Verify(x => x.SetError(It.IsAny<ModelBindingContext>(), It.IsAny<string>(), It.IsAny<IParseResult>()));
            binderMock.VerifyNoOtherCalls();
            bindingContextMock.Verify();
        }

        [Test]
        public void Test_SetResult_SetsNullWhenNullAllowed()
        {
            var resultMock = new Mock<NullParseResult>(MockBehavior.Strict, "test");
            var modelMetadataIdentity = ModelMetadataIdentity.ForProperty(typeof(string), "null_name", typeof(object));
            var modelMetadataMock = new Mock<ModelMetadata>(MockBehavior.Strict, modelMetadataIdentity);
            var bindingContextMock = new Mock<ModelBindingContext>(MockBehavior.Strict);
            bindingContextMock.SetupGet(x => x.ModelMetadata).Returns(modelMetadataMock.Object);
            var rpcContext = new JsonRpcBindingContext();
            var binderMock = new Mock<ParameterBinder>(testEnvironment.ServiceProvider.GetRequiredService<ILogger<ParameterBinder>>());
            binderMock.Setup(x => x.SetNullResult(It.IsAny<ModelBindingContext>(), It.IsAny<string>(), It.IsAny<NullParseResult>()))
                .Returns((Task) null)
                .Verifiable();
            binderMock.Setup(x => x.SetResult(It.IsAny<ModelBindingContext>(), It.IsAny<IParseResult>(), It.IsAny<string>(), It.IsAny<JsonRpcBindingContext>()))
                .CallBase();

            binderMock.Object.SetResult(bindingContextMock.Object, resultMock.Object, "test", rpcContext);

            binderMock.Verify(x => x.SetResult(It.IsAny<ModelBindingContext>(), It.IsAny<IParseResult>(), It.IsAny<string>(), It.IsAny<JsonRpcBindingContext>()));
            binderMock.Verify(x => x.SetNullResult(It.IsAny<ModelBindingContext>(), It.IsAny<string>(), It.IsAny<NullParseResult>()));
            binderMock.VerifyNoOtherCalls();
            bindingContextMock.Verify();
        }

        [Test]
        public void Test_SetResult_SetsErrorWhenNullNotAllowed()
        {
            var resultMock = new Mock<NullParseResult>(MockBehavior.Strict, "test");
            var modelMetadataIdentity = ModelMetadataIdentity.ForProperty(typeof(int), "null_name", typeof(object));
            var modelMetadataMock = new Mock<ModelMetadata>(MockBehavior.Strict, modelMetadataIdentity);
            var bindingContextMock = new Mock<ModelBindingContext>(MockBehavior.Strict);
            bindingContextMock.SetupGet(x => x.ModelMetadata).Returns(modelMetadataMock.Object);
            var rpcContext = new JsonRpcBindingContext();
            var binderMock = new Mock<ParameterBinder>(testEnvironment.ServiceProvider.GetRequiredService<ILogger<ParameterBinder>>());
            binderMock.Setup(x => x.SetError(It.IsAny<ModelBindingContext>(), It.IsAny<string>(), It.IsAny<IParseResult>()))
                .Returns((Task) null)
                .Verifiable();
            binderMock.Setup(x => x.SetResult(It.IsAny<ModelBindingContext>(), It.IsAny<IParseResult>(), It.IsAny<string>(), It.IsAny<JsonRpcBindingContext>()))
                .CallBase();

            binderMock.Object.SetResult(bindingContextMock.Object, resultMock.Object, "test", rpcContext);

            binderMock.Verify(x => x.SetResult(It.IsAny<ModelBindingContext>(), It.IsAny<IParseResult>(), It.IsAny<string>(), It.IsAny<JsonRpcBindingContext>()));
            binderMock.Verify(x => x.SetError(It.IsAny<ModelBindingContext>(), It.IsAny<string>(), It.IsAny<IParseResult>()));
            binderMock.VerifyNoOtherCalls();
            bindingContextMock.Verify();
        }

        [Test]
        public void Test_SetResult_ThrowsOnUnknownType()
        {
            var resultMock = new Mock<IParseResult>(MockBehavior.Strict);
            var bindingContextMock = new Mock<ModelBindingContext>(MockBehavior.Strict);
            var rpcContext = new JsonRpcBindingContext();
            Action action = () => parameterBinder.SetResult(bindingContextMock.Object, resultMock.Object, "test", rpcContext);

            action.Should().Throw<ArgumentOutOfRangeException>();

            bindingContextMock.Verify();
        }

        [Test]
        public void Test_SetResult_ThrowsOnNull()
        {
            var bindingContextMock = new Mock<ModelBindingContext>(MockBehavior.Strict);
            var rpcContext = new JsonRpcBindingContext();
            Action action = () => parameterBinder.SetResult(bindingContextMock.Object, null, "test", rpcContext);

            action.Should().Throw<ArgumentOutOfRangeException>();

            bindingContextMock.Verify();
        }
    }
}