using System.Text;
using BenchmarkDotNet.Attributes;
using Tochka.JsonRpc.Benchmarks.EdjCaseApp;
using Tochka.JsonRpc.Benchmarks.NewWebApp;
using Tochka.JsonRpc.Benchmarks.OldWebApp;

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

    private const string PlainRequest = """
        {
            "jsonrpc": "2.0",
            "method": "process",
            "id": "123",
            "params": {
                "data": {
                    "bool_field": true,
                    "string_field": "123",
                    "int_field": 123,
                    "double_field": 1.23,
                    "enum_field": "two",
                    "array_field": [
                        1,
                        2,
                        3
                    ],
                    "nullable_field": null
                }
            }
        }
        """;

    public static IEnumerable<string> RequestValues => new[]
    {
        PlainRequest
    };
}
