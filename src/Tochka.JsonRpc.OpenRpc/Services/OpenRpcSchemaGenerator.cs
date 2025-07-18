using System.Collections;
using System.Reflection;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using Json.Schema;
using Json.Schema.Generation;
using Namotion.Reflection;
using Tochka.JsonRpc.Common;
using Tochka.JsonRpc.OpenRpc.Models;

namespace Tochka.JsonRpc.OpenRpc.Services;

/// <inheritdoc />
public class OpenRpcSchemaGenerator : IOpenRpcSchemaGenerator
{
    private static readonly NullabilityInfoContext nullabilityInfoContext = new();
    
    private readonly Dictionary<string, JsonSchema> registeredSchemas = new();
    private readonly Dictionary<string, List<string>> requiredPropsForSchemas = new();
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

    internal static bool IsNotNullNullabilityContext(PropertyInfo property) => 
        nullabilityInfoContext.Create(property).ReadState is NullabilityState.NotNull;
    
    internal static string GetJsonPropertyName(PropertyInfo property, JsonSerializerOptions jsonSerializerOptions) =>
        property.GetCustomAttribute<JsonPropertyNameAttribute>()?.Name
        ?? jsonSerializerOptions.ConvertName(property.Name);

    /// <inheritdoc />
    public Dictionary<string, JsonSchema> GetAllSchemas() => new(registeredSchemas);

    /// <inheritdoc />
    public JsonSchema CreateOrRef(Type type, PropertyInfo? property, string methodName, JsonSerializerOptions jsonSerializerOptions)
    {
        var clearType = TryUnwrapNullableType(type);
        var clearTypeName = GetClearTypeName(methodName, clearType);

        return BuildSchema(clearType, clearTypeName, methodName, property, jsonSerializerOptions);
    }

    private JsonSchema BuildSchema(Type type, string typeName, string methodName, PropertyInfo? property, JsonSerializerOptions jsonSerializerOptions)
    {
        var propertyXmlDocs = new XmlDocValuesWrapper(property?.GetXmlDocsSummary(), property?.GetXmlDocsRemarks());

        if (registeredSchemas.ContainsKey(typeName) || registeredSchemaKeys.Contains(typeName))
        {
            return CreateRefSchema(typeName, propertyXmlDocs);
        }

        var itemType = type.GetEnumerableItemType();
        if (typeof(IEnumerable).IsAssignableFrom(type) && itemType != null)
        {
            var collectionScheme = new JsonSchemaBuilder()
                .Type(SchemaValueType.Array)
                .Items(CreateOrRef(itemType, property, methodName, jsonSerializerOptions))
                .AppendXmlDocs(propertyXmlDocs)
                .BuildWithoutUri();
            // returning schema itself if it's collection
            return collectionScheme;
        }

        if (type.IsEnum)
        {
            List<string> enumSerializedValues = [];
            List<JsonSchema> enumOneOfSerializedItems = [];

            var enumSerializerOptions = GetSerializerOptionsByConverterAttribute(property) ?? jsonSerializerOptions;

            var enumValues = type.GetEnumValues();
            var enumTypeMembers = type.GetMembers(BindingFlags.Static | BindingFlags.Public);

            for (var i = 0; i < enumValues.Length; i++)
            {
                var serializedValue = JsonSerializer.Serialize(enumValues.GetValue(i), enumSerializerOptions).Replace("\"", string.Empty);
                var summary = enumTypeMembers[i].GetXmlDocsSummary();

                var oneOfItemSchema = new JsonSchemaBuilder()
                    .Const(serializedValue)
                    .Description(summary)
                    .Build();

                enumSerializedValues.Add(serializedValue);
                enumOneOfSerializedItems.Add(oneOfItemSchema);
            }

            var enumSchema = new JsonSchemaBuilder()
                .Enum(enumSerializedValues)
                .OneOf(enumOneOfSerializedItems)
                .AppendXmlDocs(new XmlDocValuesWrapper(type.GetXmlDocsSummary(), type.GetXmlDocsRemarks()))
                .BuildWithoutUri();
            RegisterSchema(typeName, enumSchema);
            // returning ref if it's enum or regular type with properties
            return CreateRefSchema(typeName, propertyXmlDocs);
        }

        var simpleTypeSchema = new JsonSchemaBuilder()
            .FromType(type)
            .AppendXmlDocs(propertyXmlDocs)
            .BuildWithoutUri();
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
                .AppendXmlDocs(propertyXmlDocs)
                .BuildWithoutUri();
            return simpleStringSchema;
        }

        // required to break infinite recursion by ref to same type in property
        registeredSchemaKeys.Add(typeName);

        var propertiesSchemas = BuildPropertiesSchemas(type, typeName, methodName, jsonSerializerOptions);
        requiredPropsForSchemas.TryGetValue(typeName, out var requiredProperties);

        var jsonSchemaBuilder = new JsonSchemaBuilder()
            .Type(SchemaValueType.Object)
            .Properties(propertiesSchemas)
            .AppendXmlDocs(new XmlDocValuesWrapper(type.GetXmlDocsSummary(), type.GetXmlDocsRemarks()));

        if (requiredProperties is not null)
        {
            jsonSchemaBuilder.Required(requiredProperties);
        }

        var objectSchema = jsonSchemaBuilder.BuildWithoutUri();
        RegisterSchema(typeName, objectSchema);
        return CreateRefSchema(typeName, propertyXmlDocs);
    }

    private void RegisterSchema(string key, JsonSchema schema)
    {
        registeredSchemaKeys.Add(key);
        registeredSchemas[key] = schema;
    }

    private Dictionary<string, JsonSchema> BuildPropertiesSchemas(Type type, string typeName, string methodName, JsonSerializerOptions jsonSerializerOptions)
    {
        Dictionary<string, JsonSchema> schemas = new();
        var properties = type.GetProperties();

        foreach (var property in properties)
        {
            if (property.GetCustomAttribute<JsonIgnoreAttribute>() is not null)
            {
                continue;
            }

            var jsonPropertyName = GetJsonPropertyName(property, jsonSerializerOptions);

            TrySetRequiredState(property, jsonPropertyName, typeName, methodName, jsonSerializerOptions);
            var schema = CreateOrRef(property.PropertyType, property, methodName, jsonSerializerOptions);
            schemas.Add(jsonPropertyName, schema);
        }

        return schemas;
    }

    private void TrySetRequiredState(PropertyInfo property, string jsonPropertyName, string typeName, string methodName, JsonSerializerOptions jsonSerializerOptions)
    {
        if (property.PropertyType.IsGenericType)
        {
            var propertiesInGenericType = property.PropertyType.GetProperties();

            var genericPropertiesContext = nullabilityInfoContext.Create(property);
            var clearGenericTypeName = GetClearTypeName(methodName, property.PropertyType);
            var propsNullabilityInfo = genericPropertiesContext.GenericTypeArguments.Zip(propertiesInGenericType, (nullabilityInfo, propInfo) => new { nullabilityInfo, propInfo });

            foreach (var requiredPropState in propsNullabilityInfo.Where(x => x.nullabilityInfo.ReadState is NullabilityState.NotNull))
            {
                var innerJsonPropertyName = GetJsonPropertyName(requiredPropState.propInfo, jsonSerializerOptions);
                TryAddRequiredMember(clearGenericTypeName, innerJsonPropertyName);
            }
        }

        if (IsNotNullNullabilityContext(property))
        {
            TryAddRequiredMember(typeName, jsonPropertyName);
        }
    }

    private void TryAddRequiredMember(string typeName, string jsonPropertyName)
    {
        var requiredProperties = requiredPropsForSchemas.GetValueOrDefault(typeName) ?? [];
        if (!requiredProperties.Contains(jsonPropertyName))
        {
            requiredProperties.Add(jsonPropertyName);
        }

        requiredPropsForSchemas.TryAdd(typeName, requiredProperties);
    }

    private static JsonSerializerOptions? GetSerializerOptionsByConverterAttribute(PropertyInfo? property)
    {
        var converterAttribute = property?.GetCustomAttribute<JsonConverterAttribute>();
        if (converterAttribute is { ConverterType: { } converterType })
        {
            if (Activator.CreateInstance(converterType) is JsonConverter converterInstance)
            {
                var options = new JsonSerializerOptions();
                options.Converters.Add(converterInstance);
                // Default encoder converts Cyrillic characters to Unicode symbols
                options.Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping;
                return options;
            }
        }

        return null;
    }

    private static Type TryUnwrapNullableType(Type type) => Nullable.GetUnderlyingType(type) ?? type;

    private static string GetClearTypeName(string methodName, Type clearType)
    {
        var clearTypeName = clearType.Name;
        if (!clearTypeName.StartsWith($"{methodName} ", StringComparison.Ordinal))
        {
            clearTypeName = $"{methodName} {clearTypeName}" + GetGenericTypeArgumentNames(clearType);
        }

        return clearTypeName;
    }

    private static string? GetGenericTypeArgumentNames(Type type)
    {
        if (type.GenericTypeArguments.Length > 0)
        {
            List<string> typeArgumentNames = new(type.GenericTypeArguments.Length);

            foreach (var typeArgument in type.GenericTypeArguments)
            {
                var clearTypeArgument = TryUnwrapNullableType(typeArgument);
                var clearTypeArgumentName = clearTypeArgument.Name + GetGenericTypeArgumentNames(clearTypeArgument);
                typeArgumentNames.Add(clearTypeArgumentName);
            }

            return $"[{string.Join(',', typeArgumentNames)}]";
        }

        return null;
    }

    private static JsonSchema CreateRefSchema(string typeName, XmlDocValuesWrapper propertyXmlDocs)
    {
        var refSchemaBuilder = new JsonSchemaBuilder()
            .Ref($"#/components/schemas/{typeName}")
            .AppendXmlDocs(propertyXmlDocs);

        return refSchemaBuilder.BuildWithoutUri();
    }
}
