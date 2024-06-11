using System.Diagnostics.CodeAnalysis;
using Tochka.JsonRpc.Common.Models.Id;
using Tochka.JsonRpc.Common.Models.Response;

namespace Tochka.JsonRpc.Client.Models.Batch;

/// <summary>
/// Extensions for batch JSON-RPC responses
/// </summary>
public static class BatchExtensions
{
    /// <summary>
    /// Gets the JSON-RPC response that is associated with the specified key.
    /// </summary>
    /// <param name="responses">Collection of JSON-RPC responses</param>
    /// <param name="id">JSON-RPC id</param>
    /// <param name="response">Received response, if the key is found; otherwise, null. This parameter is passed uninitialized</param>
    /// <returns>true if the object contains an element that has the specified key; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">key is null</exception>
    [ExcludeFromCodeCoverage]
    public static bool TryGetResponse(this IReadOnlyDictionary<IRpcId, IResponse> responses,
        IRpcId id,
        [NotNullWhen(true)] out IResponse? response) =>
        responses.TryGetValue(id, out response);
}