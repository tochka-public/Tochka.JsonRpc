using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;

namespace Tochka.JsonRpc.OpenRpc.Models;

/// <summary>
/// This is the root object of the OpenRPC document.
/// The contents of this object represent a whole OpenRPC document.
/// How this object is constructed or stored is outside the scope of the OpenRPC Specification.
/// </summary>
[PublicAPI]
[ExcludeFromCodeCoverage]
public sealed record OpenRpc(OpenRpcInfo Info)
{
    /// <summary>
    /// REQUIRED. This string MUST be the semantic version number of the OpenRPC Specification version that the OpenRPC document uses.
    /// The openrpc field SHOULD be used by tooling specifications and clients to interpret the OpenRPC document.
    /// This is not related to the API info.version string.
    /// </summary>
    public string Openrpc { get; set; } = OpenRpcConstants.SpecVersion;

    /// <summary>
    /// REQUIRED. Provides metadata about the API. The metadata MAY be used by tooling as required.
    /// </summary>
    public OpenRpcInfo Info { get; set; } = Info;

    /// <summary>
    /// An array of <see cref="OpenRpcServer" /> Objects, which provide connectivity information to a target server.
    /// If the servers property is not provided, or is an empty array, the default value would be a <see cref="OpenRpcServer" /> Object with a url value of localhost.
    /// </summary>
    public List<OpenRpcServer>? Servers { get; set; }

    /// <summary>
    /// REQUIRED. The available methods for the API. While it is required, the array may be empty (to handle security filtering, for example).
    /// </summary>
    public List<OpenRpcMethod> Methods { get; set; } = new();

    /// <summary>
    /// An element to hold various schemas for the specification.
    /// </summary>
    public OpenRpcComponents? Components { get; set; }

    /// <summary>
    /// Additional external documentation.
    /// </summary>
    public OpenRpcExternalDocumentation? ExternalDocs { get; set; }
}
