using System.Diagnostics.CodeAnalysis;
using System.Text;
using BenchmarkDotNet.Attributes;
using Tochka.JsonRpc.Benchmarks.Data;
using Tochka.JsonRpc.Benchmarks.EdjCaseApp;
using Tochka.JsonRpc.Benchmarks.NewWebApp;
using Tochka.JsonRpc.Benchmarks.OldWebApp;
using Tochka.JsonRpc.TestUtils;

namespace Tochka.JsonRpc.Benchmarks.Benchmarks;

[MemoryDiagnoser]
public class GetRequestBenchmark
{
    [ParamsSource(nameof(RequestValues))]
    public string Request { get; set; }

    private HttpClient newClient;
    private HttpClient oldClient;
    private HttpClient edjCaseClient;

    [GlobalSetup]
    public void Setup()
    {
        var newFactory = new NewApplicationFactory().WithWebHostBuilder(static _ => { });
        var oldFactory = new OldApplicationFactory().WithWebHostBuilder(static _ => { });
        var edjCaseFactory = new EdjCaseApplicationFactory().WithWebHostBuilder(static _ => { });
        newClient = newFactory.CreateClient();
        oldClient = oldFactory.CreateClient();
        edjCaseClient = edjCaseFactory.CreateClient();
    }

    [Benchmark(Baseline = true)]
    public async Task<HttpResponseMessage?> New()
    {
        using var request = new StringContent(Request, Encoding.UTF8, "application/json");
        return await newClient.PostAsync("api/jsonrpc", request);
    }

    [Benchmark]
    public async Task<HttpResponseMessage?> Old()
    {
        using var request = new StringContent(Request, Encoding.UTF8, "application/json");
        return await oldClient.PostAsync("api/jsonrpc", request);
    }

    [Benchmark]
    public async Task<HttpResponseMessage?> EdjCase()
    {
        using var request = new StringContent(Request, Encoding.UTF8, "application/json");
        return await edjCaseClient.PostAsync("api/jsonrpc", request);
    }

    private const string Method = "process";

    [SuppressMessage("ReSharper", "StaticMemberInitializerReferesToMemberBelow")]
    public static IEnumerable<string> RequestValues { get; } = new[]
    {
        Requests.GetRequest(Id, Method, TestData.Plain),
        Requests.GetRequest(Id, Method, TestData.Nested),
        Requests.GetRequest(Id, Method, TestData.Big)
    };

    private static readonly Guid Id = Guid.NewGuid();
}
