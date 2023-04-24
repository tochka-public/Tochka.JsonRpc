namespace Tochka.JsonRpc.Server.Binding.ParseResults;

internal record NoParseResult(string JsonKey) : IParseResult;
