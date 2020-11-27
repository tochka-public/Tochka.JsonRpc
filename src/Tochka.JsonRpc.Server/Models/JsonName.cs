using System.Diagnostics.CodeAnalysis;

namespace Tochka.JsonRpc.Server.Models
{
    [ExcludeFromCodeCoverage]
    public class JsonName
    {
        /// <summary>
        /// Original property/method/action/controller/parameter name
        /// </summary>
        public string Original { get; }

        /// <summary>
        /// Serialized property/method/action/controller/parameter name
        /// </summary>
        public string Json { get; }

        public JsonName(string original, string json)
        {
            Original = original;
            Json = json;
        }

        public override string ToString()
        {
            return $"{Original}/{Json}";
        }
    }
}