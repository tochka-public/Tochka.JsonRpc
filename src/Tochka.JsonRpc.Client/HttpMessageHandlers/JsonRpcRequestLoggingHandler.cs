using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Logging;

namespace Tochka.JsonRpc.Client.HttpMessageHandlers;

/// <inheritdoc />
/// <summary>
/// Logs outgoing request
/// </summary>
[ExcludeFromCodeCoverage(Justification = "don't want to test logs")]
public class JsonRpcRequestLoggingHandler : DelegatingHandler
{
    private readonly ILogger<JsonRpcRequestLoggingHandler> logger;

    /// <inheritdoc />
    public JsonRpcRequestLoggingHandler(ILogger<JsonRpcRequestLoggingHandler> logger) => this.logger = logger;

    /// <inheritdoc />
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (request.Content is not null)
        {
            var body = await request.Content.ReadAsStringAsync(cancellationToken);
            logger.LogInformation("{jsonRpcRequest}", body);
        }

        return await base.SendAsync(request, cancellationToken);
    }
}
