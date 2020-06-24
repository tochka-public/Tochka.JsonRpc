using System;
using Microsoft.Extensions.Logging;
using Tochka.JsonRpc.Common;
using Tochka.JsonRpc.Server.Models;
using Tochka.JsonRpc.Server.Settings;

namespace Tochka.JsonRpc.Server.Services
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
            log.LogTrace($"Matching [{method}] to action [{actionName}]: {result}");
            return result;
        }

        public string GetActionName(MethodMetadata methodMetadata)
        {
            switch (methodMetadata.MethodOptions.MethodStyle)
            {
                case MethodStyle.ControllerAndAction:
                    return $"{methodMetadata.Controller.Json}{JsonRpcConstants.ControllerMethodSeparator}{methodMetadata.Action.Json}";
                case MethodStyle.ActionOnly:
                    return methodMetadata.Action.Json;
                default:
                    throw new ArgumentOutOfRangeException(nameof(methodMetadata.MethodOptions.MethodStyle), methodMetadata.MethodOptions.MethodStyle, null);
            }
        }
    }
}