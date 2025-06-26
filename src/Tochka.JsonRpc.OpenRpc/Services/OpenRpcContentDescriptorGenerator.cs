﻿using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Text.Json;
using Namotion.Reflection;
using Tochka.JsonRpc.Common;
using Tochka.JsonRpc.OpenRpc.Models;
using Tochka.JsonRpc.Server.Metadata;

namespace Tochka.JsonRpc.OpenRpc.Services;

/// <inheritdoc />
public class OpenRpcContentDescriptorGenerator : IOpenRpcContentDescriptorGenerator
{
    private readonly IOpenRpcSchemaGenerator schemaGenerator;

    /// <summary></summary>
    public OpenRpcContentDescriptorGenerator(IOpenRpcSchemaGenerator schemaGenerator) => this.schemaGenerator = schemaGenerator;

    /// <inheritdoc />
    public OpenRpcContentDescriptor GenerateForType(ContextualType type, string methodName, JsonSerializerOptions jsonSerializerOptions)
    {
        var name = jsonSerializerOptions.ConvertName(type.Name);
        var summary = type.GetXmlDocsSummary();
        var description = type.GetXmlDocsRemarks();
        const bool required = false; // this method only used for collection items, collection can be empty => it's always optional
        var deprecated = type.GetAttribute<ObsoleteAttribute>(true) != null;

        return Generate(type, null, methodName, jsonSerializerOptions, name, summary, description, required, deprecated);
    }

    /// <inheritdoc />
    public OpenRpcContentDescriptor GenerateForParameter(ContextualPropertyInfo propertyInfo, string methodName, JsonRpcParameterMetadata parameterMetadata, JsonSerializerOptions jsonSerializerOptions)
    {
        var type = propertyInfo.PropertyType;
        var name = parameterMetadata.PropertyName;
        var summary = propertyInfo.GetXmlDocsSummary();
        var description = propertyInfo.GetXmlDocsRemarks();
        var required = !parameterMetadata.IsOptional;
        var deprecated = propertyInfo.GetAttribute<ObsoleteAttribute>(true) != null;

        return Generate(type, null, methodName, jsonSerializerOptions, name, summary, description, required, deprecated);
    }

    /// <inheritdoc />
    public OpenRpcContentDescriptor GenerateForProperty(ContextualPropertyInfo propertyInfo, string methodName, JsonSerializerOptions jsonSerializerOptions)
    {
        var type = propertyInfo.PropertyType;
        var name = OpenRpcSchemaGenerator.GetJsonPropertyName(propertyInfo.PropertyInfo, jsonSerializerOptions);
        var summary = propertyInfo.GetXmlDocsSummary();
        var description = propertyInfo.GetXmlDocsRemarks();
        var required = propertyInfo.GetAttribute<RequiredAttribute>(true) != null || OpenRpcSchemaGenerator.IsNotNullNullabilityContext(propertyInfo.PropertyInfo);
        var deprecated = propertyInfo.GetAttribute<ObsoleteAttribute>(true) != null;

        return Generate(type, propertyInfo.PropertyInfo, methodName, jsonSerializerOptions, name, summary, description, required, deprecated);
    }

    private OpenRpcContentDescriptor Generate(ContextualType type, PropertyInfo? property, string methodName, JsonSerializerOptions jsonSerializerOptions, string name, string summary, string description, bool required, bool deprecated)
    {
        var schema = schemaGenerator.CreateOrRef(type, property, methodName, jsonSerializerOptions);
        return new OpenRpcContentDescriptor(name, schema)
        {
            Summary = summary,
            Description = description,
            Required = required,
            Deprecated = deprecated
        };
    }
}
