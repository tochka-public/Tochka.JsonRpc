# Server/Utils

There are some utilities to work with JSON-RPC request/response objects in pipeline.
In case you need to obtain request `id` or manipulate data in any way.

See examples: [Examples#Access extra information](examples?id=access-extra-information)

## HttpContext.GetJsonRpcCall()

Use this to get JSON-RPC request or notification.

Some clarifications:

* `ICall` is an abstraction of Request and Notification.
* `IUntypedCall` is an object with headers, but its `params` are not deserialized yet and stored as `JsonDocument`.

## HttpContext.GetRawJsonRpcCall()

Use this to get raw JSON-RPC call as `JsonDocument`.

## HttpContext.GetJsonRpcResponse()

Use this to get JSON-RPC response.

Some clarifications:

* `IResponse` is an abstraction of successful and error responses.
* `UntypedResponse` is an object with headers, but its `result` already serialized and stored as `JsonDocument`.

## HttpContext.JsonRpcRequestIsBatch()

Use this to check if this call is part of batch request.

## HttpContext.SetJsonRpcResponse(IResponse response)

Use this to set response manually. Warning: it may be overwritten later by filters

## IJsonRpcFeature

You can access all JSON-RPC call information manually by reading/changing `IJsonRpcFeature` in `HttpContext.Features`.
