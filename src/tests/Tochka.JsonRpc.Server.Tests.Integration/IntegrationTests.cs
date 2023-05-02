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

    private const string JsonRpcUrl = "/api/jsonrpc";
}
