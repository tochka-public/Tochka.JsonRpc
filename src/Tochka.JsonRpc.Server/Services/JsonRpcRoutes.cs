using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Logging;

namespace Tochka.JsonRpc.Server.Services
{
    public class JsonRpcRoutes : IJsonRpcRoutes
    {
        private readonly HashSet<string> routes;

        private readonly ILogger<JsonRpcRoutes> log;

        public JsonRpcRoutes(ILogger<JsonRpcRoutes> log)
        {
            routes = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase);
            this.log = log;
        }

        public void Register(string route)
        {
            routes.Add(route.TrimEnd('/'));
            
            log.LogTrace("Registered route [{route}]", route);
        }

        // TODO use TemplateMatcher
        public bool IsJsonRpcRoute(string route) => routes.Contains(route.TrimEnd('/'));
    }
}