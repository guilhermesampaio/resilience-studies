using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace Polly.Samples.Position.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class PositionController : ControllerBase
    {

        private static int _positionRequests = 0;
        private static int _custodyRequests = 0;

        [HttpGet("position")]
        public IActionResult GetPostion()
        {
            _positionRequests++;

            if (_positionRequests >= 2 && _positionRequests <= 4)
                return StatusCode((int)HttpStatusCode.ServiceUnavailable, "Service Unvailable");

            return Ok(35690.89M);
        }

        [HttpGet("custody")]
        public IActionResult GetCustody() 
        {
            _custodyRequests++;

            if(_custodyRequests % 5 == 0)
                return StatusCode((int)HttpStatusCode.GatewayTimeout);

            return Ok();


        }
    }
}