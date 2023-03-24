using System.Text.Json;
using Tochka.JsonRpc.TestUtils.Integration;

namespace Tochka.JsonRpc.Client.Tests.WebApplication;

internal class SimpleRequestValidator : IRequestValidator
{
    public void Validate(HttpRequest request, JsonDocument body)
    {
    }

    public void Validate(TestData data)
    {
    }

    public void Validate(object? data)
    {
    }
}
