using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;
using Tochka.JsonRpc.Common;
using Tochka.JsonRpc.Tests.WebApplication;
using Tochka.JsonRpc.TestUtils;
using Tochka.JsonRpc.TestUtils.Integration;

namespace Tochka.JsonRpc.Server.Tests.Integration;

[TestFixture]
internal sealed class IntegrationTests : IntegrationTestsBase<Program>
{
    private Mock<IResponseProvider> responseProviderMock;
    private Mock<IRequestValidator> requestValidatorMock;
    private Mock<IBusinessLogicExceptionHandler> businessLogicExceptionHandlerMock;

    public override void Setup()
    {
        base.Setup();
        responseProviderMock = new Mock<IResponseProvider>();
        requestValidatorMock = new Mock<IRequestValidator>();
        businessLogicExceptionHandlerMock = new Mock<IBusinessLogicExceptionHandler>();
    }

    protected override void SetupServices(IServiceCollection services)
    {
        base.SetupServices(services);
        services.AddTransient(_ => responseProviderMock.Object);
        services.AddTransient(_ => requestValidatorMock.Object);
        services.AddTransient(_ => businessLogicExceptionHandlerMock.Object);
    }

    [Test]
    public async Task NotJson_Return404()
    {
        const string requestContent = "Hello World!";

        using var request = new StringContent(requestContent, Encoding.UTF8, "text/plain");
        var response = await ApiClient.PostAsync(JsonRpcConstants.DefaultRoutePrefix, request);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Test]
    public async Task CustomExceptionFilter_ExpectedException_HandleOnlyWithCustomFilters()
    {
        var requestContent = """
                             {
                                 "id": 123,
                                 "method": "business_logic_exception",
                                 "jsonrpc": "2.0"
                             }
                             """;
        businessLogicExceptionHandlerMock.Setup(static h => h.Handle(It.IsAny<BusinessLogicException>()))
            .Verifiable();

        using var request = new StringContent(requestContent, Encoding.UTF8, "application/json");
        var response = await ApiClient.PostAsync(JsonRpcConstants.DefaultRoutePrefix, request);

        var expectedResponse =
            $$"""
                  {
                      "id": 123,
                      "error": {
                          "code": -32603,
                          "message": "Internal error",
                          "data": "{{BusinessLogicExceptionWrappingFilter.ErrorData}}"
                      },
                      "jsonrpc": "2.0"
                  }
                  """.TrimAllLines();
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var responseContent = await response.Content.ReadAsStringAsync();
        responseContent.TrimAllLines().Should().Be(expectedResponse);
        businessLogicExceptionHandlerMock.Verify();
    }

    [Test]
    public async Task CustomExceptionFilter_UnexpectedException_HandleWithDefaultFilter()
    {
        var requestContent = """
                             {
                                 "id": 123,
                                 "method": "unexpected_exception",
                                 "jsonrpc": "2.0"
                             }
                             """;

        using var request = new StringContent(requestContent, Encoding.UTF8, "application/json");
        var response = await ApiClient.PostAsync(JsonRpcConstants.DefaultRoutePrefix, request);

        var expectedResponse = """
                               {
                                   "id": 123,
                                   "error": {
                                       "code": -32000,
                                       "message": "Server error",
                                       "data": {
                                           "type": "System.InvalidOperationException",
                                           "message": "Operation is not valid due to the current state of the object.",
                                           "details": null
                                       }
                                   },
                                   "jsonrpc": "2.0"
                               }
                               """.TrimAllLines();
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var responseContent = await response.Content.ReadAsStringAsync();
        responseContent.TrimAllLines().Should().Be(expectedResponse);
    }
}
