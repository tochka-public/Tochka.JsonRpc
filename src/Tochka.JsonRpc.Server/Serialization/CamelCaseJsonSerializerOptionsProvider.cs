using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using Tochka.JsonRpc.Common;

namespace Tochka.JsonRpc.Server.Serialization;

[ExcludeFromCodeCoverage]
public class CamelCaseJsonSerializerOptionsProvider : IJsonSerializerOptionsProvider
{
    public JsonSerializerOptions Options => JsonRpcSerializerOptions.CamelCase;
}
