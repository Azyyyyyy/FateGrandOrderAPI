using System;
using System.Text;
using FateGrandOrderApi;
using System.Diagnostics;
using FateGrandOrderApi.Classes;
using System.Collections.Generic;

namespace ApiTest
{
    class Program
    {
        public static List<Servant> ServantsParsed = new List<Servant>();

        static void Main(string[] args)
        {
            Console.WriteLine("Api Tester - AzyIsCool");
            Stopwatch stopwatch = new Stopwatch();
            foreach (var servant in Servants)
            {
                stopwatch.Reset();
                Console.WriteLine(Line($"Getting {servant} data"));
                Console.WriteLine($"Getting {servant} data");
                stopwatch.Start();
                var persondata = FateGrandOrderParsing.GetPerson(servant, GetImages: true).ConfigureAwait(true).GetAwaiter().GetResult();
                stopwatch.Stop();
#if !DEBUG
                Console.WriteLine($"It took {stopwatch.Elapsed} to get {servant} data");
#endif
#if DEBUG
                Console.WriteLine($"It took {stopwatch.Elapsed} to get {servant} data (Is cached: {persondata.FromCache})");
#endif
#if DEBUG
                Console.WriteLine(Line($"It took {stopwatch.Elapsed} to get {servant} data (Is cached: {persondata.FromCache})"));
#endif
#if !DEBUG
                Console.WriteLine(Line($"It took {stopwatch.Elapsed} to get {servant} data"));
#endif
                StringBuilder servantInfo = new StringBuilder();
                servantInfo.AppendLine($"Name: {persondata.BasicInformation.EnglishName}");
                servantInfo.AppendLine($"Jap name: {persondata.BasicInformation.JapaneseName}");
                servantInfo.AppendLine($"Gender: {persondata.BasicInformation.Gender}");
                servantInfo.AppendLine($"ATK: {persondata.BasicInformation.ATK}");
                servantInfo.AppendLine($"Class: {persondata.BasicInformation.Class}");
                servantInfo.AppendLine($"Cost: {persondata.BasicInformation.Cost}");
                Console.Write(servantInfo);
                ServantsParsed.Add(persondata);
            }
            Console.ReadLine();
        }

        static string Line(string person)
        {
            StringBuilder b = new StringBuilder();
            foreach (var charc in person)
            {
                b.Append('-');
            }
            return b.ToString();
        }

        //static readonly string[] Servants = { "Jeanne d'Arc (Alter)", "Lancelot (Saber)", "Sigurd", "Artoria Pendragon (Alter)", "Medb (Saber)", "Diarmuid Ua Duibhne (Saber)", "Jack the Ripper", "Helena Blavatsky" };
        static readonly string[] Servants = { "Jack the Ripper" }; //Check NoblePhantasms
    }
}
