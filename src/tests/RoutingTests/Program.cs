using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Tochka.JsonRpc.Server;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.Configure<MvcOptions>(c => c.Conventions.Add(new JsonRpcActionModelConvention()));
builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<MatcherPolicy, JsonRpcMatcherPolicy>());

var app = builder.Build();

app.UseMiddleware<JsonRpcMiddleware>();
app.UseRouting();
app.UseEndpoints(c => c.MapControllers());

await app.RunAsync();
