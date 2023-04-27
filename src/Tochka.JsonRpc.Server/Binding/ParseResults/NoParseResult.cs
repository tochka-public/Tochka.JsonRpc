namespace Tochka.JsonRpc.Server.Binding.ParseResults;

public sealed record NoParseResult(string JsonKey) : IParseResult;
