using System.Collections;
using System.Text.Json;
using Json.Schema;
using Json.Schema.Generation;
using Namotion.Reflection;

namespace Tochka.JsonRpc.OpenRpc;

public class OpenRpcSchemaGenerator : IOpenRpcSchemaGenerator
{
    private readonly Dictionary<string, JsonSchema> registeredSchemas = new();
    private readonly HashSet<string> registeredSchemaKeys = new();

    public Dictionary<string, JsonSchema> GetAllSchemas() => registeredSchemas;

    public JsonSchema CreateOrRef(Type type, string methodName, JsonSerializerOptions jsonSerializerOptions)
    {
        if (typeof(IEnumerable).IsAssignableFrom(type) && type.GetEnumerableItemType() != null)
        {
            return new JsonSchemaBuilder()
                .Type(SchemaValueType.Array)
                .Items(CreateOrRef(type.GetEnumerableItemType()!, methodName, jsonSerializerOptions))
                .Build();
        }

        var typeName = type.Name;
        if (!typeName.StartsWith($"{methodName} ", StringComparison.Ordinal))
        {
            typeName = $"{methodName} {typeName}";
        }

        if (!registeredSchemas.ContainsKey(typeName) && !registeredSchemaKeys.Contains(typeName))
        {
            var options = new SchemaGeneratorConfiguration
            {
                PropertyNamingMethod = p => jsonSerializerOptions.PropertyNamingPolicy?.ConvertName(p) ?? p
            };
            var schema = new JsonSchemaBuilder()
                .FromType(type, options)
                .Build();
            if (schema.GetProperties() == null)
            {
                return schema;
            }

            var properties = type.GetProperties();
            registeredSchemaKeys.Add(typeName);
            registeredSchemas[typeName] = new JsonSchemaBuilder()
                .Type(SchemaValueType.Object)
                .Properties(properties.ToDictionary(p => jsonSerializerOptions.PropertyNamingPolicy?.ConvertName(p.Name) ?? p.Name, p => CreateOrRef(p.PropertyType, methodName, jsonSerializerOptions)))
                .Build();
        }

        return new JsonSchemaBuilder()
            .Ref($"#/components/schemas/{typeName}")
            .Build();
    }
}
