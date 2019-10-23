using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace Polly.Samples.Position.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class PositionController : ControllerBase
    {

        private static int _requests = 0;



        [HttpGet]
        public IActionResult GetPostion()
        {
            _requests++;

            if (_requests >= 2 && _requests <= 4)
                return StatusCode((int)HttpStatusCode.ServiceUnavailable, "Service Unvailable");

            return Ok(35690.89M);
        }
    }
}