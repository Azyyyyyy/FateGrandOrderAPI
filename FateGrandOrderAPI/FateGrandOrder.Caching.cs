using System;
using System.IO;
using Newtonsoft.Json;
using System.Threading.Tasks;
using FateGrandOrderApi.Classes;
using FateGrandOrderApi.Logging;
using System.Collections.Generic;
using FateGrandOrderApi.Settings;

namespace FateGrandOrderApi.Caching
{
    /// <summary>
    /// Everything that has been setup for being cached
    /// </summary>
    internal static class FateGrandOrderApiCache
    {
        static FateGrandOrderApiCache()
        {
            if (!Cache.SaveCachedPartsToDisk)
            goto End;

            try
            {
                if (!Directory.Exists(CacheLocation))
                    goto End;

                if (Cache.CacheServants && File.Exists(Path.Combine(CacheLocation, "Servants.json")))
                    Servants = JsonConvert.DeserializeObject<List<Servant>>(File.ReadAllText(Path.Combine(CacheLocation, "Servants.json")));
                if (Cache.CacheItems && File.Exists(Path.Combine(CacheLocation, "Items.json")))
                    Items = JsonConvert.DeserializeObject<List<Item>>(File.ReadAllText(Path.Combine(CacheLocation, "Items.json")));
                if (Cache.CacheEnemies && File.Exists(Path.Combine(CacheLocation, "Enemies.json")))
                    Enemies = JsonConvert.DeserializeObject<List<Enemy>>(File.ReadAllText(Path.Combine(CacheLocation, "Enemies.json")));
                if (Cache.CacheActiveSkills && File.Exists(Path.Combine(CacheLocation, "Active Skills.json")))
                    ActiveSkills = JsonConvert.DeserializeObject<List<ActiveSkill>>(File.ReadAllText(Path.Combine(CacheLocation, "Active Skills.json")));
                if (Cache.CacheSkills && File.Exists(Path.Combine(CacheLocation, "Skills.json")))
                    Skills = JsonConvert.DeserializeObject<List<Skill>>(File.ReadAllText(Path.Combine(CacheLocation, "Skills.json")));
                if (Cache.CacheImages && File.Exists(Path.Combine(CacheLocation, "Images.json")))
                    Images = JsonConvert.DeserializeObject<List<ImageInformation>>(File.ReadAllText(Path.Combine(CacheLocation, "Images.json")));
            }
            catch (Exception e)
            {
                Logger.LogConsole(e, "Looks like something happened when accessing/editing the cache");
                Logger.LogFile(e, "Looks like something happened when accessing/editing the cache");
            }

            End:
            if (Skills == null && Cache.CacheSkills)
                Skills = new List<Skill>();
            if (ActiveSkills == null && Cache.CacheActiveSkills)
                ActiveSkills = new List<ActiveSkill>();
            if (Items == null && Cache.CacheItems)
                Items = new List<Item>();
            if (Enemies == null && Cache.CacheEnemies)
                Enemies = new List<Enemy>();
            if (Servants == null && Cache.CacheServants)
                Servants = new List<Servant>();
            if (Images == null && Cache.CacheImages)
                Images = new List<ImageInformation>();

            CacheTimer();
        }
        
        internal static string CacheLocation = Path.Combine(Cache.GlobalFilesLocation, "FGOCache");

        /// <summary>
        /// Servants that are currently in cache
        /// </summary>
        public static List<Servant> Servants { get; set; }
        /// <summary>
        /// Items that are currently in cache
        /// </summary>
        public static List<Item> Items { get; set; }
        /// <summary>
        /// Enemies that are currently in cache
        /// </summary>
        public static List<Enemy> Enemies { get; set; }
        /// <summary>
        /// Active Skills that are currently in cache
        /// </summary>
        public static List<ActiveSkill> ActiveSkills { get; set; }
        /// <summary>
        /// Skills that are currently in cache
        /// </summary>
        public static List<Skill> Skills { get; set; }
        /// <summary>
        /// Images that are currently in cache
        /// </summary>
        public static List<ImageInformation> Images { get; set; }

        /// <summary>
        /// This will make sure all the cache is up to date, updating any that isn't
        /// </summary>
        /// <returns></returns>
        public static async Task UpdateCache()
        {
            if (Servants.Count != 0)
            {
                for (int i = 0; i < Servants.Count; i++)
                {
                    await FateGrandOrderParsing.GetServant(Servants[i].EnglishNamePassed);
                }
                Logger.LogConsole(null, "Updated Servant cache");
            }
            if (Items.Count != 0)
            {
                for (int i = 0; i < Items.Count; i++)
                {
                    await FateGrandOrderParsing.GetItem(Items[i].EnglishName);
                }
                Logger.LogConsole(null, "Updated Item cache");
            }
            if (Enemies.Count != 0)
            {
                for (int i = 0; i < Enemies.Count; i++)
                {
                    await FateGrandOrderParsing.GetEnemy(Enemies[i].EnglishName);
                }
                Logger.LogConsole(null, "Updated Enemy cache");
            }
            if (ActiveSkills.Count != 0)
            {
                for (int i = 0; i < ActiveSkills.Count; i++)
                {
                    await FateGrandOrderParsing.GetActiveSkill(ActiveSkills[i].Name);
                }
                Logger.LogConsole(null, "Updated Active Skill cache");
            }
            if (Skills.Count != 0)
            {
                for (int i = 0; i < Skills.Count; i++)
                {
                    await FateGrandOrderParsing.GetSkill(Skills[i].Name);
                }
                Logger.LogConsole(null, "Updated Skill cache");
            }
            if (Images.Count != 0)
            {
                for (int i = 0; i < Images.Count; i++)
                {
                    await FateGrandOrderParsing.AssigningContent.Image(Images[i].GeneratedWith);
                }
                Logger.LogConsole(null, "Updated Image cache");
            }
        }

        /// <summary>
        /// Updates the Cache after a set amount of time
        /// </summary>
        /// <returns></returns>
        private static async Task CacheTimer()
        {
            while (true)
            {
                if (Cache.CheckCacheTime.TotalMilliseconds != 0)
                {
                    DateTime cacheTime = DateTime.Now;
                    while (Cache.CheckCacheTime.TotalMilliseconds > DateTime.Now.Subtract(cacheTime).TotalMilliseconds)
                    {
                        await Task.Delay(1000);
                    }
                    await UpdateCache();
                }
                else
                {
                    await Task.Delay(1000);
                }
            }
        }
        
        /// <summary>
        /// Saves Cache to Disk
        /// </summary>
        /// <param name="CacheToSave">Cache to save</param>
        /// <returns></returns>
        internal static async Task SaveCache(dynamic CacheToSave)
        {
            if (!Cache.SaveCachedPartsToDisk)
                return;

            try
            {
                if (!Directory.Exists(CacheLocation))
                    Directory.CreateDirectory(CacheLocation);

                if (Cache.CacheServants && CacheToSave is List<Servant>)
                    File.WriteAllText(Path.Combine(CacheLocation, "Servants.json"), JsonConvert.SerializeObject(CacheToSave, Cache.JsonSerializerSettings));
                else if (Cache.CacheItems && CacheToSave is List<Item>)
                    File.WriteAllText(Path.Combine(CacheLocation, "Items.json"), JsonConvert.SerializeObject(CacheToSave, Cache.JsonSerializerSettings));
                else if (Cache.CacheEnemies && CacheToSave is List<Enemy>)
                    File.WriteAllText(Path.Combine(CacheLocation, "Enemies.json"), JsonConvert.SerializeObject(CacheToSave, Cache.JsonSerializerSettings));
                else if (Cache.CacheActiveSkills && CacheToSave is List<ActiveSkill>)
                    File.WriteAllText(Path.Combine(CacheLocation, "Active Skills.json"), JsonConvert.SerializeObject(CacheToSave, Cache.JsonSerializerSettings));
                else if (Cache.CacheSkills && CacheToSave is List<Skill>)
                    File.WriteAllText(Path.Combine(CacheLocation, "Skills.json"), JsonConvert.SerializeObject(CacheToSave, Cache.JsonSerializerSettings));
                else if (Cache.CacheImages && CacheToSave is List<ImageInformation>)
                    File.WriteAllText(Path.Combine(CacheLocation, "Images.json"), JsonConvert.SerializeObject(CacheToSave, Cache.JsonSerializerSettings));
                else
                    new Exception("Unable to find what type of cache this is ;-;");
            }
            catch (Exception e)
            {
                Logger.LogConsole(e, "Looks like something happened when accessing/editing the cache", $"Cache: {CacheToSave}");
                Logger.LogFile(e, "Looks like something happened when accessing/editing the cache", $"Cache: {CacheToSave}");
            }
        }
    }
}
