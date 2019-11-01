using Microsoft.AspNetCore.Mvc;
using Polly.CircuitBreaker;
using Polly.Fallback;
using Polly.Retry;
using System;
using System.Net.Http;
using System.Threading.Tasks;

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
        private static readonly int _retryCount = 3;

        private readonly HttpClient _httpClient;

        private readonly AsyncRetryPolicy<HttpResponseMessage> _httpRetryPolicy;

        private static readonly AsyncFallbackPolicy<HttpResponseMessage> _fallBackPolicy =
            Policy<HttpResponseMessage>
                .Handle<BrokenCircuitException>()
                .FallbackAsync(
                    new HttpResponseMessage() { StatusCode = System.Net.HttpStatusCode.BadRequest },
                    (httpResponse) =>
                    {
                        Console.WriteLine("Executando fallback");
                        return Task.CompletedTask;
                    });

        private static readonly AsyncCircuitBreakerPolicy<HttpResponseMessage> _breakerPolicy =
            Policy
                .HandleResult<HttpResponseMessage>(it => !it.IsSuccessStatusCode)
                .CircuitBreakerAsync(
                    _retryCount,
                    TimeSpan.FromSeconds(10),
                    (response, circuitState, timeSpan, context) => Console.WriteLine($"Circuito Aberto -> HttpResponse:{response.Result.StatusCode}, CircuitState: {circuitState}, TimeSpan:{timeSpan}"),
                    (context) => Console.WriteLine($"Circuito Fechado"),
                    () => Console.WriteLine("Circuito Half Open"));

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

            var waitRetryPolicy = Policy
                .HandleResult<HttpResponseMessage>(it => !it.IsSuccessStatusCode)
                .WaitAndRetryAsync(
                    _retryCount,
                    (retry) => TimeSpan.FromSeconds(Math.Pow(retry, 2)),
                    (httpResponse, timeSpan, count, context) => Console.WriteLine($"Realizando retentativa -> {Response}, {timeSpan}, {count}, {context}"));

            var response = await _fallBackPolicy
                .WrapAsync(waitRetryPolicy)
                .WrapAsync(_breakerPolicy)
                .ExecuteAsync(() => _httpClient.GetAsync(custodyEndpoint));

            return StatusCode((int)response.StatusCode, new { response.StatusCode });
        }
    }
}