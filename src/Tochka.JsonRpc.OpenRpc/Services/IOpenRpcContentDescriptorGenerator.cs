using System.Text.Json;
using Namotion.Reflection;
using Tochka.JsonRpc.OpenRpc.Models;
using Tochka.JsonRpc.Server.Metadata;

namespace Tochka.JsonRpc.OpenRpc.Services;

public interface IOpenRpcContentDescriptorGenerator
{
    OpenRpcContentDescriptor GenerateForType(ContextualType type, string methodName, JsonSerializerOptions jsonSerializerOptions);
    OpenRpcContentDescriptor GenerateForParameter(ContextualType type, string methodName, JsonRpcParameterMetadata parameterMetadata, JsonSerializerOptions jsonSerializerOptions);
    OpenRpcContentDescriptor GenerateForProperty(ContextualPropertyInfo propertyInfo, string methodName, JsonSerializerOptions jsonSerializerOptions);
}
