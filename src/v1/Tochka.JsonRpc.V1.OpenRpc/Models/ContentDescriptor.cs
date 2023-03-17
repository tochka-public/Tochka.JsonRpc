using NJsonSchema;

namespace Tochka.JsonRpc.V1.OpenRpc.Models
{
    public class ContentDescriptor
    {
        public string Name { get; set; }
        
        public string Summary { get; set; }

        public string Description { get; set; }
        
        public bool Required { get; set; }
        
        public JsonSchema Schema { get; set; }
        
        public bool Deprecated { get; set; }
    }
}