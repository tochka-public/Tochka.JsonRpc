using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;

namespace Tochka.JsonRpc.Client.Models;

/// <inheritdoc />
/// <summary>
/// Exception with IJsonRpcCallContext
/// </summary>
[PublicAPI]
[ExcludeFromCodeCoverage]
[SuppressMessage("Design", "CA1032:Implement standard exception constructors", Justification = "Context is required")]
public class JsonRpcException : Exception
{
    /// <summary>
    /// JSON-RPC call context
    /// </summary>
    public IJsonRpcCallContext Context { get; }

    /// <inheritdoc />
    public override string Message => $"{base.Message}{Environment.NewLine}{Context}";

    /// <inheritdoc />
    public JsonRpcException(string message, IJsonRpcCallContext context) : base(message) => Context = context;

    // for easy mocking
    internal JsonRpcException() => Context = new JsonRpcCallContext();
}
