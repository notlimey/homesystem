using System.Diagnostics;
using System.Net.NetworkInformation;
using API.Interfaces.Devices;

namespace API.Classes.Job.Jobs;

public class DeviceOnlineJob : JobBase
{
    private readonly ILogger<DeviceOnlineJob> _logger;
    private readonly IDeviceService _deviceService;
    
    public override string Name => "Device Online Job";
    public override TimeSpan Interval => TimeSpan.FromMinutes(1);

    public DeviceOnlineJob(IDeviceService deviceService, ILogger<DeviceOnlineJob> logger)
    {
        _deviceService = deviceService;
        _logger = logger;
    }
    public override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            var stopwatch = Stopwatch.StartNew();
        
            try
            {
                _logger.LogInformation("Started checking for online devices...");

                var devices = await _deviceService.GetDevicesAsync();
            
                foreach (var device in devices)
                {
                    var ping = new Ping();
                    var reply = await ping.SendPingAsync(device.IP, 1000);
                    if (reply.Status == IPStatus.Success)
                    {
                        device.IsOnline = true;
                    }
                    else
                    {
                        device.IsOnline = false;
                    }
                    
                    await _deviceService.UpdateDeviceAsync(device.Id, device);
                }
                
                stopwatch.Stop();
                _logger.LogInformation("Device online job excecuted. Execution time: {ExecutionTime}", stopwatch.Elapsed);
                
                this.LastRun = DateTime.UtcNow;
                this.NextRun = DateTime.UtcNow.Add(Interval);
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
}