using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using JetBrains.Annotations;
using Tochka.JsonRpc.Common.Models.Request;
using Tochka.JsonRpc.Common.Models.Response;

namespace Tochka.JsonRpc.Server.Features;

/// <summary>
/// Feature with information for JSON-RPC call processing
/// </summary>
[PublicAPI]
[SuppressMessage("Naming", "CA1716:Identifiers should not match keywords", Justification = "Call is official name")]
public interface IJsonRpcFeature
{
    /// <summary>
    /// Raw JSON-RPC call
    /// </summary>
    JsonDocument? RawCall { get; set; }

    /// <summary>
    /// Deserialized JSON-RPC call
    /// </summary>
    ICall? Call { get; set; }

    /// <summary>
    /// JSON-RPC response
    /// </summary>
    IResponse? Response { get; set; }

    /// <summary>
    /// True if this call is part of batch request
    /// </summary>
    bool IsBatch { get; set; }
}
