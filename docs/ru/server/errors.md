# Сервер/Ошибки

Обрабатывать ошибки - сложно. Здесь описано, что для этого делает библиотека, и чем можно воспользоваться вручную. Также см. [Примеры#Ошибки и исключения](examples?id=Ошибки-и-исключения).

## Возврат ошибок вручную

Можно возвращать объект `IError` из методов. Для удобства есть сервис `IJsonRpcErrorFactory`,
который реализует спецификацию JSON-RPC (ошибки описанные в протоколе, проверки для зарезервированных кодов)
и прячет детали исключений, если выставлены соответствующие глобальные настройки.

Лучше всего делать **аналогично REST**: метод должен возвращать `IActionResult`/`IActionResult<T>`/`ObjectResult<T>`.

> `IJsonRpcErrorFactory` проверяет, не передали ли в его методы объект-наследник `Exception`, и подменяет исключения на `ExceptionInfo`. Потому что исключения не всегда сериализуются в JSON

## Возврат "плохого" HTTP статус-кода

В классах контроллера есть стандартные методы типа `NotFound()`, которые возвращают `IActionResult` с определенным HTTP статус-кодом.
Если код 4xx или 5xx, то он будет обернут в response с полем error с помощью `IJsonRpcErrorFactory.HttpError`.

## Исключения

Исключения, выброшенные из метода или фильтров, оборачиваются в JSON-RPC response с полем error с помощью `IJsonRpcErrorFactory.Exception`.

Можно переопределить логику создания ошибок, если подменить реализацию `IJsonRpcErrorFactory` в DI. Другой вариант - создать свой `IExceptionFilter`, который будет конвертировать исключение в ошибку и складывать в `Result`, аналогично [`JsonRpcExceptionFilter`](https://github.com/tochka-public/Tochka.JsonRpc/blob/master/src/Tochka.JsonRpc.Server/Filters/JsonRpcExceptionFilter.cs).

## Возврат ошибки при сохранении возвращаемого значения

Бывает, нужно сохранить чистую сигнатуру метода, например, `public User GetUser(int id) {}`.
При этом может потребоваться вернуть JSON-RPC ошибку с заданным кодом, сообщением и данными, например, `-10, "user not found", {id: 1}`. Выкидывание обычных исключений не поможет, так как они будут обернуты в `ServerError` (как необработанное исключение).

Для этого есть `IError.ThrowAsException`. Еще можно выбросить исключение `JsonRpcErrorException` напрямую:

```cs
errorFactory.MethodNotFound("oops!").ThrowAsException();
errorFactory.Exception(e).ThrowAsException();
errorFactory.Error(-10, "user not found", new { id = 1 }).ThrowAsException();
errorFactory.Error(-10, "users not found", new List<string> { "user1", "user2" }).ThrowAsException();
throw new JsonRpcErrorException(new Error<T>(...));
```

Такой подход не рекомендуется, потому что нет причин отказываться от `IActionResult`/`IActionResult<T>`/`ObjectResult<T>` в сигнатурах методов.

## Ранние исключения в пайплайне

Исключения, выброшенные до того, как стало известно, какой метод будет вызван, также оборачиваются в JSON-RPC ответ с ошибкой. Но сериализуются по-другому, так как еще не известно, какие настройки сериализации использовать. См. [Сериализация](serialization).

## Исключения выброшенные мидлварями до `JsonRpcMiddleware`

С этим ничего нельзя сделать, поэтому результат может быть любой, смотря как настроено приложение.
