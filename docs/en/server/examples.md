# Server/Examples

Here are examples for different scenarios. Common things like default HTTP headers, creating/building/running application instance, registering/mapping controllers are omitted.

> For details beyond basic usage check [Configuration](configuration) page

## Request, Notification, Batch with default configuration

Examples of basic JSON-RPC calls with default configuration

<details>
<summary>Expand</summary>

> `Program.cs`
```cs
builder.Services.AddJsonRpcServer();

app.UseJsonRpc();
```

> `EchoController.cs`
```cs
public class EchoController : JsonRpcControllerBase
{
    public string ToLower(string value) => value.ToLowerInvariant();
}
```

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

Break protocol a bit and return bytes, HTTP codes, etc.
<details>
<summary>Expand</summary>

> `Program.cs`
```cs
builder.Services.AddJsonRpcServer(static options => options.AllowRawResponses = true);

app.UseJsonRpc();
```

> `DataController.cs`
```cs
public class DataController : JsonRpcControllerBase
{
    public IActionResult GetBytes(int count)
    {
        var bytes = Enumerable.Range(0, count).Select(static x => (byte) x).ToArray();
        return new FileContentResult(bytes, "application/octet-stream");
    }

    public IActionResult RedirectTo(string url) => RedirectPermanent(url);
}
```

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

Hide or reveal exception information
<details>
<summary>Expand</summary>

> `Program.cs`
```cs
builder.Services.AddJsonRpcServer(static options => options.DetailedResponseExceptions = /* true or false */);

app.UseJsonRpc();
```

> `ErrorController.cs`
```cs
public class ErrorController : JsonRpcControllerBase
{
    public string Fail() => throw new NotImplementedException("exception message");
}
```

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

Override routing with global setting or attribute
<details>
<summary>Expand</summary>

All JSON-RPC handlers must have same route prefix (`/api/jsonrpc` by default) to distinguish them from REST when you use both APIs in same project. If prefix is not defined explicitly in handler's route, it will be added automatically. For handlers without manually defined route, prefix will be used as full route (without `/controllerName` part).

How to change default route and override it with custom route in controller or action:
> `Program.cs`
```cs
builder.Services.AddJsonRpcServer(static options => options.RoutePrefix = "/public_api");

app.UseJsonRpc();
```

> `UsersController.cs`
```cs
/* [Route] override is also possible here */
public class UsersController : JsonRpcControllerBase
{
    public List<string> GetNames() => new() { "Alice", "Bob" };

    [Route("/admin_api")] // add user to DB and return ID
    public Guid Create(string name) => Guid.NewGuid();
}
```

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

Change how `method` property is matched to controllers and actions. Request's `method` property can be sent in different formats depending on global setting: as `controller.action` or as `action`. It's also can be set manually with `JsonRpcMethodAttribute`.

<details>
<summary>Expand</summary>


> `Program.cs`
```cs
builder.Services.AddJsonRpcServer(static options => options.DefaultMethodStyle = /* JsonRpcMethodStyle.ControllerAndAction or JsonRpcMethodStyle.ActionOnly */);

app.UseJsonRpc();
```

> `EchoController.cs`
```cs
/* [JsonRpcMethodStyle] override is also possible here */
public class EchoController : JsonRpcControllerBase
{
    /* [JsonRpcMethodStyle] or [JsonRpcMethod] override is also possible here */
    public string ToLower(string value) => value.ToLowerInvariant();

    [JsonRpcMethod("to upper")]
    public string ToUpper(string value) => value.ToUpperInvariant();
}
```

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
// you can also use predefined options from JsonRpcSerializerOptions class
var jsonSerializerOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
builder.Services.AddJsonRpcServer(options => options.DefaultDataJsonSerializerOptions = jsonSerializerOptions);

// options provider to use in JsonRpcSerializerOptionsAttribute
builder.Services.AddSingleton<IJsonSerializerOptionsProvider, SnakeCaseJsonSerializerOptionsProvider>();

app.UseJsonRpc();
```

> `SimpleCalcController.cs`
```cs
/* [JsonRpcSerializerOptions] override is also possible here */
public class SimpleCalcController : JsonRpcControllerBase
{
    public object SubtractIntegers(int firstValue, int secondValue) => new
    {
        firstValue,
        secondValue,
        firstMinusSecond = firstValue - secondValue
    };

    // IMPORTANT: SnakeCaseJsonSerializerOptionsProvider must be registered in DI as IJsonSerializerOptionsProvider
    [JsonRpcSerializerOptions(typeof(SnakeCaseJsonSerializerOptionsProvider))]
    public object AddIntegers(int firstValue, int secondValue) => new
    {
        firstValue,
        secondValue,
        firstPlusSecond = firstValue + secondValue
    };
}
```

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
    <td>
        Request
    </td>
    <td>
        Action method
    </td>
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
public void Foo(int bar, string baz)
{
    // bar == 1
    // baz == "test"
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
public void Foo(int bar, string baz)
{
    // bar == 1
    // baz == "test"
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
    <td>
        Request
    </td>
    <td>
        Action method
    </td>
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

public void Foo([FromParams(BindingStyle.Object)] Data data)
{
    // data.Bar == 1
    // data.Baz == "test"
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

public void Foo([FromParams(BindingStyle.Object)] Data data)
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
    <td>
        Request
    </td>
    <td>
        Action method
    </td>
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
public void Foo([FromParams(BindingStyle.Array)] List<int> data)
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
public void Foo([FromParams(BindingStyle.Array)] List<int> data)
{
    // data[0] == 1
    // data[1] == 2
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
public void Foo1(object bar, dynamic baz, [FromParams(BindingStyle.Object)] Data data, [FromServices] ICustomService service, CancellationToken token)
{
    // bar, baz are bound by default
    // data is bound with specified behavior
    // service and token are bound by framework as usual
}

public void Foo2(int? bar, string baz = "default_value")
{
    // Request "params" can have nullable "bar" and omit "baz" property entirely
}
```

</details>

## Access extra information

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

See [Errors](errors) first.

<details>
<summary>Different ways to return an error from Action</summary>

Consider actions in this controller. Below are examples of their output. HTTP headers are omitted, response is always `200 OK`.

```cs
public record MyData(int Bar, string Baz);

public class FailController : JsonRpcControllerBase
{
    private readonly IJsonRpcErrorFactory jsonRpcErrorFactory;
    public FailController(IJsonRpcErrorFactory jsonRpcErrorFactory) => this.jsonRpcErrorFactory = jsonRpcErrorFactory;
    // see methods in examples below
}
```

<table>
<tr>
    <td>
        Action
    </td>
    <td>
        Response without DetailedResponseExceptions
    </td>
    <td>
        Response with DetailedResponseExceptions
    </td>
</tr>

<tr>

<td valign="top">

```cs
public void ThrowException() =>
    throw new DivideByZeroException("test");
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

<tr>

<td valign="top">

```cs
public IError Error() =>
    jsonRpcErrorFactory.Error(123,
        "error with custom data",
        new MyData(456, "baz"));
```

</td>

<td valign="top">

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

</td>

<td valign="top">

no difference

</td>
</tr>

<tr>

<td valign="top">

```cs
public IError PredefinedError()
{
    return jsonRpcErrorFactory.InvalidParams("oops");
    // or others:
    //return jsonRpcErrorFactory.ParseError("oops");
    //return jsonRpcErrorFactory.InvalidRequest("oops");
}
```

</td>

<td valign="top">

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

</td>

<td valign="top">

no difference

</td>
</tr>

<tr>

<td valign="top">

```cs
public IActionResult MvcError() =>
    this.BadRequest(new MyData(123, "baz"));
```

</td>

<td valign="top">

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

</td>

<td valign="top">

no difference

</td>
</tr>

<tr>

<td valign="top">

```cs
public IActionResult WrapExceptionManually()
{
    try
    {
        throw new DivideByZeroException("oops");
    }
    catch (Exception e)
    {
        var error = jsonRpcErrorFactory.Exception(e);
        return new ObjectResult(error);
    }

    return Ok();
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

<tr>

<td valign="top">

```cs
public IError WrapHttpErrorManually()
{
    var innerException = new DivideByZeroException("inner!");
    var e = new ArgumentException("message!", innerException);
    return jsonRpcErrorFactory.HttpError(500, e);
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

<tr>

<td valign="top">

```cs
public IError ManuallyCreateError() =>
    new Error<MyData>(123,
        "error with custom data",
        new MyData(456, "baz"));
```

</td>

<td valign="top">

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

</td>

<td valign="top">

no difference

</td>
</tr>

<tr>

<td valign="top">

```cs
public void ThrowErrorAsException()
{
    var error = jsonRpcErrorFactory.Error(123,
        "error with custom data",
        new MyData(456, "baz"));
    error.ThrowAsException();
}
```

</td>

<td valign="top">

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

</td>

<td valign="top">

no difference

</td>
</tr>


</table>

</details>
