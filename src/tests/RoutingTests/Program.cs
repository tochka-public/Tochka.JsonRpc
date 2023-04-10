using Tochka.JsonRpc.Server;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddJsonRpcServer();

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
