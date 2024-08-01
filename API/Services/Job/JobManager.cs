namespace API.Services.Job;

public class JobManager : BackgroundService
{
    
    private readonly IEnumerable<IJob> _jobs;
    private readonly ILogger<JobManager> _logger;

    public JobManager(IEnumerable<IJob> jobs, ILogger<JobManager> logger)
    {
        _jobs = jobs;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        foreach (var job in _jobs)
        {
            await job.StartAsync();
        }

        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
        }

        foreach (var job in _jobs)
        {
            await job.StopAsync();
        }
    }

    public async Task StartJobAsync(string jobName)
    {
        var job = _jobs.FirstOrDefault(j => j.Name == jobName);
        if (job != null)
        {
            await job.StartAsync();
            _logger.LogInformation($"Started job: {jobName}");
        }
    }

    public async Task StopJobAsync(string jobName)
    {
        var job = _jobs.FirstOrDefault(j => j.Name == jobName);
        if (job != null)
        {
            await job.StopAsync();
            _logger.LogInformation($"Stopped job: {jobName}");
        }
    }

    public IEnumerable<JobStatus> GetJobStatuses()
    {
        return _jobs.Select(j => new JobStatus { Name = j.Name, IsRunning = j.IsRunning });
    }

}
public class JobStatus
{
    public string Name { get; set; }
    public bool IsRunning { get; set; }
}