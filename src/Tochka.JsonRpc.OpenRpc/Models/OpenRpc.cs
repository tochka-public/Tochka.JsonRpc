namespace Tochka.JsonRpc.OpenRpc.Models;

/// <summary>
/// This is the root object of the OpenRPC document.
/// The contents of this object represent a whole OpenRPC document.
/// How this object is constructed or stored is outside the scope of the OpenRPC Specification.
/// </summary>
public sealed record OpenRpc
{
    /// <summary>
    /// REQUIRED. This string MUST be the semantic version number of the OpenRPC Specification version that the OpenRPC document uses.
    /// The openrpc field SHOULD be used by tooling specifications and clients to interpret the OpenRPC document.
    /// This is not related to the API info.version string.
    /// </summary>
    public string Openrpc { get; set; }

    /// <summary>
    /// REQUIRED. Provides metadata about the API. The metadata MAY be used by tooling as required.
    /// </summary>
    public Info Info { get; set; }

    /// <summary>
    /// An array of <see cref="Server" /> Objects, which provide connectivity information to a target server.
    /// If the servers property is not provided, or is an empty array, the default value would be a <see cref="Server" /> Object with a url value of localhost.
    /// </summary>
    public List<Server>? Servers { get; set; }

    /// <summary>
    /// REQUIRED. The available methods for the API. While it is required, the array may be empty (to handle security filtering, for example).
    /// </summary>
    public List<Method> Methods { get; set; }

    /// <summary>
    /// An element to hold various schemas for the specification.
    /// </summary>
    public Components? Components { get; set; }

    /// <summary>
    /// Additional external documentation.
    /// </summary>
    public ExternalDocumentation? ExternalDocs { get; set; }
}
