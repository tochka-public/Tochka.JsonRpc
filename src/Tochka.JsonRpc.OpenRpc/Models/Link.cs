using System.Collections.Generic;
using Microsoft.OpenApi.Models;

namespace Tochka.JsonRpc.OpenRpc.Models
{
    public class Link
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string Summary { get; set; }
        public string Method { get; set; }
        public Dictionary<string, object> Params { get; set; }
        public Server Server { get; set; }
    }
}