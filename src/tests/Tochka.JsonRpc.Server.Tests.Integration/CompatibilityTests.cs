using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;
using Tochka.JsonRpc.Common;
using Tochka.JsonRpc.Tests.WebApplication.Auth;
using Tochka.JsonRpc.TestUtils;
using Tochka.JsonRpc.TestUtils.Integration;

namespace Tochka.JsonRpc.Server.Tests.Integration;

[TestFixture]
internal class CompatibilityTests : IntegrationTestsBase<Program>
{
    [Test]
    public async Task Authorization_NotAuthorizedButRequired_Return401()
    {
        var requestContent = """
            {
                "id": 123,
                "method": "with_auth",
                "jsonrpc": "2.0"
            }
            """;

        using var request = new StringContent(requestContent, Encoding.UTF8, "application/json");
        var response = await ApiClient.PostAsync(JsonRpcConstants.DefaultRoutePrefix, request);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Test]
    public async Task Authorization_NotAuthorizedAndNotRequired_ReturnJsonRpcResponse()
    {
        var requestContent = """
            {
                "id": 123,
                "method": "without_auth",
                "jsonrpc": "2.0"
            }
            """;

        using var request = new StringContent(requestContent, Encoding.UTF8, "application/json");
        var response = await ApiClient.PostAsync(JsonRpcConstants.DefaultRoutePrefix, request);

        var expectedResponse = """
            {
                "id": 123,
                "result": true,
                "jsonrpc": "2.0"
            }
            """.TrimAllLines();
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var responseContent = await response.Content.ReadAsStringAsync();
        responseContent.TrimAllLines().Should().Be(expectedResponse);
    }

    [Test]
    public async Task Authorization_AuthorizedAndRequired_ReturnJsonRpcResponse()
    {
        var requestContent = """
            {
                "id": 123,
                "method": "without_auth",
                "jsonrpc": "2.0"
            }
            """;

        using var request = new StringContent(requestContent, Encoding.UTF8, "application/json");
        request.Headers.Add(AuthConstants.Header, AuthConstants.Key);
        var response = await ApiClient.PostAsync(JsonRpcConstants.DefaultRoutePrefix, request);

        var expectedResponse = """
            {
                "id": 123,
                "result": true,
                "jsonrpc": "2.0"
            }
            """.TrimAllLines();
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var responseContent = await response.Content.ReadAsStringAsync();
        responseContent.TrimAllLines().Should().Be(expectedResponse);
    }

    [Test]
    public async Task Versioning_DefaultVersion_ProcessSuccessfully()
    {
        var requestContent = """
            {
                "id": 123,
                "method": "process_versioned",
                "jsonrpc": "2.0"
            }
            """;

        using var request = new StringContent(requestContent, Encoding.UTF8, "application/json");
        request.Headers.Add(AuthConstants.Header, AuthConstants.Key);
        var response = await ApiClient.PostAsync(JsonRpcConstants.DefaultRoutePrefix, request);

        var expectedResponse = """
            {
                "id": 123,
                "result": true,
                "jsonrpc": "2.0"
            }
            """.TrimAllLines();
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var responseContent = await response.Content.ReadAsStringAsync();
        responseContent.TrimAllLines().Should().Be(expectedResponse);
    }

    [Test]
    public async Task Versioning_SpecifiedVersion_ProcessSuccessfully()
    {
        var requestContent = """
            {
                "id": 123,
                "method": "process_versioned",
                "jsonrpc": "2.0"
            }
            """;

        using var request = new StringContent(requestContent, Encoding.UTF8, "application/json");
        request.Headers.Add(AuthConstants.Header, AuthConstants.Key);
        var response = await ApiClient.PostAsync($"{JsonRpcConstants.DefaultRoutePrefix}/v2", request);

        var expectedResponse = """
            {
                "id": 123,
                "result": false,
                "jsonrpc": "2.0"
            }
            """.TrimAllLines();
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var responseContent = await response.Content.ReadAsStringAsync();
        responseContent.TrimAllLines().Should().Be(expectedResponse);
    }
}
