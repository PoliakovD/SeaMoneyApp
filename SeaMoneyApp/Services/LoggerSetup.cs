using System;
using System.IO;
using Splat;

namespace SeaMoneyApp.Services;

/// <summary>
/// Сервис для настройки глобального логирования с использованием библиотеки <c>Splat</c>.
/// Обеспечивает запись логов в файл в подпапке <c>log</c> рядом с исполняемым файлом приложения.
/// В случае отсутствия прав на запись — автоматически переключается на временный каталог.
/// </summary>
/// <remarks>
/// Для использования: вызовите <see cref="SetupLogger(LogLevel)"/> один раз при запуске приложения.
/// Логи будут доступны по пути:
/// <list type="bullet">
///   <item>Основной путь: <c>{AppDirectory}/log/logs.txt</c></item>
///   <item>Резервный путь: <c>{TempDirectory}/SeaMoneyApp/log/logs.txt</c></item>
/// </list>
/// Поддерживает кроссплатформенность (Windows, Linux, macOS).
/// </remarks>
public static class LoggerSetup
{
    /// <summary>
    /// Настраивает глобальный логгер <see cref="ILogger"/> через <c>Splat</c> с записью в файл.
    /// Уровень логирования можно задать при вызове.
    /// </summary>
    /// <param name="level">Минимальный уровень логирования (по умолчанию <see cref="LogLevel.Debug"/>).</param>
    /// <example>
    /// Пример использования:
    /// <code>
    /// LoggerSetup.SetupLogger(LogLevel.Info); // Логировать сообщения уровня Info и выше
    /// </code>
    /// </example>
    /// <remarks>
    /// Метод должен быть вызван один раз при старте приложения (например, в конструкторе <c>App</c> или <c>MainWindow</c>).
    /// Перед настройкой убедитесь, что <c>Locator.CurrentMutable</c> доступен.
    /// </remarks>
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

    /// <summary>
    /// Внутренняя реализация <see cref="ILogger"/> для записи логов в файл.
    /// Поддерживает различные перегрузки метода <c>Write</c> и фильтрацию по уровню.
    /// </summary>
    /// <remarks>
    /// Все записи дополняются временной меткой и автоматически переводятся на новую строку.
    /// Ошибки ввода-вывода подавляются, чтобы не нарушать работу приложения.
    /// </remarks>
    private class FileLogger : ILogger
    {
        private readonly string _logPath;

        /// <summary>
        /// Получает минимальный уровень логирования.
        /// Сообщения ниже этого уровня игнорируются.
        /// </summary>
        /// <value>Значение типа <see cref="LogLevel"/>.</value>
        public LogLevel Level { get; init; }

        /// <summary>
        /// Инициализирует новый экземпляр <see cref="FileLogger"/>.
        /// </summary>
        /// <param name="logPath">Полный путь к файлу лога.</param>
        /// <exception cref="ArgumentNullException">Выбрасывается, если <paramref name="logPath"/> равен <c>null</c>.</exception>
        public FileLogger(string logPath)
        {
            _logPath = logPath ?? throw new ArgumentNullException(nameof(logPath));
        }

        /// <summary>
        /// Записывает сообщение в лог, если его уровень соответствует или превышает <see cref="Level"/>.
        /// </summary>
        /// <param name="message">Текст сообщения.</param>
        /// <param name="logLevel">Уровень логирования (например, Debug, Info, Warn и т.д.).</param>
        public void Write(string message, LogLevel logLevel)
        {
            if (logLevel < Level) return;
            WriteToLog($"[{logLevel}] {message}");
        }

        /// <summary>
        /// Записывает сообщение об ошибке с информацией об исключении.
        /// </summary>
        /// <param name="exception">Исключение, связанное с сообщением.</param>
        /// <param name="message">Текст сообщения.</param>
        /// <param name="logLevel">Уровень логирования.</param>
        public void Write(Exception exception, string message, LogLevel logLevel)
        {
            if (logLevel < Level) return;
            WriteToLog($"[{logLevel}] {message}\nEXCEPTION: {exception}");
        }

        /// <summary>
        /// Записывает сообщение с указанием типа (класса), откуда оно пришло.
        /// </summary>
        /// <param name="message">Текст сообщения.</param>
        /// <param name="type">Тип, инициировавший запись (обычно <c>typeof(ThisClass)</c>).</param>
        /// <param name="logLevel">Уровень логирования.</param>
        public void Write(string message, Type type, LogLevel logLevel)
        {
            if (logLevel < Level) return;
            WriteToLog($"[{logLevel}] [{type.Name}] {message}");
        }

        /// <summary>
        /// Записывает сообщение с указанием типа и исключением.
        /// </summary>
        /// <param name="exception">Исключение, связанное с сообщением.</param>
        /// <param name="message">Текст сообщения.</param>
        /// <param name="type">Тип, инициировавший запись.</param>
        /// <param name="logLevel">Уровень логирования.</param>
        public void Write(Exception exception, string message, Type type, LogLevel logLevel)
        {
            if (logLevel < Level) return;
            WriteToLog($"[{logLevel}] [{type.Name}] {message}\nEXCEPTION: {exception}");
        }

        /// <summary>
        /// Форматирует и записывает строку в файл лога с временной меткой.
        /// </summary>
        /// <param name="line">Форматированная строка для записи.</param>
        /// <remarks>
        /// В случае ошибки (диск полон, нет прав и т.п.) ошибка подавляется.
        /// </remarks>
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