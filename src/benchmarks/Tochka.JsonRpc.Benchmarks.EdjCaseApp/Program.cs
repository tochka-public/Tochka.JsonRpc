var builder = WebApplication.CreateBuilder(args);

builder.Services.AddJsonRpc();

var app = builder.Build();

app.UseJsonRpc();

app.Run();
