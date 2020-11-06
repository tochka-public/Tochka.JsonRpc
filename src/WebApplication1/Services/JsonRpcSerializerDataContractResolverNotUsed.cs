using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.Newtonsoft;
using Swashbuckle.AspNetCore.SwaggerGen;
using Tochka.JsonRpc.Common.Serializers;
using Tochka.JsonRpc.Server.Attributes;

namespace WebApplication1.Services
{
    public class JsonRpcSerializerDataContractResolverNotUsed : ISerializerDataContractResolver
    {
        private readonly NewtonsoftDataContractResolver defaultResolver;
        private readonly IEnumerable<IJsonRpcSerializer> serializers;
        private readonly ILogger log;
        private readonly SchemaGeneratorOptions schemaGeneratorOptions;
            
        public JsonRpcSerializerDataContractResolverNotUsed(NewtonsoftDataContractResolver defaultResolver, IEnumerable<IJsonRpcSerializer> serializers, IOptions<SchemaGeneratorOptions> options, ILogger<JsonRpcSerializerDataContractResolverNotUsed> log)
        {
            this.defaultResolver = defaultResolver;
            this.serializers = serializers;
            this.log = log;
            this.schemaGeneratorOptions = options.Value;
        }

        public DataContract GetDataContractForType(Type type)
        {
            var jsonRpcModelAttribute = type.GetCustomAttribute<JsonRpcSerializerAttribute>();
            if (jsonRpcModelAttribute == null)
            {
                log.LogDebug("Contract for {}: default", type.Name);
                return defaultResolver.GetDataContractForType(type);
            }
            
            var serializer = serializers.First(x => x.GetType() == jsonRpcModelAttribute.SerializerType);
            var resolver = new NewtonsoftDataContractResolver(schemaGeneratorOptions, serializer.Settings);
            log.LogDebug("Contract for {}: {}", type.Name, jsonRpcModelAttribute.SerializerType);
            //return resolver.GetDataContractForType(type);
            // unwrap request body type
            return resolver.GetDataContractForType(type);
        }
    }
}