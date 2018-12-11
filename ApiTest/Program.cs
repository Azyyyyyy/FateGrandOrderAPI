using System;
using FateGrandOrderApi;
using System.Diagnostics;
using System.Text;

namespace ApiTest
{
    class Program
    {
        static void Main(string[] args)
        {
            Stopwatch stopwatch = new Stopwatch();
            Console.WriteLine("  Api Tester  ");
            Console.WriteLine("--------------");
            Console.WriteLine("Getting Jeanne d'Arc (Alter) data");
            stopwatch.Start();
            var Person1 = FateGrandOrderParsing.GetPerson("Jeanne d'Arc (Alter)");
            stopwatch.Stop();
            Console.WriteLine($"It took {stopwatch.Elapsed} to get Jeanne d'Arc (Alter) data");
            stopwatch.Reset();
            Console.WriteLine("Getting Jeanne d'Arc (Alter) again");
            stopwatch.Start();
            var Person2 = FateGrandOrderParsing.GetPerson("Jeanne d'Arc (Alter)");
            stopwatch.Stop();
#if !DEBUG
            Console.WriteLine($"It took {stopwatch.Elapsed} to get Jeanne d'Arc (Alter) data again");
#endif
#if DEBUG
            Console.WriteLine($"It took {stopwatch.Elapsed} to get Jeanne d'Arc (Alter) data again (Is cached: {Person2.FromCache})");
#endif

            StringBuilder Servant = new StringBuilder();
            Console.WriteLine("Getting Jack the Ripper data");
            var person = FateGrandOrderParsing.GetPerson("Jack_the_Ripper");
            Servant.AppendLine($"Name: {person.BasicInfomation.EnglishName}");
            Servant.AppendLine($"Jap name: {person.BasicInfomation.JapaneseName}");
            Servant.AppendLine($"Gender: {person.BasicInfomation.Gender}");
            Servant.AppendLine($"ATK: {person.BasicInfomation.ATK}");
            Servant.AppendLine($"Class: {person.BasicInfomation.Class}");
            Servant.AppendLine($"Cost: {person.BasicInfomation.Cost}");
            Console.WriteLine(Servant);
            Console.ReadKey();
        }
    }
}
