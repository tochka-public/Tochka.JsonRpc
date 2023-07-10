# Server/Errors

Error handling is difficult. Here is what library does and what you can do. Also see [errors and exceptions](examples?id=errors-and-exceptions) in examples.

## Return error manually

You can return `IError` object. For convenience, there is an `IJsonRpcErrorFactory` service,
which follows JSON Rpc spec (some predefined errors, checks for reserved error codes)
and hides exception details as configured in global options.

> `IJsonRpcErrorFactory` checks if data passed to any of methods is an `Exception` and creates `ExceptionInfo` instead.

## Return bad HTTP code

In controller class, you can call methods like `NotFound()` which result in `IActionResult` with some HTTP status code.
If the code is 4xx or 5xx, it will be converted with `IJsonRpcErrorFactory.HttpError`

## Exceptions

Exceptions thrown by action or filter are wrapped into JSON Rpc error responses with `IJsonRpcErrorFactory.Exception`

You can customize wrapping logic by changing `IJsonRpcErrorFactory` implementation in DI or creating custom `IExceptionFilter` which converts exception to error and stores it in `Result`, similar to [`JsonRpcExceptionFilter`](https://github.com/tochka-public/Tochka.JsonRpc/blob/master/src/Tochka.JsonRpc.Server/Filters/JsonRpcExceptionFilter.cs).

## Return custom error but keep controller return type

Imagine if you have clean controller signature like `public User GetUser(int id) {}`.
Sometimes you need to return JSON Rpc errors with given code, message and data, like `-10, "user not found", {id: 1}`.

* Throwing regular exceptions won't help because they are wrapped into ServerError (they are treated as unexpected exceptions).
* You don't want to return `IActionResult` for some reason

Use `IError.ThrowAsException` or throw `JsonRpcErrorException` directly:

```cs
errorFactory.MethodNotFound("oops!").ThrowAsException();
errorFactory.Exception(e).ThrowAsException();
errorFactory.Error(-10, "user not found", new { id = 1 }).ThrowAsException();
errorFactory.Error(-10, "users not found", new List<string> { "user1", "user2" }).ThrowAsException();
throw new JsonRpcErrorException(new Error<T>(...));
```

## Early pipeline exceptions

Exceptions thrown before it is known what action is going to be invoked are wrapped into JSON Rpc error responses, but serialized differently,
because there it is not known yet which serializer to use. See [Serialization](/docs/en/server/serialization).

## Exceptions thrown by middleware before `JsonRpcMiddleware`

Simply nothing can be done, so expect what your application is configured to do.
