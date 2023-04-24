using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Matching;
using Tochka.JsonRpc.Server.Attributes;
using Tochka.JsonRpc.Server.Exceptions;
using Tochka.JsonRpc.Server.Extensions;

namespace Tochka.JsonRpc.Server.Routing;

public class JsonRpcMatcherPolicy : MatcherPolicy, IEndpointSelectorPolicy
{
    public override int Order => 0;

    public bool AppliesToEndpoints(IReadOnlyList<Endpoint> endpoints) =>
        endpoints.Any(IsJsonRpcEndpoint);

    public Task ApplyAsync(HttpContext httpContext, CandidateSet candidates)
    {
        var call = httpContext.GetJsonRpcCall();
        if (call == null)
        {
            throw new JsonRpcServerException("Not found json rpc call in HttpContext, maybe middleware is missing or registered after routing");
        }

        var validCandidatesExist = false;
        for (var i = 0; i < candidates.Count; i++)
        {
            var candidate = candidates[i];
            if (httpContext.Request.Method != HttpMethods.Post || !IsJsonRpcEndpoint(candidate.Endpoint))
            {
                candidates.SetValidity(i, false);
                continue;
            }

            var jsonRpcMetadata = candidate.Endpoint.Metadata.GetMetadata<JsonRpcMethodAttribute>()!;
            var methodMatches = call.Method == jsonRpcMetadata.Method;
            candidates.SetValidity(i, methodMatches);
            if (methodMatches)
            {
                validCandidatesExist = true;
            }
        }

        // hack to distinguish between unknown route (== 404 Not Found) and unknown method (== json rpc error with code -32601)
        if (!validCandidatesExist)
        {
            throw new JsonRpcMethodNotFoundException(call.Method);
        }

        return Task.CompletedTask;
    }

    private static bool IsJsonRpcEndpoint(Endpoint endpoint) =>
        endpoint.Metadata.GetMetadata<JsonRpcControllerAttribute>() != null
        && endpoint.Metadata.GetMetadata<JsonRpcMethodAttribute>() != null;
}
