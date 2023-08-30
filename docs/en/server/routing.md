# Server/Routing

All JSON-RPC handlers must have same route prefix (`/api/jsonrpc` by default) to distinguish them from REST when you use both APIs in same project. If prefix is not defined explicitly in handler's route, it will be added automatically. For handlers without manually defined route, prefix will be used as full route (without `/controllerName` part).

## Overriding global prefix

See [Configuration](configuration#RoutePrefix) and [Examples](examples#routes)

```cs
builder.Services.AddJsonRpcServer(static options => options.RoutePrefix = "/api");
```

Prefix `/api` will be added to all project endpoints if it wasn't explicitly defined in attribute `RouteAttribute`.

<table>
<tr>
    <th>
        RouteAttribute
    </th>
    <th>
        Final route for requests
    </th>
</tr>

<tr>
    <td>
        Not used
    </td>
    <td>
        `/api`
    </td>
</tr>

<tr>
    <td>
        `[Route("action")]` or `[Route("/action")]`
    </td>
    <td>
        `/api/action`
    </td>
</tr>

<tr>
    <td>
        `[Route("api/action")]` or `[Route("/api/action")]`
    </td>
    <td>
        `/api/action`
    </td>
</tr>
</table>


## Overriding route for method/controller

See [Configuration](configuration#RoutePrefix) and [Examples](examples#routes)

You can explicitly define route for controller or method by using `RouteAttribute`. But if this route doesn't contain global `RoutePrefix`, then this explicitly defined route will be added added to prefix, but won't override it. This allows to distinguish between JSON-RPC and REST requests.

If `RoutePrefix` from global settings has default value = `/api/jsonrpc/` (see [Overriding global prefix](#Overriding-global-prefix)) and defined controller with such code:
```cs
[Route("users")]
public class UsersController : JsonRpcControllerBase
{
    [Route("names")]
    public List<string> GetNames() => new() { "Alice", "Bob" };
}
```
Then method `GetNames` will be available by route = `/api/jsonrpc/users/names`

## Route templates

See [Configuration](configuration#RoutePrefix)

Both `RoutePrefix` from global settings and `RouteAttribute` support route templates. [Tokens defined by ASP.NET](https://learn.microsoft.com/en-us/aspnet/core/mvc/controllers/routing?view=aspnetcore-6.0#token-replacement-in-route-templates-controller-action-area): `[area]`, `[controller]` and `[action]` are supported by default, as well as parameter with API version `{version:apiVersion}` (`1.0` by default) from [API Versioning](https://github.com/dotnet/aspnet-api-versioning) library (see [Versioning](versioning))

If RoutePrefix from global settings defined with such code:
```cs
builder.Services.AddJsonRpcServer(static options => options.RoutePrefix = "/api/v{version:apiVersion}/jsonrpc/[controller]");
```
Then methods of controller with name `SomethingController` will be available by route = `/api/v1/jsonrpc/something`

> You need to use [ASP.NET tokens](https://learn.microsoft.com/en-us/aspnet/core/mvc/controllers/routing?view=aspnetcore-6.0#token-replacement-in-route-templates-controller-action-area) in square brackets (`[ ]`), but all the others parameters, including [API version](https://github.com/dotnet/aspnet-api-versioning) - in curly brackets (`{ }`), for correct routing and autodocs generation