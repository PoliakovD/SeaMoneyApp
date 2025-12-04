using System.Text;
using HtmlParcerCbrCources;

namespace TestAutoUpdateCources
{
    
    class Program
    {

        static async Task Main(string[] args)
        {
            Console.WriteLine("=== РУЧНОЙ ТЕСТ: Получение курса доллара с ЦБ РФ ===\n");
            var ressultCources = await HTMLParcerCbrCources.GetUsdCourcesFrom2020();
            foreach (var course in ressultCources)
            {
                Console.WriteLine($"Дата:{course.Date} - Курс:{course.Value}");
            }
            Console.WriteLine("Нажмите любую клавишу для выхода...");
            Console.ReadKey();
        }
    }
}