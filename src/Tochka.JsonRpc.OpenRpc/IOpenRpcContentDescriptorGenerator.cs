using System.Reflection;
using Namotion.Reflection;
using Tochka.JsonRpc.OpenRpc.Models;
using Tochka.JsonRpc.Server.Metadata;

namespace Tochka.JsonRpc.OpenRpc;

public interface IOpenRpcContentDescriptorGenerator
{
    ContentDescriptor GenerateForType(ContextualType type, string methodName, Type? serializerType);
    ContentDescriptor GenerateForParameter(ContextualType type, string methodName, Type? serializerType, JsonRpcParameterMetadata parameterMetadata);
    ContentDescriptor GenerateForProperty(PropertyInfo propertyInfo, string methodName, Type? serializerType);
}
