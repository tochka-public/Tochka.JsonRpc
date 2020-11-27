using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Tochka.JsonRpc.Server.Settings;

namespace Tochka.JsonRpc.Server.Models
{
    [ExcludeFromCodeCoverage]
    public class MethodMetadata
    {
        public JsonRpcMethodOptions MethodOptions { get; }
        public JsonName Controller { get; }
        public JsonName Action { get; }
        public string HumanReadableFullName => $"{Controller.Original}.{Action.Original}";

        internal readonly Dictionary<string, ParameterMetadata> ParametersInternal = new Dictionary<string, ParameterMetadata>();

        public IReadOnlyDictionary<string, ParameterMetadata> Parameters => ParametersInternal;

        public MethodMetadata(JsonRpcMethodOptions methodOptions, JsonName controller, JsonName action)
        {
            MethodOptions = methodOptions;
            Controller = controller ?? throw new ArgumentNullException(nameof(controller));
            Action = action ?? throw new ArgumentNullException(nameof(action));
        }

        public void Add(ParameterMetadata parameterMetadata)
        {
            ParametersInternal[parameterMetadata.Name.Original] = parameterMetadata;
        }

        public ParameterMetadata Get(string originalName)
        {
            if(ParametersInternal.TryGetValue(originalName, out var result))
            {
                return result;
            }

            return null;
        }

        public override string ToString()
        {
            return $"{HumanReadableFullName}({ParametersInternal.Count} args): {Controller.Json}.{Action.Json}, {MethodOptions.MethodStyle}";
        }
    }
}