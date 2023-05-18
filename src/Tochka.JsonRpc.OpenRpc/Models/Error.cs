using System.Diagnostics.CodeAnalysis;

namespace Tochka.JsonRpc.OpenRpc.Models;

/// <summary>
/// Defines an application level error.
/// </summary>
[SuppressMessage("Naming", "CA1716:Identifiers should not match keywords", Justification = "Error is official name")]
public sealed record Error
{
    /// <summary>
    /// REQUIRED. A Number that indicates the error type that occurred.
    /// This MUST be an integer. The error codes from and including -32768 to -32000 are reserved for pre-defined errors.
    /// These pre-defined errors SHOULD be assumed to be returned from any JSON-RPC api.
    /// </summary>
    public int Code { get; set; }

    /// <summary>
    /// REQUIRED. A String providing a short description of the error. The message SHOULD be limited to a concise single sentence.
    /// </summary>
    public string Message { get; set; }

    /// <summary>
    /// A Primitive or Structured value that contains additional information about the error. This may be omitted.
    /// The value of this member is defined by the Server (e.g. detailed error information, nested errors etc.).
    /// </summary>
    public object? Data { get; set; }
}
