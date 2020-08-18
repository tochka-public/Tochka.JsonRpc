# Server/Utils

There are some utilities to work with JSON Rpc request/response objects in pipeline.
In case you need to obtain request `id` or manipulate data in any way.

See examples: [access extra information](examples?id=access-extra-information)

## HttpContext.GetJsonRpcCall

Use this to get raw JSON Rpc request or notification. It is stored in HttpContext.Items

Some clarifications:

* `ICall` is an abstraction of Request and Notification.
* `IUntypedCall` is an object with headers, but its `params` are not deserialized yet and stored as `JToken`.
* `IUntypedCall` has `RawJson` property which is a string copy of JSON HTTP body

## JsonRpcConstants

Check this class and its xmldoc to see what's also available in HttpContext.Items

## Logging

You should do logging yourself, but as a reference implementation there are:

* [`JsonRpcRequestLoggingMiddleware`](https://github.com/tochka-public/Tochka.JsonRpc/blob/master/src/Tochka.JsonRpc.Server/Pipeline/JsonRpcRequestLoggingMiddleware.cs) to log raw JSON requests
* [`JsonRpcResultLoggingFilter`](https://github.com/tochka-public/Tochka.JsonRpc/blob/master/src/Tochka.JsonRpc.Server/Pipeline/JsonRpcResultLoggingFilter.cs) to log serialized response data

See examples for usage: [logging](examples?id=logging)