using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Matching;

namespace Tochka.JsonRpc.Server;

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
            throw new JsonRpcServerException("Not found json rpc call in http context, may be middleware is missing or registered after routing");
        }

        for (var i = 0; i < candidates.Count; i++)
        {
            var candidate = candidates[i];
            if (httpContext.Request.Method != HttpMethods.Post || !IsJsonRpcEndpoint(candidate.Endpoint))
            {
                candidates.SetValidity(i, false);
                continue;
            }

            var jsonRpcMetadata = candidate.Endpoint.Metadata.GetMetadata<JsonRpcActionAttribute>()!;
            var methodMatches = call.Method == jsonRpcMetadata.Method;
            candidates.SetValidity(i, methodMatches);
        }

        return Task.CompletedTask;
    }

    private static bool IsJsonRpcEndpoint(Endpoint endpoint) =>
        endpoint.Metadata.GetMetadata<JsonRpcControllerAttribute>() != null
        && endpoint.Metadata.GetMetadata<JsonRpcActionAttribute>() != null;
}
