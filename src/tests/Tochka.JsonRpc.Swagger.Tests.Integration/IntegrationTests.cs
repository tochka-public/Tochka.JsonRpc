using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;
using Tochka.JsonRpc.TestUtils.Integration;

namespace Tochka.JsonRpc.Swagger.Tests.Integration;

[TestFixture]
internal class IntegrationTests : IntegrationTestsBase<Program>
{
    [TestCase("jsonrpc", "/api/jsonrpc/custom/controller#custom_controller_route")]
    [TestCase("rest", "/notification")]
    [TestCase("rest", "/request")]
    [TestCase("rest", "/batch")]
    [TestCase("jsonrpc", "/api/jsonrpc#auto_doc_experiments")]
    [TestCase("jsonrpc", "/api/jsonrpc#process_anything")]
    [TestCase("jsonrpc", "/api/jsonrpc#action_only")]
    [TestCase("jsonrpc", "/api/jsonrpc#simple_json_rpc.controller_and_action")]
    [TestCase("jsonrpc", "/api/jsonrpc#binding_style_default")]
    [TestCase("jsonrpc", "/api/jsonrpc#binding_style_object")]
    [TestCase("jsonrpc", "/api/jsonrpc#binding_style_array")]
    [TestCase("jsonrpc", "/api/jsonrpc#nullable_default_params")]
    [TestCase("jsonrpc", "/api/jsonrpc#nullable_object_params")]
    [TestCase("jsonrpc", "/api/jsonrpc#nullable_array_params")]
    [TestCase("jsonrpc", "/api/jsonrpc#default_params")]
    [TestCase("jsonrpc", "/api/jsonrpc#default_object_params")]
    [TestCase("jsonrpc", "/api/jsonrpc#default_array_params")]
    [TestCase("jsonrpc", "/api/jsonrpc#no_params")]
    [TestCase("jsonrpc_snakecase", "/api/jsonrpc#snake_case_params")]
    [TestCase("jsonrpc_camelcase", "/api/jsonrpc#camelCaseParams")]
    [TestCase("jsonrpc_kebabcaseupper", "/api/jsonrpc#KEBAB-CASE-UPPER-CASE-PARAMS")]
    [TestCase("jsonrpc", "/api/jsonrpc/custom/action#custom_action_route")]
    [TestCase("jsonrpc", "/api/jsonrpc#throw_exception")]
    [TestCase("jsonrpc", "/api/jsonrpc#return_error_from_factory")]
    [TestCase("jsonrpc", "/api/jsonrpc#return_mvc_error")]
    [TestCase("jsonrpc", "/api/jsonrpc#error_throw_as_response_exception")]
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
}
