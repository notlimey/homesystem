namespace API.Classes.Job.Jobs;

public class WeatherUpdateJob : JobBase
{
    
    public override string Name => "Weather Update Job";
    public override TimeSpan Interval => TimeSpan.FromMinutes(5);

    public override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            Console.WriteLine("Weather Update Job is running...");
            // Do something here
            await Task.Delay(Interval, cancellationToken);
        }
    }
}