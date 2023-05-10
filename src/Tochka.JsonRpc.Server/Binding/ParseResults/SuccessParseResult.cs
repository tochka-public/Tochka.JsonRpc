using System.Text.Json;

namespace Tochka.JsonRpc.Server.Binding.ParseResults;

public sealed record SuccessParseResult(JsonElement Value, string JsonKey) : IParseResult;
