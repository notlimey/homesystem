namespace API.Services.Job;

public interface IJob
{
    string Name { get; }
    TimeSpan Interval { get; }
    bool IsRunning { get; }
    Task ExecuteAsync(CancellationToken cancellationToken);
    Task StartAsync();
    Task StopAsync();
}