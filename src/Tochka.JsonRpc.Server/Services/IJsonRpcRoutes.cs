namespace Tochka.JsonRpc.Server.Services
{
    public interface IJsonRpcRoutes
    {
        void Register(string route);
        bool IsJsonRpcRoute(string route);
    }
}