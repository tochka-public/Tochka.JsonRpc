using System.Reflection;
using Microsoft.OpenApi.Models;
using Tochka.JsonRpc.Server.Extensions;
using Tochka.JsonRpc.Server.Serialization;
using Tochka.JsonRpc.Server.Settings;
using Tochka.JsonRpc.Swagger;
using Tochka.JsonRpc.Tests.WebApplication;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddJsonRpcServer(static options => options.DefaultMethodStyle = JsonRpcMethodStyle.ActionOnly);
builder.Services.AddScoped<IResponseProvider, SimpleResponseProvider>();
builder.Services.AddScoped<IRequestValidator, SimpleRequestValidator>();
builder.Services.AddSingleton<IJsonSerializerOptionsProvider, SnakeCaseJsonSerializerOptionsProvider>();
builder.Services.AddSingleton<IJsonSerializerOptionsProvider, CamelCaseJsonSerializerOptionsProvider>();
builder.Services.AddSingleton<IJsonSerializerOptionsProvider, KebabCaseUpperJsonSerializerOptionsProvider>();

builder.Services.AddSwaggerWithJsonRpc(Assembly.GetExecutingAssembly());
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("rest", new OpenApiInfo { Title = "RESTful API", Version = "v1" });
});

var app = builder.Build();

app.UseSwaggerUI(c =>
{
    c.JsonRpcSwaggerEndpoints(app);
    c.SwaggerEndpoint("/swagger/rest/swagger.json", "RESTful");
});
app.UseJsonRpc();
app.UseRouting();
app.UseEndpoints(c =>
{
    c.MapControllers();
    c.MapSwagger();
});

await app.RunAsync();
