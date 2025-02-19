using FluentValidation;

namespace Tochka.JsonRpc.Tests.WebApplication.Controllers;

public class ModelValidator : AbstractValidator<ValidationModel>
{
    public ModelValidator() => RuleFor(static m => m.Str).NotEmpty().WithMessage(Error);

    internal const string Error = "Str is empty";
}
