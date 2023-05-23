using System.Reflection;
using System.Text.Json;
using Namotion.Reflection;
using Tochka.JsonRpc.OpenRpc.Models;
using Tochka.JsonRpc.Server.Metadata;

namespace Tochka.JsonRpc.OpenRpc;

public interface IOpenRpcContentDescriptorGenerator
{
    ContentDescriptor GenerateForType(ContextualType type, string methodName, JsonSerializerOptions jsonSerializerOptions);
    ContentDescriptor GenerateForParameter(ContextualType type, string methodName, JsonRpcParameterMetadata parameterMetadata, JsonSerializerOptions jsonSerializerOptions);
    ContentDescriptor GenerateForProperty(ContextualPropertyInfo propertyInfo, string methodName, JsonSerializerOptions jsonSerializerOptions);
}
