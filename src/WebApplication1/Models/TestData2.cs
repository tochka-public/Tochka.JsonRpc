using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace WebApplication1.Controllers
{
    //[DataContract(Name = "overriden-name123")]
    public class TestData2
    {
        //
        public int Value { get; set; }
        public int Value2 { get; set; }
        public string AnotherValue { get; set; }
        public NestedData2 Nested { get; set; }
    }
}