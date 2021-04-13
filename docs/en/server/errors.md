# Server/Errors

Error handling is difficult. Here is what library does and what you can do. Also see [errors and exceptions](examples?id=errors-and-exceptions) in examples.

## Return error manually

You can return `IError` object. For convenience, there is an `IErrorFactory` service,
which follows JSON Rpc spec (some predefined errors, checks for reserved error codes)
and hides exception details as configured in global options.

> `IErrorFactory` checks if data passed to any of methods is an `Exception` and creates `ExceptionInfo` instead.

## Return bad HTTP code

In controller class, you can call methods like `NotFound()` which result in `IActionResult` with some HTTP status code.
If the code is not 2xx, it will be converted with `IErrorFactory.HttpError`

## Exceptions

Exceptions thrown by action or filter are wrapped into JSON Rpc error responses with `IErrorFactory.Exception`

## Return custom error but keep controller return type

Imagine if you have clean controller signature like `public User GetUser(int id){}`.
Sometimes you need to return JSON Rpc errors with given code, message and data, like `-10, "user not found", {id: 1}`.

* Throwing regular exceptions won't help because they are wrapped into ServerError (they are treated as unexpected exceptions).
* You don't want to return `IActionResult` for some reason

Use `IError.ThrowAsResponseException` or throw `JsonRpcErrorResponseException` directly:

```cs
errorFactory.MethodNotFound("oops!").ThrowAsResponseException();
errorFactory.Exception(e).ThrowAsResponseException();
errorFactory.Error(-10, "user not found", new {id=1}).ThrowAsResponseException();
errorFactory.Error(-10, "users not found", new List<string>{"user1", "user2"}).ThrowAsResponseException();
throw new JsonRpcErrorResponseException(new Error<T>{...});
```

## Early pipeline exceptions

Exceptions thrown before it is known what action is going to be invoked are wrapped into JSON Rpc error responses, but serialized differently,
because there it is not known yet which serializer to use. See [Serialization](/docs/en/server/serialization).

## Exceptions thrown by middleware before `JsonRpcMiddleware`

Simply nothing can be done, so expect what your application is configured to do.