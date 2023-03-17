using System.Collections.Generic;
using Microsoft.OpenApi.Models;

namespace Tochka.JsonRpc.V1.OpenRpc.Models
{
    public class OpenRpc
    {
        public string Openrpc { get; set; }
        public OpenApiInfo Info { get; set; }
        public List<Server> Servers { get; set; }
        public List<Method> Methods { get; set; }
        public Components Components { get; set; }
        public ExternalDocs ExternalDocs { get; set; }
    }
}