using System.Buffers;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Tochka.JsonRpc.Server.Binding;

public class JsonRpcFormatter : NewtonsoftJsonOutputFormatter
{
    private readonly HeaderJsonRpcSerializer headerJsonRpcSerializer;

    public JsonRpcFormatter(HeaderJsonRpcSerializer headerJsonRpcSerializer, ArrayPool<char> charPool, IOptions<MvcOptions> mvcOptions, IOptions<MvcNewtonsoftJsonOptions> mvcNewtonsoftJsonOptions) : base(headerJsonRpcSerializer.Settings, charPool, mvcOptions.Value, mvcNewtonsoftJsonOptions.Value)
    {
        this.headerJsonRpcSerializer = headerJsonRpcSerializer;
    }

    protected override JsonSerializer CreateJsonSerializer()
    {
        return headerJsonRpcSerializer.Serializer;
    }
}
