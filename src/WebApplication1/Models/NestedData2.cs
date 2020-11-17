using System.Runtime.Serialization;

namespace WebApplication1.Controllers
{
    [DataContract(Name = "overriden-nested-name")]
    public class NestedData2 : NestedData
    {
    }
}