using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using JetBrains.Annotations;
using Tochka.JsonRpc.Common.Models.Request.Untyped;

namespace Tochka.JsonRpc.Common.Models.Request;

/// <inheritdoc />
/// <summary>
/// Notification with typed params
/// </summary>
/// <param name="Method">Name of the method to be invoked</param>
/// <param name="Params">Parameter values to be used during the invocation of the method</param>
/// <param name="Jsonrpc">Version of the JSON-RPC protocol</param>
[PublicAPI]
[ExcludeFromCodeCoverage]
public record Notification<TParams>(string Method, TParams? Params, string Jsonrpc = JsonRpcConstants.Version) : ICall<TParams>
    where TParams : class
{
    public IUntypedCall WithSerializedParams(JsonSerializerOptions serializerOptions)
    {
        var serializedParams = Utils.SerializeParams(Params, serializerOptions);
        return new UntypedNotification(Method, serializedParams);
    }
}
