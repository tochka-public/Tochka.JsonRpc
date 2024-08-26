namespace Tochka.JsonRpc.Common;

/// <summary>
/// Reserved pre-defined JSON-RPC error codes
/// </summary>
public static class JsonRpcPreDefinedErrorCodes
{
    /// <summary>
    /// Invalid JSON was received by the server.
    /// An error occurred on the server while parsing the JSON text.
    /// </summary>
    public const int ParseError = -32700;

    /// <summary>
    /// The JSON sent is not a valid Request object
    /// </summary>
    public const int InvalidRequest = -32600;

    /// <summary>
    /// The method does not exist or is not available
    /// </summary>
    public const int MethodNotFound = -32601;

    /// <summary>
    /// Invalid method parameters
    /// </summary>
    public const int InvalidParams = -32602;

    /// <summary>
    /// Internal JSON-RPC error
    /// </summary>
    public const int InternalError = -32603;
}
