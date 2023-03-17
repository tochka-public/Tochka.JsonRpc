using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.OpenApi.Models;

namespace Tochka.JsonRpc.V1.OpenRpc
{
    public class OpenRpcOptions
    {
        public Dictionary<string, OpenApiInfo> Docs { get; } = new Dictionary<string, OpenApiInfo>();
        public bool IgnoreObsoleteActions { get; set; }
        public Func<string, ApiDescription, bool> DocInclusionPredicate { get; set; } = DefaultDocInclusionPredicate;
        public string DocumentPath { get; set; } = OpenRpcConstants.DefaultDocumentPath;

        private static bool DefaultDocInclusionPredicate(string documentName, ApiDescription apiDescription)
        {
            return true;
        }
    }
}
