using Microsoft.Extensions.Options;
using Tochka.JsonRpc.Common.Models.Id;
using Tochka.JsonRpc.Common.Models.Response.Untyped;
using Tochka.JsonRpc.Server.Extensions;
using Tochka.JsonRpc.Server.Settings;

namespace Tochka.JsonRpc.Server.Services;

internal class JsonRpcExceptionWrapper : IJsonRpcExceptionWrapper
{
    private readonly IJsonRpcErrorFactory errorFactory;
    private readonly JsonRpcServerOptions options;

    public JsonRpcExceptionWrapper(IJsonRpcErrorFactory errorFactory, IOptions<JsonRpcServerOptions> options)
    {
        this.errorFactory = errorFactory;
        this.options = options.Value;
    }

    public UntypedErrorResponse WrapGeneralException(Exception exception, IRpcId? id = null)
    {
        var error = errorFactory.Exception(exception);
        return new UntypedErrorResponse(id ?? new NullRpcId(), error.AsUntypedError(options.HeadersJsonSerializerOptions));
    }

    public UntypedErrorResponse WrapParseException(Exception exception)
    {
        var error = errorFactory.ParseError(exception);
        return new UntypedErrorResponse(new NullRpcId(), error.AsUntypedError(options.HeadersJsonSerializerOptions));
    }
}
