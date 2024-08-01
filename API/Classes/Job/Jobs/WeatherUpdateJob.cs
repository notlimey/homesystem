namespace API.Classes.Job.Jobs;
using System.Diagnostics;

public class WeatherUpdateJob : JobBase
{
    private readonly ILogger<WeatherUpdateJob> _logger;
    
    public override string Name => "Weather Update Job";
    public override TimeSpan Interval => TimeSpan.FromMinutes(5);
    
    public WeatherUpdateJob(ILogger<WeatherUpdateJob> logger) {
        _logger = logger;
    }

    public override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            var stopwatch = Stopwatch.StartNew();
        
            try
            {
                _logger.LogInformation("Weather Update Job is starting...");
            
                // Do something here
                await UpdateWeather(cancellationToken);
            
                stopwatch.Stop();
                _logger.LogInformation("Weather Update Job completed. Execution time: {ExecutionTime}", stopwatch.Elapsed);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "Weather Update Job failed. Execution time: {ExecutionTime}", stopwatch.Elapsed);
            }

            // Calculate remaining delay time
            var remainingDelay = Interval - stopwatch.Elapsed;
            if (remainingDelay > TimeSpan.Zero)
            {
                await Task.Delay(remainingDelay, cancellationToken);
            }
        }
    }
    
    private async Task UpdateWeather(CancellationToken cancellationToken)
    {
        // Do something here
        await Task.CompletedTask;
    }
}

