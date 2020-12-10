using System;
using System.Diagnostics.CodeAnalysis;

namespace Tochka.JsonRpc.Server.Models.Binding
{
    /// <summary>
    /// Indicate that json had null at given key
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class NullParseResult : IParseResult
    {
        public string Key { get; }

        public NullParseResult(string key)
        {
            Key = key ?? throw new ArgumentNullException(nameof(key));
        }

        public override string ToString()
        {
            return $"Bind value is null. Json key [{Key}]";
        }
    }
}