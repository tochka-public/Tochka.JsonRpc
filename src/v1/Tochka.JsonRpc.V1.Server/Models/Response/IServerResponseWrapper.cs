using System.Threading.Tasks;
using Tochka.JsonRpc.V1.Common.Serializers;

namespace Tochka.JsonRpc.V1.Server.Models.Response
{
    public interface IServerResponseWrapper
    {
        Task Write(HandlingContext context, HeaderJsonRpcSerializer headerJsonRpcSerializer);
    }
}
