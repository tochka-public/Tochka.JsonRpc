namespace Tochka.JsonRpc.V1.Server.Services
{
    public interface IJsonRpcRoutes
    {
        void Register(string route);
        bool IsJsonRpcRoute(string route);
    }
}