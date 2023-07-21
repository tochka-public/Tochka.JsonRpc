using Newtonsoft.Json;

namespace Tochka.JsonRpc.V1.Common.Serializers
{
    public interface IJsonRpcSerializer
    {
        JsonSerializerSettings Settings { get; }
        JsonSerializer Serializer { get; }
    }
}
