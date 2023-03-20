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
    public async Task<object?> ProcessNotification([FromBody] JsonDocument body)
    {
        requestValidator.Validate(Request, body);
        return Ok();
    }

    [HttpPost("/request")]
    public async Task<object?> ProcessRequest([FromBody] JsonDocument body)
    {
        requestValidator.Validate(Request, body);
        return responseProvider.GetResponse();
    }

    [HttpPost("/batch")]
    public async Task<object?> ProcessBatch([FromBody] JsonDocument body)
    {
        requestValidator.Validate(Request, body);
        return responseProvider.GetResponse();
    }
}
