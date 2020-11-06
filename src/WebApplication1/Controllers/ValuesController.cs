using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace WebApplication1.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class Values2Controller : ControllerBase
    {
        [HttpPost]
        public TestData2 Post([FromBody] TestData2 value)
        {
            return value;
        }
    }

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
