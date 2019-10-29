using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Polly.CircuitBreaker;
using Polly.Fallback;
using Polly.Retry;
using Polly.Wrap;

namespace Polly.Samples.Api.Controllers
{
    /// <summary>
    /// Objetivo de simular os seguintes cenários
    /// Retry
    // Retry with wait
    // Circuit Breaker
    // Fallback
    /// </summary>
    [Route("[controller]")]
    [ApiController]
    public class SummaryController : ControllerBase
    {
        private readonly HttpClient _httpClient;

        private readonly AsyncRetryPolicy<HttpResponseMessage> _httpRetryPolicy;
                
        private static readonly AsyncCircuitBreakerPolicy<HttpResponseMessage> _breakerPolicy = 
            Policy
                .HandleResult<HttpResponseMessage>(it => !it.IsSuccessStatusCode)
                .CircuitBreakerAsync(1, TimeSpan.FromSeconds(10));

        private static readonly AsyncFallbackPolicy<HttpResponseMessage> _fallBackPolicy = 
            Policy<HttpResponseMessage>
                .Handle<BrokenCircuitException>()                
                .FallbackAsync(new HttpResponseMessage() { StatusCode = System.Net.HttpStatusCode.BadRequest });

        private const string _baseUri = "http://localhost:5000";

        public SummaryController(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient();

            _httpRetryPolicy = Policy
                .HandleResult<HttpResponseMessage>(it => !it.IsSuccessStatusCode)
                .RetryAsync(3);
        }

        [HttpGet("position")]
        public async Task<IActionResult> GetPostion()
        {
            var positionPath = $"{_baseUri}/position/position";
            var postionEndpoint = new Uri(positionPath);

            var response = await _httpRetryPolicy.ExecuteAsync(() => _httpClient.GetAsync(postionEndpoint));

            if (response.IsSuccessStatusCode)
            {
                return Ok(response.Content.ReadAsStringAsync());
            }

            return StatusCode((int)response.StatusCode);
        }

        [HttpGet("custody")]
        public async Task<IActionResult> GetCustody()
        {
            var custodyPath = $"{_baseUri}/position/custody";
            var custodyEndpoint = new Uri(custodyPath);

            //var response = await _fallBackPolicy
            //    .WrapAsync(_breakerPolicy)
            //    .ExecuteAsync(() => _httpClient.GetAsync(custodyEndpoint));

            var response = await _breakerPolicy
                .ExecuteAsync(() => _httpClient.GetAsync(custodyEndpoint));

            return StatusCode((int)response.StatusCode, new { });
        }
    }
}