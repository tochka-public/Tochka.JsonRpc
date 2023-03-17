using System.Collections.Generic;
using Microsoft.OpenApi.Models;

namespace Tochka.JsonRpc.V1.OpenRpc.Models
{
    public class Method
    {
        /// <summary>
        /// The cannonical name for the method. The name MUST be unique within the methods array.
        /// </summary>
        public string Name { get; set; }

        public List<Tag> Tags { get; set; }

        /// <summary>
        /// A short summary of what the method does.
        /// </summary>
        public string Summary { get; set; }

        /// <summary>
        /// A verbose explanation of the method behavior. GitHub Flavored Markdown syntax MAY be used
        /// for rich text representation.
        /// </summary>
        public string Description { get; set; }

        public OpenApiExternalDocs ExternalDocs { get; set; }

        public List<ContentDescriptor> Params { get; set; }

        public ContentDescriptor Result { get; set; }
        
        public bool? Deprecated { get; set; }

        public List<Server> Servers { get; set; }

        /// <summary>
        /// Defines an application level error.
        /// </summary>
        public List<Error> Errors { get; set; }
        
        public List<Link> Links { get; set; }

        /// <summary>
        /// Format the server expects the params. Defaults to 'either'.
        /// </summary>
        public MethodObjectParamStructure ParamStructure { get; set; }
        
        public List<ExamplePairing> Examples { get; set; }
    }
}