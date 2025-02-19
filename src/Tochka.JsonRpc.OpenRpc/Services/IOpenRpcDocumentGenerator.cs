using Tochka.JsonRpc.OpenRpc.Models;

namespace Tochka.JsonRpc.OpenRpc.Services;

/// <summary>
/// Service to encapsulate logic of OpenRPC document generation
/// </summary>
public interface IOpenRpcDocumentGenerator
{
    /// <summary>
    /// Generate OpenRPC document for JSON-RPC API
    /// </summary>
    /// <param name="info">Metadata about API</param>
    /// <param name="documentName">Name of generated document</param>
    /// <param name="host">Base URL for servers description</param>
    Models.OpenRpc Generate(OpenRpcInfo info, string documentName, Uri host);
}
