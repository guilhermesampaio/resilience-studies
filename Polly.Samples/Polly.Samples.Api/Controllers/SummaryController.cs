using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Polly.Retry;

namespace Polly.Samples.Api.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class SummaryController : ControllerBase
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly AsyncRetryPolicy<HttpResponseMessage> _httpRetryPolicy;

        public SummaryController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
            _httpRetryPolicy = Policy.HandleResult<HttpResponseMessage>(it => !it.IsSuccessStatusCode)
                .RetryAsync(3);
        }

        [HttpGet]
        public async Task<IActionResult> GetPostion()
        {
            // Retry
            // Retry with wait
            // Circuit Breaker
            // Fallback

            var client = _httpClientFactory.CreateClient();

            var uriBase = "http://localhost:5000/";
            var path = "position";
            var endpoint = $"{uriBase}{path}";
            var uri = new Uri(endpoint);


            var response = await _httpRetryPolicy.ExecuteAsync(() => client.GetAsync(uri));
            
            if (response.IsSuccessStatusCode)
            {
                return Ok(response.Content.ReadAsStringAsync());
            }

            return StatusCode((int)response.StatusCode);
        }
    }
}