using Tochka.JsonRpc.Common;
using Tochka.JsonRpc.Server;
using Tochka.JsonRpc.Server.Pipeline;
using WebAppWIthJsonRpcRouter;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers(options => options.EnableActionInvokers = true)
    .AddNewtonsoftJson()
    .AddJsonRpcServer()
    ;

// builder.Services.TryAddConvention<ControllerConvention>();

var app = builder.Build();

app.UseMiddleware<JsonRpcMiddleware>();
app.UseRouting(); // используем систему маршрутизации
app.UseEndpoints(endpoints => { endpoints.MapControllers(); });

app.Run();