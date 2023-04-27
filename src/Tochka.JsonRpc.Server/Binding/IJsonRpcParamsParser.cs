using System.Text.Json;
using Tochka.JsonRpc.Server.Binding.ParseResults;

namespace Tochka.JsonRpc.Server.Binding;

internal interface IJsonRpcParamsParser
{
    IParseResult Parse(JsonDocument rawCall, JsonDocument? parameters, JsonRpcParameterMetadata parameterMetadata);
}
