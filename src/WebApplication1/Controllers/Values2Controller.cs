using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace WebApplication1.Controllers
{
    /// <summary>
    /// Values2.Summary~
    /// </summary>
    /// <returns>Values2.Returns~</returns>
    /// <remarks>Values2.Remarks~</remarks>
    /// <example>Values2.Example~</example>
    /// <response code="222">Values2.Response222~</response>
    /// <response code="200">Values2.Response200~</response>
    [Route("api/[controller]")]
    [ApiController]
    public class Values2Controller : ControllerBase
    {
        /// <summary>
        /// Values2.Post.Summary~
        /// </summary>
        /// <param name="value">Values2.Post.Param1~</param>
        /// <returns>Values2.Post.Returns~</returns>
        /// /// <remarks>Values2.Post.Remarks~</remarks>
        /// <example>Values2.Post.Example~</example>
        /// <response code="111">Values2.Post.Response2</response>
        [HttpPost]
        public TestData2 Post([FromBody] TestData2 value)
        {
            return value;
        }
    }
}
