using System;
using System.Diagnostics.CodeAnalysis;
using Tochka.JsonRpc.V1.Server.Settings;

namespace Tochka.JsonRpc.V1.Server.Attributes
{
    /// <summary>
    /// Override matching rule for JSON Rpc "method"
    /// </summary>
    [ExcludeFromCodeCoverage]
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class JsonRpcMethodStyleAttribute : Attribute
    {
        public MethodStyle MethodStyle { get; }

        public JsonRpcMethodStyleAttribute(MethodStyle methodStyle)
        {
            MethodStyle = methodStyle;
        }
    }
}