using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;

namespace Tochka.JsonRpc.OpenRpc
{
    /// <summary>
    /// Registers OpenRpc middleware at the beginning of the pipeline
    /// </summary>
    public class OpenRpcStartupFilter : IStartupFilter
    {
        public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
        {
            return builder =>
            {
                builder.UseMiddleware<OpenRpcMiddleware>();
                next(builder);
            };
        }
    }
}