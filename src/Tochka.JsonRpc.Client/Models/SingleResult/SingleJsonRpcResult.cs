using System.Text.Json;
using JetBrains.Annotations;
using Tochka.JsonRpc.Common.Models.Response;
using Tochka.JsonRpc.Common.Models.Response.Untyped;

namespace Tochka.JsonRpc.Client.Models.SingleResult;

/// <inheritdoc cref="ISingleJsonRpcResult" />
[PublicAPI]
public sealed class SingleJsonRpcResult : SingleJsonRpcResult<object>, ISingleJsonRpcResult
{
    /// <summary></summary>
    public SingleJsonRpcResult(IJsonRpcCallContext context, JsonSerializerOptions headersJsonSerializerOptions, JsonSerializerOptions dataJsonSerializerOptions)
        : base(context, headersJsonSerializerOptions, dataJsonSerializerOptions)
    {
    }

    /// <inheritdoc />
    public TResponse? GetResponseOrThrow<TResponse>() => Advanced.GetResponseOrThrow<TResponse>();

    /// <inheritdoc />
    public TResponse? AsResponse<TResponse>() => Advanced.AsResponse<TResponse>();
}


/// <inheritdoc />
[PublicAPI]
public class SingleJsonRpcResult<TResponse> : ISingleJsonRpcResult<TResponse>
{
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
    /// Response if call was request
    /// </summary>
    protected readonly IResponse? Response;

    /// <inheritdoc />
    public ISingleJsonRpcResultAdvanced Advanced { get; init; }

    /// <summary></summary>
    public SingleJsonRpcResult(IJsonRpcCallContext context, JsonSerializerOptions headersJsonSerializerOptions, JsonSerializerOptions dataJsonSerializerOptions)
    {
        Context = context;
        if (context.BatchResponse != null)
        {
            throw new ArgumentOutOfRangeException(nameof(context), "Expected single response");
        }

        Response = context.SingleResponse;
        HeadersJsonSerializerOptions = headersJsonSerializerOptions;
        DataJsonSerializerOptions = dataJsonSerializerOptions;
        Advanced = new SingleJsonSingleJsonRpcResultAdvanced(Context, HeadersJsonSerializerOptions, DataJsonSerializerOptions);
    }

    /// <inheritdoc />
    public TResponse? GetResponseOrThrow() => Advanced.GetResponseOrThrow<TResponse>();

    /// <inheritdoc />
    public TResponse? AsResponse() => Advanced.AsResponse<TResponse>();

    /// <inheritdoc />
    public bool HasError() => Response is UntypedErrorResponse;
}
