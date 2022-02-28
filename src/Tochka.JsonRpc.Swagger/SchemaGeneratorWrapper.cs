using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.Newtonsoft;
using Swashbuckle.AspNetCore.SwaggerGen;
using Tochka.JsonRpc.ApiExplorer;
using Tochka.JsonRpc.Common.Serializers;
using Tochka.JsonRpc.Server.Attributes;

namespace Tochka.JsonRpc.Swagger
{
    public class SchemaGeneratorWrapper : ISchemaGenerator
    {
        private readonly SchemaGeneratorOptions generatorOptions;
        private readonly ISerializerDataContractResolver defaultResolver;
        private readonly IEnumerable<IJsonRpcSerializer> serializers;

        public SchemaGeneratorWrapper(SchemaGeneratorOptions generatorOptions, ISerializerDataContractResolver defaultResolver, IEnumerable<IJsonRpcSerializer> serializers)
        {
            this.generatorOptions = generatorOptions;
            this.defaultResolver = defaultResolver;
            this.serializers = serializers;
        }

        public OpenApiSchema GenerateSchema(Type type, SchemaRepository schemaRepository, MemberInfo memberInfo = null, ParameterInfo parameterInfo = null)
        {
            var serializerType = type.GetCustomAttribute<JsonRpcTypeInfoAttribute>()?.SerializerType;
            var resolver = GetResolver(serializerType);
            var generator = new SchemaGenerator(generatorOptions, resolver);
            return generator.GenerateSchema(type, schemaRepository, memberInfo, parameterInfo);
        }

        private ISerializerDataContractResolver GetResolver(Type serializerType)
        {
            if (serializerType == null)
            {
                return defaultResolver;
            }

            var serializer = serializers.First(x => x.GetType() == serializerType);
            return new NewtonsoftDataContractResolver(serializer.Settings);
        }
    }
}