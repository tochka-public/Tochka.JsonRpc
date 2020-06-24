using Newtonsoft.Json;

namespace Tochka.JsonRpc.Common.Serializers
{
    public interface IRpcSerializer
    {
        JsonSerializerSettings Settings { get; }
        JsonSerializer Serializer { get; }
    }
}