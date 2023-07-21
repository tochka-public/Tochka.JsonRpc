using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using JetBrains.Annotations;
using Namotion.Reflection;
using Tochka.JsonRpc.Common;
using Tochka.JsonRpc.OpenRpc.Models;
using Tochka.JsonRpc.Server.Metadata;

namespace Tochka.JsonRpc.OpenRpc.Services;

/// <inheritdoc />
[PublicAPI]
public class OpenRpcContentDescriptorGenerator : IOpenRpcContentDescriptorGenerator
{
    private readonly IOpenRpcSchemaGenerator schemaGenerator;

    public OpenRpcContentDescriptorGenerator(IOpenRpcSchemaGenerator schemaGenerator) => this.schemaGenerator = schemaGenerator;

    public OpenRpcContentDescriptor GenerateForType(ContextualType type, string methodName, JsonSerializerOptions jsonSerializerOptions)
    {
        var name = jsonSerializerOptions.ConvertName(type.TypeName);
        var summary = type.GetXmlDocsSummary();
        var description = type.GetXmlDocsRemarks();
        const bool required = false; // this method only used for collection items, collection can be empty => it's always optional
        var deprecated = type.GetAttribute<ObsoleteAttribute>() != null;

        return Generate(type, methodName, jsonSerializerOptions, name, summary, description, required, deprecated);
    }

    public OpenRpcContentDescriptor GenerateForParameter(ContextualPropertyInfo propertyInfo, string methodName, JsonRpcParameterMetadata parameterMetadata, JsonSerializerOptions jsonSerializerOptions)
    {
        var type = propertyInfo.PropertyType;
        var name = parameterMetadata.PropertyName;
        var summary = propertyInfo.GetXmlDocsSummary();
        var description = propertyInfo.GetXmlDocsRemarks();
        var required = !parameterMetadata.IsOptional;
        var deprecated = propertyInfo.GetContextAttribute<ObsoleteAttribute>() != null;

        return Generate(type, methodName, jsonSerializerOptions, name, summary, description, required, deprecated);
    }

    public OpenRpcContentDescriptor GenerateForProperty(ContextualPropertyInfo propertyInfo, string methodName, JsonSerializerOptions jsonSerializerOptions)
    {
        var type = propertyInfo.PropertyType;
        var name = jsonSerializerOptions.ConvertName(propertyInfo.Name);
        var summary = propertyInfo.GetXmlDocsSummary();
        var description = propertyInfo.GetXmlDocsRemarks();
        var required = propertyInfo.GetContextAttribute<RequiredAttribute>() != null;
        var deprecated = propertyInfo.GetContextAttribute<ObsoleteAttribute>() != null;

        return Generate(type, methodName, jsonSerializerOptions, name, summary, description, required, deprecated);
    }

    private OpenRpcContentDescriptor Generate(ContextualType type, string methodName, JsonSerializerOptions jsonSerializerOptions, string name, string summary, string description, bool required, bool deprecated)
    {
        var schema = schemaGenerator.CreateOrRef(type, methodName, jsonSerializerOptions);
        return new OpenRpcContentDescriptor(name, schema)
        {
            Summary = summary,
            Description = description,
            Required = required,
            Deprecated = deprecated
        };
    }
}
