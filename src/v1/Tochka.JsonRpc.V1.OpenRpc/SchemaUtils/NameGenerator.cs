using System;
using NJsonSchema.Generation;
using Tochka.JsonRpc.V1.Common.Serializers;
using Tochka.JsonRpc.V1.Server;

namespace Tochka.JsonRpc.V1.OpenRpc.SchemaUtils
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
            return jsonRpcSerializer.GetJsonName(type.Name).Json;
        }
    }
}