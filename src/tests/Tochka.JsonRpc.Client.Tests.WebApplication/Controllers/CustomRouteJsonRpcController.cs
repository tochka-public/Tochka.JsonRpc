using Microsoft.AspNetCore.Mvc;
using Tochka.JsonRpc.Server;
using Tochka.JsonRpc.Server.Attributes;
using Tochka.JsonRpc.Server.Settings;
using Tochka.JsonRpc.TestUtils;

namespace Tochka.JsonRpc.Client.Tests.WebApplication.Controllers;

[Route("/custom/controller")]
public class CustomRouteJsonRpcController : JsonRpcControllerBase
{
    private readonly IResponseProvider responseProvider;
    private readonly IRequestValidator requestValidator;

    public CustomRouteJsonRpcController(IResponseProvider responseProvider, IRequestValidator requestValidator)
    {
        this.responseProvider = responseProvider;
        this.requestValidator = requestValidator;
    }

    public TestData CustomControllerRoute([FromParams(BindingStyle.Object)] TestData data)
    {
        requestValidator.Validate(data);
        return responseProvider.GetJsonRpcResponse();
    }
}
