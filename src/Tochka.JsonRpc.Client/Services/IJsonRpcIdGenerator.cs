using JetBrains.Annotations;
using Tochka.JsonRpc.Common.Models.Id;

namespace Tochka.JsonRpc.Client.Services
{
    [PublicAPI]
    public interface IJsonRpcIdGenerator
    {
        IRpcId GenerateId();
    }
}
