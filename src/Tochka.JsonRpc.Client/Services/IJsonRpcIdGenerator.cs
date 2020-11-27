using Tochka.JsonRpc.Common.Models.Id;

namespace Tochka.JsonRpc.Client.Services
{
    public interface IJsonRpcIdGenerator
    {
        IRpcId GenerateId();
    }
}