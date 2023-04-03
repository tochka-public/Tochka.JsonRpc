namespace Tochka.JsonRpc.Server.Models.Response;

public interface IServerResponseWrapper
{
    Task Write(HandlingContext context, HeaderJsonRpcSerializer headerJsonRpcSerializer);
}