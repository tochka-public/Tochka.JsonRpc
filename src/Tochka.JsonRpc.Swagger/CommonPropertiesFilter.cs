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
    /// <summary>
    /// Don't want to deal with polymorphism in OpenApi schema, will just override common props: Id, Version
    /// </summary>
    public class CommonPropertiesFilter : ISchemaFilter
    {
        public void Apply(OpenApiSchema schema, SchemaFilterContext context)
        {
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
            }
        }
    }
}