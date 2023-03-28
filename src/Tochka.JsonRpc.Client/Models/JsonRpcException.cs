using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;

namespace Tochka.JsonRpc.Client.Models
{
    [ExcludeFromCodeCoverage]
    [SuppressMessage("Design", "CA1032:Implement standard exception constructors", Justification = "Context is required")]
    [PublicAPI]
    public class JsonRpcException : Exception
    {
        public IJsonRpcCallContext Context { get; }

        // for easy mocking
        internal JsonRpcException() => Context = new JsonRpcCallContext();

        public JsonRpcException(string message, IJsonRpcCallContext context) : base(message) => Context = context;

        public override string Message => $"{base.Message}{Environment.NewLine}{Context}";
    }
}
