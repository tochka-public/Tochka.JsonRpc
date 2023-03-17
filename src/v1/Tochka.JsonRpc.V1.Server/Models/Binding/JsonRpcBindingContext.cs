using System.Diagnostics.CodeAnalysis;
using Tochka.JsonRpc.V1.Common.Models.Request.Untyped;
using Tochka.JsonRpc.V1.Common.Serializers;

namespace Tochka.JsonRpc.V1.Server.Models.Binding
{
    [ExcludeFromCodeCoverage]
    public class JsonRpcBindingContext
    {
        public IUntypedCall Call { get; set; }
        public ParameterMetadata ParameterMetadata { get; set; }
        public IJsonRpcSerializer Serializer { get; set; }
    }
}
