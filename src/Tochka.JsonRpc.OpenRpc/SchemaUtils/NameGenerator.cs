using System;
using NJsonSchema.Generation;
using Tochka.JsonRpc.Common.Serializers;
using Tochka.JsonRpc.Server;

namespace Tochka.JsonRpc.OpenRpc.SchemaUtils
{
    public class NameGenerator : ISchemaNameGenerator
    {
        private readonly IJsonRpcSerializer jsonRpcSerializer;

        public NameGenerator(IJsonRpcSerializer jsonRpcSerializer)
        {
            this.jsonRpcSerializer = jsonRpcSerializer;
        }

        public string Generate(Type type)
        {
            var result = jsonRpcSerializer.GetJsonName(type.Name).Json;
            return result;
        }
    }
}