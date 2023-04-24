namespace Tochka.JsonRpc.Server.Binding.ParseResults;

internal record ErrorParseResult(string Message) : IParseResult;
