using Tochka.JsonRpc.Common.Models.Request;
using Tochka.JsonRpc.Common.Models.Response;

namespace Tochka.JsonRpc.Server;

internal class JsonRpcFeature
{
    public ICall? Call { get; set; }
    public IResponse? Response { get; set; }
    public bool IsBatch { get; set; }
}
