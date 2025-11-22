namespace BoomBust.Logging;

/// <summary>
/// Configuration options for BetterStack logging
/// </summary>
public class BetterStackOptions
{
    /// <summary>
    /// Configuration section name in appsettings.json
    /// </summary>
    public const string SectionName = "BetterStack";

    /// <summary>
    /// BetterStack source token for authentication
    /// </summary>
    public string SourceToken { get; set; } = string.Empty;

    /// <summary>
    /// BetterStack API endpoint
    /// </summary>
    public string Endpoint { get; set; } = "https://in.logs.betterstack.com";

    /// <summary>
    /// Whether BetterStack logging is enabled (automatically determined by presence of SourceToken)
    /// </summary>
    public bool IsEnabled => !string.IsNullOrWhiteSpace(SourceToken);
}
