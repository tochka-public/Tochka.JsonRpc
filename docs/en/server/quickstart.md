# Server/Quickstart

## Installation

Install nuget package `Tochka.JsonRpc.Server`.

Register standard ASP.NET Core Controllers (`AddControllers()`) and this library services (`AddJsonRpcServer()`) in `Program.cs`. Add middleware (`UseJsonRpc()`) as early as possible. Add Endpoint routing to controllers (`app.UseRouting()` and `app.UseEndpoints(static c => c.MapControllers())`)

```cs
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers(); // <-- Register controllers (same as for REST)
builder.Services.AddJsonRpcServer(); // <-- Register this library services

var app = builder.Build();

app.UseJsonRpc(); // <-- Add middleware
app.UseRouting(); // <-- Add routing (same as for REST)
app.UseEndpoints(static c => c.MapControllers()) // <-- Add endpoints (same as for REST)

await app.RunAsync();
```

Write your API controller as usual, but instead of inheriting from `ControllerBase`, inherit from `JsonRpcControllerBase`. To make it work, you don't need any attributes, special naming or constructors.

```cs
public class EchoController : JsonRpcControllerBase
{
    public string ToLower(string value) => value.ToLowerInvariant();
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
    "method": "echo.to_lower",
    "params": {
        "value": "TEST"
    },
    "jsonrpc": "2.0"
}
```

</td>
<td valign="top">

```http
HTTP/1.1 200 OK
Content-Type: application/json; charset=utf-8
```
```json
{
    "id": 1,
    "result": "test",
    "jsonrpc": "2.0"
}
```

</td>
</tr>
</table>

That's it! Write more controllers, methods, send batches, add middlewares, filters and attributes like normal.
Check out other pages for more advanced usage:

- [Examples](examples)
- [Configuration](configuration)
