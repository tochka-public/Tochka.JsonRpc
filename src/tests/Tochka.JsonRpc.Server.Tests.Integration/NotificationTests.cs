using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;
using Tochka.JsonRpc.Tests.WebApplication;
using Tochka.JsonRpc.TestUtils;
using Tochka.JsonRpc.TestUtils.Integration;

namespace Tochka.JsonRpc.Server.Tests.Integration;

internal sealed class NotificationTests : IntegrationTestsBase<Program>
{
    private Mock<IResponseProvider> responseProviderMock;
    private Mock<IRequestValidator> requestValidatorMock;

    public override void Setup()
    {
        base.Setup();
        responseProviderMock = new Mock<IResponseProvider>();
        requestValidatorMock = new Mock<IRequestValidator>();
    }

    protected override void SetupServices(IServiceCollection services)
    {
        base.SetupServices(services);
        services.AddTransient(_ => responseProviderMock.Object);
        services.AddTransient(_ => requestValidatorMock.Object);
    }

    [Test]
    public async Task ActionOnly_DeserializeSuccessfully()
    {
        const string requestJson =
            $$"""
              {
                  "method": "action_only",
                  "params": {{TestData.PlainRequiredSnakeCaseJson}},
                  "jsonrpc": "2.0"
              }
              """;
        var expectedRequestData = TestData.Plain;

        TestData actualRequestData = null;
        requestValidatorMock.Setup(static v => v.Validate(It.IsAny<TestData>()))
            .Callback<TestData>(requestData => actualRequestData = requestData);

        using var request = new StringContent(requestJson, Encoding.UTF8, "application/json");
        var response = await ApiClient.PostAsync(JsonRpcUrl, request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        actualRequestData.Should().BeEquivalentTo(expectedRequestData);
    }

    [Test]
    public async Task ControllerAndAction_DeserializeSuccessfully()
    {
        const string requestJson =
            $$"""
              {
                  "method": "simple_json_rpc.controller_and_action",
                  "params": {{TestData.PlainRequiredSnakeCaseJson}},
                  "jsonrpc": "2.0"
              }
              """;
        var expectedRequestData = TestData.Plain;

        TestData actualRequestData = null;
        requestValidatorMock.Setup(static v => v.Validate(It.IsAny<TestData>()))
            .Callback<TestData>(requestData => actualRequestData = requestData);

        using var request = new StringContent(requestJson, Encoding.UTF8, "application/json");
        var response = await ApiClient.PostAsync(JsonRpcUrl, request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        actualRequestData.Should().BeEquivalentTo(expectedRequestData);
    }

    [Test]
    public async Task BindingStyleDefault_ObjectParams_DeserializeSuccessfully()
    {
        const string requestJson =
            $$"""
              {
                  "method": "binding_style_default",
                  "params": {
                      "data": {{TestData.PlainRequiredSnakeCaseJson}}
                  },
                  "jsonrpc": "2.0"
              }
              """;
        var expectedRequestData = TestData.Plain;

        TestData actualRequestData = null;
        requestValidatorMock.Setup(static v => v.Validate(It.IsAny<TestData>()))
            .Callback<TestData>(requestData => actualRequestData = requestData);

        using var request = new StringContent(requestJson, Encoding.UTF8, "application/json");
        var response = await ApiClient.PostAsync(JsonRpcUrl, request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        actualRequestData.Should().BeEquivalentTo(expectedRequestData);
    }

    [Test]
    public async Task BindingStyleDefault_ArrayParams_DeserializeSuccessfully()
    {
        const string requestJson =
            $$"""
              {
                  "method": "binding_style_default",
                  "params": [
                      {{TestData.PlainRequiredSnakeCaseJson}}
                  ],
                  "jsonrpc": "2.0"
              }
              """;
        var expectedRequestData = TestData.Plain;

        TestData actualRequestData = null;
        requestValidatorMock.Setup(static v => v.Validate(It.IsAny<TestData>()))
            .Callback<TestData>(requestData => actualRequestData = requestData);

        using var request = new StringContent(requestJson, Encoding.UTF8, "application/json");
        var response = await ApiClient.PostAsync(JsonRpcUrl, request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        actualRequestData.Should().BeEquivalentTo(expectedRequestData);
    }

    [Test]
    public async Task BindingStyleDefault_NullParams_DontProcess()
    {
        const string requestJson = """
                                   {
                                       "method": "binding_style_default",
                                       "params": null,
                                       "jsonrpc": "2.0"
                                   }
                                   """;

        using var request = new StringContent(requestJson, Encoding.UTF8, "application/json");
        var response = await ApiClient.PostAsync(JsonRpcUrl, request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        requestValidatorMock.VerifyNoOtherCalls();
    }

    [Test]
    public async Task BindingStyleDefault_WithoutParams_DontProcess()
    {
        const string requestJson = """
                                   {
                                       "method": "binding_style_default",
                                       "jsonrpc": "2.0"
                                   }
                                   """;

        using var request = new StringContent(requestJson, Encoding.UTF8, "application/json");
        var response = await ApiClient.PostAsync(JsonRpcUrl, request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        requestValidatorMock.VerifyNoOtherCalls();
    }

    [Test]
    public async Task BindingStyleDefault_EmptyObjectParams_DontProcess()
    {
        const string requestJson = """
                                   {
                                       "method": "binding_style_default",
                                       "params": {},
                                       "jsonrpc": "2.0"
                                   }
                                   """;

        using var request = new StringContent(requestJson, Encoding.UTF8, "application/json");
        var response = await ApiClient.PostAsync(JsonRpcUrl, request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        requestValidatorMock.VerifyNoOtherCalls();
    }

    [Test]
    public async Task BindingStyleDefault_EmptyArrayParams_DontProcess()
    {
        const string requestJson = """
                                   {
                                       "method": "binding_style_default",
                                       "params": [],
                                       "jsonrpc": "2.0"
                                   }
                                   """;

        using var request = new StringContent(requestJson, Encoding.UTF8, "application/json");
        var response = await ApiClient.PostAsync(JsonRpcUrl, request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        requestValidatorMock.VerifyNoOtherCalls();
    }

    [Test]
    public async Task BindingStyleObject_DeserializeSuccessfully()
    {
        const string requestJson =
            $$"""
              {
                  "method": "binding_style_object",
                  "params": {{TestData.PlainRequiredSnakeCaseJson}},
                  "jsonrpc": "2.0"
              }
              """;
        var expectedRequestData = TestData.Plain;

        TestData actualRequestData = null;
        requestValidatorMock.Setup(static v => v.Validate(It.IsAny<TestData>()))
            .Callback<TestData>(requestData => actualRequestData = requestData);

        using var request = new StringContent(requestJson, Encoding.UTF8, "application/json");
        var response = await ApiClient.PostAsync(JsonRpcUrl, request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        actualRequestData.Should().BeEquivalentTo(expectedRequestData);
    }

    [Test]
    public async Task BindingStyleObject_NullParams_DontProcess()
    {
        const string requestJson = """
                                   {
                                       "method": "binding_style_object",
                                       "params": null,
                                       "jsonrpc": "2.0"
                                   }
                                   """;

        using var request = new StringContent(requestJson, Encoding.UTF8, "application/json");
        var response = await ApiClient.PostAsync(JsonRpcUrl, request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        requestValidatorMock.VerifyNoOtherCalls();
    }

    [Test]
    public async Task BindingStyleObject_WithoutParams_DontProcess()
    {
        const string requestJson = """
                                   {
                                       "method": "binding_style_object",
                                       "jsonrpc": "2.0"
                                   }
                                   """;

        using var request = new StringContent(requestJson, Encoding.UTF8, "application/json");
        var response = await ApiClient.PostAsync(JsonRpcUrl, request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        requestValidatorMock.VerifyNoOtherCalls();
    }

    [Test]
    public async Task BindingStyleObject_EmptyObjectParams_DontProcess()
    {
        const string requestJson = """
                                   {
                                       "method": "binding_style_object",
                                       "params": {},
                                       "jsonrpc": "2.0"
                                   }
                                   """;

        using var request = new StringContent(requestJson, Encoding.UTF8, "application/json");
        var response = await ApiClient.PostAsync(JsonRpcUrl, request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        requestValidatorMock.VerifyNoOtherCalls();
    }

    [Test]
    public async Task BindingStyleObject_EmptyArrayParams_DontProcess()
    {
        const string requestJson = """
                                   {
                                       "method": "binding_style_object",
                                       "params": [],
                                       "jsonrpc": "2.0"
                                   }
                                   """;

        using var request = new StringContent(requestJson, Encoding.UTF8, "application/json");
        var response = await ApiClient.PostAsync(JsonRpcUrl, request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        requestValidatorMock.VerifyNoOtherCalls();
    }

    [Test]
    public async Task BindingStyleArray_DeserializeSuccessfully()
    {
        const string requestJson =
            $$"""
              {
                  "method": "binding_style_array",
                  "params": [
                      {{TestData.PlainRequiredSnakeCaseJson}}
                  ],
                  "jsonrpc": "2.0"
              }
              """;
        var expectedRequestData = new List<TestData> { TestData.Plain };

        object? actualRequestData = null;
        requestValidatorMock.Setup(static v => v.Validate(It.IsAny<object?>()))
            .Callback<object?>(requestData => actualRequestData = requestData);

        using var request = new StringContent(requestJson, Encoding.UTF8, "application/json");
        var response = await ApiClient.PostAsync(JsonRpcUrl, request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        actualRequestData.Should().BeEquivalentTo(expectedRequestData);
    }

    [Test]
    public async Task BindingStyleArray_NullParams_DontProcess()
    {
        const string requestJson = """
                                   {
                                       "method": "binding_style_array",
                                       "params": null,
                                       "jsonrpc": "2.0"
                                   }
                                   """;

        using var request = new StringContent(requestJson, Encoding.UTF8, "application/json");
        var response = await ApiClient.PostAsync(JsonRpcUrl, request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        requestValidatorMock.VerifyNoOtherCalls();
    }

    [Test]
    public async Task BindingStyleArray_WithoutParams_DontProcess()
    {
        const string requestJson = """
                                   {
                                       "method": "binding_style_array",
                                       "jsonrpc": "2.0"
                                   }
                                   """;

        using var request = new StringContent(requestJson, Encoding.UTF8, "application/json");
        var response = await ApiClient.PostAsync(JsonRpcUrl, request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        requestValidatorMock.VerifyNoOtherCalls();
    }

    [Test]
    public async Task BindingStyleArray_EmptyObjectParams_DontProcess()
    {
        const string requestJson = """
                                   {
                                       "method": "binding_style_array",
                                       "params": {},
                                       "jsonrpc": "2.0"
                                   }
                                   """;

        using var request = new StringContent(requestJson, Encoding.UTF8, "application/json");
        var response = await ApiClient.PostAsync(JsonRpcUrl, request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        requestValidatorMock.VerifyNoOtherCalls();
    }

    [Test]
    public async Task BindingStyleArray_EmptyArrayParams_DeserializeSuccessfully()
    {
        const string requestJson = """
                                   {
                                       "method": "binding_style_array",
                                       "params": [],
                                       "jsonrpc": "2.0"
                                   }
                                   """;
        var expectedRequestData = new List<TestData>();

        object? actualRequestData = null;
        requestValidatorMock.Setup(static v => v.Validate(It.IsAny<object?>()))
            .Callback<object?>(requestData => actualRequestData = requestData);

        using var request = new StringContent(requestJson, Encoding.UTF8, "application/json");
        var response = await ApiClient.PostAsync(JsonRpcUrl, request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        actualRequestData.Should().BeEquivalentTo(expectedRequestData);
    }

    [Test]
    public async Task NullableDefaultParams_NullParams_DontProcess()
    {
        const string requestJson = """
                                   {
                                       "method": "nullable_default_params",
                                       "params": null,
                                       "jsonrpc": "2.0"
                                   }
                                   """;

        using var request = new StringContent(requestJson, Encoding.UTF8, "application/json");
        var response = await ApiClient.PostAsync(JsonRpcUrl, request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        requestValidatorMock.VerifyNoOtherCalls();
    }

    [Test]
    public async Task NullableDefaultParams_WithoutParams_DontProcess()
    {
        const string requestJson = """
                                   {
                                       "method": "nullable_default_params",
                                       "jsonrpc": "2.0"
                                   }
                                   """;

        using var request = new StringContent(requestJson, Encoding.UTF8, "application/json");
        var response = await ApiClient.PostAsync(JsonRpcUrl, request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        requestValidatorMock.VerifyNoOtherCalls();
    }

    [Test]
    public async Task NullableDefaultParams_EmptyObjectParams_DontProcess()
    {
        const string requestJson = """
                                   {
                                       "method": "nullable_default_params",
                                       "params": {},
                                       "jsonrpc": "2.0"
                                   }
                                   """;

        using var request = new StringContent(requestJson, Encoding.UTF8, "application/json");
        var response = await ApiClient.PostAsync(JsonRpcUrl, request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        requestValidatorMock.VerifyNoOtherCalls();
    }

    [Test]
    public async Task NullableDefaultParams_EmptyArrayParams_DontProcess()
    {
        const string requestJson = """
                                   {
                                       "method": "nullable_default_params",
                                       "params": [],
                                       "jsonrpc": "2.0"
                                   }
                                   """;

        using var request = new StringContent(requestJson, Encoding.UTF8, "application/json");
        var response = await ApiClient.PostAsync(JsonRpcUrl, request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        requestValidatorMock.VerifyNoOtherCalls();
    }

    [Test]
    public async Task NullableObjectParams_NullParams_SetNull()
    {
        const string requestJson = """
                                   {
                                       "method": "nullable_object_params",
                                       "params": null,
                                       "jsonrpc": "2.0"
                                   }
                                   """;
        object? expectedRequestData = null;

        object? actualRequestData = null;
        requestValidatorMock.Setup(static v => v.Validate(It.IsAny<object?>()))
            .Callback<object?>(requestData => actualRequestData = requestData)
            .Verifiable();

        using var request = new StringContent(requestJson, Encoding.UTF8, "application/json");
        var response = await ApiClient.PostAsync(JsonRpcUrl, request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        requestValidatorMock.Verify();
        actualRequestData.Should().BeEquivalentTo(expectedRequestData);
    }

    [Test]
    public async Task NullableObjectParams_WithoutParams_DontProcess()
    {
        const string requestJson = """
                                   {
                                       "method": "nullable_object_params",
                                       "jsonrpc": "2.0"
                                   }
                                   """;

        using var request = new StringContent(requestJson, Encoding.UTF8, "application/json");
        var response = await ApiClient.PostAsync(JsonRpcUrl, request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        requestValidatorMock.VerifyNoOtherCalls();
    }

    [Test]
    public async Task NullableObjectParams_EmptyObjectParams_SetEmptyObject()
    {
        const string requestJson = """
                                   {
                                       "method": "nullable_object_params",
                                       "params": {},
                                       "jsonrpc": "2.0"
                                   }
                                   """;

        object? actualRequestData = null;
        requestValidatorMock.Setup(static v => v.Validate(It.IsAny<object?>()))
            .Callback<object?>(requestData => actualRequestData = requestData)
            .Verifiable();

        using var request = new StringContent(requestJson, Encoding.UTF8, "application/json");
        var response = await ApiClient.PostAsync(JsonRpcUrl, request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        requestValidatorMock.Verify();
        actualRequestData.Should().NotBeNull();
    }

    [Test]
    public async Task NullableObjectParams_EmptyArrayParams_DontProcess()
    {
        const string requestJson = """
                                   {
                                       "method": "nullable_object_params",
                                       "params": ,
                                       "jsonrpc": "2.0"
                                   }
                                   """;

        using var request = new StringContent(requestJson, Encoding.UTF8, "application/json");
        var response = await ApiClient.PostAsync(JsonRpcUrl, request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        requestValidatorMock.VerifyNoOtherCalls();
    }

    [Test]
    public async Task NullableArrayParams_NullParams_SetNull()
    {
        const string requestJson = """
                                   {
                                       "method": "nullable_array_params",
                                       "params": null,
                                       "jsonrpc": "2.0"
                                   }
                                   """;
        object? expectedRequestData = null;

        object? actualRequestData = null;
        requestValidatorMock.Setup(static v => v.Validate(It.IsAny<object?>()))
            .Callback<object?>(requestData => actualRequestData = requestData)
            .Verifiable();

        using var request = new StringContent(requestJson, Encoding.UTF8, "application/json");
        var response = await ApiClient.PostAsync(JsonRpcUrl, request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        requestValidatorMock.Verify();
        actualRequestData.Should().BeEquivalentTo(expectedRequestData);
    }

    [Test]
    public async Task NullableArrayParams_WithoutParams_DontProcess()
    {
        const string requestJson = """
                                   {
                                       "method": "nullable_array_params",
                                       "jsonrpc": "2.0"
                                   }
                                   """;

        using var request = new StringContent(requestJson, Encoding.UTF8, "application/json");
        var response = await ApiClient.PostAsync(JsonRpcUrl, request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        requestValidatorMock.VerifyNoOtherCalls();
    }

    [Test]
    public async Task NullableArrayParams_EmptyObjectParams_DontProcess()
    {
        const string requestJson = """
                                   {
                                       "method": "nullable_array_params",
                                       "params": {},
                                       "jsonrpc": "2.0"
                                   }
                                   """;

        using var request = new StringContent(requestJson, Encoding.UTF8, "application/json");
        var response = await ApiClient.PostAsync(JsonRpcUrl, request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        requestValidatorMock.VerifyNoOtherCalls();
    }

    [Test]
    public async Task NullableArrayParams_EmptyArrayParams_SetEmptyArray()
    {
        const string requestJson = """
                                   {
                                       "method": "nullable_array_params",
                                       "params": [],
                                       "jsonrpc": "2.0"
                                   }
                                   """;
        object? expectedRequestData = new List<object?>();

        object? actualRequestData = null;
        requestValidatorMock.Setup(static v => v.Validate(It.IsAny<object?>()))
            .Callback<object?>(requestData => actualRequestData = requestData)
            .Verifiable();

        using var request = new StringContent(requestJson, Encoding.UTF8, "application/json");
        var response = await ApiClient.PostAsync(JsonRpcUrl, request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        requestValidatorMock.Verify();
        actualRequestData.Should().BeEquivalentTo(expectedRequestData);
    }

    [Test]
    public async Task DefaultParams_NullParams_DontProcess()
    {
        const string requestJson = """
                                   {
                                       "method": "default_params",
                                       "params": null,
                                       "jsonrpc": "2.0"
                                   }
                                   """;

        using var request = new StringContent(requestJson, Encoding.UTF8, "application/json");
        var response = await ApiClient.PostAsync(JsonRpcUrl, request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        requestValidatorMock.VerifyNoOtherCalls();
    }

    [Test]
    public async Task DefaultParams_WithoutParams_DontProcess()
    {
        const string requestJson = """
                                   {
                                       "method": "default_params",
                                       "jsonrpc": "2.0"
                                   }
                                   """;

        using var request = new StringContent(requestJson, Encoding.UTF8, "application/json");
        var response = await ApiClient.PostAsync(JsonRpcUrl, request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        requestValidatorMock.VerifyNoOtherCalls();
    }

    [Test]
    public async Task DefaultParams_EmptyObjectParams_SetDefaultValue()
    {
        const string requestJson = """
                                   {
                                       "method": "default_params",
                                       "params": {},
                                       "jsonrpc": "2.0"
                                   }
                                   """;
        object? expectedRequestData = "123";

        object? actualRequestData = null;
        requestValidatorMock.Setup(static v => v.Validate(It.IsAny<object?>()))
            .Callback<object?>(requestData => actualRequestData = requestData)
            .Verifiable();

        using var request = new StringContent(requestJson, Encoding.UTF8, "application/json");
        var response = await ApiClient.PostAsync(JsonRpcUrl, request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        requestValidatorMock.Verify();
        actualRequestData.Should().BeEquivalentTo(expectedRequestData);
    }

    [Test]
    public async Task DefaultParams_EmptyArrayParams_SetDefaultValue()
    {
        const string requestJson = """
                                   {
                                       "method": "default_params",
                                       "params": [],
                                       "jsonrpc": "2.0"
                                   }
                                   """;
        object? expectedRequestData = "123";

        object? actualRequestData = null;
        requestValidatorMock.Setup(static v => v.Validate(It.IsAny<object?>()))
            .Callback<object?>(requestData => actualRequestData = requestData)
            .Verifiable();

        using var request = new StringContent(requestJson, Encoding.UTF8, "application/json");
        var response = await ApiClient.PostAsync(JsonRpcUrl, request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        requestValidatorMock.Verify();
        actualRequestData.Should().BeEquivalentTo(expectedRequestData);
    }

    [Test]
    public async Task DefaultObjectParams_NullParams_SetDefault()
    {
        const string requestJson = """
                                   {
                                       "method": "default_object_params",
                                       "params": null,
                                       "jsonrpc": "2.0"
                                   }
                                   """;
        object? expectedRequestData = "123";

        object? actualRequestData = null;
        requestValidatorMock.Setup(static v => v.Validate(It.IsAny<object?>()))
            .Callback<object?>(requestData => actualRequestData = requestData)
            .Verifiable();

        using var request = new StringContent(requestJson, Encoding.UTF8, "application/json");
        var response = await ApiClient.PostAsync(JsonRpcUrl, request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        requestValidatorMock.Verify();
        actualRequestData.Should().BeEquivalentTo(expectedRequestData);
    }

    [Test]
    public async Task DefaultObjectParams_WithoutParams_SetDefaultValue()
    {
        const string requestJson = """
                                   {
                                       "method": "default_object_params",
                                       "jsonrpc": "2.0"
                                   }
                                   """;
        object? expectedRequestData = "123";

        object? actualRequestData = null;
        requestValidatorMock.Setup(static v => v.Validate(It.IsAny<object?>()))
            .Callback<object?>(requestData => actualRequestData = requestData)
            .Verifiable();

        using var request = new StringContent(requestJson, Encoding.UTF8, "application/json");
        var response = await ApiClient.PostAsync(JsonRpcUrl, request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        requestValidatorMock.Verify();
        actualRequestData.Should().BeEquivalentTo(expectedRequestData);
    }

    [Test]
    public async Task DefaultObjectParams_EmptyObjectParams_DontProcess()
    {
        const string requestJson = """
                                   {
                                       "method": "default_object_params",
                                       "params": {},
                                       "jsonrpc": "2.0"
                                   }
                                   """;

        using var request = new StringContent(requestJson, Encoding.UTF8, "application/json");
        var response = await ApiClient.PostAsync(JsonRpcUrl, request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        requestValidatorMock.VerifyNoOtherCalls();
    }

    [Test]
    public async Task DefaultObjectParams_EmptyArrayParams_DontProcess()
    {
        const string requestJson = """
                                   {
                                       "method": "default_object_params",
                                       "params": [],
                                       "jsonrpc": "2.0"
                                   }
                                   """;

        using var request = new StringContent(requestJson, Encoding.UTF8, "application/json");
        var response = await ApiClient.PostAsync(JsonRpcUrl, request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        requestValidatorMock.VerifyNoOtherCalls();
    }

    [Test]
    public async Task DefaultArrayParams_NullParams_SetNull()
    {
        const string requestJson = """
                                   {
                                       "method": "default_array_params",
                                       "params": null,
                                       "jsonrpc": "2.0"
                                   }
                                   """;
        object? expectedRequestData = null;

        object? actualRequestData = null;
        requestValidatorMock.Setup(static v => v.Validate(It.IsAny<object?>()))
            .Callback<object?>(requestData => actualRequestData = requestData)
            .Verifiable();

        using var request = new StringContent(requestJson, Encoding.UTF8, "application/json");
        var response = await ApiClient.PostAsync(JsonRpcUrl, request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        requestValidatorMock.Verify();
        actualRequestData.Should().BeEquivalentTo(expectedRequestData);
    }

    [Test]
    public async Task DefaultArrayParams_WithoutParams_SetDefault()
    {
        const string requestJson = """
                                   {
                                       "method": "default_array_params",
                                       "jsonrpc": "2.0"
                                   }
                                   """;
        object? expectedRequestData = null;

        object? actualRequestData = null;
        requestValidatorMock.Setup(static v => v.Validate(It.IsAny<object?>()))
            .Callback<object?>(requestData => actualRequestData = requestData)
            .Verifiable();

        using var request = new StringContent(requestJson, Encoding.UTF8, "application/json");
        var response = await ApiClient.PostAsync(JsonRpcUrl, request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        requestValidatorMock.Verify();
        actualRequestData.Should().BeEquivalentTo(expectedRequestData);
    }

    [Test]
    public async Task DefaultArrayParams_EmptyObjectParams_DontProcess()
    {
        const string requestJson = """
                                   {
                                       "method": "default_array_params",
                                       "params": {},
                                       "jsonrpc": "2.0"
                                   }
                                   """;

        using var request = new StringContent(requestJson, Encoding.UTF8, "application/json");
        var response = await ApiClient.PostAsync(JsonRpcUrl, request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        requestValidatorMock.VerifyNoOtherCalls();
    }

    [Test]
    public async Task DefaultArrayParams_EmptyArrayParams_SetEmptyArray()
    {
        const string requestJson = """
                                   {
                                       "method": "default_array_params",
                                       "params": [],
                                       "jsonrpc": "2.0"
                                   }
                                   """;
        object? expectedRequestData = new List<string?>();

        object? actualRequestData = null;
        requestValidatorMock.Setup(static v => v.Validate(It.IsAny<object?>()))
            .Callback<object?>(requestData => actualRequestData = requestData)
            .Verifiable();

        using var request = new StringContent(requestJson, Encoding.UTF8, "application/json");
        var response = await ApiClient.PostAsync(JsonRpcUrl, request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        requestValidatorMock.Verify();
        actualRequestData.Should().BeEquivalentTo(expectedRequestData);
    }

    [Test]
    public async Task NoParams_ProcessSuccessfully()
    {
        const string requestJson = """
                                   {
                                       "method": "no_params",
                                       "jsonrpc": "2.0"
                                   }
                                   """;

        using var request = new StringContent(requestJson, Encoding.UTF8, "application/json");
        var response = await ApiClient.PostAsync(JsonRpcUrl, request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Test]
    public async Task SnakeCaseParams_DeserializeSuccessfully()
    {
        const string requestJson =
            $$"""
              {
                  "method": "snake_case_params",
                  "params": {{TestData.PlainRequiredSnakeCaseJson}},
                  "jsonrpc": "2.0"
              }
              """;
        var expectedRequestData = TestData.Plain;

        TestData actualRequestData = null;
        requestValidatorMock.Setup(static v => v.Validate(It.IsAny<TestData>()))
            .Callback<TestData>(requestData => actualRequestData = requestData);

        using var request = new StringContent(requestJson, Encoding.UTF8, "application/json");
        var response = await ApiClient.PostAsync(JsonRpcUrl, request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        actualRequestData.Should().BeEquivalentTo(expectedRequestData);
    }

    [Test]
    public async Task CamelCaseParams_DeserializeSuccessfully()
    {
        const string requestJson =
            $$"""
              {
                  "method": "camelCaseParams",
                  "params": {{TestData.PlainRequiredCamelCaseJson}},
                  "jsonrpc": "2.0"
              }
              """;
        var expectedRequestData = TestData.Plain;

        TestData actualRequestData = null;
        requestValidatorMock.Setup(static v => v.Validate(It.IsAny<TestData>()))
            .Callback<TestData>(requestData => actualRequestData = requestData);

        using var request = new StringContent(requestJson, Encoding.UTF8, "application/json");
        var response = await ApiClient.PostAsync(JsonRpcUrl, request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        actualRequestData.Should().BeEquivalentTo(expectedRequestData);
    }

    [Test]
    public async Task CustomActionRoute_DeserializeSuccessfully()
    {
        const string requestJson =
            $$"""
              {
                  "method": "custom_action_route",
                  "params": {{TestData.PlainRequiredSnakeCaseJson}},
                  "jsonrpc": "2.0"
              }
              """;
        var expectedRequestData = TestData.Plain;

        TestData actualRequestData = null;
        requestValidatorMock.Setup(static v => v.Validate(It.IsAny<TestData>()))
            .Callback<TestData>(requestData => actualRequestData = requestData);

        using var request = new StringContent(requestJson, Encoding.UTF8, "application/json");
        var response = await ApiClient.PostAsync($"{JsonRpcUrl}/custom/action", request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        actualRequestData.Should().BeEquivalentTo(expectedRequestData);
    }

    [Test]
    public async Task CustomControllerRoute_DeserializeSuccessfully()
    {
        const string requestJson =
            $$"""
              {
                  "method": "custom_controller_route",
                  "params": {{TestData.PlainRequiredSnakeCaseJson}},
                  "jsonrpc": "2.0"
              }
              """;
        var expectedRequestData = TestData.Plain;

        TestData actualRequestData = null;
        requestValidatorMock.Setup(static v => v.Validate(It.IsAny<TestData>()))
            .Callback<TestData>(requestData => actualRequestData = requestData);

        using var request = new StringContent(requestJson, Encoding.UTF8, "application/json");
        var response = await ApiClient.PostAsync($"{JsonRpcUrl}/custom/controller", request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        actualRequestData.Should().BeEquivalentTo(expectedRequestData);
    }

    private const string JsonRpcUrl = "/api/jsonrpc";
}
