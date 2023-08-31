# Server/Configuration

There are two ways to configure library behavior: `Program.cs` options lambda and attributes for controllers/actions/parameters.

*It is also possible to replace or inherit services used to process requests, but this is not described here.*

> Check [Examples](examples) page to see how options affect requests/responses

## Program.cs options

`.AddJsonRpcServer()` supports an overload to set up `JsonRpcServerOptions`.

```cs
builder.Services.AddJsonRpcServer(static options =>
{
    options.AllowRawResponses = false;
    options.DetailedResponseExceptions = false;
    options.DefaultMethodStyle = JsonRpcMethodStyle.ControllerAndAction;
    options.DefaultDataJsonSerializerOptions = JsonRpcSerializerOptions.SnakeCase;
    options.HeadersJsonSerializerOptions = JsonRpcSerializerOptions.Headers;
    options.RoutePrefix = "/api/jsonrpc";
});
```

### AllowRawResponses

```cs
builder.Services.AddJsonRpcServer(static options => options.AllowRawResponses = /* true or false */);
```

> Default: `false`

> If `true`, server is allowed to return non JSON-RPC responses, like HTTP redirects, binary content, etc.

[Usage examples](examples#AllowRawResponses).

ASP.Net Core actions/filters return `IActionResult` with HTTP code, content, etc.
We are trying to convert them to response which is always `200 OK` and serialize any content to JSON-RPC response.
But this breaks some perfectly valid scenarios.

For example, if you use authentication, request without cookies will be rejected by `AuthenticationFilter` with `ChallengeResult` which is serialized as
HTTP redirect. This breaks JSON-RPC protocol, but is surely useful. If you want authentication to **just work**, set `AllowRawResponses = true`.

Another use case: you may want to return HTTP response with arbitrary non-JSON content. This surely breaks protocol,
but can help avoid unneeded serialization of large files, for example.

Currently recognized `IActionResult` types which are convertible to JSON-RPC response:

* `ObjectResult`: when action returns regular object
* `StatusCodeResult`: eg. when you call `NotFound()`
* `EmptyResult`: when action has `void` return type

For all other results:

* When `false`, server responds with JSON-RPC server error
* When `true`, let framework interpret it as with normal REST controllers

**Note:** Batches will **break** if this option is enabled and one of requests returns non-JSON data! See [Batches](batches).

### DetailedResponseExceptions

```cs
builder.Services.AddJsonRpcServer(static options => options.DetailedResponseExceptions = /* true or false */);
```

> Default: `false`

> If `true`, exceptions are serialized with their `.ToString()` which includes stack trace

[Usage examples](examples#DetailedResponseExceptions).

Exceptions thrown by this library, middleware, or user code, are intercepted and serialized as JSON-RPC error response with `ExceptionInfo` object.

`ExceptionInfo` always has exception message and exception type name.
If this is `true`, it will also have exception's `.ToString()` with all the details.

You may not want this enabled in production environment.

### DefaultMethodStyle

```cs
builder.Services.AddJsonRpcServer(static options => options.DetailedResponseExceptions = /* JsonRpcMethodStyle.ControllerAndAction or JsonRpcMethodStyle.ActionOnly */);
```

> Default: `JsonRpcMethodStyle.ControllerAndAction`

> Rules how JSON-RPC `method` property is matched to controllers/actions

[Usage examples](examples#Method) and [details](serialization#Matching-method-name-to-controlleraction-names).

* `ControllerAndAction`: treat `method` as `controller.action`. Values like `foo.bar` are matched to `FooController.Bar`
* `ActionOnly`: treat `method` as `action`. Values like `bar` are matched to `Bar` action in any JsonRpcController

Serialization of names is handled by `DataJsonSerializerOptions`, see below and [Serialization](serialization) for more info.

It can be overridden by `JsonRpcMethodStyleAttribute` or ignored if custom method name is defined with `JsonRpcMethodAttribute`.

### DefaultDataJsonSerializerOptions

```cs
// you can also use predefined options from JsonRpcSerializerOptions class
var jsonSerializerOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
builder.Services.AddJsonRpcServer(options => options.DefaultDataJsonSerializerOptions = jsonSerializerOptions);
```

> Default: `JsonRpcSerializerOptions.SnakeCase`

> `JsonSerializerOptions` for serialization of `params` and `method` and deserialization of `result` or `error.data`

[Usage examples](examples#Serialization) and [details](serialization).

You can serialize **content** differently from JSON-RPC "header" object.
For typical use cases, there are `JsonRpcSerializerOptions.SnakeCase` and `JsonRpcSerializerOptions.CamelCase` in `Tochka.JsonRpc.Common` package.

It can be overridden by `JsonRpcSerializerOptionsAttribute` by using implementation of `IJsonSerializerOptionsProvider` interface registered in DI

### HeadersJsonSerializerOptions

```cs
// you can also use predefined options from JsonRpcSerializerOptions class
var jsonSerializerOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
builder.Services.AddJsonRpcServer(options => options.HeadersJsonSerializerOptions = jsonSerializerOptions);
```

> Default: `JsonRpcSerializerOptions.Headers`

> `JsonSerializerOptions` for serialization/deserialization of JSON-RPC "headers": `id`, `jsonrpc`, etc.

[Details](serialization).

Changing this not recommended, because request/response "header" object format is fixed and does not imply any changes.

### RoutePrefix

```cs
builder.Services.AddJsonRpcServer(static options => options.RoutePrefix = "/public_api");
```

> Default: `JsonRpcConstants.DefaultRoutePrefix` which is `"/api/jsonrpc"`

> This is the default route prefix for all controllers/actions inherited from `JsonRpcControllerBase`

[Usage examples](examples#Routes) and [details](routing).

All JSON-RPC handlers must have same route prefix to distinguish them from REST when you use both APIs in same project. If prefix is not defined explicitly in handler's route, it will be added automatically. For handlers without manually defined route, prefix will be used as full route (without `/controllerName` part).

Route can be overridden with framework's `RouteAttribute` like usual, and global prefix will be added if custom route doesn't start with it.

* Should start with `/`
* Prefix can be set to `"/"` to get rid of it
* Templates are supported (see [Routing#Route templates](routing#Route-templates))

## Attributes

### JsonRpcSerializerOptionsAttribute

Used with:
 - controllers
 - methods

> Override [`DefaultDataJsonSerializerOptions`](#DefaultDataJsonSerializerOptions) on any controller/action.

[Usage examples](examples#Serialization) and [details](serialization#IJsonSerializerOptionsProvider).

### JsonRpcMethodStyleAttribute

Used with:
 - controllers
 - methods

> Override [`DefaultMethodStyle`](#DefaultMethodStyle) on any controller/action.

[Usage examples](examples#Method) and [details](serialization#Matching-method-name-to-controlleraction-names).

### JsonRpcMethodAttribute

Used with:
 - methods

> Define custom `method` value on any action, ignoring [`DefaultMethodStyle`](#DefaultMethodStyle) and [`JsonRpcMethodStyleAttribute`](#JsonRpcMethodStyleAttribute).

[Usage examples](examples#Method) and [details](serialization#Matching-method-name-to-controlleraction-names).

### FromParamsAttribute

Used with:
 - method arguments

> Override default parameter binding behavior which is `BindingStyle.Default`

Change how JSON-RPC `params` are bound to action arguments.

[Usage examples](examples#Binding) and [details](binding).
