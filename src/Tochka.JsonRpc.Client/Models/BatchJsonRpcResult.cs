using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using Tochka.JsonRpc.Common.Models.Id;
using Tochka.JsonRpc.Common.Models.Response;
using Tochka.JsonRpc.Common.Models.Response.Errors;
using Tochka.JsonRpc.Common.Models.Response.Untyped;
using Tochka.JsonRpc.Common.Serializers;

namespace Tochka.JsonRpc.Client.Models
{
    public interface IBatchJsonRpcResult
    {
        T GetResponse<T>(IRpcId id);
        T AsResponse<T>(IRpcId id);
        Error<JToken> AsRawError(IRpcId id);
        Error<T> AsError<T>(IRpcId id);
        Error<ExceptionInfo> AsErrorWithExceptionInfo(IRpcId id);
    }

    public class BatchJsonRpcResult : IBatchJsonRpcResult
    {
        private readonly IJsonRpcCallContext context;
        private readonly HeaderRpcSerializer headerRpcSerializer;
        private readonly IRpcSerializer serializer;
        private readonly Dictionary<IRpcId, IResponse> responses;

        public BatchJsonRpcResult(IJsonRpcCallContext context, HeaderRpcSerializer headerRpcSerializer, IRpcSerializer serializer)
        {
            this.context = context ?? throw new ArgumentNullException(nameof(context));
            if (context.SingleResponse != null)
            {
                throw new ArgumentOutOfRangeException(nameof(context), "Expected batch response");
            }
            this.responses = CreateDictionary(context.BatchResponse);
            this.headerRpcSerializer = headerRpcSerializer;
            this.serializer = serializer;
        }

        private static Dictionary<IRpcId, IResponse> CreateDictionary(List<IResponse> items)
        {
            return items.ToDictionary(x => x.Id ?? NullId, x => x);
        }

        public T GetResponse<T>(IRpcId id)
        {
            if (!TryGetValue(id, out var response))
            {
                throw new JsonRpcException($"Expected successful response id [{id}] with [{typeof(T).Name}] params, got nothing", context);
            }

            switch (response)
            {
                case UntypedResponse untypedResponse:
                    return untypedResponse.Result.ToObject<T>(serializer.Serializer);
                case UntypedErrorResponse untypedErrorResponse:
                    context.WithError(untypedErrorResponse.Error);
                    throw new JsonRpcException($"Expected successful response id [{id}] with [{typeof(T).Name}] params, got error", context);
                default:
                    throw new ArgumentOutOfRangeException(nameof(response), response.GetType().Name);
            }
        }

        public T AsResponse<T>(IRpcId id)
        {
            TryGetValue(id, out var response);
            if (response is UntypedResponse untypedResponse)
            {
                return untypedResponse.Result.ToObject<T>(serializer.Serializer);
            }
            return default(T);
        }

        public Error<JToken> AsRawError(IRpcId id)
        {
            TryGetValue(id, out var response);
            if (response is UntypedErrorResponse untypedErrorResponse)
            {
                return untypedErrorResponse.Error;
            }

            return null;
        }

        public Error<T> AsError<T>(IRpcId id)
        {
            TryGetValue(id, out var response);
            if (response is UntypedErrorResponse untypedErrorResponse)
            {
                var error = untypedErrorResponse.Error;
                var data = error.Data.ToObject<T>(serializer.Serializer);
                if (data.Equals(default(T)))
                {
                    // if user serializer failed: maybe this is server error, try header serializer
                    data = error.Data.ToObject<T>(headerRpcSerializer.Serializer);
                }

                return new Error<T>()
                {
                    Code = error.Code,
                    Message = error.Message,
                    Data = data
                };
            }

            return null;
        }

        public Error<ExceptionInfo> AsErrorWithExceptionInfo(IRpcId id)
        {
            return AsError<ExceptionInfo>(id);
        }

        private bool TryGetValue(IRpcId id, out IResponse response)
        {
            return responses.TryGetValue(id ?? NullId, out response);
        }

        /// <summary>
        /// Dummy value for storing responses in dictionary
        /// </summary>
        internal static IRpcId NullId = new NullRpcId();

        /// <summary>
        /// Dummy id type for storing responses in dictionary
        /// </summary>
        private class NullRpcId : IRpcId, IEquatable<NullRpcId>
        {
            public bool Equals(NullRpcId other)
            {
                return !ReferenceEquals(null, other);
            }

            public bool Equals(IRpcId other)
            {
                return Equals(other as NullRpcId);
            }
        }
    }
}