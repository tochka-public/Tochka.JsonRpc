using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Moq;
using NUnit.Framework;
using Tochka.JsonRpc.Client.Models;
using Tochka.JsonRpc.Common;
using Tochka.JsonRpc.Common.Models.Response.Untyped;

namespace Tochka.JsonRpc.Client.Tests.Models;

internal class SingleJsonRpcResultTests
{
    [Test]
    public void METHOD()
    {
        var response = new UntypedErrorResponse { Error = new() { Data = JsonDocument.Parse("{ \"abc_def\": \"b\" }") } };
        var contextMock = new Mock<IJsonRpcCallContext>();
        contextMock.Setup(static c => c.SingleResponse)
            .Returns(response);
        var result = new SingleJsonRpcResult(contextMock.Object, JsonRpcSerializerOptions.Headers, JsonRpcSerializerOptions.CamelCase);
        JsonRpcSerializerOptions.Headers.Converters.Add(new IaConverter());
        var value = result.AsError<IA>();
        Console.WriteLine(value.Data.GetType());
        Console.WriteLine(value.Data);
    }

    private interface IA
    {
    }

    private class IaConverter : JsonConverter<IA>
    {
        public override IA? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            reader.Skip();
            return new A("123");
        }

        public override void Write(Utf8JsonWriter writer, IA value, JsonSerializerOptions options) => throw new NotImplementedException();
    }

    private record A(string AbcDef) : IA;
}
