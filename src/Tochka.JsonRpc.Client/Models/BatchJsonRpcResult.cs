using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using Tochka.JsonRpc.Common;
using Tochka.JsonRpc.Common.Models.Id;
using Tochka.JsonRpc.Common.Models.Response;
using Tochka.JsonRpc.Common.Models.Response.Errors;
using Tochka.JsonRpc.Common.Models.Response.Untyped;

namespace Tochka.JsonRpc.Client.Models;

internal class BatchJsonRpcResult : IBatchJsonRpcResult
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

    public bool HasResponse(IRpcId? id) => TryGetValue(id, out _);

    public bool HasError(IRpcId? id)
    {
        TryGetValue(id, out var response);
        return response is UntypedErrorResponse;
    }

    public Error<JsonDocument>? AsUntypedError(IRpcId? id)
    {
        TryGetValue(id, out var response);
        return response switch
        {
            UntypedErrorResponse untypedErrorResponse => untypedErrorResponse.Error,
            _ => null
        };
    }

    public Error<T>? AsError<T>(IRpcId? id)
    {
        TryGetValue(id, out var response);
        return response switch
        {
            UntypedErrorResponse untypedErrorResponse => new Error<T>
            {
                Code = untypedErrorResponse.Error.Code,
                Message = untypedErrorResponse.Error.Message,
                Data = Utils.DeserializeErrorData<T>(untypedErrorResponse.Error.Data, headersJsonSerializerOptions, dataJsonSerializerOptions)
            },
            _ => null
        };
    }

    public Error<ExceptionInfo>? AsErrorWithExceptionInfo(IRpcId? id) => AsError<ExceptionInfo>(id);

    private bool TryGetValue(IRpcId? id, [NotNullWhen(true)] out IResponse? response) =>
        responses.TryGetValue(id ?? NullId, out response);

    private static Dictionary<IRpcId, IResponse> CreateDictionary(IEnumerable<IResponse>? items) =>
        items?.ToDictionary(static x => x.Id ?? NullId, static x => x) ?? new Dictionary<IRpcId, IResponse>();

    /// <summary>
    /// Dummy value for storing responses in dictionary
    /// </summary>
    private static readonly IRpcId NullId = new NullRpcId();

    /// <inheritdoc cref="Tochka.JsonRpc.Common.Models.Id.IRpcId" />
    /// <summary>
    /// Dummy id type for storing responses in dictionary
    /// </summary>
    private class NullRpcId : IRpcId, IEquatable<NullRpcId>
    {
        public bool Equals(NullRpcId? other) => !ReferenceEquals(null, other);

        public bool Equals(IRpcId? other) => Equals(other as NullRpcId);

        public override bool Equals(object? obj) => Equals(obj as NullRpcId);

        public override int GetHashCode() => 0;
    }
}
