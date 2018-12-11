using System;
using FateGrandOrderApi;

namespace ApiTest
{
    class Program
    {
        static void Main(string[] args)
        {
            var Person1 = FateGrandOrderParsing.GetPerson("Jeanne d'Arc (Alter)");
            Console.WriteLine("Hello World!");
        }
    }
}
