# Server/Versioning

Versioning (library [API Versioning](https://github.com/dotnet/aspnet-api-versioning)) is enabled by default with default API version = `1.0`.
But if you don't use `[ApiVersion]` attributes and don't add API version parameter in route (see [Routing#Route templates](routing#route-templates)), then both JSON-RPC and REST controllers will work even without sending version.

## API version usage example

> `Program.cs`
```cs
builder.Services.AddJsonRpcServer(static options => options.RoutePrefix = "/api/v{version:apiVersion}/jsonrpc");
```
> `Controllers.cs`
```cs
[ApiVersion(1)]
[ApiVersion(2)]
public class VersionedController : JsonRpcControllerBase
{
    public async Task<IActionResult> Do1()
    {
        // ...
    };
}

public class UnversionedController : JsonRpcControllerBase
{
    public async Task<IActionResult> Do2()
    {
        // ...
    };
}
```
Method `VersionedController.Do1` will be available by routes:
 - `/api/v1/jsonrpc`
 - `/api/v2/jsonrpc`

But method `UnversionedController.Do2` will be available only by route = `/api/v1/jsonrpc`

## Autodocs

See [Autodocs](autodocs)

If you use multiple API versions, then OpenAPI and OpenRPC documents (including separate documents for registered in DI `IJsonSerializerOptionsProvider` implementations) will be divided according to versions.
All documents will be available in Swagger UI.

If you use `[ApiVersion(1)]`, `[ApiVersion(2)]` and `[ApiVersion("3-str")]` in your project, then you will get such documents:
 - OpenAPI (Swagger):
   - `/swagger/jsonrpc_v1/swagger.json`
   - `/swagger/jsonrpc_v2/swagger.json`
   - `/swagger/jsonrpc_v3-str/swagger.json`
 - OpenRPC:
   - `/openrpc/jsonrpc_v1.json`
   - `/openrpc/jsonrpc_v2.json`
   - `/openrpc/jsonrpc_v3-str.json`