using System.Text.Json;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Tochka.JsonRpc.Server.Binding.ParseResults;
using Tochka.JsonRpc.Server.Metadata;

namespace Tochka.JsonRpc.Server.Binding;

public interface IJsonRpcParameterBinder
{
    void SetResult(ModelBindingContext bindingContext, JsonRpcParameterMetadata parameterMetadata, IParseResult parseResult, JsonSerializerOptions jsonSerializerOptions);
}
