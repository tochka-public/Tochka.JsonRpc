# Клиент/Краткое руководство

## Установка

Установите nuget пакет `Tochka.JsonRpc.Client`.

Реализуйте клиент, унаследованный от `JsonRpcClientBase`, и используйте методы базового класса `SendNotification`, `SendRequest` и `SendBatch` в своей логике.

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

Зарегистрируйте настройки и реализацию клиента в `Program.cs`.

```cs
builder.Services.Configure<MyJsonRpcClientOptions>(_ => { });
builder.Services.AddJsonRpcClient<MyJsonRpcClient>(); // также доступна перегрузка для регистрации через интерфейс
```
Этого достаточно для работы! Теперь вы можете использовать клиент в своем коде для отправки запросов и обработки ответов.
Посмотрите остальные страницы для более продвинутого использования:

- [Примеры](examples)
- [Конфигурация](configuration)
