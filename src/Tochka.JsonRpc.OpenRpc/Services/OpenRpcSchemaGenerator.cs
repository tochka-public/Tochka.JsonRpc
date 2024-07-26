using System.Collections;
using System.Reflection;
using System.Text.Json;
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
    private readonly Dictionary<string, List<string>> requiredPropsForSchemas = new();
    private readonly HashSet<string> registeredSchemaKeys = new();
    private readonly NullabilityInfoContext nullabilityInfoContext = new();

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

    private JsonSchema CreateOrRefInternal(Type type, string methodName, string? propertySummary, JsonSerializerOptions jsonSerializerOptions)
    {
        var clearType = TryUnwrapNullableType(type);
        var clearTypeName = GetClearTypeName(methodName, clearType);

        return BuildSchema(clearType, clearTypeName, methodName, propertySummary, jsonSerializerOptions);
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

    private JsonSchema BuildSchema(Type type, string typeName, string methodName, string? propertySummary, JsonSerializerOptions jsonSerializerOptions)
    {
        if (registeredSchemas.ContainsKey(typeName) || registeredSchemaKeys.Contains(typeName))
        {
            return CreateRefSchema(typeName, propertySummary);
        }

        var itemType = type.GetEnumerableItemType();
        if (typeof(IEnumerable).IsAssignableFrom(type) && itemType != null)
        {
            var collectionScheme = new JsonSchemaBuilder()
                   .Type(SchemaValueType.Array)
                   .Items(CreateOrRefInternal(itemType, methodName, null, jsonSerializerOptions))
                   .TryAppendTitle(propertySummary)
                   .BuildWithoutUri();
            // returning schema itself if it's collection
            return collectionScheme;
        }

        if (type.IsEnum)
        {
            var enumSchema = new JsonSchemaBuilder()
                             .Enum(type.GetEnumNames().Select(jsonSerializerOptions.ConvertName))
                             .BuildWithoutUri();
            RegisterSchema(typeName, enumSchema);
            // returning ref if it's enum or regular type with properties
            return CreateRefSchema(typeName, propertySummary);
        }

        var simpleTypeSchema = new JsonSchemaBuilder()
                               .FromType(type)
                               .TryAppendTitle(propertySummary)
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
                                     .TryAppendTitle(propertySummary)
                                     .BuildWithoutUri();
            return simpleStringSchema;
        }

        // required to break infinite recursion by ref to same type in property
        registeredSchemaKeys.Add(typeName);

        var propertiesSchemas = BuildPropertiesSchemas(type, typeName, methodName, jsonSerializerOptions);
        requiredPropsForSchemas.TryGetValue(typeName, out var requiredProperties);

        var jsonSchemaBuilder = new JsonSchemaBuilder()
                                .Type(SchemaValueType.Object)
                                .Properties(propertiesSchemas);
        if (requiredProperties is not null)
        {
            jsonSchemaBuilder.Required(requiredProperties);
        }

        var objectSchema = jsonSchemaBuilder.BuildWithoutUri();
        RegisterSchema(typeName, objectSchema);
        return CreateRefSchema(typeName, propertySummary);
    }

    private static JsonSchema CreateRefSchema(string typeName, string? propertySummary)
    {
        var refSchemaBuilder = new JsonSchemaBuilder()
            .Ref($"#/components/schemas/{typeName}")
            .TryAppendTitle(propertySummary);

        return refSchemaBuilder.BuildWithoutUri();
    }

    private void RegisterSchema(string key, JsonSchema schema)
    {
        registeredSchemaKeys.Add(key);
        registeredSchemas[key] = schema;
    }

    private Dictionary<string, JsonSchema> BuildPropertiesSchemas(Type type, string typeName, string methodName, JsonSerializerOptions jsonSerializerOptions) =>
        type
            .GetProperties()
            .ToDictionary(p => jsonSerializerOptions.ConvertName(p.Name),
                p =>
                {
                    TrySetRequiredState(p, typeName, methodName, jsonSerializerOptions);
                    return CreateOrRefInternal(p.PropertyType, methodName, p.GetXmlDocsSummary(), jsonSerializerOptions);
                });

    private void TrySetRequiredState(PropertyInfo propertyInfo, string typeName, string methodName, JsonSerializerOptions jsonSerializerOptions)
    {
        if (propertyInfo.PropertyType.IsGenericType)
        {
            var propertiesInGenericType = propertyInfo.PropertyType.GetProperties();

            var genericPropertiesContext = nullabilityInfoContext.Create(propertyInfo);
            var clearGenericTypeName = GetClearTypeName(methodName, propertyInfo.PropertyType);
            var propsNullabilityInfo = genericPropertiesContext.GenericTypeArguments.Zip(propertiesInGenericType,
                (nullabilityInfo, propInfo) => new { nullabilityInfo, propInfo });

            foreach (var requiredPropState in propsNullabilityInfo.Where(x => x.nullabilityInfo.ReadState is NullabilityState.NotNull))
            {
                TryAddRequiredMember(clearGenericTypeName, requiredPropState.propInfo.Name, jsonSerializerOptions);
            }
        }

        var propContext = nullabilityInfoContext.Create(propertyInfo);
        var required = propContext.ReadState is NullabilityState.NotNull;
        if (required)
        {
            TryAddRequiredMember(typeName, propertyInfo.Name, jsonSerializerOptions);
        }
    }

    private void TryAddRequiredMember(string typeName, string propertyName, JsonSerializerOptions jsonSerializerOptions)
    {
        var clearPropertyName = jsonSerializerOptions.ConvertName(propertyName);
        var requiredProperties = requiredPropsForSchemas.GetValueOrDefault(typeName) ?? new List<string>();
        if (!requiredProperties.Contains(clearPropertyName))
        {
            requiredProperties.Add(clearPropertyName);
        }

        requiredPropsForSchemas.TryAdd(typeName, requiredProperties);
    }
}