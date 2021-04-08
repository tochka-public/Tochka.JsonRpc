using System;

namespace Tochka.JsonRpc.OpenRpc.Models
{
    /// <summary>
    /// Information about external documentation
    /// </summary>
    public class ExternalDocs
    {
        public string Description { get; set; }
        public Uri Url { get; set; }
    }
}