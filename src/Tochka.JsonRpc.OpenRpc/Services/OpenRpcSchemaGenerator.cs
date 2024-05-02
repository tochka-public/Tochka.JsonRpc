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

    private string GetClearTypeName(string methodName, Type clearType)
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
            var enumSchema = new JsonSchemaBuilder()
                             .Enum(type.GetEnumNames().Select(jsonSerializerOptions.ConvertName))
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

        var propertiesSchemas = BuildPropertiesSchemas(type, typeName, methodName, jsonSerializerOptions);

        requiredPropsForSchemas.TryGetValue(typeName, out var requiredProperties);

        JsonSchema objectSchema;
        if (requiredProperties is not null)
        {
            objectSchema = new JsonSchemaBuilder()
                           .Type(SchemaValueType.Object)
                           .Properties(propertiesSchemas)
                           .Required(requiredProperties)
                           .Build();
        }
        else
        {
            objectSchema = new JsonSchemaBuilder()
                           .Type(SchemaValueType.Object)
                           .Properties(propertiesSchemas)
                           .Build();
        }

        RegisterSchema(typeName, objectSchema);
        return CreateRefSchema(typeName, propertySummary);
    }

    private static JsonSchema CreateRefSchema(string typeName, string? propertySummary)
    {
        var refSchemaBuilder = new JsonSchemaBuilder()
            .Ref($"#/components/schemas/{typeName}")
            .TryAppendTitle(propertySummary);

        return refSchemaBuilder.Build();
    }

    private void RegisterSchema(string key, JsonSchema schema)
    {
        registeredSchemaKeys.Add(key);
        registeredSchemas[key] = schema;
    }

    private Dictionary<string, JsonSchema> BuildPropertiesSchemas(Type type, string typeName, string methodName,
        JsonSerializerOptions jsonSerializerOptions)
    {
        var properties = type.GetProperties().ToList();
        properties.ForEach(p => CheckRequiredState(p, typeName, methodName, jsonSerializerOptions));

        return properties.ToDictionary(p => jsonSerializerOptions.ConvertName(p.Name),
                p => CreateOrRefInternal(p.PropertyType, methodName, p.GetXmlDocsSummary(), jsonSerializerOptions));
    }

    private void CheckRequiredState(PropertyInfo propertyInfo, string typeName, string methodName, JsonSerializerOptions jsonSerializerOptions)
    {
        if (propertyInfo.PropertyType.IsGenericType)
        {
            // need get names of properties which are possible can be null in a generic type
            var propertiesInGenericType = propertyInfo.PropertyType.GetProperties();

            var genericPropertiesContext = nullabilityInfoContext.Create(propertyInfo);
            var clearGenericTypeName = GetClearTypeName(methodName, propertyInfo.PropertyType);

            for (var i = 0; i < genericPropertiesContext.GenericTypeArguments.Length; i++)
            {
                var genericNullabilityState = genericPropertiesContext.GenericTypeArguments[i];
                var genericPropRequired = genericNullabilityState.ReadState is NullabilityState.NotNull;
                if (genericPropRequired)
                {
                    var prop = propertiesInGenericType[i];
                    TryAddRequiredMember(clearGenericTypeName, prop.Name, jsonSerializerOptions);
                }
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
        if (requiredPropsForSchemas.TryGetValue(typeName, out var requiredProperties))
        {
            if (!requiredProperties.Contains(clearPropertyName))
            {
                requiredProperties.Add(clearPropertyName);
            }
        }
        else
        {
            requiredPropsForSchemas.Add(typeName, new List<string> { clearPropertyName });
        }
    }
}

internal static class JsonSchemaBuilderExtensions
{
    public static JsonSchemaBuilder TryAppendTitle(this JsonSchemaBuilder builder, string? propertySummary)
    {
        if (propertySummary is { Length: > 0 })
        {
            builder.Title(propertySummary);
        }
        return builder;
    }
}
