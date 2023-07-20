# Server/Batches

Batches are handled in `JsonRpcMiddleware` by replacing `HttpContext`'s Endpoint and JSON-RPC data for every element in the batch. `HttpContext` isn't thread safe, so currently only sequential handling is supported. The only other way would be to copy entire `HttpContext` for every batch element, but it leads to data duplication when merging them into batch response, and implementation is complicated, so no concurrent batches for now.

It's possible to find out if current call is part of a Batch by calling `HttpContext`'s extension method `JsonRpcRequestIsBatch()`.
Can be useful if you want to set some header only once, or create custom logic in middlewares or filters.

If `AllowRawResponses` is enabled, and one of actions returns raw data,
batch response will contain it among other JSON responses, which **will break** whole response JSON structure. Currently there is no good solution to this, but maybe some checks can be added to protect against this situation.

Another thing to note: no separate scope is created for batch element pipelines, so all `Scoped` services are going to be the same in single `Batch`.
This can be good or bad, depending on your scenario.