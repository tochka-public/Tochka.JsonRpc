using System.Linq;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;
using Tochka.JsonRpc.TestUtils.Integration;

namespace Tochka.JsonRpc.OpenRpc.Tests.Integration;

[TestFixture]
internal class IntegrationTests : IntegrationTestsBase<Program>
{
    [TestCase("custom_controller_route")]
    [TestCase("auto_doc_experiments")]
    [TestCase("process_anything")]
    [TestCase("action_only")]
    [TestCase("simple_json_rpc.controller_and_action")]
    [TestCase("binding_style_default")]
    [TestCase("binding_style_object")]
    [TestCase("binding_style_array")]
    [TestCase("nullable_default_params")]
    [TestCase("nullable_object_params")]
    [TestCase("nullable_array_params")]
    [TestCase("default_params")]
    [TestCase("default_object_params")]
    [TestCase("default_array_params")]
    [TestCase("no_params")]
    [TestCase("snake_case_params")]
    [TestCase("camelCaseParams")]
    [TestCase("KEBAB-CASE-UPPER-CASE-PARAMS")]
    [TestCase("custom_action_route")]
    [TestCase("throw_exception")]
    [TestCase("return_error_from_factory")]
    [TestCase("return_mvc_error")]
    [TestCase("error_throw_as_response_exception")]
    public async Task GetDocument_ReturnsAllMethods(string method)
    {
        var response = await ApiClient.GetAsync("/openrpc/jsonrpc_v1.json");

        var responseContent = await response.Content.ReadAsStringAsync();
        var responseJson = JsonDocument.Parse(responseContent);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var methods = responseJson.RootElement.GetProperty("methods").EnumerateArray().ToArray();
        methods.Should().Contain(m => m.GetProperty("name").GetString() == method);
    }

    [Test]
    public async Task GetDocument_AttributeWithCustomGroup_ReturnAllCustomGroupMethods()
    {
        var response = await ApiClient.GetAsync("/openrpc/custom_v1.json");

        var responseContent = await response.Content.ReadAsStringAsync();
        var responseJson = JsonDocument.Parse(responseContent);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var methods = responseJson.RootElement.GetProperty("methods").EnumerateArray().ToArray();
        methods.Should().OnlyContain(static m => m.GetProperty("name").GetString() == "custom_group");
    }
}
