# Сервер/Быстрый старт

## Установка

Ставим nuget пакет `Tochka.JsonRpc.Server`. Затем, в `Program.cs`:

* Регистрируем стандартные ASP.NET Core контроллеры: `AddControllers()`
* Добавляем необходимые сервисы: `AddJsonRpcServer()`
* Как можно раньше добавляем мидлварь: `UseJsonRpc()`
* Добавляем Endpoint Routing к контроллерам стандартным способом: `app.UseRouting()` и `app.UseEndpoints(static c => c.MapControllers())`

```cs
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers(); // <-- Регистрируем контроллеры (также как для REST)
builder.Services.AddJsonRpcServer(); // <-- Регистрируем сервисы библиотеки

var app = builder.Build();

app.UseJsonRpc(); // <-- Добавляем мидлварь
app.UseRouting(); // <-- Добавляем роутинг (также как для REST)
app.UseEndpoints(static c => c.MapControllers()) // <-- Добавляем эндпоинты (также как для REST)

await app.RunAsync();
```

Пишем контроллеры для своего API аналогично тому, как это делается для REST, но вместо наследования от `ControllerBase`, наследуемся от `JsonRpcControllerBase`. Не нужны никакие дополнительные атрибуты, специальное именование или конструкторы.

```cs
public class EchoController : JsonRpcControllerBase
{
    public async Task<ActionResult<string>> ToLower(string value)
    {
        // ...
        var result = value.ToLowerInvariant();
        return this.Ok(result);
    }
}
```

## Отправляем запрос

Запускаем приложение и отправляем POST запрос (дополнительные заголовки опущены)

<table>
    <tr>
        <th>
            Запрос
        </th>
        <th>
            Ответ
        </th>
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

Этого достаточно для работы! Можно писать контроллеры, методы, отправлять батчи, использовать мидлвари, фильтры и атрибуты аналогично обычным контроллерам.
Про более продвинутого использование читаем дальше:

- [Примеры](examples)
- [Конфигурация](configuration)
