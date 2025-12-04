using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using HtmlParcerCbrCources;
using SeaMoneyApp.DataAccess.Models;

namespace HtmlParcerCbrCources.ConsoleTest
{
    /// <summary>
    ///  сгенерировано гигакодом
    /// </summary>
    class Program
    {
        private static readonly HttpClient client = new HttpClient();

        static async Task Main(string[] args)
        {
            // 🔥 САМАЯ ВАЖНАЯ СТРОКА — ДО ВСЕГО ОСТАЛЬНОГО
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            
            Console.WriteLine($"Поддерживаемые кодировки: {string.Join(", ", Encoding.GetEncodings().Select(e => e.Name))}");
            
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Console.WriteLine("=== РУЧНОЙ ТЕСТ: Получение курса доллара с ЦБ РФ ===\n");

            // Добавим User-Agent
            client.DefaultRequestHeaders.Add("User-Agent", 
                "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36");

            var testDates = new[]
            {
                DateTime.Today,
                DateTime.Today.AddDays(-1),
                new DateTime(2024, 4, 1),
                new DateTime(2024, 1, 13), // Сб
                new DateTime(2024, 1, 14), // Вс
                DateTime.Today.AddDays(7)
            };

            foreach (var date in testDates)
            {
                Console.WriteLine($"📅 ТЕСТ: Запрос курса на дату — {date:dd.MM.yyyy} ({date:dddd})");
                string dateString = date.ToString("dd/MM/yyyy");
                string url = $"https://www.cbr.ru/scripts/XML_daily.asp?date_req={dateString}";
                Console.WriteLine($"   🌐 URL запроса: {url}");

                try
                {
                    Console.WriteLine("   📥 Выполняем HTTP-запрос...");
                    var response = await client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);

                    if (!response.IsSuccessStatusCode)
                    {
                        Console.WriteLine($"   ❌ Ошибка HTTP: {response.StatusCode} ({(int)response.StatusCode})");
                        Console.WriteLine($"      Reason: {response.ReasonPhrase}");
                        continue;
                    }

                    // Читаем байты
                    byte[] rawBytes = await response.Content.ReadAsByteArrayAsync();

                    // ✅ Теперь windows-1251 доступна
                    Encoding win1251 = Encoding.GetEncoding("windows-1251");
                    string xmlContent = win1251.GetString(rawBytes);

                    Console.WriteLine("   ✅ Ответ получен и декодирован из Windows-1251. Длина: " + xmlContent.Length);

                    // Выводим начало XML
                    Console.WriteLine("   📄 Начало XML (первые 500 символов):");
                    Console.WriteLine("   " + new string('─', 48));
                    Console.WriteLine("   " + xmlContent.Substring(0, Math.Min(500, xmlContent.Length)).Replace("\n", "\n   "));
                    if (xmlContent.Length > 500) Console.WriteLine("   ... [обрезано]");

                    // Проверка наличия USD
                    if (xmlContent.Contains("<CharCode>USD</CharCode>"))
                    {
                        Console.WriteLine("   ✅ Найден блок с USD");
                    }
                    else
                    {
                        Console.WriteLine("   ⚠️ Блок USD не найден в XML");
                    }

                    Console.WriteLine("   🔍 Вызываем GetUsdCourseOnDateAsync(date)...");
                    var result = await HTMLParcerCbrCources.GetUsdCourseOnDateAsync(date);

                    if (result != null)
                    {
                        Console.WriteLine($"   ✅ УСПЕШНО: Курс USD = {result.Value:F4} ₽");
                        Console.WriteLine($"      Применённая дата: {result.Date:dd.MM.yyyy}");
                    }
                    else
                    {
                        Console.WriteLine("   ❌ Метод вернул NULL — возможно, проблема в парсинге");
                    }
                }
                catch (HttpRequestException httpEx)
                {
                    Console.WriteLine($"   💥 СЕТЬ: Ошибка HTTP — {httpEx.Message}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"   💥 ОШИБКА: {ex.GetType().Name}");
                    Console.WriteLine($"      Сообщение: {ex.Message}");
                }

                Console.WriteLine(new string('=', 60));
                await Task.Delay(1200);
            }

            Console.WriteLine("🔚 Все тесты завершены.");
            Console.WriteLine("Нажмите любую клавишу для выхода...");
            Console.ReadKey();
        }
    }
}