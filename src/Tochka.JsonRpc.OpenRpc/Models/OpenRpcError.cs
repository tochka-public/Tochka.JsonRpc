﻿using System.Diagnostics.CodeAnalysis;

namespace Tochka.JsonRpc.OpenRpc.Models;

/// <summary>
/// Defines an application level error.
/// </summary>
[ExcludeFromCodeCoverage]
public sealed record OpenRpcError
(
    int Code,
    string Message
)
{
    /// <summary>
    /// REQUIRED. A Number that indicates the error type that occurred.
    /// This MUST be an integer. The error codes from and including -32768 to -32000 are reserved for pre-defined errors.
    /// These pre-defined errors SHOULD be assumed to be returned from any JSON-RPC api.
    /// </summary>
    public int Code { get; set; } = Code;

    /// <summary>
    /// REQUIRED. A String providing a short description of the error. The message SHOULD be limited to a concise single sentence.
    /// </summary>
    public string Message { get; set; } = Message;

    /// <summary>
    /// A Primitive or Structured value that contains additional information about the error. This may be omitted.
    /// The value of this member is defined by the Server (e.g. detailed error information, nested errors etc.).
    /// </summary>
    public object? Data { get; set; }
}
