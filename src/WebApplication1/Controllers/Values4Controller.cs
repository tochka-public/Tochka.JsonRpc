using Microsoft.AspNetCore.Mvc;

namespace WebApplication1.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class Values4Controller : ControllerBase
    {
        [HttpPost]
        public AnotherData Post([FromBody] AnotherData value)
        {
            return value;
        }
    }
}