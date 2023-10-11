# Server/Examples

Here are examples for different scenarios. Common things like default HTTP headers, creating/building/running application instance, registering/mapping controllers are omitted.

> For details beyond basic usage check [Configuration](configuration) page

## Request, Notification, Batch with default configuration

Examples of basic JSON-RPC calls with default configuration.

<details>
<summary>Expand</summary>

> `Program.cs`
```cs
// ...
builder.Services.AddJsonRpcServer();
// ...
app.UseJsonRpc();
// ...
```

> `EchoController.cs`
```cs
public class EchoController : JsonRpcControllerBase
{
    public async Task<ActionResult<string>> ToLower(string value)
    {
        // ...
        var result = value.ToLowerInvariant();
        return this.Ok(result);
    }
}
```

<table>
<tr>
    <th>
        Request
    </th>
    <th>
        Response
    </th>
</tr>

<tr>

<td valign="top">

JSON-RPC Request
```http
POST /api/jsonrpc HTTP/1.1
Content-Type: application/json; charset=utf-8
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

Normal response
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

<tr>

<td valign="top">

JSON-RPC Notification
```http
POST /api/jsonrpc HTTP/1.1
Content-Type: application/json; charset=utf-8
```
```json
{
    "method": "echo.to_lower",
    "params": {
        "value": "TEST"
    },
    "jsonrpc": "2.0"
}
```

</td>
<td valign="top">

No response content by specification
```http
HTTP/1.1 200 OK
Content-Length: 0
```

</td>
</tr>

<tr>

<td valign="top">

JSON-RPC Batch
```http
POST /api/jsonrpc HTTP/1.1
Content-Type: application/json; charset=utf-8
```
```json
[
    {
        "id": 1,
        "method": "echo.to_lower",
        "params": {
            "value": "REQUEST WITH ID AS NUMBER"
        },
        "jsonrpc": "2.0"
    },
    {
        "id": "abc",
        "method": "echo.to_lower",
        "params": {
            "value": "REQUEST WITH ID AS STRING"
        },
        "jsonrpc": "2.0"
    },
    {
        "id": null,
        "method": "echo.to_lower",
        "params": {
            "value": "REQUEST WITH NULL ID"
        },
        "jsonrpc": "2.0"
    },
    {
        "method": "echo.to_lower",
        "params": {
            "value": "NOTIFICATION, NO RESPONSE EXPECTED"
        },
        "jsonrpc": "2.0"
    }
]
```

</td>
<td valign="top">

Responses for all items, except for notifications
```http
HTTP/1.1 200 OK
Content-Type: application/json; charset=utf-8
```
```json
[
    {
        "id": 1,
        "result": "request with id as number",
        "jsonrpc": "2.0"
    },
    {
        "id": "abc",
        "result": "request with id as string",
        "jsonrpc": "2.0"
    },
    {
        "id": null,
        "result": "request with null id",
        "jsonrpc": "2.0"
    }
]
```

</td>
</tr>


</table>
</details>


## AllowRawResponses

Break protocol a bit and return bytes, HTTP codes, etc. See [Configuration#AllowRawResponses](configuration#AllowRawResponses) for details.
<details>
<summary>Expand</summary>

> `Program.cs`
```cs
// ...
builder.Services.AddJsonRpcServer(static options => options.AllowRawResponses = true);
// ...
app.UseJsonRpc();
// ...
```

> `DataController.cs`
```cs
public class DataController : JsonRpcControllerBase
{
    public async Task<IActionResult> GetBytes(int count)
    {
        // ...
        var bytes = Enumerable.Range(0, count).Select(static x => (byte) x).ToArray();
        return new FileContentResult(bytes, "application/octet-stream");
    }

    public async Task<IActionResult> RedirectTo(string url)
    {
        // ...
        return this.RedirectPermanent(url);
    }
}
```

<table>
<tr>
    <th>
        Request
    </th>
    <th>
        Response
    </th>
</tr>

<tr>

<td valign="top">

GetBytes Request
```http
POST /api/jsonrpc HTTP/1.1
Content-Type: application/json; charset=utf-8
```
```json
{
    "id": 1,
    "method": "data.get_bytes",
    "params": {
        "count": 100
    },
    "jsonrpc": "2.0"
}
```

</td>
<td valign="top">

Unmodified bytes in response
```http
HTTP/1.1 200 OK
Content-Type: application/octet-stream
Content-Length: 100
```
```
�

 !"#$%&'()*+,-./0123456789:;<=>?@ABCDEFGHIJKLMNOPQRSTUVWXYZ[\]^_`abc
```

</td>
</tr>

<tr>

<td valign="top">

RedirectTo Request
```http
POST /api/jsonrpc HTTP/1.1
Content-Type: application/json; charset=utf-8
```
```json
{
    "id": 1,
    "method": "data.redirect_to",
    "params": {
        "url": "https://google.com"
    },
    "jsonrpc": "2.0"
}
```

</td>
<td valign="top">

HTTP Redirect
```http
HTTP/1.1 301 Moved Permanently
Content-Length: 0
Location: https://google.com
```

</td>
</tr>

<tr>

<td valign="top">

JSON-RPC Batch
```http
POST /api/jsonrpc HTTP/1.1
Content-Type: application/json; charset=utf-8
```
```json
[
    {
        "id": 1,
        "method": "data.get_bytes",
        "params": {
            "count": 100
        },
        "jsonrpc": "2.0"
    }
]
```

</td>
<td valign="top">

JSON-RPC Error
```http
HTTP/1.1 200 OK
Content-Type: application/json; charset=utf-8
```
```json
[
    {
        "id": 1,
        "error": {
            "code": -32001,
            "message": "Server error",
            "data": {
                "type": "Tochka.JsonRpc.Server.Exceptions.JsonRpcServerException",
                "message": "Raw responses are not allowed in batch requests",
                "details": null
            }
        },
        "jsonrpc": "2.0"
    }
]
```

</td>
</tr>


</table>
</details>


## DetailedResponseExceptions

Hide or reveal exception information. See [Configuration#DetailedResponseExceptions](configuration#DetailedResponseExceptions) for details.

<details>
<summary>Expand</summary>

> `Program.cs`
```cs
// ...
builder.Services.AddJsonRpcServer(static options => options.DetailedResponseExceptions = /* true or false */);
// ...
app.UseJsonRpc();
// ...
```

> `ErrorController.cs`
```cs
public class ErrorController : JsonRpcControllerBase
{
    public async Task<IActionResult> Fail()
    {
        // ...
        throw new NotImplementedException("exception message");
    }
}
```

<table>
<tr>
    <th>
        Request
    </th>
    <th>
        Response
    </th>
</tr>

<tr>

<td valign="top">

Request
```http
POST /api/jsonrpc HTTP/1.1
Content-Type: application/json; charset=utf-8
```
```json
{
    "id": 1,
    "method": "error.fail",
    "params": null,
    "jsonrpc": "2.0"
}
```

</td>
<td valign="top">

No details when `DetailedResponseExceptions` is **false**
```http
HTTP/1.1 200 OK
Content-Type: application/json; charset=utf-8
```
```json
{
    "id": 1,
    "error": {
        "code": -32000,
        "message": "Server error",
        "data": {
            "type": "System.NotImplementedException",
            "message": "exception message",
            "details": null
        }
    },
    "jsonrpc": "2.0"
}
```

</td>
</tr>

<tr>

<td valign="top">

Request
```http
POST /api/jsonrpc HTTP/1.1
Content-Type: application/json; charset=utf-8
```
```json
{
    "id": 1,
    "method": "error.fail",
    "params": null,
    "jsonrpc": "2.0"
}
```

</td>
<td valign="top">

`exception.ToString()` in details when `DetailedResponseExceptions` is **true**
```http
HTTP/1.1 200 OK
Content-Type: application/json; charset=utf-8
```
```json
{
    "id": 1,
    "error": {
        "code": -32000,
        "message": "Server error",
        "data": {
            "type": "System.NotImplementedException",
            "message": "exception message",
            "details": "System.NotImplementedException: exception message\r\n   at Application.Controllers.ErrorController.Fail() in C:\\Path\\To\\Application\\Controllers\\ErrorController.cs:line 7\r\n   at lambda_method6(Closure , Object , Object[] )\r\n   at Microsoft.AspNetCore.Mvc.Infrastructure.ActionMethodExecutor.SyncObjectResultExecutor.Execute(IActionResultTypeMapper mapper, ObjectMethodExecutor executor, Object controller, Object[] arguments)\r\n   at Microsoft.AspNetCore.Mvc.Infrastructure.ControllerActionInvoker.InvokeActionMethodAsync()\r\n   at Microsoft.AspNetCore.Mvc.Infrastructure.ControllerActionInvoker.Next(State& next, Scope& scope, Object& state, Boolean& isCompleted)\r\n   at Microsoft.AspNetCore.Mvc.Infrastructure.ControllerActionInvoker.InvokeNextActionFilterAsync()\r\n--- End of stack trace from previous location ---\r\n   at Microsoft.AspNetCore.Mvc.Infrastructure.ControllerActionInvoker.Rethrow(ActionExecutedContextSealed context)\r\n   at Microsoft.AspNetCore.Mvc.Infrastructure.ControllerActionInvoker.Next(State& next, Scope& scope, Object& state, Boolean& isCompleted)\r\n   at Microsoft.AspNetCore.Mvc.Infrastructure.ControllerActionInvoker.InvokeInnerFilterAsync()\r\n--- End of stack trace from previous location ---\r\n   at Microsoft.AspNetCore.Mvc.Infrastructure.ResourceInvoker.<InvokeNextExceptionFilterAsync>g__Awaited|26_0(ResourceInvoker invoker, Task lastTask, State next, Scope scope, Object state, Boolean isCompleted)"
        }
    },
    "jsonrpc": "2.0"
}
```

</td>
</tr>


</table>
</details>


## Routes

Override routing with global setting or attribute. See [Routing](routing) for details.

<details>
<summary>Expand</summary>

All JSON-RPC handlers must have same route prefix (`/api/jsonrpc` by default) to distinguish them from REST when you use both APIs in same project. If prefix is not defined explicitly in handler's route, it will be added automatically. For handlers without manually defined route, prefix will be used as full route (without `/controllerName` part).

How to change default route and override it with custom route in controller or action:
> `Program.cs`
```cs
// ...
builder.Services.AddJsonRpcServer(static options => options.RoutePrefix = "/public_api");
// ...
app.UseJsonRpc();
// ...
```

> `UsersController.cs`
```cs
/* [Route] override is also possible here */
public class UsersController : JsonRpcControllerBase
{
    public async Task<ActionResult<List<string>>> GetNames()
    {
        // ...
        return this.Ok(new List<string>() { "Alice", "Bob" });
    }

    [Route("/admin_api")]
    public async Task<ActionResult<Guid>> Create(string name)
    {
        // add user to DB and return ID
        // ...
        return this.Ok(Guid.NewGuid());
    }
}
```

<table>
<tr>
    <th>
        Request
    </th>
    <th>
        Response
    </th>
</tr>

<tr>

<td valign="top">

Request to GetNames at default route
```http
POST /public_api HTTP/1.1
Content-Type: application/json; charset=utf-8
```
```json
{
    "id": 1,
    "method": "users.get_names",
    "params": null,
    "jsonrpc": "2.0"
}
```

</td>
<td valign="top">

Normal response
```http
HTTP/1.1 200 OK
Content-Type: application/json; charset=utf-8
```
```json
{
    "id": 1,
    "result": [
        "Alice",
        "Bob"
    ],
    "jsonrpc": "2.0"
}
```

</td>
</tr>

<tr>

<td valign="top">

Request to Create at overridden route without prefix
```http
POST /admin_api HTTP/1.1
Content-Type: application/json; charset=utf-8
```
```json
{
    "id": 1,
    "method": "users.create",
    "params": {
        "name": "Charlie"
    },
    "jsonrpc": "2.0"
}
```

</td>
<td valign="top">

404 Error response
```http
HTTP/1.1 404 Not Found
Content-Length: 0
```

</td>
</tr>

<tr>

<td valign="top">

Request to Create at overridden route with prefix
```http
POST /public_api/admin_api HTTP/1.1
Content-Type: application/json; charset=utf-8
```
```json
{
    "id": 1,
    "method": "users.create",
    "params": {
        "name": "Charlie"
    },
    "jsonrpc": "2.0"
}
```

</td>
<td valign="top">

Normal response
```http
HTTP/1.1 200 OK
Content-Type: application/json; charset=utf-8
```
```json
{
    "id": 1,
    "result": "82a160a8-ad1d-472f-84d3-569b1514f384",
    "jsonrpc": "2.0"
}
```

</td>
</tr>

</table>
</details>


## Method

Change how `method` property is matched to controllers and actions. Request's `method` property can be sent in different formats depending on global setting: as `controller.action` or as `action`. It's also can be set manually with `JsonRpcMethodAttribute`. See [Serialization#Matching method name to controller/action names](serialization#Matching-method-name-to-controlleraction-names) for details.

<details>
<summary>Expand</summary>


> `Program.cs`
```cs
// ...
builder.Services.AddJsonRpcServer(static options => options.DefaultMethodStyle = /* JsonRpcMethodStyle.ControllerAndAction or JsonRpcMethodStyle.ActionOnly */);
// ...
app.UseJsonRpc();
// ...
```

> `EchoController.cs`
```cs
/* [JsonRpcMethodStyle] override is also possible here */
public class EchoController : JsonRpcControllerBase
{
    /* [JsonRpcMethodStyle] or [JsonRpcMethod] override is also possible here */
    public async Task<ActionResult<string>> ToLower(string value)
    {
        // ...
        var result = value.ToLowerInvariant();
        return this.Ok(result);
    }

    [JsonRpcMethod("to upper")]
    public async Task<ActionResult<string>> ToUpper(string value)
    {
        // ...
        var result = value.ToUpperInvariant();
        return this.Ok(result);
    }
}
```

<table>
<tr>
    <th>
        Request
    </th>
    <th>
        Response
    </th>
</tr>

<tr>

<td valign="top">

Request with method with `controller.action` (`JsonRpcMethodStyle.ControllerAndAction`)
```http
POST /api/jsonrpc HTTP/1.1
Content-Type: application/json; charset=utf-8
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

Response from `EchoController.ToLower`
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

<tr>

<td valign="top">

Request with method with `action` (`JsonRpcMethodStyle.ActionOnly`)
```http
POST /api/jsonrpc HTTP/1.1
Content-Type: application/json; charset=utf-8
```
```json
{
    "id": 1,
    "method": "to_lower",
    "params": {
        "value": "TEST"
    },
    "jsonrpc": "2.0"
}
```

</td>
<td valign="top">

Response from `EchoController.ToLower`
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

<tr>

<td valign="top">

Request with custom method name (set by `JsonRpcMethodAttribute`)
```http
POST /api/jsonrpc HTTP/1.1
Content-Type: application/json; charset=utf-8
```
```json
{
    "id": 1,
    "method": "to upper",
    "params": {
        "value": "test"
    },
    "jsonrpc": "2.0"
}
```

</td>
<td valign="top">

Response from `EchoController.ToUpper`
```http
HTTP/1.1 200 OK
Content-Type: application/json; charset=utf-8
```
```json
{
    "id": 1,
    "result": "TEST",
    "jsonrpc": "2.0"
}
```

</td>
</tr>


</table>
</details>


## Serialization

Change default JSON serialization options or override it for controller/action. See [Serialization](serialization) for details.

<details>
<summary>Expand</summary>

Note how changing serialization affects `params` and `method`.
> `Program.cs`
```cs
// ...

// you can also use predefined options from JsonRpcSerializerOptions class
var jsonSerializerOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
builder.Services.AddJsonRpcServer(options => options.DefaultDataJsonSerializerOptions = jsonSerializerOptions);

// options provider to use in JsonRpcSerializerOptionsAttribute
builder.Services.AddSingleton<IJsonSerializerOptionsProvider, SnakeCaseJsonSerializerOptionsProvider>();
// ...
app.UseJsonRpc();
// ...
```

> `SimpleCalcController.cs`
```cs
/* [JsonRpcSerializerOptions] override is also possible here */
public class SimpleCalcController : JsonRpcControllerBase
{
    public async Task<ActionResult<object>> SubtractIntegers(int firstValue, int secondValue)
    {
        // ...
        return this.Ok(new
        {
            firstValue,
            secondValue,
            firstMinusSecond = firstValue - secondValue
        });
    }

    // IMPORTANT: SnakeCaseJsonSerializerOptionsProvider must be registered in DI as IJsonSerializerOptionsProvider
    [JsonRpcSerializerOptions(typeof(SnakeCaseJsonSerializerOptionsProvider))]
    public async Task<ActionResult<object>> AddIntegers(int firstValue, int secondValue)
    {
        // ...
        return this.Ok(new
        {
            firstValue,
            secondValue,
            firstPlusSecond = firstValue + secondValue
        });
    }
}
```

<table>
<tr>
    <th>
        Request
    </th>
    <th>
        Response
    </th>
</tr>

<tr>

<td valign="top">

Request with camelCase
```http
POST /api/jsonrpc HTTP/1.1
Content-Type: application/json; charset=utf-8
```
```json
{
    "id": 1,
    "method": "simpleCalc.subtractIntegers",
    "params": {
        "firstValue": 42,
        "secondValue": 38
    },
    "jsonrpc": "2.0"
}
```

</td>
<td valign="top">

Response with camelCase
```http
HTTP/1.1 200 OK
Content-Type: application/json; charset=utf-8
```
```json
{
    "id": 1,
    "result": {
        "firstValue": 42,
        "secondValue": 38,
        "firstMinusSecond": 4
    },
    "jsonrpc": "2.0"
}
```

</td>
</tr>

<tr>

<td valign="top">

Request with snake_case
```http
POST /api/jsonrpc HTTP/1.1
Content-Type: application/json; charset=utf-8
```
```json
{
    "id": 1,
    "method": "simple_calc.add_integers",
    "params": {
        "first_value": 42,
        "second_value": 38
    },
    "jsonrpc": "2.0"
}
```

</td>
<td valign="top">

Response with snake_case
```http
HTTP/1.1 200 OK
Content-Type: application/json; charset=utf-8
```
```json
{
    "id": 1,
    "result": {
        "first_value": 42,
        "second_value": 38,
        "first_plus_second": 80
    },
    "jsonrpc": "2.0"
}
```

</td>
</tr>


</table>
</details>


## Binding

Change how `params` are bound to method arguments. See [Binding](binding) for details.

<details>
<summary>Default behavior: params are bound to method arguments. Params can be [] or {} by specification</summary>

<table>
<tr>
    <th>
        Request
    </th>
    <th>
        Action method
    </th>
</tr>

<tr>

<td valign="top">

Request has object with two properties
```http
POST /api/jsonrpc HTTP/1.1
Content-Type: application/json; charset=utf-8
```
```json
{
    "id": 1,
    "method": "foo",
    "params": {
        "bar": 1,
        "baz": "test"
    },
    "jsonrpc": "2.0"
}
```

</td>
<td valign="top">

`params` are bound to method arguments by names
```cs
public async Task<IActionResult> Foo(int bar, string baz)
{
    // bar == 1
    // baz == "test"

    // ...
}
```

</td>
</tr>

<tr>

<td valign="top">

Request has array with two items
```http
POST /api/jsonrpc HTTP/1.1
Content-Type: application/json; charset=utf-8
```
```json
{
    "id": 1,
    "method": "foo",
    "params": [
        1,
        "test"
    ],
    "jsonrpc": "2.0"
}
```

</td>
<td valign="top">

`params` are bound to method arguments by indices
```cs
public async Task<IActionResult> Foo(int bar, string baz)
{
    // bar == 1
    // baz == "test"

    // ...
}
```

</td>
</tr>


</table>
</details>


<details>
<summary>Bind whole params object into one model, eg. when model has lots of properties</summary>

<table>
<tr>
    <th>
        Request
    </th>
    <th>
        Action method
    </th>
</tr>

<tr>

<td valign="top">

Request has object with two properties
```http
POST /api/jsonrpc HTTP/1.1
Content-Type: application/json; charset=utf-8
```
```json
{
    "id": 1,
    "method": "foo",
    "params": {
        "bar": 1,
        "baz": "test"
    },
    "jsonrpc": "2.0"
}
```

</td>
<td valign="top">

`params` are bound to single method argument
```cs
public record Data(int Bar, string Baz);

public async Task<IActionResult> Foo([FromParams(BindingStyle.Object)] Data data)
{
    // data.Bar == 1
    // data.Baz == "test"

    // ...
}
```

</td>
</tr>

<tr>

<td valign="top">

Request has array with two items
```http
POST /api/jsonrpc HTTP/1.1
Content-Type: application/json; charset=utf-8
```
```json
{
    "id": 1,
    "method": "foo",
    "params": [
        1,
        "test"
    ],
    "jsonrpc": "2.0"
}
```

</td>
<td valign="top">

Error because array items can not be bound to object properties
```cs
public record Data(int Bar, string Baz);

public async Task<IActionResult> Foo([FromParams(BindingStyle.Object)] Data data)
{
    // does not work for `params` array
}
```
```json
{
    "id": "123",
    "error": {
        "code": -32602,
        "message": "Invalid params",
        "data": {
            "data": [
                "Error while binding value by JSON key = [params] - Can't bind array to object parameter"
            ]
        }
    },
    "jsonrpc": "2.0"
}
```

</td>
</tr>


</table>

</details>


<details>
<summary>Bind params array into one collection, eg. when request has variable count of parameters</summary>

<table>
<tr>
    <th>
        Request
    </th>
    <th>
        Action method
    </th>
</tr>

<tr>

<td valign="top">

Request has object with two properties
```http
POST /api/jsonrpc HTTP/1.1
Content-Type: application/json; charset=utf-8
```
```json
{
    "id": 1,
    "method": "foo",
    "params": {
        "bar": 1,
        "baz": 2
    },
    "jsonrpc": "2.0"
}
```

</td>
<td valign="top">

Error because object properties can not be bound to array items
```cs
public async Task<IActionResult> Foo([FromParams(BindingStyle.Array)] List<int> data)
{
    // does not work for `params` object
}
```
```json
{
    "id": 1,
    "error": {
        "code": -32602,
        "message": "Invalid params",
        "data": {
            "data": [
                "Error while binding value by JSON key = [params] - Can't bind object to collection parameter"
            ]
        }
    },
    "jsonrpc": "2.0"
}
```

</td>
</tr>

<tr>

<td valign="top">

Request has array with two items
```http
POST /api/jsonrpc HTTP/1.1
Content-Type: application/json; charset=utf-8
```
```json
{
    "id": 1,
    "method": "foo",
    "params": [
        1,
        2
    ],
    "jsonrpc": "2.0"
}
```

</td>
<td valign="top">

Array items are bound to collection
```cs
public async Task<IActionResult> Foo([FromParams(BindingStyle.Array)] List<int> data)
{
    // data[0] == 1
    // data[1] == 2

    // ...
}
```

</td>
</tr>


</table>

</details>


<details>
<summary>Mix different binding sources</summary>

Also try default params, object, dynamic and custom serialization...
```cs
public async Task<IActionResult> Foo1(object bar, dynamic baz, [FromParams(BindingStyle.Object)] Data data, [FromServices] ICustomService service, CancellationToken token)
{
    // bar, baz are bound by default
    // data is bound with specified behavior
    // service and token are bound by framework as usual

    // ...
}

public async Task<IActionResult> Foo2(int? bar, string baz = "default_value")
{
    // Request "params" can have nullable "bar" and omit "baz" property entirely

    // ...
}
```

</details>

## Access extra information

Utility methods to work with JSON-RPC request/response objects. See [Utils](utils) for details.

<details>
<summary>Expand</summary>

Several extension methods to `HttpContext` are added for convenience. Useful for additional custom middlewares and filters.

Get JSON-RPC call object:
```cs
var call = HttpContext.GetJsonRpcCall();

var id = (call as UntypedRequest)?.Id;
var method = call.Method;
var parameters = call.Params
```

Get raw JSON-RPC call as `JsonDocument`:
```cs
var rawCall = HttpContext.GetRawJsonRpcCall();

Console.WriteLine(rawCall.RootElement);
```

Get JSON-RPC response object:
```cs
var call = HttpContext.GetJsonRpcResponse();

var id = (call as UntypedResponse)?.Id;
var result = call.Result
```

Check if this call is part of batch request:
```cs
var isBatch = HttpContext.JsonRpcRequestIsBatch();

if (isBatch)
{
    Console.WriteLine("This call is part of batch request!");
}
```

Manually set response. Warning: may be overwritten later by filters!
```cs
var response = new UntypedResponse(request.Id, result)

HttpContext.SetJsonRpcResponse(response);
```

</details>

## Errors and exceptions

Different ways to return error from method. See [Errors](errors) for details.

<details>
<summary>IJsonRpcErrorFactory methods</summary>

```cs
public class FailController : JsonRpcControllerBase
{
    private readonly IJsonRpcErrorFactory jsonRpcErrorFactory;
    public FailController(IJsonRpcErrorFactory jsonRpcErrorFactory) => this.jsonRpcErrorFactory = jsonRpcErrorFactory;

    public async Task<ActionResult<IError>> PredefinedError()
    {
        // ...
        return this.Ok(jsonRpcErrorFactory.InvalidParams("oops"));
        // or others:
        //return this.Ok(jsonRpcErrorFactory.ParseError("oops"));
        //return this.Ok(jsonRpcErrorFactory.InvalidRequest("oops"));
    }
}
```

Response (does not depend on [`DetailedResponseExceptions`](configuration#DetailedResponseExceptions)):
```json
{
    "id": 1,
    "error": {
        "code": -32602,
        "message": "Invalid params",
        "data": "oops"
    },
    "jsonrpc": "2.0"
}
```

</details>

<details>
<summary>Any exception</summary>

```cs
public class FailController : JsonRpcControllerBase
{
    public async Task<IActionResult> ThrowException()
    {
        // ...
        throw new DivideByZeroException("test");
    }
}
```

Response (depends on [`DetailedResponseExceptions`](configuration#DetailedResponseExceptions)):
<table>
<tr>
    <th>
        DetailedResponseExceptions = false
    </th>
    <th>
        DetailedResponseExceptions = true
    </th>
</tr>

<tr>
<td valign="top">

```json
{
    "id": 1,
    "error": {
        "code": -32000,
        "message": "Server error",
        "data": {
            "type": "System.DivideByZeroException",
            "message": "test",
            "details": null
        }
    },
    "jsonrpc": "2.0"
}
```

</td>
<td valign="top">

```json
{
    "id": 1,
    "error": {
        "code": -32000,
        "message": "Server error",
        "data": {
            "type": "System.DivideByZeroException",
            "message": "test",
            "details": "System.DivideByZeroException: test\r\n   at Application.Controllers.FailController.ThrowException() ... (and the rest of the stack trace) ..."
        }
    },
    "jsonrpc": "2.0"
}
```

</td>
</tr>
</table>

</details>

<details>
<summary>Creating error using factory IJsonRpcErrorFactory.Error</summary>

```cs
public record MyData(int Bar, string Baz);

public class FailController : JsonRpcControllerBase
{
    private readonly IJsonRpcErrorFactory jsonRpcErrorFactory;
    public FailController(IJsonRpcErrorFactory jsonRpcErrorFactory) => this.jsonRpcErrorFactory = jsonRpcErrorFactory;

    public async Task<ActionResult<IError>> Error()
    {
        // ...
        return this.Ok(jsonRpcErrorFactory.Error(123, "error with custom data", new MyData(456, "baz"));
    }
}
```

Response (does not depend on [`DetailedResponseExceptions`](configuration#DetailedResponseExceptions)):
```json
{
    "id": 1,
    "error": {
        "code": 123,
        "message": "error with custom data",
        "data": {
            "bar": 456,
            "baz": "baz"
        }
    },
    "jsonrpc": "2.0"
}
```

</details>

<details>
<summary>ActionResult with HTTP error codes</summary>

```cs
public record MyData(int Bar, string Baz);

public class FailController : JsonRpcControllerBase
{
    public async Task<IActionResult> MvcError()
    {
        // ...
        return this.BadRequest(new MyData(123, "baz"));
    }
}
```

Response (does not depend on [`DetailedResponseExceptions`](configuration#DetailedResponseExceptions)):
```json
{
    "id": 1,
    "error": {
        "code": -32602,
        "message": "Invalid params",
        "data": {
            "bar": 123,
            "baz": "baz"
        }
    },
    "jsonrpc": "2.0"
}
```

</details>

<details>
<summary>Wrapping exception manually IJsonRpcErrorFactory.Exception</summary>

```cs
public class FailController : JsonRpcControllerBase
{
    private readonly IJsonRpcErrorFactory jsonRpcErrorFactory;
    public FailController(IJsonRpcErrorFactory jsonRpcErrorFactory) => this.jsonRpcErrorFactory = jsonRpcErrorFactory;

    public async Task<IActionResult> WrapExceptionManually()
    {
        // ...
        try
        {
            throw new DivideByZeroException("oops");
        }
        catch (Exception e)
        {
            var error = jsonRpcErrorFactory.Exception(e);
            return new ObjectResult(error);
        }

        return this.Ok();
    }
}
```

Response (depends on [`DetailedResponseExceptions`](configuration#DetailedResponseExceptions)):
<table>
<tr>
    <th>
        DetailedResponseExceptions = false
    </th>
    <th>
        DetailedResponseExceptions = true
    </th>
</tr>

<tr>
<td valign="top">

```json
{
    "id": 1,
    "error": {
        "code": -32000,
        "message": "Server error",
        "data": {
            "type": "System.DivideByZeroException",
            "message": "oops",
            "details": null
        }
    },
    "jsonrpc": "2.0"
}
```

</td>
<td valign="top">

```json
{
    "id": 1,
    "error": {
        "code": -32000,
        "message": "Server error",
        "data": {
            "type": "System.DivideByZeroException",
            "message": "oops",
            "details": "System.DivideByZeroException: oops\r\n   at Application.Controllers.FailController.WrapExceptionManually() ... (and the rest of the stack trace) ..."
        }
    },
    "jsonrpc": "2.0"
}
```

</td>
</tr>
</table>

</details>

<details>
<summary>Wrapping HTTP status code manually IJsonRpcErrorFactory.HttpError</summary>

```cs
public class FailController : JsonRpcControllerBase
{
    private readonly IJsonRpcErrorFactory jsonRpcErrorFactory;
    public FailController(IJsonRpcErrorFactory jsonRpcErrorFactory) => this.jsonRpcErrorFactory = jsonRpcErrorFactory;

    public async Task<ActionResult<IError>> WrapHttpErrorManually()
    {
        // ...
        var innerException = new DivideByZeroException("inner!");
        var e = new ArgumentException("message!", innerException);
        return this.Ok(jsonRpcErrorFactory.HttpError(500, e));
    }
}
```

Response (depends on [`DetailedResponseExceptions`](configuration#DetailedResponseExceptions)):
<table>
<tr>
    <th>
        DetailedResponseExceptions = false
    </th>
    <th>
        DetailedResponseExceptions = true
    </th>
</tr>

<tr>
<td valign="top">

```json
{
    "id": 1,
    "error": {
        "code": -32603,
        "message": "Internal error",
        "data": {
            "type": "System.ArgumentException",
            "message": "message!",
            "details": null
        }
    },
    "jsonrpc": "2.0"
}
```

</td>
<td valign="top">

```json
{
    "id": 1,
    "error": {
        "code": -32603,
        "message": "Internal error",
        "data": {
            "type": "System.ArgumentException",
            "message": "message!",
            "details": "System.ArgumentException: message!\r\n ---> System.DivideByZeroException: inner!\r\n   --- End of inner exception stack trace ---"
        }
    },
    "jsonrpc": "2.0"
}
```

</td>
</tr>
</table>

</details>

<details>
<summary>Creating error manually</summary>

```cs
public record MyData(int Bar, string Baz);

public class FailController : JsonRpcControllerBase
{
    private readonly IJsonRpcErrorFactory jsonRpcErrorFactory;
    public FailController(IJsonRpcErrorFactory jsonRpcErrorFactory) => this.jsonRpcErrorFactory = jsonRpcErrorFactory;

    public async Task<ActionResult<IError>> ManuallyCreateError()
    {
        // ...
        var error = new Error<MyData>(123, "error with custom data", new MyData(456, "baz"));
        return this.Ok(error);
    }
}
```

Response (does not depend on [`DetailedResponseExceptions`](configuration#DetailedResponseExceptions)):
```json
{
    "id": 1,
    "error": {
        "code": 123,
        "message": "error with custom data",
        "data": {
            "bar": 456,
            "baz": "baz"
        }
    },
    "jsonrpc": "2.0"
}
```

</details>

<details>
<summary>Throwing exception with error using throw and method IError.AsException</summary>

```cs
public record MyData(int Bar, string Baz);

public class FailController : JsonRpcControllerBase
{
    private readonly IJsonRpcErrorFactory jsonRpcErrorFactory;
    public FailController(IJsonRpcErrorFactory jsonRpcErrorFactory) => this.jsonRpcErrorFactory = jsonRpcErrorFactory;

    public async Task<IActionResult> ThrowErrorAsException()
    {
        // ...
        var error = jsonRpcErrorFactory.Error(123, "error with custom data", new MyData(456, "baz"));
        throw error.AsException();
    }
}
```

Response (does not depend on [`DetailedResponseExceptions`](configuration#DetailedResponseExceptions)):
```json
{
    "id": 1,
    "error": {
        "code": 123,
        "message": "error with custom data",
        "data": {
            "bar": 456,
            "baz": "baz"
        }
    },
    "jsonrpc": "2.0"
}
```

</details>

<details>
<summary>Throwing exception with error from method IError.ThrowAsException</summary>

```cs
public record MyData(int Bar, string Baz);

public class FailController : JsonRpcControllerBase
{
    private readonly IJsonRpcErrorFactory jsonRpcErrorFactory;
    public FailController(IJsonRpcErrorFactory jsonRpcErrorFactory) => this.jsonRpcErrorFactory = jsonRpcErrorFactory;

    public async Task<IActionResult> ThrowErrorAsException()
    {
        // ...
        var error = jsonRpcErrorFactory.Error(123, "error with custom data", new MyData(456, "baz"));
        error.ThrowAsException();
    }
}
```

Response (does not depend on [`DetailedResponseExceptions`](configuration#DetailedResponseExceptions)):
```json
{
    "id": 1,
    "error": {
        "code": 123,
        "message": "error with custom data",
        "data": {
            "bar": 456,
            "baz": "baz"
        }
    },
    "jsonrpc": "2.0"
}
```

</details>

<details>
<summary>Throwing exception with error manually</summary>

```cs
public record MyData(int Bar, string Baz);

public class FailController : JsonRpcControllerBase
{
    private readonly IJsonRpcErrorFactory jsonRpcErrorFactory;
    public FailController(IJsonRpcErrorFactory jsonRpcErrorFactory) => this.jsonRpcErrorFactory = jsonRpcErrorFactory;

    public async Task<IActionResult> ThrowExceptionWithError()
    {
        // ...
        var error = jsonRpcErrorFactory.Error(123, "error with custom data", new MyData(456, "baz"));
        throw new JsonRpcErrorException(error);
    }
}
```

Response (does not depend on [`DetailedResponseExceptions`](configuration#DetailedResponseExceptions)):
```json
{
    "id": 1,
    "error": {
        "code": 123,
        "message": "error with custom data",
        "data": {
            "bar": 456,
            "baz": "baz"
        }
    },
    "jsonrpc": "2.0"
}
```

</details>

## Requests logging

<details>
<summary>Log request</summary>

```cs
app.UseJsonRpc().WithJsonRpcRequestLogging()
```

</details>
