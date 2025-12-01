using ReactiveUI;
using SeaMoneyApp.Services.Logger;
using Splat;

namespace SeaMoneyApp.ViewModels;

public class LogsViewModel : RoutableViewModel
{
    private string _logContent = string.Empty;

    public string LogContent
    {
        get => _logContent;
        set => this.RaiseAndSetIfChanged(ref _logContent, value);
    }

    public LogsViewModel()
    {
        var logFilePath = LoggerSetup.GetLogFilePath();
        //LogHost.Default.Debug($"GetLogFilePath is {logFilePath}");
        if (System.IO.File.Exists(logFilePath))
        {
            LogContent = System.IO.File.ReadAllText(logFilePath);
           
        }
        else
        {
            LogContent = $"# Ошибка\nФайл лога не найден:\n{logFilePath}";
        }
    }
}