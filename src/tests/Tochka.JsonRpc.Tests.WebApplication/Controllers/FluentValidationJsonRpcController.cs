using FluentValidation;
using JetBrains.Annotations;
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

public record ValidationModel(string? Str);

[UsedImplicitly]
public class ModelValidator : AbstractValidator<ValidationModel>
{
    public ModelValidator() => RuleFor(static m => m.Str).NotEmpty().WithMessage(Error);

    internal const string Error = "Str is empty";
}

[UsedImplicitly]
public class StringValidator : AbstractValidator<string>
{
    public StringValidator() => RuleFor(static x => x).MinimumLength(3).WithMessage(Error);

    internal const string Error = "Str is too short";
}
