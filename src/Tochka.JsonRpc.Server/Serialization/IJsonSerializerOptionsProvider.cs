using System.Text.Json;

namespace Tochka.JsonRpc.Server.Serialization;

public interface IJsonSerializerOptionsProvider
{
    JsonSerializerOptions Options { get; }
}
