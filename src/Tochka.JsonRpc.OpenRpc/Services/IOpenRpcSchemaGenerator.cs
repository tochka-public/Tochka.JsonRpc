using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Text.Json;
using Json.Schema;

namespace Tochka.JsonRpc.OpenRpc.Services;

/// <summary>
/// Service to generate JSON schemas for C# Types or reference already generated ones
/// </summary>
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
    /// <param name="property">Property whose type is provided</param>
    /// <param name="methodName">Name of JSON-RPC method</param>
    /// <param name="jsonSerializerOptions">Data serializer options</param>
    /// <returns>Schema itself if Type is collection or simple type without JSON properties, ref to schema otherwise</returns>
    [SuppressMessage("Naming", "CA1716:Identifiers should not match keywords", Justification = "Property is a property")]
    JsonSchema CreateOrRef(Type type, PropertyInfo? property, string methodName, JsonSerializerOptions jsonSerializerOptions);
}
