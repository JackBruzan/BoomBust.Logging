using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;

namespace BoomBust.Logging;

/// <summary>
/// Extension methods for adding BoomBust logging with BetterStack integration
/// </summary>
public static class BoomBustLoggingExtensions
{
    /// <summary>
    /// Adds BoomBust logging with BetterStack integration to the host builder
    /// </summary>
    /// <param name="builder">The host builder</param>
    /// <param name="configure">Optional action to configure logging options</param>
    /// <returns>The host builder for chaining</returns>
    public static IHostBuilder UseBoomBustLogging(
        this IHostBuilder builder,
        Action<BoomBustLoggingOptions>? configure = null)
    {
        return builder.UseSerilog((context, services, loggerConfiguration) =>
        {
            // Create and configure options
            var options = new BoomBustLoggingOptions();
            configure?.Invoke(options);

            // Get BetterStack configuration
            var betterStackConfig = context.Configuration.GetSection(BetterStackOptions.SectionName);
            var betterStackOptions = new BetterStackOptions();
            betterStackConfig.Bind(betterStackOptions);

            // Configure Serilog
            ConfigureSerilog(loggerConfiguration, options, betterStackOptions, context.Configuration);
        });
    }

    /// <summary>
    /// Adds BoomBust logging with BetterStack integration to the web application builder
    /// </summary>
    /// <param name="builder">The web application builder</param>
    /// <param name="configure">Optional action to configure logging options</param>
    /// <returns>The web application builder for chaining</returns>
    public static T UseBoomBustLogging<T>(
        this T builder,
        Action<BoomBustLoggingOptions>? configure = null)
        where T : IHostApplicationBuilder
    {
        var options = new BoomBustLoggingOptions();
        configure?.Invoke(options);

        // Get BetterStack configuration
        var betterStackConfig = builder.Configuration.GetSection(BetterStackOptions.SectionName);
        var betterStackOptions = new BetterStackOptions();
        betterStackConfig.Bind(betterStackOptions);

        builder.Services.AddSerilog((services, loggerConfiguration) =>
        {
            ConfigureSerilog(loggerConfiguration, options, betterStackOptions, builder.Configuration);
        });

        return builder;
    }

    private static void ConfigureSerilog(
        LoggerConfiguration loggerConfiguration,
        BoomBustLoggingOptions options,
        BetterStackOptions betterStackOptions,
        IConfiguration configuration)
    {
        // Set minimum level
        var minimumLevel = ParseLogLevel(options.MinimumLevel);
        loggerConfiguration.MinimumLevel.Is(minimumLevel);

        // Add console sink
        if (options.EnableConsole)
        {
            loggerConfiguration.WriteTo.Console();
        }

        // Add file sink
        if (options.EnableFile)
        {
            var rollingInterval = options.RollingInterval switch
            {
                RollingInterval.Infinite => Serilog.RollingInterval.Infinite,
                RollingInterval.Year => Serilog.RollingInterval.Year,
                RollingInterval.Month => Serilog.RollingInterval.Month,
                RollingInterval.Day => Serilog.RollingInterval.Day,
                RollingInterval.Hour => Serilog.RollingInterval.Hour,
                RollingInterval.Minute => Serilog.RollingInterval.Minute,
                _ => Serilog.RollingInterval.Day
            };

            loggerConfiguration.WriteTo.File(
                options.LogFilePath,
                rollingInterval: rollingInterval);
        }

        // Add BetterStack sink if configured
        if (betterStackOptions.IsEnabled)
        {
            loggerConfiguration.WriteTo.BetterStack(
                sourceToken: betterStackOptions.SourceToken,
                betterStackEndpoint: betterStackOptions.Endpoint);
        }

        // Apply namespace overrides
        foreach (var ns in options.OverrideToWarning)
        {
            loggerConfiguration.MinimumLevel.Override(ns, LogEventLevel.Warning);
        }

        // Enrich with application name if provided
        if (!string.IsNullOrWhiteSpace(options.ApplicationName))
        {
            loggerConfiguration.Enrich.WithProperty("Application", options.ApplicationName);
        }

        // Read from configuration
        loggerConfiguration.ReadFrom.Configuration(configuration);
    }

    private static LogEventLevel ParseLogLevel(string level)
    {
        return level.ToLowerInvariant() switch
        {
            "verbose" => LogEventLevel.Verbose,
            "debug" => LogEventLevel.Debug,
            "information" => LogEventLevel.Information,
            "warning" => LogEventLevel.Warning,
            "error" => LogEventLevel.Error,
            "fatal" => LogEventLevel.Fatal,
            _ => LogEventLevel.Information
        };
    }
}
