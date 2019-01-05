using FateGrandOrderApi.Classes;
using System.Collections.Generic;

namespace FateGrandOrderApi.Caching
{
    /// <summary>
    /// Everything that has been setup for caching
    /// </summary>
    internal static class FateGrandOrderPersonCache
    {
        /// <summary>
        /// People that are currently in the cache
        /// </summary>
        public static List<FateGrandOrderPerson> FateGrandOrderPeople { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public static List<Item> Items { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public static List<Enemy> Enemies { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public static List<ActiveSkill> ActiveSkills { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public static List<Skill> Skills { get; set; }
    }
}
