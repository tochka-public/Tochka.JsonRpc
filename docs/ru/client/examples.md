# Клиент/Примеры

Здесь представлены примеры для различных сценариев. Обычные штуки, вроде HTTP заголовков, конструкторов, регистрации сервисов в DI опущены для краткости.

> Больше деталей и продвинутое использование: [Конфигурация](configuration)

## Request, Notification, Batch с настройками по умолчанию

Примеры базовых JSON-RPC вызовов с настройками по умолчанию

<details>
<summary>Развернуть</summary>

<table>
<tr>
    <td>
        Метод клиента
    </td>
    <td>
        Отправленный JSON-RPC вызов
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

Request without params
```cs
public async Task<UserModel[]> GetUsers(CancellationToken token)
{
    var response = await SendRequest("users.get", token);
    return response.GetResponseOrThrow<UserModel[]>();
}

var response = await myClient.GetUsers(token);
```

</td>
<td>

```json
{
    "id": "56249f26-9748-461c-aeaf-b74b6a244ac6",
    "method": "users.get",
    "params": null,
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

## Получение сырых данных ответа

Возврат сырых данных без десериализации и валидации

<details>
<summary>Развернуть</summary>

```cs
public async Task<byte[]> GetFile(string name, CancellationToken token)
{
    var call = new Request<GetFileRequest>(RpcIdGenerator.GenerateId(), "file.get", new(name));
    var response = await Send(call, token); // response имеет тип HttpResponseMessage
    response.EnsureSuccessStatusCode();
    return await response.Content.ReadAsByteArrayAsync(token);
}
```

</details>

## Настройка HttpClient (например, BaseAddress, Timeout, заголовки авторизации)

Настройка внутреннего `HttpClient`, который используется для отправки запросов

<details>
<summary>В конструкторе реализованного клиента</summary>

```cs
public class MyJsonRpcClient
{
    public override string UserAgent => "User-Agent header value";
    protected override Encoding Encoding => Encoding.UTF32;

    public MyJsonRpcClient(HttpClient client, IOptions<MyJsonRpcClientOptions> options, IJsonRpcIdGenerator jsonRpcIdGenerator, ILogger<MyJsonRpcClient> logger)
        : base(client, jsonRpcIdGenerator, logger)
    {
        client.BaseAddress = new Uri(options.Value.BaseAddress);
        client.Timeout = TimeSpan.FromSeconds(options.Value.Timeout);
        var basicAuth = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{options.Value.Login}:{options.Value.Password}"));
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", basicAuth);
    }
}
```

</details>

<details>
<summary>Через ConfigureHttpClient()</summary>

> `Program.cs`
```cs
builder.Services.AddJsonRpcClient<MyJsonRpcClient>();
    .ConfigureHttpClient(static client =>
    {
        client.BaseAddress = new Uri("https://another.api/jsonrpc/");
        client.Timeout = TimeSpan.FromSeconds(10);
        var basicAuth = Convert.ToBase64String(Encoding.UTF8.GetBytes("login:password"));
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", basicAuth);
    });
```

</details>

## Настройка сериализации параметров и десериализации результата

Изменение настроек сериализации JSON для использования другой политики именования или дополнительных конвертеров

<details>
<summary>Развернуть</summary>

Можно использовать одно из значений в классе `JsonRpcSerializerOptions`, или создать собственный объект `JsonSerializerOptions`.

> Эти настройки не повлияют на "заголовки" JSON-RPC (id, method, jsonrpc) - логика их сериализации настраивается через `HeadersJsonSerializerOptions` и изменять ее не рекомендуется!

```cs
public class MyJsonRpcClient
{
    public override JsonSerializerOptions DataJsonSerializerOptions => JsonRpcSerializerOptions.CamelCase;

    public MyJsonRpcClient(HttpClient client, IJsonRpcIdGenerator jsonRpcIdGenerator, ILogger<MyJsonRpcClient> logger)
        : base(client, jsonRpcIdGenerator, logger)
    {
    }
}
```

</details>

## Обработка ошибок

Обработка ошибок в ответах без выбрасывания исключений

<details>
<summary>Развернуть</summary>

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

## Логирование запросов

Логирование исходящих запросов

<details>
<summary>Развернуть</summary>

```cs
builder.Services.AddJsonRpcClient<IFooClient, FooClient>()
    .WithJsonRpcRequestLogging();
```

</details>
