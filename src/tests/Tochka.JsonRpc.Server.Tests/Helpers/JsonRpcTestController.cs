using System.Collections.Generic;
using Tochka.JsonRpc.Server.Pipeline;

namespace Tochka.JsonRpc.Server.Tests.Helpers
{
    public class JsonRpcTestController : JsonRpcController
    {
        public void VoidAction(int a, long? b = null, object obj = null, List<int> list=null)
        {
        }
    }
}