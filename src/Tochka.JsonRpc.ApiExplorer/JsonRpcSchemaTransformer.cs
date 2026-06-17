using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi;
using Tochka.JsonRpc.Common;
using Tochka.JsonRpc.Common.Models.Id;
using Tochka.JsonRpc.Common.Models.Request;
using Tochka.JsonRpc.Common.Models.Response;

namespace Tochka.JsonRpc.ApiExplorer;

public class JsonRpcSchemaTransformer
(
    ILogger<JsonRpcSchemaTransformer> log
) : IOpenApiSchemaTransformer
{
    public async Task TransformAsync(OpenApiSchema schema, OpenApiSchemaTransformerContext context, CancellationToken cancellationToken)
    {
        var target = context.JsonTypeInfo.Type;
        if (target == typeof(IRpcId))
        {
            PatchIdType(schema);
        }
        else if (typeof(ICall).IsAssignableFrom(target))
        {
            PatchRequestType(schema, context);
        }
        else if (typeof(IResponse).IsAssignableFrom(target))
        {
            PatchResponseType(schema);
        }
    }

    private void PatchRequestType(OpenApiSchema schema, OpenApiSchemaTransformerContext context)
    {
        log.LogDebug("patch request {s}", schema);
        PatchId(schema);
        PatchMethod(schema, context);
        PatchVersion(schema);
        PatchParams(schema);
        schema.Required ??= new HashSet<string>();
        schema.Required.Add(JsonRpcConstants.MethodProperty);
        schema.Required.Add(JsonRpcConstants.JsonrpcVersionProperty);
    }

    private void PatchResponseType(IOpenApiSchema schema)
    {
        log.LogDebug("patch response {s}", schema);
        PatchId(schema);
        PatchVersion(schema);
        PatchResult(schema);
    }

    private static void PatchIdType(OpenApiSchema schema)
    {
        schema.Type = JsonSchemaType.String | JsonSchemaType.Number | JsonSchemaType.Null;
        schema.AdditionalPropertiesAllowed = false;
        schema.Example = ExampleNumber;
    }

    private static void PatchId(IOpenApiSchema schema) => TryFixPropertyKey(schema, JsonRpcConstants.IdProperty, out _);

    private static void PatchMethod(IOpenApiSchema schema, OpenApiSchemaTransformerContext context)
    {
        if (!TryFixPropertyKey(schema, JsonRpcConstants.MethodProperty, out var propSchema))
        {
            return;
        }

        var methodName = context.JsonTypeInfo.Type.GetCustomAttribute<JsonRpcTypeMetadataAttribute>()?.MethodName;
        if (string.IsNullOrEmpty(methodName))
        {
            return;
        }

        propSchema.Type = JsonSchemaType.String;
        propSchema.Const = methodName;
    }

    private static void PatchVersion(IOpenApiSchema schema)
    {
        if (!TryFixPropertyKey(schema, JsonRpcConstants.JsonrpcVersionProperty, out var propSchema))
        {
            return;
        }

        propSchema.Type = JsonSchemaType.String;
        propSchema.Const = JsonRpcConstants.Version;
    }

    private static void PatchParams(IOpenApiSchema schema)
    {
        if (!TryFixPropertyKey(schema, JsonRpcConstants.ParamsProperty, out var propSchema))
        {
            return;
        }

        propSchema.Metadata?.Remove(NullableProperty);
    }

    private static void PatchResult(IOpenApiSchema schema)
    {
        if (!TryFixPropertyKey(schema, JsonRpcConstants.ResultProperty, out var propSchema))
        {
            return;
        }

        propSchema.Metadata?.Remove(NullableProperty);
    }

    /// <summary>
    /// Fix naming of JsonRpc header properties that might've been renamed by serializer (e.g. to upper case)
    /// </summary>
    private static bool TryFixPropertyKey(IOpenApiSchema schema, string key, [NotNullWhen(true)] out OpenApiSchema? propertySchema)
    {
        propertySchema = null;
        var currentKey = schema.Properties?.Keys.FirstOrDefault(k => k.Equals(key, StringComparison.OrdinalIgnoreCase));
        if (currentKey == null)
        {
            return false;
        }

        propertySchema = (OpenApiSchema) schema.Properties![currentKey];
        schema.Properties.Remove(currentKey);
        schema.Properties[key] = propertySchema;
        return true;
    }

    /// <summary>
    /// Used to make Request.Params and Response.Result non-nullable. Request'T and Response'T types don't carry real nullability info:<br />
    /// 1. JsonRpcParameterMetadata does not track nullability of action arguments or return types
    /// 2. generic parameter T can not hold nullability information because NRT works on properties and method arguments, not types<br />
    /// </summary>
    /// <remarks>Value from internal Microsoft.AspNetCore.OpenApi.OpenApiConstants.NullableProperty</remarks>
    private const string NullableProperty = "x-is-nullable-property";

    private static readonly JsonValue ExampleNumber = JsonValue.Create(1);
}
