namespace API.Services.Job;

public interface IJob
{
    string Name { get; }
    
    TimeSpan Interval { get; }
    
    bool IsRunning { get; }
    DateTime LastRun { get; }
    
    DateTime NextRun { get; }
    
    Task ExecuteAsync(CancellationToken cancellationToken);
    
    Task StartAsync();
    
    Task StopAsync();
}