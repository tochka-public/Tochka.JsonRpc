# Клиент/Быстрый старт

## Установка

Ставим nuget пакет `Tochka.JsonRpc.Client`.

Реализуем клиент, унаследованный от `JsonRpcClientBase`, используем методы базового класса `SendNotification`, `SendRequest` и `SendBatch` в своей логике.

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

Регистрируем реализацию клиента в `Program.cs` и указываем `BaseAddress`.

```cs
builder.Services.Configure<MyJsonRpcClientOptions>(_ => { });
builder.Services.AddJsonRpcClient<MyJsonRpcClient>(); // еще есть перегрузка для регистрации через интерфейс
    .ConfigureHttpClient(static c => c.BaseAddress = new Uri("https://another.api/jsonrpc/")); // HttpClient можно настроить здесь или в конструкторе
```
Этого достаточно для работы! Теперь можно использовать клиент в своем коде для отправки запросов и обработки ответов.
Про более продвинутое использование читаем дальше:

- [Примеры](examples)
- [Конфигурация](configuration)
