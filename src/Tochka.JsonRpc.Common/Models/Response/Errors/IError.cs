namespace Tochka.JsonRpc.Common.Models.Response.Errors;

/// <summary>
/// Base interface for errors
/// </summary>
public interface IError
{
    /// <summary>
    /// Number that indicates the error type that occurred
    /// </summary>
    int Code { get; }

    /// <summary>
    /// Short description of the error
    /// </summary>
    /// <remarks>
    /// SHOULD be limited to a concise single sentence
    /// </remarks>
    string Message { get; }

    /// <summary>
    /// Additional information about the error
    /// </summary>
    object? Data { get; }
}
