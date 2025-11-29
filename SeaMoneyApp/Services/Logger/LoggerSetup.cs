using System;
using System.IO;
using Splat;

namespace SeaMoneyApp.Services.Logger;
public static class LoggerSetup
{
    public static void SetupLogger(LogLevel level = LogLevel.Debug)
    {
        string logPath;
        var appDirectory = AppDomain.CurrentDomain.BaseDirectory;
        var logDirectory = Path.Combine(appDirectory, "log");

        try
        {
            // Пытаемся создать папку и проверить доступ
            Directory.CreateDirectory(logDirectory);
            logPath = Path.Combine(logDirectory, "logs.txt");
            
            // Проверим, можем ли мы записать тестовую строку 
            File.AppendAllText(logPath, "");
        }
        catch
        {
            // Если не удалось записать в папку приложения — используем временный каталог
            var tempDir = Path.Combine(Path.GetTempPath(), "SeaMoneyApp", "log");
            Directory.CreateDirectory(tempDir);
            logPath = Path.Combine(tempDir, "logs.txt");
        }

        Locator.CurrentMutable.Register<ILogger>(() => new FileLogger(logPath){Level = level});
    }
    
    private class FileLogger : ILogger
    {
        private readonly string _logPath;
        public LogLevel Level { get; init; }
        public FileLogger(string logPath)
        {
            _logPath = logPath ?? throw new ArgumentNullException(nameof(logPath));
        }

        public void Write(string message, LogLevel logLevel)
        {
            if (logLevel < Level) return;
            WriteToLog($"[{logLevel}] {message}");
        }

        public void Write(Exception exception, string message, LogLevel logLevel)
        {
            if (logLevel < Level) return;
            WriteToLog($"[{logLevel}] {message}\nEXCEPTION: {exception}");
        }
        
        public void Write(string message, Type type, LogLevel logLevel)
        {
            if (logLevel < Level) return;
            WriteToLog($"[{logLevel}] [{type.Name}] {message}");
        }
        
        public void Write(Exception exception, string message, Type type, LogLevel logLevel)
        {
            if (logLevel < Level) return;
            WriteToLog($"[{logLevel}] [{type.Name}] {message}\nEXCEPTION: {exception}");
        }
        
        private void WriteToLog(string line)
        {
            try
            {
                var entry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {line}";
                File.AppendAllText(_logPath, entry + Environment.NewLine);
            }
            catch
            {
                // Игнорируем ошибки записи (например, нет прав или диск полон)
            }
        }
    }
}