using Tochka.JsonRpc.Common.Models.Id;

namespace Tochka.JsonRpc.Common.Models.Response;

public interface IResponse
{
    IRpcId? Id { get; set; }

    string Jsonrpc { get; set; }
}
