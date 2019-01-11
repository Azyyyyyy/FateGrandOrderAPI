using System;
using System.IO;
using Newtonsoft.Json;
using FateGrandOrderApi.Logging;

namespace FateGrandOrderApi.Settings
{
    /// <summary>
    /// Cache Settings (What to cache)
    /// </summary>
    public static class Cache
    {
        static Cache()
        {
            if (!Directory.Exists(Path.Combine(UserFilesLocation, "FGOSettings")))
            {
                try { Directory.CreateDirectory(Path.Combine(UserFilesLocation, "FGOSettings")); return; }
                catch (Exception e)
                {
                    Logger.LogConsole(e, $"Looks like something failed when making the cache settings folder");
                    Logger.LogFile(e, $"Looks like something failed when making the cache settings folder");
                }
            }
            try
            {
                if (Directory.Exists(Path.Combine(UserFilesLocation, "FGOSettings")))
                {
                    if (File.Exists(Path.Combine(UserFilesLocation, "FGOSettings", "Cache.People.json"))) { cacheServants = JsonConvert.DeserializeObject<bool>(File.ReadAllText(Path.Combine(UserFilesLocation, "FGOSettings", "Cache.People.json"))); }
                    if (File.Exists(Path.Combine(UserFilesLocation, "FGOSettings", "Cache.Items.json"))) { cacheItems = JsonConvert.DeserializeObject<bool>(File.ReadAllText(Path.Combine(UserFilesLocation, "FGOSettings", "Cache.Items.json"))); }
                    if (File.Exists(Path.Combine(UserFilesLocation, "FGOSettings", "Cache.Enemies.json"))) { cacheEnemies = JsonConvert.DeserializeObject<bool>(File.ReadAllText(Path.Combine(UserFilesLocation, "FGOSettings", "Cache.Enemies.json"))); }
                    if (File.Exists(Path.Combine(UserFilesLocation, "FGOSettings", "Cache.Active Skills.json"))) { cacheActiveSkills = JsonConvert.DeserializeObject<bool>(File.ReadAllText(Path.Combine(UserFilesLocation, "FGOSettings", "Cache.Active Skills.json"))); }
                    if (File.Exists(Path.Combine(UserFilesLocation, "FGOSettings", "Cache.Skills.json"))) { cacheSkills = JsonConvert.DeserializeObject<bool>(File.ReadAllText(Path.Combine(UserFilesLocation, "FGOSettings", "Cache.Skills.json"))); }
                    if (File.Exists(Path.Combine(UserFilesLocation, "FGOSettings", "Cache.Images.json"))) { cacheImages = JsonConvert.DeserializeObject<bool>(File.ReadAllText(Path.Combine(UserFilesLocation, "FGOSettings", "Cache.Images.json"))); }
                    if (File.Exists(Path.Combine(UserFilesLocation, "FGOSettings", "Cache.ToDisk.json"))) { cacheToDisk = JsonConvert.DeserializeObject<bool>(File.ReadAllText(Path.Combine(UserFilesLocation, "FGOSettings", "Cache.ToDisk.json"))); }
                }
            }
            catch (Exception e)
            {
                Logger.LogConsole(e, $"Looks like something failed when assigning the cache settings");
                Logger.LogFile(e, $"Looks like something failed when assigning the cache settings");
            }
        }

        internal static readonly string UserFilesLocation = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "AzyIsCool","FGOApi");
        internal static readonly string GlobalFilesLocation = Path.Combine(Directory.GetCurrentDirectory(), "FGOApi");
        //Thanks to https://stackoverflow.com/questions/13510204/json-net-self-referencing-loop-detected
        internal static JsonSerializerSettings JsonSerializerSettings = new JsonSerializerSettings
        {
            PreserveReferencesHandling = PreserveReferencesHandling.Objects,
            ReferenceLoopHandling = ReferenceLoopHandling.Serialize
        };
        private static bool cacheServants = true;
        private static bool cacheItems = true;
        private static bool cacheEnemies = true;
        private static bool cacheActiveSkills = true;
        private static bool cacheSkills = true;
        private static bool cacheImages = true;
        private static bool cacheToDisk = false;

        /// <summary>
        /// If we should cache servants
        /// </summary>
        public static bool CacheServants { get { return cacheServants; } set { cacheServants = value; File.WriteAllText(Path.Combine(UserFilesLocation, "FGOSettings", "Cache.People.json"), JsonConvert.SerializeObject(value)); } }
        /// <summary>
        /// If we should cache items
        /// </summary>
        public static bool CacheItems { get { return cacheItems; } set { cacheItems = value; File.WriteAllText(Path.Combine(UserFilesLocation, "FGOSettings", "Cache.Items.json"), JsonConvert.SerializeObject(value)); } }
        /// <summary>
        /// If we should cache enemies
        /// </summary>
        public static bool CacheEnemies { get { return cacheEnemies; } set { cacheEnemies = value; File.WriteAllText(Path.Combine(UserFilesLocation, "FGOSettings", "Cache.Enemies.json"), JsonConvert.SerializeObject(value)); } }
        /// <summary>
        /// If we should cache active skills
        /// </summary>
        public static bool CacheActiveSkills { get { return cacheActiveSkills; } set { cacheActiveSkills = value; File.WriteAllText(Path.Combine(UserFilesLocation, "FGOSettings", "Cache.Active Skills.json"), JsonConvert.SerializeObject(value)); } }
        /// <summary>
        /// If we should cache skills
        /// </summary>
        public static bool CacheSkills { get { return cacheSkills; } set { cacheSkills = value; File.WriteAllText(Path.Combine(UserFilesLocation, "FGOSettings", "Cache.Skills.json"), JsonConvert.SerializeObject(value)); } }
        /// <summary>
        /// If we should cache images
        /// </summary>
        public static bool CacheImages { get { return cacheImages; } set { cacheImages = value; File.WriteAllText(Path.Combine(UserFilesLocation, "FGOSettings", "Cache.Images.json"), JsonConvert.SerializeObject(value)); } }
        /// <summary>
        /// If we should save cached content to the user's disk 
        /// </summary>
        public static bool SaveCachedPartsToDisk { get { return cacheToDisk; } set { cacheToDisk = value; File.WriteAllText(Path.Combine(UserFilesLocation, "FGOSettings", "Cache.ToDisk.json"), JsonConvert.SerializeObject(value)); } }
    }
}
