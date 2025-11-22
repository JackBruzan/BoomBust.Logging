# BoomBust.Logging

A reusable .NET 9 logging library that provides easy integration with BetterStack (formerly Logtail) using Serilog. Perfect for scrapers, background services, and web applications.

## Features

- ?? **Easy Setup** - One line of code to configure logging
- ?? **Multiple Sinks** - Console, File, and BetterStack
- ?? **Highly Configurable** - Customize everything or use smart defaults
- ?? **Secure** - BetterStack is optional and configured via environment variables
- ?? **NuGet Ready** - Install and use across all your projects
- ?? **Structured Logging** - Full support for structured logging patterns

## Installation

```bash
dotnet add package BoomBust.Logging
```

Or manually add to your `.csproj`:

```xml
<PackageReference Include="BoomBust.Logging" Version="1.0.0" />
```

## Quick Start

### Minimal Configuration

```csharp
using BoomBust.Logging;

var builder = WebApplication.CreateBuilder(args);

// Add BoomBust logging with default settings
builder.UseBoomBustLogging();

var app = builder.Build();
app.Run();
```

### With Custom Options

```csharp
using BoomBust.Logging;

var builder = WebApplication.CreateBuilder(args);

builder.UseBoomBustLogging(options =>
{
    options.ApplicationName = "MyAwesomeScraper";
    options.LogFilePath = "logs/scraper-.txt";
    options.MinimumLevel = "Debug";
    options.OverrideToWarning = new[] { "Microsoft", "System", "Quartz" };
});

var app = builder.Build();
app.Run();
```

### For Generic Host (Console Apps, Background Services)

```csharp
using BoomBust.Logging;
using Microsoft.Extensions.Hosting;

var builder = Host.CreateDefaultBuilder(args);

builder.UseBoomBustLogging(options =>
{
    options.ApplicationName = "MyScraper";
    options.EnableConsole = true;
    options.EnableFile = true;
});

await builder.Build().RunAsync();
```

## Configuration

### appsettings.json

Add BetterStack configuration to your `appsettings.json`:

```json
{
  "BetterStack": {
    "SourceToken": "",
    "Endpoint": "https://in.logs.betterstack.com"
  }
}
```

**Important**: Don't commit your source token! Use environment variables instead.

### Environment Variables

```bash
# PowerShell
$env:BetterStack__SourceToken="your-token-here"

# Bash/Mac
export BetterStack__SourceToken="your-token-here"

# Docker
docker run -e BetterStack__SourceToken=your-token-here myapp
```

### Docker Compose

```yaml
environment:
  - BetterStack__SourceToken=${BETTERSTACK_SOURCE_TOKEN}
  - BetterStack__Endpoint=https://in.logs.betterstack.com
```

## Options

### BoomBustLoggingOptions

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `ApplicationName` | `string?` | `null` | Application name to enrich logs with |
| `LogFilePath` | `string` | `"logs/log.txt"` | Path for log files |
| `RollingInterval` | `RollingInterval` | `Day` | How often to roll log files |
| `MinimumLevel` | `string` | `"Information"` | Minimum log level to capture |
| `EnableConsole` | `bool` | `true` | Enable console logging |
| `EnableFile` | `bool` | `true` | Enable file logging |
| `OverrideToWarning` | `string[]` | `["Microsoft", "System"]` | Namespaces to set to Warning level |

### RollingInterval Options

- `Infinite` - Single log file
- `Year` - New file each year
- `Month` - New file each month
- `Day` - New file each day (default)
- `Hour` - New file each hour
- `Minute` - New file each minute

## Usage Examples

### Simple Scraper

```csharp
using BoomBust.Logging;
using Serilog;

var builder = Host.CreateDefaultBuilder(args);
builder.UseBoomBustLogging();

var app = builder.Build();

Log.Information("Scraper starting...");

try
{
    // Your scraping logic
    Log.Information("Processing {Count} items", items.Count);
}
catch (Exception ex)
{
    Log.Error(ex, "Scraper failed");
}
finally
{
    Log.CloseAndFlush();
}
```

### Web API with Custom Configuration

```csharp
using BoomBust.Logging;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.UseBoomBustLogging(options =>
{
    options.ApplicationName = "FantasyCalcAPI";
    options.LogFilePath = "logs/api-.txt";
    options.RollingInterval = RollingInterval.Hour;
    options.MinimumLevel = builder.Environment.IsDevelopment() ? "Debug" : "Information";
});

builder.Services.AddControllers();

var app = builder.Build();

Log.Information("API starting on {Environment}", app.Environment.EnvironmentName);

app.MapControllers();
await app.RunAsync();
```

### Background Service with Quartz

```csharp
using BoomBust.Logging;
using Quartz;
using Serilog;

var builder = Host.CreateDefaultBuilder(args);

builder.UseBoomBustLogging(options =>
{
    options.ApplicationName = "DataSyncService";
    options.OverrideToWarning = new[] { "Microsoft", "System", "Quartz" };
});

builder.ConfigureServices(services =>
{
    services.AddQuartz(q =>
    {
        var jobKey = new JobKey("SyncJob");
        q.AddJob<SyncJob>(opts => opts.WithIdentity(jobKey));
        q.AddTrigger(opts => opts
            .ForJob(jobKey)
            .WithCronSchedule("0 0 */4 * * ?")); // Every 4 hours
    });
    services.AddQuartzHostedService();
});

await builder.Build().RunAsync();
```

## Getting Your BetterStack Token

1. Visit [https://logs.betterstack.com](https://logs.betterstack.com)
2. Sign up for free (1GB/month included)
3. Go to **Sources** ? **Connect source** ? **HTTP**
4. Copy your **Source Token**
5. Add to environment variables or configuration

## Without BetterStack

The library works perfectly without BetterStack! If no token is provided:
- ? Logs to console
- ? Logs to file
- ? No errors or warnings
- ? Add BetterStack later when ready

## Structured Logging Best Practices

Use message templates with properties for better searchability:

```csharp
// ? Good - Structured
Log.Information("Processing player {PlayerId} with value {Value}", playerId, value);

// ? Bad - String interpolation
Log.Information($"Processing player {playerId} with value {value}");
```

This allows you to search in BetterStack:
```
PlayerId:12345
Value:>1000
```

## Log Levels

- **Verbose** - Very detailed diagnostic information
- **Debug** - Internal system events (dev only)
- **Information** - Normal application flow
- **Warning** - Unexpected but recoverable
- **Error** - Operation failures
- **Fatal** - Critical application failures

## Building from Source

```bash
git clone https://github.com/JackBruzan/BoomBust.Logging
cd BoomBust.Logging
dotnet build
dotnet pack
```

## Publishing to NuGet

```bash
dotnet pack -c Release
dotnet nuget push bin/Release/BoomBust.Logging.1.0.0.nupkg --api-key YOUR_API_KEY --source https://api.nuget.org/v3/index.json
```

## License

MIT License - Use freely in your projects!

## Support

- **Issues**: [GitHub Issues](https://github.com/JackBruzan/BoomBust.Logging/issues)
- **BetterStack Docs**: [https://betterstack.com/docs/logs/](https://betterstack.com/docs/logs/)
- **Serilog Docs**: [https://serilog.net/](https://serilog.net/)

## Changelog

### 1.0.0 (2025-01-20)
- Initial release
- BetterStack integration
- Console and file logging
- Configurable options
- .NET 9 support
