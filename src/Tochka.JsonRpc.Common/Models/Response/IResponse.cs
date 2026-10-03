using System.Diagnostics.CodeAnalysis;
using Tochka.JsonRpc.Common.Models.Id;
using Tochka.JsonRpc.Common.Models.Response.Errors;

namespace Tochka.JsonRpc.Common.Models.Response;

/// <summary>
/// Base interface for responses
/// </summary>
public interface IResponse
{
    /// <summary>
    /// Identifier established by the Client
    /// </summary>
    IRpcId Id { get; }

    /// <summary>
    /// Version of the JSON-RPC protocol
    /// </summary>
    string Jsonrpc { get; }
}

public interface IErrorResponse : IResponse
{
    [SuppressMessage("Naming", "CA1716:Identifiers should not match keywords")]
    public IError Error { get; }
}
