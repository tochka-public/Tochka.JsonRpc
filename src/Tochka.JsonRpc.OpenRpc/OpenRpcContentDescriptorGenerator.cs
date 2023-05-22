using System.Reflection;
using Microsoft.Extensions.Options;
using Namotion.Reflection;
using Tochka.JsonRpc.Common;
using Tochka.JsonRpc.OpenRpc.Models;
using Tochka.JsonRpc.Server.Metadata;
using Tochka.JsonRpc.Server.Serialization;
using Tochka.JsonRpc.Server.Settings;
using Utils = Tochka.JsonRpc.Server.Utils;

namespace Tochka.JsonRpc.OpenRpc;

public class OpenRpcContentDescriptorGenerator : IOpenRpcContentDescriptorGenerator
{
    private readonly IOpenRpcSchemaGenerator schemaGenerator;
    private readonly IEnumerable<IJsonSerializerOptionsProvider> serializerOptionsProviders;
    private readonly JsonRpcServerOptions serverOptions;

    public OpenRpcContentDescriptorGenerator(IOpenRpcSchemaGenerator schemaGenerator, IEnumerable<IJsonSerializerOptionsProvider> serializerOptionsProviders, IOptions<JsonRpcServerOptions> serverOptions)
    {
        this.schemaGenerator = schemaGenerator;
        this.serializerOptionsProviders = serializerOptionsProviders;
        this.serverOptions = serverOptions.Value;
    }

    public ContentDescriptor GenerateForType(ContextualType type, string methodName, Type? serializerType)
    {
        var serializerOptions = serializerType == null
            ? serverOptions.DefaultDataJsonSerializerOptions
            : Utils.GetJsonSerializerOptions(serializerOptionsProviders, serializerType);
        return new ContentDescriptor
        {
            Name = serializerOptions.PropertyNamingPolicy?.ConvertName(type.TypeName) ?? type.TypeName,
            Schema = schemaGenerator.CreateOrRef(type, methodName, serializerOptions)
        };
    }

    public ContentDescriptor GenerateForParameter(ContextualType type, string methodName, Type? serializerType, JsonRpcParameterMetadata parameterMetadata)
    {
        var serializerOptions = serializerType == null
            ? serverOptions.DefaultDataJsonSerializerOptions
            : Utils.GetJsonSerializerOptions(serializerOptionsProviders, serializerType);
        return new ContentDescriptor
        {
            Name = parameterMetadata.PropertyName,
            Schema = schemaGenerator.CreateOrRef(type, methodName, serializerOptions)
        };
    }

    public ContentDescriptor GenerateForProperty(PropertyInfo propertyInfo, string methodName, Type? serializerType)
    {
        var serializerOptions = serializerType == null
            ? serverOptions.DefaultDataJsonSerializerOptions
            : Utils.GetJsonSerializerOptions(serializerOptionsProviders, serializerType);
        return new ContentDescriptor
        {
            Name = serializerOptions.PropertyNamingPolicy?.ConvertName(propertyInfo.Name) ?? propertyInfo.Name,
            Schema = schemaGenerator.CreateOrRef(propertyInfo.PropertyType, methodName, serializerOptions)
        };
    }
}
