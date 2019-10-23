using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Polly.Samples.Api.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class SummaryController : ControllerBase
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public SummaryController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        [HttpGet]
        public async Task<IActionResult> GetPostion()
        {
            var client = _httpClientFactory.CreateClient();

            var uriBase = "http://localhost:6000/";
            var path = "position";
            var endpoint = $"{uriBase}{path}";
            var uri = new Uri(endpoint);

            var response = await client.GetAsync(uri);

            if (response.IsSuccessStatusCode)
            {
                return Ok(response.Content.ReadAsStringAsync());
            }

            return StatusCode((int)response.StatusCode);
        }
    }
}