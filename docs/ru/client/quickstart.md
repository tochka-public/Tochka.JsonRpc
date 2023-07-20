# Клиент/Быстрый старт

## Установка

Ставим nuget пакет `Tochka.JsonRpc.Client`.

Реализуем клиент, унаследованный от `JsonRpcClientBase`, используем методы базового класса `SendNotification`, `SendRequest` и `SendBatch` в своей логике.

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

Регистрируем настройки и реализацию клиента в `Program.cs`.

```cs
builder.Services.Configure<MyJsonRpcClientOptions>(_ => { });
builder.Services.AddJsonRpcClient<MyJsonRpcClient>(); // также доступна перегрузка для регистрации через интерфейс
```
Этого достаточно для работы! Теперь можно использовать клиент в своем коде для отправки запросов и обработки ответов.
Про более продвинутого использование читаем дальше:

- [Примеры](examples)
- [Конфигурация](configuration)
