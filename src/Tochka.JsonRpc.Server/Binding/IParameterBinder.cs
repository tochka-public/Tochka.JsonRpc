using System.Text.Json;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Tochka.JsonRpc.Server.Binding.ParseResults;

namespace Tochka.JsonRpc.Server.Binding;

internal interface IParameterBinder
{
    void SetResult(ModelBindingContext bindingContext, JsonRpcParameterMetadata parameterMetadata, IParseResult parseResult, JsonSerializerOptions jsonSerializerOptions);
}
