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
}