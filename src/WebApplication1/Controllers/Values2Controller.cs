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
}
