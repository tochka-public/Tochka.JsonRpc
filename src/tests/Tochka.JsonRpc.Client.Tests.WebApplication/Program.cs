using Tochka.JsonRpc.Client.Tests.WebApplication;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddScoped<IResponseProvider, SimpleResponseProvider>();
builder.Services.AddScoped<IRequestValidator, SimpleRequestValidator>();

var app = builder.Build();

app.UseRouting();
app.UseEndpoints(c => c.MapControllers());

await app.RunAsync();
