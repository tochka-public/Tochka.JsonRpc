using System;
using Newtonsoft.Json.Linq;
using Tochka.JsonRpc.Common.Models.Response;
using Tochka.JsonRpc.Common.Models.Response.Errors;
using Tochka.JsonRpc.Common.Models.Response.Untyped;
using Tochka.JsonRpc.Common.Serializers;

namespace Tochka.JsonRpc.Client.Models
{
    public interface ISingleJsonRpcResult
    {
        T GetResponse<T>();
        T AsResponse<T>();
        Error<JToken> AsRawError();
        Error<T> AsError<T>();
        Error<ExceptionInfo> AsErrorWithExceptionInfo();
    }

    public class SingleJsonRpcResult : ISingleJsonRpcResult
    {
        private readonly IJsonRpcCallContext context;
        private readonly HeaderRpcSerializer headerRpcSerializer;
        private readonly IRpcSerializer serializer;
        private readonly IResponse response;

        public SingleJsonRpcResult(IJsonRpcCallContext context, HeaderRpcSerializer headerRpcSerializer, IRpcSerializer serializer)
        {
            this.context = context ?? throw new ArgumentNullException(nameof(context));
            if (context.BatchResponse != null)
            {
                throw new ArgumentOutOfRangeException(nameof(context), "Expected single response");
            }

            this.response = context.SingleResponse;
            this.headerRpcSerializer = headerRpcSerializer;
            this.serializer = serializer;
        }

        public T GetResponse<T>()
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

        public Error<JToken> AsRawError()
        {
            if (response is UntypedErrorResponse untypedErrorResponse)
            {
                return untypedErrorResponse.Error;
            }

            return null;
        }

        public Error<T> AsError<T>()
        {
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

        public Error<ExceptionInfo> AsErrorWithExceptionInfo()
        {
            return AsError<ExceptionInfo>();
        }
    }
}