# Client/Examples

Here are examples for different scenarios. Common things like default HTTP headers, constructors, options implementations, registering services in DI are omitted.

## Request, Notification, Batch with default configuration

Examples of basic JSON Rpc calls with default configuration

<details>
<summary>Expand</summary>

<table>
<tr>
    <td>
        Client's method
    </td>
    <td>
        Sended JSON Rpc call 
    </td>
</tr>

<tr>
<td valign="top">

Request
```cs
public async Task<Guid> CreateUser(string login, CancellationToken token)
{
    var response = await SendRequest("users.create", new CreateRequest(login), token);
    return response.GetResponseOrThrow<Guid>();
}

var response = await myClient.CreateUser("user_login", token);
```

</td>
<td>

```json
{
    "id": "56249f26-9748-461c-aeaf-b74b6a244ac6",
    "method": "users.create",
    "params": {
        "login": "user_login"
    },
    "jsonrpc": "2.0"
}
```

</td>
</tr>

<tr>
<td valign="top">

Notification
```cs
public async Task CreateUser(string login, CancellationToken token) =>
    await SendNotification("users.create", new CreateRequest(login), token);

await myClient.CreateUser("user_login", token);
```

</td>
<td>

```json
{
    "method": "users.create",
    "params": {
        "login": "user_login"
    },
    "jsonrpc": "2.0"
}
```

</td>
</tr>

<tr>
<td valign="top">

Batch
```cs
public async Task<Dictionary<string, Guid>> CreateUsers(IEnumerable<string> logins, CancellationToken token)
{
    var calls = logins.Select(l =>
            new Request<CreateRequest>(RpcIdGenerator.GenerateId(), "user.create", new(l)))
        .ToArray();
    var response = await SendBatch(calls, token);
    return calls.ToDictionary(static c => c.Params.Login, c => response.GetResponseOrThrow<Guid>(c.Id));
}

var response = await myClient.CreateUsers(new[] { "user_login1", "user_login2" }, token);
```

</td>
<td>

```json
[
    {
        "id": "8fc6020d-c9a7-4d9b-913a-6868580a5f72",
        "method": "users.create",
        "params": {
            "login": "user_login1"
        },
        "jsonrpc": "2.0"
    },
    {
        "id": "5c24149a-c6b3-47ba-babf-1e5ad774973d",
        "method": "users.create",
        "params": {
            "login": "user_login2"
        },
        "jsonrpc": "2.0"
    }
]
```

</td>
</tr>

</table>

</details>

## Receive raw data in response

Don't parse and validate response and just return it

<details>
<summary>Expand</summary>

```cs
public async Task<byte[]> GetFile(string name, CancellationToken token)
{
    var call = new Request<GetFileRequest>(RpcIdGenerator.GenerateId(), "file.get", new(name));
    var response = await Send(call, token); // response is HttpResponseMessage
    response.EnsureSuccessStatusCode();
    return await response.Content.ReadAsByteArrayAsync(token);
}
```

</details>

</details>

## Configure HttpClient (eg. Authorization headers)

Configure internal HttpClient used to send requests

<details>
<summary>Expand</summary>

```cs
public class MyJsonRpcClient
{
    public override string UserAgent => "User-Agent header value";
    protected override Encoding Encoding => Encoding.UTF32;

    public MyJsonRpcClient(HttpClient client, IOptions<MyJsonRpcClientOptions> options, IJsonRpcIdGenerator jsonRpcIdGenerator, ILogger<MyJsonRpcClient> logger)
        : base(client, options.Value, jsonRpcIdGenerator, logger)
    {
        var basicAuth = Convert.ToBase64String(Encoding.UTF8.GetBytes("login:password"));
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", basicAuth);
    }
}
```

</details>

## Configure params serialization and result deserialization

Change json serialization options to use other naming policy or custom converters

<details>
<summary>Expand</summary>

You can use one of predefined values from `JsonRpcSerializerOptions` class or create your own `JsonSerializerOptions`.

> This options won't affect JSON Rpc "headers" (id, method, params) - their serialization logic configured by `HeadersJsonSerializerOptions` and it's not recommended to change it

```cs
public class MyJsonRpcClient
{
    public override JsonSerializerOptions DataJsonSerializerOptions => JsonRpcSerializerOptions.CamelCase;

    public MyJsonRpcClient(HttpClient client, IOptions<MyJsonRpcClientOptions> options, IJsonRpcIdGenerator jsonRpcIdGenerator, ILogger<MyJsonRpcClient> logger)
        : base(client, options.Value, jsonRpcIdGenerator, logger)
    {
    }
}
```

</details>

## Process errors

Process errors in responses without throwing exceptions

<details>
<summary>Expand</summary>

```cs
public async Task<BusinessError?> GetError(CancellationToken token)
{
    var response = await SendRequest("error.get", new { }, token);
    if (!response.HasError())
    {
        return null;
    }

    var errorCode = response.AsAnyError().Code;
    if (errorCode != 123)
    {
        throw new ArgumentException($"Unexpected error code {errorCode}");
    }

    return response.AsTypedError<BusinessError>().Data;
}
```

</details>