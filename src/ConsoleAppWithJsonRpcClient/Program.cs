using ConsoleAppWithJsonRpcClient;
using ConsoleAppWithJsonRpcClient.RandomOrg;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Tochka.JsonRpc.Client;
using Tochka.JsonRpc.Common.Serializers;

Host.CreateDefaultBuilder(args).ConfigureServices((hostContext, services) =>
{
    services.Configure<RandomOrgOptions>(config =>
    {
        config.Token = "";
        config.Url = "https://api.random.org/json-rpc/1/invoke";
    });
    services.AddJsonRpcClient<IRandomOrgClient, RandomOrgClient>();
    services.AddSingleton<IJsonRpcSerializer, CamelCaseJsonRpcSerializer>();
    services.AddHostedService<JsonRpcClientHostedService>();
}).Build().Run();