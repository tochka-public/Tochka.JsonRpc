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
    public class JsonRpcSerializerAttribute : Attribute
    {
        public Type SerializerType { get; }

        public JsonRpcSerializerAttribute(Type serializerType)
        {
            if (!typeof(IJsonRpcSerializer).IsAssignableFrom(serializerType))
            {
                throw new ArgumentException($"Expected implementation of {nameof(IJsonRpcSerializer)}", nameof(serializerType));
            }

            SerializerType = serializerType;
        }
    }
}