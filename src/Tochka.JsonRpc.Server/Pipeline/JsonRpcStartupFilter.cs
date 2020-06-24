using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;

namespace Tochka.JsonRpc.Server.Pipeline
{
    /// <summary>
    /// Registers JSON Rpc middleware at the beginning of the pipeline
    /// </summary>
    public class JsonRpcStartupFilter : IStartupFilter
    {
        public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
        {
            return builder =>
            {
                builder.UseMiddleware<JsonRpcMiddleware>();
                next(builder);
            };
        }
    }
}