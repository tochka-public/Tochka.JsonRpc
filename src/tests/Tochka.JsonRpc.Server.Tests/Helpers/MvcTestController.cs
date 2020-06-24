using Microsoft.AspNetCore.Mvc;

namespace Tochka.JsonRpc.Server.Tests.Helpers
{
    public class MvcTestController : Controller
    {
        public void VoidAction(int a)
        {
        }
    }
}