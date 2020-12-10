using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace WebApplication1.Controllers
{
    /// <summary>
    /// TestData2.Summary~
    /// </summary>
    /// <remarks>TestData2.Remarks~</remarks>
    /// <example>TestData2.Example~</example>
    public class TestData2
    {
        /// <summary>
        /// TestData2.Value.Summary~
        /// </summary>
        /// <remarks>TestData2.Value.Remarks~</remarks>
        /// <example>90909090</example>
        public int Value { get; set; }
        public int Value2 { get; set; }
        public string AnotherValue { get; set; }
        public NestedData2 Nested { get; set; }
    }
}