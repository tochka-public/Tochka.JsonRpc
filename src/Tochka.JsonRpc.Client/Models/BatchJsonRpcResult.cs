using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using Tochka.JsonRpc.Common;
using Tochka.JsonRpc.Common.Models.Id;
using Tochka.JsonRpc.Common.Models.Response;
using Tochka.JsonRpc.Common.Models.Response.Errors;
using Tochka.JsonRpc.Common.Models.Response.Untyped;

namespace Tochka.JsonRpc.Client.Models;

public class BatchJsonRpcResult : IBatchJsonRpcResult
{
    private readonly IJsonRpcCallContext context;
    private readonly JsonSerializerOptions headersJsonSerializerOptions;
    private readonly JsonSerializerOptions dataJsonSerializerOptions;
    private readonly Dictionary<IRpcId, IResponse> responses;

    public BatchJsonRpcResult(IJsonRpcCallContext context, JsonSerializerOptions headersJsonSerializerOptions, JsonSerializerOptions dataJsonSerializerOptions)
    {
        this.context = context ?? throw new ArgumentNullException(nameof(context));
        if (context.SingleResponse != null)
        {
            throw new ArgumentOutOfRangeException(nameof(context), "Expected batch response");
        }

        responses = CreateDictionary(context.BatchResponse);
        this.headersJsonSerializerOptions = headersJsonSerializerOptions;
        this.dataJsonSerializerOptions = dataJsonSerializerOptions;
    }

    public T? GetResponseOrThrow<T>(IRpcId? id)
    {
        if (!TryGetValue(id, out var response))
        {
            throw new JsonRpcException($"Expected successful response with id [{id}] and [{typeof(T).Name}] params, got nothing", context);
        }

        switch (response)
        {
            case UntypedResponse untypedResponse:
                return untypedResponse.Result.Deserialize<T>(dataJsonSerializerOptions);
            case UntypedErrorResponse untypedErrorResponse:
                context.WithError(untypedErrorResponse);
                throw new JsonRpcException($"Expected successful response with id [{id}] and [{typeof(T).Name}] params, got error", context);
            default:
                throw new ArgumentOutOfRangeException(nameof(response), response.GetType().Name);
        }
    }

    public T? AsResponse<T>(IRpcId? id)
    {
        TryGetValue(id, out var response);
        return response switch
        {
            UntypedResponse untypedResponse => untypedResponse.Result.Deserialize<T>(dataJsonSerializerOptions),
            _ => default
        };
    }

    public bool HasError(IRpcId? id)
    {
        if (!TryGetValue(id, out var response))
        {
            throw new JsonRpcException($"Expected response id [{id}], got nothing", context);
        }

        return response is UntypedErrorResponse;
    }

    public Error<JsonDocument>? AsAnyError(IRpcId? id)
    {
        TryGetValue(id, out var response);
        return response switch
        {
            UntypedErrorResponse untypedErrorResponse => untypedErrorResponse.Error,
            _ => null
        };
    }

    public Error<T>? AsTypedError<T>(IRpcId? id)
    {
        TryGetValue(id, out var response);
        return response switch
        {
            UntypedErrorResponse untypedErrorResponse => new Error<T>(untypedErrorResponse.Error.Code,
                untypedErrorResponse.Error.Message,
                Utils.DeserializeErrorData<T>(untypedErrorResponse.Error.Data, headersJsonSerializerOptions, dataJsonSerializerOptions)),
            _ => null
        };
    }

    public Error<ExceptionInfo>? AsErrorWithExceptionInfo(IRpcId? id) => AsTypedError<ExceptionInfo>(id);

    private bool TryGetValue(IRpcId? id, [NotNullWhen(true)] out IResponse? response) =>
        responses.TryGetValue(id ?? NullId, out response);

    private static Dictionary<IRpcId, IResponse> CreateDictionary(IEnumerable<IResponse>? items) =>
        items?.ToDictionary(static x => x.Id, static x => x) ?? new Dictionary<IRpcId, IResponse>();

    private static readonly IRpcId NullId = new NullRpcId();
}
