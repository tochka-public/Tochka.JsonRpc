using JetBrains.Annotations;
using Tochka.JsonRpc.Common.Models.Id;

namespace Tochka.JsonRpc.Client.Services;

/// <summary>
/// Service to generate id for JSON-RPC requests
/// </summary>
[PublicAPI]
public interface IJsonRpcIdGenerator
{
    /// <summary>
    /// Generate new id
    /// </summary>
    IRpcId GenerateId();
}
