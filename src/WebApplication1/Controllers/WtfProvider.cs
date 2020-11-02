using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Reflection.Emit;
using System.Security.Cryptography.X509Certificates;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Internal;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
using Microsoft.AspNetCore.Routing.Template;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using Tochka.JsonRpc.Common;
using Tochka.JsonRpc.Common.Models.Request;
using Tochka.JsonRpc.Common.Models.Response;
using Tochka.JsonRpc.Server.Binding;
using Tochka.JsonRpc.Server.Models;
using Tochka.JsonRpc.Server.Services;
using Tochka.JsonRpc.Server.Settings;

namespace WebApplication1.Controllers
{
    public class WtfProvider : IApiDescriptionProvider
    {
        private readonly IMethodMatcher methodMatcher;
        private readonly IModelMetadataProvider modelMetadataProvider;

        public WtfProvider(IMethodMatcher methodMatcher, IModelMetadataProvider modelMetadataProvider)
        {
            this.methodMatcher = methodMatcher;
            this.modelMetadataProvider = modelMetadataProvider;
        }

        public void OnProvidersExecuting(ApiDescriptionProviderContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            foreach (var action in context.Actions.OfType<ControllerActionDescriptor>())
            {
                if (action.AttributeRouteInfo != null && action.AttributeRouteInfo.SuppressPathMatching)
                {
                    continue;
                }

                var extensionData = action.GetProperty<ApiDescriptionActionData>();
                var methodMetadata = action.GetProperty<MethodMetadata>();
                if (extensionData != null && methodMetadata != null)
                {

                    context.Results.Add(CreateApiDescription(action, extensionData.GroupName, methodMetadata));
                }
            }
        }

        private ApiDescription CreateApiDescription(ControllerActionDescriptor action, string groupName, MethodMetadata methodMetadata)
        {
            var apiDescription = new ApiDescription
            {
                ActionDescriptor = action,
                GroupName = groupName,
                HttpMethod = HttpMethods.Post,
            };

            var parsedTemplate = TemplateParser.Parse(action.AttributeRouteInfo.Template);
            apiDescription.RelativePath = GetRelativePathWithHash(parsedTemplate, methodMetadata);

            var templateParameters = parsedTemplate?.Parameters?.ToList() ?? new List<TemplatePart>();
            var parameterContext = new ApiParameterContext(modelMetadataProvider, action, templateParameters);

            return apiDescription;
        }

        private string GetRelativePathWithHash(RouteTemplate parsedTemplate, MethodMetadata methodMetadata)
        {
            if (parsedTemplate == null)
            {
                return null;
            }

            var segments = new List<string>();

            foreach (var segment in parsedTemplate.Segments)
            {
                var currentSegment = string.Empty;
                foreach (var part in segment.Parts)
                {
                    if (part.IsLiteral)
                    {
                        currentSegment += part.Text;
                    }
                    else if (part.IsParameter)
                    {
                        currentSegment += "{" + part.Name + "}";
                    }
                }

                segments.Add(currentSegment);
            }

            var methodName = methodMatcher.GetActionName(methodMetadata);
            return string.Join("/", segments) + $"#{methodName}";
        }

        public void OnProvidersExecuted(ApiDescriptionProviderContext context)
        {
        }

        public int Order => -700;
    }

    internal class ApiParameterContext
    {
        public ApiParameterContext(
            IModelMetadataProvider metadataProvider,
            ControllerActionDescriptor actionDescriptor,
            IReadOnlyList<TemplatePart> routeParameters)
        {
            MetadataProvider = metadataProvider;
            ActionDescriptor = actionDescriptor;
            RouteParameters = routeParameters;

            Results = new List<ApiParameterDescription>();
        }

        public ControllerActionDescriptor ActionDescriptor { get; }

        public IModelMetadataProvider MetadataProvider { get; }

        public IList<ApiParameterDescription> Results { get; }

        public IReadOnlyList<TemplatePart> RouteParameters { get; }
    }

    public class KostylProvider : IApiDescriptionProvider
    {
        private readonly IMethodMatcher methodMatcher;
        private readonly SchemaGeneratorOptions schemaGeneratorOptions;
        private readonly ModelTypeEmitter modelTypeEmitter;

        public KostylProvider(IMethodMatcher methodMatcher, SchemaGeneratorOptions schemaGeneratorOptions)
        {
            this.methodMatcher = methodMatcher;
            this.schemaGeneratorOptions = schemaGeneratorOptions;
            modelTypeEmitter = new ModelTypeEmitter();
        }

        public void OnProvidersExecuting(ApiDescriptionProviderContext context)
        {
        }

        public void OnProvidersExecuted(ApiDescriptionProviderContext context)
        {
            foreach (var apiDescription in context.Results.Where(x => x.ActionDescriptor.GetProperty<MethodMetadata>() != null))
            {
                var methodMetadata = apiDescription.ActionDescriptor.GetProperty<MethodMetadata>();
                var methodName = methodMatcher.GetActionName(methodMetadata);
                PatchPath(apiDescription, methodName);
                PatchParams(apiDescription, methodMetadata, methodName);
                PatchRequestFormats(apiDescription);
                PatchResponseTypes(apiDescription);
            }
        }

        /// <summary>
        /// Add #method to JsonRpc urls to make them different for swagger
        /// </summary>
        /// <param name="apiDescription"></param>
        /// <param name="methodMetadata"></param>
        private void PatchPath(ApiDescription apiDescription, string methodName)
        {
            
            apiDescription.RelativePath += $"#{methodName}";
        }

        /// <summary>
        /// Hide separate JsonRpc-bound parameters and wrap them into Request`T. T is emitted dynamically to allow swagger schema generation
        /// </summary>
        /// <param name="apiDescription"></param>
        /// <param name="methodMetadata"></param>
        /// <param name="methodName"></param>
        private void PatchParams(ApiDescription apiDescription, MethodMetadata methodMetadata, string methodName)
        {
            var requestType = GetRequestType(apiDescription, methodMetadata);

            var allJsonRpcParams = apiDescription.ParameterDescriptions
                .Where(x => x.ParameterDescriptor.BindingInfo.BinderType == typeof(JsonRpcModelBinder))
                .ToList();

            // hide params bound by JsonRpc
            foreach (var apiParameterDescription in allJsonRpcParams)
            {
                apiDescription.ParameterDescriptions.Remove(apiParameterDescription);
            }

            var jsonRpcParamsDescription = new ApiParameterDescription()
            {
                Name = "params",
                Source = BindingSource.Body,
                ParameterDescriptor = null,
                DefaultValue = null,
                IsRequired = true,
                ModelMetadata = new DumbMetadata(ModelMetadataIdentity.ForType(requestType)), // swagger generator uses ModelMetadata.ModelType
                RouteInfo = null,
                Type = requestType
            };
            apiDescription.ParameterDescriptions.Add(jsonRpcParamsDescription);

            // TODO use schema filters?
            return;
            schemaGeneratorOptions.CustomTypeMappings[requestType] = () => new OpenApiSchema()
            {
                Example = new OpenApiObject()
                {
                    {"id", new OpenApiString("the_id")},
                    {"jsonrpc", new OpenApiString("2.0")},
                    {"method", new OpenApiString(methodName)},
                    {"params", new OpenApiObject()},
                    // uhh... what next? maybe use schema filters?
                }
            };

            

            // TODO: apply custom serialization for examples
            // TODO: XMLDOC jsonRpc = 2.0 works, but will it work for referenced nuget library?
            // TODO: XMLDOC id does not work because it is object?
        }

        private Type GetRequestType(ApiDescription apiDescription, MethodMetadata methodMetadata)
        {
            var objectBoundParam = apiDescription.ParameterDescriptions
                .FirstOrDefault(x => methodMetadata.Get(x.Name)?.BindingStyle == BindingStyle.Object);
            var arrayBoundParam = apiDescription.ParameterDescriptions
                .FirstOrDefault(x => methodMetadata.Get(x.Name)?.BindingStyle == BindingStyle.Array);
            var defaultBoundParams = apiDescription.ParameterDescriptions
                .Where(x => methodMetadata.Get(x.Name)?.BindingStyle == BindingStyle.Default)
                .ToDictionary(x => x.Name, x => x.Type);

            if (arrayBoundParam != null)
            {
                // return Request<List<T>> (or whatever collection is used)
                // because other stuff won't bind if its type is different from T
                // so no difference for user, it is always visible as List<T>
                return typeof(Request<>).MakeGenericType(arrayBoundParam.Type);
            }

            if (objectBoundParam != null && !defaultBoundParams.Any())
            {
                // only whole object is bound, return Request<T>
                return typeof(Request<>).MakeGenericType(objectBoundParam.Type);
            }

            // emit type with properties corresponding to bound parameters
            if (objectBoundParam != null)
            {
                // also add properties from bound object
                foreach (var propertyInfo in objectBoundParam.Type.GetProperties())
                {
                    defaultBoundParams[propertyInfo.Name] = propertyInfo.PropertyType;
                }
            }

            var requestBodyType = modelTypeEmitter.CreateModelType(methodMetadata.Action.Original, defaultBoundParams);
            return typeof(Request<>).MakeGenericType(requestBodyType);
        }

        /// <summary>
        /// Set request content-type as application/json
        /// </summary>
        /// <param name="apiDescription"></param>
        private static void PatchRequestFormats(ApiDescription apiDescription)
        {
            apiDescription.SupportedRequestFormats.Clear();
            apiDescription.SupportedRequestFormats.Add(new ApiRequestFormat()
            {
                Formatter = null,
                MediaType = JsonRpcConstants.ContentType
            });
        }

        /// <summary>
        /// Wrap action response type into Response`T, set content-type as application/json and HTTP code 200
        /// </summary>
        /// <param name="apiDescription"></param>
        private static void PatchResponseTypes(ApiDescription apiDescription)
        {
            var existingResponse = apiDescription.SupportedResponseTypes.Single();
            var responseType = typeof(Response<>).MakeGenericType(existingResponse.Type);
            apiDescription.SupportedResponseTypes.Clear();
            apiDescription.SupportedResponseTypes.Add(new ApiResponseType()
            {
                ApiResponseFormats = new List<ApiResponseFormat>()
                {
                    new ApiResponseFormat()
                    {
                        MediaType = JsonRpcConstants.ContentType,
                        Formatter = null
                    }
                },
                IsDefaultResponse = false,
                ModelMetadata = new DumbMetadata(ModelMetadataIdentity.ForType(responseType)),
                StatusCode = (int)HttpStatusCode.OK,
                Type = responseType
            });
        }

        /// <summary>
        /// Arbitrary number greater than -1000 in DefaultApiDescriptionProvider
        /// </summary>
        public int Order => 100;
    }

    public class WtfFilter : ISchemaFilter
    {
        private readonly IMethodMatcher methodMatcher;

        public WtfFilter(IMethodMatcher methodMatcher)
        {
            this.methodMatcher = methodMatcher;
        }

        public void Apply(OpenApiSchema schema, SchemaFilterContext context)
        {
            // TODO: filter different stuff
            // IRpcId = 1 (easy)
            // jsonrpc = 2.0 (easy)
            // method = serialized method value (lookup MethodMetadata by parameter type, omg)
            // use MemberInfo.DeclaringType? emit type ALWAYS for guaranteed lookup!
            // params = [] or {} (works by default)
            // apply json serializers... omg... walk through schema, apply names using JsonRpcSerializer? or somehow affect schema generation befor filter?
        }
    }

    /// <summary>
    /// Creates types for proper schema generation. See https://jonathancrozier.com/blog/time-to-reflect-how-to-add-properties-to-a-c-sharp-object-dynamically-at-runtime
    /// </summary>
    public class ModelTypeEmitter
    {
        private readonly ModuleBuilder moduleBuilder;

        public ModelTypeEmitter()
        {
            var uniqueIdentifier = Guid.NewGuid().ToString();
            var assemblyName = new AssemblyName(uniqueIdentifier);

            var assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.RunAndCollect);
            moduleBuilder = assemblyBuilder.DefineDynamicModule(uniqueIdentifier);
        }

        public Type CreateModelType(string actionName, IReadOnlyDictionary<string, Type> properties)
        {
            var typeBuilder = moduleBuilder.DefineType($"{actionName}ParamsModel", TypeAttributes.Public);

            foreach (var property in properties)
            {
                CreateProperty(typeBuilder, property.Key, property.Value);
            }
            return typeBuilder.CreateType();
        }

        private void CreateProperty(TypeBuilder typeBuilder, string propertyName, Type propertyType)
        {
            var fieldName = $"_{propertyName}";
            var fieldBuilder = typeBuilder.DefineField(fieldName, propertyType, FieldAttributes.Private);
            // The property set and get methods require a special set of attributes.
            var getSetAttributes = MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig;
            // Define the 'get' accessor method.
            var getMethodBuilder = typeBuilder.DefineMethod($"get_{propertyName}", getSetAttributes, propertyType, Type.EmptyTypes);
            var propertyGetGenerator = getMethodBuilder.GetILGenerator();
            propertyGetGenerator.Emit(OpCodes.Ldarg_0);
            propertyGetGenerator.Emit(OpCodes.Ldfld, fieldBuilder);
            propertyGetGenerator.Emit(OpCodes.Ret);
            // Define the 'set' accessor method.
            var setMethodBuilder = typeBuilder.DefineMethod($"set_{propertyName}", getSetAttributes, null, new[] { propertyType });
            var propertySetGenerator = setMethodBuilder.GetILGenerator();
            propertySetGenerator.Emit(OpCodes.Ldarg_0);
            propertySetGenerator.Emit(OpCodes.Ldarg_1);
            propertySetGenerator.Emit(OpCodes.Stfld, fieldBuilder);
            propertySetGenerator.Emit(OpCodes.Ret);
            // Lastly, we must map the two methods created above to a PropertyBuilder and their corresponding behaviors, 'get' and 'set' respectively.
            var propertyBuilder = typeBuilder.DefineProperty(propertyName, PropertyAttributes.HasDefault, propertyType, null);
            propertyBuilder.SetGetMethod(getMethodBuilder);
            propertyBuilder.SetSetMethod(setMethodBuilder);
        }
    }

    public class DumbMetadata : ModelMetadata
    {
        public DumbMetadata(ModelMetadataIdentity identity) : base(identity)
        {
        }

        public override IReadOnlyDictionary<object, object> AdditionalValues { get; }
        public override ModelPropertyCollection Properties { get; }
        public override string BinderModelName { get; }
        public override Type BinderType { get; }
        public override BindingSource BindingSource { get; }
        public override bool ConvertEmptyStringToNull { get; }
        public override string DataTypeName { get; }
        public override string Description { get; }
        public override string DisplayFormatString { get; }
        public override string DisplayName { get; }
        public override string EditFormatString { get; }
        public override ModelMetadata ElementMetadata { get; }
        public override IEnumerable<KeyValuePair<EnumGroupAndName, string>> EnumGroupedDisplayNamesAndValues { get; }
        public override IReadOnlyDictionary<string, string> EnumNamesAndValues { get; }
        public override bool HasNonDefaultEditFormat { get; }
        public override bool HtmlEncode { get; }
        public override bool HideSurroundingHtml { get; }
        public override bool IsBindingAllowed { get; }
        public override bool IsBindingRequired { get; }
        public override bool IsEnum { get; }
        public override bool IsFlagsEnum { get; }
        public override bool IsReadOnly { get; }
        public override bool IsRequired { get; }
        public override ModelBindingMessageProvider ModelBindingMessageProvider { get; }
        public override int Order { get; }
        public override string Placeholder { get; }
        public override string NullDisplayText { get; }
        public override IPropertyFilterProvider PropertyFilterProvider { get; }
        public override bool ShowForDisplay { get; }
        public override bool ShowForEdit { get; }
        public override string SimpleDisplayProperty { get; }
        public override string TemplateHint { get; }
        public override bool ValidateChildren { get; }
        public override IReadOnlyList<object> ValidatorMetadata { get; }
        public override Func<object, object> PropertyGetter { get; }
        public override Action<object, object> PropertySetter { get; }
    }
}