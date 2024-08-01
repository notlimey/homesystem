using API.Models.Devices;

namespace API.Interfaces.Devices;

public interface IDeviceService
{
    Task<List<Device>> GetDevicesAsync();
    
    Task<Device> GetDeviceByIdAsync(Guid id);
    
    Task<Device> CreateDeviceAsync(Device device);
    
    Task<Device> UpdateDeviceAsync(Guid id, Device device);
    
    Task<bool> DeleteDeviceAsync(Guid id);
    
    Task<bool> WakeOnLanAsync(Guid id);
    
    Task<bool> PingAsync(Guid id);
    
    Task<bool> FindIpByMacAsync(Guid id);
}