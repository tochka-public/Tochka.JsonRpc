using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

namespace Tochka.JsonRpc.Client.Tests.WebApplication.Controllers;

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
    public async Task<object?> ProcessNotification()
    {
        requestValidator.Validate(Request);
        return Ok();
    }

    [HttpPost("/request")]
    public async Task<object?> ProcessRequest()
    {
        requestValidator.Validate(Request);
        return responseProvider.GetResponse();
    }

    [HttpPost("/batch")]
    public async Task<object?> ProcessBatch()
    {
        requestValidator.Validate(Request);
        return responseProvider.GetResponse();
    }
}
