using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;

namespace Tochka.JsonRpc.OpenRpc.Models;

/// <summary>
/// An object representing a Server Variable for server URL template substitution.
/// </summary>
[PublicAPI]
[ExcludeFromCodeCoverage]
public sealed record OpenRpcServerVariable
(
    string Default
)
{
    /// <summary>
    /// An enumeration of string values to be used if the substitution options are from a limited set.
    /// </summary>
    public string? Enum { get; set; }

    /// <summary>
    /// REQUIRED. The default value to use for substitution, which SHALL be sent if an alternate value is not supplied.
    /// Note this behavior is different than the Schema Object’s treatment of default values, because in those cases parameter values are optional.
    /// </summary>
    public string Default { get; set; } = Default;

    /// <summary>
    /// An optional description for the server variable. GitHub Flavored Markdown syntax MAY be used for rich text representation.
    /// </summary>
    public string? Description { get; set; }
}
