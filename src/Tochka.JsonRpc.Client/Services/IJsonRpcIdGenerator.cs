using Tochka.JsonRpc.Common.Models.Id;

namespace Tochka.JsonRpc.Client.Services;

/// <summary>
/// Service to generate id for JSON-RPC requests
/// </summary>
public interface IJsonRpcIdGenerator
{
    /// <summary>
    /// Generate new id
    /// </summary>
    IRpcId GenerateId();
}
