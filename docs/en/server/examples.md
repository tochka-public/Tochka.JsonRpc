# Server/Examples

Here are examples for different scenarios. Common things like default HTTP headers, calls to `AddMvc().SetCompatibilityVersion()` are omitted.

## Request, Notification, Batch with default configuration
<details>
<summary>Expand</summary>

> `Startup.cs`
```cs
.AddJsonRpcServer()
```

> `EchoController.cs`
```cs
public class EchoController : JsonRpcController
{
    public string ToLower(string value)
    {
        return value.ToLower();
    }
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

JSON Rpc Request
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

Normal response
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

<tr>

<td valign="top">

JSON Rpc Notification
```http
POST /api/jsonrpc HTTP/1.1
Content-Type: application/json
```
```json
{
    "jsonrpc": "2.0",
    "method": "echo.to_lower",
    "params": {
        "value": "TEST"
    }
}
```

</td>
<td valign="top">

No response content by specification
```HTTP
HTTP/1.1 200 OK
Content-Length: 0
```

</td>
</tr>

<tr>

<td valign="top">

JSON Rpc Batch
```http
POST /api/jsonrpc HTTP/1.1
Content-Type: application/json
```
```json
[
    {
        "id": 1,
        "jsonrpc": "2.0",
        "method": "echo.to_lower",
        "params": {
            "value": "REQUEST WITH ID AS NUMBER"
        }
    },
    {
        "id": "abc",
        "jsonrpc": "2.0",
        "method": "echo.to_lower",
        "params": {
            "value": "REQUEST WITH ID AS STRING"
        }
    },
    {
        "id": null,
        "jsonrpc": "2.0",
        "method": "echo.to_lower",
        "params": {
            "value": "REQUEST WITH NULL ID"
        }
    },
    {
        "jsonrpc": "2.0",
        "method": "echo.to_lower",
        "params": {
            "value": "NOTIFICATION, NO RESPONSE EXPECTED"
        }
    }
]
```

</td>
<td valign="top">

Responses for all items, except for notifications
```HTTP
HTTP/1.1 200 OK
Content-Type: application/json
```
```json
[
    {
        "id": 1,
        "jsonrpc": "2.0",
        "result": "request with id as number"
    },
    {
        "id": "abc",
        "jsonrpc": "2.0",
        "result": "request with id as string"
    },
    {
        "id": null,
        "jsonrpc": "2.0",
        "result": "request with null id"
    }
]
```

</td>
</tr>


</table>
</details>


## AllowRawResponses
<details>
<summary>Expand</summary>

> `Startup.cs`
```cs
.AddJsonRpcServer(options => {
    options.AllowRawResponses = true;
});
```

> `DataController.cs`
```cs
public class DataController : JsonRpcController
{
    public ActionResult GetBytes(int count)
    {
        var bytes = Enumerable.Range(0, count).Select(x => (byte)x).ToArray();
        return new FileContentResult(bytes, "application/octet-stream");
    }

    public ActionResult Redirect(string url)
    {
        return RedirectPermanent(url);
    }
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
Content-Type: application/json
```
```json
{
    "id": 1,
    "jsonrpc": "2.0",
    "method": "data.get_bytes",
    "params": {
        "count": 100
    }
}
```

</td>
<td valign="top">

Unmodified bytes in response
```HTTP
HTTP/1.1 200 OK
Content-Type: application/octet-stream
Content-Length →100
```
```
�	

 !"#$%&'()*+,-./0123456789:;<=>?@ABCDEFGHIJKLMNOPQRSTUVWXYZ[\]^_`abc
```

</td>
</tr>

<tr>

<td valign="top">

Redirect Request
```http
POST /api/jsonrpc HTTP/1.1
Content-Type: application/json
```
```json
{
    "id": 1,
    "jsonrpc": "2.0",
    "method": "data.redirect_to",
    "params": {
        "url": "https://google.com"
    }
}
```

</td>
<td valign="top">

HTTP Redirect
```HTTP
HTTP/1.1 301 Moved Permanently
Content-Length: 0
Location: https://google.com
```

</td>
</tr>

<tr>

<td valign="top">

JSON Rpc Batch
```http
POST /api/jsonrpc HTTP/1.1
Content-Type: application/json
```
```json
[
    {
        "id": 1,
        "jsonrpc": "2.0",
        "method": "data.get_bytes",
        "params": {
            "count": 100
        }
    }
]
```

</td>
<td valign="top">

JSON Rpc Error
```HTTP
HTTP/1.1 200 OK
Content-Type: application/json
```
```json
[
    {
        "id": 1,
        "jsonrpc": "2.0",
        "error": {
            "code": -32001,
            "message": "Server error",
            "data": {
                "internal_http_code": null,
                "message": "Raw responses are not allowed by default and not supported in batches, check JsonRpcOptions",
                "details": null,
                "type": "Tochka.JsonRpc.Server.Exceptions.JsonRpcInternalException"
            }
        }
    }
]
```

</td>
</tr>


</table>
</details>


## DetailedResponseExceptions
<details>
<summary>Expand</summary>

> `Startup.cs`
```cs
.AddJsonRpcServer(options => {
    options.DetailedResponseExceptions = /*true or false*/;
});
```

> `ErrorController.cs`
```cs
public class ErrorController : JsonRpcController
{
    public string Fail()
    {
        throw new NotImplementedException("not ready yet, come here later!");
    }
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
Content-Type: application/json
```
```json
{
	"id": 1,
    "jsonrpc": "2.0",
    "method": "error.fail",
    "params": null
}
```

</td>
<td valign="top">

No details when `DetailedResponseExceptions` is **false**
```HTTP
HTTP/1.1 200 OK
Content-Type: application/json; charset=utf-8
```
```json
{
    "id": 1,
    "jsonrpc": "2.0",
    "error": {
        "code": -32000,
        "message": "Server error",
        "data": {
            "internal_http_code": null,
            "message": "not ready yet, come here later!",
            "details": null,
            "type": "System.NotImplementedException"
        }
    }
}
```

</td>
</tr>

<tr>

<td valign="top">

Request
```http
POST /api/jsonrpc HTTP/1.1
Content-Type: application/json
```
```json
{
	"id": 1,
    "jsonrpc": "2.0",
    "method": "error.fail",
    "params": null
}
```

</td>
<td valign="top">

`ExceptionInfo` object when `DetailedResponseExceptions` is **true**
```HTTP
HTTP/1.1 200 OK
Content-Type: application/json; charset=utf-8
```
```json
{
    "id": 1,
    "jsonrpc": "2.0",
    "error": {
        "code": -32000,
        "message": "Server error",
        "data": {
            "internal_http_code": null,
            "message": "not ready yet, come here later!",
            "details": "System.NotImplementedException: not ready yet, come here later!\r\n   at WebApplication1.Controllers.ErrorController.Fail() in C:\\Users\\rast\\source\\repos\\WebApplication1\\WebApplication1\\Controllers\\ValuesController.cs:line 73\r\n   at lambda_method(Closure , Object , Object[] )\r\n   at Microsoft.AspNetCore.Mvc.Internal.ActionMethodExecutor.SyncObjectResultExecutor.Execute(IActionResultTypeMapper mapper, ObjectMethodExecutor executor, Object controller, Object[] arguments)\r\n   at Microsoft.AspNetCore.Mvc.Internal.ControllerActionInvoker.InvokeActionMethodAsync()\r\n   at Microsoft.AspNetCore.Mvc.Internal.ControllerActionInvoker.InvokeNextActionFilterAsync()\r\n   at Microsoft.AspNetCore.Mvc.Internal.ControllerActionInvoker.Rethrow(ActionExecutedContext context)\r\n   at Microsoft.AspNetCore.Mvc.Internal.ControllerActionInvoker.Next(State& next, Scope& scope, Object& state, Boolean& isCompleted)\r\n   at Microsoft.AspNetCore.Mvc.Internal.ControllerActionInvoker.InvokeInnerFilterAsync()\r\n   at Microsoft.AspNetCore.Mvc.Internal.ResourceInvoker.InvokeNextResourceFilter()\r\n   at Microsoft.AspNetCore.Mvc.Internal.ResourceInvoker.Rethrow(ResourceExecutedContext context)\r\n   at Microsoft.AspNetCore.Mvc.Internal.ResourceInvoker.Next(State& next, Scope& scope, Object& state, Boolean& isCompleted)\r\n   at Microsoft.AspNetCore.Mvc.Internal.ResourceInvoker.InvokeFilterPipelineAsync()\r\n   at Microsoft.AspNetCore.Mvc.Internal.ResourceInvoker.InvokeAsync()\r\n   at Microsoft.AspNetCore.Routing.EndpointMiddleware.Invoke(HttpContext httpContext)\r\n   at Microsoft.AspNetCore.Routing.EndpointRoutingMiddleware.Invoke(HttpContext httpContext)\r\n   at Tochka.JsonRpc.Server.Services.RequestHandler.SafeNext(IUntypedCall call, HandlingContext context, Boolean allowRawResponses)",
            "type": "System.NotImplementedException"
        }
    }
}
```

</td>
</tr>

<tr>

<td valign="top">

JSON Rpc Batch
```http
POST /api/jsonrpc HTTP/1.1
Content-Type: application/json
```
```json
[
    {
        "id": 1,
        "jsonrpc": "2.0",
        "method": "echo.to_lower",
        "params": {
            "value": "REQUEST WITH ID AS NUMBER"
        }
    },
    {
        "id": "abc",
        "jsonrpc": "2.0",
        "method": "echo.to_lower",
        "params": {
            "value": "REQUEST WITH ID AS STRING"
        }
    },
    {
        "id": null,
        "jsonrpc": "2.0",
        "method": "echo.to_lower",
        "params": {
            "value": "REQUEST WITH NULL ID"
        }
    },
    {
        "jsonrpc": "2.0",
        "method": "echo.to_lower",
        "params": {
            "value": "NOTIFICATION, NO RESPONSE EXPECTED"
        }
    }
]
```

</td>
<td valign="top">

Responses for all items, except for notifications
```HTTP
HTTP/1.1 200 OK
Content-Type: application/json
```
```json
[
    {
        "id": 1,
        "jsonrpc": "2.0",
        "result": "request with id as number"
    },
    {
        "id": "abc",
        "jsonrpc": "2.0",
        "result": "request with id as string"
    },
    {
        "id": null,
        "jsonrpc": "2.0",
        "result": "request with null id"
    }
]
```

</td>
</tr>


</table>
</details>
