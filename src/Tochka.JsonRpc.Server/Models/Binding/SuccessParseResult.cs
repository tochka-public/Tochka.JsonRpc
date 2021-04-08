using System;
using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json.Linq;

namespace Tochka.JsonRpc.Server.Models.Binding
{
    /// <summary>
    /// Indicate that json had no-null value at given key
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class SuccessParseResult : IParseResult
    {
        public string Key { get; }

        public JToken Value { get; }

        public SuccessParseResult(JToken value, string key)
        {
            Key = key ?? throw new ArgumentNullException(nameof(key));
            Value = value ?? throw new ArgumentNullException(nameof(value));
        }

        public override string ToString()
        {
            return $"Bind value is [{Value.Type}]. Json key [{Key}]";
        }
    }
}