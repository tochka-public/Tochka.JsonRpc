namespace Tochka.JsonRpc.Server.Binding.ParseResults;

internal record ErrorParseResult(string Message, string JsonKey) : IParseResult;
