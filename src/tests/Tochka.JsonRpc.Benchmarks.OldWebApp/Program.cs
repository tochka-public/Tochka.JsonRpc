using Tochka.JsonRpc.V1.Server;
using Tochka.JsonRpc.V1.Server.Pipeline;
using Tochka.JsonRpc.V1.Server.Settings;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers().AddJsonRpcServer(static options => options.DefaultMethodOptions.MethodStyle = MethodStyle.ActionOnly);

var app = builder.Build();

app.UseMiddleware<JsonRpcMiddleware>();
app.UseRouting();
app.UseEndpoints(c =>
{
    c.MapControllers();
});

await app.RunAsync();
