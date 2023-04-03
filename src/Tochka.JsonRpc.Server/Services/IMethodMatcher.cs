using Tochka.JsonRpc.Server.Models;

namespace Tochka.JsonRpc.Server.Services;

public interface IMethodMatcher
{
    bool IsMatch(MethodMetadata methodMetadata, string method);
    string GetActionName(MethodMetadata methodMetadata);
}