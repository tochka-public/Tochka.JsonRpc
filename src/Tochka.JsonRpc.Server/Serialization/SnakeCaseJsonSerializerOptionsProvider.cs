using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using Tochka.JsonRpc.Common;

namespace Tochka.JsonRpc.Server.Serialization;

[ExcludeFromCodeCoverage]
public class SnakeCaseJsonSerializerOptionsProvider : IJsonSerializerOptionsProvider
{
    public JsonSerializerOptions Options => JsonRpcSerializerOptions.SnakeCase;
}
