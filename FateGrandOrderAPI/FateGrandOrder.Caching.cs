using System;
using System.IO;
using Newtonsoft.Json;
using System.Threading.Tasks;
using FateGrandOrderApi.Classes;
using FateGrandOrderApi.Logging;
using System.Collections.Generic;

namespace FateGrandOrderApi.Caching
{
    /// <summary>
    /// Everything that has been setup for being cached
    /// </summary>
    internal static class FateGrandOrderApiCache
    {
        internal static string CacheLocation = Path.Combine(Settings.Cache.UserFilesLocation, "FGOCache");

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
        //PassiveSkills ToAdd
        //Images ToAdd

        public static async Task SaveCache(dynamic CacheToSave)
        {
            if (!Settings.Cache.SaveCachedPartsToDisk)
                return;

            try
            {
                if (!Directory.Exists(CacheLocation))
                    Directory.CreateDirectory(CacheLocation);

                if (Settings.Cache.CacheServants && CacheToSave is List<Servant>)
                    File.WriteAllText(Path.Combine(CacheLocation, "Servants.json"), JsonConvert.SerializeObject(CacheToSave, Settings.Cache.JsonSerializerSettings));
                else if (Settings.Cache.CacheItems && CacheToSave is List<Item>)
                    File.WriteAllText(Path.Combine(CacheLocation, "Items.json"), JsonConvert.SerializeObject(CacheToSave, Settings.Cache.JsonSerializerSettings));
                else if (Settings.Cache.CacheEnemies && CacheToSave is List<Enemy>)
                    File.WriteAllText(Path.Combine(CacheLocation, "Enemies.json"), JsonConvert.SerializeObject(CacheToSave, Settings.Cache.JsonSerializerSettings));
                else if (Settings.Cache.CacheActiveSkills && CacheToSave is List<ActiveSkill>)
                    File.WriteAllText(Path.Combine(CacheLocation, "ActiveSkills.json"), JsonConvert.SerializeObject(CacheToSave, Settings.Cache.JsonSerializerSettings));
                else if (Settings.Cache.CacheSkills && CacheToSave is List<Skill>)
                    File.WriteAllText(Path.Combine(CacheLocation, "Skills.json"), JsonConvert.SerializeObject(CacheToSave, Settings.Cache.JsonSerializerSettings));
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
