using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace SoundHub.Infrastructure.Persistence;

/// <summary>
/// Watches the devices.json file for changes and notifies subscribers.
/// Implements hot-reload functionality for device configuration.
/// </summary>
public sealed class DeviceFileWatcher : IHostedService, IDisposable
{
    private readonly string _filePath;
    private readonly ILogger<DeviceFileWatcher> _logger;
    private readonly bool _enableHotReload;
    private FileSystemWatcher? _watcher;
    private readonly object _eventLock = new();
    private DateTime _lastNotificationTime = DateTime.MinValue;
    private readonly TimeSpan _debounceInterval = TimeSpan.FromMilliseconds(500);

    /// <summary>
    /// Raised when the devices.json file is modified externally.
    /// </summary>
    public event EventHandler<DevicesFileChangedEventArgs>? DevicesFileChanged;

    public DeviceFileWatcher(
        IOptions<FileDeviceRepositoryOptions> options,
        ILogger<DeviceFileWatcher> logger)
    {
        ArgumentNullException.ThrowIfNull(options);
        _filePath = options.Value.FilePath;
        _enableHotReload = options.Value.EnableHotReload;
        _logger = logger;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        if (!_enableHotReload)
        {
            _logger.LogInformation("Hot-reload is disabled for devices.json");
            return Task.CompletedTask;
        }

        var directory = Path.GetDirectoryName(_filePath);
        var fileName = Path.GetFileName(_filePath);

        if (string.IsNullOrEmpty(directory))
        {
            _logger.LogWarning("Cannot watch devices.json: directory path is empty");
            return Task.CompletedTask;
        }

        if (!Directory.Exists(directory))
        {
            _logger.LogWarning("Cannot watch devices.json: directory {Directory} does not exist", directory);
            return Task.CompletedTask;
        }

        try
        {
            _watcher = new FileSystemWatcher(directory, fileName)
            {
                NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.Size | NotifyFilters.CreationTime,
                EnableRaisingEvents = true
            };

            _watcher.Changed += OnFileChanged;
            _watcher.Created += OnFileChanged;
            _watcher.Error += OnWatcherError;

            _logger.LogInformation("Started watching {FilePath} for changes (hot-reload enabled)", _filePath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to start file watcher for {FilePath}", _filePath);
        }

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        DisposeWatcher();
        _logger.LogInformation("Stopped watching devices.json");
        return Task.CompletedTask;
    }

    private void OnFileChanged(object sender, FileSystemEventArgs e)
    {
        // Debounce rapid file change events (common with text editors that do multiple writes)
        lock (_eventLock)
        {
            var now = DateTime.UtcNow;
            if (now - _lastNotificationTime < _debounceInterval)
            {
                return;
            }
            _lastNotificationTime = now;
        }

        _logger.LogInformation("Detected change in {FilePath}, triggering hot-reload", _filePath);

        try
        {
            DevicesFileChanged?.Invoke(this, new DevicesFileChangedEventArgs(_filePath, e.ChangeType));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error notifying subscribers of devices.json change");
        }
    }

    private void OnWatcherError(object sender, ErrorEventArgs e)
    {
        _logger.LogError(e.GetException(), "File watcher error for {FilePath}", _filePath);

        // Attempt to restart the watcher
        try
        {
            DisposeWatcher();
            _ = StartAsync(CancellationToken.None);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to restart file watcher");
        }
    }

    private void DisposeWatcher()
    {
        if (_watcher != null)
        {
            _watcher.Changed -= OnFileChanged;
            _watcher.Created -= OnFileChanged;
            _watcher.Error -= OnWatcherError;
            _watcher.EnableRaisingEvents = false;
            _watcher.Dispose();
            _watcher = null;
        }
    }

    public void Dispose()
    {
        DisposeWatcher();
    }
}

/// <summary>
/// Event arguments for devices file change notifications.
/// </summary>
public class DevicesFileChangedEventArgs : EventArgs
{
    /// <summary>
    /// The path to the devices.json file that changed.
    /// </summary>
    public string FilePath { get; }

    /// <summary>
    /// The type of change that occurred.
    /// </summary>
    public WatcherChangeTypes ChangeType { get; }

    public DevicesFileChangedEventArgs(string filePath, WatcherChangeTypes changeType)
    {
        FilePath = filePath;
        ChangeType = changeType;
    }
}
