using System;
using System.Diagnostics.CodeAnalysis;
using Tochka.JsonRpc.Server.Settings;

namespace Tochka.JsonRpc.Server.Attributes
{
    /// <summary>
    /// Override matching rule for JSON Rpc "method"
    /// </summary>
    [ExcludeFromCodeCoverage]
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class RpcMethodStyleAttribute : Attribute
    {
        public MethodStyle MethodStyle { get; }

        public RpcMethodStyleAttribute(MethodStyle methodStyle)
        {
            MethodStyle = methodStyle;
        }
    }
}