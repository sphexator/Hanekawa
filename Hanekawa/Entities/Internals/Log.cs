namespace Hanekawa.Entities.Internals;

public class Log
{
    public int Id { get; set; }
    public string TimeStamp { get; set; } = string.Empty;
    public string Level { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string Logger { get; set; } = string.Empty;
    public string CallSite { get; set; } = string.Empty;
    public string Exception { get; set; } = string.Empty;
}