using System.Reflection;
using JetBrains.Annotations;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using Tochka.JsonRpc.ApiExplorer;
using Tochka.JsonRpc.Common;
using Tochka.JsonRpc.Common.Models.Id;
using Tochka.JsonRpc.Common.Models.Request;
using Tochka.JsonRpc.Common.Models.Response;

namespace Tochka.JsonRpc.Swagger;

/// <summary>
/// Schema filter to fix JSON-RPC protocol properties ("headers") that could be broken by data serializer options
/// </summary>
[PublicAPI]
public class JsonRpcPropertiesFilter : ISchemaFilter
{
    /// <inheritdoc />
    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
        if (context.Type == typeof(IRpcId))
        {
            PatchIdSchema(schema);
        }
        else if (typeof(ICall).IsAssignableFrom(context.Type))
        {
            PatchRequest(schema, context);
        }
        else if (typeof(IResponse).IsAssignableFrom(context.Type))
        {
            PatchResponse(schema);
        }
    }

    private static void PatchIdSchema(OpenApiSchema idSchema)
    {
        idSchema.Nullable = true;
        idSchema.AdditionalPropertiesAllowed = false;
        idSchema.Example = new OpenApiString("1");
        idSchema.OneOf = new List<OpenApiSchema>
        {
            new() { Type = "string" },
            new() { Type = "number" }
        };
    }

    private static void PatchRequest(OpenApiSchema schema, SchemaFilterContext context)
    {
        PatchId(schema);
        PatchMethod(schema, context);
        PatchVersion(schema);
        PatchParams(schema);
        schema.Required.Add(JsonRpcConstants.MethodProperty);
        schema.Required.Add(JsonRpcConstants.JsonrpcVersionProperty);
    }

    private static void PatchResponse(OpenApiSchema schema)
    {
        PatchId(schema);
        PatchVersion(schema);
        PatchResult(schema);
    }

    private static void PatchId(OpenApiSchema schema) => TryFixPropertyKey(schema, JsonRpcConstants.IdProperty, out _);

    private static void PatchMethod(OpenApiSchema schema, SchemaFilterContext context)
    {
        var hasMethodSchema = TryFixPropertyKey(schema, JsonRpcConstants.MethodProperty, out var methodSchema);
        if (!hasMethodSchema)
        {
            return;
        }

        methodSchema!.Nullable = false;
        var methodName = context.Type.GetCustomAttribute<JsonRpcTypeMetadataAttribute>()?.MethodName;
        if (!string.IsNullOrEmpty(methodName))
        {
            methodSchema.Example = new OpenApiString(methodName);
        }
    }

    private static void PatchVersion(OpenApiSchema schema)
    {
        var hasVersionSchema = TryFixPropertyKey(schema, JsonRpcConstants.JsonrpcVersionProperty, out var versionSchema);
        if (!hasVersionSchema)
        {
            return;
        }

        versionSchema!.Nullable = false;
        versionSchema.Example = new OpenApiString(JsonRpcConstants.Version);
    }

    private static void PatchParams(OpenApiSchema schema) => TryFixPropertyKey(schema, JsonRpcConstants.ParamsProperty, out _);

    private static void PatchResult(OpenApiSchema schema) => TryFixPropertyKey(schema, JsonRpcConstants.ResultProperty, out _);

    // official json rpc properties could've been renamed by serializer (e.g. to upper case)
    private static bool TryFixPropertyKey(OpenApiSchema schema, string key, out OpenApiSchema? propertySchema)
    {
        propertySchema = null;
        var currentKey = schema.Properties.Keys.FirstOrDefault(k => k.Equals(key, StringComparison.OrdinalIgnoreCase));
        if (currentKey == null)
        {
            return false;
        }

        propertySchema = schema.Properties[currentKey];
        schema.Properties.Remove(currentKey);
        schema.Properties[key] = propertySchema;
        return true;
    }
}
