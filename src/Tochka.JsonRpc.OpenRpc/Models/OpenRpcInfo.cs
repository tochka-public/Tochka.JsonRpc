namespace Tochka.JsonRpc.OpenRpc.Models;

/// <summary>
/// The object provides metadata about the API.
/// The metadata MAY be used by the clients if needed, and MAY be presented in editing or documentation generation tools for convenience.
/// </summary>
public sealed record OpenRpcInfo(string Title, string Version)
{
    /// <summary>
    /// REQUIRED. The title of the application.
    /// </summary>
    public string Title { get; set; } = Title;

    /// <summary>
    /// A verbose description of the application. GitHub Flavored Markdown syntax MAY be used for rich text representation.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// A URL to the Terms of Service for the API. MUST be in the format of a URL.
    /// </summary>
    public string? TermsOfService { get; set; }

    /// <summary>
    /// The contact information for the exposed API.
    /// </summary>
    public OpenRpcContact? Contact { get; set; }

    /// <summary>
    /// The license information for the exposed API.
    /// </summary>
    public OpenRpcLicense? License { get; set; }

    /// <summary>
    /// REQUIRED. The version of the OpenRPC document (which is distinct from the OpenRPC Specification version or the API implementation version).
    /// </summary>
    public string Version { get; set; } = Version;
}
