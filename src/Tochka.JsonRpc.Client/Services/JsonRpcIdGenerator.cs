using Tochka.JsonRpc.Common.Models.Id;

namespace Tochka.JsonRpc.Client.Services;

/// <inheritdoc />
public sealed class JsonRpcIdGenerator : IJsonRpcIdGenerator
{
    /// <inheritdoc />
    /// <summary>
    /// Creates string id from Guid
    /// </summary>
    public IRpcId GenerateId()
    {
        var value = Guid.NewGuid().ToString();
        var result = new StringRpcId(value);
        return result;
    }
}
