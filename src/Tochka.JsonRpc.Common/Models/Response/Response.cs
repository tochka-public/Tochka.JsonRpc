using System.Diagnostics.CodeAnalysis;
using Tochka.JsonRpc.Common.Models.Id;

namespace Tochka.JsonRpc.Common.Models.Response;

[ExcludeFromCodeCoverage]
public record Response<TResult>(IRpcId Id, TResult? Result, string Jsonrpc = JsonRpcConstants.Version) : IResponse
{
    // required for autodoc metadata generation
    internal Response() : this(null!)
    {
    }
}
