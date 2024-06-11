using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using Tochka.JsonRpc.Common;
using Tochka.JsonRpc.Common.Models.Response;
using Tochka.JsonRpc.Common.Models.Response.Errors;
using Tochka.JsonRpc.Common.Models.Response.Untyped;

namespace Tochka.JsonRpc.Client.Models.Single;

/// <inheritdoc />
public class SingleJsonSingleJsonRpcResultAdvanced : ISingleJsonRpcResultAdvanced
{
    private readonly IJsonRpcCallContext context;
    private readonly JsonSerializerOptions headersJsonSerializerOptions;
    private readonly JsonSerializerOptions dataJsonSerializerOptions;
    private readonly IResponse? response;

    /// <summary></summary>
    public SingleJsonSingleJsonRpcResultAdvanced(IJsonRpcCallContext context, JsonSerializerOptions headersJsonSerializerOptions, JsonSerializerOptions dataJsonSerializerOptions)
    {
        response = context.SingleResponse;
        this.headersJsonSerializerOptions = headersJsonSerializerOptions;
        this.dataJsonSerializerOptions = dataJsonSerializerOptions;
        this.context = context;
    }

    /// <inheritdoc />
    public TResponse? GetResponseOrThrow<TResponse>() => response switch
    {
        null => throw new JsonRpcException($"Expected successful response with [{typeof(TResponse).Name}] params, got nothing", context),
        UntypedResponse { Result: null } => default,
        UntypedResponse untypedResponse => untypedResponse.Result.Deserialize<TResponse>(dataJsonSerializerOptions),
        UntypedErrorResponse untypedErrorResponse => throw new JsonRpcException($"Expected successful response with [{typeof(TResponse).Name}] params, got error", context.WithError(untypedErrorResponse)),
        _ => throw new ArgumentOutOfRangeException(nameof(response), response.GetType().Name)
    };
    
    /// <inheritdoc />
    public TResponse? AsResponse<TResponse>() => response switch
    {
        UntypedResponse { Result: not null } untypedResponse => untypedResponse.Result.Deserialize<TResponse>(dataJsonSerializerOptions),
        _ => default
    };
    
    /// <inheritdoc />
    public Error<JsonDocument>? AsAnyError() => response switch
    {
        UntypedErrorResponse untypedErrorResponse => untypedErrorResponse.Error,
        _ => null
    };

    /// <inheritdoc />
    public Error<TError>? AsTypedError<TError>() => response switch
    {
        UntypedErrorResponse untypedErrorResponse => new Error<TError>(untypedErrorResponse.Error.Code,
            untypedErrorResponse.Error.Message,
            Utils.DeserializeErrorData<TError>(untypedErrorResponse.Error.Data, headersJsonSerializerOptions, dataJsonSerializerOptions)),
        _ => null
    };

    /// <inheritdoc />
    [ExcludeFromCodeCoverage]
    public Error<ExceptionInfo>? AsErrorWithExceptionInfo() => AsTypedError<ExceptionInfo>();
}