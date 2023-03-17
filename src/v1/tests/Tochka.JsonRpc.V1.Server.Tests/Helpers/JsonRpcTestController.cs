using System.Collections.Generic;
using Tochka.JsonRpc.V1.Server.Pipeline;

namespace Tochka.JsonRpc.V1.Server.Tests.Helpers
{
    public class JsonRpcTestController : JsonRpcController
    {
        public void VoidAction(int a, long? b = null, object obj = null, List<int> list=null)
        {
        }
    }
}