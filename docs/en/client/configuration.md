# Client/Configuration

You can customize client behavior by overriding properties or methods and by replacing internal services.

> Check [Examples](examples) page to see how it works

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

> `JsonSerializerOptions` used to serialize and deserialize JSON-RPC "headers": `id`, `jsonrpc`, etc.

Changing this is not recommended, because request/response "header" object format is fixed and does not imply any changes.

See [Serialization](serialization) for more info.

### Encoding

> Default: `Encoding.UTF8`

> Encoding used to send HTTP requests

### Client

> Internal `HttpClient` used to send HTTP requests

You can configure it to achieve custom logic for sending HTTP requests.

### ParseBody(...)

> Logic of parsing HTTP response body

For example, you can override this method if responses from API violate JSON-RPC protocol or have additional data.

Changing this not recommended, because JSON-RPC responses must have fixed format.

### CreateHttpContent(...)

> Logic of serializing request and wrapping it in `HttpContent` with encoding and Content-Type

Changing this not recommended, because JSON-RPC requests must have fixed format.

### GetContent(...)

> Logic of reading `HttpResponseMessage` content

## Services

### RpcIdGenerator

> Default: `JsonRpcIdGenerator`

> Service to generate `id` for requests in overloads that don't accept `id` as argument

Can be replaced in DI to override `id` generation logic.
