using FluentValidation;
using JetBrains.Annotations;

namespace Tochka.JsonRpc.Tests.WebApplication.Controllers;

[UsedImplicitly]
public class ModelValidator : AbstractValidator<ValidationModel>
{
    public ModelValidator() => RuleFor(static m => m.Str).NotEmpty().WithMessage(Error);

    internal const string Error = "Str is empty";
}
