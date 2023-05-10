using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using Tochka.JsonRpc.Common.Models.Request;
using Tochka.JsonRpc.Common.Models.Response;

namespace Tochka.JsonRpc.Server.Features;

[SuppressMessage("Naming", "CA1716:Identifiers should not match keywords", Justification = "Call is official name")]
public interface IJsonRpcFeature
{
    JsonDocument? RawCall { get; set; }
    ICall? Call { get; set; }
    IResponse? Response { get; set; }
    bool IsBatch { get; set; }
}
