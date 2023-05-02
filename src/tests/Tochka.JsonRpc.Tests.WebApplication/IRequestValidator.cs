using System.Text.Json;
using Tochka.JsonRpc.TestUtils;
using Tochka.JsonRpc.TestUtils.Integration;

namespace Tochka.JsonRpc.Tests.WebApplication;

public interface IRequestValidator
{
    void Validate(HttpRequest request);
    void Validate(TestData data);
    void Validate(object? data);
}
