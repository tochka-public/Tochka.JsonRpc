# Client/Configuration

You can customize client behavior by `JsonRpcClientOptionsBase`, overriding properties or methods and by replacing internal services.

> Check [Examples](examples) page to see how it works

## JsonRpcClientOptionsBase

### Url

> Default: `null`

> Base URL to send HTTP requests to (configures internal HttpClient)

Each client method has overload that accepts `requestUrl` as first argument - it will be concatenated to `JsonRpcClientOptionsBase.Url` for cases when some of API methods are hosted by another route

There are two limitations to prevent unexpected behavior when joining url parts:
 - `JsonRpcClientOptionsBase.Url` must end with `/`
 - `requestUrl` must not start with `/`

### Timeout

> Default: `TimeSpan.FromSeconds(10)`

> HTTP requests timeout (configures internal HttpClient)

## Properties and methods

### UserAgent

> Default: `"Tochka.JsonRpc.Client"`

> Value of User-Agent header in HTTP requests (configures internal HttpClient)

### DataJsonSerializerOptions

> Default: `JsonRpcSerializerOptions.SnakeCase`

> `JsonSerializerOptions` used to serialize `params` and deserialize `result` or `error.data`

There are `JsonRpcSerializerOptions.SnakeCase` and `JsonRpcSerializerOptions.CamelCase` in `Tochka.JsonRpc.Common` package.

See [Serialization](serialization) for more info.

### HeadersJsonSerializerOptions

> Default: `JsonRpcSerializerOptions.Headers`

> `JsonSerializerOptions` used to serialize and deserialize JSON Rpc "headers": `id`, `jsonrpc`, etc.

Changing this not recommended, because request/response "header" object format is fixed and does not imply any changes.

See [Serialization](serialization) for more info.

### Encoding

> Default: `Encoding.UTF8`

> Encoding used to send HTTP requests

### Client

> Internal `HttpClient` used to send HTTP requests

You can configure it to achieve custom logic for sending requests

### ParseBody(...)

> Logic of parsing HTTP response body

For example, you can override this method in case responses from API have not JSON Rpc format or additional data

Changing this not recommended, because JSON Rpc responses must have fixed format.

### CreateHttpContent(...)

> Logic of serializing request and wrapping it in `HttpContent` with encoding and Content-Type

Changing this not recommended, because JSON Rpc requests must have fixed format.

### GetContent(...)

> Logic of reading `HttpResponseMessage` content

## Services

### RpcIdGenerator

> Default: `JsonRpcIdGenerator`

> Service to generate `id` for requests in overloads that don't accept `id` as argument

Can be replaced in DI to override `id` generation logic
