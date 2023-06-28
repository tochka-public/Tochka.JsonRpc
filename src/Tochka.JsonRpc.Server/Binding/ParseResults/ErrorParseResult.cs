using System.Diagnostics.CodeAnalysis;

namespace Tochka.JsonRpc.Server.Binding.ParseResults;

[ExcludeFromCodeCoverage]
public sealed record ErrorParseResult(string Message, string JsonKey) : IParseResult;
