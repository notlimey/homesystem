using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API.Models.Devices;

public class Device
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; }
    
    public string Name { get; set; } = string.Empty;
    
    public string IP { get; set; } = string.Empty;
    
    public bool SupportMagicPacket { get; set; }
    
    public int MagicPacketPort { get; set; } = 9;
    
    public bool SupportsBluetooth { get; set; }
    
    public string MainTask  { get; set; } = string.Empty;
    
    [Required]
    public string MacAddress { get; set; } = string.Empty;
    
    public bool IsOnline { get; set; }
    
    public DateTime LastOnline { get; set; }
    
    public DateTime CreatedAt { get; set; }
    
    public DateTime UpdatedAt { get; set; }
}