namespace Tochka.JsonRpc.OpenRpc.Models;

/// <summary>
/// The Example object is an object that defines an example that is intended to match the schema of a given <see cref="OpenRpcContentDescriptor" />.
/// </summary>
public sealed record OpenRpcExample
{
    /// <summary>
    /// Canonical name of the example.
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// Short description for the example.
    /// </summary>
    public string? Summary { get; set; }

    /// <summary>
    /// A verbose explanation of the example. GitHub Flavored Markdown syntax MAY be used for rich text representation.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Embedded literal example. The value field and externalValue field are mutually exclusive.
    /// To represent examples of media types that cannot naturally represented in JSON,
    /// use a string value to contain the example, escaping where necessary.
    /// </summary>
    public object? Value { get; set; }

    /// <summary>
    /// A URL that points to the literal example.
    /// This provides the capability to reference examples that cannot easily be included in JSON documents.
    /// The value field and externalValue field are mutually exclusive.
    /// </summary>
    public Uri? ExternalValue { get; set; }
}
