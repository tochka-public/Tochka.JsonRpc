# Сервер/Примеры

Здесь представлены примеры для различных сценариев. Обычные штуки, вроде HTTP заголовков, создания/запуска приложения и регистрации контроллеров опущены для краткости.

> Больше деталей и продвинутое использование: [Конфигурация](configuration)

## Request, Notification, Batch с настройками по умолчанию

Примеры базовых JSON-RPC вызовов с настройками по умолчанию.

<details>
<summary>Развернуть</summary>

> `Program.cs`
```cs
// ...
builder.Services.AddJsonRpcServer();
// ...
app.UseJsonRpc();
// ...
```

> `EchoController.cs`
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

JSON-RPC Request
```http
POST /api/jsonrpc HTTP/1.1
Content-Type: application/json; charset=utf-8
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

Обычный ответ
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

<tr>

<td valign="top">

JSON-RPC Notification
```http
POST /api/jsonrpc HTTP/1.1
Content-Type: application/json; charset=utf-8
```
```json
{
    "method": "echo.to_lower",
    "params": {
        "value": "TEST"
    },
    "jsonrpc": "2.0"
}
```

</td>
<td valign="top">

Нет ответа, по спецификации
```http
HTTP/1.1 200 OK
Content-Length: 0
```

</td>
</tr>

<tr>

<td valign="top">

JSON-RPC Batch
```http
POST /api/jsonrpc HTTP/1.1
Content-Type: application/json; charset=utf-8
```
```json
[
    {
        "id": 1,
        "method": "echo.to_lower",
        "params": {
            "value": "REQUEST WITH ID AS NUMBER"
        },
        "jsonrpc": "2.0"
    },
    {
        "id": "abc",
        "method": "echo.to_lower",
        "params": {
            "value": "REQUEST WITH ID AS STRING"
        },
        "jsonrpc": "2.0"
    },
    {
        "id": null,
        "method": "echo.to_lower",
        "params": {
            "value": "REQUEST WITH NULL ID"
        },
        "jsonrpc": "2.0"
    },
    {
        "method": "echo.to_lower",
        "params": {
            "value": "NOTIFICATION, NO RESPONSE EXPECTED"
        },
        "jsonrpc": "2.0"
    }
]
```

</td>
<td valign="top">

Ответы для всех элементов запроса, кроме notification-ов
```http
HTTP/1.1 200 OK
Content-Type: application/json; charset=utf-8
```
```json
[
    {
        "id": 1,
        "result": "request with id as number",
        "jsonrpc": "2.0"
    },
    {
        "id": "abc",
        "result": "request with id as string",
        "jsonrpc": "2.0"
    },
    {
        "id": null,
        "result": "request with null id",
        "jsonrpc": "2.0"
    }
]
```

</td>
</tr>


</table>
</details>


## AllowRawResponses

Нарушаем протокол, чтобы вернуть байты, HTTP код и тп. Подробности см. в [Конфигурация#AllowRawResponses](configuration#AllowRawResponses).

<details>
<summary>Развернуть</summary>

> `Program.cs`
```cs
// ...
builder.Services.AddJsonRpcServer(static options => options.AllowRawResponses = true);
// ...
app.UseJsonRpc();
// ...
```

> `DataController.cs`
```cs
public class DataController : JsonRpcControllerBase
{
    public async Task<IActionResult> GetBytes(int count)
    {
        // ...
        var bytes = Enumerable.Range(0, count).Select(static x => (byte) x).ToArray();
        return new FileContentResult(bytes, "application/octet-stream");
    }

    public async Task<IActionResult> RedirectTo(string url)
    {
        // ...
        return this.RedirectPermanent(url);
    }
}
```

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

Запрос GetBytes
```http
POST /api/jsonrpc HTTP/1.1
Content-Type: application/json; charset=utf-8
```
```json
{
    "id": 1,
    "method": "data.get_bytes",
    "params": {
        "count": 100
    },
    "jsonrpc": "2.0"
}
```

</td>
<td valign="top">

Неизмененные байты в ответе
```http
HTTP/1.1 200 OK
Content-Type: application/octet-stream
Content-Length: 100
```
```
�

 !"#$%&'()*+,-./0123456789:;<=>?@ABCDEFGHIJKLMNOPQRSTUVWXYZ[\]^_`abc
```

</td>
</tr>

<tr>

<td valign="top">

Запрос RedirectTo
```http
POST /api/jsonrpc HTTP/1.1
Content-Type: application/json; charset=utf-8
```
```json
{
    "id": 1,
    "method": "data.redirect_to",
    "params": {
        "url": "https://google.com"
    },
    "jsonrpc": "2.0"
}
```

</td>
<td valign="top">

HTTP Redirect
```http
HTTP/1.1 301 Moved Permanently
Content-Length: 0
Location: https://google.com
```

</td>
</tr>

<tr>

<td valign="top">

JSON-RPC Batch
```http
POST /api/jsonrpc HTTP/1.1
Content-Type: application/json; charset=utf-8
```
```json
[
    {
        "id": 1,
        "method": "data.get_bytes",
        "params": {
            "count": 100
        },
        "jsonrpc": "2.0"
    }
]
```

</td>
<td valign="top">

JSON-RPC Error
```http
HTTP/1.1 200 OK
Content-Type: application/json; charset=utf-8
```
```json
[
    {
        "id": 1,
        "error": {
            "code": -32001,
            "message": "Server error",
            "data": {
                "type": "Tochka.JsonRpc.Server.Exceptions.JsonRpcServerException",
                "message": "Raw responses are not allowed in batch requests",
                "details": null
            }
        },
        "jsonrpc": "2.0"
    }
]
```

</td>
</tr>


</table>
</details>


## DetailedResponseExceptions

Скрытие или предоставление информации об исключениях. Подробности см. в [Конфигурация#DetailedResponseExceptions](configuration#DetailedResponseExceptions).

<details>
<summary>Развернуть</summary>

> `Program.cs`
```cs
// ...
builder.Services.AddJsonRpcServer(static options => options.DetailedResponseExceptions = /* true или false */);
// ...
app.UseJsonRpc();
// ...
```

> `ErrorController.cs`
```cs
public class ErrorController : JsonRpcControllerBase
{
    public async Task<IActionResult> Fail()
    {
        // ...
        throw new NotImplementedException("exception message");
    }
}
```

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

Запрос
```http
POST /api/jsonrpc HTTP/1.1
Content-Type: application/json; charset=utf-8
```
```json
{
    "id": 1,
    "method": "error.fail",
    "params": null,
    "jsonrpc": "2.0"
}
```

</td>
<td valign="top">

Отсутствуют details, если `DetailedResponseExceptions` равен **false**
```http
HTTP/1.1 200 OK
Content-Type: application/json; charset=utf-8
```
```json
{
    "id": 1,
    "error": {
        "code": -32000,
        "message": "Server error",
        "data": {
            "type": "System.NotImplementedException",
            "message": "exception message",
            "details": null
        }
    },
    "jsonrpc": "2.0"
}
```

</td>
</tr>

<tr>

<td valign="top">

Запрос
```http
POST /api/jsonrpc HTTP/1.1
Content-Type: application/json; charset=utf-8
```
```json
{
    "id": 1,
    "method": "error.fail",
    "params": null,
    "jsonrpc": "2.0"
}
```

</td>
<td valign="top">

`exception.ToString()` в поле details, если `DetailedResponseExceptions` равен **true**
```http
HTTP/1.1 200 OK
Content-Type: application/json; charset=utf-8
```
```json
{
    "id": 1,
    "error": {
        "code": -32000,
        "message": "Server error",
        "data": {
            "type": "System.NotImplementedException",
            "message": "exception message",
            "details": "System.NotImplementedException: exception message\r\n   at Application.Controllers.ErrorController.Fail() in C:\\Path\\To\\Application\\Controllers\\ErrorController.cs:line 7\r\n   at lambda_method6(Closure , Object , Object[] )\r\n   at Microsoft.AspNetCore.Mvc.Infrastructure.ActionMethodExecutor.SyncObjectResultExecutor.Execute(IActionResultTypeMapper mapper, ObjectMethodExecutor executor, Object controller, Object[] arguments)\r\n   at Microsoft.AspNetCore.Mvc.Infrastructure.ControllerActionInvoker.InvokeActionMethodAsync()\r\n   at Microsoft.AspNetCore.Mvc.Infrastructure.ControllerActionInvoker.Next(State& next, Scope& scope, Object& state, Boolean& isCompleted)\r\n   at Microsoft.AspNetCore.Mvc.Infrastructure.ControllerActionInvoker.InvokeNextActionFilterAsync()\r\n--- End of stack trace from previous location ---\r\n   at Microsoft.AspNetCore.Mvc.Infrastructure.ControllerActionInvoker.Rethrow(ActionExecutedContextSealed context)\r\n   at Microsoft.AspNetCore.Mvc.Infrastructure.ControllerActionInvoker.Next(State& next, Scope& scope, Object& state, Boolean& isCompleted)\r\n   at Microsoft.AspNetCore.Mvc.Infrastructure.ControllerActionInvoker.InvokeInnerFilterAsync()\r\n--- End of stack trace from previous location ---\r\n   at Microsoft.AspNetCore.Mvc.Infrastructure.ResourceInvoker.<InvokeNextExceptionFilterAsync>g__Awaited|26_0(ResourceInvoker invoker, Task lastTask, State next, Scope scope, Object state, Boolean isCompleted)"
        }
    },
    "jsonrpc": "2.0"
}
```

</td>
</tr>


</table>
</details>


## Маршрутизация

Переопределение маршрутизации через глобальную настройку или атрибут. Подробности см. в [Маршрутизация](routing).

<details>
<summary>Развернуть</summary>

Все методы JSON-RPC должны иметь одинаковый префикс адреса (по умолчанию `/api/jsonrpc`), чтобы их можно было отличить от REST запросов, если оба API используются в одном проекте. Если префикс не указан явно в route метода, то он будет добавлен автоматически. Для методов, у которых адрес не указан вручную, префикс будет использоваться как полный route (без части `/controllerName`).

Изменение адреса по умолчанию и переопределение его для контроллера или метода:
> `Program.cs`
```cs
// ...
builder.Services.AddJsonRpcServer(static options => options.RoutePrefix = "/public_api");
// ...
app.UseJsonRpc();
// ...
```

> `UsersController.cs`
```cs
/* Здесь тоже можно переопределить [Route] */
public class UsersController : JsonRpcControllerBase
{
    public async Task<ActionResult<List<string>>> GetNames()
    {
        // ...
        return this.Ok(new List<string>() { "Alice", "Bob" });
    }

    [Route("/admin_api")]
    public async Task<ActionResult<Guid>> Create(string name)
    {
        // добавляем пользователя в БД и возвращаем ID
        // ...
        return this.Ok(Guid.NewGuid());
    }
}
```

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

Запрос GetNames по адресу по умолчанию
```http
POST /public_api HTTP/1.1
Content-Type: application/json; charset=utf-8
```
```json
{
    "id": 1,
    "method": "users.get_names",
    "params": null,
    "jsonrpc": "2.0"
}
```

</td>
<td valign="top">

Обычный ответ
```http
HTTP/1.1 200 OK
Content-Type: application/json; charset=utf-8
```
```json
{
    "id": 1,
    "result": [
        "Alice",
        "Bob"
    ],
    "jsonrpc": "2.0"
}
```

</td>
</tr>

<tr>

<td valign="top">

Запрос Create на переопределенный адрес без префикса
```http
POST /admin_api HTTP/1.1
Content-Type: application/json; charset=utf-8
```
```json
{
    "id": 1,
    "method": "users.create",
    "params": {
        "name": "Charlie"
    },
    "jsonrpc": "2.0"
}
```

</td>
<td valign="top">

Ответ с ошибкой 404
```http
HTTP/1.1 404 Not Found
Content-Length: 0
```

</td>
</tr>

<tr>

<td valign="top">

Запрос Create на переопределенный адрес с префиксом
```http
POST /public_api/admin_api HTTP/1.1
Content-Type: application/json; charset=utf-8
```
```json
{
    "id": 1,
    "method": "users.create",
    "params": {
        "name": "Charlie"
    },
    "jsonrpc": "2.0"
}
```

</td>
<td valign="top">

Обычный ответ
```http
HTTP/1.1 200 OK
Content-Type: application/json; charset=utf-8
```
```json
{
    "id": 1,
    "result": "82a160a8-ad1d-472f-84d3-569b1514f384",
    "jsonrpc": "2.0"
}
```

</td>
</tr>

</table>
</details>


## Method

Изменение сравнения поля `method` с контроллерами и методами. Поле `method` в запросе можно трактовать по-разному, в зависимости от глобальной настройки: как `controller.action` или как `action`. Также можно вручную установить значение через `JsonRpcMethodAttribute`. Подробности см. в [Сериализация#Сравнение метода из запроса и имени контроллера/метода](serialization#Сравнение-метода-из-запроса-и-имени-контроллераметода).

<details>
<summary>Развернуть</summary>


> `Program.cs`
```cs
// ...
builder.Services.AddJsonRpcServer(static options => options.DefaultMethodStyle = /* JsonRpcMethodStyle.ControllerAndAction или JsonRpcMethodStyle.ActionOnly */);
// ...
app.UseJsonRpc();
// ...
```

> `EchoController.cs`
```cs
/* Здесь тоже можно переопределить [JsonRpcMethodStyle] */
public class EchoController : JsonRpcControllerBase
{
    /* Здесь тоже можно переопределить [JsonRpcMethodStyle] или [JsonRpcMethod] */
    public async Task<ActionResult<string>> ToLower(string value)
    {
        // ...
        var result = value.ToLowerInvariant();
        return this.Ok(result);
    }

    [JsonRpcMethod("to upper")]
    public async Task<ActionResult<string>> ToUpper(string value)
    {
        // ...
        var result = value.ToUpperInvariant();
        return this.Ok(result);
    }
}
```

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

Запрос со значением method в виде `controller.action` (`JsonRpcMethodStyle.ControllerAndAction`)
```http
POST /api/jsonrpc HTTP/1.1
Content-Type: application/json; charset=utf-8
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

Ответ от `EchoController.ToLower`
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

<tr>

<td valign="top">

Запрос со значением method в виде `action` (`JsonRpcMethodStyle.ActionOnly`)
```http
POST /api/jsonrpc HTTP/1.1
Content-Type: application/json; charset=utf-8
```
```json
{
    "id": 1,
    "method": "to_lower",
    "params": {
        "value": "TEST"
    },
    "jsonrpc": "2.0"
}
```

</td>
<td valign="top">

Ответ от `EchoController.ToLower`
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

<tr>

<td valign="top">

Запрос с установленным вручную method (через `JsonRpcMethodAttribute`)
```http
POST /api/jsonrpc HTTP/1.1
Content-Type: application/json; charset=utf-8
```
```json
{
    "id": 1,
    "method": "to upper",
    "params": {
        "value": "test"
    },
    "jsonrpc": "2.0"
}
```

</td>
<td valign="top">

Ответ от `EchoController.ToUpper`
```http
HTTP/1.1 200 OK
Content-Type: application/json; charset=utf-8
```
```json
{
    "id": 1,
    "result": "TEST",
    "jsonrpc": "2.0"
}
```

</td>
</tr>


</table>
</details>


## Сериализация

Изменение настроек сериализации JSON по умолчанию или их переопределение для контроллера/метода. Подробности см. в [Сериализация](serialization).

<details>
<summary>Развернуть</summary>

Обращаем внимание, как сериализация влияет на поля `params` и `method`.
> `Program.cs`
```cs
// ...

// Еще можно использовать настройки из класса JsonRpcSerializerOptions
var jsonSerializerOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
builder.Services.AddJsonRpcServer(options => options.DefaultDataJsonSerializerOptions = jsonSerializerOptions);

// Провайдер настроек для использования в JsonRpcSerializerOptionsAttribute
builder.Services.AddSingleton<IJsonSerializerOptionsProvider, SnakeCaseJsonSerializerOptionsProvider>();
// ...
app.UseJsonRpc();
// ...
```

> `SimpleCalcController.cs`
```cs
/* Здесь тоже можно переопределить [JsonRpcSerializerOptions] */
public class SimpleCalcController : JsonRpcControllerBase
{
    public async Task<ActionResult<object>> SubtractIntegers(int firstValue, int secondValue)
    {
        // ...
        return this.Ok(new
        {
            firstValue,
            secondValue,
            firstMinusSecond = firstValue - secondValue
        });
    }

    // ВАЖНО: SnakeCaseJsonSerializerOptionsProvider должен быть зарегистрирован в DI как IJsonSerializerOptionsProvider
    [JsonRpcSerializerOptions(typeof(SnakeCaseJsonSerializerOptionsProvider))]
    public async Task<ActionResult<object>> AddIntegers(int firstValue, int secondValue)
    {
        // ...
        return this.Ok(new
        {
            firstValue,
            secondValue,
            firstPlusSecond = firstValue + secondValue
        });
    }
}
```

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

Запрос с camelCase
```http
POST /api/jsonrpc HTTP/1.1
Content-Type: application/json; charset=utf-8
```
```json
{
    "id": 1,
    "method": "simpleCalc.subtractIntegers",
    "params": {
        "firstValue": 42,
        "secondValue": 38
    },
    "jsonrpc": "2.0"
}
```

</td>
<td valign="top">

Ответ с camelCase
```http
HTTP/1.1 200 OK
Content-Type: application/json; charset=utf-8
```
```json
{
    "id": 1,
    "result": {
        "firstValue": 42,
        "secondValue": 38,
        "firstMinusSecond": 4
    },
    "jsonrpc": "2.0"
}
```

</td>
</tr>

<tr>

<td valign="top">

Запрос со snake_case
```http
POST /api/jsonrpc HTTP/1.1
Content-Type: application/json; charset=utf-8
```
```json
{
    "id": 1,
    "method": "simple_calc.add_integers",
    "params": {
        "first_value": 42,
        "second_value": 38
    },
    "jsonrpc": "2.0"
}
```

</td>
<td valign="top">

Ответ со snake_case
```http
HTTP/1.1 200 OK
Content-Type: application/json; charset=utf-8
```
```json
{
    "id": 1,
    "result": {
        "first_value": 42,
        "second_value": 38,
        "first_plus_second": 80
    },
    "jsonrpc": "2.0"
}
```

</td>
</tr>


</table>
</details>


## Binding

Изменение биндинга поля `params` в аргументы метода. Подробности см. в [Binding](binding).

<details>
<summary>Поведение по умолчанию: params биндится в аргументы метода. Значение поля params может быть [] или {} согласно спецификации</summary>

<table>
<tr>
    <th>
        Запрос
    </th>
    <th>
        Метод
    </th>
</tr>

<tr>

<td valign="top">

В запросе объект с двумя полями
```http
POST /api/jsonrpc HTTP/1.1
Content-Type: application/json; charset=utf-8
```
```json
{
    "id": 1,
    "method": "foo",
    "params": {
        "bar": 1,
        "baz": "test"
    },
    "jsonrpc": "2.0"
}
```

</td>
<td valign="top">

`params` биндится в аргументы метода по именам
```cs
public async Task<IActionResult> Foo(int bar, string baz)
{
    // bar == 1
    // baz == "test"

    // ...
}
```

</td>
</tr>

<tr>

<td valign="top">

В запросе массив с двумя элементами
```http
POST /api/jsonrpc HTTP/1.1
Content-Type: application/json; charset=utf-8
```
```json
{
    "id": 1,
    "method": "foo",
    "params": [
        1,
        "test"
    ],
    "jsonrpc": "2.0"
}
```

</td>
<td valign="top">

`params` биндится в аргументы метода по индексам
```cs
public async Task<IActionResult> Foo(int bar, string baz)
{
    // bar == 1
    // baz == "test"

    // ...
}
```

</td>
</tr>


</table>
</details>


<details>
<summary>Биндинг всего объекта params к одной модели, например, когда в модели много полей</summary>

<table>
<tr>
    <th>
        Запрос
    </th>
    <th>
        Метод
    </th>
</tr>

<tr>

<td valign="top">

В запросе объект с двумя полями
```http
POST /api/jsonrpc HTTP/1.1
Content-Type: application/json; charset=utf-8
```
```json
{
    "id": 1,
    "method": "foo",
    "params": {
        "bar": 1,
        "baz": "test"
    },
    "jsonrpc": "2.0"
}
```

</td>
<td valign="top">

`params` биндится в один аргумент метода
```cs
public record Data(int Bar, string Baz);

public async Task<IActionResult> Foo([FromParams(BindingStyle.Object)] Data data)
{
    // data.Bar == 1
    // data.Baz == "test"

    // ...
}
```

</td>
</tr>

<tr>

<td valign="top">

В запросе массив с двумя элементами
```http
POST /api/jsonrpc HTTP/1.1
Content-Type: application/json; charset=utf-8
```
```json
{
    "id": 1,
    "method": "foo",
    "params": [
        1,
        "test"
    ],
    "jsonrpc": "2.0"
}
```

</td>
<td valign="top">

Ошибка, потому что элементы массива не биндятся в поля объекта
```cs
public record Data(int Bar, string Baz);

public async Task<IActionResult> Foo([FromParams(BindingStyle.Object)] Data data)
{
    // не работает для массива `params`
}
```
```json
{
    "id": "123",
    "error": {
        "code": -32602,
        "message": "Invalid params",
        "data": {
            "data": [
                "Error while binding value by JSON key = [params] - Can't bind array to object parameter"
            ]
        }
    },
    "jsonrpc": "2.0"
}
```

</td>
</tr>


</table>

</details>


<details>
<summary>Биндинг всего массива params к одной коллекции, например, когда в запросе может быть неопределенное количество параметров</summary>

<table>
<tr>
    <th>
        Запрос
    </th>
    <th>
        Метод
    </th>
</tr>

<tr>

<td valign="top">

В запросе объект с двумя полями
```http
POST /api/jsonrpc HTTP/1.1
Content-Type: application/json; charset=utf-8
```
```json
{
    "id": 1,
    "method": "foo",
    "params": {
        "bar": 1,
        "baz": 2
    },
    "jsonrpc": "2.0"
}
```

</td>
<td valign="top">

Ошибка, потому что поля объекта не биндятся в элементы массива
```cs
public async Task<IActionResult> Foo([FromParams(BindingStyle.Array)] List<int> data)
{
    //  не работает для объекта `params`
}
```
```json
{
    "id": 1,
    "error": {
        "code": -32602,
        "message": "Invalid params",
        "data": {
            "data": [
                "Error while binding value by JSON key = [params] - Can't bind object to collection parameter"
            ]
        }
    },
    "jsonrpc": "2.0"
}
```

</td>
</tr>

<tr>

<td valign="top">

В запросе массив с двумя элементами
```http
POST /api/jsonrpc HTTP/1.1
Content-Type: application/json; charset=utf-8
```
```json
{
    "id": 1,
    "method": "foo",
    "params": [
        1,
        2
    ],
    "jsonrpc": "2.0"
}
```

</td>
<td valign="top">

Элементы массива биндятся в коллекцию
```cs
public async Task<IActionResult> Foo([FromParams(BindingStyle.Array)] List<int> data)
{
    // data[0] == 1
    // data[1] == 2

    // ...
}
```

</td>
</tr>


</table>

</details>


<details>
<summary>Использование разных бинжеров</summary>

Также можно попробовать значения по умолчанию, object, dynamic и собственную сериализацию...
```cs
public async Task<IActionResult> Foo1(object bar, dynamic baz, [FromParams(BindingStyle.Object)] Data data, [FromServices] ICustomService service, CancellationToken token)
{
    // bar, baz биндятся по умолчанию
    // data биндится согласно указанному поведению
    // service и token биндятся фреймворком

    // ...
}

public async Task<IActionResult> Foo2(int? bar, string baz = "default_value")
{
    // В запросе "params" может содержать nullable "bar" и полностью, а поле "baz" может полностью отсутствовать

    // ...
}
```

</details>

## Доступ к дополнительной информации

Вспомогательные методы для работы с объектами запросов/ответов JSON-RPC. Подробности см. в [Utils](utils).

<details>
<summary>Развернуть</summary>

Для удобства добавлено несколько extension-методов для `HttpContext`. Полезно для использования в мидлварях и фильтрах.

Получение объекта JSON-RPC вызова:
```cs
var call = HttpContext.GetJsonRpcCall();

var id = (call as UntypedRequest)?.Id;
var method = call.Method;
var parameters = call.Params
```

Получение сырого JSON-RPC вызова в виде `JsonDocument`:
```cs
var rawCall = HttpContext.GetRawJsonRpcCall();

Console.WriteLine(rawCall.RootElement);
```

Получение объекта JSON-RPC ответа:
```cs
var call = HttpContext.GetJsonRpcResponse();

var id = (call as UntypedResponse)?.Id;
var result = call.Result
```

Проверка, является ли текущий вызов частью batch-запроса:
```cs
var isBatch = HttpContext.JsonRpcRequestIsBatch();

if (isBatch)
{
    Console.WriteLine("This call is part of batch request!");
}
```

Ручная установка ответа. Осторожно: его могут перезаписать фильтры!
```cs
var response = new UntypedResponse(request.Id, result)

HttpContext.SetJsonRpcResponse(response);
```

</details>

## Ошибки и исключения

Разные способы вернуть ошибку из метода. Подробности см. в [Ошибки](errors).

<details>
<summary>Методы IJsonRpcErrorFactory</summary>

```cs
public class FailController : JsonRpcControllerBase
{
    private readonly IJsonRpcErrorFactory jsonRpcErrorFactory;
    public FailController(IJsonRpcErrorFactory jsonRpcErrorFactory) => this.jsonRpcErrorFactory = jsonRpcErrorFactory;

    public async Task<ActionResult<IError>> PredefinedError()
    {
        // ...
        return this.Ok(jsonRpcErrorFactory.InvalidParams("oops"));
        // или другие:
        //return this.Ok(jsonRpcErrorFactory.ParseError("oops"));
        //return this.Ok(jsonRpcErrorFactory.InvalidRequest("oops"));
    }
}
```

Ответ на запрос (вне зависимости от [`DetailedResponseExceptions`](configuration#DetailedResponseExceptions)):
```json
{
    "id": 1,
    "error": {
        "code": -32602,
        "message": "Invalid params",
        "data": "oops"
    },
    "jsonrpc": "2.0"
}
```

</details>

<details>
<summary>Любое исключение</summary>

```cs
public class FailController : JsonRpcControllerBase
{
    public async Task<IActionResult> ThrowException()
    {
        // ...
        throw new DivideByZeroException("test");
    }
}
```

Ответ на запрос (зависит от [`DetailedResponseExceptions`](configuration#DetailedResponseExceptions)):
<table>
<tr>
    <th>
        DetailedResponseExceptions = false
    </th>
    <th>
        DetailedResponseExceptions = true
    </th>
</tr>

<tr>
<td valign="top">

```json
{
    "id": 1,
    "error": {
        "code": -32000,
        "message": "Server error",
        "data": {
            "type": "System.DivideByZeroException",
            "message": "test",
            "details": null
        }
    },
    "jsonrpc": "2.0"
}
```

</td>
<td valign="top">

```json
{
    "id": 1,
    "error": {
        "code": -32000,
        "message": "Server error",
        "data": {
            "type": "System.DivideByZeroException",
            "message": "test",
            "details": "System.DivideByZeroException: test\r\n   at Application.Controllers.FailController.ThrowException() ... (and the rest of the stack trace) ..."
        }
    },
    "jsonrpc": "2.0"
}
```

</td>
</tr>
</table>

</details>

<details>
<summary>Создание ошибки с помощью фабрики IJsonRpcErrorFactory.Error</summary>

```cs
public record MyData(int Bar, string Baz);

public class FailController : JsonRpcControllerBase
{
    private readonly IJsonRpcErrorFactory jsonRpcErrorFactory;
    public FailController(IJsonRpcErrorFactory jsonRpcErrorFactory) => this.jsonRpcErrorFactory = jsonRpcErrorFactory;

    public async Task<ActionResult<IError>> Error()
    {
        // ...
        return this.Ok(jsonRpcErrorFactory.Error(123, "error with custom data", new MyData(456, "baz"));
    }
}
```

Ответ на запрос (вне зависимости от [`DetailedResponseExceptions`](configuration#DetailedResponseExceptions)):
```json
{
    "id": 1,
    "error": {
        "code": 123,
        "message": "error with custom data",
        "data": {
            "bar": 456,
            "baz": "baz"
        }
    },
    "jsonrpc": "2.0"
}
```

</details>

<details>
<summary>ActionResult с HTTP кодами ошибок</summary>

```cs
public record MyData(int Bar, string Baz);

public class FailController : JsonRpcControllerBase
{
    public async Task<IActionResult> MvcError()
    {
        // ...
        return this.BadRequest(new MyData(123, "baz"));
    }
}
```

Ответ на запрос (вне зависимости от [`DetailedResponseExceptions`](configuration#DetailedResponseExceptions)):
```json
{
    "id": 1,
    "error": {
        "code": -32602,
        "message": "Invalid params",
        "data": {
            "bar": 123,
            "baz": "baz"
        }
    },
    "jsonrpc": "2.0"
}
```

</details>

<details>
<summary>Оборачивание исключения вручную IJsonRpcErrorFactory.Exception</summary>

```cs
public class FailController : JsonRpcControllerBase
{
    private readonly IJsonRpcErrorFactory jsonRpcErrorFactory;
    public FailController(IJsonRpcErrorFactory jsonRpcErrorFactory) => this.jsonRpcErrorFactory = jsonRpcErrorFactory;

    public async Task<IActionResult> WrapExceptionManually()
    {
        // ...
        try
        {
            throw new DivideByZeroException("oops");
        }
        catch (Exception e)
        {
            var error = jsonRpcErrorFactory.Exception(e);
            return new ObjectResult(error);
        }

        return this.Ok();
    }
}
```

Ответ на запрос (зависит от [`DetailedResponseExceptions`](configuration#DetailedResponseExceptions)):
<table>
<tr>
    <th>
        DetailedResponseExceptions = false
    </th>
    <th>
        DetailedResponseExceptions = true
    </th>
</tr>

<tr>
<td valign="top">

```json
{
    "id": 1,
    "error": {
        "code": -32000,
        "message": "Server error",
        "data": {
            "type": "System.DivideByZeroException",
            "message": "oops",
            "details": null
        }
    },
    "jsonrpc": "2.0"
}
```

</td>
<td valign="top">

```json
{
    "id": 1,
    "error": {
        "code": -32000,
        "message": "Server error",
        "data": {
            "type": "System.DivideByZeroException",
            "message": "oops",
            "details": "System.DivideByZeroException: oops\r\n   at Application.Controllers.FailController.WrapExceptionManually() ... (and the rest of the stack trace) ..."
        }
    },
    "jsonrpc": "2.0"
}
```

</td>
</tr>
</table>

</details>

<details>
<summary>Оборачивание HTTP кода ответа вручную IJsonRpcErrorFactory.HttpError</summary>

```cs
public class FailController : JsonRpcControllerBase
{
    private readonly IJsonRpcErrorFactory jsonRpcErrorFactory;
    public FailController(IJsonRpcErrorFactory jsonRpcErrorFactory) => this.jsonRpcErrorFactory = jsonRpcErrorFactory;

    public async Task<ActionResult<IError>> WrapHttpErrorManually()
    {
        // ...
        var innerException = new DivideByZeroException("inner!");
        var e = new ArgumentException("message!", innerException);
        return this.Ok(jsonRpcErrorFactory.HttpError(500, e));
    }
}
```

Ответ на запрос (зависит от [`DetailedResponseExceptions`](configuration#DetailedResponseExceptions)):
<table>
<tr>
    <th>
        DetailedResponseExceptions = false
    </th>
    <th>
        DetailedResponseExceptions = true
    </th>
</tr>

<tr>
<td valign="top">

```json
{
    "id": 1,
    "error": {
        "code": -32603,
        "message": "Internal error",
        "data": {
            "type": "System.ArgumentException",
            "message": "message!",
            "details": null
        }
    },
    "jsonrpc": "2.0"
}
```

</td>
<td valign="top">

```json
{
    "id": 1,
    "error": {
        "code": -32603,
        "message": "Internal error",
        "data": {
            "type": "System.ArgumentException",
            "message": "message!",
            "details": "System.ArgumentException: message!\r\n ---> System.DivideByZeroException: inner!\r\n   --- End of inner exception stack trace ---"
        }
    },
    "jsonrpc": "2.0"
}
```

</td>
</tr>
</table>

</details>

<details>
<summary>Создание ошибки вручную</summary>

```cs
public record MyData(int Bar, string Baz);

public class FailController : JsonRpcControllerBase
{
    private readonly IJsonRpcErrorFactory jsonRpcErrorFactory;
    public FailController(IJsonRpcErrorFactory jsonRpcErrorFactory) => this.jsonRpcErrorFactory = jsonRpcErrorFactory;

    public async Task<ActionResult<IError>> ManuallyCreateError()
    {
        // ...
        var error = new Error<MyData>(123, "error with custom data", new MyData(456, "baz"));
        return this.Ok(error);
    }
}
```

Ответ на запрос (вне зависимости от [`DetailedResponseExceptions`](configuration#DetailedResponseExceptions)):
```json
{
    "id": 1,
    "error": {
        "code": 123,
        "message": "error with custom data",
        "data": {
            "bar": 456,
            "baz": "baz"
        }
    },
    "jsonrpc": "2.0"
}
```

</details>

<details>
<summary>Выбрасывание исключения с ошибкой с помощью throw и метода IError.AsException</summary>

```cs
public record MyData(int Bar, string Baz);

public class FailController : JsonRpcControllerBase
{
    private readonly IJsonRpcErrorFactory jsonRpcErrorFactory;
    public FailController(IJsonRpcErrorFactory jsonRpcErrorFactory) => this.jsonRpcErrorFactory = jsonRpcErrorFactory;

    public async Task<IActionResult> ThrowErrorAsException()
    {
        // ...
        var error = jsonRpcErrorFactory.Error(123, "error with custom data", new MyData(456, "baz"));
        throw error.AsException();
    }
}
```

Ответ на запрос (вне зависимости от [`DetailedResponseExceptions`](configuration#DetailedResponseExceptions)):
```json
{
    "id": 1,
    "error": {
        "code": 123,
        "message": "error with custom data",
        "data": {
            "bar": 456,
            "baz": "baz"
        }
    },
    "jsonrpc": "2.0"
}
```

</details>

<details>
<summary>Выбрасывание исключения с ошибкой из метода IError.ThrowAsException</summary>

```cs
public record MyData(int Bar, string Baz);

public class FailController : JsonRpcControllerBase
{
    private readonly IJsonRpcErrorFactory jsonRpcErrorFactory;
    public FailController(IJsonRpcErrorFactory jsonRpcErrorFactory) => this.jsonRpcErrorFactory = jsonRpcErrorFactory;

    public async Task<IActionResult> ThrowErrorAsException()
    {
        // ...
        var error = jsonRpcErrorFactory.Error(123, "error with custom data", new MyData(456, "baz"));
        error.ThrowAsException();
    }
}
```

Ответ на запрос (вне зависимости от [`DetailedResponseExceptions`](configuration#DetailedResponseExceptions)):
```json
{
    "id": 1,
    "error": {
        "code": 123,
        "message": "error with custom data",
        "data": {
            "bar": 456,
            "baz": "baz"
        }
    },
    "jsonrpc": "2.0"
}
```

</details>

<details>
<summary>Выбрасывание исключения с ошибкой вручную</summary>

```cs
public record MyData(int Bar, string Baz);

public class FailController : JsonRpcControllerBase
{
    private readonly IJsonRpcErrorFactory jsonRpcErrorFactory;
    public FailController(IJsonRpcErrorFactory jsonRpcErrorFactory) => this.jsonRpcErrorFactory = jsonRpcErrorFactory;

    public async Task<IActionResult> ThrowExceptionWithError()
    {
        // ...
        var error = jsonRpcErrorFactory.Error(123, "error with custom data", new MyData(456, "baz"));
        throw new JsonRpcErrorException(error);
    }
}
```

Ответ на запрос (вне зависимости от [`DetailedResponseExceptions`](configuration#DetailedResponseExceptions)):
```json
{
    "id": 1,
    "error": {
        "code": 123,
        "message": "error with custom data",
        "data": {
            "bar": 456,
            "baz": "baz"
        }
    },
    "jsonrpc": "2.0"
}
```

</details>

## Логирование запросов

<details>
<summary>Логируем запрос</summary>

```cs
app.UseJsonRpc().WithJsonRpcRequestLogging()
```

</details>
