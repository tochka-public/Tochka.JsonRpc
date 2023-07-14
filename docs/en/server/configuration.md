# Server/Configuration

There are two ways to configure library behavior: `Program.cs` options lambda and attributes for controllers/actions/parameters.

*It is also possible to replace or inherit services used to process requests, but this is not described here.*

> Check [Examples](examples) page to see how options affect requests/responses

## Startup options

`.AddJsonRpcServer()` supports an overload to set up `JsonRpcServerOptions`.

```cs
builder.Services.AddJsonRpcServer(static options =>
{
    options.AllowRawResponses = true;
    options.RoutePrefix = "/api/test";
});
```

### AllowRawResponses

> Default: `false`

> If `true`, server is allowed to return non JSON Rpc responses, like HTTP redirects, binary content, etc.

ASP.Net Core actions/filters return `IActionResult` with HTTP code, content, etc.
We are trying to convert them to response which is always `200 OK` and serialize any content to JSON Rpc response.
But this breaks some perfectly valid scenarios.

For example, if you use authentication, request without cookies will be rejected by `AuthenticationFilter` with `ChallengeResult` which is serialized as
HTTP redirect. This breaks JSON Rpc protocol, but is surely useful. If you want authentication to **just work**, set `AllowRawResponses = true`.

Another use case: you may want to return HTTP response with arbitrary non-JSON content. This surely breaks protocol,
but can help avoid unneeded serialization of large files, for example.

Currently recognized `IActionResult` types which are convertible to JSON Rpc response:

* `ObjectResult`: when action returns regular object
* `StatusCodeResult`: eg. when you call `NotFound()`
* `EmptyResult`: when action has `void` return type

For all other results:

* When `false`, server responds with JSON Rpc server error
* When `true`, let framework interpret it as with normal REST controllers

**Note:** Batches will **break** if this option is enabled and one of requests returns non-json data! See [Batches](batches).

### DetailedResponseExceptions

> Default: `false`

> If `true`, exceptions are serialized with their `.ToString()` which includes stack trace

Exceptions thrown by this library, middleware, or user code, are intercepted and serialized as JSON Rpc error response with `ExceptionInfo` object.

`ExceptionInfo` always has exception message and exception type name.
If this is `true`, it will also have exception's `.ToString()` with all the details.

You may not want this enabled in production environment.

### DefaultMethodStyle

> Default: `JsonRpcMethodStyle.ControllerAndAction`

> Rules how JSON Rpc `method` property is matched to controllers/actions

* `ControllerAndAction`: treat `method` as `controller.action`. Values like `foo.bar` are matched to `FooController.Bar`
* `ActionOnly`: treat `method` as `action`. Values like `bar` are matched to `Bar` action in any JsonRpcController

Serialization of names is handled by `DataJsonSerializerOptions`, see below and [Serialization](serialization) for more info.

It can be overridden by `JsonRpcMethodStyleAttribute` or ignored if custom method name is defined with `JsonRpcMethodAttribute`

### DefaultDataJsonSerializerOptions

> Default: `JsonRpcSerializerOptions.SnakeCase`

> `JsonSerializerOptions` for serialization of `params` and `method` and deserialization of `result` or `error.data`

You can serialize **content** differently from JSON Rpc "header" object.
For typical use cases, there are `JsonRpcSerializerOptions.SnakeCase` and `JsonRpcSerializerOptions.CamelCase` in `Tochka.JsonRpc.Common` package.

See [Serialization](serialization) for usage details.

It can be overridden by `JsonRpcSerializerOptionsAttribute` by using implementation of `IJsonSerializerOptionsProvider` interface registered in DI

### HeadersJsonSerializerOptions

> Default: `JsonRpcSerializerOptions.Headers`

> `JsonSerializerOptions` for serialization/deserialization of JSON Rpc "headers": `id`, `jsonrpc`, etc.

Changing this not recommended, because request/response "header" object format is fixed and does not imply any changes.

### RoutePrefix

> Default: `JsonRpcConstants.DefaultRoutePrefix` which is `"/api/jsonrpc"`

> This is the default route prefix for all controllers/actions inherited from `JsonRpcControllerBase`.

All JSON Rpc handlers must have same route prefix to distinguish them from REST when you use both APIs in same project. If prefix is not defined explicitly in handler's route, it will be added automatically. For handlers without manually defined route, prefix will be used as full route (without `/controllerName` part).

Route can be overridden with framework's `RouteAttribute` like usual, and global prefix will be added if custom route doesn't start with it.

* Should start with `/`
* Prefix can be set to `"/"` to get rid of it

## Attributes

### JsonRpcSerializerOptionsAttribute

> Override `DefaultDataJsonSerializerOptions` on any controller/action. See details above for `IJsonSerializerOptionsProvider` and [Serialization](serialization).

### JsonRpcMethodStyleAttribute

> Override `DefaultMethodStyle` on any controller/action. See details above for `DefaultMethodStyle`.

### JsonRpcMethodAttribute

> Define custom `method` value on any action, ignoring `DefaultMethodStyle` and `JsonRpcMethodStyleAttribute`. See details above for `DefaultMethodStyle`.

### FromParamsAttribute

> Override default parameter binding behavior which is `BindingStyle.Default`

Change how JSON Rpc `params` are bound to action arguments. See [Binding](binding).
