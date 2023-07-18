# Сервер/Примеры

Здесь представлены примеры для различных сценариев. Типичные подробности, вроде HTTP заголовков, создания/запуска приложения и регистрации контроллеров опущены для простоты.

> Для деталей о более продвинутом использовании смотрите страницу [Конфигурация](configuration)

##  Запрос, Уведомление, Батч с настройками по умолчанию

Примеры базовых JSON-RPC вызовов с настройками по умолчанию
<details>
<summary>Развернуть</summary>

> `Program.cs`
```cs
builder.Services.AddJsonRpcServer();

app.UseJsonRpc();
```

> `EchoController.cs`
```cs
public class EchoController : JsonRpcControllerBase
{
    public string ToLower(string value) => value.ToLowerInvariant();
}
```

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

JSON-RPC Запрос
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

Стандартный ответ
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

JSON-RPC Уведомление
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

Ответ отсутствует согласно спецификации
```http
HTTP/1.1 200 OK
Content-Length: 0
```

</td>
</tr>

<tr>

<td valign="top">

JSON-RPC Батч
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

Ответы для всех вызовов кроме уведомлений
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

Небольшое нарушение протокола для возврата байтов, HTTP кодов ответа и т.п.
<details>
<summary>Развернуть</summary>

> `Program.cs`
```cs
builder.Services.AddJsonRpcServer(static options => options.AllowRawResponses = true);

app.UseJsonRpc();
```

> `DataController.cs`
```cs
public class DataController : JsonRpcControllerBase
{
    public IActionResult GetBytes(int count)
    {
        var bytes = Enumerable.Range(0, count).Select(static x => (byte) x).ToArray();
        return new FileContentResult(bytes, "application/octet-stream");
    }

    public IActionResult RedirectTo(string url) => RedirectPermanent(url);
}
```

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

JSON-RPC Батч
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

JSON-RPC Ошибка
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

Скрытие или предоставление информации об исключениях
<details>
<summary>Развернуть</summary>

> `Program.cs`
```cs
builder.Services.AddJsonRpcServer(static options => options.DetailedResponseExceptions = /* true или false */);

app.UseJsonRpc();
```

> `ErrorController.cs`
```cs
public class ErrorController : JsonRpcControllerBase
{
    public string Fail() => throw new NotImplementedException("exception message");
}
```

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

details равно null, если `DetailedResponseExceptions` равен **false**
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

Переопределение маршрутизации через глобальную настройку или атрибут
<details>
<summary>Развернуть</summary>

Все обработчики JSON-RPC должны иметь одинаковый префикс адреса (по умолчанию `/api/jsonrpc`), чтобы отличать их от REST запросов, если оба API используются в одном проекте. Если префикс не указан явно в адресе обработчика, то он будет добавлен автоматически. Для обработчиков, у которых адрес не указан вручную, префикс будет использоваться как полный адрес (без части `/controllerName`).

Изменение адреса по умолчанию и переопределение его для контроллера или метода:
> `Program.cs`
```cs
builder.Services.AddJsonRpcServer(static options => options.RoutePrefix = "/public_api");

app.UseJsonRpc();
```

> `UsersController.cs`
```cs
/* Переопределение через [Route] также доступно здесь */
public class UsersController : JsonRpcControllerBase
{
    public List<string> GetNames() => new() { "Alice", "Bob" };

    [Route("/admin_api")] // добавить пользователя в БД и вернуть ID
    public Guid Create(string name) => Guid.NewGuid();
}
```

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

Запрос Create на переопределенный адрес без установленного префикса
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

Запрос Create на переопределенный адрес с установленным префиксом
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

Изменение привязки поля `method` к контроллерам и методам. Поле `method` из запроса может быть отправлено в разных форматах в зависимости от глобальной настройки: как `controller.action` или как `action`. Также можно вручную установить значение через `JsonRpcMethodAttribute`.

<details>
<summary>Развернуть</summary>


> `Program.cs`
```cs
builder.Services.AddJsonRpcServer(static options => options.DefaultMethodStyle = /* JsonRpcMethodStyle.ControllerAndAction или JsonRpcMethodStyle.ActionOnly */);

app.UseJsonRpc();
```

> `EchoController.cs`
```cs
/* Переопределение через [JsonRpcMethodStyle] также доступно здесь */
public class EchoController : JsonRpcControllerBase
{
    /* Переопределение через [JsonRpcMethodStyle] или [JsonRpcMethod] также доступно здесь */
    public string ToLower(string value) => value.ToLowerInvariant();

    [JsonRpcMethod("to upper")]
    public string ToUpper(string value) => value.ToUpperInvariant();
}
```

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

Запрос с установленным вручную значением method (установлено через `JsonRpcMethodAttribute`)
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

Response from `EchoController.ToUpper`
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

Изменение настроек сериализации JSON по умолчанию или их переопределение для контроллера/метода. Смотрите страницу [Сериализация](serialization) для подробностей.

<details>
<summary>Развернуть</summary>

Обратите внимание как сериализация влияет на поля `params` и `method`.
> `Program.cs`
```cs
// Вы также можете использовать настройки из класса JsonRpcSerializerOptions
var jsonSerializerOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
builder.Services.AddJsonRpcServer(options => options.DefaultDataJsonSerializerOptions = jsonSerializerOptions);

// Провайдер настроек для использования в JsonRpcSerializerOptionsAttribute
builder.Services.AddSingleton<IJsonSerializerOptionsProvider, SnakeCaseJsonSerializerOptionsProvider>();

app.UseJsonRpc();
```

> `SimpleCalcController.cs`
```cs
/* Переопределение через [JsonRpcSerializerOptions] также доступно здесь */
public class SimpleCalcController : JsonRpcControllerBase
{
    public object SubtractIntegers(int firstValue, int secondValue) => new
    {
        firstValue,
        secondValue,
        firstMinusSecond = firstValue - secondValue
    };

    // ВАЖНО: SnakeCaseJsonSerializerOptionsProvider Должен быть зарегистрирован в DI как IJsonSerializerOptionsProvider
    [JsonRpcSerializerOptions(typeof(SnakeCaseJsonSerializerOptionsProvider))]
    public object AddIntegers(int firstValue, int secondValue) => new
    {
        firstValue,
        secondValue,
        firstPlusSecond = firstValue + secondValue
    };
}
```

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


## Привязка моделей

Изменение привязки поля `params` к аргументам метода. Смотрите [Привязка моделей](binding) для подробностей.

<details>
<summary>Поведение по умолчанию: params привязывается к аргументам метода. Значение поля params может быть [] или {} согласно спецификации</summary>

<table>
<tr>
    <td>
        Запрос
    </td>
    <td>
        Метод
    </td>
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

`params` привязывается к аргументам метода по именам
```cs
public void Foo(int bar, string baz)
{
    // bar == 1
    // baz == "test"
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

`params` привязывается к аргументам метода по индексам
```cs
public void Foo(int bar, string baz)
{
    // bar == 1
    // baz == "test"
}
```

</td>
</tr>


</table>
</details>


<details>
<summary>Привязка всего объекта params к одной модели, например, когда в модели много полей</summary>

<table>
<tr>
    <td>
        Запрос
    </td>
    <td>
        Метод
    </td>
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

`params` привязывается к одному аргументу метода
```cs
public record Data(int Bar, string Baz);

public void Foo([FromParams(BindingStyle.Object)] Data data)
{
    // data.Bar == 1
    // data.Baz == "test"
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

Ошибка, потому что элементы массива не могут быть привязаны к полям объекта
```cs
public record Data(int Bar, string Baz);

public void Foo([FromParams(BindingStyle.Object)] Data data)
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
<summary>Привязка всего массива params к одной коллекции, например, когда в запросе может быть неопределенное количество параметров</summary>

<table>
<tr>
    <td>
        Запрос
    </td>
    <td>
        Метод
    </td>
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

Ошибка, потому что поля объекта не могут быть привязаны к элементам массива
```cs
public void Foo([FromParams(BindingStyle.Array)] List<int> data)
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

Элементы массива привязываются к коллекции
```cs
public void Foo([FromParams(BindingStyle.Array)] List<int> data)
{
    // data[0] == 1
    // data[1] == 2
}
```

</td>
</tr>


</table>

</details>


<details>
<summary>Использование сразу нескольких источников</summary>

Также попробуйте значения по умолчанию, object, dynamic и собственную сериализацию...
```cs
public void Foo1(object bar, dynamic baz, [FromParams(BindingStyle.Object)] Data data, [FromServices] ICustomService service, CancellationToken token)
{
    // bar, baz привязываются по умолчанию
    // data привязывается согласно указанному поведению
    // service и token привязываются фреймворком
}

public void Foo2(int? bar, string baz = "default_value")
{
    // В запросе "params" может содержать nullable "bar" и полностью опустить поле "baz"
}
```

</details>

## Доступ к дополнительной информации

<details>
<summary>Развернуть</summary>

Для удобства добавлено несколько методов расширения для `HttpContext`. Полезно для использования в мидлварях и фильтрах.

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

Проверка, является ли текущий вызов частью батч запроса:
```cs
var isBatch = HttpContext.JsonRpcRequestIsBatch();

if (isBatch)
{
    Console.WriteLine("This call is part of batch request!");
}
```

Ручная установка ответа. Осторожно: он может быть позже перезаписан фильтрами!
```cs
var response = new UntypedResponse(request.Id, result)

HttpContext.SetJsonRpcResponse(response);
```

</details>

## Ошибки и исключения

Для начала посмотрите [Ошибки](errors).

<details>
<summary>Разные способы вернуть ошибку из метода</summary>

Рассмотрим методы в данном контроллере. Ниже представлены примеры результатов их работы. HTTP заголовки опущены, код ответа всегда `200 OK`.

```cs
public record MyData(int Bar, string Baz);

public class FailController : JsonRpcControllerBase
{
    private readonly IJsonRpcErrorFactory jsonRpcErrorFactory;
    public FailController(IJsonRpcErrorFactory jsonRpcErrorFactory) => this.jsonRpcErrorFactory = jsonRpcErrorFactory;
    // методы представлены в примерах ниже
}
```

<table>
<tr>
    <td>
        Метод
    </td>
    <td>
        Запрос без DetailedResponseExceptions
    </td>
    <td>
        Запрос с DetailedResponseExceptions
    </td>
</tr>

<tr>

<td valign="top">

```cs
public void ThrowException() =>
    throw new DivideByZeroException("test");
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

<tr>

<td valign="top">

```cs
public IError Error() =>
    jsonRpcErrorFactory.Error(123,
        "error with custom data",
        new MyData(456, "baz"));
```

</td>

<td valign="top">

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

</td>

<td valign="top">

без отличий

</td>
</tr>

<tr>

<td valign="top">

```cs
public IError PredefinedError()
{
    return jsonRpcErrorFactory.InvalidParams("oops");
    // или другие:
    //return jsonRpcErrorFactory.ParseError("oops");
    //return jsonRpcErrorFactory.InvalidRequest("oops");
}
```

</td>

<td valign="top">

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

</td>

<td valign="top">

без отличий

</td>
</tr>

<tr>

<td valign="top">

```cs
public IActionResult MvcError() =>
    this.BadRequest(new MyData(123, "baz"));
```

</td>

<td valign="top">

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

</td>

<td valign="top">

без отличий

</td>
</tr>

<tr>

<td valign="top">

```cs
public IActionResult WrapExceptionManually()
{
    try
    {
        throw new DivideByZeroException("oops");
    }
    catch (Exception e)
    {
        var error = jsonRpcErrorFactory.Exception(e);
        return new ObjectResult(error);
    }

    return Ok();
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

<tr>

<td valign="top">

```cs
public IError WrapHttpErrorManually()
{
    var innerException = new DivideByZeroException("inner!");
    var e = new ArgumentException("message!", innerException);
    return jsonRpcErrorFactory.HttpError(500, e);
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

<tr>

<td valign="top">

```cs
public IError ManuallyCreateError() =>
    new Error<MyData>(123,
        "error with custom data",
        new MyData(456, "baz"));
```

</td>

<td valign="top">

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

</td>

<td valign="top">

без отличий

</td>
</tr>

<tr>

<td valign="top">

```cs
public void ThrowErrorAsException()
{
    var error = jsonRpcErrorFactory.Error(123,
        "error with custom data",
        new MyData(456, "baz"));
    error.ThrowAsException();
}
```

</td>

<td valign="top">

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

</td>

<td valign="top">

без отличий

</td>
</tr>


</table>

</details>
