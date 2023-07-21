using System.Collections.Generic;

namespace Tochka.JsonRpc.V1.OpenRpc.Models
{
    public class ServerVariable
    {
        public List<string> Enum { get; set; }
        public string Default { get; set; }
        public string Description { get; set; }
    }
}