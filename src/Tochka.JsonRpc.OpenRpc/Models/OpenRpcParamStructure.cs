using JetBrains.Annotations;

namespace Tochka.JsonRpc.OpenRpc.Models;

/// <summary>
/// The expected format of the parameters.
/// </summary>
[PublicAPI]
public enum OpenRpcParamStructure
{
    /// <summary>
    /// params field is object or array
    /// </summary>
    Either,

    /// <summary>
    /// params field is object
    /// </summary>
    ByName,

    /// <summary>
    /// params field is array
    /// </summary>
    ByPosition
}
