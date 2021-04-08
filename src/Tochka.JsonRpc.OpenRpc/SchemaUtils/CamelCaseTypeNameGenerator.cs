using System.Collections.Generic;
using NJsonSchema;

namespace Tochka.JsonRpc.OpenRpc.SchemaUtils
{
    /// <summary>
    /// Uses default generator and converts first char to lowercase for compatibility with JSON serialization
    /// </summary>
    public class CamelCaseTypeNameGenerator : ITypeNameGenerator
    {
        private readonly DefaultTypeNameGenerator defaultTypeNameGenerator = new DefaultTypeNameGenerator();

        public string Generate(JsonSchema schema, string typeNameHint, IEnumerable<string> reservedTypeNames)
        {
            var name = defaultTypeNameGenerator.Generate(schema, typeNameHint, reservedTypeNames);
            return name[0].ToString().ToLowerInvariant() + (name.Length > 1 ? name.Substring(1) : string.Empty);


        }
    }
}