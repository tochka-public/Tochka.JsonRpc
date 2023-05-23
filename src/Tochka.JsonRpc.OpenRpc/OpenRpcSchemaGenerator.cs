using System.Collections;
using System.Text.Json;
using Json.Schema;
using Json.Schema.Generation;
using Namotion.Reflection;
using Tochka.JsonRpc.Common;

namespace Tochka.JsonRpc.OpenRpc;

public class OpenRpcSchemaGenerator : IOpenRpcSchemaGenerator
{
    private readonly Dictionary<string, JsonSchema> registeredSchemas = new();
    private readonly HashSet<string> registeredSchemaKeys = new();

    public Dictionary<string, JsonSchema> GetAllSchemas() => new(registeredSchemas);

    public JsonSchema CreateOrRef(Type type, string methodName, JsonSerializerOptions jsonSerializerOptions)
    {
        var itemType = type.GetEnumerableItemType();
        if (typeof(IEnumerable).IsAssignableFrom(type) && itemType != null)
        {
            // returning schema itself if it's collection
            return new JsonSchemaBuilder()
                .Type(SchemaValueType.Array)
                .Items(CreateOrRef(itemType, methodName, jsonSerializerOptions))
                .Build();
        }

        var typeName = type.Name;
        if (!typeName.StartsWith($"{methodName} ", StringComparison.Ordinal))
        {
            // adding method name in case it uses not default serializer settings
            typeName = $"{methodName} {typeName}";
        }

        if (!registeredSchemas.ContainsKey(typeName) && !registeredSchemaKeys.Contains(typeName))
        {
            var schema = BuildSchema(type, typeName, methodName, jsonSerializerOptions);
            if (schema != null)
            {
                // returning schema itself if it's simple type
                return schema;
            }
        }

        // returning ref if it's enum or regular type with properties
        return new JsonSchemaBuilder()
            .Ref($"#/components/schemas/{typeName}")
            .Build();
    }

    private JsonSchema? BuildSchema(Type type, string typeName, string methodName, JsonSerializerOptions jsonSerializerOptions)
    {
        if (type.IsEnum)
        {
            // adding it just for the same logic as for normal types
            registeredSchemaKeys.Add(typeName);
            registeredSchemas[typeName] = new JsonSchemaBuilder()
                .Enum(type.GetEnumNames().Select(jsonSerializerOptions.ConvertName))
                .Build();
            return null;
        }

        // can't check type.GetProperties() here because simple types have properties too
        var schema = new JsonSchemaBuilder()
            .FromType(type)
            .Build();
        if (schema.GetProperties() == null)
        {
            // string, int, bool, etc...
            return schema;
        }

        // required to break infinite recursion
        registeredSchemaKeys.Add(typeName);
        registeredSchemas[typeName] = new JsonSchemaBuilder()
            .Type(SchemaValueType.Object)
            .Properties(BuildPropertiesSchemas(type, methodName, jsonSerializerOptions))
            .Build();
        return null;
    }

    private Dictionary<string, JsonSchema> BuildPropertiesSchemas(Type type, string methodName, JsonSerializerOptions jsonSerializerOptions) =>
        type
            .GetProperties()
            .ToDictionary(p => jsonSerializerOptions.ConvertName(p.Name),
                p => CreateOrRef(p.PropertyType, methodName, jsonSerializerOptions));
}
