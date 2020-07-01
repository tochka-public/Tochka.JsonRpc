# Server/Batches

Batches are handled in `JsonRpcMiddleware` by copying HttpContext, its Headers and Items, and invoking pipeline with it.
This transparently allows framework to function as intended, with all middlewares and filters working on independent requests.
For example, actions with different `[Authorize]` levels will work as expected: some of batch requests
will result in error, some will pass all checks and execute, and all responses will be collected. Response headers are all merged together.

However, if `AllowRawResponses` is enabled, and one of actions returns raw data,
batch response will contain it among other JSON responses, which will break whole document structure.

Currently there is no good solution to this, but maybe some checks can be added to protect against this situation.

Another thing to note: no scope is created for copied request services, so all `Scoped` services are going to be the same in single `Batch`.
This can be good or bad, depending on your scenario.