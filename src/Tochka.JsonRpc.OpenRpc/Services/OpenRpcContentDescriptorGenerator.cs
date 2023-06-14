using System.Text.Json;
using Json.Schema.Generation;
using Namotion.Reflection;
using Tochka.JsonRpc.Common;
using Tochka.JsonRpc.OpenRpc.Models;
using Tochka.JsonRpc.Server.Metadata;

namespace Tochka.JsonRpc.OpenRpc.Services;

public class OpenRpcContentDescriptorGenerator : IOpenRpcContentDescriptorGenerator
{
    private readonly IOpenRpcSchemaGenerator schemaGenerator;

    public OpenRpcContentDescriptorGenerator(IOpenRpcSchemaGenerator schemaGenerator) => this.schemaGenerator = schemaGenerator;

    public OpenRpcContentDescriptor GenerateForType(ContextualType type, string methodName, JsonSerializerOptions jsonSerializerOptions)
    {
        var name = jsonSerializerOptions.ConvertName(type.TypeName);
        var isRequired = type.GetAttribute<RequiredAttribute>() != null;
        return Generate(type, name, isRequired, methodName, jsonSerializerOptions);
    }

    public OpenRpcContentDescriptor GenerateForParameter(ContextualType type, string methodName, JsonRpcParameterMetadata parameterMetadata, JsonSerializerOptions jsonSerializerOptions)
    {
        var name = parameterMetadata.PropertyName;
        var isRequired = !parameterMetadata.IsOptional;
        return Generate(type, name, isRequired, methodName, jsonSerializerOptions);
    }

    public OpenRpcContentDescriptor GenerateForProperty(ContextualPropertyInfo propertyInfo, string methodName, JsonSerializerOptions jsonSerializerOptions)
    {
        var name = jsonSerializerOptions.ConvertName(propertyInfo.Name);
        var isRequired = propertyInfo.PropertyType.GetAttribute<RequiredAttribute>() != null;
        return Generate(propertyInfo.PropertyType, name, isRequired, methodName, jsonSerializerOptions);
    }

    private OpenRpcContentDescriptor Generate(ContextualType type, string name, bool isRequired, string methodName, JsonSerializerOptions jsonSerializerOptions)
    {
        var schema = schemaGenerator.CreateOrRef(type, methodName, jsonSerializerOptions);
        return new OpenRpcContentDescriptor(name, schema)
        {
            Summary = type.GetXmlDocsSummary(),
            Description = type.GetXmlDocsRemarks(),
            Required = isRequired,
            Deprecated = type.GetAttribute<ObsoleteAttribute>() != null
        };
    }
}
