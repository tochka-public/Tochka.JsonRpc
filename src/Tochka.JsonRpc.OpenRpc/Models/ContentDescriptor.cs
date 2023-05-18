using Json.Schema;

namespace Tochka.JsonRpc.OpenRpc.Models;

/// <summary>
/// Content Descriptors are objects that do just as they suggest - describe content.
/// They are reusable ways of describing either parameters or result. They MUST have a schema.
/// </summary>
public sealed record ContentDescriptor
{
    /// <summary>
    /// REQUIRED. Name of the content that is being described
    /// If the content described is a method parameter assignable by-name, this field SHALL define the parameter’s key (ie name).
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// A short summary of the content that is being described.
    /// </summary>
    public string? Summary { get; set; }

    /// <summary>
    /// A verbose explanation of the content descriptor behavior. GitHub Flavored Markdown syntax MAY be used for rich text representation.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Determines if the content is a required field. Default value is false.
    /// </summary>
    public bool? Required { get; set; }

    /// <summary>
    /// REQUIRED. Schema that describes the content.
    /// </summary>
    public JsonSchema Schema { get; set; }

    /// <summary>
    /// Specifies that the content is deprecated and SHOULD be transitioned out of usage. Default value is false.
    /// </summary>
    public bool? Deprecated { get; set; }
}
