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
            Console.WriteLine("   Api Tester   ");
            Console.WriteLine("----------------");
            Console.WriteLine("Getting Jeanne d'Arc (Alter)");
            stopwatch.Start();
            var Person1 = FateGrandOrderParsing.GetPerson("Jeanne d'Arc (Alter)");
            stopwatch.Stop();
#if !DEBUG
            Console.WriteLine($"It took {stopwatch.Elapsed} to get Jeanne d'Arc (Alter) data");
#endif
#if DEBUG
            Console.WriteLine($"It took {stopwatch.Elapsed} to get Jeanne d'Arc (Alter) data (Is cached: {Person1.FromCache})");
#endif
            stopwatch.Reset();
            Console.WriteLine("----------------------------------");
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
            stopwatch.Reset();
            Console.WriteLine("------------------------");
            Console.WriteLine("Getting Lancelot (Saber)");
            stopwatch.Start();
            var Person3 = FateGrandOrderParsing.GetPerson("Lancelot (Saber)");
            stopwatch.Stop();
#if !DEBUG
            Console.WriteLine($"It took {stopwatch.Elapsed} to get Lancelot (Saber) data");
#endif
#if DEBUG
            Console.WriteLine($"It took {stopwatch.Elapsed} to get Lancelot (Saber) data (Is cached: {Person3.FromCache})");
#endif
            stopwatch.Reset();
            Console.WriteLine("------------------------");
            Console.WriteLine("Getting Sigurd");
            stopwatch.Start();
            var Person4 = FateGrandOrderParsing.GetPerson("Sigurd");
            stopwatch.Stop();
#if !DEBUG
            Console.WriteLine($"It took {stopwatch.Elapsed} to get Sigurd data");
#endif
#if DEBUG
            Console.WriteLine($"It took {stopwatch.Elapsed} to get Sigurd data (Is cached: {Person4.FromCache})");
#endif
            stopwatch.Reset();
            Console.WriteLine("------------------------");
            Console.WriteLine("Getting Artoria Pendragon (Alter)");
            stopwatch.Start();
            var Person5 = FateGrandOrderParsing.GetPerson("Artoria Pendragon (Alter)");
            stopwatch.Stop();
#if !DEBUG
            Console.WriteLine($"It took {stopwatch.Elapsed} to get Artoria Pendragon (Alter) data");
#endif
#if DEBUG
            Console.WriteLine($"It took {stopwatch.Elapsed} to get Artoria Pendragon (Alter) data (Is cached: {Person5.FromCache})");
#endif
            stopwatch.Reset();
            Console.WriteLine("----------------------------");
            Console.WriteLine("Getting Medb (Saber) data");
            stopwatch.Start();
            var Person6 = FateGrandOrderParsing.GetPerson("Medb_(Saber)");
            stopwatch.Stop();
#if !DEBUG
            Console.WriteLine($"It took {stopwatch.Elapsed} to get Medb (Saber) data");
#endif
#if DEBUG
            Console.WriteLine($"It took {stopwatch.Elapsed} to get Medb (Saber) data (Is cached: {Person6.FromCache})");
#endif
            stopwatch.Reset();
            Console.WriteLine("----------------------------");
            Console.WriteLine("Getting Jack the Ripper data");
            stopwatch.Start();
            var person = FateGrandOrderParsing.GetPerson("Jack_the_Ripper");
            stopwatch.Stop();
#if !DEBUG
            Console.WriteLine($"It took {stopwatch.Elapsed} to get Jack the Ripper data");
#endif
#if DEBUG
            Console.WriteLine($"It took {stopwatch.Elapsed} to get Jack the Ripper data (Is cached: {person.FromCache})");
#endif
            Console.WriteLine("----------------------------------");
            StringBuilder Servant = new StringBuilder();
            Servant.AppendLine($"Name: {person.BasicInfomation.EnglishName}");
            Servant.AppendLine($"Jap name: {person.BasicInfomation.JapaneseName}");
            Servant.AppendLine($"Gender: {person.BasicInfomation.Gender}");
            Servant.AppendLine($"ATK: {person.BasicInfomation.ATK}");
            Servant.AppendLine($"Class: {person.BasicInfomation.Class}");
            Servant.AppendLine($"Cost: {person.BasicInfomation.Cost}");
            Console.Write(Servant);
            Console.ReadKey();
        }
    }
    static class People
    {
        static FateGrandOrderApi.Classes.FateGrandOrderPerson JackTheRipper = new FateGrandOrderApi.Classes.FateGrandOrderPerson
        {
            BasicInfomation = new FateGrandOrderApi.Classes.FateGrandOrderPersonBasic
            {
                //Cost = 
            }
        };
    }
}
