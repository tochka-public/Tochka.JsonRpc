using Tochka.JsonRpc.V1.Server.Models;

namespace Tochka.JsonRpc.V1.Server.Services
{
    public interface IMethodMatcher
    {
        bool IsMatch(MethodMetadata methodMetadata, string method);
        string GetActionName(MethodMetadata methodMetadata);
    }
}
