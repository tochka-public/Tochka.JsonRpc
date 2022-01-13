using Tochka.JsonRpc.Client;
using Tochka.JsonRpc.Common.Serializers;
using WebAppWithJsonRpcClient.RandomOrg;

var builder = WebApplication
    .CreateBuilder(args);

var a = await Task.FromResult(1);

builder.Services.Configure<RandomOrgOptions>(config =>
{
    config.Token = "";
    config.Url = "https://api.random.org/json-rpc/1/invoke";
});
builder.Services.AddJsonRpcClient<IRandomOrgClient, RandomOrgClient>();
builder.Services.AddSingleton<IJsonRpcSerializer, CamelCaseJsonRpcSerializer>();
var app = builder.Build();

app.MapGet("/", async (IRandomOrgClient randomClient) =>
{
    var randomResponse = await randomClient.GetRandomInts(10, 1, 10000, true, CancellationToken.None);
    if (randomResponse?.Random?.Data == null)
    {
        return "null";
    }

    return $@"Completion time = {randomResponse.Random.CompletionTime}
Random ints = {string.Join(", ", randomResponse.Random.Data)}";
});

app.Run();