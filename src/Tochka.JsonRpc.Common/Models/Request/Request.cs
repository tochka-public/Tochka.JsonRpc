using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using Tochka.JsonRpc.Common.Models.Id;
using Tochka.JsonRpc.Common.Models.Request.Untyped;

namespace Tochka.JsonRpc.Common.Models.Request;

/// <inheritdoc />
/// <summary>
/// Request with typed params
/// </summary>
/// <param name="Id">Identifier established by the Client</param>
/// <param name="Method">Name of the method to be invoked</param>
/// <param name="Params">Parameter values to be used during the invocation of the method</param>
/// <param name="Jsonrpc">Version of the JSON-RPC protocol</param>
[ExcludeFromCodeCoverage]
public record Request<TParams>
(
    IRpcId Id,
    string Method,
    TParams? Params,
    string Jsonrpc = JsonRpcConstants.Version
) : ICall<TParams>
    where TParams : class
{
    // required for autodoc metadata generation
    internal Request() : this(null!)
    {
    }

    /// <inheritdoc />
    public IUntypedCall WithSerializedParams(JsonSerializerOptions serializerOptions)
    {
        var serializedParams = Utils.SerializeParams(Params, serializerOptions);
        return new UntypedRequest(Id, Method, serializedParams);
    }
}

/// <inheritdoc />
/// <summary>
/// Request without params
/// </summary>
/// <param name="Id">Identifier established by the Client</param>
/// <param name="Method">Name of the method to be invoked</param>
/// <param name="Jsonrpc">Version of the JSON-RPC protocol</param>
[ExcludeFromCodeCoverage]
public record Request
(
    IRpcId Id,
    string Method,
    string Jsonrpc = JsonRpcConstants.Version
) : ICall
{
    // required for autodoc metadata generation
    internal Request() : this(null!)
    {
    }

    /// <inheritdoc />
    public IUntypedCall WithSerializedParams(JsonSerializerOptions serializerOptions) => new UntypedRequest(Id, Method, null);
}
