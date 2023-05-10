namespace Tochka.JsonRpc.Server.Binding.ParseResults;

public sealed record NullParseResult(string JsonKey) : IParseResult;
