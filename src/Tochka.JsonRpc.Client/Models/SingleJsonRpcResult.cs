using System.Text.Json;
using JetBrains.Annotations;
using Tochka.JsonRpc.Common;
using Tochka.JsonRpc.Common.Models.Response;
using Tochka.JsonRpc.Common.Models.Response.Errors;
using Tochka.JsonRpc.Common.Models.Response.Untyped;

namespace Tochka.JsonRpc.Client.Models;

[PublicAPI]
public sealed class SingleJsonRpcResult : ISingleJsonRpcResult
{
    private readonly IJsonRpcCallContext context;
    private readonly JsonSerializerOptions headersJsonSerializerOptions;
    private readonly JsonSerializerOptions dataJsonSerializerOptions;
    private readonly IResponse? response;

    public SingleJsonRpcResult(IJsonRpcCallContext context, JsonSerializerOptions headersJsonSerializerOptions, JsonSerializerOptions dataJsonSerializerOptions)
    {
        this.context = context ?? throw new ArgumentNullException(nameof(context));
        if (context.BatchResponse != null)
        {
            throw new ArgumentOutOfRangeException(nameof(context), "Expected single response");
        }

        response = context.SingleResponse;
        this.headersJsonSerializerOptions = headersJsonSerializerOptions;
        this.dataJsonSerializerOptions = dataJsonSerializerOptions;
    }

    public TResponse? GetResponseOrThrow<TResponse>() => response switch
    {
        null => throw new JsonRpcException($"Expected successful response with [{typeof(TResponse).Name}] params, got nothing", context),
        UntypedResponse { Result: null } => default,
        UntypedResponse untypedResponse => untypedResponse.Result.Deserialize<TResponse>(dataJsonSerializerOptions),
        UntypedErrorResponse untypedErrorResponse => throw new JsonRpcException($"Expected successful response with [{typeof(TResponse).Name}] params, got error", context.WithError(untypedErrorResponse)),
        _ => throw new ArgumentOutOfRangeException(nameof(response), response.GetType().Name)
    };

    public TResponse? AsResponse<TResponse>() => response switch
    {
        UntypedResponse { Result: { } } untypedResponse => untypedResponse.Result.Deserialize<TResponse>(dataJsonSerializerOptions),
        _ => default
    };

    public bool HasError() => response is UntypedErrorResponse;

    public Error<JsonDocument>? AsAnyError() => response switch
    {
        UntypedErrorResponse untypedErrorResponse => untypedErrorResponse.Error,
        _ => null
    };

    public Error<TError>? AsTypedError<TError>() => response switch
    {
        UntypedErrorResponse untypedErrorResponse => new Error<TError>(untypedErrorResponse.Error.Code,
            untypedErrorResponse.Error.Message,
            Utils.DeserializeErrorData<TError>(untypedErrorResponse.Error.Data, headersJsonSerializerOptions, dataJsonSerializerOptions)),
        _ => null
    };

    public Error<ExceptionInfo>? AsErrorWithExceptionInfo() => AsTypedError<ExceptionInfo>();
}
