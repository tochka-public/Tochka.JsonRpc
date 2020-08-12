# Server/Examples

Here are examples for different scenarios. Common things like HTTP headers, calls to `AddMvc().SetCompatibilityVersion()` are omitted.

## Basic example with default configuration

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