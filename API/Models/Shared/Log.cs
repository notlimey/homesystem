namespace API.Models.Shared;

public class Log
{
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