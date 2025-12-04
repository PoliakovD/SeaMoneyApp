using System;
using System.Globalization;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using SeaMoneyApp.DataAccess.Models;

namespace HtmlParcerCbrCources
{
    public static class HTMLParcerCbrCources
    {
        private static readonly HttpClient HttpClient = new HttpClient();

        static HTMLParcerCbrCources()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            HttpClient.DefaultRequestHeaders.Add("User-Agent",
                "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36");
        }

        public static async Task<ChangeRubToDollar> GetUsdCourseOnDateAsync(DateTime date)
        {
            string dateString = date.ToString("dd/MM/yyyy");
            string url = $"https://www.cbr.ru/scripts/XML_daily.asp?date_req={dateString}";

            try
            {
                var response = await HttpClient.GetAsync(url);
                if (!response.IsSuccessStatusCode) return null;

                byte[] raw = await response.Content.ReadAsByteArrayAsync();
                string xmlContent = Encoding.GetEncoding("windows-1251").GetString(raw);

                var doc = XDocument.Parse(xmlContent);

                var usdElement = doc.Descendants("Valute")
                    .FirstOrDefault(v => v.Element("CharCode")?.Value == "USD");

                if (usdElement == null) return null;

                string valueText = usdElement.Element("Value")?.Value?.Replace(',', '.');
                if (decimal.TryParse(valueText, NumberStyles.Float, null, out decimal course))
                {
                    return new ChangeRubToDollar
                    {
                        Id = null,
                        Value = course,
                        Date = date.Date
                    };
                }

                return null;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public static List<DateTime> GetDatesFrom2020()
        {
            var result = new List<DateTime>();
            int year = 2024;
            int month = 1;
            const int day = 15;
            DateTime observedDate = new DateTime(year, month, day);
            var currentDate = DateTime.Today;

            while (observedDate <= currentDate)
            {
                observedDate = new DateTime(year, month, day);
                result.Add(observedDate);
                if (month == 12)
                {
                    month = 1;
                    ++year;
                }
                ++month;
            }
            return result;
        }

        public static async Task<List<ChangeRubToDollar>> GetUsdCourcesFrom2020()
        {
            var listDates = GetDatesFrom2020();
            var result = new List<ChangeRubToDollar>();
            foreach (var date in listDates)
            {
                var course =  await GetUsdCourseOnDateAsync(date);
                result.Add(course);
                Console.WriteLine($"Дата:{course.Date} - Курс:{course.Value}");
                Thread.Sleep(200);
            }
            return result;
        }
    }
}