using System.ComponentModel.DataAnnotations;

namespace API.DTOs.Devices;

public class CreateDeviceDTO
{
    public string Name { get; set; } = string.Empty;
    
    public string IP { get; set; } = string.Empty;
    
    public bool SupportMagicPacket { get; set; }
    
    public int? MagicPacketPort { get; set; } = 9;
    
    [Required]
    public string MacAddress { get; set; } = string.Empty;
}