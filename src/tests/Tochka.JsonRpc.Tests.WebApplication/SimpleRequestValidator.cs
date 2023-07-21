using Tochka.JsonRpc.TestUtils;

namespace Tochka.JsonRpc.Tests.WebApplication;

internal class SimpleRequestValidator : IRequestValidator
{
    public void Validate(HttpRequest request)
    {
    }

    public void Validate(TestData data)
    {
    }

    public void Validate(object? data)
    {
    }
}
