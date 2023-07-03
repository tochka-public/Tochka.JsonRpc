using System.Diagnostics.CodeAnalysis;

namespace Tochka.JsonRpc.Server.Binding.ParseResults;

[ExcludeFromCodeCoverage]
public sealed record NullParseResult(string JsonKey) : IParseResult;
