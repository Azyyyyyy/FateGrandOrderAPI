using System;
using FateGrandOrderApi;
using System.Diagnostics;

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
            Console.ReadKey();
        }
    }
}
