using System;
using System.Diagnostics.CodeAnalysis;

namespace Tochka.JsonRpc.V1.Server.Models.Binding
{
    /// <summary>
    /// Indicate that json had no given key
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class NoParseResult : IParseResult
    {
        public string Key { get; }

        public NoParseResult(string key)
        {
            Key = key ?? throw new ArgumentNullException(nameof(key));
        }

        public override string ToString()
        {
            return $"Bind value not found. Json key [{Key}]";
        }
    }
}