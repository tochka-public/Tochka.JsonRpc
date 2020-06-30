using System;
using Newtonsoft.Json.Linq;

namespace Tochka.JsonRpc.Common.Serializers
{
    public static class Extensions
    {
        public static JContainer SerializeParams<T>(this IRpcSerializer serializer, T data)
        {
            if (data == null)
            {
                return null;
            }

            var serialized = JToken.FromObject(data, serializer.Serializer);
            if (serialized is JContainer serializedParams)
            {
                return serializedParams;
            }

            throw new InvalidOperationException($"Expected params [{typeof(T).Name}] to be serializable into [{nameof(JContainer)}], got [{serialized.GetType().Name}]");
        }
    }
}