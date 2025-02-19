using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using Tochka.JsonRpc.Common.Models.Request;
using Tochka.JsonRpc.Common.Models.Response;

namespace Tochka.JsonRpc.Common.Features;

/// <inheritdoc />
[ExcludeFromCodeCoverage]
public class JsonRpcFeature : IJsonRpcFeature
{
    /// <inheritdoc />
    public JsonDocument? RawCall { get; set; }

    /// <inheritdoc />
    public ICall? Call { get; set; }

    /// <inheritdoc />
    public IResponse? Response { get; set; }

    /// <inheritdoc />
    public bool IsBatch { get; set; }
}
