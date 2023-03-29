using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;

namespace Tochka.JsonRpc.Client.Models;

[PublicAPI]
[ExcludeFromCodeCoverage]
[SuppressMessage("Design", "CA1032:Implement standard exception constructors", Justification = "Context is required")]
public class JsonRpcException : Exception
{
    public IJsonRpcCallContext Context { get; }

    public override string Message => $"{base.Message}{Environment.NewLine}{Context}";

    public JsonRpcException(string message, IJsonRpcCallContext context) : base(message) => Context = context;

    // for easy mocking
    internal JsonRpcException() => Context = new JsonRpcCallContext();
}
