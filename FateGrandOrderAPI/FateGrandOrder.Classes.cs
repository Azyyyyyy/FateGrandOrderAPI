using System.Collections.Generic;

namespace FateGrandOrderApi.Classes
{

    #region Servants and Enemy
    /// <summary>
    /// A Fate/Grand Order Servant
    /// </summary>
    public class Servant
    {
        public Servant(string generatedWith, string englishNamePassed) { GeneratedWith = generatedWith; EnglishNamePassed = englishNamePassed; }
        /// <summary>
        /// String that was used when generating this servant class, used for checking if in cache and if cache version is up to date
        /// </summary>
        internal string GeneratedWith { get; private set; }
        /// <summary>
        /// English name that was passed when generating this, used for checking if in cache
        /// </summary>
        public string EnglishNamePassed { get; private set; }
        /// <summary>
        /// Basic information about this servant (.e.g. Name, HP, ATK etc...)
        /// </summary>
        public FateGrandOrderServantBasic BasicInformation { get; set; }
        /// <summary>
        /// Every Active Skills this servant has
        /// </summary>
        public List<ActiveSkill> ActiveSkills { get; set; }
        /// <summary>
        /// Every Passive Skills this servant has
        /// </summary>
        public List<PassiveSkillList> PassiveSkills { get; set; }
        /// <summary>
        /// Every Noble Phantasm this servant has
        /// </summary>
        public List<NoblePhantasmList> NoblePhantasms { get; set; }
        /// <summary>
        /// Every Ascension this servant has
        /// </summary>
        public Ascension Ascension { get; set; }
        /// <summary>
        /// Every Skill Reinforcement this servant has
        /// </summary>
        public SkillReinforcement SkillReinforcement { get; set; }
        /// <summary>
        /// What stats this servant has
        /// </summary>
        public Stats Stats { get; set; }
        /// <summary>
        /// What Bond Levels this servant has
        /// </summary>
        public BondLevels BondLevels { get; set; }
        /// <summary>
        /// The servant's Biography
        /// </summary>
        public Biography Biography { get; set; }
        /// <summary>
        /// When this servant has been available
        /// </summary>
        public string[] Availability { get; set; }
        /// <summary>
        /// Extra infomation about this servant
        /// </summary>
        public string[] Trivia { get; set; }
        /// <summary>
        /// Images of/with this servant
        /// </summary>
        public List<ImageInformation> Images { get; set; }
#if DEBUG
        /// <summary>
        /// If the servant that was returned from cache
        /// </summary>
        public bool FromCache { get; set; }
#endif
    }

    /// <summary>
    /// Basic information about a Fate/Grand Order servant
    /// </summary>
    public class FateGrandOrderServantBasic
    {
        public FateGrandOrderServantBasic() { EnglishName = ""; }

        public FateGrandOrderServantBasic(string englishName) { EnglishName = englishName; }

        /// <summary>
        /// Gives you the English name of the servant (gets assigned when class is made)
        /// </summary>
        public string EnglishName { get; private set; }
        /// <summary>
        /// Gives you the Japanese name of the servant (jname)
        /// </summary>
        public string JapaneseName { get; set; }
        /// <summary>
        /// Gives you the name of the servants voice actor (voicea)
        /// </summary>
        public string VoiceActor { get; set; }
        /// <summary>
        /// Gives you the name of the illustrator (illus)
        /// </summary>
        public string Illustrator { get; set; }
        /// <summary>
        /// Gives you the servants class (class)
        /// </summary>
        public string Class { get; set; }
        /// <summary>
        /// Gives both the basic and max level ATK of the servant (atk)
        /// </summary>
        public string ATK { get; set; }
        /// <summary>
        /// Gives both the basic and max level HP of the servant (hp)
        /// </summary>
        public string HP { get; set; }
        /// <summary>
        /// Gives you the max ATK after max ascension and after it's reached max level with grails (gatk)
        /// </summary>
        public string GrailATK { get; set; }
        /// <summary>
        /// Gives you the max HP amount after max ascension and after it's reached max level with grails (ghp)
        /// </summary>
        public string GrailHP { get; set; }
        /// <summary>
        /// What tier the servant is (stars)
        /// </summary>
        public string Stars { get; set; }
        /// <summary>
        /// Shows you the cost of the servant (cost)
        /// </summary>
        public string Cost { get; set; }
        /// <summary>
        /// Command Code (cc)
        /// </summary>
        public string CommandCode { get; set; }
        /// <summary>
        /// Gives you the servant max level with ascension (mlevel)
        /// </summary>
        public string MaxLevel { get; set; }
        /// <summary>
        /// Returns the servants ID (id)
        /// </summary>
        public string ID { get; set; }
        /// <summary>
        /// What attribute the servant is, showing what they are weak and strong against (Man, Earth, Sky, Beast) (attribute)
        /// </summary>
        public string Attribute { get; set; }
        /// <summary>
        /// Quick Hits (qhits)
        /// </summary>
        public string QuickHits { get; set; }
        /// <summary>
        /// Arts Hits (ahits)
        /// </summary>
        public string ArtsHits { get; set; }
        /// <summary>
        /// Buster Hits (bhits)
        /// </summary>
        public string BusterHits { get; set; }
        /// <summary>
        /// Extra Hits (ehits)
        /// </summary>
        public string ExtraHits { get; set; }
        /// <summary>
        /// Chances of this servant being killed by the Instant-Kill effect (deathrate)
        /// </summary>
        public string DeathRate { get; set; }
        /// <summary>
        /// How many critical stars this servant gets when the stars are distributed (starabsorption)
        /// </summary>
        public string StarAbsorption { get; set; }
        /// <summary>
        /// Shows how many critical stars this servant generates when attacking (stargeneration)
        /// </summary>
        public string StarGeneration { get; set; }
        /// <summary>
        /// Shows how much the NP Gauge is increased by when attacking enemies (npchargeatk)
        /// </summary>
        public string NPChargeATK { get; set; }
        /// <summary>
        /// Shows how much the NP Gauge is increased by when being attacked (npchargedef)
        /// </summary>
        public string NPChargeDEF { get; set; }
        /// <summary>
        /// Shows the servants growth curve which relates to the stats gained per level (growthc)
        /// </summary>
        public string GrowthCurve { get; set; }
        /// <summary>
        /// Also known as. This will show what other names the servant is also known by (aka)
        /// </summary>
        public string[] AKA { get; set; }
        /// <summary>
        /// Shows you the trait(s) of the servant (traits)
        /// </summary>
        public string[] Traits { get; set; }
        /// <summary>
        /// Returns the servants gender (gender)
        /// </summary>
        public string Gender { get; set; }
        /// <summary>
        /// Returns the servants alignment (Lawful - Neutral - Chaotic | Good - Neutral - Evil) (alignment)
        /// </summary>
        public string Alignment { get; set; }
    }

    /// <summary>
    /// A Fate/Grand Order Enemy
    /// </summary>
    public class Enemy
    {
        public Enemy() { EnglishName = ""; }
        public Enemy(string englishName, string generatedWith) { EnglishName = englishName; GeneratedWith = generatedWith; }
        /// <summary>
        /// Images of/with the enemy (image)
        /// </summary>
        public List<ImageInformation> EnemyImage { get; set; }
        /// <summary>
        /// The Classes of the enemy (class)
        /// </summary>
        public string[] Class { get; set; }
        /// <summary>
        /// Where the enemy spawns (area)
        /// </summary>
        public string[] Areas { get; set; }
        /// <summary>
        /// What the name is in Japanese (jname)
        /// </summary>
        public string JapaneseName { get; set; }
        /// <summary>
        /// What the name is in English (gets passed to the class)
        /// </summary>
        public string EnglishName { get; set; }
        /// <summary>
        /// What the rank of the enemy is (rank)
        /// </summary>
        public string Rank { get; set; }
        /// <summary>
        /// What Gender the enemy is (gender)
        /// </summary>
        public string Gender { get; set; }
        /// <summary>
        /// What attribute the servant is, showing what they are weak and strong against (Man, Earth, Sky, Beast) (attribute)
        /// </summary>
        public string Attribute { get; set; }
        /// <summary>
        /// The enemy Traits (traits)
        /// </summary>
        public string[] Traits { get; set; }
        /// <summary>
        /// What Items this enemy drops (drop)
        /// </summary>
        public List<Item> WhatThisEnemyDrops { get; set; }
        /// <summary>
        /// What the wiki recommends to use to fight this enemy (== Recommended Servants ==)
        /// </summary>
        public List<Servant> RecommendedServants { get; set; }
        /// <summary>
        /// String that was used when generating this servant class, used for checking if in cache and if cache version is up to date
        /// </summary>
        internal string GeneratedWith { get; set; }
#if DEBUG
        /// <summary>
        /// If the enemy that was returned from cache
        /// </summary>
        public bool FromCache { get; set; }
#endif
    }
    #endregion

    #region Image and Video
    /// <summary>
    /// Contains a uri and name of a image
    /// </summary>
    public class ImageInformation
    {
        public ImageInformation(string generatedWith)
        {
            GeneratedWith = generatedWith;
        }
        public ImageInformation() { }
        /// <summary>
        /// Returns image name
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Returns image Uri
        /// </summary>
        public string Uri { get; set; }
        /// <summary>
        /// This is the image hash from MD5, used to make the uri link
        /// </summary>
        public string ImageHash { get; set; }
        /// <summary>
        /// Returns the file name of the image
        /// </summary>
        public string FileName { get; set; }
        /// <summary>
        /// String that was used when generating this servant class, used for checking if in cache and if cache version is up to date
        /// </summary>
        internal string GeneratedWith { get; set; }
#if DEBUG
        /// <summary>
        /// If the enemy that was returned from cache
        /// </summary>
        public bool FromCache { get; set; }
#endif
    }

    /// <summary>
    ///  Contains a uri and name of a video
    /// </summary>
    public class VideoInformation : ImageInformation
    {
        public VideoInformation(string generatedWith)
        {
            GeneratedWith = generatedWith;
        }
        public VideoInformation() { }
    }
    #endregion

    #region Ascension and Skill Reinforcement
    /// <summary>
    /// Container for Ascension/Skill Reinforcement
    /// </summary>
    public class AscensionSkillReinforcement
    {
        /// <summary>
        /// (passed on)
        /// </summary>
        public string AscensionNumber { get; set; }
        /// <summary>
        /// What the first item is in the skill reinforcement/ascension (1)
        /// </summary>
        public Item Item1 { get; set; }
        /// <summary>
        /// What the second item is in the skill reinforcement/ascension (2)
        /// </summary>
        public Item Item2 { get; set; }
        /// <summary>
        /// What the third item is in the skill reinforcement/ascension (3)
        /// </summary>
        public Item Item3 { get; set; }
        /// <summary>
        /// What the fourth item is in the skill reinforcement/ascension (4)
        /// </summary>
        public Item Item4 { get; set; }
        /// <summary>
        /// How much Quantum Piece you need to get the skill reinforcment/ascension (qp)
        /// </summary>
        public string QP { get; set; }
    }
    /// <summary>
    /// Ascension
    /// </summary>
    public class Ascension
    {
        /// <summary>
        /// First skill reinforcement/ascension
        /// </summary>
        public AscensionSkillReinforcement Ascension1 { get; set; }
        /// <summary>
        /// Second skill reinforcment/ascension
        /// </summary>
        public AscensionSkillReinforcement Ascension2 { get; set; }
        /// <summary>
        /// Third skill reinforcment/ascension
        /// </summary>
        public AscensionSkillReinforcement Ascension3 { get; set; }
        /// <summary>
        /// Fourth skill reinforcment/ascension
        /// </summary>
        public AscensionSkillReinforcement Ascension4 { get; set; }
        /// <summary>
        /// fifth skill reinforcment/ascension
        /// </summary>
        public AscensionSkillReinforcement Ascension5 { get; set; }
    }
    /// <summary>
    /// Skill Reinforcment
    /// </summary>
    public class SkillReinforcement : Ascension
    {
        /// <summary>
        /// Sixth skill reinforcment
        /// </summary>
        public AscensionSkillReinforcement Ascension6 { get; set; }
        /// <summary>
        /// Seventh skill reinforcment
        /// </summary>
        public AscensionSkillReinforcement Ascension7 { get; set; }
        /// <summary>
        /// Eighth skill reinforcment
        /// </summary>
        public AscensionSkillReinforcement Ascension8 { get; set; }
        /// <summary>
        /// Ninth skill reinforcment
        /// </summary>
        public AscensionSkillReinforcement Ascension9 { get; set; }
    }
    #endregion

    #region Skills
    /// <summary>
    /// The core part of makes a skill
    /// </summary>
    public class Skill
    {
        public Skill(string skillName, string generatedWith)
        {
            GeneratedWith = generatedWith;
            NamePassed = skillName;
        }
        public Skill() { }

        /// <summary>
        /// Returns the image information of this skill (img) 
        /// </summary>
        public ImageInformation Image { get; set; }
        /// <summary>
        /// Gives you skill's name (name)
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// English name that was passed when generating this, used for checking if in cache
        /// </summary>
        public string NamePassed { get; set; }
        /// <summary>
        /// Gives you what rank this skill is at (rank)
        /// </summary>
        public string Rank { get; set; }
        /// <summary>
        /// Gives you what effect's the skill does (effect) 
        /// </summary>
        public string[] Effect { get; set; }
        /// <summary>
        /// String that was passed when generating this, used for checking if in cache and if cache version is up to date
        /// </summary>
        internal string GeneratedWith { get; set; }
#if DEBUG
        /// <summary>
        /// Is returned from cache
        /// </summary>
        public bool FromCache { get; set; }
#endif
    }

    /// <summary>
    /// Active Skill
    /// </summary>
    public class ActiveSkill : Skill
    {
        public ActiveSkill(string skillName, string generatedWith)
        {
            GeneratedWith = generatedWith;
            NamePassed = skillName;
        }
        public ActiveSkill()
        {
        }

        /// <summary>
        /// Returns when the skill gets unlocked through Ascension ({{unlock|)
        /// </summary>
        public string WhenSkillUnlocks { get; set; }

        /// <summary>
        /// Gives you what servant's have this skill (servant)
        /// </summary>
        public List<Servant> ServantsThatHaveThisSkill { get; set; }
        /// <summary>
        /// Gives you all the cooldown and effects strength (leveleffect)
        /// </summary>
        public List<LevelEffect10> LevelEffects { get; set; }
        /// <summary>
        /// Returns if this ActiveSkill is for an NPC
        /// </summary>
        public bool ForNPC { get; set; }
    }

    /// <summary>
    /// Passive Skills
    /// </summary>
    public class PassiveSkills : Skill
    {
        //In case there something I'm missing from a passiveskill
    }

    /// <summary>
    /// The Passive Skills class with an Category string
    /// </summary>
    public class PassiveSkillList
    {
        public PassiveSkillList()
        {
            PassiveSkills = new List<PassiveSkills>();
        }

        /// <summary>
        /// Gives you what category these passive skills belongs too ({CategoryName}=)
        /// </summary>
        public string Category { get; set; }
        /// <summary>
        /// All the passive skills that this list has
        /// </summary>
        public List<PassiveSkills> PassiveSkills { get; set; }
    }
    #endregion

    #region Item Drop Location
    /// <summary>
    /// Item Drop Location List
    /// </summary>
    public class ItemDropLocationList
    {
        public ItemDropLocationList() { DropLocations = new List<ItemDropLocation>(); }
        /// <summary>
        /// Category ({CategoryName}=)
        /// </summary>
        public string Category { get; set; }
        /// <summary>
        /// Where this item does dropped
        /// </summary>
        public List<ItemDropLocation> DropLocations { get; set; }
    }

    /// <summary>
    /// Item Drop Location
    /// </summary>
    public class ItemDropLocation
    {
        /// <summary>
        /// Will Get replaced with a location class soon™
        /// </summary>
        public string Location { get; set; }
        /// <summary>
        /// How much that might be dropped
        /// </summary>
        public string PossibleDrops { get; set; }
        /// <summary>
        /// AP Cost
        /// </summary>
        public string APCost { get; set; }
    }
    #endregion

    #region Level Effect
    /// <summary>
    /// The core part of the Level Effects
    /// </summary>
    public class LevelEffectCore
    {
        /// <summary>
        /// Gives you the effect's strength (l{level number})
        /// </summary>
        public string EffectStrength { get; set; }
        /// <summary>
        /// Gives you the cooldown period of this effect (c{level number})
        /// </summary>
        public string Cooldown { get; set; }
    }

    /// <summary>
    /// Contains all the information about the level effect that has 5 levels
    /// </summary>
    public class LevelEffect5
    {
        public LevelEffect5()
        {
            Level1Effect = new LevelEffectCore();
            Level2Effect = new LevelEffectCore();
            Level3Effect = new LevelEffectCore();
            Level4Effect = new LevelEffectCore();
            Level5Effect = new LevelEffectCore();
        }

        /// <summary>
        /// Gives you the name of this LevelEffect (leveleffect)
        /// </summary>
        public string LevelEffectName { get; set; }
        /// <summary>
        /// The Level Effect when the Level is Level 1 (l1 and c1)
        /// </summary>
        public LevelEffectCore Level1Effect { get; set; }
        /// <summary>
        /// The Level Effect when the Level is Level 2 (l2 and c2)
        /// </summary>
        public LevelEffectCore Level2Effect { get; set; }
        /// <summary>
        /// The Level Effect when the Level is Level 3 (l3 and c3)
        /// </summary>
        public LevelEffectCore Level3Effect { get; set; }
        /// <summary>
        /// The Level Effect when the Level is Level 4 (l4 and c4)
        /// </summary>
        public LevelEffectCore Level4Effect { get; set; }
        /// <summary>
        /// The Level Effect when the Level is Level 6 (l5 and c5)
        /// </summary>
        public LevelEffectCore Level5Effect { get; set; }
    }

    /// <summary>
    /// Contains all the information about the level effect that has 10 levels
    /// </summary>
    public class LevelEffect10 : LevelEffect5 
    {
        public LevelEffect10()
        {
            Level6Effect = new LevelEffectCore();
            Level7Effect = new LevelEffectCore();
            Level8Effect = new LevelEffectCore();
            Level9Effect = new LevelEffectCore();
            Level10Effect = new LevelEffectCore();
        }

        /// <summary>
        /// The Level Effect when the Level is Level 6 (l6 and c6)
        /// </summary>
        public LevelEffectCore Level6Effect { get; set; }
        /// <summary>
        /// The Level Effect when the Level is Level 7 (l7 and c7)
        /// </summary>
        public LevelEffectCore Level7Effect { get; set; }
        /// <summary>
        /// The Level Effect when the Level is Level 8 (l8 and c8)
        /// </summary>
        public LevelEffectCore Level8Effect { get; set; }
        /// <summary>
        /// The Level Effect when the Level is Level 9 (l9 and c9)
        /// </summary>
        public LevelEffectCore Level9Effect { get; set; }
        /// <summary>
        /// The Level Effect when the Level is Level 10 (l10 and c10)
        /// </summary>
        public LevelEffectCore Level10Effect { get; set; }
    }
    #endregion

    #region Noble Phantasm
    /// <summary>
    /// Noble Phantasm
    /// </summary>
    public class NoblePhantasm
    {
        /// <summary>
        /// The noble phantasm name (name)
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// The noble phantasm rank (rank)
        /// </summary>
        public string Rank { get; set; }
        /// <summary>
        /// The noble phantasm classification (classification)
        /// </summary>
        public string Classification { get; set; }
        /// <summary>
        /// The noble phantasm type (type)
        /// </summary>
        public string Type { get; set; }
        /// <summary>
        /// The noble phantasm hitcount (hitcount)
        /// </summary>
        public string HitCount { get; set; }
        /// <summary>
        /// The noble phantasm effects (effect)
        /// </summary>
        public string[] Effects { get; set; }
        /// <summary>
        /// The effect that happens when you overcharge (overchargeeffect )
        /// </summary>
        public string[] OverChargeEffect { get; set; }
        /// <summary>
        /// Information about the level effect
        /// </summary>
        public LevelEffect LevelEffect { get; set; }
        /// <summary>
        /// Information about the chargeEffect
        /// </summary>
        public ChargeEffect ChargeEffect { get; set; }
        /// <summary>
        /// If this is a video
        /// </summary>
        public bool IsVideo { get; set; }
        /// <summary>
        /// Information about the video (if it's a video)
        /// </summary>
        public VideoInformation VideoInformation { get; set; }
    }
    /// <summary>
    /// Noble Phantasm (List)
    /// </summary>
    public class NoblePhantasmList
    {
        public NoblePhantasmList() { NoblePhantasm = new NoblePhantasm(); }
        /// <summary>
        /// Category ({Name}=)
        /// </summary>
        public string Category { get; set; }
        /// <summary>
        /// Infomation about the NoblePhantasm
        /// </summary>
        public NoblePhantasm NoblePhantasm { get; set; }
    }
    #endregion

    #region Effects
    /// <summary>
    /// Charge Effect
    /// </summary>
    public class ChargeEffect : LevelEffect
    {
    }

    /// <summary>
    /// Level Effect
    /// </summary>
    public class LevelEffect
    {
        /// <summary>
        /// (leveleffect)
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// NP Level 1
        /// </summary>
        public string NPLevel1 { get; set; }
        /// <summary>
        /// NP Level 2
        /// </summary>
        public string NPLevel2 { get; set; }
        /// <summary>
        /// NP Level 3
        /// </summary>
        public string NPLevel3 { get; set; }
        /// <summary>
        /// NP Level 4
        /// </summary>
        public string NPLevel4 { get; set; }
        /// <summary>
        /// NP Level 5
        /// </summary>
        public string NPLevel5 { get; set; }
    }
    #endregion

    #region Stats
    /// <summary>
    /// Stats
    /// </summary>
    public class Stats
    {
        public Stats() { Strength = new Status(); Endurance = new Status(); Agility = new Status(); Mana = new Status(); Luck = new Status(); NP = new Status(); }
        /// <summary>
        /// Strength
        /// </summary>
        public Status Strength { get; set; }
        /// <summary>
        /// Endurance
        /// </summary>
        public Status Endurance { get; set; }
        /// <summary>
        /// Agility
        /// </summary>
        public Status Agility { get; set; }
        /// <summary>
        /// Mana
        /// </summary>
        public Status Mana { get; set; }
        /// <summary>
        /// Luck
        /// </summary>
        public Status Luck { get; set; }
        /// <summary>
        /// NP
        /// </summary>
        public Status NP { get; set; }
    }

    /// <summary>
    /// Status
    /// </summary>
    public class Status
    {
        /// <summary>
        /// Grade
        /// </summary>
        public string Grade { get; set; }
        /// <summary>
        /// BarNumber
        /// </summary>
        public string BarNumber { get; set; }
    }
    #endregion

    #region Bond Level
    /// <summary>
    /// Bond Levels
    /// </summary>
    public class BondLevels
    {
        public BondLevels() { BondLevel1 = new BondLevel(); BondLevel2 = new BondLevel(); BondLevel3 = new BondLevel(); BondLevel4 = new BondLevel(); BondLevel5 = new BondLevel(); BondLevel6 = new BondLevel(); BondLevel7 = new BondLevel(); BondLevel8 = new BondLevel(); BondLevel9 = new BondLevel(); BondLevel10 = new BondLevel(); Bond10Reward = new Bond10Reward(); }
        /// <summary>
        /// Bond Level 1
        /// </summary>
        public BondLevel BondLevel1 { get; set; }
        /// <summary>
        /// Bond Level 2
        /// </summary>
        public BondLevel BondLevel2 { get; set; }
        /// <summary>
        /// Bond Level 3
        /// </summary>
        public BondLevel BondLevel3 { get; set; }
        /// <summary>
        /// Bond Level 4
        /// </summary>
        public BondLevel BondLevel4 { get; set; }
        /// <summary>
        /// Bond Level 5
        /// </summary>
        public BondLevel BondLevel5 { get; set; }
        /// <summary>
        /// Bond Level 6
        /// </summary>
        public BondLevel BondLevel6 { get; set; }
        /// <summary>
        /// Bond Level 7
        /// </summary>
        public BondLevel BondLevel7 { get; set; }
        /// <summary>
        /// Bond Level 8
        /// </summary>
        public BondLevel BondLevel8 { get; set; }
        /// <summary>
        /// Bond Level 9
        /// </summary>
        public BondLevel BondLevel9 { get; set; }
        /// <summary>
        /// Bond Level 10
        /// </summary>
        public BondLevel BondLevel10 { get; set; }
        /// <summary>
        /// Reward when the Bond Level is Bond Level 10
        /// </summary>
        public Bond10Reward Bond10Reward { get; set; }
    }

    /// <summary>
    /// Bond Level
    /// </summary>
    public class BondLevel
    {
        /// <summary>
        /// Bond Required
        /// </summary>
        public string BondRequired { get; set; }
        /// <summary>
        /// Total Bond
        /// </summary>
        public string TotalBond { get; set; }
    }

    /// <summary>
    /// Bond 10 Reward
    /// </summary>
    public class Bond10Reward
    {
        /// <summary>
        /// Image that with the reward
        /// </summary>
        public ImageInformation Image { get; set; }
        /// <summary>
        /// To be replaced with a Craft Essence class
        /// </summary>
        public string Effect { get; set; }
    }
    #endregion

    #region Biography
    /// <summary>
    /// Biography
    /// </summary>
    public class Biography
    {
        public Biography() { Default = new BiographyPart(); Bond1 = new BiographyPart(); Bond2 = new BiographyPart(); Bond3 = new BiographyPart(); Bond4 = new BiographyPart(); Bond5 = new BiographyPart(); Extra = new BiographyPart(); }
        /// <summary>
        /// Biography (Default)
        /// </summary>
        public BiographyPart Default { get; set; }
        /// <summary>
        /// Biography at bond 1
        /// </summary>
        public BiographyPart Bond1 { get; set; }
        /// <summary>
        /// Biography at bond 2
        /// </summary>
        public BiographyPart Bond2 { get; set; }
        /// <summary>
        /// Biography at bond 3
        /// </summary>
        public BiographyPart Bond3 { get; set; }
        /// <summary>
        /// Biography at bond 4
        /// </summary>
        public BiographyPart Bond4 { get; set; }
        /// <summary>
        /// Biography at bond 5
        /// </summary>
        public BiographyPart Bond5 { get; set; }
        /// <summary>
        /// Biography (Extra)
        /// </summary>
        public BiographyPart Extra { get; set; }
    }

    /// <summary>
    /// Biography Content
    /// </summary>
    public class BiographyPart
    {
        /// <summary>
        /// What the Biography is in Japanese
        /// </summary>
        public string JapaneseText { get; set; }
        /// <summary>
        /// What the Biography is in English
        /// </summary>
        public string EnglishText { get; set; }
    }
    #endregion

    #region Other
    /// <summary>
    /// Presets for what information to get
    /// </summary>
    public enum PresetsForInformation
    {
        /// <summary>
        /// Get all possible information
        /// </summary>
        AllInformation,
        /// <summary>
        /// Only get the basic set of information
        /// </summary>
        BasicInformation,
        /// <summary>
        /// Not Set for what to get
        /// </summary>
        NotSet
    }

    /// <summary>
    /// Container for anything that drops items
    /// </summary>
    public class ItemDrops
    {
        /// <summary>
        /// Enemies that drop this item
        /// </summary>
        public List<Enemy> Enemies { get; set; }
        /// <summary>
        /// Servants that drop this item
        /// </summary>
        public List<Servant> Servants { get; set; }
    }

    /// <summary>
    /// Item
    /// </summary>
    public class Item
    {
        public Item() { GeneratedWith = ""; }
        public Item(string generatedWith, string englishName) { GeneratedWith = generatedWith; EnglishName = englishName; }
        /// <summary>
        /// String that was used when generating this servant class, used for checking if in cache and if cache version is up to date
        /// </summary>
        internal string GeneratedWith { get; private set; }
        /// <summary>
        /// Item's name in english
        /// </summary>
        public string EnglishName { get; set; }
        /// <summary>
        /// Item's name in japanese
        /// </summary>
        public string JapaneseName { get; set; }
        /// <summary>
        /// Item's description in english
        /// </summary>
        public string EnglishDescription { get; set; }
        /// <summary>
        /// Item's description in japanese
        /// </summary>
        public string JapaneseDescription { get; set; }
        /// <summary>
        /// Image of the item
        /// </summary>
        public ImageInformation ItemImage { get; set; }
        /// <summary>
        /// Enemies that drop this
        /// </summary>
        public ItemDrops AnythingThatDropsThis { get; set; }
        /// <summary>
        /// Where this item gets dropped
        /// </summary>
        public List<ItemDropLocationList> DropLocations { get; set; }
        /// <summary>
        /// The servants that uses this
        /// </summary>
        public List<Servant> Uses { get; set; }
#if DEBUG
        /// <summary>
        /// If from cache
        /// </summary>
        public bool FromCache { get; set; }
#endif
    }
    #endregion
}
