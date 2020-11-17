using Microsoft.AspNetCore.Mvc;

namespace WebApplication1.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class Values3Controller : ControllerBase
    {
        [HttpPost]
        public TestData3 Post([FromBody] TestData3 value)
        {
            return value;
        }
    }
}