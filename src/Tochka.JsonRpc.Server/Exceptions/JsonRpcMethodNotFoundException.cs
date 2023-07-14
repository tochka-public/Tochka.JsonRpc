using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;

namespace Tochka.JsonRpc.Server.Exceptions;

/// <inheritdoc />
/// <summary>
/// Exception indicating that there were no JSON-RPC endpoint for given method
/// </summary>
[PublicAPI]
[ExcludeFromCodeCoverage]
[SuppressMessage("Design", "CA1032:Implement standard exception constructors", Justification = "Method is required")]
public class JsonRpcMethodNotFoundException : Exception
{
    /// <summary>
    /// JSON-RPC method name that wasn't match to any action
    /// </summary>
    public string Method { get; }

    /// <inheritdoc />
    /// <param name="method">JSON-RPC method name that wasn't match to any action</param>
    public JsonRpcMethodNotFoundException(string method) => Method = method;
}
