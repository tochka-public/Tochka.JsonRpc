using Tochka.JsonRpc.V1.Common.Models.Id;

namespace Tochka.JsonRpc.V1.Common.Models.Response
{
    public interface IResponse
    {
        IRpcId Id { get; set; }

        string Jsonrpc { get; set; }
    }
}
