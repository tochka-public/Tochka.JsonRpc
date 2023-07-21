using System.Text;
using BenchmarkDotNet.Attributes;
using Tochka.JsonRpc.Benchmarks.Data;
using Tochka.JsonRpc.Benchmarks.EdjCaseApp;
using Tochka.JsonRpc.Benchmarks.NewWebApp;
using Tochka.JsonRpc.Benchmarks.OldWebApp;
using Tochka.JsonRpc.TestUtils;

namespace Tochka.JsonRpc.Benchmarks.Benchmarks;

[MemoryDiagnoser]
public class GetNotificationBenchmark
{
    [ParamsSource(nameof(NotificationValues))]
    public string Notification { get; set; }

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
        using var request = new StringContent(Notification, Encoding.UTF8, "application/json");
        return await newClient.PostAsync("api/jsonrpc", request);
    }

    [Benchmark]
    public async Task<HttpResponseMessage?> Old()
    {
        using var request = new StringContent(Notification, Encoding.UTF8, "application/json");
        return await oldClient.PostAsync("api/jsonrpc", request);
    }

    [Benchmark]
    public async Task<HttpResponseMessage?> EdjCase()
    {
        using var request = new StringContent(Notification, Encoding.UTF8, "application/json");
        return await edjCaseClient.PostAsync("api/jsonrpc", request);
    }

    private const string Method = "process";

    public static IEnumerable<string> NotificationValues { get; } = new[]
    {
        Requests.GetNotification(Method, TestData.Plain),
        Requests.GetNotification(Method, TestData.Nested),
        Requests.GetNotification(Method, TestData.Big)
    };
}
