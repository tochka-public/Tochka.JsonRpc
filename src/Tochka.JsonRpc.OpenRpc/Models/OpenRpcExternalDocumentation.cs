using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;

namespace Tochka.JsonRpc.OpenRpc.Models;

/// <summary>
/// Allows referencing an external resource for extended documentation.
/// </summary>
[PublicAPI]
[ExcludeFromCodeCoverage]
public sealed record OpenRpcExternalDocumentation(Uri Url)
{
    /// <summary>
    /// A verbose explanation of the target documentation. GitHub Flavored Markdown syntax MAY be used for rich text representation.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// REQUIRED. The URL for the target documentation. Value MUST be in the format of a URL.
    /// </summary>
    public Uri Url { get; set; } = Url;
}
