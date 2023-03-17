using Newtonsoft.Json.Linq;
using Tochka.JsonRpc.V1.Server.Models;
using Tochka.JsonRpc.V1.Server.Models.Binding;

namespace Tochka.JsonRpc.V1.Server.Binding
{
    public interface IParamsParser
    {
        IParseResult ParseParams(JToken jsonParams, ParameterMetadata parameterMetadata);
    }
}