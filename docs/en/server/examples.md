# Server/Examples

Here are examples for different scenarios. Common things like HTTP headers, calls to `AddMvc().SetCompatibilityVersion()` are omitted.

## Basic example (from quickstart)

<table>
    <tr>
        <td>
            Code
        </td>
        <td>
            Request
        </td>
        <td>
            Response
        </td>
    </tr>
<tr>
<td valign="top">

`Startup.cs`
```cs
.AddJsonRpcServer()
```

</td>
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