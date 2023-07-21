using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;

namespace Tochka.JsonRpc.Server.Settings;

/// <summary>
/// Binding Style for JSON-RPC parameters
/// </summary>
[PublicAPI]
[SuppressMessage("Naming", "CA1720:Identifiers should not contain type names", Justification = "Object is official name")]
public enum BindingStyle
{
    /// <summary>
    /// Parameter will be bound from property in `params` object with corresponding name
    /// or from `params` array by it's index (same as argument position)
    /// </summary>
    Default,

    /// <summary>
    /// Whole `params` object will be bound to parameter
    /// </summary>
    Object,

    /// <summary>
    /// Whole `params` array will be bound to parameter
    /// </summary>
    Array
}
