﻿using System.Net;
using BenchmarkDotNet.Attributes;
using RichardSzalay.MockHttp;
using Tochka.JsonRpc.Benchmarks.Data;
using Tochka.JsonRpc.TestUtils;

namespace Tochka.JsonRpc.Benchmarks.Benchmarks;

[MemoryDiagnoser]
public class SendNotificationBenchmark
{
    [ParamsSource(nameof(DataValues))]
    public TestData Data { get; set; }

    private MockHttpMessageHandler handlerMock;
    private NewJsonRpcClient newClient;

    [GlobalSetup]
    public void Setup()
    {
        handlerMock = new MockHttpMessageHandler();
        handlerMock.When("*").Respond(HttpStatusCode.OK);

        newClient = new NewJsonRpcClient(handlerMock.ToHttpClient());
    }

    [Benchmark(Baseline = true)]
    public async Task New() => await newClient.SendNotification(Method, Data, CancellationToken.None);

    private const string Method = "method";

    public static IEnumerable<TestData> DataValues { get; } = Requests.AllDataValues;
}
