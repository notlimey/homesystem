using API.Services.Job;

namespace API.Classes.Job;

public abstract class JobBase : IJob
{
    protected CancellationTokenSource _cts;
    protected Task _executingTask;

    public abstract string Name { get; }
    public abstract TimeSpan Interval { get; }
    public bool IsRunning => _executingTask != null && !_executingTask.IsCompleted;
    public DateTime LastRun { get; set; }
    public DateTime NextRun { get; set; }

    public abstract Task ExecuteAsync(CancellationToken cancellationToken);

    public virtual async Task StartAsync()
    {
        if (IsRunning)
            return;

        _cts = new CancellationTokenSource();
        _executingTask = ExecuteAsync(_cts.Token);

        await Task.CompletedTask;
    }

    public virtual async Task StopAsync()
    {
        if (_executingTask == null)
            return;

        _cts.Cancel();
        await Task.WhenAny(_executingTask, Task.Delay(Timeout.Infinite));
        _cts.Dispose();
        _executingTask = null;
    }
}