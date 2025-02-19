namespace Tochka.JsonRpc.Server.Binding.ParseResults;

/// <summary>
/// Result of parsing one of JSON-RPC parameters
/// </summary>
public interface IParseResult
{
    /// <summary>
    /// JSON key of parameter that was parsed
    /// </summary>
    string JsonKey { get; }
}
