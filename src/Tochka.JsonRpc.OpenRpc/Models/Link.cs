namespace Tochka.JsonRpc.OpenRpc.Models;

/// <summary>
/// The Link object represents a possible design-time link for a result.
/// The presence of a link does not guarantee the caller’s ability to successfully invoke it,
/// rather it provides a known relationship and traversal mechanism between results and other methods.
/// </summary>
public sealed record Link
{
    /// <summary>
    /// REQUIRED. Canonical name of the link.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// A description of the link. GitHub Flavored Markdown syntax MAY be used for rich text representation.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Short description for the link.
    /// </summary>
    public string? Summary { get; set; }

    /// <summary>
    /// The name of an existing, resolvable OpenRPC method, as defined with a unique method.
    /// This field MUST resolve to a unique <see cref="Method"/> Object. As opposed to Open Api, Relative method values ARE NOT permitted.
    /// </summary>
    public string? Method { get; set; }

    /// <summary>
    /// A map representing parameters to pass to a method as specified with method.
    /// The key is the parameter name to be used,
    /// whereas the value can be a constant or a runtime expression to be evaluated and passed to the linked method.
    /// </summary>
    public Dictionary<string, object?>? Params { get; set; }

    /// <summary>
    /// A server object to be used by the target method.
    /// </summary>
    public Server? Server { get; set; }
}
