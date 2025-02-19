using System.Diagnostics.CodeAnalysis;

namespace Tochka.JsonRpc.OpenRpc.Models;

/// <summary>
/// License information for the exposed API.
/// </summary>
[ExcludeFromCodeCoverage]
public sealed record OpenRpcLicense
(
    string Name
)
{
    /// <summary>
    /// REQUIRED. The license name used for the API.
    /// </summary>
    public string Name { get; set; } = Name;

    /// <summary>
    /// A URL to the license used for the API. MUST be in the format of a URL.
    /// </summary>
    public Uri? Url { get; set; }
}
