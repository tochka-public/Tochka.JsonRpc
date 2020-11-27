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
    public class JsonRpcTypeInfoAttribute : Attribute
    {
        public Type SerializerType { get; }
        public string ActionName { get; }

        public JsonRpcTypeInfoAttribute(Type serializerType, string actionName)
        {
            if (!typeof(IJsonRpcSerializer).IsAssignableFrom(serializerType))
            {
                throw new ArgumentException($"Expected implementation of {nameof(IJsonRpcSerializer)}", nameof(serializerType));
            }

            if (string.IsNullOrEmpty(actionName))
            {
                throw new ArgumentNullException(nameof(actionName));
            }

            SerializerType = serializerType;
            ActionName = actionName;
        }
    }
}