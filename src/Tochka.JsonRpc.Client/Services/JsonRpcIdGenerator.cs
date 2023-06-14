using Tochka.JsonRpc.Common.Models.Id;

namespace Tochka.JsonRpc.Client.Services;

public sealed class JsonRpcIdGenerator : IJsonRpcIdGenerator
{
    /// <summary>
    /// Creates string ID from GUID
    /// </summary>
    public IRpcId GenerateId()
    {
        var value = Guid.NewGuid().ToString();
        var result = new StringRpcId(value);
        return result;
    }
}
