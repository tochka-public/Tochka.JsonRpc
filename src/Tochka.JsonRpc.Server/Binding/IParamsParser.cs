using Newtonsoft.Json.Linq;
using Tochka.JsonRpc.Server.Models;
using Tochka.JsonRpc.Server.Models.Binding;

namespace Tochka.JsonRpc.Server.Binding
{
    public interface IParamsParser
    {
        IParseResult ParseParams(JToken jsonParams, ParameterMetadata parameterMetadata);
    }
}