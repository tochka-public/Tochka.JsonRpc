using System.Text.Json;
using Json.Schema;

namespace Tochka.JsonRpc.OpenRpc;

public interface IOpenRpcSchemaGenerator
{
    Dictionary<string, JsonSchema> GetAllSchemas();
    JsonSchema CreateOrRef(Type type, string methodName, JsonSerializerOptions jsonSerializerOptions);
}
