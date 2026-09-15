using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OpenApi;
using Tochka.JsonRpc.ApiExplorer;
using Tochka.JsonRpc.Server.Extensions;
using Tochka.JsonRpc.Server.Settings;
using Tochka.JsonRpc.Tests.WebApplication;
using Tochka.JsonRpc.Tests.WebApplication.Auth;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers(static options =>
    {
        options.Filters.Add<BusinessLogicExceptionWrappingFilter>();
        options.Filters.Add<BusinessLogicExceptionHandlingFilter>();
    })
    .ConfigureApiBehaviorOptions(static o =>
    {
        o.SuppressModelStateInvalidFilter = true; // controllers with [ApiController] wrap errors with RFC 7807 ProblemDetails, we skip that for tests
    })
    ;

static void JsonSetup(JsonSerializerOptions o)
{
    var naming = JsonNamingPolicy.SnakeCaseLower;
    o.PropertyNamingPolicy = naming;
    o.Converters.Add(new JsonStringEnumConverter(naming));
}

builder.Services.Configure<JsonOptions>(static o => JsonSetup(o.JsonSerializerOptions));
builder.Services.Configure<Microsoft.AspNetCore.Http.Json.JsonOptions>(static o => JsonSetup(o.SerializerOptions));

builder.Services.AddJsonRpcServer(static options => options.DefaultMethodStyle = JsonRpcMethodStyle.ActionOnly);
builder.Services.AddJsonRpcOpenApiIntegration();
builder.Services.AddOpenApi(static o =>
{
    o.CreateSchemaReferenceId = static typeInfo =>
    {
        // Schemas are invalid by design, need a fix https://github.com/dotnet/aspnetcore/issues/64325
        var id = OpenApiOptions.CreateDefaultSchemaReferenceId(typeInfo);
        return id switch
        {
            null => null,
            not null when typeInfo.Type.Namespace is not null => $"{typeInfo.Type.Namespace}.{id}",
            not null => id
        };
    };
});

// "business logic"
builder.Services.AddScoped<IResponseProvider, SimpleResponseProvider>();
builder.Services.AddScoped<IRequestValidator, SimpleRequestValidator>();
builder.Services.AddScoped<IBusinessLogicExceptionHandler, BusinessLogicExceptionHandler>();

// auth
builder.Services.AddAuthentication(AuthConstants.SchemeName)
    .AddScheme<ApiAuthenticationOptions, ApiAuthenticationHandler>(AuthConstants.SchemeName, null);
builder.Services.AddAuthorization();

// FluentValidation
builder.Services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
builder.Services.AddFluentValidationAutoValidation();

var app = builder.Build();

app.MapOpenApi().AllowAnonymous();
app.UseSwaggerUI();
app.UseAuthentication();
app.UseJsonRpc();
app.UseRouting();
app.UseAuthorization();
app.MapControllers();

await app.RunAsync();
