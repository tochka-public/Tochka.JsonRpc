using Tochka.JsonRpc.Client.Tests.WebApplication;
using Tochka.JsonRpc.Server;
using Tochka.JsonRpc.Server.Extensions;
using Tochka.JsonRpc.Server.Settings;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddJsonRpcServer(static options => options.DefaultMethodStyle = JsonRpcMethodStyle.ActionOnly);
builder.Services.AddScoped<IResponseProvider, SimpleResponseProvider>();
builder.Services.AddScoped<IRequestValidator, SimpleRequestValidator>();

var app = builder.Build();

app.UseMiddleware<JsonRpcMiddleware>();
app.UseRouting();
app.UseEndpoints(c => c.MapControllers());

await app.RunAsync();
