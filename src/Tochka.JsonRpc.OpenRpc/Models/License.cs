namespace Tochka.JsonRpc.OpenRpc.Models;

/// <summary>
/// License information for the exposed API.
/// </summary>
public sealed record License
{
    /// <summary>
    /// REQUIRED. The license name used for the API.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// A URL to the license used for the API. MUST be in the format of a URL.
    /// </summary>
    public Uri? Url { get; set; }
}
