using System.Text.Json;

namespace Tochka.JsonRpc.Client.Tests.WebApplication;

public interface IRequestValidator
{
    void Validate(HttpRequest request);
}
