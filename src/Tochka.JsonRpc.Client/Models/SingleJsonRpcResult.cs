using System;
using Newtonsoft.Json.Linq;
using Tochka.JsonRpc.Common.Models.Response;
using Tochka.JsonRpc.Common.Models.Response.Errors;
using Tochka.JsonRpc.Common.Models.Response.Untyped;
using Tochka.JsonRpc.Common.Serializers;

namespace Tochka.JsonRpc.Client.Models
{
    public class SingleJsonRpcResult : ISingleJsonRpcResult
    {
        private readonly IJsonRpcCallContext context;
        private readonly HeaderJsonRpcSerializer headerJsonRpcSerializer;
        private readonly IJsonRpcSerializer serializer;
        private readonly IResponse response;

        public SingleJsonRpcResult(IJsonRpcCallContext context, HeaderJsonRpcSerializer headerJsonRpcSerializer, IJsonRpcSerializer serializer)
        {
            this.context = context ?? throw new ArgumentNullException(nameof(context));
            if (context.BatchResponse != null)
            {
                throw new ArgumentOutOfRangeException(nameof(context), "Expected single response");
            }

            this.response = context.SingleResponse;
            this.headerJsonRpcSerializer = headerJsonRpcSerializer;
            this.serializer = serializer;
        }

        public T GetResponseOrThrow<T>()
        {
            if (response == null)
            {
                throw new JsonRpcException($"Expected successful response with [{typeof(T).Name}] params, got nothing", context);
            }

            switch (response)
            {
                case UntypedResponse untypedResponse:
                    return untypedResponse.Result.ToObject<T>(serializer.Serializer);
                case UntypedErrorResponse untypedErrorResponse:
                    context.WithError(untypedErrorResponse.Error);
                    throw new JsonRpcException($"Expected successful response with [{typeof(T).Name}] paparamsyload, got error", context);
                default:
                    throw new ArgumentOutOfRangeException(nameof(response), response.GetType().Name);
            }
        }

        public T AsResponse<T>()
        {
            if (response is UntypedResponse untypedResponse)
            {
                return untypedResponse.Result.ToObject<T>(serializer.Serializer);
            }
            return default(T);
        }

        public bool HasError() => response is UntypedErrorResponse;

        public Error<JToken> AsAnyError()
        {
            if (response is UntypedErrorResponse untypedErrorResponse)
            {
                return untypedErrorResponse.Error;
            }

            return null;
        }

        public Error<T> AsTypedError<T>()
        {
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

        public Error<ExceptionInfo> AsErrorWithExceptionInfo()
        {
            return AsTypedError<ExceptionInfo>();
        }
    }
}