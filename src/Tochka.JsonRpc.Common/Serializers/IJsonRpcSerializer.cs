using Newtonsoft.Json;

namespace Tochka.JsonRpc.Common.Serializers
{
    public interface IJsonRpcSerializer
    {
        JsonSerializerSettings Settings { get; }
        JsonSerializer Serializer { get; }
    }
}