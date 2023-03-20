using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace Tochka.JsonRpc.TestUtils.Integration;

public abstract class IntegrationTestsBase<TEntryPoint>
    where TEntryPoint : class
{
    protected WebApplicationFactory<TEntryPoint> ApplicationFactory { get; private set; }
    protected HttpClient ApiClient { get; private set; }

    [OneTimeSetUp]
    public virtual Task OneTimeSetup()
    {
        ApplicationFactory = new WebApplicationFactory<TEntryPoint>()
            .WithWebHostBuilder(builder => builder.ConfigureTestServices(SetupServices));
        ApiClient = ApplicationFactory.CreateClient();
        return Task.CompletedTask;
    }

    [SetUp]
    public virtual void Setup() => ApiClient.DefaultRequestHeaders.Clear();

    [TearDown]
    public virtual Task TearDown() => Task.CompletedTask;

    protected virtual void SetupServices(IServiceCollection services)
    {
    }
}
