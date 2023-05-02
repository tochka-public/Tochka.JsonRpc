using System.Text.Json;
using Tochka.JsonRpc.Server.Binding.ParseResults;
using Tochka.JsonRpc.Server.Metadata;

namespace Tochka.JsonRpc.Server.Binding;

public interface IJsonRpcParamsParser
{
    IParseResult Parse(JsonDocument rawCall, JsonDocument? parameters, JsonRpcParameterMetadata parameterMetadata);
}
