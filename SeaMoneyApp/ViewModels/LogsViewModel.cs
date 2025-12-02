using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Media;
using ReactiveUI;
using SeaMoneyApp.Services.Logger;
using Splat;

namespace SeaMoneyApp.ViewModels;

public class LogsViewModel : RoutableViewModel
{
    private InlineCollection _logContent = new();

    public InlineCollection LogContent
    {
        get => _logContent;
        set => this.RaiseAndSetIfChanged(ref _logContent, value);
    }

    public LogsViewModel()
    {
        UpdateLogs();
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
}