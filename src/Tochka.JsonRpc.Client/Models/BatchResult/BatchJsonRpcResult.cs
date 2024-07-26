using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using JetBrains.Annotations;
using Tochka.JsonRpc.Common.Models.Id;
using Tochka.JsonRpc.Common.Models.Response;
using Tochka.JsonRpc.Common.Models.Response.Untyped;

namespace Tochka.JsonRpc.Client.Models.BatchResult;

/// <inheritdoc cref="IBatchJsonRpcResult" />
[PublicAPI]
public sealed class BatchJsonRpcResult : BatchJsonRpcResult<object>, IBatchJsonRpcResult
{
    /// <summary></summary>
    public BatchJsonRpcResult(IJsonRpcCallContext context, JsonSerializerOptions headersJsonSerializerOptions, JsonSerializerOptions dataJsonSerializerOptions)
        : base(context, headersJsonSerializerOptions, dataJsonSerializerOptions)
    {
    }

    /// <inheritdoc />
    public TResponse? GetResponseOrThrow<TResponse>(IRpcId id) => Advanced.GetResponseOrThrow<TResponse>(id);

    /// <inheritdoc />
    public TResponse? AsResponse<TResponse>(IRpcId id) => Advanced.AsResponse<TResponse>(id);
}

/// <inheritdoc />
[PublicAPI]
public class BatchJsonRpcResult<TResponse> : IBatchJsonRpcResult<TResponse>
{
    /// <inheritdoc />
    public IBatchJsonRpcResultAdvanced Advanced { get; init; }

    /// <summary>
    /// Context with all information about request and response
    /// </summary>
    protected readonly IJsonRpcCallContext Context;

    /// <summary>
    /// JsonSerializerOptions used to process JSON-RPC root object. Does not affect JSON-RPC params/result/error.data
    /// </summary>
    protected readonly JsonSerializerOptions HeadersJsonSerializerOptions;

    /// <summary>
    /// JsonSerializerOptions used to process JSON-RPC params/result/error.data. Does not affect JSON-RPC root object
    /// </summary>
    protected readonly JsonSerializerOptions DataJsonSerializerOptions;

    /// <summary>
    /// Collection of responses if call was batch
    /// </summary>
    protected readonly Dictionary<IRpcId, IResponse> Responses;

    /// <inheritdoc cref="IBatchJsonRpcResult{TResponse}" />
    public BatchJsonRpcResult(IJsonRpcCallContext context, JsonSerializerOptions headersJsonSerializerOptions, JsonSerializerOptions dataJsonSerializerOptions)
    {
        Context = context;
        if (context.SingleResponse != null)
        {
            throw new ArgumentOutOfRangeException(nameof(context), "Expected batch response");
        }

        Responses = CreateDictionary(context.BatchResponse);
        HeadersJsonSerializerOptions = headersJsonSerializerOptions;
        DataJsonSerializerOptions = dataJsonSerializerOptions;
        Advanced = new BatchJsonRpcResultAdvanced(Context, Responses, HeadersJsonSerializerOptions, DataJsonSerializerOptions);
    }

    /// <inheritdoc />
    public TResponse? GetResponseOrThrow(IRpcId id) => Advanced.GetResponseOrThrow<TResponse>(id);

    /// <inheritdoc />
    public TResponse? AsResponse(IRpcId id) => Advanced.AsResponse<TResponse>(id);

    /// <inheritdoc />
    public bool HasError(IRpcId id)
    {
        if (!Responses.TryGetResponse(id, out var response))
        {
            throw new JsonRpcException($"Expected response id [{id}], got nothing", Context);
        }

        return response is UntypedErrorResponse;
    }

    [ExcludeFromCodeCoverage]
    private static Dictionary<IRpcId, IResponse> CreateDictionary(IEnumerable<IResponse>? items) =>
        items?.ToDictionary(static x => x.Id, static x => x) ?? new Dictionary<IRpcId, IResponse>();
}
