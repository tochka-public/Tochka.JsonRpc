using Microsoft.AspNetCore.Mvc;
using Tochka.JsonRpc.TestUtils;
using Tochka.JsonRpc.V1.Server.Attributes;
using Tochka.JsonRpc.V1.Server.Pipeline;
using Tochka.JsonRpc.V1.Server.Settings;

namespace Tochka.JsonRpc.Client.Tests.WebApplication.Controllers;

[Route("/custom/controller")]
public class CustomRouteJsonRpcController : JsonRpcController
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
