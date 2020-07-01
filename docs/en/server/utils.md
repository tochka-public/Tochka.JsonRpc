# Server/Utils

There are some utilities to work with JSON Rpc request/response objects in pipeline.
In case you need to obtain request `id` or manipulate data in any way.

## HttpContext.GetJsonRpcCall

Use this to get raw JSON Rpc request or notification. It is stored in HttpContext.Items

Some clarifications:

* `ICall` is an abstraction of Request and Notification.
* `IUntypedCall` is an object with headers, but its `params` are not deserialized yet and stored as `JToken`.
* `IUntypedCall` has `RawJson` property which is a string copy of JSON HTTP body

## JsonRpcConstants

Check this class and its xmldoc to see what's also available in HttpContext.Items