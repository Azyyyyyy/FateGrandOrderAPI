using System;
using System.Collections.Generic;
using FateGrandOrderApi;
using System.Diagnostics;
using System.Text;
using FateGrandOrderApi.Classes;

namespace ApiTest
{
    class Program
    {
        public static List<FateGrandOrderPerson> ServantsParsed = new List<FateGrandOrderPerson>();

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
                var persondata = FateGrandOrderParsing.GetPerson(servant, presetsForInformation: PresetsForInformation.BasicInformation, GetImages: true).ConfigureAwait(true).GetAwaiter().GetResult();
                stopwatch.Stop();
#if !DEBUG
                Console.WriteLine($"It took {stopwatch.Elapsed} to get {person} data");
#endif
#if DEBUG
                Console.WriteLine($"It took {stopwatch.Elapsed} to get {servant} data (Is cached: {persondata.FromCache})");
#endif
                Console.WriteLine(Line($"It took {stopwatch.Elapsed} to get {servant} data (Is cached: {persondata.FromCache})"));
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
            Console.WriteLine(Line($"Has cache cached all servants: {EverythingCached()}"));
            Console.WriteLine($"Has cache cached all servants: {EverythingCached()}");
            Console.ReadLine();
        }

        public static bool EverythingCached()
        {
            int Count = 0;
            while(Count + 1 != FateGrandOrderApi.Caching.FateGrandOrderPersonCache.FateGrandOrderPeople.Count)
            {
                if (FateGrandOrderApi.Caching.FateGrandOrderPersonCache.FateGrandOrderPeople[Count] != ServantsParsed[Count])
                    return false;
                Count++;
            }
            return true;
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

        //static string[] Servants = { "Jeanne d'Arc (Alter)", "Lancelot (Saber)", "Sigurd", "Artoria Pendragon (Alter)", "Medb (Saber)", "Diarmuid Ua Duibhne (Saber)", "Jack the Ripper", "Helena Blavatsky" };
        static string[] Servants = { "Jack the Ripper" };
    }
}
