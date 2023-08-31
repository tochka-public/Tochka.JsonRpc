# Server/Errors

Error handling is difficult. Here is what library does and what you can do. Also see [Examples#Errors and Exceptions](examples#errors-and-exceptions) in examples.

## Return error manually

Return `IError` object from action in case of failures. For convenience, there is an `IJsonRpcErrorFactory` service,
which follows JSON-RPC spec (some predefined errors, checks for reserved error codes)
and hides exception details as configured in global options.

Best way to use it is the **same as with REST** controllers: action return type should be `IActionResult`/`IActionResult<T>`/`ObjectResult<T>`.

[Examples](examples#Errors-and-exceptions):
 - IJsonRpcErrorFactory methods
 - Creating error using factory IJsonRpcErrorFactory.Error
 - Creating error manually

> `IJsonRpcErrorFactory` checks if data passed to any of its methods is an `Exception` and creates `ExceptionInfo` instead, because exceptions are not always serializable to JSON

## Return bad HTTP code

In controller class, you can call methods like `NotFound()` which result in `IActionResult` with some HTTP status code.
If the code is 4xx or 5xx, it will be converted with `IJsonRpcErrorFactory.HttpError`.

[Examples](examples#Errors-and-exceptions):
 - ActionResult with HTTP error codes
 - Wrapping HTTP status code manually IJsonRpcErrorFactory.HttpError

## Exceptions

Exceptions thrown by action or filter are wrapped into JSON-RPC error responses with `IJsonRpcErrorFactory.Exception`.

You can customize wrapping logic by changing `IJsonRpcErrorFactory` implementation in DI or creating custom `IExceptionFilter` which converts exception to error and stores it in `Result`, similar to [`JsonRpcExceptionFilter`](https://github.com/tochka-public/Tochka.JsonRpc/blob/master/src/Tochka.JsonRpc.Server/Filters/JsonRpcExceptionFilter.cs).

[Examples](examples#Errors-and-exceptions):
 - Any exception
 - Wrapping exception manually IJsonRpcErrorFactory.Exception

## Return custom error but keep controller return type

Imagine if you have clean controller signature like `public User GetUser(int id) {}`.
Sometimes you need to return JSON-RPC errors with given code, message and data, like `-10, "user not found", {id: 1}`. Throwing regular exceptions won't help because they are wrapped into `ServerError` (they are treated as unexpected exceptions).

Use `IError.ThrowAsException`, `IError.AsException` or throw `JsonRpcErrorException` directly:

```cs
errorFactory.MethodNotFound("oops!").ThrowAsException();
errorFactory.Exception(e).AsException();
errorFactory.Error(-10, "user not found", new { id = 1 }).ThrowAsException();
errorFactory.Error(-10, "users not found", new List<string> { "user1", "user2" }).AsException();
throw new JsonRpcErrorException(new Error<T>(...));
```

This approach is not recommended because there is no reason to avoid `IActionResult`/`IActionResult<T>`/`ObjectResult<T>` in method signatures.

[Examples](examples#Errors-and-exceptions):
 - Throwing exception with error using throw and method IError.AsException
 - Throwing exception with error from method IError.ThrowAsException
 - Throwing exception with error manually

## Early pipeline exceptions

Exceptions thrown before it is known what action is going to be invoked are wrapped into JSON-RPC error responses, but serialized differently, because it is not known yet which serializer to use. See [Serialization](serialization).

## Exceptions thrown by middleware before `JsonRpcMiddleware`

Simply nothing can be done, so expect what your application is configured to do.
