using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace WebApplication1.Controllers
{
    //[DataContract(Name = "overriden-name123")]
    public class TestData2
    {
        //[JsonProperty("testdata2_value_JsonProperty")]
        public int Value { get; set; }
        public int Value2 { get; set; }
        public string AnotherValue { get; set; }
        public NestedData Nested { get; set; }
    }

    public class TestData3 : TestData2
    {
        public NestedData2 Nested2 { get; set; }
    }

    public class NestedData
    {
        public Guid Id { get; set; }
        public DateTime Date { get; set; }
    }

    [DataContract(Name = "overriden-nested-name")]
    public class NestedData2 : NestedData
    {
    }

    public class AnotherData
    {
        public TimeSpan Timespan { get; set; }
        public List<int> Values { get; set; }
    }
}