using System;
using System.IO;
using System.Linq;
using Splat;

namespace SeaMoneyApp.Services.Logger;
public static class LoggerSetup
{
    private static string? _logFilePath; // Приватное поле

    public static string LogFilePath => _logFilePath ?? throw new InvalidOperationException("Logger not initialized");
    
    public static void SetupLogger(LogLevel level = LogLevel.Debug)
    {
        if (_logFilePath != null) return; // Защита от повторного вызова
        _logFilePath = GenerateLogFilePath();
        Locator.CurrentMutable.Register<ILogger>(() => new FileLogger(LogFilePath){Level = level});
    }
    
    private static string GenerateLogFilePath()
    {
        string logPath;
        var appDirectory = AppDomain.CurrentDomain.BaseDirectory;
        var logDirectory = Path.Combine(appDirectory, "log");

        try
        {
            // Создаём папку log, если её нет
            Directory.CreateDirectory(logDirectory);
            
            // Удаляем старые логи, оставляя только 3 последних
            CleanupOldLogFiles(logDirectory, 3);

            // Генерируем имя файла с меткой времени (каждый запуск — новый файл)
            var timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
            var fileName = $"logs_{timestamp}.log";
            logPath = Path.Combine(logDirectory, fileName);

            // Создаём новый лог-файл
            File.WriteAllText(logPath, $"Log started at {DateTime.Now:yyyy-MM-dd HH:mm:ss}{Environment.NewLine}");
        }
        catch
        {
            // Если не удалось записать в основную папку — используем временную
            var tempDir = Path.Combine(Path.GetTempPath(), "SeaMoneyApp", "log");
            Directory.CreateDirectory(tempDir);
            
            // Очищаем резервную папку
            CleanupOldLogFiles(tempDir, 3);
            
            var timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
            var fileName = $"logs_{timestamp}.log";
            logPath = Path.Combine(tempDir, fileName);

            File.WriteAllText(logPath, $"Log started at {DateTime.Now:yyyy-MM-dd HH:mm:ss}{Environment.NewLine}");
        }

        return logPath;
    }
    
    private static void CleanupOldLogFiles(string directory, int maxFilesToKeep = 3)
    {
        if (!Directory.Exists(directory)) return;

        var logFiles = Directory.GetFiles(directory, "logs_*.log")
            .Select(f => new FileInfo(f))
            .OrderByDescending(f => f.CreationTime)  // Сортируем по времени создания: новые сверху
            .Skip(maxFilesToKeep)                    // Пропускаем первые N (самые новые)
            .ToList();

        foreach (var file in logFiles)
        {
            try
            {
                file.Delete();
            }
            catch
            {
                // Игнорируем ошибки удаления (файл может быть занят)
            }
        }
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