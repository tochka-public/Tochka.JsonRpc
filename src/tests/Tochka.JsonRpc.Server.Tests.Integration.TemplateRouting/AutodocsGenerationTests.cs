using System;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;
using Tochka.JsonRpc.TestUtils.Integration;

namespace Tochka.JsonRpc.Server.Tests.Integration.TemplateRouting;

[TestFixture]
internal sealed class AutodocsGenerationTests : IntegrationTestsBase<Program>
{
    [TestCase("jsonrpc_v1", "/api/v1/jsonrpc/CustomRoute/route#check")]
    [TestCase("jsonrpc_v1", "/api/v1/jsonrpc/OldVersion#check")]
    [TestCase("jsonrpc_v1", "/api/v1/jsonrpc/Unversioned#check")]
    [TestCase("jsonrpc_v1", "/api/v1/jsonrpc/Versioned#check")]
    [TestCase("jsonrpc_v2", "/api/v2/jsonrpc/NewVersion#check")]
    [TestCase("jsonrpc_v2", "/api/v2/jsonrpc/Versioned#check")]
    [TestCase("jsonrpc_v3-str", "/api/v3-str/jsonrpc/Versioned#check")]
    [TestCase("v1", "/api/Rest/check", "get")]
    public async Task GetSwaggerDocument_ReturnsAllMethodsForVersion(string document, string path, string httpMethod = "post")
    {
        var response = await ApiClient.GetAsync($"/swagger/{document}/swagger.json");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var responseContent = await response.Content.ReadAsStringAsync();
        var responseJson = JsonDocument.Parse(responseContent);
        responseJson.RootElement.GetProperty("paths").TryGetProperty(path, out var method);
        method.TryGetProperty(httpMethod, out _).Should().BeTrue();
    }

    [TestCase("jsonrpc_v1", "/api/v1/jsonrpc/CustomRoute/route", "check")]
    [TestCase("jsonrpc_v1", "/api/v1/jsonrpc/OldVersion", "check")]
    [TestCase("jsonrpc_v1", "/api/v1/jsonrpc/Unversioned", "check")]
    [TestCase("jsonrpc_v1", "/api/v1/jsonrpc/Versioned", "check")]
    [TestCase("jsonrpc_v2", "/api/v2/jsonrpc/NewVersion", "check")]
    [TestCase("jsonrpc_v2", "/api/v2/jsonrpc/Versioned", "check")]
    [TestCase("jsonrpc_v3-str", "/api/v3-str/jsonrpc/Versioned", "check")]
    public async Task GetOpenRpcDocument_ReturnsAllMethodsForVersion(string document, string serverPath, string methodName)
    {
        var response = await ApiClient.GetAsync($"/openrpc/{document}.json");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var responseContent = await response.Content.ReadAsStringAsync();
        var responseJson = JsonDocument.Parse(responseContent);
        var methods = responseJson.RootElement.GetProperty("methods").EnumerateArray().ToList();
        methods.Should()
            .Contain(m =>
                m.GetProperty("name").GetString() == methodName
                && m.GetProperty("servers")
                    .EnumerateArray()
                    .Any(s => s.GetProperty("url")
                        .GetString()
                        .EndsWith(serverPath, StringComparison.Ordinal)));
    }
}
