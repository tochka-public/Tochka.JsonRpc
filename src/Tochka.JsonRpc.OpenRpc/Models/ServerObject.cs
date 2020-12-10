using System;
using System.Collections.Generic;
using Microsoft.OpenApi.Models;

namespace Tochka.JsonRpc.OpenRpc.Models
{
    public class Server
    {
        public string Name { get; set; }
        public Uri Url { get; set; }
        public string Summary { get; set; }
        public string Description { get; set; }
        public Dictionary<string, ServerVariable> Variables { get; set; }
    }
}