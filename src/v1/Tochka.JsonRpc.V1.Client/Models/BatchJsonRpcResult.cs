using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using Tochka.JsonRpc.V1.Common.Models.Id;
using Tochka.JsonRpc.V1.Common.Models.Response;
using Tochka.JsonRpc.V1.Common.Models.Response.Errors;
using Tochka.JsonRpc.V1.Common.Models.Response.Untyped;
using Tochka.JsonRpc.V1.Common.Serializers;

namespace Tochka.JsonRpc.V1.Client.Models
{
    public class BatchJsonRpcResult : IBatchJsonRpcResult
    {
        private readonly IJsonRpcCallContext context;
        private readonly HeaderJsonRpcSerializer headerJsonRpcSerializer;
        private readonly IJsonRpcSerializer serializer;
        private readonly Dictionary<IRpcId, IResponse> responses;

        public BatchJsonRpcResult(IJsonRpcCallContext context, HeaderJsonRpcSerializer headerJsonRpcSerializer, IJsonRpcSerializer serializer)
        {
            this.context = context ?? throw new ArgumentNullException(nameof(context));
            if (context.SingleResponse != null)
            {
                throw new ArgumentOutOfRangeException(nameof(context), "Expected batch response");
            }
            this.responses = CreateDictionary(context.BatchResponse);
            this.headerJsonRpcSerializer = headerJsonRpcSerializer;
            this.serializer = serializer;
        }

        private static Dictionary<IRpcId, IResponse> CreateDictionary(List<IResponse> items)
        {
            return items.ToDictionary(x => x.Id ?? NullId, x => x);
        }

        public T GetResponseOrThrow<T>(IRpcId id)
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
                    context.WithError(untypedErrorResponse);
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

        public bool HasError(IRpcId id)
        {
            if (!TryGetValue(id, out var response))
            {
                throw new JsonRpcException($"Expected response id [{id}], got nothing", context);
            }

            return response is UntypedErrorResponse;
        }

        public Error<JToken> AsAnyError(IRpcId id)
        {
            TryGetValue(id, out var response);
            if (response is UntypedErrorResponse untypedErrorResponse)
            {
                return untypedErrorResponse.Error;
            }

            return null;
        }

        public Error<T> AsTypedError<T>(IRpcId id)
        {
            TryGetValue(id, out var response);
            if (response is UntypedErrorResponse untypedErrorResponse)
            {
                var error = untypedErrorResponse.Error;
                var data = GetData<T>(error);
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
            return AsTypedError<ExceptionInfo>(id);
        }

        private bool TryGetValue(IRpcId id, out IResponse response)
        {
            return responses.TryGetValue(id ?? NullId, out response);
        }

        private T GetData<T>(Error<JToken> error)
        {
            if (error.Data == null)
            {
                // if data was not present at all, do not throw
                return default(T);
            }

            var data = error.Data.ToObject<T>(serializer.Serializer);
            if (data.Equals(default(T)))
            {
                // if user serializer failed: maybe this is server error, try header serializer
                data = error.Data.ToObject<T>(headerJsonRpcSerializer.Serializer);
            }

            return data;
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
