using System.Diagnostics.CodeAnalysis;
using Tochka.JsonRpc.Common.Models.Id;

namespace Tochka.JsonRpc.Common.Models.Response;

[ExcludeFromCodeCoverage]
public class Response<TResult> : IResponse
{
    public IRpcId Id { get; set; }

    public string Jsonrpc { get; set; } = JsonRpcConstants.Version;

    public TResult Result { get; set; }

    public override string ToString() => $"{nameof(Response<object>)}<{typeof(TResult).Name}>: {nameof(Id)} [{Id}], {nameof(Result)} [{Result}]";
}
