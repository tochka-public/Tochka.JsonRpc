# Server/Configuration

There are two ways to configure library behavior: `Startup` options lambda and attributes for controllers/actions/parameters.
*It is also possible to replace or inherit services used to process requests, but this is not described here.*

> Check [Examples](examples.md) page to see how options affect requests/responses

## Startup options

`.AddJsonRpcServer()` supports an overload to set up `JsonRpcOptions`.

```cs
    services.AddMvc()
        .AddJsonRpcServer(options => {
            options.AllowRawResponses = true;
            options.DefaultMethodOptions.Route = "/api/test";
        });
```

### JsonRpcOptions

#### AllowRawResponses

> Default: `false`

> If `true`, server is allowed to return non JSON Rpc responses, like HTTP redirects, 400 and 500 errors, binary content, etc.

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

**Note:** Batches will **break** if this option is enabled and one of requests returns non-json data! See [Batches](batches.md).

#### DetailedResponseExceptions

> Default: `false`

> If `true`, exceptions are serialized with their `.ToString()` which includes stack trace

Exceptions thrown by this library, middleware, or user code, are intercepted and serialized as JSON Rpc error response with `ExceptionInfo` object.

`ExceptionInfo` always has exception message, exception type name, may have internal HTTP status code.
If this is `true`, it will also have exception's `.ToString()` with all the details.

You may not want this enabled in production environment.

#### BatchHandling

> Default: `BatchHandling.Sequential`

> Currently batches can be handled only sequentially. This is planned to be extended with `Parallel` or something like that.

#### DefaultMethodOptions

> Routing and parsing options, see `JsonRpcMethodOptions` below.

#### HeaderSerializer

> Default: `typeof(HeaderJsonRpcSerializer)`

> Do not change this, because request/response "header" object format is fixed and does not imply any changes.

### JsonRpcMethodOptions

#### Route

> Default: `JsonRpcConstants.DefaultRoute` which is `"/api/jsonrpc"`

> This is the default route for all controllers/actions inherited from `JsonRpcController`.

It can be overridden with framework's `RouteAttribute` like usual. Conventional routing is not supported.

#### MethodStyle

> Default: `MethodStyle.ControllerAndAction`

> Rules how JSON Rpc `method` property is matched to controllers/actions

* `ControllerAndAction`: treat `method` as `controller.action`. Vaules like `foo.bar` are mathed to `FooController.Bar`
* `ActionOnly`: treat `method` as `action`. Vaules like `bar` are mathed to `Bar` action in any JsonRpcController

Serialization of names is handled by `RequestSerializer`, see below and [Serialization](serialization.md) for more info.

It can be overridden by `JsonRpcMethodStyleAttribute`

#### RequestSerializer

> Default: `typeof(SnakeCaseJsonRpcSerializer)`

> How request/notification `params` and response `result`/`error` should be serialized/deserialized

You can serialize content in a way different from JSON Rpc "header" object.
There are `SnakeCaseJsonRpcSerializer` and `CamelCaseJsonRpcSerializer` in `Tochka.JsonRpc.Common` package.
See [Serialization](serialization.md) for more info.

It can be overridden by `JsonRpcSerializerAttribute`


## Attributes

#### JsonRpcMethodStyleAttribute

> Override default `MethodStyle` on any controller/action. See details above for `MethodStyle`.

#### JsonRpcSerializerAttribute

> Override default `RequestSerializer` on any controller/action. See details above for `RequestSerializer` and [Serialization](serialization.md).

#### FromParamsAttribute

> Override default parameter binding behavior which is `BindingStyle.Default`

Change how JSON Rpc `params` are bound to action arguments. See [Binding](binding.md).