using FluentValidation;

namespace Tochka.JsonRpc.Tests.WebApplication.Controllers;

public class StringValidator : AbstractValidator<string>
{
    public StringValidator() => RuleFor(static x => x).MinimumLength(3).WithMessage(Error);

    internal const string Error = "Str is too short";
}
