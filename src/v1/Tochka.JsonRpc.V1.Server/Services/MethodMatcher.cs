using System;
using Microsoft.Extensions.Logging;
using Tochka.JsonRpc.V1.Common;
using Tochka.JsonRpc.V1.Server.Models;
using Tochka.JsonRpc.V1.Server.Settings;

namespace Tochka.JsonRpc.V1.Server.Services
{
    public class MethodMatcher : IMethodMatcher
    {
        private readonly ILogger log;

        public MethodMatcher(ILogger<MethodMatcher> log)
        {
            this.log = log;
        }

        public bool IsMatch(MethodMetadata methodMetadata, string method)
        {
            var actionName = GetActionName(methodMetadata);
            var result = method.Equals(actionName, StringComparison.OrdinalIgnoreCase);

            log.LogTrace("Matching [{method}] to action [{actionName}]: {result}",
                         method,
                         actionName,
                         result);
            
            return result;
        }

        public string GetActionName(MethodMetadata methodMetadata)
        {
            return methodMetadata.MethodOptions.MethodStyle switch
            {
                MethodStyle.ControllerAndAction => $"{methodMetadata.Controller.Json}{JsonRpcConstants.ControllerMethodSeparator}{methodMetadata.Action.Json}",
                MethodStyle.ActionOnly => methodMetadata.Action.Json,
                _ => throw new ArgumentOutOfRangeException(nameof(methodMetadata.MethodOptions.MethodStyle), methodMetadata.MethodOptions.MethodStyle, null)
            };
        }
    }
}
