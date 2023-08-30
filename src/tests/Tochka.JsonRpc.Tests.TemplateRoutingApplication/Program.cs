using System.Reflection;
using Microsoft.OpenApi.Models;
using Tochka.JsonRpc.OpenRpc;
using Tochka.JsonRpc.Server.Extensions;
using Tochka.JsonRpc.Server.Settings;
using Tochka.JsonRpc.Swagger;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddJsonRpcServer(static options =>
{
    options.DefaultMethodStyle = JsonRpcMethodStyle.ActionOnly;
    options.RoutePrefix = "/api/v{version:apiVersion}/jsonrpc/[controller]";
});
builder.Services.AddSwaggerWithJsonRpc(Assembly.GetExecutingAssembly());
builder.Services.AddSwaggerGen(static c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "RESTful API", Version = "v1" });
});
builder.Services.AddOpenRpc(Assembly.GetExecutingAssembly());

var app = builder.Build();

app.UseSwaggerUI(c =>
{
    c.JsonRpcSwaggerEndpoints(app.Services);
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "RESTful");
});
app.UseJsonRpc();
app.UseRouting();
app.UseEndpoints(static c =>
{
    c.MapControllers();
    c.MapSwagger();
    c.MapOpenRpc();
});

await app.RunAsync();
