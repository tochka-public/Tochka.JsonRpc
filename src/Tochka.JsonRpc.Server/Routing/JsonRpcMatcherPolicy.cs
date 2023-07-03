using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Matching;
using Tochka.JsonRpc.Common.Models.Request;
using Tochka.JsonRpc.Server.Attributes;
using Tochka.JsonRpc.Server.Exceptions;
using Tochka.JsonRpc.Server.Extensions;

namespace Tochka.JsonRpc.Server.Routing;

internal class JsonRpcMatcherPolicy : MatcherPolicy, IEndpointSelectorPolicy
{
    [ExcludeFromCodeCoverage]
    public override int Order => 0;

    public bool AppliesToEndpoints(IReadOnlyList<Endpoint> endpoints) =>
        endpoints.Any(IsJsonRpcEndpoint);

    public Task ApplyAsync(HttpContext httpContext, CandidateSet candidates)
    {
        var call = httpContext.GetJsonRpcCall();
        if (call == null)
        {
            // If we got not json rpc request
            RejectAllCandidates(candidates);
            return Task.CompletedTask;
        }

        var validCandidatesExist = ValidateCandidates(candidates, call);
        if (!validCandidatesExist)
        {
            // hack to distinguish between unknown route (== 404 Not Found) and unknown method (== json rpc error with code -32601)
            throw new JsonRpcMethodNotFoundException(call.Method);
        }

        return Task.CompletedTask;
    }

    private static bool IsJsonRpcEndpoint(Endpoint endpoint) =>
        endpoint.Metadata.GetMetadata<JsonRpcControllerAttribute>() != null
        && endpoint.Metadata.GetMetadata<JsonRpcMethodAttribute>() != null;

    private static void RejectAllCandidates(CandidateSet candidates)
    {
        for (var i = 0; i < candidates.Count; i++)
        {
            candidates.SetValidity(i, false);
        }
    }

    private static bool ValidateCandidates(CandidateSet candidates, ICall call)
    {
        var validCandidatesExist = false;
        for (var i = 0; i < candidates.Count; i++)
        {
            var candidate = candidates[i];
            var jsonRpcMetadata = candidate.Endpoint.Metadata.GetMetadata<JsonRpcMethodAttribute>()!;
            var methodMatches = call.Method == jsonRpcMetadata.Method;
            candidates.SetValidity(i, methodMatches);
            if (methodMatches)
            {
                validCandidatesExist = true;
            }
        }

        return validCandidatesExist;
    }
}
