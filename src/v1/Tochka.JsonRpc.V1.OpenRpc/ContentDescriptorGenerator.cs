using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Namotion.Reflection;
using NJsonSchema;
using NJsonSchema.Generation;
using NJsonSchema.Infrastructure;
using Tochka.JsonRpc.V1.Common.Serializers;
using Tochka.JsonRpc.V1.OpenRpc.Models;
using Tochka.JsonRpc.V1.OpenRpc.SchemaUtils;
using Tochka.JsonRpc.V1.Server;
using Tochka.JsonRpc.V1.Server.Models;

namespace Tochka.JsonRpc.V1.OpenRpc
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
            var isRequired = propertyType.GetContextAttribute<RequiredAttribute>() != null;
            return Generate(propertyType.PropertyType, id, serializer, isRequired);
        }

        private ContentDescriptor Generate(ContextualType type, string id, IJsonRpcSerializer serializer, bool isRequired)
        {
            var settings = new JsonSchemaGeneratorSettings();
            var schema = schemaGenerator.GenerateSchema(type, serializer);
            var contentDescriptor = new ContentDescriptor
            {
                Name = id,
                Schema = schema,
                Required = isRequired,
                Description = type.GetXmlDocsRemarks(),  // dont get confused: Description is taken from "remarks" tag;
                Summary = type.GetDescription(settings),  // Summary is taken from attributes or "summary" tag (method should be named "GetSummary"?)
                Deprecated = type.GetAttribute<ObsoleteAttribute>() != null
            };
            return contentDescriptor;
        }
    }
}
