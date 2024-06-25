using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;
using Json.Schema;

namespace Tochka.JsonRpc.OpenRpc.Models;

/// <summary>
/// Describes the interface for the given method name.
/// The method name is used as the method field of the JSON-RPC body. It therefore MUST be unique.
/// </summary>
[PublicAPI]
[ExcludeFromCodeCoverage]
public sealed record OpenRpcMethod(string Name)
{
    /// <summary>
    /// REQUIRED. The canonical name for the method. The name MUST be unique within the methods array.
    /// </summary>
    public string Name { get; set; } = Name;

    /// <summary>
    /// A list of tags for API documentation control. Tags can be used for logical grouping of methods by resources or any other qualifier.
    /// The list can use the Reference Object to link to tags that are defined by the <see cref="OpenRpcTag" /> Object.
    /// </summary>
    public List<JsonSchema>? Tags { get; set; }

    /// <summary>
    /// A short summary of what the method does.
    /// </summary>
    public string? Summary { get; set; }

    /// <summary>
    /// A verbose explanation of the method behavior. GitHub Flavored Markdown syntax MAY be used for rich text representation.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Additional external documentation for this method.
    /// </summary>
    public OpenRpcExternalDocumentation? ExternalDocs { get; set; }

    /// <summary>
    /// REQUIRED. A list of parameters that are applicable for this method.
    /// The list MUST NOT include duplicated parameters and therefore require name to be unique.
    /// The list can use the Reference Object to link to parameters that are defined by the <see cref="OpenRpcContentDescriptor" /> Object.
    /// All optional params (content descriptor objects with “required”: false) MUST be positioned after all required params in the list.
    /// </summary>
    public List<OpenRpcContentDescriptor> Params { get; set; } = new();

    /// <summary>
    /// The description of the result returned by the method.
    /// If defined, it MUST be a <see cref="OpenRpcContentDescriptor" /> or Reference Object.
    /// If undefined, the method MUST only be used as a notification.
    /// </summary>
    public OpenRpcContentDescriptor? Result { get; set; }

    /// <summary>
    /// Declares this method to be deprecated. Consumers SHOULD refrain from usage of the declared method. Default value is false.
    /// </summary>
    public bool? Deprecated { get; set; }

    /// <summary>
    /// An alternative <see cref="OpenRpcServer" /> array to service this method.
    /// If an alternative servers array is specified at the Root level, it will be overridden by this value.
    /// </summary>
    public List<OpenRpcServer>? Servers { get; set; }

    /// <summary>
    /// A list of custom application defined errors that MAY be returned. The Errors MUST have unique error codes.
    /// </summary>
    public List<OpenRpcError>? Errors { get; set; }

    /// <summary>
    /// A list of possible links from this method call.
    /// </summary>
    public List<OpenRpcLink>? Links { get; set; }

    /// <summary>
    /// The expected format of the parameters.
    /// As per the JSON-RPC 2.0 specification, the params of a JSON-RPC request object may be an array, object, or either
    /// (represented as by-position, by-name, and either respectively).
    /// When a method has a paramStructure value of by-name, callers of the method MUST send a JSON-RPC request object whose params field is an object.
    /// Further, the key names of the params object MUST be the same as the contentDescriptor.names for the given method. Defaults to "either".
    /// </summary>
    public OpenRpcParamStructure? ParamStructure { get; set; }

    /// <summary>
    /// Array of Example Pairing Object where each example includes a valid params-to-result <see cref="OpenRpcContentDescriptor" /> pairing.
    /// </summary>
    public List<OpenRpcExamplePairing>? Examples { get; set; }
}
