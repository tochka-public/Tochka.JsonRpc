using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;
using Tochka.JsonRpc.TestUtils;
using Tochka.JsonRpc.TestUtils.Integration;

namespace Tochka.JsonRpc.Server.Tests.Integration.TemplateRouting;

[TestFixture]
internal class TemplateRoutingTests : IntegrationTestsBase<Program>
{
    [Test]
    public async Task VersionedController_V1Route_RouteSuccessfully()
    {
        const string requestJson = """
                                   {
                                       "id": "123",
                                       "method": "check",
                                       "jsonrpc": "2.0"
                                   }
                                   """;

        using var request = new StringContent(requestJson, Encoding.UTF8, "application/json");
        var response = await ApiClient.PostAsync("/api/v1/jsonrpc/Versioned", request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var actualResponseJson = (await response.Content.ReadAsStringAsync()).TrimAllLines();
        var expectedJson = """
                           {
                               "id": "123",
                               "result": true,
                               "jsonrpc": "2.0"
                           }
                           """.TrimAllLines();
        actualResponseJson.Should().Be(expectedJson);
    }

    [Test]
    public async Task VersionedController_V2Route_RouteSuccessfully()
    {
        const string requestJson = """
                                   {
                                       "id": "123",
                                       "method": "check",
                                       "jsonrpc": "2.0"
                                   }
                                   """;

        using var request = new StringContent(requestJson, Encoding.UTF8, "application/json");
        var response = await ApiClient.PostAsync("/api/v2/jsonrpc/Versioned", request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var actualResponseJson = (await response.Content.ReadAsStringAsync()).TrimAllLines();
        var expectedJson = """
                           {
                               "id": "123",
                               "result": true,
                               "jsonrpc": "2.0"
                           }
                           """.TrimAllLines();
        actualResponseJson.Should().Be(expectedJson);
    }

    [Test]
    public async Task VersionedController_StrVersionRoute_RouteSuccessfully()
    {
        const string requestJson = """
                                   {
                                       "id": "123",
                                       "method": "check",
                                       "jsonrpc": "2.0"
                                   }
                                   """;

        using var request = new StringContent(requestJson, Encoding.UTF8, "application/json");
        var response = await ApiClient.PostAsync("/api/v3-str/jsonrpc/Versioned", request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var actualResponseJson = (await response.Content.ReadAsStringAsync()).TrimAllLines();
        var expectedJson = """
                           {
                               "id": "123",
                               "result": true,
                               "jsonrpc": "2.0"
                           }
                           """.TrimAllLines();
        actualResponseJson.Should().Be(expectedJson);
    }

    [Test]
    public async Task VersionedController_LowercaseControllerName_RouteSuccessfully()
    {
        const string requestJson = """
                                   {
                                       "id": "123",
                                       "method": "check",
                                       "jsonrpc": "2.0"
                                   }
                                   """;

        using var request = new StringContent(requestJson, Encoding.UTF8, "application/json");
        var response = await ApiClient.PostAsync("/api/v1/jsonrpc/versioned", request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var actualResponseJson = (await response.Content.ReadAsStringAsync()).TrimAllLines();
        var expectedJson = """
                           {
                               "id": "123",
                               "result": true,
                               "jsonrpc": "2.0"
                           }
                           """.TrimAllLines();
        actualResponseJson.Should().Be(expectedJson);
    }

    [Test]
    public async Task OldVersionController_V1Route_RouteSuccessfully()
    {
        const string requestJson = """
                                   {
                                       "id": "123",
                                       "method": "check",
                                       "jsonrpc": "2.0"
                                   }
                                   """;

        using var request = new StringContent(requestJson, Encoding.UTF8, "application/json");
        var response = await ApiClient.PostAsync("/api/v1/jsonrpc/OldVersion", request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var actualResponseJson = (await response.Content.ReadAsStringAsync()).TrimAllLines();
        var expectedJson = """
                           {
                               "id": "123",
                               "result": true,
                               "jsonrpc": "2.0"
                           }
                           """.TrimAllLines();
        actualResponseJson.Should().Be(expectedJson);
    }

    [Test]
    public async Task OldVersionController_V2Route_Return404()
    {
        const string requestJson = """
                                   {
                                       "id": "123",
                                       "method": "check",
                                       "jsonrpc": "2.0"
                                   }
                                   """;

        using var request = new StringContent(requestJson, Encoding.UTF8, "application/json");
        var response = await ApiClient.PostAsync("/api/v2/jsonrpc/OldVersion", request);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Test]
    public async Task NewVersionController_V2Route_RouteSuccessfully()
    {
        const string requestJson = """
                                   {
                                       "id": "123",
                                       "method": "check",
                                       "jsonrpc": "2.0"
                                   }
                                   """;

        using var request = new StringContent(requestJson, Encoding.UTF8, "application/json");
        var response = await ApiClient.PostAsync("/api/v2/jsonrpc/NewVersion", request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var actualResponseJson = (await response.Content.ReadAsStringAsync()).TrimAllLines();
        var expectedJson = """
                           {
                               "id": "123",
                               "result": true,
                               "jsonrpc": "2.0"
                           }
                           """.TrimAllLines();
        actualResponseJson.Should().Be(expectedJson);
    }

    [Test]
    public async Task NewVersionController_V1Route_Return404()
    {
        const string requestJson = """
                                   {
                                       "id": "123",
                                       "method": "check",
                                       "jsonrpc": "2.0"
                                   }
                                   """;

        using var request = new StringContent(requestJson, Encoding.UTF8, "application/json");
        var response = await ApiClient.PostAsync("/api/v1/jsonrpc/NewVersion", request);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Test]
    public async Task VersionedController_UnknownVersionRoute_Return404()
    {
        const string requestJson = """
                                   {
                                       "id": "123",
                                       "method": "check",
                                       "jsonrpc": "2.0"
                                   }
                                   """;

        using var request = new StringContent(requestJson, Encoding.UTF8, "application/json");
        var response = await ApiClient.PostAsync("/api/v4/jsonrpc/Versioned", request);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Test]
    public async Task UnversionedController_DefaultVersionRoute_RouteSuccessfully()
    {
        const string requestJson = """
                                   {
                                       "id": "123",
                                       "method": "check",
                                       "jsonrpc": "2.0"
                                   }
                                   """;

        using var request = new StringContent(requestJson, Encoding.UTF8, "application/json");
        var response = await ApiClient.PostAsync("/api/v1/jsonrpc/Unversioned", request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var actualResponseJson = (await response.Content.ReadAsStringAsync()).TrimAllLines();
        var expectedJson = """
                           {
                               "id": "123",
                               "result": true,
                               "jsonrpc": "2.0"
                           }
                           """.TrimAllLines();
        actualResponseJson.Should().Be(expectedJson);
    }

    [Test]
    public async Task UnversionedController_V2Route_Return404()
    {
        const string requestJson = """
                                   {
                                       "id": "123",
                                       "method": "check",
                                       "jsonrpc": "2.0"
                                   }
                                   """;

        using var request = new StringContent(requestJson, Encoding.UTF8, "application/json");
        var response = await ApiClient.PostAsync("/api/v2/jsonrpc/Unversioned", request);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Test]
    public async Task CustomRouteController_DefaultVersionRoute_RouteSuccessfully()
    {
        const string requestJson = """
                                   {
                                       "id": "123",
                                       "method": "check",
                                       "jsonrpc": "2.0"
                                   }
                                   """;

        using var request = new StringContent(requestJson, Encoding.UTF8, "application/json");
        var response = await ApiClient.PostAsync("/api/v1/jsonrpc/CustomRoute/route", request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var actualResponseJson = (await response.Content.ReadAsStringAsync()).TrimAllLines();
        var expectedJson = """
                           {
                               "id": "123",
                               "result": true,
                               "jsonrpc": "2.0"
                           }
                           """.TrimAllLines();
        actualResponseJson.Should().Be(expectedJson);
    }

    [Test]
    public async Task RestController_NoVersionRoute_RouteSuccessfully()
    {
        var response = await ApiClient.GetAsync("/api/Rest/check");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var actualResponseJson = await response.Content.ReadAsStringAsync();
        var expectedJson = "true";
        actualResponseJson.Should().Be(expectedJson);
    }
}
