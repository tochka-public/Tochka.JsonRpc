using System.Diagnostics.CodeAnalysis;

namespace Tochka.JsonRpc.OpenRpc.Models;

/// <summary>
/// An object representing a Server.
/// </summary>
[ExcludeFromCodeCoverage]
public sealed record OpenRpcServer
(
    string Name,
    Uri Url
)
{
    /// <summary>
    /// REQUIRED. A name to be used as the canonical name for the server.
    /// </summary>
    public string Name { get; set; } = Name;

    /// <summary>
    /// REQUIRED. A URL to the target host.
    /// </summary>
    public Uri Url { get; set; } = Url;

    /// <summary>
    /// A short summary of what the server is.
    /// </summary>
    public string? Summary { get; set; }

    /// <summary>
    /// An optional string describing the host designated by the URL. GitHub Flavored Markdown syntax MAY be used for rich text representation.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// A map between a variable name and its value. The value is passed into the Runtime Expression to produce a server URL.
    /// </summary>
    public Dictionary<string, OpenRpcServerVariable>? Variables { get; set; }
}
