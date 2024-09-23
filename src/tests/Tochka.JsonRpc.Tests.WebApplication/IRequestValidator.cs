using Tochka.JsonRpc.TestUtils;

namespace Tochka.JsonRpc.Tests.WebApplication;

public interface IRequestValidator
{
    void Validate(HttpRequest request);
    void Validate(TestData data);
    void Validate(object? data = null);
}
