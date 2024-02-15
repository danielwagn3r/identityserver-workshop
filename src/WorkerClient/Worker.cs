using System.Net;

namespace WorkerClient;

public class Worker : BackgroundService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<Worker> _logger;

    public Worker(ILogger<Worker> logger, IHttpClientFactory factory)
    {
        _logger = logger;
        _httpClient = factory.CreateClient("client");

        _httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);

            using (var result = await _httpClient.GetAsync("/Double/3"))
            {
                try
                {
                    result.EnsureSuccessStatusCode();

                    var content = await result.Content.ReadAsStringAsync();

                    _logger.LogInformation("Result: {Content}", content);
                }
                catch (HttpRequestException e)
                {
                    _logger.LogError(e, "HTTP Status Code {StatusCode} '{StatusCodeText}'", (int)e.StatusCode, e.StatusCode);
                }
            }

            await Task.Delay(2000, stoppingToken);
        }
    }
}