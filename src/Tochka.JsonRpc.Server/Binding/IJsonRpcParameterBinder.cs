using System.Text.Json;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Tochka.JsonRpc.Server.Binding.ParseResults;
using Tochka.JsonRpc.Server.Metadata;

namespace Tochka.JsonRpc.Server.Binding;

/// <summary>
/// Service to bind parameter parse result to actual action arguments
/// </summary>
[PublicAPI]
public interface IJsonRpcParameterBinder
{
    /// <summary>
    /// Bind parameter parse result to action arguments
    /// </summary>
    /// <param name="bindingContext">Context with information for model binding and validation</param>
    /// <param name="parameterMetadata">Metadata for parameter to bind</param>
    /// <param name="parseResult">JSON-RPC parameter parse result</param>
    /// <param name="jsonSerializerOptions">Data serializer options</param>
    void SetResult(ModelBindingContext bindingContext, JsonRpcParameterMetadata parameterMetadata, IParseResult parseResult, JsonSerializerOptions jsonSerializerOptions);
}
