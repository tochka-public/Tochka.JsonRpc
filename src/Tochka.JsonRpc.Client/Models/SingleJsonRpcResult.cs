using System.Text.Json;
using Tochka.JsonRpc.Common;
using Tochka.JsonRpc.Common.Models.Response;
using Tochka.JsonRpc.Common.Models.Response.Errors;
using Tochka.JsonRpc.Common.Models.Response.Untyped;

namespace Tochka.JsonRpc.Client.Models;

public class SingleJsonRpcResult : ISingleJsonRpcResult
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

    public T? GetResponseOrThrow<T>()
    {
        switch (response)
        {
            case null:
                throw new JsonRpcException($"Expected successful response with [{typeof(T).Name}] params, got nothing", context);
            case UntypedResponse untypedResponse:
                return untypedResponse.Result.Deserialize<T>(dataJsonSerializerOptions);
            case UntypedErrorResponse untypedErrorResponse:
                context.WithError(untypedErrorResponse);
                throw new JsonRpcException($"Expected successful response with [{typeof(T).Name}] params, got error", context);
            default:
                throw new ArgumentOutOfRangeException(nameof(response), response.GetType().Name);
        }
    }

    public T? AsResponse<T>() => response switch
    {
        UntypedResponse untypedResponse => untypedResponse.Result.Deserialize<T>(dataJsonSerializerOptions),
        _ => default
    };

    public bool HasError() => response is UntypedErrorResponse;

    public Error<JsonDocument>? AsUntypedError() => response switch
    {
        UntypedErrorResponse untypedErrorResponse => untypedErrorResponse.Error,
        _ => null
    };

    public Error<T>? AsError<T>() => response switch
    {
        UntypedErrorResponse untypedErrorResponse => new Error<T>
        {
            Code = untypedErrorResponse.Error.Code,
            Message = untypedErrorResponse.Error.Message,
            Data = Utils.DeserializeErrorData<T>(untypedErrorResponse.Error.Data, headersJsonSerializerOptions, dataJsonSerializerOptions)
        },
        _ => null
    };

    public Error<ExceptionInfo>? AsErrorWithExceptionInfo() => AsError<ExceptionInfo>();
}
