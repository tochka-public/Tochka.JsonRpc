using System.Linq;
using System.Reflection;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using Tochka.JsonRpc.ApiExplorer;
using Tochka.JsonRpc.Common;
using Tochka.JsonRpc.Common.Models.Id;
using Tochka.JsonRpc.Common.Models.Request;
using Tochka.JsonRpc.Common.Models.Response;
using Tochka.JsonRpc.Server.Attributes;

namespace Tochka.JsonRpc.Swagger
{
    /// <summary>
    /// Don't want to deal with polymorphism in OpenApi schema, will just override common props: Id, Version
    /// </summary>
    public class CommonJsonRpcPropertiesFilter : ISchemaFilter
    {
        public void Apply(OpenApiSchema schema, SchemaFilterContext context)
        {
            if (context.Type == typeof(IRpcId))
            {
                PatchId(schema);
            }
            else if (typeof(ICall).IsAssignableFrom(context.Type))
            {
                PatchRequest(schema, context);
            }

            else if (typeof(IResponse).IsAssignableFrom(context.Type))
            {
                PatchResponse(schema, context);
            }
        }

        
        private void PatchRequest(OpenApiSchema schema, SchemaFilterContext context)
        {
            PatchVersion(schema.Properties[JsonRpcConstants.JsonrpcVersionProperty]);
            var methodSchema = schema.Properties[JsonRpcConstants.MethodProperty];
            var actionName = context.Type.GetCustomAttribute<JsonRpcTypeInfoAttribute>().ActionName;
            methodSchema.Nullable = false;
            methodSchema.Example = new OpenApiString(actionName);
        }
        
        private void PatchResponse(OpenApiSchema schema, SchemaFilterContext context)
        {
            PatchVersion(schema.Properties[JsonRpcConstants.JsonrpcVersionProperty]);
        }
        
        private void PatchId(OpenApiSchema idSchema)
        {
            idSchema.Nullable = false;
            idSchema.AdditionalPropertiesAllowed = false;
            idSchema.Type = "string";
            idSchema.Example = new OpenApiString("1");
        }
        
        private void PatchVersion(OpenApiSchema versionSchema)
        {
            versionSchema.Nullable = false;
            versionSchema.Example = new OpenApiString(JsonRpcConstants.Version);
        }

    }
}