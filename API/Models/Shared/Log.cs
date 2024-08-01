using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API.Models.Shared;

public class Log
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; }
    
    public DateTime Timestamp { get; set; }
    
    public string Level { get; set; } = LogLevel.Info.ToString();
    
    public string Message { get; set; } = string.Empty;
    
    public string Exception { get; set; } = string.Empty;
}

public enum LogLevel
{
    Debug,
    Info,
    Warning,
    Error,
    Critical
}