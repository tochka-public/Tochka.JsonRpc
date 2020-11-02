using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace WebApplication1.Controllers
{
    public class TestData2
    {
        [JsonProperty("testdata2_value_JsonProperty")]
        public int Value { get; set; }
        public string AnotherValue { get; set; }
    }
}