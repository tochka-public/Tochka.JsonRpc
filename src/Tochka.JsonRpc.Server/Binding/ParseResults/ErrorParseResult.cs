namespace Tochka.JsonRpc.Server.Binding.ParseResults;

public sealed record ErrorParseResult(string Message, string JsonKey) : IParseResult;
