using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;
using Tochka.JsonRpc.Client.Tests.WebApplication;
using Tochka.JsonRpc.TestUtils;
using Tochka.JsonRpc.TestUtils.Integration;

namespace Tochka.JsonRpc.Server.Tests.Integration;

[TestFixture]
internal class IntegrationTests : IntegrationTestsBase<Program>
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
    public async Task NotJson_Return404()
    {
        const string requestContent = "Hello World!";

        using var request = new StringContent(requestContent, Encoding.UTF8, "text/plain");
        var response = await ApiClient.PostAsync(JsonRpcUrl, request);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Test]
    public async Task Notification_ActionOnly_DeserializeSuccessfully()
    {
        const string requestJson = $$"""
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
    public async Task Notification_ControllerAndAction_DeserializeSuccessfully()
    {
        const string requestJson = $$"""
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
    public async Task Notification_BindingStyleDefaultWithObjectParams_DeserializeSuccessfully()
    {
        const string requestJson = $$"""
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
    public async Task Notification_BindingStyleDefaultWithArrayParams_DeserializeSuccessfully()
    {
        const string requestJson = $$"""
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
    public async Task Notification_BindingStyleDefaultWithNullParams_DontProcess()
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
    public async Task Notification_BindingStyleDefaultWithoutParams_DontProcess()
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
    public async Task Notification_BindingStyleDefaultWithEmptyObjectParams_DontProcess()
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
    public async Task Notification_BindingStyleDefaultWithEmptyArrayParams_DontProcess()
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
    public async Task Notification_BindingStyleObject_DeserializeSuccessfully()
    {
        const string requestJson = $$"""
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
    public async Task Notification_BindingStyleObjectWithNullParams_DontProcess()
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
    public async Task Notification_BindingStyleObjectWithoutParams_DontProcess()
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
    public async Task Notification_BindingStyleObjectWithEmptyObjectParams_DontProcess()
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
    public async Task Notification_BindingStyleObjectWithEmptyArrayParams_DontProcess()
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
    public async Task Notification_BindingStyleArray_DeserializeSuccessfully()
    {
        const string requestJson = $$"""
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
    public async Task Notification_BindingStyleArrayWithNullParams_DontProcess()
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
    public async Task Notification_BindingStyleArrayWithoutParams_DontProcess()
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
    public async Task Notification_BindingStyleArrayWithEmptyObjectParams_DontProcess()
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
    public async Task Notification_BindingStyleArrayWithEmptyArrayParams_DeserializeSuccessfully()
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
    public async Task Notification_NullableDefaultParamsWithNullParams_()
    {}

    [Test]
    public async Task Notification_NullableDefaultParamsWithoutParams_()
    {}

    [Test]
    public async Task Notification_NullableDefaultParamsWithEmptyObjectParams_()
    {}

    [Test]
    public async Task Notification_NullableDefaultParamsWithEmptyArrayParams_()
    {}

    [Test]
    public async Task Notification_NullableObjectParamsWithNullParams_()
    {}

    [Test]
    public async Task Notification_NullableObjectParamsWithoutParams_()
    {}

    [Test]
    public async Task Notification_NullableObjectParamsWithEmptyObjectParams_()
    {}

    [Test]
    public async Task Notification_NullableObjectParamsWithEmptyArrayParams_()
    {}

    [Test]
    public async Task Notification_NullableArrayParamsWithNullParams_()
    {}

    [Test]
    public async Task Notification_NullableArrayParamsWithoutParams_()
    {}

    [Test]
    public async Task Notification_NullableArrayParamsWithEmptyObjectParams_()
    {}

    [Test]
    public async Task Notification_NullableArrayParamsWithEmptyArrayParams_()
    {}

    [Test]
    public async Task Notification_DefaultParamsWithNullParams_()
    {}

    [Test]
    public async Task Notification_DefaultParamsWithoutParams_()
    {}

    [Test]
    public async Task Notification_DefaultParamsWithEmptyObjectParams_()
    {}

    [Test]
    public async Task Notification_DefaultParamsWithEmptyArrayParams_()
    {}

    [Test]
    public async Task Notification_DefaultObjectParamsWithNullParams_()
    {}

    [Test]
    public async Task Notification_DefaultObjectParamsWithoutParams_()
    {}

    [Test]
    public async Task Notification_DefaultObjectParamsWithEmptyObjectParams_()
    {}

    [Test]
    public async Task Notification_DefaultObjectParamsWithEmptyArrayParams_()
    {}

    [Test]
    public async Task Notification_DefaultArrayParamsWithNullParams_()
    {}

    [Test]
    public async Task Notification_DefaultArrayParamsWithoutParams_()
    {}

    [Test]
    public async Task Notification_DefaultArrayParamsWithEmptyObjectParams_()
    {}

    [Test]
    public async Task Notification_DefaultArrayParamsWithEmptyArrayParams_()
    {}

    [Test]
    public async Task Notification_NoParams_ProcessSuccessfully()
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
    public async Task Notification_SnakeCaseParams_DeserializeSuccessfully()
    {
        const string requestJson = $$"""
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
    public async Task Notification_CamelCaseParams_DeserializeSuccessfully()
    {
        const string requestJson = $$"""
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
    public async Task Notification_CustomActionRoute_DeserializeSuccessfully()
    {
        const string requestJson = $$"""
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
    public async Task Notification_CustomControllerRoute_DeserializeSuccessfully()
    {
        const string requestJson = $$"""
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

    [Test]
    public async Task Request_StringId_DeserializeSuccessfully()
    {
        const string requestJson = $$"""
            {
                "method": "process_anything",
                "id": "123",
                "params": {{TestData.PlainRequiredSnakeCaseJson}},
                "jsonrpc": "2.0"
            }
            """;
        var expectedRequestData = TestData.Plain;
        var responseData = TestData.Plain;
        var expectedResponseJson = $$"""
            {
                "id": "123",
                "result": {{TestData.PlainFullSnakeCaseJson}},
                "jsonrpc": "2.0"
            }
            """.TrimAllLines();

        TestData actualRequestData = null;
        requestValidatorMock.Setup(static v => v.Validate(It.IsAny<TestData>()))
            .Callback<TestData>(requestData => actualRequestData = requestData);
        responseProviderMock.Setup(static p => p.GetJsonRpcResponse())
            .Returns(responseData);

        using var request = new StringContent(requestJson, Encoding.UTF8, "application/json");
        var response = await ApiClient.PostAsync(JsonRpcUrl, request);
        var actualResponseJson = (await response.Content.ReadAsStringAsync()).TrimAllLines();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        actualRequestData.Should().BeEquivalentTo(expectedRequestData);
        actualResponseJson.Should().Be(expectedResponseJson);
    }

    [Test]
    public async Task Request_IntId_DeserializeSuccessfully()
    {
        const string requestJson = $$"""
            {
                "method": "process_anything",
                "id": 123,
                "params": {{TestData.PlainRequiredSnakeCaseJson}},
                "jsonrpc": "2.0"
            }
            """;
        var expectedRequestData = TestData.Plain;
        var responseData = TestData.Plain;
        var expectedResponseJson = $$"""
            {
                "id": 123,
                "result": {{TestData.PlainFullSnakeCaseJson}},
                "jsonrpc": "2.0"
            }
            """.TrimAllLines();

        TestData actualRequestData = null;
        requestValidatorMock.Setup(static v => v.Validate(It.IsAny<TestData>()))
            .Callback<TestData>(requestData => actualRequestData = requestData);
        responseProviderMock.Setup(static p => p.GetJsonRpcResponse())
            .Returns(responseData);

        using var request = new StringContent(requestJson, Encoding.UTF8, "application/json");
        var response = await ApiClient.PostAsync(JsonRpcUrl, request);
        var actualResponseJson = (await response.Content.ReadAsStringAsync()).TrimAllLines();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        actualRequestData.Should().BeEquivalentTo(expectedRequestData);
        actualResponseJson.Should().Be(expectedResponseJson);
    }

    [Test]
    public async Task Request_NullId_DeserializeSuccessfully()
    {
        const string requestJson = $$"""
            {
                "method": "process_anything",
                "id": null,
                "params": {{TestData.PlainRequiredSnakeCaseJson}},
                "jsonrpc": "2.0"
            }
            """;
        var expectedRequestData = TestData.Plain;
        var responseData = TestData.Plain;
        var expectedResponseJson = $$"""
            {
                "id": null,
                "result": {{TestData.PlainFullSnakeCaseJson}},
                "jsonrpc": "2.0"
            }
            """.TrimAllLines();

        TestData actualRequestData = null;
        requestValidatorMock.Setup(static v => v.Validate(It.IsAny<TestData>()))
            .Callback<TestData>(requestData => actualRequestData = requestData);
        responseProviderMock.Setup(static p => p.GetJsonRpcResponse())
            .Returns(responseData);

        using var request = new StringContent(requestJson, Encoding.UTF8, "application/json");
        var response = await ApiClient.PostAsync(JsonRpcUrl, request);
        var actualResponseJson = (await response.Content.ReadAsStringAsync()).TrimAllLines();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        actualRequestData.Should().BeEquivalentTo(expectedRequestData);
        actualResponseJson.Should().Be(expectedResponseJson);
    }

    [Test]
    public async Task Request_ActionOnly_DeserializeSuccessfully()
    {
        const string requestJson = $$"""
            {
                "method": "action_only",
                "id": "123",
                "params": {{TestData.PlainRequiredSnakeCaseJson}},
                "jsonrpc": "2.0"
            }
            """;
        var expectedRequestData = TestData.Plain;
        var responseData = TestData.Plain;
        var expectedResponseJson = $$"""
            {
                "id": "123",
                "result": {{TestData.PlainFullSnakeCaseJson}},
                "jsonrpc": "2.0"
            }
            """.TrimAllLines();

        TestData actualRequestData = null;
        requestValidatorMock.Setup(static v => v.Validate(It.IsAny<TestData>()))
            .Callback<TestData>(requestData => actualRequestData = requestData);
        responseProviderMock.Setup(static p => p.GetJsonRpcResponse())
            .Returns(responseData);

        using var request = new StringContent(requestJson, Encoding.UTF8, "application/json");
        var response = await ApiClient.PostAsync(JsonRpcUrl, request);
        var actualResponseJson = (await response.Content.ReadAsStringAsync()).TrimAllLines();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        actualRequestData.Should().BeEquivalentTo(expectedRequestData);
        actualResponseJson.Should().Be(expectedResponseJson);
    }

    [Test]
    public async Task Request_ControllerAndAction_DeserializeSuccessfully()
    {
        const string requestJson = $$"""
            {
                "method": "simple_json_rpc.controller_and_action",
                "id": "123",
                "params": {{TestData.PlainRequiredSnakeCaseJson}},
                "jsonrpc": "2.0"
            }
            """;
        var expectedRequestData = TestData.Plain;
        var responseData = TestData.Plain;
        var expectedResponseJson = $$"""
            {
                "id": "123",
                "result": {{TestData.PlainFullSnakeCaseJson}},
                "jsonrpc": "2.0"
            }
            """.TrimAllLines();

        TestData actualRequestData = null;
        requestValidatorMock.Setup(static v => v.Validate(It.IsAny<TestData>()))
            .Callback<TestData>(requestData => actualRequestData = requestData);
        responseProviderMock.Setup(static p => p.GetJsonRpcResponse())
            .Returns(responseData);

        using var request = new StringContent(requestJson, Encoding.UTF8, "application/json");
        var response = await ApiClient.PostAsync(JsonRpcUrl, request);
        var actualResponseJson = (await response.Content.ReadAsStringAsync()).TrimAllLines();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        actualRequestData.Should().BeEquivalentTo(expectedRequestData);
        actualResponseJson.Should().Be(expectedResponseJson);
    }

    [Test]
    public async Task Request_BindingStyleDefaultWithObjectParams_DeserializeSuccessfully()
    {
        const string requestJson = $$"""
            {
                "method": "binding_style_default",
                "id": "123",
                "params": {
                    "data": {{TestData.PlainRequiredSnakeCaseJson}}
                },
                "jsonrpc": "2.0"
            }
            """;
        var expectedRequestData = TestData.Plain;
        var responseData = TestData.Plain;
        var expectedResponseJson = $$"""
            {
                "id": "123",
                "result": {{TestData.PlainFullSnakeCaseJson}},
                "jsonrpc": "2.0"
            }
            """.TrimAllLines();

        TestData actualRequestData = null;
        requestValidatorMock.Setup(static v => v.Validate(It.IsAny<TestData>()))
            .Callback<TestData>(requestData => actualRequestData = requestData);
        responseProviderMock.Setup(static p => p.GetJsonRpcResponse())
            .Returns(responseData);

        using var request = new StringContent(requestJson, Encoding.UTF8, "application/json");
        var response = await ApiClient.PostAsync(JsonRpcUrl, request);
        var actualResponseJson = (await response.Content.ReadAsStringAsync()).TrimAllLines();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        actualRequestData.Should().BeEquivalentTo(expectedRequestData);
        actualResponseJson.Should().Be(expectedResponseJson);
    }

    [Test]
    public async Task Request_BindingStyleDefaultWithArrayParams_DeserializeSuccessfully()
    {
        const string requestJson = $$"""
            {
                "method": "binding_style_default",
                "id": "123",
                "params": [
                    {{TestData.PlainRequiredSnakeCaseJson}}
                ],
                "jsonrpc": "2.0"
            }
            """;
        var expectedRequestData = TestData.Plain;
        var responseData = TestData.Plain;
        var expectedResponseJson = $$"""
            {
                "id": "123",
                "result": {{TestData.PlainFullSnakeCaseJson}},
                "jsonrpc": "2.0"
            }
            """.TrimAllLines();

        TestData actualRequestData = null;
        requestValidatorMock.Setup(static v => v.Validate(It.IsAny<TestData>()))
            .Callback<TestData>(requestData => actualRequestData = requestData);
        responseProviderMock.Setup(static p => p.GetJsonRpcResponse())
            .Returns(responseData);

        using var request = new StringContent(requestJson, Encoding.UTF8, "application/json");
        var response = await ApiClient.PostAsync(JsonRpcUrl, request);
        var actualResponseJson = (await response.Content.ReadAsStringAsync()).TrimAllLines();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        actualRequestData.Should().BeEquivalentTo(expectedRequestData);
        actualResponseJson.Should().Be(expectedResponseJson);
    }

    [Test]
    public async Task Request_BindingStyleDefaultWithNullParams_()
    {}

    [Test]
    public async Task Request_BindingStyleDefaultWithoutParams_()
    {}

    [Test]
    public async Task Request_BindingStyleDefaultWithEmptyObjectParams_()
    {}

    [Test]
    public async Task Request_BindingStyleDefaultWithEmptyArrayParams_()
    {}

    [Test]
    public async Task Request_BindingStyleObject_DeserializeSuccessfully()
    {
        const string requestJson = $$"""
            {
                "method": "binding_style_object",
                "id": "123",
                "params": {{TestData.PlainRequiredSnakeCaseJson}},
                "jsonrpc": "2.0"
            }
            """;
        var expectedRequestData = TestData.Plain;
        var responseData = TestData.Plain;
        var expectedResponseJson = $$"""
            {
                "id": "123",
                "result": {{TestData.PlainFullSnakeCaseJson}},
                "jsonrpc": "2.0"
            }
            """.TrimAllLines();

        TestData actualRequestData = null;
        requestValidatorMock.Setup(static v => v.Validate(It.IsAny<TestData>()))
            .Callback<TestData>(requestData => actualRequestData = requestData);
        responseProviderMock.Setup(static p => p.GetJsonRpcResponse())
            .Returns(responseData);

        using var request = new StringContent(requestJson, Encoding.UTF8, "application/json");
        var response = await ApiClient.PostAsync(JsonRpcUrl, request);
        var actualResponseJson = (await response.Content.ReadAsStringAsync()).TrimAllLines();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        actualRequestData.Should().BeEquivalentTo(expectedRequestData);
        actualResponseJson.Should().Be(expectedResponseJson);
    }

    [Test]
    public async Task Request_BindingStyleObjectWithNullParams_()
    {}

    [Test]
    public async Task Request_BindingStyleObjectWithoutParams_()
    {}

    [Test]
    public async Task Request_BindingStyleObjectWithEmptyObjectParams_()
    {}

    [Test]
    public async Task Request_BindingStyleObjectWithEmptyArrayParams_()
    {}

    [Test]
    public async Task Request_BindingStyleArray_DeserializeSuccessfully()
    {
        const string requestJson = $$"""
            {
                "method": "binding_style_array",
                "id": "123",
                "params": [
                    {{TestData.PlainRequiredSnakeCaseJson}}
                ],
                "jsonrpc": "2.0"
            }
            """;
        var expectedRequestData = TestData.Plain;
        var responseData = TestData.Plain;
        var expectedResponseJson = $$"""
            {
                "id": "123",
                "result": {{TestData.PlainFullSnakeCaseJson}},
                "jsonrpc": "2.0"
            }
            """.TrimAllLines();

        TestData actualRequestData = null;
        requestValidatorMock.Setup(static v => v.Validate(It.IsAny<TestData>()))
            .Callback<TestData>(requestData => actualRequestData = requestData);
        responseProviderMock.Setup(static p => p.GetJsonRpcResponse())
            .Returns(responseData);

        using var request = new StringContent(requestJson, Encoding.UTF8, "application/json");
        var response = await ApiClient.PostAsync(JsonRpcUrl, request);
        var actualResponseJson = (await response.Content.ReadAsStringAsync()).TrimAllLines();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        actualRequestData.Should().BeEquivalentTo(expectedRequestData);
        actualResponseJson.Should().Be(expectedResponseJson);
    }

    [Test]
    public async Task Request_BindingStyleArrayWithNullParams_()
    {}

    [Test]
    public async Task Request_BindingStyleArrayWithoutParams_()
    {}

    [Test]
    public async Task Request_BindingStyleArrayWithEmptyObjectParams_()
    {}

    [Test]
    public async Task Request_BindingStyleArrayWithEmptyArrayParams_()
    {}

    [Test]
    public async Task Request_NullableDefaultParamsWithNullParams_()
    {}

    [Test]
    public async Task Request_NullableDefaultParamsWithoutParams_()
    {}

    [Test]
    public async Task Request_NullableDefaultParamsWithEmptyObjectParams_()
    {}

    [Test]
    public async Task Request_NullableDefaultParamsWithEmptyArrayParams_()
    {}

    [Test]
    public async Task Request_NullableObjectParamsWithNullParams_()
    {}

    [Test]
    public async Task Request_NullableObjectParamsWithoutParams_()
    {}

    [Test]
    public async Task Request_NullableObjectParamsWithEmptyObjectParams_()
    {}

    [Test]
    public async Task Request_NullableObjectParamsWithEmptyArrayParams_()
    {}

    [Test]
    public async Task Request_NullableArrayParamsWithNullParams_()
    {}

    [Test]
    public async Task Request_NullableArrayParamsWithoutParams_()
    {}

    [Test]
    public async Task Request_NullableArrayParamsWithEmptyObjectParams_()
    {}

    [Test]
    public async Task Request_NullableArrayParamsWithEmptyArrayParams_()
    {}

    [Test]
    public async Task Request_DefaultParamsWithNullParams_()
    {}

    [Test]
    public async Task Request_DefaultParamsWithoutParams_()
    {}

    [Test]
    public async Task Request_DefaultParamsWithEmptyObjectParams_()
    {}

    [Test]
    public async Task Request_DefaultParamsWithEmptyArrayParams_()
    {}

    [Test]
    public async Task Request_DefaultObjectParamsWithNullParams_()
    {}

    [Test]
    public async Task Request_DefaultObjectParamsWithoutParams_()
    {}

    [Test]
    public async Task Request_DefaultObjectParamsWithEmptyObjectParams_()
    {}

    [Test]
    public async Task Request_DefaultObjectParamsWithEmptyArrayParams_()
    {}

    [Test]
    public async Task Request_DefaultArrayParamsWithNullParams_()
    {}

    [Test]
    public async Task Request_DefaultArrayParamsWithoutParams_()
    {}

    [Test]
    public async Task Request_DefaultArrayParamsWithEmptyObjectParams_()
    {}

    [Test]
    public async Task Request_DefaultArrayParamsWithEmptyArrayParams_()
    {}

    [Test]
    public async Task Request_NoParams_ProcessSuccessfully()
    {
        const string requestJson = """
            {
                "method": "no_params",
                "id": "123",
                "jsonrpc": "2.0"
            }
            """;
        var responseData = TestData.Plain;
        var expectedResponseJson = $$"""
            {
                "id": "123",
                "result": {{TestData.PlainFullSnakeCaseJson}},
                "jsonrpc": "2.0"
            }
            """.TrimAllLines();

        responseProviderMock.Setup(static p => p.GetJsonRpcResponse())
            .Returns(responseData);

        using var request = new StringContent(requestJson, Encoding.UTF8, "application/json");
        var response = await ApiClient.PostAsync(JsonRpcUrl, request);
        var actualResponseJson = (await response.Content.ReadAsStringAsync()).TrimAllLines();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        actualResponseJson.Should().Be(expectedResponseJson);
    }

    [Test]
    public async Task Request_SnakeCaseParams_DeserializeSuccessfully()
    {
        const string requestJson = $$"""
            {
                "method": "snake_case_params",
                "id": "123",
                "params": {{TestData.PlainRequiredSnakeCaseJson}},
                "jsonrpc": "2.0"
            }
            """;
        var expectedRequestData = TestData.Plain;
        var responseData = TestData.Plain;
        var expectedResponseJson = $$"""
            {
                "id": "123",
                "result": {{TestData.PlainFullSnakeCaseJson}},
                "jsonrpc": "2.0"
            }
            """.TrimAllLines();

        TestData actualRequestData = null;
        requestValidatorMock.Setup(static v => v.Validate(It.IsAny<TestData>()))
            .Callback<TestData>(requestData => actualRequestData = requestData);
        responseProviderMock.Setup(static p => p.GetJsonRpcResponse())
            .Returns(responseData);

        using var request = new StringContent(requestJson, Encoding.UTF8, "application/json");
        var response = await ApiClient.PostAsync(JsonRpcUrl, request);
        var actualResponseJson = (await response.Content.ReadAsStringAsync()).TrimAllLines();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        actualRequestData.Should().BeEquivalentTo(expectedRequestData);
        actualResponseJson.Should().Be(expectedResponseJson);
    }

    [Test]
    public async Task Request_CamelCaseParams_DeserializeSuccessfully()
    {
        const string requestJson = $$"""
            {
                "method": "camelCaseParams",
                "id": "123",
                "params": {{TestData.PlainRequiredCamelCaseJson}},
                "jsonrpc": "2.0"
            }
            """;
        var expectedRequestData = TestData.Plain;
        var responseData = TestData.Plain;
        var expectedResponseJson = $$"""
            {
                "id": "123",
                "result": {{TestData.PlainFullCamelCaseJson}},
                "jsonrpc": "2.0"
            }
            """.TrimAllLines();

        TestData actualRequestData = null;
        requestValidatorMock.Setup(static v => v.Validate(It.IsAny<TestData>()))
            .Callback<TestData>(requestData => actualRequestData = requestData);
        responseProviderMock.Setup(static p => p.GetJsonRpcResponse())
            .Returns(responseData);

        using var request = new StringContent(requestJson, Encoding.UTF8, "application/json");
        var response = await ApiClient.PostAsync(JsonRpcUrl, request);
        var actualResponseJson = (await response.Content.ReadAsStringAsync()).TrimAllLines();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        actualRequestData.Should().BeEquivalentTo(expectedRequestData);
        actualResponseJson.Should().Be(expectedResponseJson);
    }

    [Test]
    public async Task Request_NestedSnakeCaseParams_DeserializeSuccessfully()
    {
        const string requestJson = $$"""
            {
                "method": "snake_case_params",
                "id": "123",
                "params": {{TestData.NestedRequiredSnakeCaseJson}},
                "jsonrpc": "2.0"
            }
            """;
        var expectedRequestData = TestData.Nested;
        var responseData = TestData.Nested;
        var expectedResponseJson = $$"""
            {
                "id": "123",
                "result": {{TestData.NestedFullSnakeCaseJson}},
                "jsonrpc": "2.0"
            }
            """.TrimAllLines();

        TestData actualRequestData = null;
        requestValidatorMock.Setup(static v => v.Validate(It.IsAny<TestData>()))
            .Callback<TestData>(requestData => actualRequestData = requestData);
        responseProviderMock.Setup(static p => p.GetJsonRpcResponse())
            .Returns(responseData);

        using var request = new StringContent(requestJson, Encoding.UTF8, "application/json");
        var response = await ApiClient.PostAsync(JsonRpcUrl, request);
        var actualResponseJson = (await response.Content.ReadAsStringAsync()).TrimAllLines();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        actualRequestData.Should().BeEquivalentTo(expectedRequestData);
        actualResponseJson.Should().Be(expectedResponseJson);
    }

    [Test]
    public async Task Request_NestedCamelCaseParams_DeserializeSuccessfully()
    {
        const string requestJson = $$"""
            {
                "method": "camelCaseParams",
                "id": "123",
                "params": {{TestData.NestedRequiredCamelCaseJson}},
                "jsonrpc": "2.0"
            }
            """;
        var expectedRequestData = TestData.Nested;
        var responseData = TestData.Nested;
        var expectedResponseJson = $$"""
            {
                "id": "123",
                "result": {{TestData.NestedFullCamelCaseJson}},
                "jsonrpc": "2.0"
            }
            """.TrimAllLines();

        TestData actualRequestData = null;
        requestValidatorMock.Setup(static v => v.Validate(It.IsAny<TestData>()))
            .Callback<TestData>(requestData => actualRequestData = requestData);
        responseProviderMock.Setup(static p => p.GetJsonRpcResponse())
            .Returns(responseData);

        using var request = new StringContent(requestJson, Encoding.UTF8, "application/json");
        var response = await ApiClient.PostAsync(JsonRpcUrl, request);
        var actualResponseJson = (await response.Content.ReadAsStringAsync()).TrimAllLines();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        actualRequestData.Should().BeEquivalentTo(expectedRequestData);
        actualResponseJson.Should().Be(expectedResponseJson);
    }

    [Test]
    public async Task Request_AdditionalParams_DeserializeSuccessfully()
    {
        const string requestJson = """
            {
                "method": "process_anything",
                "id": "123",
                "params": {
                    "bool_field": true,
                    "string_field": "123",
                    "int_field": 123,
                    "double_field": 1.23,
                    "enum_field": "two",
                    "array_field": [
                        1,
                        2,
                        3
                    ],
                    "nullable_field": null,
                    "additional_field": "something"
                },
                "jsonrpc": "2.0"
            }
            """;
        var expectedRequestData = TestData.Plain;
        var responseData = TestData.Plain;
        var expectedResponseJson = $$"""
            {
                "id": "123",
                "result": {{TestData.PlainFullSnakeCaseJson}},
                "jsonrpc": "2.0"
            }
            """.TrimAllLines();

        TestData actualRequestData = null;
        requestValidatorMock.Setup(static v => v.Validate(It.IsAny<TestData>()))
            .Callback<TestData>(requestData => actualRequestData = requestData);
        responseProviderMock.Setup(static p => p.GetJsonRpcResponse())
            .Returns(responseData);

        using var request = new StringContent(requestJson, Encoding.UTF8, "application/json");
        var response = await ApiClient.PostAsync(JsonRpcUrl, request);
        var actualResponseJson = (await response.Content.ReadAsStringAsync()).TrimAllLines();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        actualRequestData.Should().BeEquivalentTo(expectedRequestData);
        actualResponseJson.Should().Be(expectedResponseJson);
    }

    [Test]
    public async Task Request_CustomActionRoute_DeserializeSuccessfully()
    {
        const string requestJson = $$"""
            {
                "method": "custom_action_route",
                "id": "123",
                "params": {{TestData.PlainRequiredSnakeCaseJson}},
                "jsonrpc": "2.0"
            }
            """;
        var expectedRequestData = TestData.Plain;
        var responseData = TestData.Plain;
        var expectedResponseJson = $$"""
            {
                "id": "123",
                "result": {{TestData.PlainFullSnakeCaseJson}},
                "jsonrpc": "2.0"
            }
            """.TrimAllLines();

        TestData actualRequestData = null;
        requestValidatorMock.Setup(static v => v.Validate(It.IsAny<TestData>()))
            .Callback<TestData>(requestData => actualRequestData = requestData);
        responseProviderMock.Setup(static p => p.GetJsonRpcResponse())
            .Returns(responseData);

        using var request = new StringContent(requestJson, Encoding.UTF8, "application/json");
        var response = await ApiClient.PostAsync($"{JsonRpcUrl}/custom/action", request);
        var actualResponseJson = (await response.Content.ReadAsStringAsync()).TrimAllLines();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        actualRequestData.Should().BeEquivalentTo(expectedRequestData);
        actualResponseJson.Should().Be(expectedResponseJson);
    }

    [Test]
    public async Task Request_CustomControllerRoute_DeserializeSuccessfully()
    {
        const string requestJson = $$"""
            {
                "method": "custom_controller_route",
                "id": "123",
                "params": {{TestData.PlainRequiredSnakeCaseJson}},
                "jsonrpc": "2.0"
            }
            """;
        var expectedRequestData = TestData.Plain;
        var responseData = TestData.Plain;
        var expectedResponseJson = $$"""
            {
                "id": "123",
                "result": {{TestData.PlainFullSnakeCaseJson}},
                "jsonrpc": "2.0"
            }
            """.TrimAllLines();

        TestData actualRequestData = null;
        requestValidatorMock.Setup(static v => v.Validate(It.IsAny<TestData>()))
            .Callback<TestData>(requestData => actualRequestData = requestData);
        responseProviderMock.Setup(static p => p.GetJsonRpcResponse())
            .Returns(responseData);

        using var request = new StringContent(requestJson, Encoding.UTF8, "application/json");
        var response = await ApiClient.PostAsync($"{JsonRpcUrl}/custom/controller", request);
        var actualResponseJson = (await response.Content.ReadAsStringAsync()).TrimAllLines();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        actualRequestData.Should().BeEquivalentTo(expectedRequestData);
        actualResponseJson.Should().Be(expectedResponseJson);
    }

    [Test]
    public async Task Request_ThrowException_SerializeSuccessfully()
    {
        const string requestJson = """
            {
                "method": "throw_exception",
                "id": "123",
                "jsonrpc": "2.0"
            }
            """;
        var expectedResponseJson = """
            {
                "id": "123",
                "error": {
                    "code": -32000,
                    "message": "Server error",
                    "data": {
                        "type": "System.ArgumentException",
                        "message": "Value does not fall within the expected range.",
                        "details": null
                    }
                },
                "jsonrpc": "2.0"
            }
            """.TrimAllLines();

        using var request = new StringContent(requestJson, Encoding.UTF8, "application/json");
        var response = await ApiClient.PostAsync(JsonRpcUrl, request);
        var actualResponseJson = (await response.Content.ReadAsStringAsync()).TrimAllLines();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        actualResponseJson.Should().Be(expectedResponseJson);
    }

    [Test]
    public async Task Request_ReturnErrorFromFactory_SerializeSuccessfully()
    {
        const string requestJson = """
            {
                "method": "return_error_from_factory",
                "id": "123",
                "jsonrpc": "2.0"
            }
            """;
        var expectedResponseJson = """
            {
                "id": "123",
                "error": {
                    "code": -32602,
                    "message": "Invalid params",
                    "data": "errorMessage"
                },
                "jsonrpc": "2.0"
            }
            """.TrimAllLines();

        using var request = new StringContent(requestJson, Encoding.UTF8, "application/json");
        var response = await ApiClient.PostAsync(JsonRpcUrl, request);
        var actualResponseJson = (await response.Content.ReadAsStringAsync()).TrimAllLines();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        actualResponseJson.Should().Be(expectedResponseJson);
    }

    [Test]
    public async Task Request_ReturnMvcError_SerializeSuccessfully()
    {
        const string requestJson = """
            {
                "method": "return_mvc_error",
                "id": "123",
                "jsonrpc": "2.0"
            }
            """;
        var expectedResponseJson = """
            {
                "id": "123",
                "error": {
                    "code": -32602,
                    "message": "Invalid params",
                    "data": "errorMessage"
                },
                "jsonrpc": "2.0"
            }
            """.TrimAllLines();

        using var request = new StringContent(requestJson, Encoding.UTF8, "application/json");
        var response = await ApiClient.PostAsync(JsonRpcUrl, request);
        var actualResponseJson = (await response.Content.ReadAsStringAsync()).TrimAllLines();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        actualResponseJson.Should().Be(expectedResponseJson);
    }

    [Test]
    public async Task Request_ErrorThrowAsResponseException_SerializeSuccessfully()
    {
        const string requestJson = """
            {
                "method": "error_throw_as_response_exception",
                "id": "123",
                "jsonrpc": "2.0"
            }
            """;
        var expectedResponseJson = """
            {
                "id": "123",
                "error": {
                    "code": -32602,
                    "message": "Invalid params",
                    "data": "errorMessage"
                },
                "jsonrpc": "2.0"
            }
            """.TrimAllLines();

        using var request = new StringContent(requestJson, Encoding.UTF8, "application/json");
        var response = await ApiClient.PostAsync(JsonRpcUrl, request);
        var actualResponseJson = (await response.Content.ReadAsStringAsync()).TrimAllLines();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        actualResponseJson.Should().Be(expectedResponseJson);
    }

    [Test]
    public async Task Request_UnknownMethod_ReturnError()
    {
        const string requestJson = """
            {
                "method": "some_not_existing_method",
                "id": "123",
                "jsonrpc": "2.0"
            }
            """;
        var expectedResponseJson = """
            {
                "id": "123",
                "error": {
                    "code": -32601,
                    "message": "Method not found",
                    "data": {
                        "method": "some_not_existing_method"
                    }
                },
                "jsonrpc": "2.0"
            }
            """.TrimAllLines();

        using var request = new StringContent(requestJson, Encoding.UTF8, "application/json");
        var response = await ApiClient.PostAsync(JsonRpcUrl, request);
        var actualResponseJson = (await response.Content.ReadAsStringAsync()).TrimAllLines();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        actualResponseJson.Should().Be(expectedResponseJson);
    }

    [Test]
    public async Task Request_NoMethod_ReturnError()
    {
        const string requestJson = $$"""
            {
                "id": "123",
                "params": {{TestData.PlainRequiredSnakeCaseJson}},
                "jsonrpc": "2.0"
            }
            """;
        var expectedResponseJson = """
            {
                "id": null,
                "error": {
                    "code": -32600,
                    "message": "Invalid Request",
                    "data": {
                        "type": "Tochka.JsonRpc.Common.JsonRpcFormatException",
                        "message": "JSON Rpc call does not have [method] property",
                        "details": null
                    }
                },
                "jsonrpc": "2.0"
            }
            """.TrimAllLines();

        using var request = new StringContent(requestJson, Encoding.UTF8, "application/json");
        var response = await ApiClient.PostAsync(JsonRpcUrl, request);
        var actualResponseJson = (await response.Content.ReadAsStringAsync()).TrimAllLines();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        actualResponseJson.Should().Be(expectedResponseJson);
    }

    [Test]
    public async Task Request_MethodNull_ReturnError()
    {
        const string requestJson = $$"""
            {
                "method": null,
                "id": "123",
                "params": {{TestData.PlainRequiredSnakeCaseJson}},
                "jsonrpc": "2.0"
            }
            """;
        var expectedResponseJson = """
            {
                "id": "123",
                "error": {
                    "code": -32600,
                    "message": "Invalid Request",
                    "data": {
                        "type": "Tochka.JsonRpc.Common.JsonRpcFormatException",
                        "message": "Method is null or empty",
                        "details": null
                    }
                },
                "jsonrpc": "2.0"
            }
            """.TrimAllLines();

        using var request = new StringContent(requestJson, Encoding.UTF8, "application/json");
        var response = await ApiClient.PostAsync(JsonRpcUrl, request);
        var actualResponseJson = (await response.Content.ReadAsStringAsync()).TrimAllLines();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        actualResponseJson.Should().Be(expectedResponseJson);
    }

    [Test]
    public async Task Request_MethodEmpty_ReturnError()
    {
        const string requestJson = $$"""
            {
                "method": "",
                "id": "123",
                "params": {{TestData.PlainRequiredSnakeCaseJson}},
                "jsonrpc": "2.0"
            }
            """;
        var expectedResponseJson = """
            {
                "id": "123",
                "error": {
                    "code": -32600,
                    "message": "Invalid Request",
                    "data": {
                        "type": "Tochka.JsonRpc.Common.JsonRpcFormatException",
                        "message": "Method is null or empty",
                        "details": null
                    }
                },
                "jsonrpc": "2.0"
            }
            """.TrimAllLines();

        using var request = new StringContent(requestJson, Encoding.UTF8, "application/json");
        var response = await ApiClient.PostAsync(JsonRpcUrl, request);
        var actualResponseJson = (await response.Content.ReadAsStringAsync()).TrimAllLines();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        actualResponseJson.Should().Be(expectedResponseJson);
    }

    [Test]
    public async Task Request_MethodWhiteSpace_ReturnError()
    {
        const string requestJson = $$"""
            {
                "method": " ",
                "id": "123",
                "params": {{TestData.PlainRequiredSnakeCaseJson}},
                "jsonrpc": "2.0"
            }
            """;
        var expectedResponseJson = """
            {
                "id": "123",
                "error": {
                    "code": -32600,
                    "message": "Invalid Request",
                    "data": {
                        "type": "Tochka.JsonRpc.Common.JsonRpcFormatException",
                        "message": "Method is null or empty",
                        "details": null
                    }
                },
                "jsonrpc": "2.0"
            }
            """.TrimAllLines();

        using var request = new StringContent(requestJson, Encoding.UTF8, "application/json");
        var response = await ApiClient.PostAsync(JsonRpcUrl, request);
        var actualResponseJson = (await response.Content.ReadAsStringAsync()).TrimAllLines();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        actualResponseJson.Should().Be(expectedResponseJson);
    }

    [Test]
    public async Task Request_NoRequiredParams_ReturnError()
    {
        const string requestJson = """
            {
                "method": "process_anything",
                "id": "123",
                "jsonrpc": "2.0"
            }
            """;
        var expectedResponseJson = """
            {
                "id": "123",
                "error": {
                    "code": -32602,
                    "message": "Invalid params",
                    "data": {
                        "data": [
                            "Bind value not found (expected JSON key = [data])"
                        ]
                    }
                },
                "jsonrpc": "2.0"
            }
            """.TrimAllLines();

        using var request = new StringContent(requestJson, Encoding.UTF8, "application/json");
        var response = await ApiClient.PostAsync(JsonRpcUrl, request);
        var actualResponseJson = (await response.Content.ReadAsStringAsync()).TrimAllLines();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        actualResponseJson.Should().Be(expectedResponseJson);
    }

    [Test]
    public async Task Request_EmptyRequiredParams_ReturnError()
    {
        const string requestJson = """
            {
                "method": "process_anything",
                "id": "123",
                "params": {},
                "jsonrpc": "2.0"
            }
            """;
        var expectedResponseJson = """
            {
                "id": "123",
                "error": {
                    "code": -32602,
                    "message": "Invalid params",
                    "data": {
                        "ArrayField": [
                            "The ArrayField field is required."
                        ],
                        "StringField": [
                            "The StringField field is required."
                        ]
                    }
                },
                "jsonrpc": "2.0"
            }
            """.TrimAllLines();

        using var request = new StringContent(requestJson, Encoding.UTF8, "application/json");
        var response = await ApiClient.PostAsync(JsonRpcUrl, request);
        var actualResponseJson = (await response.Content.ReadAsStringAsync()).TrimAllLines();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        actualResponseJson.Should().Be(expectedResponseJson);
    }

    [Test]
    public async Task Request_RequiredParamsNull_ReturnError()
    {
        const string requestJson = """
            {
                "method": "process_anything",
                "id": "123",
                "params": null,
                "jsonrpc": "2.0"
            }
            """;
        var expectedResponseJson = """
            {
                "id": "123",
                "error": {
                    "code": -32602,
                    "message": "Invalid params",
                    "data": {
                        "data": [
                            "Bind value not found (expected JSON key = [data])"
                        ]
                    }
                },
                "jsonrpc": "2.0"
            }
            """.TrimAllLines();

        using var request = new StringContent(requestJson, Encoding.UTF8, "application/json");
        var response = await ApiClient.PostAsync(JsonRpcUrl, request);
        var actualResponseJson = (await response.Content.ReadAsStringAsync()).TrimAllLines();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        actualResponseJson.Should().Be(expectedResponseJson);
    }

    [Test]
    public async Task Request_InvalidJsonRpcVersion_ReturnError() // Should id be null?
    {
        const string requestJson = $$"""
            {
                "method": "process_anything",
                "id": "123",
                "params": {{TestData.PlainRequiredSnakeCaseJson}},
                "jsonrpc": "3.0"
            }
            """;
        var expectedResponseJson = """
            {
                "id": "123",
                "error": {
                    "code": -32600,
                    "message": "Invalid Request",
                    "data": {
                        "type": "Tochka.JsonRpc.Common.JsonRpcFormatException",
                        "message": "Only [2.0] version supported. Got [3.0]",
                        "details": null
                    }
                },
                "jsonrpc": "2.0"
            }
            """.TrimAllLines();

        using var request = new StringContent(requestJson, Encoding.UTF8, "application/json");
        var response = await ApiClient.PostAsync(JsonRpcUrl, request);
        var actualResponseJson = (await response.Content.ReadAsStringAsync()).TrimAllLines();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        actualResponseJson.Should().Be(expectedResponseJson);
    }

    [Test]
    public async Task Request_InvalidJson_ReturnError()
    {
        const string requestJson = $$"""
            {
                123
                "method": "process_anything",
                "id": "123",
                "params": {{TestData.PlainRequiredSnakeCaseJson}},
                "jsonrpc": "2.0"
            }
            """;
        var expectedResponseJson = """
            {
                "id": null,
                "error": {
                    "code": -32700,
                    "message": "Parse error",
                    "data": {
                        "type": "System.Text.Json.JsonException",
                        "message": "\u00271\u0027 is an invalid start of a property name. Expected a \u0027\u0022\u0027. Path: $ | LineNumber: 1 | BytePositionInLine: 4.",
                        "details": null
                    }
                },
                "jsonrpc": "2.0"
            }
            """.TrimAllLines();

        using var request = new StringContent(requestJson, Encoding.UTF8, "application/json");
        var response = await ApiClient.PostAsync(JsonRpcUrl, request);
        var actualResponseJson = (await response.Content.ReadAsStringAsync()).TrimAllLines();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        actualResponseJson.Should().Be(expectedResponseJson);
    }

    [Test]
    public async Task Request_NoJsonRpc_ReturnError()
    {
        const string requestJson = $$"""
            {
                "method": "process_anything",
                "id": "123",
                "params": {{TestData.PlainRequiredSnakeCaseJson}}
            }
            """;
        var expectedResponseJson = """
            {
                "id": null,
                "error": {
                    "code": -32600,
                    "message": "Invalid Request",
                    "data": {
                        "type": "Tochka.JsonRpc.Common.JsonRpcFormatException",
                        "message": "JSON Rpc call does not have [jsonrpc] property",
                        "details": null
                    }
                },
                "jsonrpc": "2.0"
            }
            """.TrimAllLines();

        using var request = new StringContent(requestJson, Encoding.UTF8, "application/json");
        var response = await ApiClient.PostAsync(JsonRpcUrl, request);
        var actualResponseJson = (await response.Content.ReadAsStringAsync()).TrimAllLines();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        actualResponseJson.Should().Be(expectedResponseJson);
    }

    [Test]
    public async Task Batch_StringId_DeserializeSuccessfully()
    {
        const string requestJson = $$"""
            [
                {
                    "method": "process_anything",
                    "id": "123",
                    "params": {{TestData.PlainRequiredSnakeCaseJson}},
                    "jsonrpc": "2.0"
                },
                {
                    "method": "process_anything",
                    "id": "456",
                    "params": {{TestData.PlainRequiredSnakeCaseJson}},
                    "jsonrpc": "2.0"
                }
            ]
            """;
        var expectedRequestData = TestData.Plain;
        var responseData = TestData.Plain;
        var expectedResponseJson = $$"""
            [
                {
                    "id": "123",
                    "result": {{TestData.PlainFullSnakeCaseJson}},
                    "jsonrpc": "2.0"
                },
                {
                    "id": "456",
                    "result": {{TestData.PlainFullSnakeCaseJson}},
                    "jsonrpc": "2.0"
                }
            ]
            """.TrimAllLines();

        var actualRequestData = new List<TestData>();
        requestValidatorMock.Setup(static v => v.Validate(It.IsAny<TestData>()))
            .Callback<TestData>(requestData => actualRequestData.Add(requestData));
        responseProviderMock.Setup(static p => p.GetJsonRpcResponse())
            .Returns(responseData);

        using var request = new StringContent(requestJson, Encoding.UTF8, "application/json");
        var response = await ApiClient.PostAsync(JsonRpcUrl, request);
        var actualResponseJson = (await response.Content.ReadAsStringAsync()).TrimAllLines();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        actualRequestData.Should().AllBeEquivalentTo(expectedRequestData);
        actualResponseJson.Should().Be(expectedResponseJson);
    }

    [Test]
    public async Task Batch_IntId_DeserializeSuccessfully()
    {
        const string requestJson = $$"""
            [
                {
                    "method": "process_anything",
                    "id": 123,
                    "params": {{TestData.PlainRequiredSnakeCaseJson}},
                    "jsonrpc": "2.0"
                },
                {
                    "method": "process_anything",
                    "id": 456,
                    "params": {{TestData.PlainRequiredSnakeCaseJson}},
                    "jsonrpc": "2.0"
                }
            ]
            """;
        var expectedRequestData = TestData.Plain;
        var responseData = TestData.Plain;
        var expectedResponseJson = $$"""
            [
                {
                    "id": 123,
                    "result": {{TestData.PlainFullSnakeCaseJson}},
                    "jsonrpc": "2.0"
                },
                {
                    "id": 456,
                    "result": {{TestData.PlainFullSnakeCaseJson}},
                    "jsonrpc": "2.0"
                }
            ]
            """.TrimAllLines();

        var actualRequestData = new List<TestData>();
        requestValidatorMock.Setup(static v => v.Validate(It.IsAny<TestData>()))
            .Callback<TestData>(requestData => actualRequestData.Add(requestData));
        responseProviderMock.Setup(static p => p.GetJsonRpcResponse())
            .Returns(responseData);

        using var request = new StringContent(requestJson, Encoding.UTF8, "application/json");
        var response = await ApiClient.PostAsync(JsonRpcUrl, request);
        var actualResponseJson = (await response.Content.ReadAsStringAsync()).TrimAllLines();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        actualRequestData.Should().AllBeEquivalentTo(expectedRequestData);
        actualResponseJson.Should().Be(expectedResponseJson);
    }

    [Test]
    public async Task Batch_NullId_DeserializeSuccessfully()
    {
        const string requestJson = $$"""
            [
                {
                    "method": "process_anything",
                    "id": null,
                    "params": {{TestData.PlainRequiredSnakeCaseJson}},
                    "jsonrpc": "2.0"
                }
            ]
            """;
        var expectedRequestData = TestData.Plain;
        var responseData = TestData.Plain;
        var expectedResponseJson = $$"""
            [
                {
                    "id": null,
                    "result": {{TestData.PlainFullSnakeCaseJson}},
                    "jsonrpc": "2.0"
                }
            ]
            """.TrimAllLines();

        var actualRequestData = new List<TestData>();
        requestValidatorMock.Setup(static v => v.Validate(It.IsAny<TestData>()))
            .Callback<TestData>(requestData => actualRequestData.Add(requestData));
        responseProviderMock.Setup(static p => p.GetJsonRpcResponse())
            .Returns(responseData);

        using var request = new StringContent(requestJson, Encoding.UTF8, "application/json");
        var response = await ApiClient.PostAsync(JsonRpcUrl, request);
        var actualResponseJson = (await response.Content.ReadAsStringAsync()).TrimAllLines();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        actualRequestData.Should().AllBeEquivalentTo(expectedRequestData);
        actualResponseJson.Should().Be(expectedResponseJson);
    }

    [Test]
    public async Task Batch_ActionOnly_DeserializeSuccessfully()
    {
        const string requestJson = $$"""
            [
                {
                    "method": "action_only",
                    "id": "123",
                    "params": {{TestData.PlainRequiredSnakeCaseJson}},
                    "jsonrpc": "2.0"
                },
                {
                    "method": "action_only",
                    "id": "456",
                    "params": {{TestData.PlainRequiredSnakeCaseJson}},
                    "jsonrpc": "2.0"
                }
            ]
            """;
        var expectedRequestData = TestData.Plain;
        var responseData = TestData.Plain;
        var expectedResponseJson = $$"""
            [
                {
                    "id": "123",
                    "result": {{TestData.PlainFullSnakeCaseJson}},
                    "jsonrpc": "2.0"
                },
                {
                    "id": "456",
                    "result": {{TestData.PlainFullSnakeCaseJson}},
                    "jsonrpc": "2.0"
                }
            ]
            """.TrimAllLines();

        var actualRequestData = new List<TestData>();
        requestValidatorMock.Setup(static v => v.Validate(It.IsAny<TestData>()))
            .Callback<TestData>(requestData => actualRequestData.Add(requestData));
        responseProviderMock.Setup(static p => p.GetJsonRpcResponse())
            .Returns(responseData);

        using var request = new StringContent(requestJson, Encoding.UTF8, "application/json");
        var response = await ApiClient.PostAsync(JsonRpcUrl, request);
        var actualResponseJson = (await response.Content.ReadAsStringAsync()).TrimAllLines();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        actualRequestData.Should().AllBeEquivalentTo(expectedRequestData);
        actualResponseJson.Should().Be(expectedResponseJson);
    }

    [Test]
    public async Task Batch_ControllerAndAction_DeserializeSuccessfully()
    {
        const string requestJson = $$"""
            [
                {
                    "method": "simple_json_rpc.controller_and_action",
                    "id": "123",
                    "params": {{TestData.PlainRequiredSnakeCaseJson}},
                    "jsonrpc": "2.0"
                },
                {
                    "method": "simple_json_rpc.controller_and_action",
                    "id": "456",
                    "params": {{TestData.PlainRequiredSnakeCaseJson}},
                    "jsonrpc": "2.0"
                }
            ]
            """;
        var expectedRequestData = TestData.Plain;
        var responseData = TestData.Plain;
        var expectedResponseJson = $$"""
            [
                {
                    "id": "123",
                    "result": {{TestData.PlainFullSnakeCaseJson}},
                    "jsonrpc": "2.0"
                },
                {
                    "id": "456",
                    "result": {{TestData.PlainFullSnakeCaseJson}},
                    "jsonrpc": "2.0"
                }
            ]
            """.TrimAllLines();

        var actualRequestData = new List<TestData>();
        requestValidatorMock.Setup(static v => v.Validate(It.IsAny<TestData>()))
            .Callback<TestData>(requestData => actualRequestData.Add(requestData));
        responseProviderMock.Setup(static p => p.GetJsonRpcResponse())
            .Returns(responseData);

        using var request = new StringContent(requestJson, Encoding.UTF8, "application/json");
        var response = await ApiClient.PostAsync(JsonRpcUrl, request);
        var actualResponseJson = (await response.Content.ReadAsStringAsync()).TrimAllLines();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        actualRequestData.Should().AllBeEquivalentTo(expectedRequestData);
        actualResponseJson.Should().Be(expectedResponseJson);
    }

    [Test]
    public async Task Batch_BindingStyleDefaultWithObjectParams_DeserializeSuccessfully()
    {
        const string requestJson = $$"""
            [
                {
                    "method": "binding_style_default",
                    "id": "123",
                    "params": {
                        "data": {{TestData.PlainRequiredSnakeCaseJson}}
                    },
                    "jsonrpc": "2.0"
                },
                {
                    "method": "binding_style_default",
                    "id": "456",
                    "params": {
                        "data": {{TestData.PlainRequiredSnakeCaseJson}}
                    },
                    "jsonrpc": "2.0"
                }
            ]
            """;
        var expectedRequestData = TestData.Plain;
        var responseData = TestData.Plain;
        var expectedResponseJson = $$"""
            [
                {
                    "id": "123",
                    "result": {{TestData.PlainFullSnakeCaseJson}},
                    "jsonrpc": "2.0"
                },
                {
                    "id": "456",
                    "result": {{TestData.PlainFullSnakeCaseJson}},
                    "jsonrpc": "2.0"
                }
            ]
            """.TrimAllLines();

        var actualRequestData = new List<TestData>();
        requestValidatorMock.Setup(static v => v.Validate(It.IsAny<TestData>()))
            .Callback<TestData>(requestData => actualRequestData.Add(requestData));
        responseProviderMock.Setup(static p => p.GetJsonRpcResponse())
            .Returns(responseData);

        using var request = new StringContent(requestJson, Encoding.UTF8, "application/json");
        var response = await ApiClient.PostAsync(JsonRpcUrl, request);
        var actualResponseJson = (await response.Content.ReadAsStringAsync()).TrimAllLines();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        actualRequestData.Should().AllBeEquivalentTo(expectedRequestData);
        actualResponseJson.Should().Be(expectedResponseJson);
    }

    [Test]
    public async Task Batch_BindingStyleDefaultWithArrayParams_DeserializeSuccessfully()
    {
        const string requestJson = $$"""
            [
                {
                    "method": "binding_style_default",
                    "id": "123",
                    "params": [
                        {{TestData.PlainRequiredSnakeCaseJson}}
                    ],
                    "jsonrpc": "2.0"
                },
                {
                    "method": "binding_style_default",
                    "id": "456",
                    "params": [
                        {{TestData.PlainRequiredSnakeCaseJson}}
                    ],
                    "jsonrpc": "2.0"
                }
            ]
            """;
        var expectedRequestData = TestData.Plain;
        var responseData = TestData.Plain;
        var expectedResponseJson = $$"""
            [
                {
                    "id": "123",
                    "result": {{TestData.PlainFullSnakeCaseJson}},
                    "jsonrpc": "2.0"
                },
                {
                    "id": "456",
                    "result": {{TestData.PlainFullSnakeCaseJson}},
                    "jsonrpc": "2.0"
                }
            ]
            """.TrimAllLines();

        var actualRequestData = new List<TestData>();
        requestValidatorMock.Setup(static v => v.Validate(It.IsAny<TestData>()))
            .Callback<TestData>(requestData => actualRequestData.Add(requestData));
        responseProviderMock.Setup(static p => p.GetJsonRpcResponse())
            .Returns(responseData);

        using var request = new StringContent(requestJson, Encoding.UTF8, "application/json");
        var response = await ApiClient.PostAsync(JsonRpcUrl, request);
        var actualResponseJson = (await response.Content.ReadAsStringAsync()).TrimAllLines();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        actualRequestData.Should().AllBeEquivalentTo(expectedRequestData);
        actualResponseJson.Should().Be(expectedResponseJson);
    }

    [Test]
    public async Task Batch_BindingStyleDefaultWithNullParams_()
    {}

    [Test]
    public async Task Batch_BindingStyleDefaultWithoutParams_()
    {}

    [Test]
    public async Task Batch_BindingStyleDefaultWithEmptyObjectParams_()
    {}

    [Test]
    public async Task Batch_BindingStyleDefaultWithEmptyArrayParams_()
    {}

    [Test]
    public async Task Batch_BindingStyleObject_DeserializeSuccessfully()
    {
        const string requestJson = $$"""
            [
                {
                    "method": "binding_style_object",
                    "id": "123",
                    "params": {{TestData.PlainRequiredSnakeCaseJson}},
                    "jsonrpc": "2.0"
                },
                {
                    "method": "binding_style_object",
                    "id": "456",
                    "params": {{TestData.PlainRequiredSnakeCaseJson}},
                    "jsonrpc": "2.0"
                }
            ]
            """;
        var expectedRequestData = TestData.Plain;
        var responseData = TestData.Plain;
        var expectedResponseJson = $$"""
            [
                {
                    "id": "123",
                    "result": {{TestData.PlainFullSnakeCaseJson}},
                    "jsonrpc": "2.0"
                },
                {
                    "id": "456",
                    "result": {{TestData.PlainFullSnakeCaseJson}},
                    "jsonrpc": "2.0"
                }
            ]
            """.TrimAllLines();

        var actualRequestData = new List<TestData>();
        requestValidatorMock.Setup(static v => v.Validate(It.IsAny<TestData>()))
            .Callback<TestData>(requestData => actualRequestData.Add(requestData));
        responseProviderMock.Setup(static p => p.GetJsonRpcResponse())
            .Returns(responseData);

        using var request = new StringContent(requestJson, Encoding.UTF8, "application/json");
        var response = await ApiClient.PostAsync(JsonRpcUrl, request);
        var actualResponseJson = (await response.Content.ReadAsStringAsync()).TrimAllLines();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        actualRequestData.Should().AllBeEquivalentTo(expectedRequestData);
        actualResponseJson.Should().Be(expectedResponseJson);
    }

    [Test]
    public async Task Batch_BindingStyleObjectWithNullParams_()
    {}

    [Test]
    public async Task Batch_BindingStyleObjectWithoutParams_()
    {}

    [Test]
    public async Task Batch_BindingStyleObjectWithEmptyObjectParams_()
    {}

    [Test]
    public async Task Batch_BindingStyleObjectWithEmptyArrayParams_()
    {}

    [Test]
    public async Task Batch_BindingStyleArray_DeserializeSuccessfully()
    {
        const string requestJson = $$"""
            [
                {
                    "method": "binding_style_array",
                    "id": "123",
                    "params": [
                        {{TestData.PlainRequiredSnakeCaseJson}}
                    ],
                    "jsonrpc": "2.0"
                },
                {
                    "method": "binding_style_array",
                    "id": "456",
                    "params": [
                        {{TestData.PlainRequiredSnakeCaseJson}}
                    ],
                    "jsonrpc": "2.0"
                }
            ]
            """;
        var expectedRequestData = TestData.Plain;
        var responseData = TestData.Plain;
        var expectedResponseJson = $$"""
            [
                {
                    "id": "123",
                    "result": {{TestData.PlainFullSnakeCaseJson}},
                    "jsonrpc": "2.0"
                },
                {
                    "id": "456",
                    "result": {{TestData.PlainFullSnakeCaseJson}},
                    "jsonrpc": "2.0"
                }
            ]
            """.TrimAllLines();

        var actualRequestData = new List<TestData>();
        requestValidatorMock.Setup(static v => v.Validate(It.IsAny<TestData>()))
            .Callback<TestData>(requestData => actualRequestData.Add(requestData));
        responseProviderMock.Setup(static p => p.GetJsonRpcResponse())
            .Returns(responseData);

        using var request = new StringContent(requestJson, Encoding.UTF8, "application/json");
        var response = await ApiClient.PostAsync(JsonRpcUrl, request);
        var actualResponseJson = (await response.Content.ReadAsStringAsync()).TrimAllLines();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        actualRequestData.Should().AllBeEquivalentTo(expectedRequestData);
        actualResponseJson.Should().Be(expectedResponseJson);
    }

    [Test]
    public async Task Batch_BindingStyleArrayWithNullParams_()
    {}

    [Test]
    public async Task Batch_BindingStyleArrayWithoutParams_()
    {}

    [Test]
    public async Task Batch_BindingStyleArrayWithEmptyObjectParams_()
    {}

    [Test]
    public async Task Batch_BindingStyleArrayWithEmptyArrayParams_()
    {}

    [Test]
    public async Task Batch_NullableDefaultParamsWithNullParams_()
    {}

    [Test]
    public async Task Batch_NullableDefaultParamsWithoutParams_()
    {}

    [Test]
    public async Task Batch_NullableDefaultParamsWithEmptyObjectParams_()
    {}

    [Test]
    public async Task Batch_NullableDefaultParamsWithEmptyArrayParams_()
    {}

    [Test]
    public async Task Batch_NullableObjectParamsWithNullParams_()
    {}

    [Test]
    public async Task Batch_NullableObjectParamsWithoutParams_()
    {}

    [Test]
    public async Task Batch_NullableObjectParamsWithEmptyObjectParams_()
    {}

    [Test]
    public async Task Batch_NullableObjectParamsWithEmptyArrayParams_()
    {}

    [Test]
    public async Task Batch_NullableArrayParamsWithNullParams_()
    {}

    [Test]
    public async Task Batch_NullableArrayParamsWithoutParams_()
    {}

    [Test]
    public async Task Batch_NullableArrayParamsWithEmptyObjectParams_()
    {}

    [Test]
    public async Task Batch_NullableArrayParamsWithEmptyArrayParams_()
    {}

    [Test]
    public async Task Batch_DefaultParamsWithNullParams_()
    {}

    [Test]
    public async Task Batch_DefaultParamsWithoutParams_()
    {}

    [Test]
    public async Task Batch_DefaultParamsWithEmptyObjectParams_()
    {}

    [Test]
    public async Task Batch_DefaultParamsWithEmptyArrayParams_()
    {}

    [Test]
    public async Task Batch_DefaultObjectParamsWithNullParams_()
    {}

    [Test]
    public async Task Batch_DefaultObjectParamsWithoutParams_()
    {}

    [Test]
    public async Task Batch_DefaultObjectParamsWithEmptyObjectParams_()
    {}

    [Test]
    public async Task Batch_DefaultObjectParamsWithEmptyArrayParams_()
    {}

    [Test]
    public async Task Batch_DefaultArrayParamsWithNullParams_()
    {}

    [Test]
    public async Task Batch_DefaultArrayParamsWithoutParams_()
    {}

    [Test]
    public async Task Batch_DefaultArrayParamsWithEmptyObjectParams_()
    {}

    [Test]
    public async Task Batch_DefaultArrayParamsWithEmptyArrayParams_()
    {}

    [Test]
    public async Task Batch_NoParams_ProcessSuccessfully()
    {
        const string requestJson = """
            [
                {
                    "method": "no_params",
                    "id": "123",
                    "jsonrpc": "2.0"
                },
                {
                    "method": "no_params",
                    "id": "456",
                    "jsonrpc": "2.0"
                }
            ]
            """;
        var responseData = TestData.Plain;
        var expectedResponseJson = $$"""
            [
                {
                    "id": "123",
                    "result": {{TestData.PlainFullSnakeCaseJson}},
                    "jsonrpc": "2.0"
                },
                {
                    "id": "456",
                    "result": {{TestData.PlainFullSnakeCaseJson}},
                    "jsonrpc": "2.0"
                }
            ]
            """.TrimAllLines();

        responseProviderMock.Setup(static p => p.GetJsonRpcResponse())
            .Returns(responseData);

        using var request = new StringContent(requestJson, Encoding.UTF8, "application/json");
        var response = await ApiClient.PostAsync(JsonRpcUrl, request);
        var actualResponseJson = (await response.Content.ReadAsStringAsync()).TrimAllLines();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        actualResponseJson.Should().Be(expectedResponseJson);
    }

    [Test]
    public async Task Batch_SnakeCaseParams_DeserializeSuccessfully()
    {
        const string requestJson = $$"""
            [
                {
                    "method": "snake_case_params",
                    "id": "123",
                    "params": {{TestData.PlainRequiredSnakeCaseJson}},
                    "jsonrpc": "2.0"
                },
                {
                    "method": "snake_case_params",
                    "id": "456",
                    "params": {{TestData.PlainRequiredSnakeCaseJson}},
                    "jsonrpc": "2.0"
                }
            ]
            """;
        var expectedRequestData = TestData.Plain;
        var responseData = TestData.Plain;
        var expectedResponseJson = $$"""
            [
                {
                    "id": "123",
                    "result": {{TestData.PlainFullSnakeCaseJson}},
                    "jsonrpc": "2.0"
                },
                {
                    "id": "456",
                    "result": {{TestData.PlainFullSnakeCaseJson}},
                    "jsonrpc": "2.0"
                }
            ]
            """.TrimAllLines();

        var actualRequestData = new List<TestData>();
        requestValidatorMock.Setup(static v => v.Validate(It.IsAny<TestData>()))
            .Callback<TestData>(requestData => actualRequestData.Add(requestData));
        responseProviderMock.Setup(static p => p.GetJsonRpcResponse())
            .Returns(responseData);

        using var request = new StringContent(requestJson, Encoding.UTF8, "application/json");
        var response = await ApiClient.PostAsync(JsonRpcUrl, request);
        var actualResponseJson = (await response.Content.ReadAsStringAsync()).TrimAllLines();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        actualRequestData.Should().AllBeEquivalentTo(expectedRequestData);
        actualResponseJson.Should().Be(expectedResponseJson);
    }

    [Test]
    public async Task Batch_CamelCaseParams_DeserializeSuccessfully()
    {
        const string requestJson = $$"""
            [
                {
                    "method": "camelCaseParams",
                    "id": "123",
                    "params": {{TestData.PlainRequiredCamelCaseJson}},
                    "jsonrpc": "2.0"
                },
                {
                    "method": "camelCaseParams",
                    "id": "456",
                    "params": {{TestData.PlainRequiredCamelCaseJson}},
                    "jsonrpc": "2.0"
                }
            ]
            """;
        var expectedRequestData = TestData.Plain;
        var responseData = TestData.Plain;
        var expectedResponseJson = $$"""
            [
                {
                    "id": "123",
                    "result": {{TestData.PlainFullCamelCaseJson}},
                    "jsonrpc": "2.0"
                },
                {
                    "id": "456",
                    "result": {{TestData.PlainFullCamelCaseJson}},
                    "jsonrpc": "2.0"
                }
            ]
            """.TrimAllLines();

        var actualRequestData = new List<TestData>();
        requestValidatorMock.Setup(static v => v.Validate(It.IsAny<TestData>()))
            .Callback<TestData>(requestData => actualRequestData.Add(requestData));
        responseProviderMock.Setup(static p => p.GetJsonRpcResponse())
            .Returns(responseData);

        using var request = new StringContent(requestJson, Encoding.UTF8, "application/json");
        var response = await ApiClient.PostAsync(JsonRpcUrl, request);
        var actualResponseJson = (await response.Content.ReadAsStringAsync()).TrimAllLines();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        actualRequestData.Should().AllBeEquivalentTo(expectedRequestData);
        actualResponseJson.Should().Be(expectedResponseJson);
    }

    [Test]
    public async Task Batch_NestedSnakeCaseParams_DeserializeSuccessfully()
    {
        const string requestJson = $$"""
            [
                {
                    "method": "snake_case_params",
                    "id": "123",
                    "params": {{TestData.NestedRequiredSnakeCaseJson}},
                    "jsonrpc": "2.0"
                },
                {
                    "method": "snake_case_params",
                    "id": "456",
                    "params": {{TestData.NestedRequiredSnakeCaseJson}},
                    "jsonrpc": "2.0"
                }
            ]
            """;
        var expectedRequestData = TestData.Nested;
        var responseData = TestData.Nested;
        var expectedResponseJson = $$"""
            [
                {
                    "id": "123",
                    "result": {{TestData.NestedFullSnakeCaseJson}},
                    "jsonrpc": "2.0"
                },
                {
                    "id": "456",
                    "result": {{TestData.NestedFullSnakeCaseJson}},
                    "jsonrpc": "2.0"
                }
            ]
            """.TrimAllLines();

        var actualRequestData = new List<TestData>();
        requestValidatorMock.Setup(static v => v.Validate(It.IsAny<TestData>()))
            .Callback<TestData>(requestData => actualRequestData.Add(requestData));
        responseProviderMock.Setup(static p => p.GetJsonRpcResponse())
            .Returns(responseData);

        using var request = new StringContent(requestJson, Encoding.UTF8, "application/json");
        var response = await ApiClient.PostAsync(JsonRpcUrl, request);
        var actualResponseJson = (await response.Content.ReadAsStringAsync()).TrimAllLines();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        actualRequestData.Should().AllBeEquivalentTo(expectedRequestData);
        actualResponseJson.Should().Be(expectedResponseJson);
    }

    [Test]
    public async Task Batch_NestedCamelCaseParams_DeserializeSuccessfully()
    {
        const string requestJson = $$"""
            [
                {
                    "method": "camelCaseParams",
                    "id": "123",
                    "params": {{TestData.NestedRequiredCamelCaseJson}},
                    "jsonrpc": "2.0"
                },
                {
                    "method": "camelCaseParams",
                    "id": "456",
                    "params": {{TestData.NestedRequiredCamelCaseJson}},
                    "jsonrpc": "2.0"
                }
            ]
            """;
        var expectedRequestData = TestData.Nested;
        var responseData = TestData.Nested;
        var expectedResponseJson = $$"""
            [
                {
                    "id": "123",
                    "result": {{TestData.NestedFullCamelCaseJson}},
                    "jsonrpc": "2.0"
                },
                {
                    "id": "456",
                    "result": {{TestData.NestedFullCamelCaseJson}},
                    "jsonrpc": "2.0"
                }
            ]
            """.TrimAllLines();

        var actualRequestData = new List<TestData>();
        requestValidatorMock.Setup(static v => v.Validate(It.IsAny<TestData>()))
            .Callback<TestData>(requestData => actualRequestData.Add(requestData));
        responseProviderMock.Setup(static p => p.GetJsonRpcResponse())
            .Returns(responseData);

        using var request = new StringContent(requestJson, Encoding.UTF8, "application/json");
        var response = await ApiClient.PostAsync(JsonRpcUrl, request);
        var actualResponseJson = (await response.Content.ReadAsStringAsync()).TrimAllLines();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        actualRequestData.Should().AllBeEquivalentTo(expectedRequestData);
        actualResponseJson.Should().Be(expectedResponseJson);
    }

    [Test]
    public async Task Batch_AdditionalParams_DeserializeSuccessfully()
    {
        const string requestJson = """
            [
                {
                    "method": "process_anything",
                    "id": "123",
                    "params": {
                        "bool_field": true,
                        "string_field": "123",
                        "int_field": 123,
                        "double_field": 1.23,
                        "enum_field": "two",
                        "array_field": [
                            1,
                            2,
                            3
                        ],
                        "nullable_field": null,
                        "additional_field": "something"
                    },
                    "jsonrpc": "2.0"
                },
                {
                    "method": "process_anything",
                    "id": "456",
                    "params": {
                        "bool_field": true,
                        "string_field": "123",
                        "int_field": 123,
                        "double_field": 1.23,
                        "enum_field": "two",
                        "array_field": [
                            1,
                            2,
                            3
                        ],
                        "nullable_field": null,
                        "additional_field": "something"
                    },
                    "jsonrpc": "2.0"
                }
            ]
            """;
        var expectedRequestData = TestData.Plain;
        var responseData = TestData.Plain;
        var expectedResponseJson = $$"""
            [
                {
                    "id": "123",
                    "result": {{TestData.PlainFullSnakeCaseJson}},
                    "jsonrpc": "2.0"
                },
                {
                    "id": "456",
                    "result": {{TestData.PlainFullSnakeCaseJson}},
                    "jsonrpc": "2.0"
                }
            ]
            """.TrimAllLines();

        var actualRequestData = new List<TestData>();
        requestValidatorMock.Setup(static v => v.Validate(It.IsAny<TestData>()))
            .Callback<TestData>(requestData => actualRequestData.Add(requestData));
        responseProviderMock.Setup(static p => p.GetJsonRpcResponse())
            .Returns(responseData);

        using var request = new StringContent(requestJson, Encoding.UTF8, "application/json");
        var response = await ApiClient.PostAsync(JsonRpcUrl, request);
        var actualResponseJson = (await response.Content.ReadAsStringAsync()).TrimAllLines();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        actualRequestData.Should().AllBeEquivalentTo(expectedRequestData);
        actualResponseJson.Should().Be(expectedResponseJson);
    }

    [Test]
    public async Task Batch_CustomActionRoute_DeserializeSuccessfully()
    {
        const string requestJson = $$"""
            [
                {
                    "method": "custom_action_route",
                    "id": "123",
                    "params": {{TestData.PlainRequiredSnakeCaseJson}},
                    "jsonrpc": "2.0"
                },
                {
                    "method": "custom_action_route",
                    "id": "456",
                    "params": {{TestData.PlainRequiredSnakeCaseJson}},
                    "jsonrpc": "2.0"
                }
            ]
            """;
        var expectedRequestData = TestData.Plain;
        var responseData = TestData.Plain;
        var expectedResponseJson = $$"""
            [
                {
                    "id": "123",
                    "result": {{TestData.PlainFullSnakeCaseJson}},
                    "jsonrpc": "2.0"
                },
                {
                    "id": "456",
                    "result": {{TestData.PlainFullSnakeCaseJson}},
                    "jsonrpc": "2.0"
                }
            ]
            """.TrimAllLines();

        var actualRequestData = new List<TestData>();
        requestValidatorMock.Setup(static v => v.Validate(It.IsAny<TestData>()))
            .Callback<TestData>(requestData => actualRequestData.Add(requestData));
        responseProviderMock.Setup(static p => p.GetJsonRpcResponse())
            .Returns(responseData);

        using var request = new StringContent(requestJson, Encoding.UTF8, "application/json");
        var response = await ApiClient.PostAsync($"{JsonRpcUrl}/custom/action", request);
        var actualResponseJson = (await response.Content.ReadAsStringAsync()).TrimAllLines();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        actualRequestData.Should().AllBeEquivalentTo(expectedRequestData);
        actualResponseJson.Should().Be(expectedResponseJson);
    }

    [Test]
    public async Task Batch_CustomControllerRoute_DeserializeSuccessfully()
    {
        const string requestJson = $$"""
            [
                {
                    "method": "custom_controller_route",
                    "id": "123",
                    "params": {{TestData.PlainRequiredSnakeCaseJson}},
                    "jsonrpc": "2.0"
                },
                {
                    "method": "custom_controller_route",
                    "id": "456",
                    "params": {{TestData.PlainRequiredSnakeCaseJson}},
                    "jsonrpc": "2.0"
                }
            ]
            """;
        var expectedRequestData = TestData.Plain;
        var responseData = TestData.Plain;
        var expectedResponseJson = $$"""
            [
                {
                    "id": "123",
                    "result": {{TestData.PlainFullSnakeCaseJson}},
                    "jsonrpc": "2.0"
                },
                {
                    "id": "456",
                    "result": {{TestData.PlainFullSnakeCaseJson}},
                    "jsonrpc": "2.0"
                }
            ]
            """.TrimAllLines();

        var actualRequestData = new List<TestData>();
        requestValidatorMock.Setup(static v => v.Validate(It.IsAny<TestData>()))
            .Callback<TestData>(requestData => actualRequestData.Add(requestData));
        responseProviderMock.Setup(static p => p.GetJsonRpcResponse())
            .Returns(responseData);

        using var request = new StringContent(requestJson, Encoding.UTF8, "application/json");
        var response = await ApiClient.PostAsync($"{JsonRpcUrl}/custom/controller", request);
        var actualResponseJson = (await response.Content.ReadAsStringAsync()).TrimAllLines();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        actualRequestData.Should().AllBeEquivalentTo(expectedRequestData);
        actualResponseJson.Should().Be(expectedResponseJson);
    }

    [Test]
    public async Task Batch_RequestAndNotification_ReturnOnlyRequestResponse()
    {
        const string requestJson = $$"""
            [
                {
                    "method": "process_anything",
                    "id": "123",
                    "params": {{TestData.PlainRequiredSnakeCaseJson}},
                    "jsonrpc": "2.0"
                },
                {
                    "method": "process_anything",
                    "params": {{TestData.PlainRequiredSnakeCaseJson}},
                    "jsonrpc": "2.0"
                }
            ]
            """;
        var expectedRequestData = TestData.Plain;
        var responseData = TestData.Plain;
        var expectedResponseJson = $$"""
            [
                {
                    "id": "123",
                    "result": {{TestData.PlainFullSnakeCaseJson}},
                    "jsonrpc": "2.0"
                }
            ]
            """.TrimAllLines();

        var actualRequestData = new List<TestData>();
        requestValidatorMock.Setup(static v => v.Validate(It.IsAny<TestData>()))
            .Callback<TestData>(requestData => actualRequestData.Add(requestData));
        responseProviderMock.Setup(static p => p.GetJsonRpcResponse())
            .Returns(responseData);

        using var request = new StringContent(requestJson, Encoding.UTF8, "application/json");
        var response = await ApiClient.PostAsync(JsonRpcUrl, request);
        var actualResponseJson = (await response.Content.ReadAsStringAsync()).TrimAllLines();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        actualRequestData.Should().AllBeEquivalentTo(expectedRequestData);
        actualResponseJson.Should().Be(expectedResponseJson);
    }

    [Test]
    public async Task Batch_ThrowException_SerializeSuccessfully()
    {
        const string requestJson = """
            [
                {
                    "method": "throw_exception",
                    "id": "123",
                    "jsonrpc": "2.0"
                },
                {
                    "method": "throw_exception",
                    "id": "456",
                    "jsonrpc": "2.0"
                }
            ]
            """;
        var expectedResponseJson = """
            [
                {
                    "id": "123",
                    "error": {
                        "code": -32000,
                        "message": "Server error",
                        "data": {
                            "type": "System.ArgumentException",
                            "message": "Value does not fall within the expected range.",
                            "details": null
                        }
                    },
                    "jsonrpc": "2.0"
                },
                {
                    "id": "456",
                    "error": {
                        "code": -32000,
                        "message": "Server error",
                        "data": {
                            "type": "System.ArgumentException",
                            "message": "Value does not fall within the expected range.",
                            "details": null
                        }
                    },
                    "jsonrpc": "2.0"
                }
            ]
            """.TrimAllLines();

        using var request = new StringContent(requestJson, Encoding.UTF8, "application/json");
        var response = await ApiClient.PostAsync(JsonRpcUrl, request);
        var actualResponseJson = (await response.Content.ReadAsStringAsync()).TrimAllLines();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        actualResponseJson.Should().Be(expectedResponseJson);
    }

    [Test]
    public async Task Batch_ReturnErrorFromFactory_SerializeSuccessfully()
    {
        const string requestJson = """
            [
                {
                    "method": "return_error_from_factory",
                    "id": "123",
                    "jsonrpc": "2.0"
                },
                {
                    "method": "return_error_from_factory",
                    "id": "456",
                    "jsonrpc": "2.0"
                }
            ]
            """;
        var expectedResponseJson = """
            [
                {
                    "id": "123",
                    "error": {
                        "code": -32602,
                        "message": "Invalid params",
                        "data": "errorMessage"
                    },
                    "jsonrpc": "2.0"
                },
                {
                    "id": "456",
                    "error": {
                        "code": -32602,
                        "message": "Invalid params",
                        "data": "errorMessage"
                    },
                    "jsonrpc": "2.0"
                }
            ]
            """.TrimAllLines();

        using var request = new StringContent(requestJson, Encoding.UTF8, "application/json");
        var response = await ApiClient.PostAsync(JsonRpcUrl, request);
        var actualResponseJson = (await response.Content.ReadAsStringAsync()).TrimAllLines();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        actualResponseJson.Should().Be(expectedResponseJson);
    }

    [Test]
    public async Task Batch_ReturnMvcError_SerializeSuccessfully()
    {
        const string requestJson = """
            [
                {
                    "method": "return_mvc_error",
                    "id": "123",
                    "jsonrpc": "2.0"
                },
                {
                    "method": "return_mvc_error",
                    "id": "456",
                    "jsonrpc": "2.0"
                }
            ]
            """;
        var expectedResponseJson = """
            [
                {
                    "id": "123",
                    "error": {
                        "code": -32602,
                        "message": "Invalid params",
                        "data": "errorMessage"
                    },
                    "jsonrpc": "2.0"
                },
                {
                    "id": "456",
                    "error": {
                        "code": -32602,
                        "message": "Invalid params",
                        "data": "errorMessage"
                    },
                    "jsonrpc": "2.0"
                }
            ]
            """.TrimAllLines();

        using var request = new StringContent(requestJson, Encoding.UTF8, "application/json");
        var response = await ApiClient.PostAsync(JsonRpcUrl, request);
        var actualResponseJson = (await response.Content.ReadAsStringAsync()).TrimAllLines();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        actualResponseJson.Should().Be(expectedResponseJson);
    }

    [Test]
    public async Task Batch_ErrorThrowAsResponseException_SerializeSuccessfully()
    {
        const string requestJson = """
            [
                {
                    "method": "error_throw_as_response_exception",
                    "id": "123",
                    "jsonrpc": "2.0"
                },
                {
                    "method": "error_throw_as_response_exception",
                    "id": "456",
                    "jsonrpc": "2.0"
                }
            ]
            """;
        var expectedResponseJson = """
            [
                {
                    "id": "123",
                    "error": {
                        "code": -32602,
                        "message": "Invalid params",
                        "data": "errorMessage"
                    },
                    "jsonrpc": "2.0"
                },
                {
                    "id": "456",
                    "error": {
                        "code": -32602,
                        "message": "Invalid params",
                        "data": "errorMessage"
                    },
                    "jsonrpc": "2.0"
                }
            ]
            """.TrimAllLines();

        using var request = new StringContent(requestJson, Encoding.UTF8, "application/json");
        var response = await ApiClient.PostAsync(JsonRpcUrl, request);
        var actualResponseJson = (await response.Content.ReadAsStringAsync()).TrimAllLines();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        actualResponseJson.Should().Be(expectedResponseJson);
    }

    [Test]
    public async Task Batch_UnknownMethod_ReturnError()
    {
        const string requestJson = """
            [
                {
                    "method": "some_not_existing_method",
                    "id": "123",
                    "jsonrpc": "2.0"
                },
                {
                    "method": "some_not_existing_method",
                    "id": "456",
                    "jsonrpc": "2.0"
                }
            ]
            """;
        var expectedResponseJson = """
            [
                {
                    "id": "123",
                    "error": {
                        "code": -32601,
                        "message": "Method not found",
                        "data": {
                            "method": "some_not_existing_method"
                        }
                    },
                    "jsonrpc": "2.0"
                },
                {
                    "id": "456",
                    "error": {
                        "code": -32601,
                        "message": "Method not found",
                        "data": {
                            "method": "some_not_existing_method"
                        }
                    },
                    "jsonrpc": "2.0"
                }
            ]
            """.TrimAllLines();

        using var request = new StringContent(requestJson, Encoding.UTF8, "application/json");
        var response = await ApiClient.PostAsync(JsonRpcUrl, request);
        var actualResponseJson = (await response.Content.ReadAsStringAsync()).TrimAllLines();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        actualResponseJson.Should().Be(expectedResponseJson);
    }

    [Test]
    public async Task Batch_NoMethod_ReturnError()
    {
        const string requestJson = $$"""
            [
                {
                    "id": "123",
                    "params": {{TestData.PlainRequiredSnakeCaseJson}},
                    "jsonrpc": "2.0"
                },
                {
                    "id": "456",
                    "params": {{TestData.PlainRequiredSnakeCaseJson}},
                    "jsonrpc": "2.0"
                }
            ]
            """;
        var expectedResponseJson = """
            [
                {
                    "id": null,
                    "error": {
                        "code": -32600,
                        "message": "Invalid Request",
                        "data": {
                            "type": "Tochka.JsonRpc.Common.JsonRpcFormatException",
                            "message": "JSON Rpc call does not have [method] property",
                            "details": null
                        }
                    },
                    "jsonrpc": "2.0"
                },
                {
                    "id": null,
                    "error": {
                        "code": -32600,
                        "message": "Invalid Request",
                        "data": {
                            "type": "Tochka.JsonRpc.Common.JsonRpcFormatException",
                            "message": "JSON Rpc call does not have [method] property",
                            "details": null
                        }
                    },
                    "jsonrpc": "2.0"
                }
            ]
            """.TrimAllLines();

        using var request = new StringContent(requestJson, Encoding.UTF8, "application/json");
        var response = await ApiClient.PostAsync(JsonRpcUrl, request);
        var actualResponseJson = (await response.Content.ReadAsStringAsync()).TrimAllLines();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        actualResponseJson.Should().Be(expectedResponseJson);
    }

    [Test]
    public async Task Batch_MethodNull_ReturnError()
    {
        const string requestJson = $$"""
            [
                {
                    "method": null,
                    "id": "123",
                    "params": {{TestData.PlainRequiredSnakeCaseJson}},
                    "jsonrpc": "2.0"
                },
                {
                    "method": null,
                    "id": "456",
                    "params": {{TestData.PlainRequiredSnakeCaseJson}},
                    "jsonrpc": "2.0"
                }
            ]
            """;
        var expectedResponseJson = """
            [
                {
                    "id": "123",
                    "error": {
                        "code": -32600,
                        "message": "Invalid Request",
                        "data": {
                            "type": "Tochka.JsonRpc.Common.JsonRpcFormatException",
                            "message": "Method is null or empty",
                            "details": null
                        }
                    },
                    "jsonrpc": "2.0"
                },
                {
                    "id": "456",
                    "error": {
                        "code": -32600,
                        "message": "Invalid Request",
                        "data": {
                            "type": "Tochka.JsonRpc.Common.JsonRpcFormatException",
                            "message": "Method is null or empty",
                            "details": null
                        }
                    },
                    "jsonrpc": "2.0"
                }
            ]
            """.TrimAllLines();

        using var request = new StringContent(requestJson, Encoding.UTF8, "application/json");
        var response = await ApiClient.PostAsync(JsonRpcUrl, request);
        var actualResponseJson = (await response.Content.ReadAsStringAsync()).TrimAllLines();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        actualResponseJson.Should().Be(expectedResponseJson);
    }

    [Test]
    public async Task Batch_MethodEmpty_ReturnError()
    {
        const string requestJson = $$"""
            [
                {
                    "method": "",
                    "id": "123",
                    "params": {{TestData.PlainRequiredSnakeCaseJson}},
                    "jsonrpc": "2.0"
                },
                {
                    "method": "",
                    "id": "456",
                    "params": {{TestData.PlainRequiredSnakeCaseJson}},
                    "jsonrpc": "2.0"
                }
            ]
            """;
        var expectedResponseJson = """
            [
                {
                    "id": "123",
                    "error": {
                        "code": -32600,
                        "message": "Invalid Request",
                        "data": {
                            "type": "Tochka.JsonRpc.Common.JsonRpcFormatException",
                            "message": "Method is null or empty",
                            "details": null
                        }
                    },
                    "jsonrpc": "2.0"
                },
                {
                    "id": "456",
                    "error": {
                        "code": -32600,
                        "message": "Invalid Request",
                        "data": {
                            "type": "Tochka.JsonRpc.Common.JsonRpcFormatException",
                            "message": "Method is null or empty",
                            "details": null
                        }
                    },
                    "jsonrpc": "2.0"
                }
            ]
            """.TrimAllLines();

        using var request = new StringContent(requestJson, Encoding.UTF8, "application/json");
        var response = await ApiClient.PostAsync(JsonRpcUrl, request);
        var actualResponseJson = (await response.Content.ReadAsStringAsync()).TrimAllLines();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        actualResponseJson.Should().Be(expectedResponseJson);
    }

    [Test]
    public async Task Batch_MethodWhiteSpace_ReturnError()
    {
        const string requestJson = $$"""
            [
                {
                    "method": " ",
                    "id": "123",
                    "params": {{TestData.PlainRequiredSnakeCaseJson}},
                    "jsonrpc": "2.0"
                },
                {
                    "method": " ",
                    "id": "456",
                    "params": {{TestData.PlainRequiredSnakeCaseJson}},
                    "jsonrpc": "2.0"
                }
            ]
            """;
        var expectedResponseJson = """
            [
                {
                    "id": "123",
                    "error": {
                        "code": -32600,
                        "message": "Invalid Request",
                        "data": {
                            "type": "Tochka.JsonRpc.Common.JsonRpcFormatException",
                            "message": "Method is null or empty",
                            "details": null
                        }
                    },
                    "jsonrpc": "2.0"
                },
                {
                    "id": "456",
                    "error": {
                        "code": -32600,
                        "message": "Invalid Request",
                        "data": {
                            "type": "Tochka.JsonRpc.Common.JsonRpcFormatException",
                            "message": "Method is null or empty",
                            "details": null
                        }
                    },
                    "jsonrpc": "2.0"
                }
            ]
            """.TrimAllLines();

        using var request = new StringContent(requestJson, Encoding.UTF8, "application/json");
        var response = await ApiClient.PostAsync(JsonRpcUrl, request);
        var actualResponseJson = (await response.Content.ReadAsStringAsync()).TrimAllLines();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        actualResponseJson.Should().Be(expectedResponseJson);
    }

    [Test]
    public async Task Batch_NoRequiredParams_ReturnError()
    {
        const string requestJson = """
            [
                {
                    "method": "process_anything",
                    "id": "123",
                    "jsonrpc": "2.0"
                },
                {
                    "method": "process_anything",
                    "id": "456",
                    "jsonrpc": "2.0"
                }
            ]
            """;
        var expectedResponseJson = """
            [
                {
                    "id": "123",
                    "error": {
                        "code": -32602,
                        "message": "Invalid params",
                        "data": {
                            "data": [
                                "Bind value not found (expected JSON key = [data])"
                            ]
                        }
                    },
                    "jsonrpc": "2.0"
                },
                {
                    "id": "456",
                    "error": {
                        "code": -32602,
                        "message": "Invalid params",
                        "data": {
                            "data": [
                                "Bind value not found (expected JSON key = [data])"
                            ]
                        }
                    },
                    "jsonrpc": "2.0"
                }
            ]
            """.TrimAllLines();

        using var request = new StringContent(requestJson, Encoding.UTF8, "application/json");
        var response = await ApiClient.PostAsync(JsonRpcUrl, request);
        var actualResponseJson = (await response.Content.ReadAsStringAsync()).TrimAllLines();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        actualResponseJson.Should().Be(expectedResponseJson);
    }

    [Test]
    public async Task Batch_EmptyRequiredParams_ReturnError()
    {
        const string requestJson = """
            [
                {
                    "method": "process_anything",
                    "id": "123",
                    "params": {},
                    "jsonrpc": "2.0"
                },
                {
                    "method": "process_anything",
                    "id": "456",
                    "params": {},
                    "jsonrpc": "2.0"
                }
            ]
            """;
        var expectedResponseJson = """
            [
                {
                    "id": "123",
                    "error": {
                        "code": -32602,
                        "message": "Invalid params",
                        "data": {
                            "ArrayField": [
                                "The ArrayField field is required."
                            ],
                            "StringField": [
                                "The StringField field is required."
                            ]
                        }
                    },
                    "jsonrpc": "2.0"
                },
                {
                    "id": "456",
                    "error": {
                        "code": -32602,
                        "message": "Invalid params",
                        "data": {
                            "ArrayField": [
                                "The ArrayField field is required."
                            ],
                            "StringField": [
                                "The StringField field is required."
                            ]
                        }
                    },
                    "jsonrpc": "2.0"
                }
            ]
            """.TrimAllLines();

        using var request = new StringContent(requestJson, Encoding.UTF8, "application/json");
        var response = await ApiClient.PostAsync(JsonRpcUrl, request);
        var actualResponseJson = (await response.Content.ReadAsStringAsync()).TrimAllLines();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        actualResponseJson.Should().Be(expectedResponseJson);
    }

    [Test]
    public async Task Batch_RequiredParamsNull_ReturnError()
    {
        const string requestJson = """
            [
                {
                    "method": "process_anything",
                    "id": "123",
                    "params": null,
                    "jsonrpc": "2.0"
                },
                {
                    "method": "process_anything",
                    "id": "456",
                    "params": null,
                    "jsonrpc": "2.0"
                }
            ]
            """;
        var expectedResponseJson = """
            [
                {
                    "id": "123",
                    "error": {
                        "code": -32602,
                        "message": "Invalid params",
                        "data": {
                            "data": [
                                "Bind value not found (expected JSON key = [data])"
                            ]
                        }
                    },
                    "jsonrpc": "2.0"
                },
                {
                    "id": "456",
                    "error": {
                        "code": -32602,
                        "message": "Invalid params",
                        "data": {
                            "data": [
                                "Bind value not found (expected JSON key = [data])"
                            ]
                        }
                    },
                    "jsonrpc": "2.0"
                }
            ]
            """.TrimAllLines();

        using var request = new StringContent(requestJson, Encoding.UTF8, "application/json");
        var response = await ApiClient.PostAsync(JsonRpcUrl, request);
        var actualResponseJson = (await response.Content.ReadAsStringAsync()).TrimAllLines();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        actualResponseJson.Should().Be(expectedResponseJson);
    }

    [Test]
    public async Task Batch_InvalidJsonRpcVersion_ReturnError()
    {
        const string requestJson = $$"""
            [
                {
                    "method": "process_anything",
                    "id": "123",
                    "params": {{TestData.PlainRequiredSnakeCaseJson}},
                    "jsonrpc": "3.0"
                },
                {
                    "method": "process_anything",
                    "id": "456",
                    "params": {{TestData.PlainRequiredSnakeCaseJson}},
                    "jsonrpc": "3.0"
                }
            ]
            """;
        var expectedResponseJson = """
            [
                {
                    "id": "123",
                    "error": {
                        "code": -32600,
                        "message": "Invalid Request",
                        "data": {
                            "type": "Tochka.JsonRpc.Common.JsonRpcFormatException",
                            "message": "Only [2.0] version supported. Got [3.0]",
                            "details": null
                        }
                    },
                    "jsonrpc": "2.0"
                },
                {
                    "id": "456",
                    "error": {
                        "code": -32600,
                        "message": "Invalid Request",
                        "data": {
                            "type": "Tochka.JsonRpc.Common.JsonRpcFormatException",
                            "message": "Only [2.0] version supported. Got [3.0]",
                            "details": null
                        }
                    },
                    "jsonrpc": "2.0"
                }
            ]
            """.TrimAllLines();

        using var request = new StringContent(requestJson, Encoding.UTF8, "application/json");
        var response = await ApiClient.PostAsync(JsonRpcUrl, request);
        var actualResponseJson = (await response.Content.ReadAsStringAsync()).TrimAllLines();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        actualResponseJson.Should().Be(expectedResponseJson);
    }

    [Test]
    public async Task Batch_InvalidJson_ReturnError()
    {
        const string requestJson = $$"""
            [
                {
                    123
                    "method": "process_anything",
                    "id": "123",
                    "params": {{TestData.PlainRequiredSnakeCaseJson}},
                    "jsonrpc": "2.0"
                },
                {
                    123
                    "method": "process_anything",
                    "id": "123",
                    "params": {{TestData.PlainRequiredSnakeCaseJson}},
                    "jsonrpc": "2.0"
                }
            ]
            """;
        var expectedResponseJson = """
            {
                "id": null,
                "error": {
                    "code": -32700,
                    "message": "Parse error",
                    "data": {
                        "type": "System.Text.Json.JsonException",
                        "message": "\u00271\u0027 is an invalid start of a property name. Expected a \u0027\u0022\u0027. Path: $ | LineNumber: 2 | BytePositionInLine: 8.",
                        "details": null
                    }
                },
                "jsonrpc": "2.0"
            }
            """.TrimAllLines();

        using var request = new StringContent(requestJson, Encoding.UTF8, "application/json");
        var response = await ApiClient.PostAsync(JsonRpcUrl, request);
        var actualResponseJson = (await response.Content.ReadAsStringAsync()).TrimAllLines();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        actualResponseJson.Should().Be(expectedResponseJson);
    }

    [Test]
    public async Task Batch_NoJsonRpc_ReturnError()
    {
        const string requestJson = $$"""
            [
                {
                    "method": "process_anything",
                    "id": "123",
                    "params": {{TestData.PlainRequiredSnakeCaseJson}}
                },
                {
                    "method": "process_anything",
                    "id": "456",
                    "params": {{TestData.PlainRequiredSnakeCaseJson}}
                }
            ]
            """;
        var expectedResponseJson = """
            [
                {
                    "id": null,
                    "error": {
                        "code": -32600,
                        "message": "Invalid Request",
                        "data": {
                            "type": "Tochka.JsonRpc.Common.JsonRpcFormatException",
                            "message": "JSON Rpc call does not have [jsonrpc] property",
                            "details": null
                        }
                    },
                    "jsonrpc": "2.0"
                },
                {
                    "id": null,
                    "error": {
                        "code": -32600,
                        "message": "Invalid Request",
                        "data": {
                            "type": "Tochka.JsonRpc.Common.JsonRpcFormatException",
                            "message": "JSON Rpc call does not have [jsonrpc] property",
                            "details": null
                        }
                    },
                    "jsonrpc": "2.0"
                }
            ]
            """.TrimAllLines();

        using var request = new StringContent(requestJson, Encoding.UTF8, "application/json");
        var response = await ApiClient.PostAsync(JsonRpcUrl, request);
        var actualResponseJson = (await response.Content.ReadAsStringAsync()).TrimAllLines();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        actualResponseJson.Should().Be(expectedResponseJson);
    }

    [Test]
    [Ignore("Processes successfully, but should?")]
    public async Task Batch_SameId_ReturnError()
    {
        const string requestJson = $$"""
            [
                {
                    "method": "process_anything",
                    "id": "123",
                    "params": {{TestData.PlainRequiredSnakeCaseJson}},
                    "jsonrpc": "2.0"
                },
                {
                    "method": "process_anything",
                    "id": "123",
                    "params": {{TestData.PlainRequiredSnakeCaseJson}},
                    "jsonrpc": "2.0"
                }
            ]
            """;
        var expectedResponseJson = """
            [
                {
                    "id": "123",
                    "error": {
                        "code": TODO,
                        "message": "TODO",
                        "data": {TODO}
                    },
                    "jsonrpc": "2.0"
                },
                {
                    "id": "456",
                    "error": {
                        "code": TODO,
                        "message": "TODO",
                        "data": {TODO}
                    },
                    "jsonrpc": "2.0"
                }
            ]
            """.TrimAllLines();

        using var request = new StringContent(requestJson, Encoding.UTF8, "application/json");
        var response = await ApiClient.PostAsync(JsonRpcUrl, request);
        var actualResponseJson = (await response.Content.ReadAsStringAsync()).TrimAllLines();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        actualResponseJson.Should().Be(expectedResponseJson);
    }

    private const string JsonRpcUrl = "/api/jsonrpc";
}
