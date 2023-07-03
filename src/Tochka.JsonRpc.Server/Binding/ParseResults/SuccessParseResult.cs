using System.Diagnostics.CodeAnalysis;
using System.Text.Json;

namespace Tochka.JsonRpc.Server.Binding.ParseResults;

[ExcludeFromCodeCoverage]
public sealed record SuccessParseResult(JsonElement Value, string JsonKey) : IParseResult;
