using System.Text.Json;
using Tochka.JsonRpc.Common.Models.Request.Untyped;

namespace Tochka.JsonRpc.Common.Models.Request;

/// <summary>
/// Base interface for all calls
/// </summary>
public interface ICall
{
    /// <summary>
    /// Version of the JSON-RPC protocol
    /// </summary>
    string Jsonrpc { get; }

    /// <summary>
    /// Name of the method to be invoked
    /// </summary>
    string Method { get; }

    /// <summary>
    /// Serialize params to JsonDocument
    /// </summary>
    IUntypedCall WithSerializedParams(JsonSerializerOptions serializerOptions);
}

/// <inheritdoc />
/// <summary>
/// Base interface for calls to access typed params
/// </summary>
/// <typeparam name="TParams">Type of params</typeparam>
public interface ICall<out TParams> : ICall
    where TParams : class?
{
    /// <summary>
    /// Parameter values to be used during the invocation of the method
    /// </summary>
    TParams? Params { get; }
}
