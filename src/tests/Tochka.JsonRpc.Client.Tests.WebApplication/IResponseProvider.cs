using System.Text.Json;

namespace Tochka.JsonRpc.Client.Tests.WebApplication;

public interface IResponseProvider
{
    JsonDocument GetResponse();
}
