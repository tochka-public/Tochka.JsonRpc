using Tochka.JsonRpc.Server.Extensions;
using Tochka.JsonRpc.Server.Settings;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddJsonRpcServer(static options => options.DefaultMethodStyle = JsonRpcMethodStyle.ActionOnly);

var app = builder.Build();

app.UseJsonRpc();
app.UseRouting();
app.UseEndpoints(c =>
{
    c.MapControllers();
});

await app.RunAsync();
