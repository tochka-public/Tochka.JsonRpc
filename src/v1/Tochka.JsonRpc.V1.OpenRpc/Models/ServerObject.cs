using System;
using System.Collections.Generic;

namespace Tochka.JsonRpc.V1.OpenRpc.Models
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