using System.Collections.Generic;
using Namotion.Reflection;
using NJsonSchema;
using Tochka.JsonRpc.Common.Serializers;

namespace Tochka.JsonRpc.OpenRpc.SchemaUtils
{
    public class SchemaGenerator
    {
        private readonly Dictionary<IJsonRpcSerializer, SchemaGeneratorItem> items = new Dictionary<IJsonRpcSerializer, SchemaGeneratorItem>();
        private readonly JsonSchema root = new JsonSchema();
        private readonly object lockObject = new object();

        public JsonSchema GenerateSchema(ContextualType type, IJsonRpcSerializer serializer)
        {
            var item = GetOrAdd(serializer);
            item.Generate(type);  // if first time will return schema itself, we don't need that
            return item.Generate(type);  // will always return reference
        }

        public IDictionary<string, JsonSchema> Definitions => root.Definitions;

        private SchemaGeneratorItem GetOrAdd(IJsonRpcSerializer serializer)
        {
            lock (lockObject)
            {
                if (items.TryGetValue(serializer, out var item))
                {
                    return item;
                }

                var newItem = new SchemaGeneratorItem(serializer, root);
                items.Add(serializer, newItem);
                return newItem;
            }
        }
    }
}