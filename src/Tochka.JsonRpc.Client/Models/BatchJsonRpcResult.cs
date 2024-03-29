﻿using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using JetBrains.Annotations;
using Tochka.JsonRpc.Common;
using Tochka.JsonRpc.Common.Models.Id;
using Tochka.JsonRpc.Common.Models.Response;
using Tochka.JsonRpc.Common.Models.Response.Errors;
using Tochka.JsonRpc.Common.Models.Response.Untyped;

namespace Tochka.JsonRpc.Client.Models;

/// <inheritdoc />
[PublicAPI]
public sealed class BatchJsonRpcResult : IBatchJsonRpcResult
{
    private readonly IJsonRpcCallContext context;
    private readonly JsonSerializerOptions headersJsonSerializerOptions;
    private readonly JsonSerializerOptions dataJsonSerializerOptions;
    private readonly Dictionary<IRpcId, IResponse> responses;

    /// <summary></summary>
    public BatchJsonRpcResult(IJsonRpcCallContext context, JsonSerializerOptions headersJsonSerializerOptions, JsonSerializerOptions dataJsonSerializerOptions)
    {
        this.context = context;
        if (context.SingleResponse != null)
        {
            throw new ArgumentOutOfRangeException(nameof(context), "Expected batch response");
        }

        responses = CreateDictionary(context.BatchResponse);
        this.headersJsonSerializerOptions = headersJsonSerializerOptions;
        this.dataJsonSerializerOptions = dataJsonSerializerOptions;
    }

    /// <inheritdoc />
    public TResponse? GetResponseOrThrow<TResponse>(IRpcId id)
    {
        if (!TryGetValue(id, out var response))
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
        TryGetValue(id, out var response);
        return response switch
        {
            UntypedResponse { Result: not null } untypedResponse => untypedResponse.Result.Deserialize<TResponse>(dataJsonSerializerOptions),
            _ => default
        };
    }

    /// <inheritdoc />
    public bool HasError(IRpcId id)
    {
        if (!TryGetValue(id, out var response))
        {
            throw new JsonRpcException($"Expected response id [{id}], got nothing", context);
        }

        return response is UntypedErrorResponse;
    }

    /// <inheritdoc />
    public Error<JsonDocument>? AsAnyError(IRpcId id)
    {
        TryGetValue(id, out var response);
        return response switch
        {
            UntypedErrorResponse untypedErrorResponse => untypedErrorResponse.Error,
            _ => null
        };
    }

    /// <inheritdoc />
    public Error<TError>? AsTypedError<TError>(IRpcId id)
    {
        TryGetValue(id, out var response);
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

    [ExcludeFromCodeCoverage]
    private bool TryGetValue(IRpcId id, [NotNullWhen(true)] out IResponse? response) =>
        responses.TryGetValue(id, out response);

    [ExcludeFromCodeCoverage]
    private static Dictionary<IRpcId, IResponse> CreateDictionary(IEnumerable<IResponse>? items) =>
        items?.ToDictionary(static x => x.Id, static x => x) ?? new Dictionary<IRpcId, IResponse>();
}
