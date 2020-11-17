using System.Collections.Generic;
using System.Xml.Schema;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Tochka.JsonRpc.OpenRpc.Models
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