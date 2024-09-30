using System.Collections;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using JetBrains.Annotations;
using Json.Schema;
using Json.Schema.Generation;
using Namotion.Reflection;
using Tochka.JsonRpc.Common;

namespace Tochka.JsonRpc.OpenRpc.Services;

/// <inheritdoc />
[PublicAPI]
public class OpenRpcSchemaGenerator : IOpenRpcSchemaGenerator
{
    private readonly Dictionary<string, JsonSchema> registeredSchemas = new();
    private readonly HashSet<string> registeredSchemaKeys = new();

    private readonly Dictionary<Type, Format> defaultStringConvertedSimpleTypes = new()
    {
        { typeof(DateTime), Formats.DateTime },
        { typeof(DateTimeOffset), Formats.DateTime },
        { typeof(DateOnly), Formats.Date },
        { typeof(TimeOnly), Formats.Time },
        { typeof(TimeSpan), Formats.Duration },
        { typeof(Guid), Formats.Uuid }
    };

    /// <inheritdoc />
    public Dictionary<string, JsonSchema> GetAllSchemas() => new(registeredSchemas);

    /// <inheritdoc />
    public JsonSchema CreateOrRef(Type type, string methodName, JsonSerializerOptions jsonSerializerOptions) =>
        CreateOrRefInternal(type, methodName, null, jsonSerializerOptions);
    
    private JsonSchema CreateOrRefInternal(Type type, string methodName, PropertyInfo? property, JsonSerializerOptions jsonSerializerOptions)
    {
        var clearType = Nullable.GetUnderlyingType(type) ?? type;
        
        var clearTypeName = clearType.Name;
        if (!clearTypeName.StartsWith($"{methodName} ", StringComparison.Ordinal))
        {
            // adding method name in case it uses not default serializer settings
            clearTypeName = $"{methodName} {clearTypeName}";
        }

        return BuildSchema(clearType, clearTypeName, methodName, property, jsonSerializerOptions);
    }

    private JsonSchema BuildSchema(Type type, string typeName, string methodName, PropertyInfo? property, JsonSerializerOptions jsonSerializerOptions)
    {
        var propertySummary = property?.GetXmlDocsSummary();
        
        if (registeredSchemas.ContainsKey(typeName) || registeredSchemaKeys.Contains(typeName))
        {
            return CreateRefSchema(typeName, propertySummary);
        }
        
        var itemType = type.GetEnumerableItemType();
        if (typeof(IEnumerable).IsAssignableFrom(type) && itemType != null)
        {
            var collectionScheme =  new JsonSchemaBuilder()
                   .Type(SchemaValueType.Array)
                   .Items(CreateOrRefInternal(itemType, methodName, null, jsonSerializerOptions))
                   .TryAppendTitle(propertySummary)
                   .Build();
            // returning schema itself if it's collection
            return collectionScheme;
        }
        
        if (type.IsEnum)
        {
            List<string> enumValues = new();
            
            var converterOptions = GetSerializerOptionsByConverterAttribute(property);
            if (converterOptions is not null)
            {
                foreach (var val in type.GetEnumValues())
                {
                    enumValues.Add(JsonSerializer.Serialize(val, converterOptions).Replace("\"", string.Empty));
                }
            }
            else
            {
                enumValues.AddRange(type.GetEnumNames().Select(jsonSerializerOptions.ConvertName));
            }
            
            var enumSchema = new JsonSchemaBuilder()
                             .Enum(enumValues)
                             .TryAppendTitle(type.GetXmlDocsSummary())
                             .Build();
            RegisterSchema(typeName, enumSchema);
            // returning ref if it's enum or regular type with properties
            return CreateRefSchema(typeName, propertySummary);
        }

        var simpleTypeSchema = new JsonSchemaBuilder()
                               .FromType(type)
                               .TryAppendTitle(propertySummary)
                               .Build();
        // can't check type.GetProperties() here because simple types have properties too
        if (simpleTypeSchema.GetProperties() == null)
        {
            // returning schema itself if it's simple type
            // string, int, bool, etc...
            return simpleTypeSchema;
        }
        
        if (defaultStringConvertedSimpleTypes.TryGetValue(type, out var format))
        {
            var simpleStringSchema = new JsonSchemaBuilder()
                                     .Type(SchemaValueType.String)
                                     .Format(format)
                                     .TryAppendTitle(propertySummary)
                                     .Build();
            return simpleStringSchema;
        }

        // required to break infinite recursion by ref to same type in property
        registeredSchemaKeys.Add(typeName);
        
        var objectSchema = new JsonSchemaBuilder()
              .Type(SchemaValueType.Object)
              .Properties(BuildPropertiesSchemas(type, methodName, jsonSerializerOptions))
              .TryAppendTitle(type.GetXmlDocsSummary())
              .Build();
        RegisterSchema(typeName, objectSchema);
        return CreateRefSchema(typeName, propertySummary);
    }

    private static JsonSchema CreateRefSchema(string typeName, string? summary)
    {
        var refSchemaBuilder = new JsonSchemaBuilder()
            .Ref($"#/components/schemas/{typeName}")
            .TryAppendTitle(summary);
        
        return refSchemaBuilder.Build();
    }
    
    private void RegisterSchema(string key, JsonSchema schema)
    {
        registeredSchemaKeys.Add(key);
        registeredSchemas[key] = schema;
    }

    private Dictionary<string, JsonSchema> BuildPropertiesSchemas(Type type, string methodName, JsonSerializerOptions jsonSerializerOptions)
    {
        Dictionary<string, JsonSchema> schemas = new();
        
        var properties = type.GetProperties();

        foreach (var property in properties)
        {
            if (property.GetCustomAttribute<JsonIgnoreAttribute>() is not null)
                continue;
            
            var jsonPropertyName = property.GetCustomAttribute<JsonPropertyNameAttribute>()?.Name
                                   ?? jsonSerializerOptions.ConvertName(property.Name);
            
            var schema = CreateOrRefInternal(property.PropertyType, methodName, property, jsonSerializerOptions);
            
            schemas.Add(jsonPropertyName, schema);
        }

        return schemas;
    }

    private static JsonSerializerOptions? GetSerializerOptionsByConverterAttribute(PropertyInfo? property)
    {
        var converterAttribute = property?.GetCustomAttribute<JsonConverterAttribute>();
        if (converterAttribute is { ConverterType: {} converterType })
        {
            if (Activator.CreateInstance(converterType) is JsonConverter converterInstance)
            {
                var options = new JsonSerializerOptions();
                options.Converters.Add(converterInstance);
                return options; 
            }
        }

        return null;
    }
}

internal static class JsonSchemaBuilderExtensions
{
    public static JsonSchemaBuilder TryAppendTitle(this JsonSchemaBuilder builder, string? summary)
    {
        if (summary is { Length: > 0 })
        {
            var newLineIndex = summary.IndexOf(Environment.NewLine, StringComparison.Ordinal);
            if (newLineIndex >= 0)
            {
                builder.Title(summary[..newLineIndex] + "...")
                       .Description(summary);
            }
            else
            {
                builder.Title(summary);
            }
        }
        return builder;
    }
}