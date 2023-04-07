using Tochka.JsonRpc.Server;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddJsonRpcServer();

var app = builder.Build();

app.UseJsonRpc();
app.UseRouting();
app.UseEndpoints(c => c.MapControllers());

await app.RunAsync();
