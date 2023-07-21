using System.Text.Json;
using JetBrains.Annotations;
using Json.Schema;

namespace Tochka.JsonRpc.OpenRpc.Services;

/// <summary>
/// Service to generate JSON schemas for C# Types or reference already generated ones
/// </summary>
[PublicAPI]
public interface IOpenRpcSchemaGenerator
{
    /// <summary>
    /// Get all generated schemas with their type names
    /// </summary>
    Dictionary<string, JsonSchema> GetAllSchemas();

    /// <summary>
    /// Generate new schema or reference already generated one
    /// </summary>
    /// <param name="type">Type to generate schema from</param>
    /// <param name="methodName">Name of JSON-RPC method</param>
    /// <param name="jsonSerializerOptions">Data serializer options</param>
    /// <returns>Schema itself if Type is collection or simple type without JSON properties, ref to schema otherwise</returns>
    JsonSchema CreateOrRef(Type type, string methodName, JsonSerializerOptions jsonSerializerOptions);
}
