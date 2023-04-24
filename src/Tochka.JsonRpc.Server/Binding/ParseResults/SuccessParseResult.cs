using System.Text.Json;

namespace Tochka.JsonRpc.Server.Binding.ParseResults;

internal record SuccessParseResult(JsonElement Value, string JsonKey) : IParseResult;
