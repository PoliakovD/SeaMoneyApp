using System.Text;
using HtmlParcerCbrCources;
using SeaMoneyApp.DataAccess.Models;
using Splat;

namespace TestAutoUpdateCources
{
    
    class Program
    {

        static async Task Main(string[] args)
        {
            Console.WriteLine("=== РУЧНОЙ ТЕСТ: Получение курса доллара с ЦБ РФ ===\n");
            var ressultCources = await GetUsdCourcesFrom2020();
            foreach (var course in ressultCources)
            {
                Console.WriteLine($"Дата:{course.Date} - Курс:{course.Value}");
            }
            Console.WriteLine("Нажмите любую клавишу для выхода...");
            Console.ReadKey();
        }
        public static List<DateTime> GetDatesFrom2020()
        {
            var result = new List<DateTime>(); 
            int year = 2020;
            int month = 1;
            const int day = 15;
            DateTime observedDate = new DateTime(year, month, day);
            var currentDate = DateTime.Today;

            while (observedDate <= currentDate)
            {
                result.Add(observedDate);
                if (month == 12)
                {
                    month = 1;
                    ++year;
                }
                ++month;
                observedDate = new DateTime(year, month, day);
            }
            return result;
        }
        public static async Task<List<ChangeRubToDollar>> GetUsdCourcesFrom2020()
        {
            var listDates = GetDatesFrom2020();
            var result = new List<ChangeRubToDollar>();
            foreach (var date in listDates)
            {
                var course =  await HTMLParcerCbrCources.GetUsdCourseOnDateAsync(date);
                result.Add(course);
                Console.WriteLine($"Дата:{course.Date} - Курс:{course.Value}");
                Thread.Sleep(100);
            }
            return result;
        }
    }
}