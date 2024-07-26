using FluentValidation;
using JetBrains.Annotations;

namespace Tochka.JsonRpc.Tests.WebApplication.Controllers;

[UsedImplicitly]
public class StringValidator : AbstractValidator<string>
{
    public StringValidator() => RuleFor(static x => x).MinimumLength(3).WithMessage(Error);

    internal const string Error = "Str is too short";
}