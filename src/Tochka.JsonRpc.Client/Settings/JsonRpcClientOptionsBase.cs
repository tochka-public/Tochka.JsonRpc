using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;

namespace Tochka.JsonRpc.Client.Settings;

/// <summary>
/// Base class for JSON Rpc client options
/// </summary>
[PublicAPI]
[ExcludeFromCodeCoverage]
public abstract class JsonRpcClientOptionsBase
{
    /// <summary>
    /// Base HTTP endpoint URL
    /// </summary>
    public virtual string Url { get; set; } = null!;

    /// <summary>
    /// HTTP request timeout
    /// </summary>
    /// <remarks>
    /// Default value is 10 seconds
    /// </remarks>
    public virtual TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(10);
}
