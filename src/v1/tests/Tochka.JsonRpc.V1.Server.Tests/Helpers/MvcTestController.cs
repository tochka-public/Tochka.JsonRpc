using Microsoft.AspNetCore.Mvc;

namespace Tochka.JsonRpc.V1.Server.Tests.Helpers
{
    public class MvcTestController : Controller
    {
        public void VoidAction(int a)
        {
        }
    }
}