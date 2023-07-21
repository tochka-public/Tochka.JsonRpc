using Tochka.JsonRpc.V1.Common.Models.Id;

namespace Tochka.JsonRpc.V1.Client.Services
{
    public interface IJsonRpcIdGenerator
    {
        IRpcId GenerateId();
    }
}
