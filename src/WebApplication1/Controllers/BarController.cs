using System.Collections.Generic;
using Tochka.JsonRpc.Server.Attributes;
using Tochka.JsonRpc.Server.Settings;

namespace WebApplication1.Controllers
{
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
}