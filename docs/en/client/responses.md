# Client/Responses

To simplify working with responses that can have either `result` or `error` and contain different data types, client methods return special objects - `ISingleJsonRpcResult` and `IBatchJsonRpcResult`. These interfaces have several methods for easy access to request results.

*All IBatchJsonRpcResult methods similar to ISingleJsonRpcResult, but accept id as argument to access correct response*

### `GetResponseOrThrow<TResponse>()`

If response is successful - deserializes `result` property to given `TResponse` type.

If response has error - throws `JsonRpcException` with `Context` property, tha has all information about request and response.

If no response with given `id` in batch - throws `JsonRpcException` with `Context` property, tha has all information about request and response.

### `AsResponse<TResponse>()`

If response is successful - deserializes `result` property to given `TResponse` type.

If response has error - returns `default(TResponse)`.

If no response with given `id` in batch - returns `default(TResponse)`.

### `HasError()`

If response is successful - returns `false`.

If response has error - returns `true`.

If no response with given `id` in batch - throws `JsonRpcException` with `Context` property, tha has all information about request and response.

### `AsAnyError()`

If response is successful - returns `null`.

If response has error - returns raw `Error<JsonDocument>` without deserializing

If no response with given `id` in batch - returns `null`.

### `AsTypedError<TError>()`

If response is successful - returns `null`.

If response has error - returns `Error<TError>` with deserialized `error.data`

If no response with given `id` in batch - returns `null`.

### AsErrorWithExceptionInfo()

Same as `AsTypedError<ExceptionInfo>()`