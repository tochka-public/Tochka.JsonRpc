# Сервер/Конфигурация

Есть два способа настроить поведение библиотеки: лямбда с опциями в `Program.cs` и атрибуты для контроллеров/методов/параметров.

*Еще можно заменить сервисы для обработки запросов или унаследоваться от них, но здесь такой способ не описан.*

> См. [Примеры](examples), чтобы увидеть, как настройки влияют на запросы/ответы

## Program.cs options

У `.AddJsonRpcServer()` есть перегрузка с возможностью настроить `JsonRpcServerOptions`.

```cs
builder.Services.AddJsonRpcServer(static options =>
{
    options.AllowRawResponses = false;
    options.DetailedResponseExceptions = false;
    options.DefaultMethodStyle = JsonRpcMethodStyle.ControllerAndAction;
    options.DefaultDataJsonSerializerOptions = JsonRpcSerializerOptions.SnakeCase;
    options.HeadersJsonSerializerOptions = JsonRpcSerializerOptions.Headers;
    options.RoutePrefix = "/api/jsonrpc";
    options.LogExceptions = true;
});
```

### AllowRawResponses

```cs
builder.Services.AddJsonRpcServer(static options => options.AllowRawResponses = /* true или false */);
```

> Значение по умолчанию: `false`

> Если `true`, то сервер может возвращать ответы, нарушающие протокол JSON-RPC. Например, HTTP-редиректы, бинарные данные и тп.

[Примеры использования](examples#AllowRawResponses).

Методы/фильтры ASP.Net Core возвращают `IActionResult` с HTTP кодом, контентом и тд.
Библиотека пытается конвертировать их в ответ `200 OK` и сериализовать контент в формат JSON-RPC.
Но из-за такого подхода ломаются некоторые вполне нормальные сценарии.

Например, если используется аутентификация, `AuthenticationFilter` отбросит запрос без нужных cookies и вернет `ChallengeResult`, который сериализуется как HTTP редирект. Это ломает протокол JSON-RPC, но бывает полезно. Если нужно, чтоб аутентификация **просто работала**, ставим `AllowRawResponses = true`.

Другой кейс: может понадобиться вернуть HTTP ответ с произвольным не-JSON контентом. Это, естественно, нарушает протокол. Но может избавить от лишней сериализации больших файлов.

На данный момент, распознаются и приводятся в JSON-RPC формат следующие `IActionResult`:

* `ObjectResult`: когда метод возвращает объект
* `StatusCodeResult`: например, когда метод возвращает `NotFound()`
* `EmptyResult`: когда метод имеет сигнатуру `void`

Для всех остальных результатов:

* Если `false`, сервер отвечает JSON-RPC ошибкой сервера
* Если `true`, позволяет фреймворку обработать его как ответ обычного REST контроллера.

**Примечание:** Batch-ответы **ломаются**, если эта настройка включена и один из ответов содержит не-JSON данные! См. [Batches](batches).

### DetailedResponseExceptions

```cs
builder.Services.AddJsonRpcServer(static options => options.DetailedResponseExceptions = /* true или false */);
```

> Значение по умолчанию: `false`

> Если `true`, исключения сериализуются вместе с их `.ToString()`, который включает стек трейс

[Примеры использования](examples#DetailedResponseExceptions).

Исключения, вызванные этой библиотекой, мидлварями или пользовательским кодом, перехватываются и сериализуются как JSON-RPC response с полем error и объектом `ExceptionInfo`.

`ExceptionInfo` всегда содержит сообщение и тип исключения.
Если эта настройка `true`, то оно также будет содержать `.ToString()` исключения со всеми подробностями.

Не рекомендуется включать на продакшене.

### DefaultMethodStyle

```cs
builder.Services.AddJsonRpcServer(static options => options.DetailedResponseExceptions = /* JsonRpcMethodStyle.ControllerAndAction или JsonRpcMethodStyle.ActionOnly */);
```

> Значение по умолчанию: `JsonRpcMethodStyle.ControllerAndAction`

> Управление тем, как JSON-RPC поле `method` сравнивается с контроллерами/методами

[Примеры использования](examples#Method) и [подробности](serialization#Сравнение-метода-из-запроса-и-имени-контроллераметода).

* `ControllerAndAction`: `method` интерпретируется как `controller.action`. Значение `foo.bar` будет соответсововать `FooController.Bar`
* `ActionOnly`: `method` интерпретируется как `action`. Значение `bar` будет соответствовать методу `Bar` в любом контроллере

Сериализация имен делается через `DataJsonSerializerOptions`, подробности см. ниже и в [Сериализация](serialization).

Можно переопределить с помощью `JsonRpcMethodStyleAttribute`. Можно вообще проигнорировать, если задать имя метода вручную с помощью `JsonRpcMethodAttribute`.

### DefaultDataJsonSerializerOptions

```cs
// Еще можно использовать настройки из класса JsonRpcSerializerOptions
var jsonSerializerOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
builder.Services.AddJsonRpcServer(options => options.DefaultDataJsonSerializerOptions = jsonSerializerOptions);
```

> Значение по умолчанию: `JsonRpcSerializerOptions.SnakeCase`

> `JsonSerializerOptions` для сериализации полей `params` и `method`, а также десериализации полей `result` и `error.data`

[Примеры использования](examples#Сериализация) и [подробности](serialization).

**Пользовательские данные** сериализуется отдельно от "заголовков" JSON-RPC объекта.
Для типовых сценариев в пакете `Tochka.JsonRpc.Common` есть `JsonRpcSerializerOptions.SnakeCase` и `JsonRpcSerializerOptions.CamelCase`.

Можно переопределить с помощью `JsonRpcSerializerOptionsAttribute`, если использовать реализацию интерфейса `IJsonSerializerOptionsProvider`, зарегистрированную в DI.

### HeadersJsonSerializerOptions

```cs
// Еще можно использовать настройки из класса JsonRpcSerializerOptions
var jsonSerializerOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
builder.Services.AddJsonRpcServer(options => options.HeadersJsonSerializerOptions = jsonSerializerOptions);
```

> Значение по умолчанию: `JsonRpcSerializerOptions.Headers`

> `JsonSerializerOptions` для сериализации/десериализации JSON-RPC "заголовков": `id`, `jsonrpc`, и тд.

[Подробности](serialization).

Не рекомендуется менять, так как объект "заголовков" запроса/ответа имеет фиксированный формат и не подразумевает каких-либо изменений.

### RoutePrefix

```cs
builder.Services.AddJsonRpcServer(static options => options.RoutePrefix = "/public_api");
```

> Значение по умолчанию: `JsonRpcConstants.DefaultRoutePrefix` равное `"/api/jsonrpc"`

> Общий префикс route для всех контроллеров/методов, унаследованных от `JsonRpcControllerBase`

[Примеры использования](examples#Маршрутизация) и [подробности](routing).

Когда в одном проекте используются и JSON-RPC, и REST, нужен какой-то общий префикс для адреса, чтобы отличать JSON-RPC методы. Если префикс не указан явно в адресе метода, то он будет добавлен автоматически. Для методов, у которых адрес не указан вручную, префикс будет использоваться как полный адрес (без части `/controllerName`).

Route может быть переопределен с помощью стандартного `RouteAttribute` из фреймворка. Глобальный префикс будет добавлен к нему в начало, если заданный вручную route еще не содержит этот префикс.

* Должен начинаться с `/`
* Может быть установлено значение `"/"`, чтобы избавиться от него
* Поддерживает шаблонизацию (см. [Маршрутизация#Шаблонизация route](routing#Шаблонизация-route))

### LogExceptions

```cs
builder.Services.AddJsonRpcServer(static options => options.LogExceptions = /* true или false */);
```

> Значение по умолчанию: `true`

> Если `true`, все исключения будут логироваться с уровнем Error

[Примеры использования](examples#Логирование).

Исключения, вызванные этой библиотекой, мидлварями или пользовательским кодом, перехватываются и сериализуются как JSON-RPC response в `IExceptionFilter`. Из-за этого, фильтр - последнее место, где можно получить доступ к объекту исключения.

Если требуется логировать только определенные типы исключений, можно установить эту настройку в `false` и реализовать свой `IExceptionFilter` для логирования (см. [Примеры#Логирование > Определенные исключения](examples#Логирование)).

## Атрибуты

### JsonRpcSerializerOptionsAttribute

Вешается на:
 - контроллеры
 - методы

> Переопределение [`DefaultDataJsonSerializerOptions`](#DefaultDataJsonSerializerOptions) для контроллера/метода.

[Примеры использования](examples#Сериализация) и [подробности](serialization#IJsonSerializerOptionsProvider).

### JsonRpcMethodStyleAttribute

Вешается на:
 - контроллеры
 - методы

> Переопределение [`DefaultMethodStyle`](#DefaultMethodStyle) для контроллера/метода.

[Примеры использования](examples#Method) и [подробности](serialization#Сравнение-метода-из-запроса-и-имени-контроллераметода).

### JsonRpcMethodAttribute

Вешается на:
 - методы

> Определение значения поля `method` вручную для любого метода, игнорирует [`DefaultMethodStyle`](#DefaultMethodStyle) и [`JsonRpcMethodStyleAttribute`](#JsonRpcMethodStyleAttribute).

[Примеры использования](examples#Method) и [подробности](serialization#Сравнение-метода-из-запроса-и-имени-контроллераметода).

### FromParamsAttribute

Вешается на:
 - аргументы метода

> Переопределение дефолтного биндинга моделей в параметры (`BindingStyle.Default`)

Изменение того, как JSON-RPC поле `params` биндится в аргументы метода.

[Примеры использования](examples#Binding) и [подробности](binding).
