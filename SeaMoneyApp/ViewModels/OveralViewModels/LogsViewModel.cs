using System.IO;
using System.Linq;
using System.Threading;
using Avalonia.Controls.Documents;
using Avalonia.Media;
using Avalonia.Threading;
using ReactiveUI;
using SeaMoneyApp.Services.Logger;

namespace SeaMoneyApp.ViewModels.OveralViewModels;

public class LogsViewModel : RoutableViewModel
{
    private InlineCollection _logContent = new();

    public InlineCollection LogContent
    {
        get
        {
            
            return _logContent;
        }
        set => this.RaiseAndSetIfChanged(ref _logContent, value);
    }

    public LogsViewModel()
    {
        UpdateLogs();
        SetupFileWatcher();
    }

    public void UpdateLogs()
    {
        var logFilePath = LoggerSetup.LogFilePath;
        //LogHost.Default.Debug($"GetLogFilePath is {logFilePath}");
        string logString;
        if (System.IO.File.Exists(logFilePath))
        {
            logString = System.IO.File.ReadAllText(logFilePath);
        }
        else
        {
            logString = $"# [ERROR] Ошибка\nФайл лога не найден:\n{logFilePath}";
        }

        ApplyColoredLogText(LogContent, logString);
    }

    private void ApplyColoredLogText(InlineCollection inlineCollection, string logContent)
    {
        inlineCollection.Clear();
        // LogHost.Default.Debug($"ApplyColoredLogText using {logContent}");
        var lines = logContent.Split('\n', '\r').Where(l => !string.IsNullOrWhiteSpace(l)).ToList();

        for (int i = 0; i < lines.Count; i++)
        {
            var line = lines[i];
            var brush = GetColorForLine(line);

            inlineCollection.Add(new Run(line + "\n") { Foreground = brush });
        }
    }

    private IBrush GetColorForLine(string line)
    {
        // LogHost.Default.Debug($"GetColorForLine using {line}");
        return line.Contains("[Fatal]", System.StringComparison.OrdinalIgnoreCase)
            ? Brushes.DarkRed
            : line.Contains("[Error]", System.StringComparison.OrdinalIgnoreCase)
                ? Brushes.Red
                : line.Contains("[Warn]", System.StringComparison.OrdinalIgnoreCase)
                    ? Brushes.Orange
                    : line.Contains("[Info]", System.StringComparison.OrdinalIgnoreCase)
                        ? Brushes.Blue
                        : line.Contains("[Debug]", System.StringComparison.OrdinalIgnoreCase)
                            ? Brushes.Black
                            : Brushes.Gray;
    }
    
    private FileSystemWatcher _watcher;
    
    private void SetupFileWatcher()
    {
        var path = Path.GetDirectoryName(LoggerSetup.LogFilePath);
        var fileName = Path.GetFileName(LoggerSetup.LogFilePath);

        _watcher = new FileSystemWatcher(path, fileName)
        {
            EnableRaisingEvents = true,
            NotifyFilter = NotifyFilters.LastWrite
        };

        _watcher.Changed += (s, e) =>
        {
            // Защита от множественных вызовов
            Thread.Sleep(100);
            Dispatcher.UIThread.Post(UpdateLogs);
        };
    }
}