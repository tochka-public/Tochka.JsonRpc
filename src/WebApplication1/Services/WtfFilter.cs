using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using Swashbuckle.AspNetCore.Newtonsoft;
using Swashbuckle.AspNetCore.SwaggerGen;
using Tochka.JsonRpc.Common;
using Tochka.JsonRpc.Common.Models.Id;
using Tochka.JsonRpc.Common.Models.Request;
using Tochka.JsonRpc.Common.Models.Request.Untyped;
using Tochka.JsonRpc.Common.Models.Response;
using Tochka.JsonRpc.Common.Models.Response.Untyped;
using Tochka.JsonRpc.Common.Serializers;
using Tochka.JsonRpc.Server;
using Tochka.JsonRpc.Server.Attributes;
using Tochka.JsonRpc.Server.Services;

namespace WebApplication1.Services
{
    public class BodyFilter : IRequestBodyFilter
    {
        private readonly SchemaGeneratorOptions options;
        private readonly IEnumerable<IJsonRpcSerializer> serializers;
        private readonly ILogger log;

        public BodyFilter(IOptions<SchemaGeneratorOptions> options, IEnumerable<IJsonRpcSerializer> serializers, ILogger<BodyFilter> log)
        {
            this.options = options.Value;
            this.serializers = serializers;
            this.log = log;
        }

        public void Apply(OpenApiRequestBody requestBody, RequestBodyFilterContext context)
        {
            var serializerType = context.BodyParameterDescription.Type.GetCustomAttribute<JsonRpcSerializerAttribute>()?.SerializerType;
            if (serializerType == null)
            {
                return;
            }

            var originalSelector = options.SchemaIdSelector;
            options.SchemaIdSelector = t => HackSchemaIdSelector(t, serializerType);

            var serializer = serializers.First(x => x.GetType() == serializerType);
            var resolver = new NewtonsoftDataContractResolver(options, serializer.Settings);
            
            var generator = new Swashbuckle.AspNetCore.SwaggerGen.SchemaGenerator(options, resolver);

            var body = context.BodyParameterDescription;
            var repo = context.SchemaRepository; //new SchemaRepository();
            var schema = generator.GenerateSchema(body.Type, repo, body.PropertyInfo(), body.ParameterInfo());
            requestBody.Content[JsonRpcConstants.ContentType] = new OpenApiMediaType{Schema = schema};

            options.SchemaIdSelector = originalSelector;

            /*
            
            придется генерить разные сваггер-документы для разных сериалайзеров
            потому что один и тот же тип может использоваться в виде camel/snake

            либо как-то хакнуть генератор схемы и различать типы в зависимости от action-а/сериалайзера
            и хакнуть _generatorOptions.SchemaIdSelector
            
            изнутри SchemaGenerator.GenerateReferencedSchema:
                string schemaId = this._generatorOptions.SchemaIdSelector(type);
                schemaRepository.RegisterType(type, schemaId);
                OpenApiSchema schema = definitionFactory();
                this.ApplyFilters(schema, type, schemaRepository, (MemberInfo) null, (ParameterInfo) null);

            repo.TryLookubByType все ломает, грохнуть не получится.
            надо чтобы изначально схема генерилась с нужным генератором?
            может, в него напихать поиск сериалайзера?
             */
        }

        private string HackSchemaIdSelector(Type modelType, Type serializerType)
        {
            if (!modelType.IsConstructedGenericType)
                return modelType.Name.Replace("[]", "Array") + serializerType.Name;
            return modelType.GetGenericArguments()
                       .Select(x => HackSchemaIdSelector(x, serializerType))
                       .Aggregate((previous, current) => previous + current)
                   + modelType.Name.Split('`').First()
                   + serializerType.Name;
        }

        /*private OpenApiRequestBody GenerateRequestBodyFromBodyParameter(ApiDescription apiDescription,SchemaRepository schemaRepository,ApiParameterDescription bodyParameter)
        {
            IEnumerable<string> source = this.InferRequestContentTypes(apiDescription);
            bool flag = bodyParameter.CustomAttributes().Any<object>((Func<object, bool>)(attr => SwaggerGenerator.RequiredAttributeTypes.Contains<Type>(attr.GetType())));
            OpenApiSchema schema = this._schemaGenerator.GenerateSchema(bodyParameter.ModelMetadata.ModelType, schemaRepository, (MemberInfo)bodyParameter.PropertyInfo(), bodyParameter.ParameterInfo());
            return new OpenApiRequestBody()
            {
                Content = (IDictionary<string, OpenApiMediaType>)source.ToDictionary<string, string, OpenApiMediaType>((Func<string, string>)(contentType => contentType), (Func<string, OpenApiMediaType>)(contentType => new OpenApiMediaType()
                {
                    Schema = schema
                })),
                Required = flag
            };
        }*/
    }

    public class WtfFilter : ISchemaFilter
    {
        private readonly IMethodMatcher methodMatcher;
        private readonly IEnumerable<IJsonRpcSerializer> serializers;
        private readonly ILogger log;
        private readonly ISerializerDataContractResolver defaultResolver;
        private readonly SchemaGeneratorOptions schemaGeneratorOptions;

        public WtfFilter(IMethodMatcher methodMatcher, IEnumerable<IJsonRpcSerializer> serializers, ILogger<WtfFilter> log)
        {
            this.methodMatcher = methodMatcher;
            this.serializers = serializers;
            this.log = log;
            schemaGeneratorOptions = new SchemaGeneratorOptions();
            defaultResolver = new NewtonsoftDataContractResolver(schemaGeneratorOptions, new JsonSerializerSettings());
        }

        public void Apply(OpenApiSchema schema, SchemaFilterContext context)
        {
            // i'm too lazy to deal with polymorphism in OpenApi schema, will just override common props

            if (context.Type == typeof(IRpcId))
            {
                schema.Type = "string";
                schema.Example = new OpenApiString("1");
            }

            var isRequest = typeof(ICall).IsAssignableFrom(context.MemberInfo?.DeclaringType);
            var isResponse = typeof(IResponse).IsAssignableFrom(context.MemberInfo?.DeclaringType);

            if (isRequest || isResponse)
            {
                var name = context.MemberInfo?.Name;
                if (name == nameof(ICall.Jsonrpc))
                {
                    schema.Example = new OpenApiString(JsonRpcConstants.Version);
                }

                return;
                if (name == nameof(IUntypedCall.Params) || name == nameof(UntypedResponse.Result))
                {
                    log.LogDebug("FILTERING: {}", context.MemberInfo.Name);

                    return;
                    var attr = context.MemberInfo.DeclaringType.GetCustomAttribute<JsonRpcSerializerAttribute>();
                    var serializer = serializers.First(x => x.GetType() == attr.SerializerType);
                    var resolver = new NewtonsoftDataContractResolver(schemaGeneratorOptions, serializer.Settings);

                    ReplaceWithCustomProperties(schema, context.Type, resolver);
                }
            }

            



            // get serializer from attr
            // get DataContract
            // walk down through all schema.properties
            // replace names

            

            
            


            /*
            possible approach:
                get serializers, settings
                look for serializer Attribute (walk up into DeclaringType)
                try to get correct serializer and use it

            bad: we get calls for all things like "String" and "Int", etc

            better: generate schema manually in the first place
             */

            // TODO: filter different stuff
            // method = serialized method value (lookup MethodMetadata by parameter type, omg)
            // use MemberInfo.DeclaringType? emit type ALWAYS for guaranteed lookup!
            // params = [] or {} (works by default)
            // apply json serializers... omg... walk through schema, apply names using JsonRpcSerializer? or somehow affect schema generation befor filter?
        }

        private void ReplaceWithCustomProperties(OpenApiSchema schema, Type type, NewtonsoftDataContractResolver resolver)
        {
            var defaultContract = defaultResolver.GetDataContractForType(type);
            var customContract = resolver.GetDataContractForType(type);

            var defaultProps = defaultContract.ObjectProperties.ToDictionary(x => x.Name);
            var customProps = customContract.ObjectProperties.ToDictionary(x => x.MemberInfo.Name);

            var newProps = schema.Properties.Select(kv => (customProps[defaultProps[kv.Key].MemberInfo.Name].Name, defaultProps[kv.Key].MemberInfo, kv.Value)).ToList();

            schema.Properties.Clear();
            foreach (var newProp in newProps)
            {
                var t = GetUnderlyingType(newProp.Item2);
                ReplaceWithCustomProperties(newProp.Item3, t, resolver);
                schema.Properties[newProp.Item1] = newProp.Item3;
            }
        }

        public static Type GetUnderlyingType(MemberInfo member)
        {
            switch (member.MemberType)
            {
                case MemberTypes.Event:
                    return ((EventInfo)member).EventHandlerType;
                case MemberTypes.Field:
                    return ((FieldInfo)member).FieldType;
                case MemberTypes.Method:
                    return ((MethodInfo)member).ReturnType;
                case MemberTypes.Property:
                    return ((PropertyInfo)member).PropertyType;
                default:
                    throw new ArgumentException
                    (
                        "Input MemberInfo must be if type EventInfo, FieldInfo, MethodInfo, or PropertyInfo"
                    );
            }
        }
    }

    public class WtfBodyFilter : IRequestBodyFilter
    {
        public void Apply(OpenApiRequestBody requestBody, RequestBodyFilterContext context)
        {
            throw new NotImplementedException();
        }
    }
}