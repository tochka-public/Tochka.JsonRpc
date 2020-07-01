using System.Diagnostics.CodeAnalysis;
using Tochka.JsonRpc.Common.Models.Request;
using Tochka.JsonRpc.Common.Models.Request.Untyped;
using Tochka.JsonRpc.Common.Serializers;

namespace Tochka.JsonRpc.Server.Models.Binding
{
    [ExcludeFromCodeCoverage]
    public class JsonRpcBindingContext
    {
        public IUntypedCall Call { get; set; }
        public ParameterMetadata ParameterMetadata { get; set; }
        public IJsonRpcSerializer Serializer { get; set; }
    }
}