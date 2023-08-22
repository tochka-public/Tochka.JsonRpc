using Microsoft.AspNetCore.Mvc;

namespace Tochka.JsonRpc.Tests.WebApplication.Controllers;

public class SimpleController : ControllerBase
{
    private readonly IResponseProvider responseProvider;
    private readonly IRequestValidator requestValidator;

    public SimpleController(IResponseProvider responseProvider, IRequestValidator requestValidator)
    {
        this.responseProvider = responseProvider;
        this.requestValidator = requestValidator;
    }

    [HttpPost("/notification")]
    public Task<object?> ProcessNotification()
    {
        requestValidator.Validate(Request);
        return Task.FromResult<object?>(Ok());
    }

    [HttpPost("/request")]
    public Task<object?> ProcessRequest()
    {
        requestValidator.Validate(Request);
        return Task.FromResult<object?>(responseProvider.GetResponse());
    }

    [HttpPost("/batch")]
    public Task<object?> ProcessBatch()
    {
        requestValidator.Validate(Request);
        return Task.FromResult<object?>(responseProvider.GetResponse());
    }
}
