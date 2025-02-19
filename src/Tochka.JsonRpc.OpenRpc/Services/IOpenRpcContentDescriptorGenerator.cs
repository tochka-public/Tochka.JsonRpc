using System.Text.Json;
using Namotion.Reflection;
using Tochka.JsonRpc.OpenRpc.Models;
using Tochka.JsonRpc.Server.Metadata;

namespace Tochka.JsonRpc.OpenRpc.Services;

/// <summary>
/// Service to generate OpenRPC content descriptors from C# types
/// </summary>
public interface IOpenRpcContentDescriptorGenerator
{
    /// <summary>
    /// Generate OpenRPC content descriptor from C# Type
    /// </summary>
    /// <param name="type">Type to get information from</param>
    /// <param name="methodName">Name of JSON-RPC method</param>
    /// <param name="jsonSerializerOptions">Data serializer options</param>
    OpenRpcContentDescriptor GenerateForType(ContextualType type, string methodName, JsonSerializerOptions jsonSerializerOptions);

    /// <summary>
    /// Generate OpenRPC content descriptor from C# property and additional information about JSON-RPC parameter
    /// </summary>
    /// <param name="propertyInfo">Property info to get information from</param>
    /// <param name="methodName">Name of JSON-RPC method</param>
    /// <param name="parameterMetadata">Additional information about JSON-RPC parameter</param>
    /// <param name="jsonSerializerOptions">Data serializer options</param>
    OpenRpcContentDescriptor GenerateForParameter(ContextualPropertyInfo propertyInfo, string methodName, JsonRpcParameterMetadata parameterMetadata, JsonSerializerOptions jsonSerializerOptions);

    /// <summary>
    /// Generate OpenRPC content descriptor from C# property
    /// </summary>
    /// <param name="propertyInfo">Property info to get information from</param>
    /// <param name="methodName">Name of JSON-RPC method</param>
    /// <param name="jsonSerializerOptions">Data serializer options</param>
    OpenRpcContentDescriptor GenerateForProperty(ContextualPropertyInfo propertyInfo, string methodName, JsonSerializerOptions jsonSerializerOptions);
}
