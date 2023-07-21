using System.Collections.Generic;
using NJsonSchema;

namespace Tochka.JsonRpc.V1.OpenRpc.Models
{
    public class Components
    {
        
        public IDictionary<string, ContentDescriptor> ContentDescriptors { get; set; }

        
        public IDictionary<string, object> Errors { get; set; }

        
        public IDictionary<string, object> ExamplePairings { get; set; }

        
        public IDictionary<string, object> Examples { get; set; }

        
        public IDictionary<string, object> Links { get; set; }

        
        public IDictionary<string, JsonSchema> Schemas { get; set; }

        
        public IDictionary<string, object> Tags { get; set; }
    }
}