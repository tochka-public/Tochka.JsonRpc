using System.Reflection;
using System.Text;
using BenchmarkDotNet.Attributes;
using EdjCase.JsonRpc.Client;
using EdjCase.JsonRpc.Common;
using Moq;
using RichardSzalay.MockHttp;
using Tochka.JsonRpc.TestUtils;
using OldId = Tochka.JsonRpc.V1.Common.Models.Id.StringRpcId;
using NewId = Tochka.JsonRpc.Common.Models.Id.StringRpcId;

namespace Tochka.JsonRpc.Benchmarks.Benchmarks;

[MemoryDiagnoser]
public class SendRequestBenchmark
{
    [ParamsSource(nameof(DataValues))]
    public TestData Data
    {
        get => data;

        set
        {
            data = value;
            edjCaseData = value.ToDictionary();
        }
    }

    [ParamsSource(nameof(ResponseValues))]
    public string Response { get; set; }

    private readonly OldId oldId = new(Id.ToString());
    private readonly NewId newId = new(Id.ToString());
    private readonly RpcId edjCaseId = new(Id.ToString());

    private TestData data;
    private Dictionary<string, object?> edjCaseData;

    private MockHttpMessageHandler handlerMock;
    private OldJsonRpcClient oldClient;
    private NewJsonRpcClient newClient;
    private RpcClient edjCaseClient;

    [GlobalSetup]
    public void Setup()
    {
        handlerMock = new MockHttpMessageHandler();
        handlerMock.When($"{Constants.BaseUrl}big")
            .Respond(static _ => new StringContent(Responses.GetBigResponse(Id), Encoding.UTF8, "application/json"));
        handlerMock.When($"{Constants.BaseUrl}nested")
            .Respond(static _ => new StringContent(Responses.GetNestedResponse(Id), Encoding.UTF8, "application/json"));
        handlerMock.When($"{Constants.BaseUrl}plain")
            .Respond(static _ => new StringContent(Responses.GetPlainResponse(Id), Encoding.UTF8, "application/json"));

        oldClient = new OldJsonRpcClient(handlerMock.ToHttpClient());
        newClient = new NewJsonRpcClient(handlerMock.ToHttpClient());
        var httpClientFactoryMock = new Mock<IHttpClientFactory>();
        httpClientFactoryMock.Setup(static f => f.CreateClient(It.IsAny<string>()))
            .Returns(handlerMock.ToHttpClient());
        edjCaseClient = RpcClient.Builder(new Uri(Constants.BaseUrl)).Build();
        var edjCaseTransportClient = edjCaseClient.GetType().GetProperty("transportClient", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(edjCaseClient);
        edjCaseTransportClient.GetType().GetProperty("httpClientFactory", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(edjCaseTransportClient, httpClientFactoryMock.Object);
    }

    [Benchmark(Baseline = true)]
    public async Task<TestData?> New()
    {
        var response = await newClient.SendRequest(Response, newId, Method, Data, CancellationToken.None);
        return response.GetResponseOrThrow<TestData>();
    }

    [Benchmark]
    public async Task<TestData> Old()
    {
        var response = await oldClient.SendRequest(Response, oldId, Method, Data, CancellationToken.None);
        return response.GetResponseOrThrow<TestData>();
    }

    [Benchmark]
    public async Task<TestData?> EdjCase()
    {
        var request = RpcRequest.WithParameterMap(Method, edjCaseData, edjCaseId);
        var response = await edjCaseClient.SendAsync<TestData>(request, Response);
        return response.Result;
    }

    private const string Method = "method";

    public static IEnumerable<TestData> DataValues => new[]
    {
        TestData.Big,
        TestData.Nested,
        TestData.Plain
    };

    public static IEnumerable<string> ResponseValues => new[]
    {
        "big",
        "nested",
        "plain"
    };

    private static readonly Guid Id = Guid.NewGuid();
}
