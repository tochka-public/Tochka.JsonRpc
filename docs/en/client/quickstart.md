# Client/Quickstart

## Installation

Install nuget package `Tochka.JsonRpc.Client`.

Implement client inherited from `JsonRpcClientBase` and use base class's methods `SendNotification`, `SendRequest` and `SendBatch` for your logic

```cs
internal class MyJsonRpcClientOptions : JsonRpcClientOptionsBase
{
    public override string Url { get; set; } = "https://another.api/jsonrpc/";
}

internal class MyJsonRpcClient : JsonRpcClientBase
{
    public MyJsonRpcClient(HttpClient client, IOptions<MyJsonRpcClientOptions> options, IJsonRpcIdGenerator jsonRpcIdGenerator, ILogger<MyJsonRpcClient> logger)
        : base(client, options.Value, jsonRpcIdGenerator, logger)
    {
    }

    public async Task<Guid> CreateUser(string login, CancellationToken cancellationToken)
    {
        var response = await SendRequest("users.create", new { login }, cancellationToken);
        return response.GetResponseOrThrow<Guid>();
    }
}
```

Register options and client implementation in `Program.cs`

```cs
builder.Services.Configure<MyJsonRpcClientOptions>(_ => { });
builder.Services.AddJsonRpcClient<MyJsonRpcClient>(); // there is also overload to register as interface
```

That's it! Now you can use it in your logic to send requests and process responses.
Check out other pages for more advanced usage:

- [Examples](examples)
- [Configuration](configuration)
