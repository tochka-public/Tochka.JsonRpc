using System.Text.Json;
using Tochka.JsonRpc.Server.Binding.ParseResults;
using Tochka.JsonRpc.Server.Metadata;

namespace Tochka.JsonRpc.Server.Binding;

/// <summary>
/// Service to parse JSON-RPC parameters
/// </summary>
public interface IJsonRpcParamsParser
{
    /// <summary>
    /// Parse parameter of JSON-RPC call by it's metadata
    /// </summary>
    /// <param name="rawCall">Raw JSON-RPC call</param>
    /// <param name="parameters">JSON-RPC `params` object/array</param>
    /// <param name="parameterMetadata">Metadata for parameter to parse</param>
    IParseResult Parse(JsonDocument rawCall, JsonDocument? parameters, JsonRpcParameterMetadata parameterMetadata);
}
