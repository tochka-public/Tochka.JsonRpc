using System;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Tochka.JsonRpc.Server.IntegrationTests
{
    public class LoggingHandler : DelegatingHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            await LogRequest(request);
            var response = await base.SendAsync(request, cancellationToken);
            await LogResponse(response);
            return response;
        }

        private async Task LogRequest(HttpRequestMessage request)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"Request: {request}");
            if (request.Content != null)
            {
                var requestContent = await request.Content.ReadAsStringAsync();
                sb.AppendLine($"Request content:");
                sb.AppendLine(requestContent);
            }
            Console.WriteLine(sb.ToString());
        }

        private async Task LogResponse(HttpResponseMessage response)
        {
            if (response == null)
            {
                Console.WriteLine("Response is null");
                return;
            }

            var sb = new StringBuilder();
            sb.AppendLine($"Response: {response}");

            if (response.Content != null)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                sb.AppendLine($"Response content:");
                sb.AppendLine(responseContent);
            }
            Console.WriteLine(sb.ToString());
        }
    }
}