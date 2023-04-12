using System.Text.Json;
using Tochka.JsonRpc.Common;

namespace Tochka.JsonRpc.Server.Serialization;

internal class CamelCaseJsonSerializerOptionsProvider : IJsonSerializerOptionsProvider
{
    public JsonSerializerOptions Options => JsonRpcSerializerOptions.CamelCase;
}
