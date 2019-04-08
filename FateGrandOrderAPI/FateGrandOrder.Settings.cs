using System;
using System.IO;
using Newtonsoft.Json;
using FateGrandOrderApi.Logging;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("ApiTest")]
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
                    if (File.Exists(Path.Combine(UserFilesLocation, "FGOSettings", "Cache.People.json"))) { _cacheServants = JsonConvert.DeserializeObject<bool>(File.ReadAllText(Path.Combine(UserFilesLocation, "FGOSettings", "Cache.People.json"))); }
                    if (File.Exists(Path.Combine(UserFilesLocation, "FGOSettings", "Cache.Items.json"))) { _cacheItems = JsonConvert.DeserializeObject<bool>(File.ReadAllText(Path.Combine(UserFilesLocation, "FGOSettings", "Cache.Items.json"))); }
                    if (File.Exists(Path.Combine(UserFilesLocation, "FGOSettings", "Cache.Enemies.json"))) { _cacheEnemies = JsonConvert.DeserializeObject<bool>(File.ReadAllText(Path.Combine(UserFilesLocation, "FGOSettings", "Cache.Enemies.json"))); }
                    if (File.Exists(Path.Combine(UserFilesLocation, "FGOSettings", "Cache.Active Skills.json"))) { _cacheActiveSkills = JsonConvert.DeserializeObject<bool>(File.ReadAllText(Path.Combine(UserFilesLocation, "FGOSettings", "Cache.Active Skills.json"))); }
                    if (File.Exists(Path.Combine(UserFilesLocation, "FGOSettings", "Cache.Skills.json"))) { _cacheSkills = JsonConvert.DeserializeObject<bool>(File.ReadAllText(Path.Combine(UserFilesLocation, "FGOSettings", "Cache.Skills.json"))); }
                    if (File.Exists(Path.Combine(UserFilesLocation, "FGOSettings", "Cache.Images.json"))) { _cacheImages = JsonConvert.DeserializeObject<bool>(File.ReadAllText(Path.Combine(UserFilesLocation, "FGOSettings", "Cache.Images.json"))); }
                    if (File.Exists(Path.Combine(UserFilesLocation, "FGOSettings", "Cache.ToDisk.json"))) { _cacheToDisk = JsonConvert.DeserializeObject<bool>(File.ReadAllText(Path.Combine(UserFilesLocation, "FGOSettings", "Cache.ToDisk.json"))); }
                    if (File.Exists(Path.Combine(UserFilesLocation, "FGOSettings", "Cache.CheckCacheTime.json"))) { _checkCacheTime = JsonConvert.DeserializeObject<TimeSpan>(File.ReadAllText(Path.Combine(UserFilesLocation, "FGOSettings", "Cache.CheckCacheTime.json"))); }
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
        internal static readonly JsonSerializerSettings JsonSerializerSettings = new JsonSerializerSettings
        {
            PreserveReferencesHandling = PreserveReferencesHandling.Objects,
            ReferenceLoopHandling = ReferenceLoopHandling.Serialize
        };
        private static bool _cacheServants = true;
        private static bool _cacheItems = true;
        private static bool _cacheEnemies = true;
        private static bool _cacheActiveSkills = true;
        private static bool _cacheSkills = true;
        private static bool _cacheImages = true;
        private static bool _cacheToDisk;
        private static TimeSpan _checkCacheTime = TimeSpan.FromSeconds(60);

        /// <summary>
        /// If we should cache servants
        /// </summary>
        public static bool CacheServants { get => _cacheServants; set { _cacheServants = value; File.WriteAllText(Path.Combine(UserFilesLocation, "FGOSettings", "Cache.People.json"), JsonConvert.SerializeObject(value)); } }
        /// <summary>
        /// If we should cache items
        /// </summary>
        public static bool CacheItems { get => _cacheItems; set { _cacheItems = value; File.WriteAllText(Path.Combine(UserFilesLocation, "FGOSettings", "Cache.Items.json"), JsonConvert.SerializeObject(value)); } }
        /// <summary>
        /// If we should cache enemies
        /// </summary>
        public static bool CacheEnemies { get => _cacheEnemies; set { _cacheEnemies = value; File.WriteAllText(Path.Combine(UserFilesLocation, "FGOSettings", "Cache.Enemies.json"), JsonConvert.SerializeObject(value)); } }
        /// <summary>
        /// If we should cache active skills
        /// </summary>
        public static bool CacheActiveSkills { get => _cacheActiveSkills; set { _cacheActiveSkills = value; File.WriteAllText(Path.Combine(UserFilesLocation, "FGOSettings", "Cache.Active Skills.json"), JsonConvert.SerializeObject(value)); } }
        /// <summary>
        /// If we should cache skills
        /// </summary>
        public static bool CacheSkills { get => _cacheSkills; set { _cacheSkills = value; File.WriteAllText(Path.Combine(UserFilesLocation, "FGOSettings", "Cache.Skills.json"), JsonConvert.SerializeObject(value)); } }
        /// <summary>
        /// If we should cache images
        /// </summary>
        public static bool CacheImages { get => _cacheImages; set { _cacheImages = value; File.WriteAllText(Path.Combine(UserFilesLocation, "FGOSettings", "Cache.Images.json"), JsonConvert.SerializeObject(value)); } }
        /// <summary>
        /// If we should save cached content to the user's disk 
        /// </summary>
        public static bool SaveCachedPartsToDisk { get => _cacheToDisk; set { _cacheToDisk = value; File.WriteAllText(Path.Combine(UserFilesLocation, "FGOSettings", "Cache.ToDisk.json"), JsonConvert.SerializeObject(value)); } }
        /// <summary>
        /// How long until the api will check the cache to make sure it's all up to date
        /// </summary>
        public static TimeSpan CheckCacheTime { get => _checkCacheTime; set { _checkCacheTime = value; File.WriteAllText(Path.Combine(UserFilesLocation, "FGOSettings", "Cache.CheckCacheTime.json"), JsonConvert.SerializeObject(value)); } }
    }
}
