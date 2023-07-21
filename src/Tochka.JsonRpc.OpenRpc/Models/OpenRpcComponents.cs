using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;
using Json.Schema;

namespace Tochka.JsonRpc.OpenRpc.Models;

/// <summary>
/// Holds a set of reusable objects for different aspects of the OpenRPC.
/// All objects defined within the components object will have no effect on the API
/// unless they are explicitly referenced from properties outside the components object.
/// </summary>
[PublicAPI]
[ExcludeFromCodeCoverage]
public sealed record OpenRpcComponents
{
    /// <summary>
    /// An object to hold reusable <see cref="OpenRpcContentDescriptor" /> Objects.
    /// </summary>
    public Dictionary<string, OpenRpcContentDescriptor>? ContentDescriptors { get; set; }

    /// <summary>
    /// An object to hold reusable <see cref="JsonSchema" /> Objects.
    /// </summary>
    public Dictionary<string, JsonSchema>? Schemas { get; set; }

    /// <summary>
    /// An object to hold reusable <see cref="OpenRpcExample" /> Objects.
    /// </summary>
    public Dictionary<string, OpenRpcExample>? Examples { get; set; }

    /// <summary>
    /// An object to hold reusable <see cref="OpenRpcLink" /> Objects.
    /// </summary>
    public Dictionary<string, OpenRpcLink>? Links { get; set; }

    /// <summary>
    /// An object to hold reusable <see cref="OpenRpcError" /> Objects.
    /// </summary>
    public Dictionary<string, OpenRpcError>? Errors { get; set; }

    /// <summary>
    /// An object to hold reusable <see cref="OpenRpcExamplePairing" /> Objects.
    /// </summary>
    public Dictionary<string, OpenRpcExamplePairing>? ExamplePairingObjects { get; set; }

    /// <summary>
    /// An object to hold reusable <see cref="OpenRpcTag" /> Objects.
    /// </summary>
    public Dictionary<string, OpenRpcTag>? Tags { get; set; }
}
