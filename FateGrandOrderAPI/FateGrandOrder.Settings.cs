using System;
using System.IO;
using FateGrandOrderApi.Logging;
using Newtonsoft.Json;

namespace FateGrandOrderApi.Settings
{
    /// <summary>
    /// Cache Settings (What to cache)
    /// </summary>
    public static class Cache
    {
        static Cache()
        {
            if (!Directory.Exists("FGOSettings"))
            {
                try
                {
                    Directory.CreateDirectory("FGOSettings");
                    if (File.Exists(Path.Combine("FGOSettings", "Cache.People.json"))) { cacheFateGrandOrderPeople = JsonConvert.DeserializeObject<bool>(File.ReadAllText(Path.Combine("FGOSettings", "Cache.People.json"))); }
                    if (File.Exists(Path.Combine("FGOSettings", "Cache.Items.json"))) { cacheItems = JsonConvert.DeserializeObject<bool>(File.ReadAllText(Path.Combine("FGOSettings", "Cache.Items.json"))); }
                    if (File.Exists(Path.Combine("FGOSettings", "Cache.Enemies.json"))) { cacheEnemies = JsonConvert.DeserializeObject<bool>(File.ReadAllText(Path.Combine("FGOSettings", "Cache.Enemies.json"))); }
                    if (File.Exists(Path.Combine("FGOSettings", "Cache.ActiveSkills.json"))) { cacheActiveSkills = JsonConvert.DeserializeObject<bool>(File.ReadAllText(Path.Combine("FGOSettings", "Cache.ActiveSkills.json"))); }
                    if (File.Exists(Path.Combine("FGOSettings", "Cache.Skills.json"))) { cacheSkills = JsonConvert.DeserializeObject<bool>(File.ReadAllText(Path.Combine("FGOSettings", "Cache.Skills.json"))); }
                }
                catch (Exception e)
                {
                    Logger.LogConsole(e, $"Looks like something failed when assigning the cache settings");
                    Logger.LogFile(e, $"Looks like something failed when assigning the cache settings");
                }
            }
        }

        private static bool cacheFateGrandOrderPeople = true;
        private static bool cacheItems = true;
        private static bool cacheEnemies = true;
        private static bool cacheActiveSkills = true;
        private static bool cacheSkills = true;

        /// <summary>
        /// If to cache servants
        /// </summary>
        public static bool CacheFateGrandOrderPeople { get { return cacheFateGrandOrderPeople; } set { cacheFateGrandOrderPeople = value; File.WriteAllText(Path.Combine("FGOSettings", "Cache.People.json"), JsonConvert.SerializeObject(value)); } }
        /// <summary>
        /// If to cache items
        /// </summary>
        public static bool CacheItems { get { return cacheItems; } set { cacheItems = value; File.WriteAllText(Path.Combine("FGOSettings", "Cache.Items.json"), JsonConvert.SerializeObject(value)); } }
        /// <summary>
        /// If to cache enemies
        /// </summary>
        public static bool CacheEnemies { get { return cacheEnemies; } set { cacheEnemies = value; File.WriteAllText(Path.Combine("FGOSettings", "Cache.Enemies.json"), JsonConvert.SerializeObject(value)); } }
        /// <summary>
        /// If to cache active skills
        /// </summary>
        public static bool CacheActiveSkills { get { return cacheActiveSkills; } set { cacheActiveSkills = value; File.WriteAllText(Path.Combine("FGOSettings", "Cache.ActiveSkills.json"), JsonConvert.SerializeObject(value)); } }
        /// <summary>
        /// If to cache skills
        /// </summary>
        public static bool CacheSkills { get { return cacheSkills; } set { cacheSkills = value; File.WriteAllText(Path.Combine("FGOSettings", "Cache.Skills.json"), JsonConvert.SerializeObject(value)); } }
    }
}
