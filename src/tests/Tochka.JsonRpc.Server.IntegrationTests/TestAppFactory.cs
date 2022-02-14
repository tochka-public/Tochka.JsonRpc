using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Tochka.JsonRpc.Server.IntegrationTests
{
    public class TestAppFactory : WebApplicationFactory<Startup>
    {
        protected override IWebHostBuilder CreateWebHostBuilder()
        {
            return WebHost.CreateDefaultBuilder()
                .ConfigureServices(services =>
                {
                    services.AddLogging(logBuilder =>
                    {
                        logBuilder.ClearProviders();
                        logBuilder.AddConsole();
                        logBuilder.SetMinimumLevel(LogLevel.None);
                    });
                })
                .UseStartup<Startup>();
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseContentRoot(".");
            // base.ConfigureWebHost(builder);
        }
    }
}