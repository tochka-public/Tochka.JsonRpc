using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Namotion.Reflection;
using NJsonSchema;
using Tochka.JsonRpc.ApiExplorer;
using Tochka.JsonRpc.Common.Models.Request;
using Tochka.JsonRpc.OpenRpc.Models;
using Tochka.JsonRpc.Server.Models;
using Tochka.JsonRpc.Server.Settings;

namespace Tochka.JsonRpc.OpenRpc
{
    public class OpenRpcGenerator
    {
        private readonly IApiDescriptionGroupCollectionProvider apiDescriptionsProvider;
        private readonly ContentDescriptorGenerator contentDescriptorGenerator;
        private readonly OpenRpcOptions options;
        private readonly JsonRpcOptions jsonRpcOptions;


        public OpenRpcGenerator(IOptions<JsonRpcOptions> jsorRpcOptions, IOptions<OpenRpcOptions> options,IApiDescriptionGroupCollectionProvider apiDescriptionsProvider, ContentDescriptorGenerator contentDescriptorGenerator)
        {
            this.options = options.Value;
            this.jsonRpcOptions = jsorRpcOptions.Value;
            this.apiDescriptionsProvider = apiDescriptionsProvider;
            this.contentDescriptorGenerator = contentDescriptorGenerator;
        }

        public Models.OpenRpc GetDocument(OpenApiInfo info, string documentName, Uri host)
        {
            var defaultRoute = jsonRpcOptions.DefaultMethodOptions.Route;
            var document = new Models.OpenRpc
            {
                Openrpc = OpenRpcConstants.SpecVersion,
                Info = info,
                Servers = GetServers(host, defaultRoute),
                Methods = GetMethods(documentName, host, defaultRoute),
                Components = new Components(),
            };
            SetSchemas(document);
            return document;
        }

        /// <summary>
        /// Magic required to create $ref values with correct path
        /// </summary>
        /// <param name="document"></param>
        private void SetSchemas(Models.OpenRpc document)
        {
            document.Components.Schemas = contentDescriptorGenerator.GetDefinitions();
            foreach (var schema in document.Components.Schemas)
            {
                // this is used to determine $ref path
                // TODO document or document.Components? both work...
                schema.Value.Parent = document.Components;
            }

            // this actually creates $ref values
            JsonSchemaReferenceUtilities.UpdateSchemaReferencePaths(document, false, OpenRpcConstants.JsonSerializerSettings.ContractResolver);
        }

        private List<Method> GetMethods(string documentName, Uri host, string defaultRoute)
        {
            return apiDescriptionsProvider.ApiDescriptionGroups.Items
                .SelectMany(g => g.Items)
                .Where(x => !(options.IgnoreObsoleteActions && x.IsObsoleteTransitive()))
                .Where(x => x.ActionDescriptor.GetProperty<MethodMetadata>() != null)  // process only JsonRpc actions
                .Where(x => options.DocInclusionPredicate(documentName, x))
                .OrderBy(x => x.Properties[ApiExplorerConstants.ActionNameProperty])
                .Select(x => GetMethod(x, host, defaultRoute))
                .ToList();
        }

        private Method GetMethod(ApiDescription apiDescription, Uri host, string defaultRoute)
        {
            var methodMetadata = apiDescription.ActionDescriptor.GetProperty<MethodMetadata>();
            var maybeOverrideServers = methodMetadata.MethodOptions.Route != defaultRoute
                ? GetServers(host, methodMetadata.MethodOptions.Route)
                : null;
            var result = new Method()
            {
                Name = apiDescription.Properties[ApiExplorerConstants.ActionNameProperty] as string,
                Deprecated = apiDescription.IsObsoleteTransitive(),
                Servers = maybeOverrideServers,
                Summary = null, // TODO как? атрибуты? подсмотреть в сваггер
                Description = null, // TODO как? атрибуты? подсмотреть в сваггер
                Links = null, // TODO как? атрибуты? подсмотреть в сваггер
                ExternalDocs = null, // TODO как? атрибуты? подсмотреть в сваггер
                Tags = null, // TODO как? атрибуты? подсмотреть в сваггер
                Examples = null, // TODO как? атрибуты? подсмотреть в сваггер
                Errors = null, // TODO как? атрибуты? подсмотреть в сваггер
                ParamStructure = GetParamStructure(methodMetadata),
                Result = GetResultContentDescriptor(apiDescription, methodMetadata),
                Params = GetParamsContentDescriptors(apiDescription, methodMetadata).ToList(),
            };
            return result;
        }

        private IEnumerable<ContentDescriptor> GetParamsContentDescriptors(ApiDescription apiDescription, MethodMetadata methodMetadata)
        {
            var requestType = apiDescription.ParameterDescriptions.Single(x => IsInheritedFromRequest(x.Type)).Type;  // ignore other arguments
            var bodyType = requestType.BaseType.GenericTypeArguments.Single();  // unpack Request<T>
            var isCollection = Tochka.JsonRpc.Common.Utils.IsCollection(bodyType);
            var serializerType = methodMetadata.MethodOptions.RequestSerializer;

            if (isCollection)
            {
                // TODO how to bind params of variable length? will just bind single element for now
                // TODO better item name? using type name for now
                var itemType = bodyType.GetEnumerableItemType().ToContextualType();
                yield return contentDescriptorGenerator.GenerateForType(itemType, serializerType);
            }
            else
            {
                // iterate properties: they are either method arguments or bound from some object
                foreach (var propertyInfo in bodyType.GetProperties())
                {
                    var property = propertyInfo.ToContextualProperty();
                    // TODO what if param and prop names collide? eg public void Foo(int bar, [FromParams(BindingStyle.Object)] HasBar objectWithBarProperty){}
                    if (methodMetadata.Parameters.TryGetValue(propertyInfo.Name, out var parameterMetadata))
                    {
                        yield return contentDescriptorGenerator.GenerateForParameter(property, serializerType, parameterMetadata);
                    }
                    else
                    {
                        yield return contentDescriptorGenerator.GenerateForProperty(property, serializerType);
                    }
                }
            }
        }

        private ContentDescriptor GetResultContentDescriptor(ApiDescription apiDescription, MethodMetadata methodMetadata)
        {
            var responseType = apiDescription.SupportedResponseTypes.Single().Type;
            var bodyType = responseType.BaseType.GenericTypeArguments.Single().ToContextualType();  // unpack Response<T>
            return contentDescriptorGenerator.GenerateForType(bodyType, methodMetadata.MethodOptions.RequestSerializer);
        }

        private List<Models.Server> GetServers(Uri host, string route)
        {
            var builder = new UriBuilder(host)
            {
                Path = route
            };
            var server = new Models.Server()
            {
                Url = builder.Uri
            };

            return new List<Models.Server>()
            {
                server
            };
        }

        private MethodObjectParamStructure GetParamStructure(MethodMetadata methodMetadata)
        {
            var bindStyles = methodMetadata.Parameters
                .Select(x => x.Value.BindingStyle)
                .Distinct()
                .ToHashSet();

            switch (bindStyles.Count)
            {
                case 0:
                    // no parameters will work both ways
                    return MethodObjectParamStructure.Either;
                case 1:
                    return bindStyles.Single().ToParamStructure();
                default:
                    var hasArrayBinding = bindStyles.Contains(BindingStyle.Array);
                    var hasObjectBinding = bindStyles.Contains(BindingStyle.Object);
                    // mixed binding is bad but it's up to user to try this out
                    if (hasArrayBinding && !hasObjectBinding)
                    {
                        // if 'array' is present, 'default' binding works, 'object' binding does not
                        return MethodObjectParamStructure.ByPosition;
                    }
                    if (hasObjectBinding && !hasArrayBinding)
                    {
                        // if 'object' is present, 'default' binding works, 'array' binding does not
                        return MethodObjectParamStructure.ByName;
                    }

                    // both 'object' and 'array' binding will not work but we have to return something valid
                    return MethodObjectParamStructure.Either;
            }
        }

        private static bool IsInheritedFromRequest(Type t)
        {
            return t.BaseType?.IsConstructedGenericType == true && t.BaseType.GetGenericTypeDefinition() == typeof(Request<>);
        }
    }
}