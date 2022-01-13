using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;

namespace JsonRpcServer;

public class CorsStartupFilter : IStartupFilter
{
    public const string CorsPolicyName = "AllowAll";

    public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next) =>
        builder =>
        {
            _ = builder.UseCors(CorsPolicyName);
            next(builder);
        };
}