using System.Threading.Tasks;
using Tochka.JsonRpc.Common.Serializers;

namespace Tochka.JsonRpc.Server.Models.Response
{
    public interface IServerResponseWrapper
    {
        Task Write(HandlingContext context, HeaderRpcSerializer headerRpcSerializer);
    }
}