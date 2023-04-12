using Microsoft.AspNetCore.Mvc;
using RoutingTests;
using Tochka.JsonRpc.Common;
using Tochka.JsonRpc.Server;
using Tochka.JsonRpc.Server.Extensions;
using Tochka.JsonRpc.Server.Serialization;
using Tochka.JsonRpc.Server.Settings;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddJsonRpcServer(static options =>
{
    options.DefaultMethodStyle = JsonRpcMethodStyle.ActionOnly;
    options.DefaultDataJsonSerializerOptions = JsonRpcSerializerOptions.CamelCase;
    options.DetailedResponseExceptions = true;
    options.AllowRawResponses = true;
});
// builder.Services.Configure<MvcOptions>(static options =>
// {
//     options.Filters.Add<CustomActionFilter>();
// });
builder.Services.AddSingleton<IJsonSerializerOptionsProvider, SnakeCaseJsonSerializerOptionsProvider>();

var app = builder.Build();

app.UseJsonRpc();
app.UseRouting();
app.UseEndpoints(c => c.MapControllers());

var dataSources = app.Services.GetServices<EndpointDataSource>();
foreach (var endpoint in dataSources.SelectMany(static dataSource => dataSource.Endpoints))
{
    Console.WriteLine($"{endpoint.DisplayName} {(endpoint as RouteEndpoint)?.RoutePattern.RawText}");
}

await app.RunAsync();
