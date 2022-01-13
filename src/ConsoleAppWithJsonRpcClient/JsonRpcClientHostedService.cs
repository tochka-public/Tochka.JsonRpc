using ConsoleAppWithJsonRpcClient.RandomOrg;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ConsoleAppWithJsonRpcClient;

public class JsonRpcClientHostedService : BackgroundService
{
    private readonly IRandomOrgClient randomOrgClient;
    private readonly ILogger<JsonRpcClientHostedService> logger;

    public JsonRpcClientHostedService(IRandomOrgClient randomOrgClient, ILogger<JsonRpcClientHostedService> logger)
    {
        this.randomOrgClient = randomOrgClient;
        this.logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var randomResponse = await randomOrgClient.GetRandomInts(10, 1, 10000, true, stoppingToken);
        if (randomResponse?.Random?.Data == null)
        {
            logger.LogInformation("null");
            return;
        }

        logger.LogInformation($@"Completion time = {randomResponse.Random.CompletionTime}
Random ints = {string.Join(", ", randomResponse.Random.Data)}");
    }
}