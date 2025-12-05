using System;
using System.Collections.Concurrent;
using System.Globalization;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using SeaMoneyApp.DataAccess.Models;
using Splat;

namespace HtmlParcerCbrCources
{
    public static class HTMLParcerCbrCources
    {
        private static readonly HttpClient HttpClient = new HttpClient();
        
        // Кэш для курсов: дата -> курс USD/RUB
        private static readonly ConcurrentDictionary<DateTime, ChangeRubToDollar> CourseCache = new();

        static HTMLParcerCbrCources()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            HttpClient.DefaultRequestHeaders.Add("User-Agent",
                "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36");
            
            LogHost.Default.Info("HTMLParcerCbrCources Constructor");
            LogHost.Default.Info($"{HttpClient.BaseAddress}");
            LogHost.Default.Info($"{HttpClient.DefaultRequestHeaders}");
            LogHost.Default.Info($"{HttpClient.DefaultProxy}");
        }

        public static async Task<ChangeRubToDollar> GetUsdCourseOnDateAsync(DateTime date)
        {

            // Проверяем кэш
            if (CourseCache.TryGetValue(date, out var cachedCourse))
            {
                LogHost.Default.Debug($"Cache hit for date {date:dd.MM.yyyy}: {cachedCourse.Value}");
                return cachedCourse;
            }

            // Если нет в кэше — загружаем
            var course = await FetchCourseFromCbr(date);
            if (course != null)
            {
                // Сохраняем в кэш только при успешном получении
                CourseCache.TryAdd(date, course);
                LogHost.Default.Debug($"Course cached for date {date:dd.MM.yyyy}: {course.Value}");
            }

            return course;
        }

        private static async Task<ChangeRubToDollar> FetchCourseFromCbr(DateTime date)
        {
            string dateString = date.ToString("dd/MM/yyyy");
            string url = $"https://www.cbr.ru/scripts/XML_daily.asp?date_req={dateString}";

            try
            {
                var response = await HttpClient.GetAsync(url);
                
                if (!response.IsSuccessStatusCode)
                {
                    LogHost.Default.Error("Response status is not success");
                    return null;
                }
                
                LogHost.Default.Debug($"Response: {response.StatusCode}");
                
                byte[] raw = await response.Content.ReadAsByteArrayAsync();
                string xmlContent = Encoding.GetEncoding("windows-1251").GetString(raw);

                var doc = XDocument.Parse(xmlContent);
                if (doc == null)
                {
                    LogHost.Default.Error("XDocument.Parse returned null");
                    return null;
                }

                var usdElement = doc.Descendants("Valute")
                    .FirstOrDefault(v => v.Element("CharCode")?.Value == "USD");

                if (usdElement == null)
                {
                    LogHost.Default.Warn("USD element not found in XML");
                    return null;
                }

                char decimalSeparator = CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator[0];
                LogHost.Default.Info($"Current decimal separator: {decimalSeparator}");

                string? valueText = usdElement.Element("Value")?.Value;
                if (string.IsNullOrWhiteSpace(valueText))
                {
                    LogHost.Default.Warn("Value text is null or empty");
                    return null;
                }

                // Заменяем стандартный разделитель ЦБ (,) на локальный
                valueText = valueText.Replace(',', decimalSeparator);

                if (decimal.TryParse(valueText, NumberStyles.Currency | NumberStyles.Number, 
                    CultureInfo.CurrentCulture, out decimal course))
                {
                    LogHost.Default.Debug($"USD course parsed: {course}");
                    return new ChangeRubToDollar
                    {
                        Id = null,
                        Value = course,
                        Date = date
                    };
                }

                LogHost.Default.Warn($"Failed to parse course value: {valueText}");
                return null;
            }
            catch (Exception ex)
            {
                LogHost.Default.Error(ex, "Exception in GetUsdCourseOnDateAsync");
                return null;
            }
        }

        // Опционально: очистка кэша (например, при ручном обновлении)
        public static void ClearCache() => CourseCache.Clear();

        // Опционально: предзагрузка курсов за несколько дней
        public static async Task PreloadCoursesAsync(DateTime startDate, DateTime endDate)
        {
            for (var date = startDate.Date; date <= endDate; date = date.AddDays(1))
            {
                if (!CourseCache.ContainsKey(date))
                {
                    await GetUsdCourseOnDateAsync(date);
                }
            }
        }
    }
}