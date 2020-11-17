using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Namotion.Reflection;
using NJsonSchema;
using NJsonSchema.Infrastructure;
using Tochka.JsonRpc.Common.Serializers;
using Tochka.JsonRpc.OpenRpc.Models;
using Tochka.JsonRpc.OpenRpc.SchemaUtils;
using Tochka.JsonRpc.Server;
using Tochka.JsonRpc.Server.Models;

namespace Tochka.JsonRpc.OpenRpc
{
    public class ContentDescriptorGenerator
    {
        public ContentDescriptorGenerator(IEnumerable<IJsonRpcSerializer> serializers)
        {
            this.schemaGenerator = new SchemaGenerator();
            this.serializers = serializers;
        }

        private readonly SchemaGenerator schemaGenerator;
        private readonly IEnumerable<IJsonRpcSerializer> serializers;

        public IDictionary<string, JsonSchema> GetDefinitions() => new Dictionary<string, JsonSchema>(schemaGenerator.Definitions);

        public ContentDescriptor GenerateForType(ContextualType type, Type serializerType)
        {
            var serializer = serializers.First(x => x.GetType() == serializerType);
            var id = serializer.GetJsonName(type.TypeName).Json;
            var isRequired = type.GetAttribute<RequiredAttribute>() != null;
            return Generate(type, id, serializer, isRequired);
        }

        public ContentDescriptor GenerateForParameter(ContextualType type, Type serializerType, ParameterMetadata parameterMetadata)
        {
            var id = parameterMetadata.Name.Json;
            var serializer = serializers.First(x => x.GetType() == serializerType);
            return Generate(type, id, serializer, !parameterMetadata.IsOptional);
        }

        public ContentDescriptor GenerateForProperty(ContextualPropertyInfo propertyType, Type serializerType)
        {
            var serializer = serializers.First(x => x.GetType() == serializerType);
            var id = serializer.GetJsonName(propertyType.Name).Json;
            var isRequired = propertyType.GetAttribute<RequiredAttribute>() != null;
            return Generate(propertyType, id, serializer, isRequired);
        }

        private ContentDescriptor Generate(ContextualType type, string id, IJsonRpcSerializer serializer, bool isRequired)
        {
            var schema = schemaGenerator.GenerateSchema(type, serializer);
            var contentDescriptor = new ContentDescriptor
            {
                Name = id,
                Schema = schema,
                Required = isRequired,
                Description = type.GetDescription(),
                Summary = type.GetXmlDocsSummary(),
                Deprecated = type.GetAttribute<ObsoleteAttribute>() != null
            };
            return contentDescriptor;
        }
    }
}