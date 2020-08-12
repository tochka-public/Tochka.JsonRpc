# Server/Quickstart

## Installation

Install nuget package `Tochka.JsonRpc.Server`.

Register it in `Startup.cs` and set compatibility version. Note that `.AddJsonRpcServer()` is an extension of `IMvcBuilder`, not `IServiceCollection`.

```cs
public void ConfigureServices(IServiceCollection services)
{
	services.AddMvc()
		.AddJsonRpcServer()  // <-- add this
		.SetCompatibilityVersion(CompatibilityVersion.Version_2_2);  // <-- this is required because 2.1 disables endpoint routing
}

public void Configure(IApplicationBuilder app)
{
	app.UseMvc();  // <-- this is required obviously because we work on top of MVC
}
```

Write your API controller as usual, but instead of inheriting from `Controller`, inherit from `JsonRpcController`. To make it work, you don't need any attributes, special naming or constructors.

```cs
public class EchoController : JsonRpcController
{
	public string ToLower(string value)
	{
		return value.ToLower();
	}
}
```

## Make a request

Start your app and send POST (extra headers omitted)

<table>
    <tr>
        <td>
            Request
        </td>
        <td>
            Response
        </td>
    </tr>
<tr>
<td valign="top">

```http
POST /api/jsonrpc HTTP/1.1
Content-Type: application/json
```
```json
{
    "id": 1,
    "jsonrpc": "2.0",
    "method": "echo.to_lower",
    "params": {
        "value": "TEST"
    }
}
```

</td>
<td valign="top">

```HTTP
HTTP/1.1 200 OK
Content-Type: application/json; charset=utf-8
```
```json
{
    "id": 1,
    "jsonrpc": "2.0",
    "result": "test"
}
```

</td>
</tr>
</table>

That's it! Write more controllers, methods, send batches, add middlewares, filters and attributes like normal.
Check out other pages for more advanced usage:

- [Examples](/docs/en/server/examples.md)
- [Configuration](/docs/en/server/configuration.md)
- [Features](/docs/en/server/features.md)
