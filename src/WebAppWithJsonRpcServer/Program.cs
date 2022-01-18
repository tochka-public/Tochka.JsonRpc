using Tochka.JsonRpc.Server;
using Tochka.JsonRpc.Server.Pipeline;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers(options =>
        options.EnableEndpointRouting = false)
    .AddNewtonsoftJson()
    .AddJsonRpcServer();

var app = builder.Build();

app.UseMiddleware<JsonRpcMiddleware>();
app.UseMvc();

app.Run();