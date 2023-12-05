# Client/Quickstart

## Installation

Install nuget package `Tochka.JsonRpc.Client`.

Implement client inherited from `JsonRpcClientBase` and use base class's methods `SendNotification`, `SendRequest` and `SendBatch` for your logic.

```cs
internal class MyJsonRpcClient : JsonRpcClientBase
{
    public MyJsonRpcClient(HttpClient client, IJsonRpcIdGenerator jsonRpcIdGenerator, ILogger<MyJsonRpcClient> logger)
        : base(client, jsonRpcIdGenerator, logger)
    {
    }

    public async Task<Guid> CreateUser(string login, CancellationToken cancellationToken)
    {
        var response = await SendRequest("users.create", new { login }, cancellationToken);
        return response.GetResponseOrThrow<Guid>();
    }
}
```

Register client implementation in `Program.cs` and configure `BaseAddress`.

```cs
builder.Services.AddJsonRpcClient<MyJsonRpcClient>(); // there is also overload to register as interface
    .ConfigureHttpClient(static c => c.BaseAddress = new Uri("https://another.api/jsonrpc/")); // HttpClient can be configured here or in constructor
```

That's it! Now you can use it in your logic to send requests and process responses.
Check out other pages for more advanced usage:

- [Examples](examples)
- [Configuration](configuration)
