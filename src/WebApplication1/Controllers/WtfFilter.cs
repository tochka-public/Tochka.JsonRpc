using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using Tochka.JsonRpc.Common;
using Tochka.JsonRpc.Common.Models.Id;
using Tochka.JsonRpc.Common.Models.Request;
using Tochka.JsonRpc.Common.Models.Request.Untyped;
using Tochka.JsonRpc.Common.Models.Response;
using Tochka.JsonRpc.Server.Services;

namespace WebApplication1.Controllers
{
    public class WtfFilter : ISchemaFilter
    {
        private readonly IMethodMatcher methodMatcher;

        public WtfFilter(IMethodMatcher methodMatcher)
        {
            this.methodMatcher = methodMatcher;
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

            if (!isRequest && !isResponse)
            {
                return;
            }

            if (context.MemberInfo?.Name == nameof(ICall.Jsonrpc))
            {
                schema.Example = new OpenApiString(JsonRpcConstants.Version);
            }

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
    }

    public class WtfBodyFilter : IRequestBodyFilter
    {
        public void Apply(OpenApiRequestBody requestBody, RequestBodyFilterContext context)
        {
            throw new System.NotImplementedException();
        }
    }
}