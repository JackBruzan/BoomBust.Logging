namespace BoomBust.Logging;

/// <summary>
/// Configuration options for BoomBust logging
/// </summary>
public class BoomBustLoggingOptions
{
    /// <summary>
    /// Application name to include in logs (defaults to assembly name)
    /// </summary>
    public string? ApplicationName { get; set; }

    /// <summary>
    /// Path for log files (default: "logs/log.txt")
    /// </summary>
    public string LogFilePath { get; set; } = "logs/log.txt";

    /// <summary>
    /// Rolling interval for log files (default: Day)
    /// </summary>
    public RollingInterval RollingInterval { get; set; } = RollingInterval.Day;

    /// <summary>
    /// Minimum log level (default: Information)
    /// </summary>
    public string MinimumLevel { get; set; } = "Information";

    /// <summary>
    /// Whether to enable console logging (default: true)
    /// </summary>
    public bool EnableConsole { get; set; } = true;

    /// <summary>
    /// Whether to enable file logging (default: true)
    /// </summary>
    public bool EnableFile { get; set; } = true;

    /// <summary>
    /// Namespaces to override to Warning level (default: Microsoft, System)
    /// </summary>
    public string[] OverrideToWarning { get; set; } = { "Microsoft", "System" };
}

/// <summary>
/// Rolling interval for log files
/// </summary>
public enum RollingInterval
{
    Infinite,
    Year,
    Month,
    Day,
    Hour,
    Minute
}
