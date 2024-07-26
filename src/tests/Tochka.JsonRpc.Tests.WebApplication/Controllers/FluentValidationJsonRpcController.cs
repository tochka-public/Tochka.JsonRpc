using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Tochka.JsonRpc.Server;
using Tochka.JsonRpc.Server.Attributes;
using Tochka.JsonRpc.Server.Services;
using Tochka.JsonRpc.Server.Settings;

namespace Tochka.JsonRpc.Tests.WebApplication.Controllers;

public class FluentValidationJsonRpcController : JsonRpcControllerBase
{
    private readonly IValidator<string> strValidator;
    private readonly IJsonRpcErrorFactory errorFactory;

    public FluentValidationJsonRpcController(IValidator<string> strValidator, IJsonRpcErrorFactory errorFactory)
    {
        this.strValidator = strValidator;
        this.errorFactory = errorFactory;
    }

    // model validated automatically because of builder.Services.AddFluentValidationAutoValidation() in Program.cs
    public async Task<IActionResult> Validate([FromParams(BindingStyle.Object)] ValidationModel model, CancellationToken token)
    {
        var validationResult = await strValidator.ValidateAsync(model.Str, token);
        return !validationResult.IsValid
            ? Ok(errorFactory.InternalError(validationResult.ToDictionary()))
            : Ok(model.Str);
    }
}