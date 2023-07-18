# Сервер/Краткое руководство

## Установка

Установите nuget пакет `Tochka.JsonRpc.Server`. Затем, в `Program.cs`:

* Зарегистрируйте стандартные ASP.NET Core контроллеры: `AddControllers()`
* Добавьте необходимые сервисы: `AddJsonRpcServer()`
* Как можно раньше добавьте мидлварь: `UseJsonRpc()`
* Добавьте Endpoint Routing к контроллерам стандартным способом: `app.UseRouting()` и `app.UseEndpoints(static c => c.MapControllers())`

```cs
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers(); // <-- Зарегистрируйте контроллеры (также как для REST)
builder.Services.AddJsonRpcServer(); // <-- Зарегистрируйте сервисы библиотеки

var app = builder.Build();

app.UseJsonRpc(); // <-- Добавьте мидлварь
app.UseRouting(); // <-- Добавьте роутинг (также как для REST)
app.UseEndpoints(static c => c.MapControllers()) // <-- Добавьте эндпоинты (также как для REST)

await app.RunAsync();
```

Пишите контроллеры для своего API аналогично тому, как это делается для REST, но вместо наследования от `ControllerBase`, наследуйте их от `JsonRpcControllerBase`. Вам не нужно использовать дополнительные атрибуты, специальное именование или конструкторы.

```cs
public class EchoController : JsonRpcControllerBase
{
    public string ToLower(string value) => value.ToLowerInvariant();
}
```

## Отправьте запрос

Запустите приложение и отправьте POST запрос (дополнительные заголовки опущены)

<table>
    <tr>
        <td>
            Запрос
        </td>
        <td>
            Ответ
        </td>
    </tr>
<tr>
<td valign="top">

```http
POST /api/jsonrpc HTTP/1.1
Content-Type: application/json
```
```json
{
    "id": 1,
    "method": "echo.to_lower",
    "params": {
        "value": "TEST"
    },
    "jsonrpc": "2.0"
}
```

</td>
<td valign="top">

```http
HTTP/1.1 200 OK
Content-Type: application/json; charset=utf-8
```
```json
{
    "id": 1,
    "result": "test",
    "jsonrpc": "2.0"
}
```

</td>
</tr>
</table>

Этого достаточно для работы! Пишите контроллеры, методы, отправляйте батчи, используйте мидлвари, фильтры и атрибуты аналогично обычным контроллерам.
Посмотрите остальные страницы для более продвинутого использования:

- [Примеры](examples)
- [Конфигурация](configuration)
