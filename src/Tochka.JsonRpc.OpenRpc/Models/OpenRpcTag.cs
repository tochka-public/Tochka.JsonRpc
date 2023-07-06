using System.Diagnostics.CodeAnalysis;

namespace Tochka.JsonRpc.OpenRpc.Models;

/// <summary>
/// Adds metadata to a single tag that is used by the Method Object.
/// It is not mandatory to have a Tag Object per tag defined in the Method Object instances.
/// </summary>
[ExcludeFromCodeCoverage]
public sealed record OpenRpcTag(string Name)
{
    /// <summary>
    /// REQUIRED. The name of the tag.
    /// </summary>
    public string Name { get; set; } = Name;

    /// <summary>
    /// A short summary of the tag.
    /// </summary>
    public string? Summary { get; set; }

    /// <summary>
    /// A verbose explanation for the tag. GitHub Flavored Markdown syntax MAY be used for rich text representation.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Additional external documentation for this tag.
    /// </summary>
    public OpenRpcExternalDocumentation? ExternalDocs { get; set; }
}
