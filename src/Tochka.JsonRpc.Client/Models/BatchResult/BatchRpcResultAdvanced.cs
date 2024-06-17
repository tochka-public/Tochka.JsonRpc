using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using Tochka.JsonRpc.Common;
using Tochka.JsonRpc.Common.Models.Id;
using Tochka.JsonRpc.Common.Models.Response;
using Tochka.JsonRpc.Common.Models.Response.Errors;
using Tochka.JsonRpc.Common.Models.Response.Untyped;

namespace Tochka.JsonRpc.Client.Models.BatchResult;

/// <inheritdoc />
public class BatchJsonRpcResultAdvanced : IBatchJsonRpcResultAdvanced
{
    private readonly JsonSerializerOptions headersJsonSerializerOptions;
    private readonly JsonSerializerOptions dataJsonSerializerOptions;
    private readonly Dictionary<IRpcId, IResponse> responses;
    private readonly IJsonRpcCallContext context;

    /// <summary></summary>
    public BatchJsonRpcResultAdvanced(IJsonRpcCallContext context, Dictionary<IRpcId, IResponse> responses,
        JsonSerializerOptions headersJsonSerializerOptions, JsonSerializerOptions dataJsonSerializerOptions)
    {
        this.context = context;
        this.responses = responses;
        this.headersJsonSerializerOptions = headersJsonSerializerOptions;
        this.dataJsonSerializerOptions = dataJsonSerializerOptions;
    }

    /// <inheritdoc />
    public TResponse? GetResponseOrThrow<TResponse>(IRpcId id)
    {
        if (!responses.TryGetResponse(id, out var response))
        {
            throw new JsonRpcException($"Expected successful response with id [{id}] and [{typeof(TResponse).Name}] params, got nothing", context);
        }

        return response switch
        {
            UntypedResponse { Result: null } => default,
            UntypedResponse untypedResponse => untypedResponse.Result.Deserialize<TResponse>(dataJsonSerializerOptions),
            UntypedErrorResponse untypedErrorResponse => throw new JsonRpcException($"Expected successful response with id [{id}] and [{typeof(TResponse).Name}] params, got error", context.WithError(untypedErrorResponse)),
            _ => throw new ArgumentOutOfRangeException(nameof(response), response.GetType().Name)
        };
    }

    /// <inheritdoc />
    public TResponse? AsResponse<TResponse>(IRpcId id)
    {
        responses.TryGetResponse(id, out var response);
        return response switch
        {
            UntypedResponse { Result: not null } untypedResponse => untypedResponse.Result.Deserialize<TResponse>(dataJsonSerializerOptions),
            _ => default
        };
    }

    /// <inheritdoc />
    public Error<JsonDocument>? AsAnyError(IRpcId id)
    {
        responses.TryGetResponse(id, out var response);
        return response switch
        {
            UntypedErrorResponse untypedErrorResponse => untypedErrorResponse.Error,
            _ => null
        };
    }

    /// <inheritdoc />
    public Error<TError>? AsTypedError<TError>(IRpcId id)
    {
        responses.TryGetResponse(id, out var response);
        return response switch
        {
            UntypedErrorResponse untypedErrorResponse => new Error<TError>(untypedErrorResponse.Error.Code,
                untypedErrorResponse.Error.Message,
                Utils.DeserializeErrorData<TError>(untypedErrorResponse.Error.Data, headersJsonSerializerOptions, dataJsonSerializerOptions)),
            _ => null
        };
    }

    /// <inheritdoc />
    [ExcludeFromCodeCoverage]
    public Error<ExceptionInfo>? AsErrorWithExceptionInfo(IRpcId id) => AsTypedError<ExceptionInfo>(id);
}
