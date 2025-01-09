using API.DTOs.Devices;
using API.Interfaces.Devices;
using API.Models.Devices;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DeviceController
{
    private readonly IDeviceService _deviceService;

    public DeviceController(IDeviceService deviceService)
    {
        _deviceService = deviceService;
    }

    [HttpGet]
    public async Task<List<Device>> GetAllDevices()
    {
        var devices = await _deviceService.GetDevicesAsync();
        
        return devices;
    }

    
    [HttpPost]
    public async Task<Device?> CreateDevice(CreateDeviceDTO dto)
    {
        var createdDevice = await _deviceService.CreateDeviceAsync(new Device()
        {
            IP = dto.IP,
            Name = dto.Name,
            SupportMagicPacket = dto.SupportMagicPacket,
            MagicPacketPort = dto.MagicPacketPort ?? 9,
            MacAddress = dto.MacAddress,
            MainTask = dto.MainTask
        });
        return createdDevice;
    }

    
    [HttpPost("{id}/wake-on-lan")]
    public async Task<bool> WakeOnLan(Guid id)
    {
        var success = await _deviceService.WakeOnLanAsync(id);
        
        return success;
    }
    
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateDevice(Guid id, CreateDeviceDTO dto)
    {
        var device = await _deviceService.GetDeviceByIdAsync(id);
        if (device == null)
            return new NotFoundResult();
        
        device.IP = dto.IP;
        device.Name = dto.Name;
        device.SupportMagicPacket = dto.SupportMagicPacket;
        device.MagicPacketPort = dto.MagicPacketPort ?? 9;
        device.MacAddress = dto.MacAddress;
        device.MainTask = dto.MainTask;

        var updatedDevice = await _deviceService.UpdateDeviceAsync(id, device);
        if (updatedDevice == null)
            return new NotFoundResult();
        return new OkResult();
    }
    
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteDevice(Guid id)
    {
        var success = await _deviceService.DeleteDeviceAsync(id);
        if (!success)
            return new NotFoundResult();
        return new OkResult();
    }
}