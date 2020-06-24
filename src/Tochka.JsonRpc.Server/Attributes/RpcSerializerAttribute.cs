using System;
using System.Diagnostics.CodeAnalysis;
using Tochka.JsonRpc.Common.Serializers;

namespace Tochka.JsonRpc.Server.Attributes
{
    /// <summary>
    /// Override serializer used for JSON Rpc params
    /// </summary>
    [ExcludeFromCodeCoverage]
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class RpcSerializerAttribute : Attribute
    {
        public Type SerializerType { get; }

        public RpcSerializerAttribute(Type serializerType)
        {
            if (!typeof(IRpcSerializer).IsAssignableFrom(serializerType))
            {
                throw new ArgumentException($"Expected implementation of {nameof(IRpcSerializer)}", nameof(serializerType));
            }

            SerializerType = serializerType;
        }
    }
}