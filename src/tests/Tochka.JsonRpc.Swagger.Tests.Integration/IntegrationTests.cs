using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;
using Tochka.JsonRpc.Tests.WebApplication.Controllers;
using Tochka.JsonRpc.TestUtils.Integration;

namespace Tochka.JsonRpc.Swagger.Tests.Integration;

[TestFixture]
internal class IntegrationTests : IntegrationTestsBase<Program>
{
    [TestCase("jsonrpc_v1", "/api/jsonrpc/custom/controller#custom_controller_route")]
    [TestCase("rest", "/notification")]
    [TestCase("rest", "/request")]
    [TestCase("rest", "/batch")]
    [TestCase("jsonrpc_v1", "/api/jsonrpc#auto_doc_experiments")]
    [TestCase("jsonrpc_v1", "/api/jsonrpc#process_anything")]
    [TestCase("jsonrpc_v1", "/api/jsonrpc#action_only")]
    [TestCase("jsonrpc_v1", "/api/jsonrpc#simple_json_rpc.controller_and_action")]
    [TestCase("jsonrpc_v1", "/api/jsonrpc#binding_style_default")]
    [TestCase("jsonrpc_v1", "/api/jsonrpc#binding_style_object")]
    [TestCase("jsonrpc_v1", "/api/jsonrpc#binding_style_array")]
    [TestCase("jsonrpc_v1", "/api/jsonrpc#nullable_default_params")]
    [TestCase("jsonrpc_v1", "/api/jsonrpc#nullable_object_params")]
    [TestCase("jsonrpc_v1", "/api/jsonrpc#nullable_array_params")]
    [TestCase("jsonrpc_v1", "/api/jsonrpc#default_params")]
    [TestCase("jsonrpc_v1", "/api/jsonrpc#default_object_params")]
    [TestCase("jsonrpc_v1", "/api/jsonrpc#default_array_params")]
    [TestCase("jsonrpc_v1", "/api/jsonrpc#no_params")]
    [TestCase("jsonrpc_snakecase_v1", "/api/jsonrpc#snake_case_params")]
    [TestCase("jsonrpc_camelcase_v1", "/api/jsonrpc#camelCaseParams")]
    [TestCase("jsonrpc_kebabcaseupper_v1", "/api/jsonrpc#KEBAB-CASE-UPPER-CASE-PARAMS")]
    [TestCase("jsonrpc_v1", "/api/jsonrpc/custom/action#custom_action_route")]
    [TestCase("jsonrpc_v1", "/api/jsonrpc#throw_exception")]
    [TestCase("jsonrpc_v1", "/api/jsonrpc#return_error_from_factory")]
    [TestCase("jsonrpc_v1", "/api/jsonrpc#return_mvc_error")]
    [TestCase("jsonrpc_v1", "/api/jsonrpc#error_throw_as_response_exception")]
    [TestCase("jsonrpc_v1", "/api/jsonrpc#void_method")]
    [TestCase("jsonrpc_v1", "/api/jsonrpc#task_method")]
    [TestCase("jsonrpc_v1", "/api/jsonrpc#empty_ok")]
    [TestCase("custom_v1", "/api/jsonrpc#custom_group")]
    public async Task GetDocument_ReturnsAllMethods(string document, string path)
    {
        var response = await ApiClient.GetAsync($"/swagger/{document}/swagger.json");

        var responseContent = await response.Content.ReadAsStringAsync();
        var responseJson = JsonDocument.Parse(responseContent);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        responseJson.RootElement.GetProperty("paths").TryGetProperty(path, out var method);
        method.TryGetProperty("post", out _).Should().BeTrue();
    }

    [Test]
    public async Task GetUi_Returns200()
    {
        var response = await ApiClient.GetAsync("/swagger");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
    
    
    [Test]
    public async Task TimeSpan_ParsingAsString()
    {
        var response = await ApiClient.GetAsync("/swagger/custom_v1/swagger.json");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var responseContent = await response.Content.ReadAsStringAsync();
        var responseJson = JsonDocument.Parse(responseContent);

        responseJson.RootElement.GetProperty("components")
                    .GetProperty("schemas")
                    .GetProperty(nameof(TestObject))
                    .GetProperty("properties")
                    .GetProperty(nameof(TestObject.Ts).ToLower())
                    .TryGetProperty("type", out var typePropertyJson).Should().BeTrue();
        
        typePropertyJson.GetString().Should().Be("string");
    }
}
