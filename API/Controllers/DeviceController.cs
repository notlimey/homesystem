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
            MagicPacketPort = dto.MagicPacketPort  ?? 9,
            MacAddress = dto.MacAddress,
        });
        return createdDevice;
    }

    
    [HttpPost("{id}/wake-on-lan")]
    public async Task<bool> WakeOnLan(Guid id)
    {
        var success = await _deviceService.WakeOnLanAsync(id);
        return success;
    }
}