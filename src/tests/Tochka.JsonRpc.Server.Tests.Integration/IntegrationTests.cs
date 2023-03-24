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
using Tochka.JsonRpc.TestUtils.Integration;

namespace Tochka.JsonRpc.Server.Tests.Integration;

[TestFixture]
internal class IntegrationTests : IntegrationTestsBase<Program>
{
    private Mock<IResponseProvider> responseProviderMock;
    private Mock<IRequestValidator> requestValidatorMock;

    public override async Task OneTimeSetup()
    {
        await base.OneTimeSetup();
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
        const string requestJson = @"{
    ""jsonrpc"": ""2.0"",
    ""method"": ""action_only"",
    ""id"": ""123"",
    ""params"": {
        ""bool_field"": true,
        ""string_field"": ""123"",
        ""int_field"": 123,
        ""double_field"": 1.23,
        ""enum_field"": ""two"",
        ""nullable_field"": null
    }
}";
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
        const string requestJson = @"{
    ""jsonrpc"": ""2.0"",
    ""method"": ""simple_json_rpc.controller_and_action"",
    ""id"": ""123"",
    ""params"": {
        ""bool_field"": true,
        ""string_field"": ""123"",
        ""int_field"": 123,
        ""double_field"": 1.23,
        ""enum_field"": ""two"",
        ""nullable_field"": null
    }
}";
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
        const string requestJson = @"{
    ""jsonrpc"": ""2.0"",
    ""method"": ""binding_style_default"",
    ""id"": ""123"",
    ""params"": {
        ""data"": {
            ""bool_field"": true,
            ""string_field"": ""123"",
            ""int_field"": 123,
            ""double_field"": 1.23,
            ""enum_field"": ""two"",
            ""nullable_field"": null
        }
    }
}";
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
        const string requestJson = @"{
    ""jsonrpc"": ""2.0"",
    ""method"": ""binding_style_default"",
    ""id"": ""123"",
    ""params"": [
        {
            ""bool_field"": true,
            ""string_field"": ""123"",
            ""int_field"": 123,
            ""double_field"": 1.23,
            ""enum_field"": ""two"",
            ""nullable_field"": null
        }
    ]
}";
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
    public async Task Notification_BindingStyleObject_DeserializeSuccessfully()
    {
        const string requestJson = @"{
    ""jsonrpc"": ""2.0"",
    ""method"": ""binding_style_object"",
    ""id"": ""123"",
    ""params"": {
        ""bool_field"": true,
        ""string_field"": ""123"",
        ""int_field"": 123,
        ""double_field"": 1.23,
        ""enum_field"": ""two"",
        ""nullable_field"": null
    }
}";
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
    public async Task Notification_BindingStyleArray_DeserializeSuccessfully()
    {
        const string requestJson = @"{
    ""jsonrpc"": ""2.0"",
    ""method"": ""binding_style_array"",
    ""id"": ""123"",
    ""params"": [
        {
            ""bool_field"": true,
            ""string_field"": ""123"",
            ""int_field"": 123,
            ""double_field"": 1.23,
            ""enum_field"": ""two"",
            ""nullable_field"": null
        }
    ]
}";
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
    public async Task Notification_NoParams_ProcessSuccessfully()
    {
        const string requestJson = @"{
    ""jsonrpc"": ""2.0"",
    ""method"": ""no_params"",
    ""id"": ""123""
}";

        using var request = new StringContent(requestJson, Encoding.UTF8, "application/json");
        var response = await ApiClient.PostAsync(JsonRpcUrl, request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Test]
    public async Task Notification_NullParamsWithNullParams_DeserializeSuccessfully()
    {
        const string requestJson = @"{
    ""jsonrpc"": ""2.0"",
    ""method"": ""null_params"",
    ""id"": ""123"",
    ""params"": null
}";
        object? expectedRequestData = null;

        object? actualRequestData = new { };
        requestValidatorMock.Setup(static v => v.Validate(It.IsAny<object?>()))
            .Callback<object?>(requestData => actualRequestData = requestData);

        using var request = new StringContent(requestJson, Encoding.UTF8, "application/json");
        var response = await ApiClient.PostAsync(JsonRpcUrl, request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        actualRequestData.Should().BeEquivalentTo(expectedRequestData);
    }

    [Test]
    public async Task Notification_NullParamsWithoutParams_DeserializeSuccessfully()
    {
        const string requestJson = @"{
    ""jsonrpc"": ""2.0"",
    ""method"": ""null_params"",
    ""id"": ""123""
}";
        object? expectedRequestData = null;

        object? actualRequestData = new { };
        requestValidatorMock.Setup(static v => v.Validate(It.IsAny<object?>()))
            .Callback<object?>(requestData => actualRequestData = requestData);

        using var request = new StringContent(requestJson, Encoding.UTF8, "application/json");
        var response = await ApiClient.PostAsync(JsonRpcUrl, request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        actualRequestData.Should().BeEquivalentTo(expectedRequestData);
    }

    [Test]
    public async Task Notification_DefaultParamsWithEmptyParams_DeserializeSuccessfully()
    {
        const string requestJson = @"{
    ""jsonrpc"": ""2.0"",
    ""method"": ""default_params"",
    ""id"": ""123"",
    ""params"": {}
}";
        var expectedRequestData = "123";

        object? actualRequestData = null;
        requestValidatorMock.Setup(static v => v.Validate(It.IsAny<object?>()))
            .Callback<object?>(requestData => actualRequestData = requestData);

        using var request = new StringContent(requestJson, Encoding.UTF8, "application/json");
        var response = await ApiClient.PostAsync(JsonRpcUrl, request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        actualRequestData.Should().BeEquivalentTo(expectedRequestData);
    }

    [Test]
    [Ignore("Doesnt work, but should?")]
    public async Task Notification_DefaultParamsWithoutParams_DeserializeSuccessfully()
    {
        const string requestJson = @"{
    ""jsonrpc"": ""2.0"",
    ""method"": ""default_params"",
    ""id"": ""123""
}";
        var expectedRequestData = "123";

        object? actualRequestData = null;
        requestValidatorMock.Setup(static v => v.Validate(It.IsAny<object?>()))
            .Callback<object?>(requestData => actualRequestData = requestData);

        using var request = new StringContent(requestJson, Encoding.UTF8, "application/json");
        var response = await ApiClient.PostAsync(JsonRpcUrl, request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        actualRequestData.Should().BeEquivalentTo(expectedRequestData);
    }

    [Test]
    public async Task Notification_SnakeCaseParams_DeserializeSuccessfully()
    {
        const string requestJson = @"{
    ""jsonrpc"": ""2.0"",
    ""method"": ""snake_case_params"",
    ""id"": ""123"",
    ""params"": {
        ""bool_field"": true,
        ""string_field"": ""123"",
        ""int_field"": 123,
        ""double_field"": 1.23,
        ""enum_field"": ""two"",
        ""nullable_field"": null
    }
}";
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
        const string requestJson = @"{
    ""jsonrpc"": ""2.0"",
    ""method"": ""camelCaseParams"",
    ""id"": ""123"",
    ""params"": {
        ""boolField"": true,
        ""stringField"": ""123"",
        ""intField"": 123,
        ""doubleField"": 1.23,
        ""enumField"": ""two"",
        ""nullableField"": null
    }
}";
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
        const string requestJson = @"{
    ""jsonrpc"": ""2.0"",
    ""method"": ""custom_action_route"",
    ""id"": ""123"",
    ""params"": {
        ""bool_field"": true,
        ""string_field"": ""123"",
        ""int_field"": 123,
        ""double_field"": 1.23,
        ""enum_field"": ""two"",
        ""nullable_field"": null
    }
}";
        var expectedRequestData = TestData.Plain;

        TestData actualRequestData = null;
        requestValidatorMock.Setup(static v => v.Validate(It.IsAny<TestData>()))
            .Callback<TestData>(requestData => actualRequestData = requestData);

        using var request = new StringContent(requestJson, Encoding.UTF8, "application/json");
        var response = await ApiClient.PostAsync("/custom/action", request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        actualRequestData.Should().BeEquivalentTo(expectedRequestData);
    }

    [Test]
    public async Task Notification_CustomControllerRoute_DeserializeSuccessfully()
    {
        const string requestJson = @"{
    ""jsonrpc"": ""2.0"",
    ""method"": ""custom_controller_route"",
    ""id"": ""123"",
    ""params"": {
        ""bool_field"": true,
        ""string_field"": ""123"",
        ""int_field"": 123,
        ""double_field"": 1.23,
        ""enum_field"": ""two"",
        ""nullable_field"": null
    }
}";
        var expectedRequestData = TestData.Plain;

        TestData actualRequestData = null;
        requestValidatorMock.Setup(static v => v.Validate(It.IsAny<TestData>()))
            .Callback<TestData>(requestData => actualRequestData = requestData);

        using var request = new StringContent(requestJson, Encoding.UTF8, "application/json");
        var response = await ApiClient.PostAsync("/custom/controller", request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        actualRequestData.Should().BeEquivalentTo(expectedRequestData);
    }

    [Test]
    public async Task Request_StringId_DeserializeSuccessfully()
    {
        const string requestJson = @"{
    ""jsonrpc"": ""2.0"",
    ""method"": ""snake_case_params"",
    ""id"": ""123"",
    ""params"": {
        ""bool_field"": true,
        ""string_field"": ""123"",
        ""int_field"": 123,
        ""double_field"": 1.23,
        ""enum_field"": ""two"",
        ""nullable_field"": null
    }
}";
        var expectedRequestData = TestData.Plain;
        var responseData = TestData.Plain;
        var expectedResponseJson = @"{
    ""id"": ""123"",
    ""jsonrpc"": ""2.0"",
    ""result"": {
        ""bool_field"": true,
        ""string_field"": ""123"",
        ""int_field"": 123,
        ""double_field"": 1.23,
        ""enum_field"": ""two"",
        ""nullable_field"": null,
        ""not_required_field"": null,
        ""nested_field"": null
    }
}".TrimAllLines();

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
        const string requestJson = @"{
    ""jsonrpc"": ""2.0"",
    ""method"": ""snake_case_params"",
    ""id"": 123,
    ""params"": {
        ""bool_field"": true,
        ""string_field"": ""123"",
        ""int_field"": 123,
        ""double_field"": 1.23,
        ""enum_field"": ""two"",
        ""nullable_field"": null
    }
}";
        var expectedRequestData = TestData.Plain;
        var responseData = TestData.Plain;
        var expectedResponseJson = @"{
    ""id"": 123,
    ""jsonrpc"": ""2.0"",
    ""result"": {
        ""bool_field"": true,
        ""string_field"": ""123"",
        ""int_field"": 123,
        ""double_field"": 1.23,
        ""enum_field"": ""two"",
        ""nullable_field"": null,
        ""not_required_field"": null,
        ""nested_field"": null
    }
}".TrimAllLines();

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
        const string requestJson = @"{
    ""jsonrpc"": ""2.0"",
    ""method"": ""snake_case_params"",
    ""id"": null,
    ""params"": {
        ""bool_field"": true,
        ""string_field"": ""123"",
        ""int_field"": 123,
        ""double_field"": 1.23,
        ""enum_field"": ""two"",
        ""nullable_field"": null
    }
}";
        var expectedRequestData = TestData.Plain;
        var responseData = TestData.Plain;
        var expectedResponseJson = @"{
    ""id"": null,
    ""jsonrpc"": ""2.0"",
    ""result"": {
        ""bool_field"": true,
        ""string_field"": ""123"",
        ""int_field"": 123,
        ""double_field"": 1.23,
        ""enum_field"": ""two"",
        ""nullable_field"": null,
        ""not_required_field"": null,
        ""nested_field"": null
    }
}".TrimAllLines();

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
        const string requestJson = @"{
    ""jsonrpc"": ""2.0"",
    ""method"": ""action_only"",
    ""id"": ""123"",
    ""params"": {
        ""bool_field"": true,
        ""string_field"": ""123"",
        ""int_field"": 123,
        ""double_field"": 1.23,
        ""enum_field"": ""two"",
        ""nullable_field"": null
    }
}";
        var expectedRequestData = TestData.Plain;
        var responseData = TestData.Plain;
        var expectedResponseJson = @"{
    ""id"": ""123"",
    ""jsonrpc"": ""2.0"",
    ""result"": {
        ""bool_field"": true,
        ""string_field"": ""123"",
        ""int_field"": 123,
        ""double_field"": 1.23,
        ""enum_field"": ""two"",
        ""nullable_field"": null,
        ""not_required_field"": null,
        ""nested_field"": null
    }
}".TrimAllLines();

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
        const string requestJson = @"{
    ""jsonrpc"": ""2.0"",
    ""method"": ""simple_json_rpc.controller_and_action"",
    ""id"": ""123"",
    ""params"": {
        ""bool_field"": true,
        ""string_field"": ""123"",
        ""int_field"": 123,
        ""double_field"": 1.23,
        ""enum_field"": ""two"",
        ""nullable_field"": null
    }
}";
        var expectedRequestData = TestData.Plain;
        var responseData = TestData.Plain;
        var expectedResponseJson = @"{
    ""id"": ""123"",
    ""jsonrpc"": ""2.0"",
    ""result"": {
        ""bool_field"": true,
        ""string_field"": ""123"",
        ""int_field"": 123,
        ""double_field"": 1.23,
        ""enum_field"": ""two"",
        ""nullable_field"": null,
        ""not_required_field"": null,
        ""nested_field"": null
    }
}".TrimAllLines();

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
        const string requestJson = @"{
    ""jsonrpc"": ""2.0"",
    ""method"": ""binding_style_default"",
    ""id"": ""123"",
    ""params"": {
        ""data"": {
            ""bool_field"": true,
            ""string_field"": ""123"",
            ""int_field"": 123,
            ""double_field"": 1.23,
            ""enum_field"": ""two"",
            ""nullable_field"": null
        }
    }
}";
        var expectedRequestData = TestData.Plain;
        var responseData = TestData.Plain;
        var expectedResponseJson = @"{
    ""id"": ""123"",
    ""jsonrpc"": ""2.0"",
    ""result"": {
        ""bool_field"": true,
        ""string_field"": ""123"",
        ""int_field"": 123,
        ""double_field"": 1.23,
        ""enum_field"": ""two"",
        ""nullable_field"": null,
        ""not_required_field"": null,
        ""nested_field"": null
    }
}".TrimAllLines();

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
        const string requestJson = @"{
    ""jsonrpc"": ""2.0"",
    ""method"": ""binding_style_default"",
    ""id"": ""123"",
    ""params"": [
        {
            ""bool_field"": true,
            ""string_field"": ""123"",
            ""int_field"": 123,
            ""double_field"": 1.23,
            ""enum_field"": ""two"",
            ""nullable_field"": null
        }
    ]
}";
        var expectedRequestData = TestData.Plain;
        var responseData = TestData.Plain;
        var expectedResponseJson = @"{
    ""id"": ""123"",
    ""jsonrpc"": ""2.0"",
    ""result"": {
        ""bool_field"": true,
        ""string_field"": ""123"",
        ""int_field"": 123,
        ""double_field"": 1.23,
        ""enum_field"": ""two"",
        ""nullable_field"": null,
        ""not_required_field"": null,
        ""nested_field"": null
    }
}".TrimAllLines();

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
    public async Task Request_BindingStyleObject_DeserializeSuccessfully()
    {
        const string requestJson = @"{
    ""jsonrpc"": ""2.0"",
    ""method"": ""binding_style_object"",
    ""id"": ""123"",
    ""params"": {
        ""bool_field"": true,
        ""string_field"": ""123"",
        ""int_field"": 123,
        ""double_field"": 1.23,
        ""enum_field"": ""two"",
        ""nullable_field"": null
    }
}";
        var expectedRequestData = TestData.Plain;
        var responseData = TestData.Plain;
        var expectedResponseJson = @"{
    ""id"": ""123"",
    ""jsonrpc"": ""2.0"",
    ""result"": {
        ""bool_field"": true,
        ""string_field"": ""123"",
        ""int_field"": 123,
        ""double_field"": 1.23,
        ""enum_field"": ""two"",
        ""nullable_field"": null,
        ""not_required_field"": null,
        ""nested_field"": null
    }
}".TrimAllLines();

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
    public async Task Request_BindingStyleArray_DeserializeSuccessfully()
    {
        const string requestJson = @"{
    ""jsonrpc"": ""2.0"",
    ""method"": ""binding_style_array"",
    ""id"": ""123"",
    ""params"": [
        {
            ""bool_field"": true,
            ""string_field"": ""123"",
            ""int_field"": 123,
            ""double_field"": 1.23,
            ""enum_field"": ""two"",
            ""nullable_field"": null
        }
    ]
}";
        var expectedRequestData = TestData.Plain;
        var responseData = TestData.Plain;
        var expectedResponseJson = @"{
    ""id"": ""123"",
    ""jsonrpc"": ""2.0"",
    ""result"": {
        ""bool_field"": true,
        ""string_field"": ""123"",
        ""int_field"": 123,
        ""double_field"": 1.23,
        ""enum_field"": ""two"",
        ""nullable_field"": null,
        ""not_required_field"": null,
        ""nested_field"": null
    }
}".TrimAllLines();

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
    public async Task Request_NoParams_ProcessSuccessfully()
    {
        const string requestJson = @"{
    ""jsonrpc"": ""2.0"",
    ""method"": ""no_params"",
    ""id"": ""123""
}";
        var responseData = TestData.Plain;
        var expectedResponseJson = @"{
    ""id"": ""123"",
    ""jsonrpc"": ""2.0"",
    ""result"": {
        ""bool_field"": true,
        ""string_field"": ""123"",
        ""int_field"": 123,
        ""double_field"": 1.23,
        ""enum_field"": ""two"",
        ""nullable_field"": null,
        ""not_required_field"": null,
        ""nested_field"": null
    }
}".TrimAllLines();

        responseProviderMock.Setup(static p => p.GetJsonRpcResponse())
            .Returns(responseData);

        using var request = new StringContent(requestJson, Encoding.UTF8, "application/json");
        var response = await ApiClient.PostAsync(JsonRpcUrl, request);
        var actualResponseJson = (await response.Content.ReadAsStringAsync()).TrimAllLines();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        actualResponseJson.Should().Be(expectedResponseJson);
    }

    [Test]
    public async Task Request_NullParamsWithNullParams_DeserializeSuccessfully()
    {
        const string requestJson = @"{
    ""jsonrpc"": ""2.0"",
    ""method"": ""null_params"",
    ""id"": ""123"",
    ""params"": null
}";
        object? expectedRequestData = null;
        var responseData = TestData.Plain;
        var expectedResponseJson = @"{
    ""id"": ""123"",
    ""jsonrpc"": ""2.0"",
    ""result"": {
        ""bool_field"": true,
        ""string_field"": ""123"",
        ""int_field"": 123,
        ""double_field"": 1.23,
        ""enum_field"": ""two"",
        ""nullable_field"": null,
        ""not_required_field"": null,
        ""nested_field"": null
    }
}".TrimAllLines();

        object? actualRequestData = new { };
        requestValidatorMock.Setup(static v => v.Validate(It.IsAny<object?>()))
            .Callback<object?>(requestData => actualRequestData = requestData);
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
    public async Task Request_NullParamsWithoutParams_DeserializeSuccessfully()
    {
        const string requestJson = @"{
    ""jsonrpc"": ""2.0"",
    ""method"": ""null_params"",
    ""id"": ""123""
}";
        object? expectedRequestData = null;
        var responseData = TestData.Plain;
        var expectedResponseJson = @"{
    ""id"": ""123"",
    ""jsonrpc"": ""2.0"",
    ""result"": {
        ""bool_field"": true,
        ""string_field"": ""123"",
        ""int_field"": 123,
        ""double_field"": 1.23,
        ""enum_field"": ""two"",
        ""nullable_field"": null,
        ""not_required_field"": null,
        ""nested_field"": null
    }
}".TrimAllLines();

        object? actualRequestData = new { };
        requestValidatorMock.Setup(static v => v.Validate(It.IsAny<object?>()))
            .Callback<object?>(requestData => actualRequestData = requestData);
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
    public async Task Request_DefaultParamsWithEmptyParams_DeserializeSuccessfully()
    {
        const string requestJson = @"{
    ""jsonrpc"": ""2.0"",
    ""method"": ""default_params"",
    ""id"": ""123"",
    ""params"": {}
}";
        var expectedRequestData = "123";
        var responseData = TestData.Plain;
        var expectedResponseJson = @"{
    ""id"": ""123"",
    ""jsonrpc"": ""2.0"",
    ""result"": {
        ""bool_field"": true,
        ""string_field"": ""123"",
        ""int_field"": 123,
        ""double_field"": 1.23,
        ""enum_field"": ""two"",
        ""nullable_field"": null,
        ""not_required_field"": null,
        ""nested_field"": null
    }
}".TrimAllLines();

        object? actualRequestData = null;
        requestValidatorMock.Setup(static v => v.Validate(It.IsAny<object?>()))
            .Callback<object?>(requestData => actualRequestData = requestData);
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
    [Ignore("Doesnt work, but should?")]
    public async Task Request_DefaultParamsWithoutParams_DeserializeSuccessfully()
    {
        const string requestJson = @"{
    ""jsonrpc"": ""2.0"",
    ""method"": ""default_params"",
    ""id"": ""123""
}";
        var expectedRequestData = "123";
        var responseData = TestData.Plain;
        var expectedResponseJson = @"{
    ""id"": ""123"",
    ""jsonrpc"": ""2.0"",
    ""result"": {
        ""bool_field"": true,
        ""string_field"": ""123"",
        ""int_field"": 123,
        ""double_field"": 1.23,
        ""enum_field"": ""two"",
        ""nullable_field"": null,
        ""not_required_field"": null,
        ""nested_field"": null
    }
}".TrimAllLines();

        object? actualRequestData = null;
        requestValidatorMock.Setup(static v => v.Validate(It.IsAny<object?>()))
            .Callback<object?>(requestData => actualRequestData = requestData);
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
    public async Task Request_SnakeCaseParams_DeserializeSuccessfully()
    {
        const string requestJson = @"{
    ""jsonrpc"": ""2.0"",
    ""method"": ""snake_case_params"",
    ""id"": ""123"",
    ""params"": {
        ""bool_field"": true,
        ""string_field"": ""123"",
        ""int_field"": 123,
        ""double_field"": 1.23,
        ""enum_field"": ""two"",
        ""nullable_field"": null
    }
}";
        var expectedRequestData = TestData.Plain;
        var responseData = TestData.Plain;
        var expectedResponseJson = @"{
    ""id"": ""123"",
    ""jsonrpc"": ""2.0"",
    ""result"": {
        ""bool_field"": true,
        ""string_field"": ""123"",
        ""int_field"": 123,
        ""double_field"": 1.23,
        ""enum_field"": ""two"",
        ""nullable_field"": null,
        ""not_required_field"": null,
        ""nested_field"": null
    }
}".TrimAllLines();

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
        const string requestJson = @"{
    ""jsonrpc"": ""2.0"",
    ""method"": ""camelCaseParams"",
    ""id"": ""123"",
    ""params"": {
        ""boolField"": true,
        ""stringField"": ""123"",
        ""intField"": 123,
        ""doubleField"": 1.23,
        ""enumField"": ""two"",
        ""nullableField"": null
    }
}";
        var expectedRequestData = TestData.Plain;
        var responseData = TestData.Plain;
        var expectedResponseJson = @"{
    ""id"": ""123"",
    ""jsonrpc"": ""2.0"",
    ""result"": {
        ""boolField"": true,
        ""stringField"": ""123"",
        ""intField"": 123,
        ""doubleField"": 1.23,
        ""enumField"": ""two"",
        ""nullableField"": null,
        ""notRequiredField"": null,
        ""nestedField"": null
    }
}".TrimAllLines();

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
        const string requestJson = @"{
    ""jsonrpc"": ""2.0"",
    ""method"": ""snake_case_params"",
    ""id"": ""123"",
    ""params"": {
        ""bool_field"": true,
        ""string_field"": ""123"",
        ""int_field"": 123,
        ""double_field"": 1.23,
        ""enum_field"": ""two"",
        ""nullable_field"": null,
        ""nested_field"": {
            ""bool_field"": true,
            ""string_field"": ""456"",
            ""int_field"": 456,
            ""double_field"": 4.56,
            ""enum_field"": ""two"",
            ""nullable_field"": null
        }
    }
}";
        var expectedRequestData = TestData.Nested;
        var responseData = TestData.Nested;
        var expectedResponseJson = @"{
    ""id"": ""123"",
    ""jsonrpc"": ""2.0"",
    ""result"": {
        ""bool_field"": true,
        ""string_field"": ""123"",
        ""int_field"": 123,
        ""double_field"": 1.23,
        ""enum_field"": ""two"",
        ""nullable_field"": null,
        ""not_required_field"": null,
        ""nested_field"": {
            ""bool_field"": true,
            ""string_field"": ""456"",
            ""int_field"": 456,
            ""double_field"": 4.56,
            ""enum_field"": ""two"",
            ""nullable_field"": null,
            ""not_required_field"": null,
            ""nested_field"": null
        }
    }
}".TrimAllLines();

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
        const string requestJson = @"{
    ""jsonrpc"": ""2.0"",
    ""method"": ""camelCaseParams"",
    ""id"": ""123"",
    ""params"": {
        ""boolField"": true,
        ""stringField"": ""123"",
        ""intField"": 123,
        ""doubleField"": 1.23,
        ""enumField"": ""two"",
        ""nullableField"": null,
        ""nestedField"": {
            ""boolField"": true,
            ""stringField"": ""456"",
            ""intField"": 456,
            ""doubleField"": 4.56,
            ""enumField"": ""two"",
            ""nullableField"": null
        }
    }
}";
        var expectedRequestData = TestData.Nested;
        var responseData = TestData.Nested;
        var expectedResponseJson = @"{
    ""id"": ""123"",
    ""jsonrpc"": ""2.0"",
    ""result"": {
        ""boolField"": true,
        ""stringField"": ""123"",
        ""intField"": 123,
        ""doubleField"": 1.23,
        ""enumField"": ""two"",
        ""nullableField"": null,
        ""notRequiredField"": null,
        ""nestedField"": {
            ""boolField"": true,
            ""stringField"": ""456"",
            ""intField"": 456,
            ""doubleField"": 4.56,
            ""enumField"": ""two"",
            ""nullableField"": null,
            ""notRequiredField"": null,
            ""nestedField"": null
        }
    }
}".TrimAllLines();

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
        const string requestJson = @"{
    ""jsonrpc"": ""2.0"",
    ""method"": ""process_anything"",
    ""id"": ""123"",
    ""params"": {
        ""bool_field"": true,
        ""string_field"": ""123"",
        ""int_field"": 123,
        ""double_field"": 1.23,
        ""enum_field"": ""two"",
        ""nullable_field"": null,
        ""additional_field"": ""something""
    }
}";
        var expectedRequestData = TestData.Plain;
        var responseData = TestData.Plain;
        var expectedResponseJson = @"{
    ""id"": ""123"",
    ""jsonrpc"": ""2.0"",
    ""result"": {
        ""bool_field"": true,
        ""string_field"": ""123"",
        ""int_field"": 123,
        ""double_field"": 1.23,
        ""enum_field"": ""two"",
        ""nullable_field"": null,
        ""not_required_field"": null,
        ""nested_field"": null
    }
}".TrimAllLines();

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
        const string requestJson = @"{
    ""jsonrpc"": ""2.0"",
    ""method"": ""custom_action_route"",
    ""id"": ""123"",
    ""params"": {
        ""bool_field"": true,
        ""string_field"": ""123"",
        ""int_field"": 123,
        ""double_field"": 1.23,
        ""enum_field"": ""two"",
        ""nullable_field"": null
    }
}";
        var expectedRequestData = TestData.Plain;
        var responseData = TestData.Plain;
        var expectedResponseJson = @"{
    ""id"": ""123"",
    ""jsonrpc"": ""2.0"",
    ""result"": {
        ""bool_field"": true,
        ""string_field"": ""123"",
        ""int_field"": 123,
        ""double_field"": 1.23,
        ""enum_field"": ""two"",
        ""nullable_field"": null,
        ""not_required_field"": null,
        ""nested_field"": null
    }
}".TrimAllLines();

        TestData actualRequestData = null;
        requestValidatorMock.Setup(static v => v.Validate(It.IsAny<TestData>()))
            .Callback<TestData>(requestData => actualRequestData = requestData);
        responseProviderMock.Setup(static p => p.GetJsonRpcResponse())
            .Returns(responseData);

        using var request = new StringContent(requestJson, Encoding.UTF8, "application/json");
        var response = await ApiClient.PostAsync("/custom/action", request);
        var actualResponseJson = (await response.Content.ReadAsStringAsync()).TrimAllLines();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        actualRequestData.Should().BeEquivalentTo(expectedRequestData);
        actualResponseJson.Should().Be(expectedResponseJson);
    }

    [Test]
    public async Task Request_CustomControllerRoute_DeserializeSuccessfully()
    {
        const string requestJson = @"{
    ""jsonrpc"": ""2.0"",
    ""method"": ""custom_controller_route"",
    ""id"": ""123"",
    ""params"": {
        ""bool_field"": true,
        ""string_field"": ""123"",
        ""int_field"": 123,
        ""double_field"": 1.23,
        ""enum_field"": ""two"",
        ""nullable_field"": null
    }
}";
        var expectedRequestData = TestData.Plain;
        var responseData = TestData.Plain;
        var expectedResponseJson = @"{
    ""id"": ""123"",
    ""jsonrpc"": ""2.0"",
    ""result"": {
        ""bool_field"": true,
        ""string_field"": ""123"",
        ""int_field"": 123,
        ""double_field"": 1.23,
        ""enum_field"": ""two"",
        ""nullable_field"": null,
        ""not_required_field"": null,
        ""nested_field"": null
    }
}".TrimAllLines();

        TestData actualRequestData = null;
        requestValidatorMock.Setup(static v => v.Validate(It.IsAny<TestData>()))
            .Callback<TestData>(requestData => actualRequestData = requestData);
        responseProviderMock.Setup(static p => p.GetJsonRpcResponse())
            .Returns(responseData);

        using var request = new StringContent(requestJson, Encoding.UTF8, "application/json");
        var response = await ApiClient.PostAsync("/custom/controller", request);
        var actualResponseJson = (await response.Content.ReadAsStringAsync()).TrimAllLines();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        actualRequestData.Should().BeEquivalentTo(expectedRequestData);
        actualResponseJson.Should().Be(expectedResponseJson);
    }

    [Test]
    public async Task Request_ThrowException_SerializeSuccessfully()
    {
        const string requestJson = @"{
    ""jsonrpc"": ""2.0"",
    ""method"": ""throw_exception"",
    ""id"": ""123""
}";
        var expectedResponseJson = @"{
    ""id"": ""123"",
    ""jsonrpc"": ""2.0"",
    ""error"": {
        ""code"": -32000,
        ""message"": ""Server error"",
        ""data"": {
            ""internal_http_code"": null,
            ""message"": ""Value does not fall within the expected range."",
            ""details"": null,
            ""type"": ""System.ArgumentException""
        }
    }
}".TrimAllLines();

        using var request = new StringContent(requestJson, Encoding.UTF8, "application/json");
        var response = await ApiClient.PostAsync(JsonRpcUrl, request);
        var actualResponseJson = (await response.Content.ReadAsStringAsync()).TrimAllLines();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        actualResponseJson.Should().Be(expectedResponseJson);
    }

    [Test]
    public async Task Request_ReturnErrorFromFactory_SerializeSuccessfully()
    {
        const string requestJson = @"{
    ""jsonrpc"": ""2.0"",
    ""method"": ""return_error_from_factory"",
    ""id"": ""123""
}";
        var expectedResponseJson = @"{
    ""id"": ""123"",
    ""jsonrpc"": ""2.0"",
    ""error"": {
        ""code"": -32602,
        ""message"": ""Invalid params"",
        ""data"": ""errorMessage""
    }
}".TrimAllLines();

        using var request = new StringContent(requestJson, Encoding.UTF8, "application/json");
        var response = await ApiClient.PostAsync(JsonRpcUrl, request);
        var actualResponseJson = (await response.Content.ReadAsStringAsync()).TrimAllLines();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        actualResponseJson.Should().Be(expectedResponseJson);
    }

    [Test]
    public async Task Request_ReturnMvcError_SerializeSuccessfully()
    {
        const string requestJson = @"{
    ""jsonrpc"": ""2.0"",
    ""method"": ""return_mvc_error"",
    ""id"": ""123""
}";
        var expectedResponseJson = @"{
    ""id"": ""123"",
    ""jsonrpc"": ""2.0"",
    ""error"": {
        ""code"": -32602,
        ""message"": ""Invalid params"",
        ""data"": ""errorMessage""
    }
}".TrimAllLines();

        using var request = new StringContent(requestJson, Encoding.UTF8, "application/json");
        var response = await ApiClient.PostAsync(JsonRpcUrl, request);
        var actualResponseJson = (await response.Content.ReadAsStringAsync()).TrimAllLines();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        actualResponseJson.Should().Be(expectedResponseJson);
    }

    [Test]
    public async Task Request_ErrorThrowAsResponseException_SerializeSuccessfully()
    {
        const string requestJson = @"{
    ""jsonrpc"": ""2.0"",
    ""method"": ""error_throw_as_response_exception"",
    ""id"": ""123""
}";
        var expectedResponseJson = @"{
    ""id"": ""123"",
    ""jsonrpc"": ""2.0"",
    ""error"": {
        ""code"": -32602,
        ""message"": ""Invalid params"",
        ""data"": ""errorMessage""
    }
}".TrimAllLines();

        using var request = new StringContent(requestJson, Encoding.UTF8, "application/json");
        var response = await ApiClient.PostAsync(JsonRpcUrl, request);
        var actualResponseJson = (await response.Content.ReadAsStringAsync()).TrimAllLines();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        actualResponseJson.Should().Be(expectedResponseJson);
    }

    [Test]
    public async Task Request_UnknownMethod_ReturnError()
    {
        const string requestJson = @"{
    ""jsonrpc"": ""2.0"",
    ""method"": ""some_not_existing_method"",
    ""id"": ""123""
}";
        var expectedResponseJson = @"{
    ""id"": ""123"",
    ""jsonrpc"": ""2.0"",
    ""error"": {
        ""code"": -32601,
        ""message"": ""Method not found"",
        ""data"": null
    }
}".TrimAllLines();

        using var request = new StringContent(requestJson, Encoding.UTF8, "application/json");
        var response = await ApiClient.PostAsync(JsonRpcUrl, request);
        var actualResponseJson = (await response.Content.ReadAsStringAsync()).TrimAllLines();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        actualResponseJson.Should().Be(expectedResponseJson);
    }

    [Test]
    public async Task Request_NoMethod_ReturnError()
    {
        const string requestJson = @"{
    ""jsonrpc"": ""2.0"",
    ""id"": ""123"",
    ""params"": {
        ""bool_field"": true,
        ""string_field"": ""123"",
        ""int_field"": 123,
        ""double_field"": 1.23,
        ""enum_field"": ""two"",
        ""nullable_field"": null
    }
}";
        var expectedResponseJson = @"{
    ""id"": null,
    ""jsonrpc"": ""2.0"",
    ""error"": {
        ""code"": -32600,
        ""message"": ""Invalid Request"",
        ""data"": {
            ""internal_http_code"": null,
            ""message"": ""Method is null or empty"",
            ""details"": null,
            ""type"": ""Tochka.JsonRpc.V1.Server.Exceptions.JsonRpcInternalException""
        }
    }
}".TrimAllLines();

        using var request = new StringContent(requestJson, Encoding.UTF8, "application/json");
        var response = await ApiClient.PostAsync(JsonRpcUrl, request);
        var actualResponseJson = (await response.Content.ReadAsStringAsync()).TrimAllLines();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        actualResponseJson.Should().Be(expectedResponseJson);
    }

    [Test]
    public async Task Request_MethodNull_ReturnError()
    {
        const string requestJson = @"{
    ""jsonrpc"": ""2.0"",
    ""method"": null,
    ""id"": ""123"",
    ""params"": {
        ""bool_field"": true,
        ""string_field"": ""123"",
        ""int_field"": 123,
        ""double_field"": 1.23,
        ""enum_field"": ""two"",
        ""nullable_field"": null
    }
}";
        var expectedResponseJson = @"{
    ""id"": null,
    ""jsonrpc"": ""2.0"",
    ""error"": {
        ""code"": -32600,
        ""message"": ""Invalid Request"",
        ""data"": {
            ""internal_http_code"": null,
            ""message"": ""Method is null or empty"",
            ""details"": null,
            ""type"": ""Tochka.JsonRpc.V1.Server.Exceptions.JsonRpcInternalException""
        }
    }
}".TrimAllLines();

        using var request = new StringContent(requestJson, Encoding.UTF8, "application/json");
        var response = await ApiClient.PostAsync(JsonRpcUrl, request);
        var actualResponseJson = (await response.Content.ReadAsStringAsync()).TrimAllLines();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        actualResponseJson.Should().Be(expectedResponseJson);
    }

    [Test]
    public async Task Request_MethodEmpty_ReturnError()
    {
        const string requestJson = @"{
    ""jsonrpc"": ""2.0"",
    ""method"": """",
    ""id"": ""123"",
    ""params"": {
        ""bool_field"": true,
        ""string_field"": ""123"",
        ""int_field"": 123,
        ""double_field"": 1.23,
        ""enum_field"": ""two"",
        ""nullable_field"": null
    }
}";
        var expectedResponseJson = @"{
    ""id"": null,
    ""jsonrpc"": ""2.0"",
    ""error"": {
        ""code"": -32600,
        ""message"": ""Invalid Request"",
        ""data"": {
            ""internal_http_code"": null,
            ""message"": ""Method is null or empty"",
            ""details"": null,
            ""type"": ""Tochka.JsonRpc.V1.Server.Exceptions.JsonRpcInternalException""
        }
    }
}".TrimAllLines();

        using var request = new StringContent(requestJson, Encoding.UTF8, "application/json");
        var response = await ApiClient.PostAsync(JsonRpcUrl, request);
        var actualResponseJson = (await response.Content.ReadAsStringAsync()).TrimAllLines();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        actualResponseJson.Should().Be(expectedResponseJson);
    }

    [Test]
    public async Task Request_MethodWhiteSpace_ReturnError() // Should return "Method is null or empty"?
    {
        const string requestJson = @"{
    ""jsonrpc"": ""2.0"",
    ""method"": "" "",
    ""id"": ""123"",
    ""params"": {
        ""bool_field"": true,
        ""string_field"": ""123"",
        ""int_field"": 123,
        ""double_field"": 1.23,
        ""enum_field"": ""two"",
        ""nullable_field"": null
    }
}";
        var expectedResponseJson = @"{
    ""id"": ""123"",
    ""jsonrpc"": ""2.0"",
    ""error"": {
        ""code"": -32601,
        ""message"": ""Method not found"",
        ""data"": null
    }
}".TrimAllLines();

        using var request = new StringContent(requestJson, Encoding.UTF8, "application/json");
        var response = await ApiClient.PostAsync(JsonRpcUrl, request);
        var actualResponseJson = (await response.Content.ReadAsStringAsync()).TrimAllLines();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        actualResponseJson.Should().Be(expectedResponseJson);
    }

    [Test]
    public async Task Request_NoRequiredParams_ReturnError()
    {
        const string requestJson = @"{
    ""jsonrpc"": ""2.0"",
    ""method"": ""process_anything"",
    ""id"": ""123""
}";
        var expectedResponseJson = @"{
    ""id"": ""123"",
    ""jsonrpc"": ""2.0"",
    ""error"": {
        ""code"": -32602,
        ""message"": ""Invalid params"",
        ""data"": {
            """": [
                ""The data field is required.""
            ]
        }
    }
}".TrimAllLines();

        using var request = new StringContent(requestJson, Encoding.UTF8, "application/json");
        var response = await ApiClient.PostAsync(JsonRpcUrl, request);
        var actualResponseJson = (await response.Content.ReadAsStringAsync()).TrimAllLines();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        actualResponseJson.Should().Be(expectedResponseJson);
    }

    [Test]
    public async Task Request_EmptyRequiredParams_ReturnError()
    {
        const string requestJson = @"{
    ""jsonrpc"": ""2.0"",
    ""method"": ""process_anything"",
    ""id"": ""123"",
    ""params"": {}
}";
        var expectedResponseJson = @"{
    ""id"": ""123"",
    ""jsonrpc"": ""2.0"",
    ""error"": {
        ""code"": -32602,
        ""message"": ""Invalid params"",
        ""data"": {
            ""StringField"": [
                ""The StringField field is required.""
            ]
        }
    }
}".TrimAllLines();

        using var request = new StringContent(requestJson, Encoding.UTF8, "application/json");
        var response = await ApiClient.PostAsync(JsonRpcUrl, request);
        var actualResponseJson = (await response.Content.ReadAsStringAsync()).TrimAllLines();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        actualResponseJson.Should().Be(expectedResponseJson);
    }

    [Test]
    public async Task Request_RequiredParamsNull_ReturnError()
    {
        const string requestJson = @"{
    ""jsonrpc"": ""2.0"",
    ""method"": ""process_anything"",
    ""id"": ""123"",
    ""params"": null
}";
        var expectedResponseJson = @"{
    ""id"": ""123"",
    ""jsonrpc"": ""2.0"",
    ""error"": {
        ""code"": -32602,
        ""message"": ""Invalid params"",
        ""data"": {
            """": [
                ""The data field is required.""
            ]
        }
    }
}".TrimAllLines();

        using var request = new StringContent(requestJson, Encoding.UTF8, "application/json");
        var response = await ApiClient.PostAsync(JsonRpcUrl, request);
        var actualResponseJson = (await response.Content.ReadAsStringAsync()).TrimAllLines();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        actualResponseJson.Should().Be(expectedResponseJson);
    }

    [Test]
    public async Task Request_InvalidJsonRpcVersion_ReturnError() // Should id be null?
    {
        const string requestJson = @"{
    ""jsonrpc"": ""3.0"",
    ""method"": ""process_anything"",
    ""id"": ""123"",
    ""params"": {
        ""bool_field"": true,
        ""string_field"": ""123"",
        ""int_field"": 123,
        ""double_field"": 1.23,
        ""enum_field"": ""two"",
        ""nullable_field"": null
    }
}";
        var expectedResponseJson = @"{
    ""id"": null,
    ""jsonrpc"": ""2.0"",
    ""error"": {
        ""code"": -32600,
        ""message"": ""Invalid Request"",
        ""data"": {
            ""internal_http_code"": null,
            ""message"": ""Invalid JSON Rpc version [3.0]"",
            ""details"": null,
            ""type"": ""Tochka.JsonRpc.V1.Server.Exceptions.JsonRpcInternalException""
        }
    }
}".TrimAllLines();

        using var request = new StringContent(requestJson, Encoding.UTF8, "application/json");
        var response = await ApiClient.PostAsync(JsonRpcUrl, request);
        var actualResponseJson = (await response.Content.ReadAsStringAsync()).TrimAllLines();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        actualResponseJson.Should().Be(expectedResponseJson);
    }

    [Test]
    public async Task Request_InvalidJson_ReturnError()
    {
        const string requestJson = @"{
    123
    ""jsonrpc"": ""2.0"",
    ""method"": ""process_anything"",
    ""id"": ""123"",
    ""params"": {
        ""bool_field"": true,
        ""string_field"": ""123"",
        ""int_field"": 123,
        ""double_field"": 1.23,
        ""enum_field"": ""two"",
        ""nullable_field"": null
    }
}";
        var expectedResponseJson = @"{
    ""id"": null,
    ""jsonrpc"": ""2.0"",
    ""error"": {
        ""code"": -32700,
        ""message"": ""Parse error"",
        ""data"": {
            ""internal_http_code"": null,
            ""message"": ""Invalid character after parsing property name. Expected ':' but got: \"". Path '', line 3, position 4."",
            ""details"": null,
            ""type"": ""Newtonsoft.Json.JsonReaderException""
        }
    }
}".TrimAllLines();

        using var request = new StringContent(requestJson, Encoding.UTF8, "application/json");
        var response = await ApiClient.PostAsync(JsonRpcUrl, request);
        var actualResponseJson = (await response.Content.ReadAsStringAsync()).TrimAllLines();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        actualResponseJson.Should().Be(expectedResponseJson);
    }

    [Test]
    [Ignore("Processes successfully, but should?")]
    public async Task Request_NoJsonRpc_ReturnError()
    {
        const string requestJson = @"{
    ""method"": ""process_anything"",
    ""id"": ""123"",
    ""params"": {
        ""bool_field"": true,
        ""string_field"": ""123"",
        ""int_field"": 123,
        ""double_field"": 1.23,
        ""enum_field"": ""two"",
        ""nullable_field"": null
    }
}";
        var expectedResponseJson = @"{
    ""id"": null,
    ""jsonrpc"": ""2.0"",
    ""error"": {
        ""code"": TODO,
        ""message"": ""TODO"",
        ""data"": {TODO}
    }
}".TrimAllLines();

        using var request = new StringContent(requestJson, Encoding.UTF8, "application/json");
        var response = await ApiClient.PostAsync(JsonRpcUrl, request);
        var actualResponseJson = (await response.Content.ReadAsStringAsync()).TrimAllLines();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        actualResponseJson.Should().Be(expectedResponseJson);
    }

    [Test]
    public async Task Batch_StringId_DeserializeSuccessfully()
    {
        const string requestJson = @"[
    {
        ""jsonrpc"": ""2.0"",
        ""method"": ""process_anything"",
        ""id"": ""123"",
        ""params"": {
            ""bool_field"": true,
            ""string_field"": ""123"",
            ""int_field"": 123,
            ""double_field"": 1.23,
            ""enum_field"": ""two"",
            ""nullable_field"": null
        }
    },
    {
        ""jsonrpc"": ""2.0"",
        ""method"": ""process_anything"",
        ""id"": ""456"",
        ""params"": {
            ""bool_field"": true,
            ""string_field"": ""123"",
            ""int_field"": 123,
            ""double_field"": 1.23,
            ""enum_field"": ""two"",
            ""nullable_field"": null
        }
    }
]";
        var expectedRequestData = TestData.Plain;
        var responseData = TestData.Plain;
        var expectedResponseJson = @"[
    {
        ""id"": ""123"",
        ""jsonrpc"": ""2.0"",
        ""result"": {
            ""bool_field"": true,
            ""string_field"": ""123"",
            ""int_field"": 123,
            ""double_field"": 1.23,
            ""enum_field"": ""two"",
            ""nullable_field"": null,
            ""not_required_field"": null,
            ""nested_field"": null
        }
    },
    {
        ""id"": ""456"",
        ""jsonrpc"": ""2.0"",
        ""result"": {
            ""bool_field"": true,
            ""string_field"": ""123"",
            ""int_field"": 123,
            ""double_field"": 1.23,
            ""enum_field"": ""two"",
            ""nullable_field"": null,
            ""not_required_field"": null,
            ""nested_field"": null
        }
    }
]".TrimAllLines();

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
        const string requestJson = @"[
    {
        ""jsonrpc"": ""2.0"",
        ""method"": ""process_anything"",
        ""id"": 123,
        ""params"": {
            ""bool_field"": true,
            ""string_field"": ""123"",
            ""int_field"": 123,
            ""double_field"": 1.23,
            ""enum_field"": ""two"",
            ""nullable_field"": null
        }
    },
    {
        ""jsonrpc"": ""2.0"",
        ""method"": ""process_anything"",
        ""id"": 456,
        ""params"": {
            ""bool_field"": true,
            ""string_field"": ""123"",
            ""int_field"": 123,
            ""double_field"": 1.23,
            ""enum_field"": ""two"",
            ""nullable_field"": null
        }
    }
]";
        var expectedRequestData = TestData.Plain;
        var responseData = TestData.Plain;
        var expectedResponseJson = @"[
    {
        ""id"": 123,
        ""jsonrpc"": ""2.0"",
        ""result"": {
            ""bool_field"": true,
            ""string_field"": ""123"",
            ""int_field"": 123,
            ""double_field"": 1.23,
            ""enum_field"": ""two"",
            ""nullable_field"": null,
            ""not_required_field"": null,
            ""nested_field"": null
        }
    },
    {
        ""id"": 456,
        ""jsonrpc"": ""2.0"",
        ""result"": {
            ""bool_field"": true,
            ""string_field"": ""123"",
            ""int_field"": 123,
            ""double_field"": 1.23,
            ""enum_field"": ""two"",
            ""nullable_field"": null,
            ""not_required_field"": null,
            ""nested_field"": null
        }
    }
]".TrimAllLines();

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
        const string requestJson = @"[
    {
        ""jsonrpc"": ""2.0"",
        ""method"": ""process_anything"",
        ""id"": null,
        ""params"": {
            ""bool_field"": true,
            ""string_field"": ""123"",
            ""int_field"": 123,
            ""double_field"": 1.23,
            ""enum_field"": ""two"",
            ""nullable_field"": null
        }
    }
]";
        var expectedRequestData = TestData.Plain;
        var responseData = TestData.Plain;
        var expectedResponseJson = @"[
    {
        ""id"": null,
        ""jsonrpc"": ""2.0"",
        ""result"": {
            ""bool_field"": true,
            ""string_field"": ""123"",
            ""int_field"": 123,
            ""double_field"": 1.23,
            ""enum_field"": ""two"",
            ""nullable_field"": null,
            ""not_required_field"": null,
            ""nested_field"": null
        }
    }
]".TrimAllLines();

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
        const string requestJson = @"[
    {
        ""jsonrpc"": ""2.0"",
        ""method"": ""action_only"",
        ""id"": ""123"",
        ""params"": {
            ""bool_field"": true,
            ""string_field"": ""123"",
            ""int_field"": 123,
            ""double_field"": 1.23,
            ""enum_field"": ""two"",
            ""nullable_field"": null
        }
    },
    {
        ""jsonrpc"": ""2.0"",
        ""method"": ""action_only"",
        ""id"": ""456"",
        ""params"": {
            ""bool_field"": true,
            ""string_field"": ""123"",
            ""int_field"": 123,
            ""double_field"": 1.23,
            ""enum_field"": ""two"",
            ""nullable_field"": null
        }
    }
]";
        var expectedRequestData = TestData.Plain;
        var responseData = TestData.Plain;
        var expectedResponseJson = @"[
    {
        ""id"": ""123"",
        ""jsonrpc"": ""2.0"",
        ""result"": {
            ""bool_field"": true,
            ""string_field"": ""123"",
            ""int_field"": 123,
            ""double_field"": 1.23,
            ""enum_field"": ""two"",
            ""nullable_field"": null,
            ""not_required_field"": null,
            ""nested_field"": null
        }
    },
    {
        ""id"": ""456"",
        ""jsonrpc"": ""2.0"",
        ""result"": {
            ""bool_field"": true,
            ""string_field"": ""123"",
            ""int_field"": 123,
            ""double_field"": 1.23,
            ""enum_field"": ""two"",
            ""nullable_field"": null,
            ""not_required_field"": null,
            ""nested_field"": null
        }
    }
]".TrimAllLines();

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
        const string requestJson = @"[
    {
        ""jsonrpc"": ""2.0"",
        ""method"": ""simple_json_rpc.controller_and_action"",
        ""id"": ""123"",
        ""params"": {
            ""bool_field"": true,
            ""string_field"": ""123"",
            ""int_field"": 123,
            ""double_field"": 1.23,
            ""enum_field"": ""two"",
            ""nullable_field"": null
        }
    },
    {
        ""jsonrpc"": ""2.0"",
        ""method"": ""simple_json_rpc.controller_and_action"",
        ""id"": ""456"",
        ""params"": {
            ""bool_field"": true,
            ""string_field"": ""123"",
            ""int_field"": 123,
            ""double_field"": 1.23,
            ""enum_field"": ""two"",
            ""nullable_field"": null
        }
    }
]";
        var expectedRequestData = TestData.Plain;
        var responseData = TestData.Plain;
        var expectedResponseJson = @"[
    {
        ""id"": ""123"",
        ""jsonrpc"": ""2.0"",
        ""result"": {
            ""bool_field"": true,
            ""string_field"": ""123"",
            ""int_field"": 123,
            ""double_field"": 1.23,
            ""enum_field"": ""two"",
            ""nullable_field"": null,
            ""not_required_field"": null,
            ""nested_field"": null
        }
    },
    {
        ""id"": ""456"",
        ""jsonrpc"": ""2.0"",
        ""result"": {
            ""bool_field"": true,
            ""string_field"": ""123"",
            ""int_field"": 123,
            ""double_field"": 1.23,
            ""enum_field"": ""two"",
            ""nullable_field"": null,
            ""not_required_field"": null,
            ""nested_field"": null
        }
    }
]".TrimAllLines();

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
        const string requestJson = @"[
    {
        ""jsonrpc"": ""2.0"",
        ""method"": ""binding_style_default"",
        ""id"": ""123"",
        ""params"": {
            ""data"": {
                ""bool_field"": true,
                ""string_field"": ""123"",
                ""int_field"": 123,
                ""double_field"": 1.23,
                ""enum_field"": ""two"",
                ""nullable_field"": null
            }
        }
    },
    {
        ""jsonrpc"": ""2.0"",
        ""method"": ""binding_style_default"",
        ""id"": ""456"",
        ""params"": {
            ""data"": {
                ""bool_field"": true,
                ""string_field"": ""123"",
                ""int_field"": 123,
                ""double_field"": 1.23,
                ""enum_field"": ""two"",
                ""nullable_field"": null
            }
        }
    }
]";
        var expectedRequestData = TestData.Plain;
        var responseData = TestData.Plain;
        var expectedResponseJson = @"[
    {
        ""id"": ""123"",
        ""jsonrpc"": ""2.0"",
        ""result"": {
            ""bool_field"": true,
            ""string_field"": ""123"",
            ""int_field"": 123,
            ""double_field"": 1.23,
            ""enum_field"": ""two"",
            ""nullable_field"": null,
            ""not_required_field"": null,
            ""nested_field"": null
        }
    },
    {
        ""id"": ""456"",
        ""jsonrpc"": ""2.0"",
        ""result"": {
            ""bool_field"": true,
            ""string_field"": ""123"",
            ""int_field"": 123,
            ""double_field"": 1.23,
            ""enum_field"": ""two"",
            ""nullable_field"": null,
            ""not_required_field"": null,
            ""nested_field"": null
        }
    }
]".TrimAllLines();

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
        const string requestJson = @"[
    {
        ""jsonrpc"": ""2.0"",
        ""method"": ""binding_style_default"",
        ""id"": ""123"",
        ""params"": [
            {
                ""bool_field"": true,
                ""string_field"": ""123"",
                ""int_field"": 123,
                ""double_field"": 1.23,
                ""enum_field"": ""two"",
                ""nullable_field"": null
            }
        ]
    },
    {
        ""jsonrpc"": ""2.0"",
        ""method"": ""binding_style_default"",
        ""id"": ""456"",
        ""params"": [
            {
                ""bool_field"": true,
                ""string_field"": ""123"",
                ""int_field"": 123,
                ""double_field"": 1.23,
                ""enum_field"": ""two"",
                ""nullable_field"": null
            }
        ]
    }
]";
        var expectedRequestData = TestData.Plain;
        var responseData = TestData.Plain;
        var expectedResponseJson = @"[
    {
        ""id"": ""123"",
        ""jsonrpc"": ""2.0"",
        ""result"": {
            ""bool_field"": true,
            ""string_field"": ""123"",
            ""int_field"": 123,
            ""double_field"": 1.23,
            ""enum_field"": ""two"",
            ""nullable_field"": null,
            ""not_required_field"": null,
            ""nested_field"": null
        }
    },
    {
        ""id"": ""456"",
        ""jsonrpc"": ""2.0"",
        ""result"": {
            ""bool_field"": true,
            ""string_field"": ""123"",
            ""int_field"": 123,
            ""double_field"": 1.23,
            ""enum_field"": ""two"",
            ""nullable_field"": null,
            ""not_required_field"": null,
            ""nested_field"": null
        }
    }
]".TrimAllLines();

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
    public async Task Batch_BindingStyleObject_DeserializeSuccessfully()
    {
        const string requestJson = @"[
    {
        ""jsonrpc"": ""2.0"",
        ""method"": ""binding_style_object"",
        ""id"": ""123"",
        ""params"": {
            ""bool_field"": true,
            ""string_field"": ""123"",
            ""int_field"": 123,
            ""double_field"": 1.23,
            ""enum_field"": ""two"",
            ""nullable_field"": null
        }
    },
    {
        ""jsonrpc"": ""2.0"",
        ""method"": ""binding_style_object"",
        ""id"": ""456"",
        ""params"": {
            ""bool_field"": true,
            ""string_field"": ""123"",
            ""int_field"": 123,
            ""double_field"": 1.23,
            ""enum_field"": ""two"",
            ""nullable_field"": null
        }
    }
]";
        var expectedRequestData = TestData.Plain;
        var responseData = TestData.Plain;
        var expectedResponseJson = @"[
    {
        ""id"": ""123"",
        ""jsonrpc"": ""2.0"",
        ""result"": {
            ""bool_field"": true,
            ""string_field"": ""123"",
            ""int_field"": 123,
            ""double_field"": 1.23,
            ""enum_field"": ""two"",
            ""nullable_field"": null,
            ""not_required_field"": null,
            ""nested_field"": null
        }
    },
    {
        ""id"": ""456"",
        ""jsonrpc"": ""2.0"",
        ""result"": {
            ""bool_field"": true,
            ""string_field"": ""123"",
            ""int_field"": 123,
            ""double_field"": 1.23,
            ""enum_field"": ""two"",
            ""nullable_field"": null,
            ""not_required_field"": null,
            ""nested_field"": null
        }
    }
]".TrimAllLines();

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
    public async Task Batch_BindingStyleArray_DeserializeSuccessfully()
    {
        const string requestJson = @"[
    {
        ""jsonrpc"": ""2.0"",
        ""method"": ""binding_style_array"",
        ""id"": ""123"",
        ""params"": [
            {
                ""bool_field"": true,
                ""string_field"": ""123"",
                ""int_field"": 123,
                ""double_field"": 1.23,
                ""enum_field"": ""two"",
                ""nullable_field"": null
            }
        ]
    },
    {
        ""jsonrpc"": ""2.0"",
        ""method"": ""binding_style_array"",
        ""id"": ""456"",
        ""params"": [
            {
                ""bool_field"": true,
                ""string_field"": ""123"",
                ""int_field"": 123,
                ""double_field"": 1.23,
                ""enum_field"": ""two"",
                ""nullable_field"": null
            }
        ]
    }
]";
        var expectedRequestData = TestData.Plain;
        var responseData = TestData.Plain;
        var expectedResponseJson = @"[
    {
        ""id"": ""123"",
        ""jsonrpc"": ""2.0"",
        ""result"": {
            ""bool_field"": true,
            ""string_field"": ""123"",
            ""int_field"": 123,
            ""double_field"": 1.23,
            ""enum_field"": ""two"",
            ""nullable_field"": null,
            ""not_required_field"": null,
            ""nested_field"": null
        }
    },
    {
        ""id"": ""456"",
        ""jsonrpc"": ""2.0"",
        ""result"": {
            ""bool_field"": true,
            ""string_field"": ""123"",
            ""int_field"": 123,
            ""double_field"": 1.23,
            ""enum_field"": ""two"",
            ""nullable_field"": null,
            ""not_required_field"": null,
            ""nested_field"": null
        }
    }
]".TrimAllLines();

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
    public async Task Batch_NoParams_ProcessSuccessfully()
    {
        const string requestJson = @"[
    {
        ""jsonrpc"": ""2.0"",
        ""method"": ""no_params"",
        ""id"": ""123""
    },
    {
        ""jsonrpc"": ""2.0"",
        ""method"": ""no_params"",
        ""id"": ""456""
    }
]";
        var responseData = TestData.Plain;
        var expectedResponseJson = @"[
    {
        ""id"": ""123"",
        ""jsonrpc"": ""2.0"",
        ""result"": {
            ""bool_field"": true,
            ""string_field"": ""123"",
            ""int_field"": 123,
            ""double_field"": 1.23,
            ""enum_field"": ""two"",
            ""nullable_field"": null,
            ""not_required_field"": null,
            ""nested_field"": null
        }
    },
    {
        ""id"": ""456"",
        ""jsonrpc"": ""2.0"",
        ""result"": {
            ""bool_field"": true,
            ""string_field"": ""123"",
            ""int_field"": 123,
            ""double_field"": 1.23,
            ""enum_field"": ""two"",
            ""nullable_field"": null,
            ""not_required_field"": null,
            ""nested_field"": null
        }
    }
]".TrimAllLines();

        responseProviderMock.Setup(static p => p.GetJsonRpcResponse())
            .Returns(responseData);

        using var request = new StringContent(requestJson, Encoding.UTF8, "application/json");
        var response = await ApiClient.PostAsync(JsonRpcUrl, request);
        var actualResponseJson = (await response.Content.ReadAsStringAsync()).TrimAllLines();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        actualResponseJson.Should().Be(expectedResponseJson);
    }

    [Test]
    public async Task Batch_NullParamsWithNullParams_DeserializeSuccessfully()
    {
        const string requestJson = @"[
    {
        ""jsonrpc"": ""2.0"",
        ""method"": ""null_params"",
        ""id"": ""123"",
        ""params"": null
    },
    {
        ""jsonrpc"": ""2.0"",
        ""method"": ""null_params"",
        ""id"": ""456"",
        ""params"": null
    }
]";
        object? expectedRequestData = null;
        var responseData = TestData.Plain;
        var expectedResponseJson = @"[
    {
        ""id"": ""123"",
        ""jsonrpc"": ""2.0"",
        ""result"": {
            ""bool_field"": true,
            ""string_field"": ""123"",
            ""int_field"": 123,
            ""double_field"": 1.23,
            ""enum_field"": ""two"",
            ""nullable_field"": null,
            ""not_required_field"": null,
            ""nested_field"": null
        }
    },
    {
        ""id"": ""456"",
        ""jsonrpc"": ""2.0"",
        ""result"": {
            ""bool_field"": true,
            ""string_field"": ""123"",
            ""int_field"": 123,
            ""double_field"": 1.23,
            ""enum_field"": ""two"",
            ""nullable_field"": null,
            ""not_required_field"": null,
            ""nested_field"": null
        }
    }
]".TrimAllLines();

        var actualRequestData = new List<object?>();
        requestValidatorMock.Setup(static v => v.Validate(It.IsAny<object?>()))
            .Callback<object?>(requestData => actualRequestData.Add(requestData));
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
    public async Task Batch_NullParamsWithoutParams_DeserializeSuccessfully()
    {
        const string requestJson = @"[
    {
        ""jsonrpc"": ""2.0"",
        ""method"": ""null_params"",
        ""id"": ""123""
    },
    {
        ""jsonrpc"": ""2.0"",
        ""method"": ""null_params"",
        ""id"": ""456""
    }
]";
        object? expectedRequestData = null;
        var responseData = TestData.Plain;
        var expectedResponseJson = @"[
    {
        ""id"": ""123"",
        ""jsonrpc"": ""2.0"",
        ""result"": {
            ""bool_field"": true,
            ""string_field"": ""123"",
            ""int_field"": 123,
            ""double_field"": 1.23,
            ""enum_field"": ""two"",
            ""nullable_field"": null,
            ""not_required_field"": null,
            ""nested_field"": null
        }
    },
    {
        ""id"": ""456"",
        ""jsonrpc"": ""2.0"",
        ""result"": {
            ""bool_field"": true,
            ""string_field"": ""123"",
            ""int_field"": 123,
            ""double_field"": 1.23,
            ""enum_field"": ""two"",
            ""nullable_field"": null,
            ""not_required_field"": null,
            ""nested_field"": null
        }
    }
]".TrimAllLines();

        var actualRequestData = new List<object?>();
        requestValidatorMock.Setup(static v => v.Validate(It.IsAny<object?>()))
            .Callback<object?>(requestData => actualRequestData.Add(requestData));
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
    public async Task Batch_DefaultParamsWithEmptyParams_DeserializeSuccessfully()
    {
        const string requestJson = @"[
    {
        ""jsonrpc"": ""2.0"",
        ""method"": ""default_params"",
        ""id"": ""123"",
        ""params"": {}
    },
    {
        ""jsonrpc"": ""2.0"",
        ""method"": ""default_params"",
        ""id"": ""456"",
        ""params"": {}
    }
]";
        var expectedRequestData = "123";
        var responseData = TestData.Plain;
        var expectedResponseJson = @"[
    {
        ""id"": ""123"",
        ""jsonrpc"": ""2.0"",
        ""result"": {
            ""bool_field"": true,
            ""string_field"": ""123"",
            ""int_field"": 123,
            ""double_field"": 1.23,
            ""enum_field"": ""two"",
            ""nullable_field"": null,
            ""not_required_field"": null,
            ""nested_field"": null
        }
    },
    {
        ""id"": ""456"",
        ""jsonrpc"": ""2.0"",
        ""result"": {
            ""bool_field"": true,
            ""string_field"": ""123"",
            ""int_field"": 123,
            ""double_field"": 1.23,
            ""enum_field"": ""two"",
            ""nullable_field"": null,
            ""not_required_field"": null,
            ""nested_field"": null
        }
    }
]".TrimAllLines();

        var actualRequestData = new List<object?>();
        requestValidatorMock.Setup(static v => v.Validate(It.IsAny<object?>()))
            .Callback<object?>(requestData => actualRequestData.Add(requestData));
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
    [Ignore("Doesnt work, but should?")]
    public async Task Batch_DefaultParamsWithoutParams_DeserializeSuccessfully()
    {
        const string requestJson = @"[
    {
        ""jsonrpc"": ""2.0"",
        ""method"": ""default_params"",
        ""id"": ""123""
    },
    {
        ""jsonrpc"": ""2.0"",
        ""method"": ""default_params"",
        ""id"": ""456""
    }
]";
        var expectedRequestData = "123";
        var responseData = TestData.Plain;
        var expectedResponseJson = @"[
    {
        ""id"": ""123"",
        ""jsonrpc"": ""2.0"",
        ""result"": {
            ""bool_field"": true,
            ""string_field"": ""123"",
            ""int_field"": 123,
            ""double_field"": 1.23,
            ""enum_field"": ""two"",
            ""nullable_field"": null,
            ""not_required_field"": null,
            ""nested_field"": null
        }
    },
    {
        ""id"": ""456"",
        ""jsonrpc"": ""2.0"",
        ""result"": {
            ""bool_field"": true,
            ""string_field"": ""123"",
            ""int_field"": 123,
            ""double_field"": 1.23,
            ""enum_field"": ""two"",
            ""nullable_field"": null,
            ""not_required_field"": null,
            ""nested_field"": null
        }
    }
]".TrimAllLines();

        var actualRequestData = new List<object?>();
        requestValidatorMock.Setup(static v => v.Validate(It.IsAny<object?>()))
            .Callback<object?>(requestData => actualRequestData.Add(requestData));
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
    public async Task Batch_SnakeCaseParams_DeserializeSuccessfully()
    {
        const string requestJson = @"[
    {
        ""jsonrpc"": ""2.0"",
        ""method"": ""snake_case_params"",
        ""id"": ""123"",
        ""params"": {
            ""bool_field"": true,
            ""string_field"": ""123"",
            ""int_field"": 123,
            ""double_field"": 1.23,
            ""enum_field"": ""two"",
            ""nullable_field"": null
        }
    },
    {
        ""jsonrpc"": ""2.0"",
        ""method"": ""snake_case_params"",
        ""id"": ""456"",
        ""params"": {
            ""bool_field"": true,
            ""string_field"": ""123"",
            ""int_field"": 123,
            ""double_field"": 1.23,
            ""enum_field"": ""two"",
            ""nullable_field"": null
        }
    }
]";
        var expectedRequestData = TestData.Plain;
        var responseData = TestData.Plain;
        var expectedResponseJson = @"[
    {
        ""id"": ""123"",
        ""jsonrpc"": ""2.0"",
        ""result"": {
            ""bool_field"": true,
            ""string_field"": ""123"",
            ""int_field"": 123,
            ""double_field"": 1.23,
            ""enum_field"": ""two"",
            ""nullable_field"": null,
            ""not_required_field"": null,
            ""nested_field"": null
        }
    },
    {
        ""id"": ""456"",
        ""jsonrpc"": ""2.0"",
        ""result"": {
            ""bool_field"": true,
            ""string_field"": ""123"",
            ""int_field"": 123,
            ""double_field"": 1.23,
            ""enum_field"": ""two"",
            ""nullable_field"": null,
            ""not_required_field"": null,
            ""nested_field"": null
        }
    }
]".TrimAllLines();

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
        const string requestJson = @"[
    {
        ""jsonrpc"": ""2.0"",
        ""method"": ""camelCaseParams"",
        ""id"": ""123"",
        ""params"": {
            ""boolField"": true,
            ""stringField"": ""123"",
            ""intField"": 123,
            ""doubleField"": 1.23,
            ""enumField"": ""two"",
            ""nullableField"": null
        }
    },
    {
        ""jsonrpc"": ""2.0"",
        ""method"": ""camelCaseParams"",
        ""id"": ""456"",
        ""params"": {
            ""boolField"": true,
            ""stringField"": ""123"",
            ""intField"": 123,
            ""doubleField"": 1.23,
            ""enumField"": ""two"",
            ""nullableField"": null
        }
    }
]";
        var expectedRequestData = TestData.Plain;
        var responseData = TestData.Plain;
        var expectedResponseJson = @"[
    {
        ""id"": ""123"",
        ""jsonrpc"": ""2.0"",
        ""result"": {
            ""boolField"": true,
            ""stringField"": ""123"",
            ""intField"": 123,
            ""doubleField"": 1.23,
            ""enumField"": ""two"",
            ""nullableField"": null,
            ""notRequiredField"": null,
            ""nestedField"": null
        }
    },
    {
        ""id"": ""456"",
        ""jsonrpc"": ""2.0"",
        ""result"": {
            ""boolField"": true,
            ""stringField"": ""123"",
            ""intField"": 123,
            ""doubleField"": 1.23,
            ""enumField"": ""two"",
            ""nullableField"": null,
            ""notRequiredField"": null,
            ""nestedField"": null
        }
    }
]".TrimAllLines();

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
        const string requestJson = @"[
    {
        ""jsonrpc"": ""2.0"",
        ""method"": ""snake_case_params"",
        ""id"": ""123"",
        ""params"": {
            ""bool_field"": true,
            ""string_field"": ""123"",
            ""int_field"": 123,
            ""double_field"": 1.23,
            ""enum_field"": ""two"",
            ""nullable_field"": null,
            ""nested_field"": {
                ""bool_field"": true,
                ""string_field"": ""456"",
                ""int_field"": 456,
                ""double_field"": 4.56,
                ""enum_field"": ""two"",
                ""nullable_field"": null
            }
        }
    },
    {
        ""jsonrpc"": ""2.0"",
        ""method"": ""snake_case_params"",
        ""id"": ""456"",
        ""params"": {
            ""bool_field"": true,
            ""string_field"": ""123"",
            ""int_field"": 123,
            ""double_field"": 1.23,
            ""enum_field"": ""two"",
            ""nullable_field"": null,
            ""nested_field"": {
                ""bool_field"": true,
                ""string_field"": ""456"",
                ""int_field"": 456,
                ""double_field"": 4.56,
                ""enum_field"": ""two"",
                ""nullable_field"": null
            }
        }
    }
]";
        var expectedRequestData = TestData.Nested;
        var responseData = TestData.Nested;
        var expectedResponseJson = @"[
    {
        ""id"": ""123"",
        ""jsonrpc"": ""2.0"",
        ""result"": {
            ""bool_field"": true,
            ""string_field"": ""123"",
            ""int_field"": 123,
            ""double_field"": 1.23,
            ""enum_field"": ""two"",
            ""nullable_field"": null,
            ""not_required_field"": null,
            ""nested_field"": {
                ""bool_field"": true,
                ""string_field"": ""456"",
                ""int_field"": 456,
                ""double_field"": 4.56,
                ""enum_field"": ""two"",
                ""nullable_field"": null,
                ""not_required_field"": null,
                ""nested_field"": null
            }
        }
    },
    {
        ""id"": ""456"",
        ""jsonrpc"": ""2.0"",
        ""result"": {
            ""bool_field"": true,
            ""string_field"": ""123"",
            ""int_field"": 123,
            ""double_field"": 1.23,
            ""enum_field"": ""two"",
            ""nullable_field"": null,
            ""not_required_field"": null,
            ""nested_field"": {
                ""bool_field"": true,
                ""string_field"": ""456"",
                ""int_field"": 456,
                ""double_field"": 4.56,
                ""enum_field"": ""two"",
                ""nullable_field"": null,
                ""not_required_field"": null,
                ""nested_field"": null
            }
        }
    }
]".TrimAllLines();

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
        const string requestJson = @"[
    {
        ""jsonrpc"": ""2.0"",
        ""method"": ""camelCaseParams"",
        ""id"": ""123"",
        ""params"": {
            ""boolField"": true,
            ""stringField"": ""123"",
            ""intField"": 123,
            ""doubleField"": 1.23,
            ""enumField"": ""two"",
            ""nullableField"": null,
            ""nestedField"": {
                ""boolField"": true,
                ""stringField"": ""456"",
                ""intField"": 456,
                ""doubleField"": 4.56,
                ""enumField"": ""two"",
                ""nullableField"": null
            }
        }
    },
    {
        ""jsonrpc"": ""2.0"",
        ""method"": ""camelCaseParams"",
        ""id"": ""456"",
        ""params"": {
            ""boolField"": true,
            ""stringField"": ""123"",
            ""intField"": 123,
            ""doubleField"": 1.23,
            ""enumField"": ""two"",
            ""nullableField"": null,
            ""nestedField"": {
                ""boolField"": true,
                ""stringField"": ""456"",
                ""intField"": 456,
                ""doubleField"": 4.56,
                ""enumField"": ""two"",
                ""nullableField"": null
            }
        }
    }
]";
        var expectedRequestData = TestData.Nested;
        var responseData = TestData.Nested;
        var expectedResponseJson = @"[
    {
        ""id"": ""123"",
        ""jsonrpc"": ""2.0"",
        ""result"": {
            ""boolField"": true,
            ""stringField"": ""123"",
            ""intField"": 123,
            ""doubleField"": 1.23,
            ""enumField"": ""two"",
            ""nullableField"": null,
            ""notRequiredField"": null,
            ""nestedField"": {
                ""boolField"": true,
                ""stringField"": ""456"",
                ""intField"": 456,
                ""doubleField"": 4.56,
                ""enumField"": ""two"",
                ""nullableField"": null,
                ""notRequiredField"": null,
                ""nestedField"": null
            }
        }
    },
    {
        ""id"": ""456"",
        ""jsonrpc"": ""2.0"",
        ""result"": {
            ""boolField"": true,
            ""stringField"": ""123"",
            ""intField"": 123,
            ""doubleField"": 1.23,
            ""enumField"": ""two"",
            ""nullableField"": null,
            ""notRequiredField"": null,
            ""nestedField"": {
                ""boolField"": true,
                ""stringField"": ""456"",
                ""intField"": 456,
                ""doubleField"": 4.56,
                ""enumField"": ""two"",
                ""nullableField"": null,
                ""notRequiredField"": null,
                ""nestedField"": null
            }
        }
    }
]".TrimAllLines();

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
        const string requestJson = @"[
    {
        ""jsonrpc"": ""2.0"",
        ""method"": ""process_anything"",
        ""id"": ""123"",
        ""params"": {
            ""bool_field"": true,
            ""string_field"": ""123"",
            ""int_field"": 123,
            ""double_field"": 1.23,
            ""enum_field"": ""two"",
            ""nullable_field"": null,
            ""additional_field"": ""something""
        }
    },
    {
        ""jsonrpc"": ""2.0"",
        ""method"": ""process_anything"",
        ""id"": ""456"",
        ""params"": {
            ""bool_field"": true,
            ""string_field"": ""123"",
            ""int_field"": 123,
            ""double_field"": 1.23,
            ""enum_field"": ""two"",
            ""nullable_field"": null,
            ""additional_field"": ""something""
        }
    }
]";
        var expectedRequestData = TestData.Plain;
        var responseData = TestData.Plain;
        var expectedResponseJson = @"[
    {
        ""id"": ""123"",
        ""jsonrpc"": ""2.0"",
        ""result"": {
            ""bool_field"": true,
            ""string_field"": ""123"",
            ""int_field"": 123,
            ""double_field"": 1.23,
            ""enum_field"": ""two"",
            ""nullable_field"": null,
            ""not_required_field"": null,
            ""nested_field"": null
        }
    },
    {
        ""id"": ""456"",
        ""jsonrpc"": ""2.0"",
        ""result"": {
            ""bool_field"": true,
            ""string_field"": ""123"",
            ""int_field"": 123,
            ""double_field"": 1.23,
            ""enum_field"": ""two"",
            ""nullable_field"": null,
            ""not_required_field"": null,
            ""nested_field"": null
        }
    }
]".TrimAllLines();

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
        const string requestJson = @"[
    {
        ""jsonrpc"": ""2.0"",
        ""method"": ""custom_action_route"",
        ""id"": ""123"",
        ""params"": {
            ""bool_field"": true,
            ""string_field"": ""123"",
            ""int_field"": 123,
            ""double_field"": 1.23,
            ""enum_field"": ""two"",
            ""nullable_field"": null
        }
    },
    {
        ""jsonrpc"": ""2.0"",
        ""method"": ""custom_action_route"",
        ""id"": ""456"",
        ""params"": {
            ""bool_field"": true,
            ""string_field"": ""123"",
            ""int_field"": 123,
            ""double_field"": 1.23,
            ""enum_field"": ""two"",
            ""nullable_field"": null
        }
    }
]";
        var expectedRequestData = TestData.Plain;
        var responseData = TestData.Plain;
        var expectedResponseJson = @"[
    {
        ""id"": ""123"",
        ""jsonrpc"": ""2.0"",
        ""result"": {
            ""bool_field"": true,
            ""string_field"": ""123"",
            ""int_field"": 123,
            ""double_field"": 1.23,
            ""enum_field"": ""two"",
            ""nullable_field"": null,
            ""not_required_field"": null,
            ""nested_field"": null
        }
    },
    {
        ""id"": ""456"",
        ""jsonrpc"": ""2.0"",
        ""result"": {
            ""bool_field"": true,
            ""string_field"": ""123"",
            ""int_field"": 123,
            ""double_field"": 1.23,
            ""enum_field"": ""two"",
            ""nullable_field"": null,
            ""not_required_field"": null,
            ""nested_field"": null
        }
    }
]".TrimAllLines();

        var actualRequestData = new List<TestData>();
        requestValidatorMock.Setup(static v => v.Validate(It.IsAny<TestData>()))
            .Callback<TestData>(requestData => actualRequestData.Add(requestData));
        responseProviderMock.Setup(static p => p.GetJsonRpcResponse())
            .Returns(responseData);

        using var request = new StringContent(requestJson, Encoding.UTF8, "application/json");
        var response = await ApiClient.PostAsync("/custom/action", request);
        var actualResponseJson = (await response.Content.ReadAsStringAsync()).TrimAllLines();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        actualRequestData.Should().AllBeEquivalentTo(expectedRequestData);
        actualResponseJson.Should().Be(expectedResponseJson);
    }

    [Test]
    public async Task Batch_CustomControllerRoute_DeserializeSuccessfully()
    {
        const string requestJson = @"[
    {
        ""jsonrpc"": ""2.0"",
        ""method"": ""custom_controller_route"",
        ""id"": ""123"",
        ""params"": {
            ""bool_field"": true,
            ""string_field"": ""123"",
            ""int_field"": 123,
            ""double_field"": 1.23,
            ""enum_field"": ""two"",
            ""nullable_field"": null
        }
    },
    {
        ""jsonrpc"": ""2.0"",
        ""method"": ""custom_controller_route"",
        ""id"": ""456"",
        ""params"": {
            ""bool_field"": true,
            ""string_field"": ""123"",
            ""int_field"": 123,
            ""double_field"": 1.23,
            ""enum_field"": ""two"",
            ""nullable_field"": null
        }
    }
]";
        var expectedRequestData = TestData.Plain;
        var responseData = TestData.Plain;
        var expectedResponseJson = @"[
    {
        ""id"": ""123"",
        ""jsonrpc"": ""2.0"",
        ""result"": {
            ""bool_field"": true,
            ""string_field"": ""123"",
            ""int_field"": 123,
            ""double_field"": 1.23,
            ""enum_field"": ""two"",
            ""nullable_field"": null,
            ""not_required_field"": null,
            ""nested_field"": null
        }
    },
    {
        ""id"": ""456"",
        ""jsonrpc"": ""2.0"",
        ""result"": {
            ""bool_field"": true,
            ""string_field"": ""123"",
            ""int_field"": 123,
            ""double_field"": 1.23,
            ""enum_field"": ""two"",
            ""nullable_field"": null,
            ""not_required_field"": null,
            ""nested_field"": null
        }
    }
]".TrimAllLines();

        var actualRequestData = new List<TestData>();
        requestValidatorMock.Setup(static v => v.Validate(It.IsAny<TestData>()))
            .Callback<TestData>(requestData => actualRequestData.Add(requestData));
        responseProviderMock.Setup(static p => p.GetJsonRpcResponse())
            .Returns(responseData);

        using var request = new StringContent(requestJson, Encoding.UTF8, "application/json");
        var response = await ApiClient.PostAsync("/custom/controller", request);
        var actualResponseJson = (await response.Content.ReadAsStringAsync()).TrimAllLines();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        actualRequestData.Should().AllBeEquivalentTo(expectedRequestData);
        actualResponseJson.Should().Be(expectedResponseJson);
    }

    [Test]
    public async Task Batch_RequestAndNotification_ReturnOnlyRequestResponse()
    {
        const string requestJson = @"[
    {
        ""jsonrpc"": ""2.0"",
        ""method"": ""process_anything"",
        ""id"": ""123"",
        ""params"": {
            ""bool_field"": true,
            ""string_field"": ""123"",
            ""int_field"": 123,
            ""double_field"": 1.23,
            ""enum_field"": ""two"",
            ""nullable_field"": null
        }
    },
    {
        ""jsonrpc"": ""2.0"",
        ""method"": ""process_anything"",
        ""params"": {
            ""bool_field"": true,
            ""string_field"": ""123"",
            ""int_field"": 123,
            ""double_field"": 1.23,
            ""enum_field"": ""two"",
            ""nullable_field"": null
        }
    }
]";
        var expectedRequestData = TestData.Plain;
        var responseData = TestData.Plain;
        var expectedResponseJson = @"[
    {
        ""id"": ""123"",
        ""jsonrpc"": ""2.0"",
        ""result"": {
            ""bool_field"": true,
            ""string_field"": ""123"",
            ""int_field"": 123,
            ""double_field"": 1.23,
            ""enum_field"": ""two"",
            ""nullable_field"": null,
            ""not_required_field"": null,
            ""nested_field"": null
        }
    }
]".TrimAllLines();

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
        const string requestJson = @"[
    {
        ""jsonrpc"": ""2.0"",
        ""method"": ""throw_exception"",
        ""id"": ""123""
    },
    {
        ""jsonrpc"": ""2.0"",
        ""method"": ""throw_exception"",
        ""id"": ""456""
    }
]";
        var expectedResponseJson = @"[
    {
        ""id"": ""123"",
        ""jsonrpc"": ""2.0"",
        ""error"": {
            ""code"": -32000,
            ""message"": ""Server error"",
            ""data"": {
                ""internal_http_code"": null,
                ""message"": ""Value does not fall within the expected range."",
                ""details"": null,
                ""type"": ""System.ArgumentException""
            }
        }
    },
    {
        ""id"": ""456"",
        ""jsonrpc"": ""2.0"",
        ""error"": {
            ""code"": -32000,
            ""message"": ""Server error"",
            ""data"": {
                ""internal_http_code"": null,
                ""message"": ""Value does not fall within the expected range."",
                ""details"": null,
                ""type"": ""System.ArgumentException""
            }
        }
    }
]".TrimAllLines();

        using var request = new StringContent(requestJson, Encoding.UTF8, "application/json");
        var response = await ApiClient.PostAsync(JsonRpcUrl, request);
        var actualResponseJson = (await response.Content.ReadAsStringAsync()).TrimAllLines();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        actualResponseJson.Should().Be(expectedResponseJson);
    }

    [Test]
    public async Task Batch_ReturnErrorFromFactory_SerializeSuccessfully()
    {
        const string requestJson = @"[
    {
        ""jsonrpc"": ""2.0"",
        ""method"": ""return_error_from_factory"",
        ""id"": ""123""
    },
    {
        ""jsonrpc"": ""2.0"",
        ""method"": ""return_error_from_factory"",
        ""id"": ""456""
    }
]";
        var expectedResponseJson = @"[
    {
        ""id"": ""123"",
        ""jsonrpc"": ""2.0"",
        ""error"": {
            ""code"": -32602,
            ""message"": ""Invalid params"",
            ""data"": ""errorMessage""
        }
    },
    {
        ""id"": ""456"",
        ""jsonrpc"": ""2.0"",
        ""error"": {
            ""code"": -32602,
            ""message"": ""Invalid params"",
            ""data"": ""errorMessage""
        }
    }
]".TrimAllLines();

        using var request = new StringContent(requestJson, Encoding.UTF8, "application/json");
        var response = await ApiClient.PostAsync(JsonRpcUrl, request);
        var actualResponseJson = (await response.Content.ReadAsStringAsync()).TrimAllLines();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        actualResponseJson.Should().Be(expectedResponseJson);
    }

    [Test]
    public async Task Batch_ReturnMvcError_SerializeSuccessfully()
    {
        const string requestJson = @"[
    {
        ""jsonrpc"": ""2.0"",
        ""method"": ""return_mvc_error"",
        ""id"": ""123""
    },
    {
        ""jsonrpc"": ""2.0"",
        ""method"": ""return_mvc_error"",
        ""id"": ""456""
    }
]";
        var expectedResponseJson = @"[
    {
        ""id"": ""123"",
        ""jsonrpc"": ""2.0"",
        ""error"": {
            ""code"": -32602,
            ""message"": ""Invalid params"",
            ""data"": ""errorMessage""
        }
    },
    {
        ""id"": ""456"",
        ""jsonrpc"": ""2.0"",
        ""error"": {
            ""code"": -32602,
            ""message"": ""Invalid params"",
            ""data"": ""errorMessage""
        }
    }
]".TrimAllLines();

        using var request = new StringContent(requestJson, Encoding.UTF8, "application/json");
        var response = await ApiClient.PostAsync(JsonRpcUrl, request);
        var actualResponseJson = (await response.Content.ReadAsStringAsync()).TrimAllLines();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        actualResponseJson.Should().Be(expectedResponseJson);
    }

    [Test]
    public async Task Batch_ErrorThrowAsResponseException_SerializeSuccessfully()
    {
        const string requestJson = @"[
    {
        ""jsonrpc"": ""2.0"",
        ""method"": ""error_throw_as_response_exception"",
        ""id"": ""123""
    },
    {
        ""jsonrpc"": ""2.0"",
        ""method"": ""error_throw_as_response_exception"",
        ""id"": ""456""
    }
]";
        var expectedResponseJson = @"[
    {
        ""id"": ""123"",
        ""jsonrpc"": ""2.0"",
        ""error"": {
            ""code"": -32602,
            ""message"": ""Invalid params"",
            ""data"": ""errorMessage""
        }
    },
    {
        ""id"": ""456"",
        ""jsonrpc"": ""2.0"",
        ""error"": {
            ""code"": -32602,
            ""message"": ""Invalid params"",
            ""data"": ""errorMessage""
        }
    }
]".TrimAllLines();

        using var request = new StringContent(requestJson, Encoding.UTF8, "application/json");
        var response = await ApiClient.PostAsync(JsonRpcUrl, request);
        var actualResponseJson = (await response.Content.ReadAsStringAsync()).TrimAllLines();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        actualResponseJson.Should().Be(expectedResponseJson);
    }

    [Test]
    public async Task Batch_UnknownMethod_ReturnError()
    {
        const string requestJson = @"[
    {
        ""jsonrpc"": ""2.0"",
        ""method"": ""some_not_existing_method"",
        ""id"": ""123""
    },
    {
        ""jsonrpc"": ""2.0"",
        ""method"": ""some_not_existing_method"",
        ""id"": ""456""
    }
]";
        var expectedResponseJson = @"[
    {
        ""id"": ""123"",
        ""jsonrpc"": ""2.0"",
        ""error"": {
            ""code"": -32601,
            ""message"": ""Method not found"",
            ""data"": null
        }
    },
    {
        ""id"": ""456"",
        ""jsonrpc"": ""2.0"",
        ""error"": {
            ""code"": -32601,
            ""message"": ""Method not found"",
            ""data"": null
        }
    }
]".TrimAllLines();

        using var request = new StringContent(requestJson, Encoding.UTF8, "application/json");
        var response = await ApiClient.PostAsync(JsonRpcUrl, request);
        var actualResponseJson = (await response.Content.ReadAsStringAsync()).TrimAllLines();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        actualResponseJson.Should().Be(expectedResponseJson);
    }

    [Test]
    [Ignore("Returns NullReferenceException - not like request")]
    public async Task Batch_NoMethod_ReturnError()
    {
        const string requestJson = @"[
    {
        ""jsonrpc"": ""2.0"",
        ""id"": ""123"",
        ""params"": {
            ""bool_field"": true,
            ""string_field"": ""123"",
            ""int_field"": 123,
            ""double_field"": 1.23,
            ""enum_field"": ""two"",
            ""nullable_field"": null
        }
    },
    {
        ""jsonrpc"": ""2.0"",
        ""id"": ""456"",
        ""params"": {
            ""bool_field"": true,
            ""string_field"": ""123"",
            ""int_field"": 123,
            ""double_field"": 1.23,
            ""enum_field"": ""two"",
            ""nullable_field"": null
        }
    }
]";
        var expectedResponseJson = @"[
    {
        ""id"": ""123"",
        ""jsonrpc"": ""2.0"",
        ""error"": {
            ""code"": -32600,
            ""message"": ""Invalid Request"",
            ""data"": {
                ""internal_http_code"": null,
                ""message"": ""Method is null or empty"",
                ""details"": null,
                ""type"": ""Tochka.JsonRpc.V1.Server.Exceptions.JsonRpcInternalException""
            }
        }
    },
    {
        ""id"": ""456"",
        ""jsonrpc"": ""2.0"",
        ""error"": {
            ""code"": -32600,
            ""message"": ""Invalid Request"",
            ""data"": {
                ""internal_http_code"": null,
                ""message"": ""Method is null or empty"",
                ""details"": null,
                ""type"": ""Tochka.JsonRpc.V1.Server.Exceptions.JsonRpcInternalException""
            }
        }
    }
]".TrimAllLines();

        using var request = new StringContent(requestJson, Encoding.UTF8, "application/json");
        var response = await ApiClient.PostAsync(JsonRpcUrl, request);
        var actualResponseJson = (await response.Content.ReadAsStringAsync()).TrimAllLines();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        actualResponseJson.Should().Be(expectedResponseJson);
    }

    [Test]
    [Ignore("Returns NullReferenceException - not like request")]
    public async Task Batch_MethodNull_ReturnError()
    {
        const string requestJson = @"[
    {
        ""jsonrpc"": ""2.0"",
        ""method"": null,
        ""id"": ""123"",
        ""params"": {
            ""bool_field"": true,
            ""string_field"": ""123"",
            ""int_field"": 123,
            ""double_field"": 1.23,
            ""enum_field"": ""two"",
            ""nullable_field"": null
        }
    },
    {
        ""jsonrpc"": ""2.0"",
        ""method"": null,
        ""id"": ""456"",
        ""params"": {
            ""bool_field"": true,
            ""string_field"": ""123"",
            ""int_field"": 123,
            ""double_field"": 1.23,
            ""enum_field"": ""two"",
            ""nullable_field"": null
        }
    }
]";
        var expectedResponseJson = @"[
    {
        ""id"": ""123"",
        ""jsonrpc"": ""2.0"",
        ""error"": {
            ""code"": -32600,
            ""message"": ""Invalid Request"",
            ""data"": {
                ""internal_http_code"": null,
                ""message"": ""Method is null or empty"",
                ""details"": null,
                ""type"": ""Tochka.JsonRpc.V1.Server.Exceptions.JsonRpcInternalException""
            }
        }
    },
    {
        ""id"": ""456"",
        ""jsonrpc"": ""2.0"",
        ""error"": {
            ""code"": -32600,
            ""message"": ""Invalid Request"",
            ""data"": {
                ""internal_http_code"": null,
                ""message"": ""Method is null or empty"",
                ""details"": null,
                ""type"": ""Tochka.JsonRpc.V1.Server.Exceptions.JsonRpcInternalException""
            }
        }
    }
]".TrimAllLines();

        using var request = new StringContent(requestJson, Encoding.UTF8, "application/json");
        var response = await ApiClient.PostAsync(JsonRpcUrl, request);
        var actualResponseJson = (await response.Content.ReadAsStringAsync()).TrimAllLines();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        actualResponseJson.Should().Be(expectedResponseJson);
    }

    [Test]
    [Ignore("Returns method not found - not like request")]
    public async Task Batch_MethodEmpty_ReturnError()
    {
        const string requestJson = @"[
    {
        ""jsonrpc"": ""2.0"",
        ""method"": """",
        ""id"": ""123"",
        ""params"": {
            ""bool_field"": true,
            ""string_field"": ""123"",
            ""int_field"": 123,
            ""double_field"": 1.23,
            ""enum_field"": ""two"",
            ""nullable_field"": null
        }
    },
    {
        ""jsonrpc"": ""2.0"",
        ""method"": """",
        ""id"": ""456"",
        ""params"": {
            ""bool_field"": true,
            ""string_field"": ""123"",
            ""int_field"": 123,
            ""double_field"": 1.23,
            ""enum_field"": ""two"",
            ""nullable_field"": null
        }
    }
]";
        var expectedResponseJson = @"[
    {
        ""id"": ""123"",
        ""jsonrpc"": ""2.0"",
        ""error"": {
            ""code"": -32600,
            ""message"": ""Invalid Request"",
            ""data"": {
                ""internal_http_code"": null,
                ""message"": ""Method is null or empty"",
                ""details"": null,
                ""type"": ""Tochka.JsonRpc.V1.Server.Exceptions.JsonRpcInternalException""
            }
        }
    },
    {
        ""id"": ""456"",
        ""jsonrpc"": ""2.0"",
        ""error"": {
            ""code"": -32600,
            ""message"": ""Invalid Request"",
            ""data"": {
                ""internal_http_code"": null,
                ""message"": ""Method is null or empty"",
                ""details"": null,
                ""type"": ""Tochka.JsonRpc.V1.Server.Exceptions.JsonRpcInternalException""
            }
        }
    }
]".TrimAllLines();

        using var request = new StringContent(requestJson, Encoding.UTF8, "application/json");
        var response = await ApiClient.PostAsync(JsonRpcUrl, request);
        var actualResponseJson = (await response.Content.ReadAsStringAsync()).TrimAllLines();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        actualResponseJson.Should().Be(expectedResponseJson);
    }

    [Test]
    public async Task Batch_MethodWhiteSpace_ReturnError() // Should return "Method is null or empty"?
    {
        const string requestJson = @"[
    {
        ""jsonrpc"": ""2.0"",
        ""method"": "" "",
        ""id"": ""123"",
        ""params"": {
            ""bool_field"": true,
            ""string_field"": ""123"",
            ""int_field"": 123,
            ""double_field"": 1.23,
            ""enum_field"": ""two"",
            ""nullable_field"": null
        }
    },
    {
        ""jsonrpc"": ""2.0"",
        ""method"": "" "",
        ""id"": ""456"",
        ""params"": {
            ""bool_field"": true,
            ""string_field"": ""123"",
            ""int_field"": 123,
            ""double_field"": 1.23,
            ""enum_field"": ""two"",
            ""nullable_field"": null
        }
    }
]";
        var expectedResponseJson = @"[
    {
        ""id"": ""123"",
        ""jsonrpc"": ""2.0"",
        ""error"": {
            ""code"": -32601,
            ""message"": ""Method not found"",
            ""data"": null
        }
    },
    {
        ""id"": ""456"",
        ""jsonrpc"": ""2.0"",
        ""error"": {
            ""code"": -32601,
            ""message"": ""Method not found"",
            ""data"": null
        }
    }
]".TrimAllLines();

        using var request = new StringContent(requestJson, Encoding.UTF8, "application/json");
        var response = await ApiClient.PostAsync(JsonRpcUrl, request);
        var actualResponseJson = (await response.Content.ReadAsStringAsync()).TrimAllLines();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        actualResponseJson.Should().Be(expectedResponseJson);
    }

    [Test]
    public async Task Batch_NoRequiredParams_ReturnError()
    {
        const string requestJson = @"[
    {
        ""jsonrpc"": ""2.0"",
        ""method"": ""process_anything"",
        ""id"": ""123""
    },
    {
        ""jsonrpc"": ""2.0"",
        ""method"": ""process_anything"",
        ""id"": ""456""
    }
]";
        var expectedResponseJson = @"[
    {
        ""id"": ""123"",
        ""jsonrpc"": ""2.0"",
        ""error"": {
            ""code"": -32602,
            ""message"": ""Invalid params"",
            ""data"": {
                """": [
                    ""The data field is required.""
                ]
            }
        }
    },
    {
        ""id"": ""456"",
        ""jsonrpc"": ""2.0"",
        ""error"": {
            ""code"": -32602,
            ""message"": ""Invalid params"",
            ""data"": {
                """": [
                    ""The data field is required.""
                ]
            }
        }
    }
]".TrimAllLines();

        using var request = new StringContent(requestJson, Encoding.UTF8, "application/json");
        var response = await ApiClient.PostAsync(JsonRpcUrl, request);
        var actualResponseJson = (await response.Content.ReadAsStringAsync()).TrimAllLines();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        actualResponseJson.Should().Be(expectedResponseJson);
    }

    [Test]
    public async Task Batch_EmptyRequiredParams_ReturnError()
    {
        const string requestJson = @"[
    {
        ""jsonrpc"": ""2.0"",
        ""method"": ""process_anything"",
        ""id"": ""123"",
        ""params"": {}
    },
    {
        ""jsonrpc"": ""2.0"",
        ""method"": ""process_anything"",
        ""id"": ""456"",
        ""params"": {}
    }
]";
        var expectedResponseJson = @"[
    {
        ""id"": ""123"",
        ""jsonrpc"": ""2.0"",
        ""error"": {
            ""code"": -32602,
            ""message"": ""Invalid params"",
            ""data"": {
                ""StringField"": [
                    ""The StringField field is required.""
                ]
            }
        }
    },
    {
        ""id"": ""456"",
        ""jsonrpc"": ""2.0"",
        ""error"": {
            ""code"": -32602,
            ""message"": ""Invalid params"",
            ""data"": {
                ""StringField"": [
                    ""The StringField field is required.""
                ]
            }
        }
    }
]".TrimAllLines();

        using var request = new StringContent(requestJson, Encoding.UTF8, "application/json");
        var response = await ApiClient.PostAsync(JsonRpcUrl, request);
        var actualResponseJson = (await response.Content.ReadAsStringAsync()).TrimAllLines();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        actualResponseJson.Should().Be(expectedResponseJson);
    }

    [Test]
    public async Task Batch_RequiredParamsNull_ReturnError()
    {
        const string requestJson = @"[
    {
        ""jsonrpc"": ""2.0"",
        ""method"": ""process_anything"",
        ""id"": ""123"",
        ""params"": null
    },
    {
        ""jsonrpc"": ""2.0"",
        ""method"": ""process_anything"",
        ""id"": ""456"",
        ""params"": null
    }
]";
        var expectedResponseJson = @"[
    {
        ""id"": ""123"",
        ""jsonrpc"": ""2.0"",
        ""error"": {
            ""code"": -32602,
            ""message"": ""Invalid params"",
            ""data"": {
                """": [
                    ""The data field is required.""
                ]
            }
        }
    },
    {
        ""id"": ""456"",
        ""jsonrpc"": ""2.0"",
        ""error"": {
            ""code"": -32602,
            ""message"": ""Invalid params"",
            ""data"": {
                """": [
                    ""The data field is required.""
                ]
            }
        }
    }
]".TrimAllLines();

        using var request = new StringContent(requestJson, Encoding.UTF8, "application/json");
        var response = await ApiClient.PostAsync(JsonRpcUrl, request);
        var actualResponseJson = (await response.Content.ReadAsStringAsync()).TrimAllLines();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        actualResponseJson.Should().Be(expectedResponseJson);
    }

    [Test]
    [Ignore("Processes successfully - not like request")]
    public async Task Batch_InvalidJsonRpcVersion_ReturnError() // Should id be null?
    {
        const string requestJson = @"[
    {
        ""jsonrpc"": ""3.0"",
        ""method"": ""process_anything"",
        ""id"": ""123"",
        ""params"": {
            ""bool_field"": true,
            ""string_field"": ""123"",
            ""int_field"": 123,
            ""double_field"": 1.23,
            ""enum_field"": ""two"",
            ""nullable_field"": null
        }
    },
    {
        ""jsonrpc"": ""3.0"",
        ""method"": ""process_anything"",
        ""id"": ""456"",
        ""params"": {
            ""bool_field"": true,
            ""string_field"": ""123"",
            ""int_field"": 123,
            ""double_field"": 1.23,
            ""enum_field"": ""two"",
            ""nullable_field"": null
        }
    }
]";
        var expectedResponseJson = @"[
    {
        ""id"": null,
        ""jsonrpc"": ""2.0"",
        ""error"": {
            ""code"": -32600,
            ""message"": ""Invalid Request"",
            ""data"": {
                ""internal_http_code"": null,
                ""message"": ""Invalid JSON Rpc version [3.0]"",
                ""details"": null,
                ""type"": ""Tochka.JsonRpc.V1.Server.Exceptions.JsonRpcInternalException""
            }
        }
    },
    {
        ""id"": null,
        ""jsonrpc"": ""2.0"",
        ""error"": {
            ""code"": -32600,
            ""message"": ""Invalid Request"",
            ""data"": {
                ""internal_http_code"": null,
                ""message"": ""Invalid JSON Rpc version [3.0]"",
                ""details"": null,
                ""type"": ""Tochka.JsonRpc.V1.Server.Exceptions.JsonRpcInternalException""
            }
        }
    }
]".TrimAllLines();

        using var request = new StringContent(requestJson, Encoding.UTF8, "application/json");
        var response = await ApiClient.PostAsync(JsonRpcUrl, request);
        var actualResponseJson = (await response.Content.ReadAsStringAsync()).TrimAllLines();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        actualResponseJson.Should().Be(expectedResponseJson);
    }

    [Test]
    public async Task Batch_InvalidJson_ReturnError()
    {
        const string requestJson = @"[
    {
        123
        ""jsonrpc"": ""2.0"",
        ""method"": ""process_anything"",
        ""id"": ""123"",
        ""params"": {
            ""bool_field"": true,
            ""string_field"": ""123"",
            ""int_field"": 123,
            ""double_field"": 1.23,
            ""enum_field"": ""two"",
            ""nullable_field"": null
        }
    },
    {
        123
        ""jsonrpc"": ""2.0"",
        ""method"": ""process_anything"",
        ""id"": ""123"",
        ""params"": {
            ""bool_field"": true,
            ""string_field"": ""123"",
            ""int_field"": 123,
            ""double_field"": 1.23,
            ""enum_field"": ""two"",
            ""nullable_field"": null
        }
    }
]";
        var expectedResponseJson = @"{
    ""id"": null,
    ""jsonrpc"": ""2.0"",
    ""error"": {
        ""code"": -32700,
        ""message"": ""Parse error"",
        ""data"": {
            ""internal_http_code"": null,
            ""message"": ""Invalid character after parsing property name. Expected ':' but got: \"". Path '[0]', line 4, position 8."",
            ""details"": null,
            ""type"": ""Newtonsoft.Json.JsonReaderException""
        }
    }
}".TrimAllLines();

        using var request = new StringContent(requestJson, Encoding.UTF8, "application/json");
        var response = await ApiClient.PostAsync(JsonRpcUrl, request);
        var actualResponseJson = (await response.Content.ReadAsStringAsync()).TrimAllLines();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        actualResponseJson.Should().Be(expectedResponseJson);
    }

    [Test]
    [Ignore("Processes successfully, but should?")]
    public async Task Batch_NoJsonRpc_ReturnError()
    {
        const string requestJson = @"[
    {
        ""method"": ""process_anything"",
        ""id"": ""123"",
        ""params"": {
            ""bool_field"": true,
            ""string_field"": ""123"",
            ""int_field"": 123,
            ""double_field"": 1.23,
            ""enum_field"": ""two"",
            ""nullable_field"": null
        }
    },
    {
        ""method"": ""process_anything"",
        ""id"": ""456"",
        ""params"": {
            ""bool_field"": true,
            ""string_field"": ""123"",
            ""int_field"": 123,
            ""double_field"": 1.23,
            ""enum_field"": ""two"",
            ""nullable_field"": null
        }
    }
]";
        var expectedResponseJson = @"[
    {
        ""id"": ""123"",
        ""jsonrpc"": ""2.0"",
        ""error"": {
            ""code"": TODO,
            ""message"": ""TODO"",
            ""data"": {TODO}
        }
    },
    {
        ""id"": ""456"",
        ""jsonrpc"": ""2.0"",
        ""error"": {
            ""code"": TODO,
            ""message"": ""TODO"",
            ""data"": {TODO}
        }
    }
]".TrimAllLines();

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
        const string requestJson = @"[
    {
        ""method"": ""process_anything"",
        ""id"": ""123"",
        ""params"": {
            ""bool_field"": true,
            ""string_field"": ""123"",
            ""int_field"": 123,
            ""double_field"": 1.23,
            ""enum_field"": ""two"",
            ""nullable_field"": null
        }
    },
    {
        ""method"": ""process_anything"",
        ""id"": ""123"",
        ""params"": {
            ""bool_field"": true,
            ""string_field"": ""123"",
            ""int_field"": 123,
            ""double_field"": 1.23,
            ""enum_field"": ""two"",
            ""nullable_field"": null
        }
    }
]";
        var expectedResponseJson = @"[
    {
        ""id"": ""123"",
        ""jsonrpc"": ""2.0"",
        ""error"": {
            ""code"": TODO,
            ""message"": ""TODO"",
            ""data"": {TODO}
        }
    },
    {
        ""id"": ""456"",
        ""jsonrpc"": ""2.0"",
        ""error"": {
            ""code"": TODO,
            ""message"": ""TODO"",
            ""data"": {TODO}
        }
    }
]".TrimAllLines();

        using var request = new StringContent(requestJson, Encoding.UTF8, "application/json");
        var response = await ApiClient.PostAsync(JsonRpcUrl, request);
        var actualResponseJson = (await response.Content.ReadAsStringAsync()).TrimAllLines();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        actualResponseJson.Should().Be(expectedResponseJson);
    }

    private const string JsonRpcUrl = "/api/jsonrpc";
}
