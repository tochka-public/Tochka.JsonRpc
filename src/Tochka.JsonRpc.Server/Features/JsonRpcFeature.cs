using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using JetBrains.Annotations;
using Tochka.JsonRpc.Common.Models.Request;
using Tochka.JsonRpc.Common.Models.Response;

namespace Tochka.JsonRpc.Server.Features;

/// <inheritdoc />
[PublicAPI]
[ExcludeFromCodeCoverage]
public class JsonRpcFeature : IJsonRpcFeature
{
    public JsonDocument? RawCall { get; set; }
    public ICall? Call { get; set; }
    public IResponse? Response { get; set; }
    public bool IsBatch { get; set; }
}
