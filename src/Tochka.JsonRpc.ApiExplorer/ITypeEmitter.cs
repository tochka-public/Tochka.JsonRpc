using System.Text.Json;
using System.Xml.Linq;

namespace Tochka.JsonRpc.ApiExplorer;

public interface ITypeEmitter
{
    Type CreateRequestType(string methodName, Type baseParamsType, IReadOnlyDictionary<string, Type> defaultBoundParams, Type? serializerOptionsProviderType);
    Type CreateResponseType(string methodName, Type resultType, Type? serializerOptionsProviderType);
}
