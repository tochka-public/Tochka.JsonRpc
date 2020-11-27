using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;

namespace Tochka.JsonRpc.Swagger
{
    /// <summary>
    /// Registers SwaggerUI middleware at the beginning of the pipeline with JSON Rpc docs
    /// </summary>
    public class SwaggerUiWithJsonRpcStartupFilter : IStartupFilter
    {
        public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
        {
            return builder =>
            {
                builder.UseSwagger();
                builder.UseSwaggerUiWithJsonRpc();
                next(builder);
            };
        }
    }
}