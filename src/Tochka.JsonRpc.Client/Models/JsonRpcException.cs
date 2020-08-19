using System;
using System.Diagnostics.CodeAnalysis;

namespace Tochka.JsonRpc.Client.Models
{
    [ExcludeFromCodeCoverage]
    public class JsonRpcException : Exception
    {
        public IJsonRpcCallContext Context { get; }

        // for easy mocking
        public JsonRpcException()
        {
        }

        public JsonRpcException(string message, IJsonRpcCallContext context) : base(message)
        {
            Context = context;
        }

        public override string Message => $"{base.Message}{Environment.NewLine}{Context}";
    }
}