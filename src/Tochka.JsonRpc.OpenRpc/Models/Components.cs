using Json.Schema;

namespace Tochka.JsonRpc.OpenRpc.Models;

/// <summary>
/// Holds a set of reusable objects for different aspects of the OpenRPC.
/// All objects defined within the components object will have no effect on the API
/// unless they are explicitly referenced from properties outside the components object.
/// </summary>
public sealed record Components
{
    /// <summary>
    /// An object to hold reusable <see cref="ContentDescriptor"/> Objects.
    /// </summary>
    public Dictionary<string, ContentDescriptor>? ContentDescriptors { get; set; }

    /// <summary>
    /// An object to hold reusable <see cref="JsonSchema"/> Objects.
    /// </summary>
    public Dictionary<string, JsonSchema>? Schemas { get; set; }

    /// <summary>
    /// An object to hold reusable <see cref="Example"/> Objects.
    /// </summary>
    public Dictionary<string, Example>? Examples { get; set; }

    /// <summary>
    /// An object to hold reusable <see cref="Link"/> Objects.
    /// </summary>
    public Dictionary<string, Link>? Links { get; set; }

    /// <summary>
    /// An object to hold reusable <see cref="Error"/> Objects.
    /// </summary>
    public Dictionary<string, Error>? Errors { get; set; }

    /// <summary>
    /// An object to hold reusable <see cref="ExamplePairing"/> Objects.
    /// </summary>
    public Dictionary<string, ExamplePairing>? ExamplePairingObjects { get; set; }

    /// <summary>
    /// An object to hold reusable <see cref="Tag"/> Objects.
    /// </summary>
    public Dictionary<string, Tag>? Tags { get; set; }
}
