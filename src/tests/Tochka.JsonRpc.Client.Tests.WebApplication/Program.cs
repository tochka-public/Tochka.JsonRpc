using Tochka.JsonRpc.Client.Tests.WebApplication;
using Tochka.JsonRpc.V1.Common.Serializers;
using Tochka.JsonRpc.V1.Server;
using Tochka.JsonRpc.V1.Server.Pipeline;
using Tochka.JsonRpc.V1.Server.Settings;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddJsonRpcServer(static options => options.DefaultMethodOptions.MethodStyle = MethodStyle.ActionOnly);
builder.Services.TryAddJsonRpcSerializer<CamelCaseJsonRpcSerializer>();
builder.Services.AddScoped<IResponseProvider, SimpleResponseProvider>();
builder.Services.AddScoped<IRequestValidator, SimpleRequestValidator>();

var app = builder.Build();

app.UseMiddleware<JsonRpcMiddleware>();
app.UseRouting();
app.UseEndpoints(c => c.MapControllers());

await app.RunAsync();
