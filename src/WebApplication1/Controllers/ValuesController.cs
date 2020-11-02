using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Tochka.JsonRpc.Server.Attributes;
using Tochka.JsonRpc.Server.Pipeline;
using Tochka.JsonRpc.Server.Settings;

namespace WebApplication1.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ValuesController : ControllerBase
    {
        // GET api/values/5
        [HttpGet("{id}")]
        private ActionResult<string> Get(int id)
        {
            return "value";
        }

        // POST api/values
        [HttpPost]
        public TestData2 Post([FromBody] TestData2 value)
        {
            return value;
        }
    }

    public class FooController : JsonRpcController
    {
        private readonly IActionDescriptorProvider actionDescriptorProvider;
        private readonly IApiDescriptionProvider apiDescriptionProvider;
        private readonly IActionDescriptorCollectionProvider actionDescriptorCollectionProvider;
        private readonly IApiDescriptionGroupCollectionProvider apiDescriptionGroupCollectionProvider;

        public FooController(IActionDescriptorProvider actionDescriptorProvider, IApiDescriptionProvider apiDescriptionProvider, IActionDescriptorCollectionProvider actionDescriptorCollectionProvider, IApiDescriptionGroupCollectionProvider apiDescriptionGroupCollectionProvider)
        {
            this.actionDescriptorProvider = actionDescriptorProvider;
            this.apiDescriptionProvider = apiDescriptionProvider;
            this.actionDescriptorCollectionProvider = actionDescriptorCollectionProvider;
            this.apiDescriptionGroupCollectionProvider = apiDescriptionGroupCollectionProvider;
        }

        public string Simple(int a, string b)
        {
            return $"{a}, {b}";
        }

        public TestData BindObject([FromParams(BindingStyle.Object)] TestData data)
        {
            return data;
        }

        public string BindMixedObject(int a, [FromParams(BindingStyle.Object)] TestData data)
        {
            return $"{a}, {data.Value}, {data.AnotherValue}";
        }

        public string BindMixedArray(string s, [FromParams(BindingStyle.Array)] List<int> values)
        {
            return $"{s}; {string.Join(", ", values)}";
        }

        private List<int> BindArray([FromParams(BindingStyle.Array)] List<int> values)
        {
            return values;
        }

        public class TestData
        {
            public int Value { get; set; }
            public string AnotherValue { get; set; }
        }
    }

    public class BarController //: JsonRpcController
    {
        public string Simple(int a, string b)
        {
            return $"{a}, {b}";
        }

        public TestData BindObject([FromParams(BindingStyle.Object)] TestData data)
        {
            return data;
        }

        public List<int> BindArray([FromParams(BindingStyle.Array)] List<int> values)
        {
            return values;
        }
    }

    public class TestData
    {
        public int Value { get; set; }
        public string AnotherValue { get; set; }
    }

    public class TestData2
    {
        public int Value { get; set; }
        public string AnotherValue { get; set; }
    }
}
