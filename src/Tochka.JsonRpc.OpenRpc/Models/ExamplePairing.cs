namespace Tochka.JsonRpc.OpenRpc.Models;

/// <summary>
/// The Example Pairing object consists of a set of example params and result.
/// The result is what you can expect from the JSON-RPC service given the exact params.
/// </summary>
public sealed record ExamplePairing
{
    /// <summary>
    /// Name for the example pairing.
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// A verbose explanation of the example pairing.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Short description for the example pairing.
    /// </summary>
    public string? Summary { get; set; }

    /// <summary>
    /// Example parameters.
    /// </summary>
    public List<Example>? Params { get; set; }

    /// <summary>
    /// Example result. When undefined, the example pairing represents usage of the method as a notification.
    /// </summary>
    public Example? Result { get; set; }
}
