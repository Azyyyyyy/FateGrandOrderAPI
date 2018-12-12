using System.Collections.Generic;

namespace FateGrandOrderApi.Classes
{
    /// <summary>
    /// Your Fate/Grand Order servant
    /// </summary>
    public class FateGrandOrderPerson
    {
        //Got to at least instantiate them even without them (do some warnings if you want to)
        public FateGrandOrderPerson() { GeneratedWith = ""; BasicInfomation = new FateGrandOrderPersonBasic(""); ActiveSkills = new List<ActiveSkill>(); PassiveSkills = new List<PassiveSkillList>(); }

        public FateGrandOrderPerson(string generatedWith, string englishName) { GeneratedWith = generatedWith; BasicInfomation = new FateGrandOrderPersonBasic(englishName); ActiveSkills = new List<ActiveSkill>(); PassiveSkills = new List<PassiveSkillList>(); }
        /// <summary>
        /// The string that was used when generating this servants class
        /// </summary>
        public string GeneratedWith { get; private set; }
        /// <summary>
        /// The basic infomation about this servant (.e.g. Name, HP, ATK etc...)
        /// </summary>
        public FateGrandOrderPersonBasic BasicInfomation { get; set; }
        /// <summary>
        /// All the Active Skills this servant has
        /// </summary>
        public List<ActiveSkill> ActiveSkills { get; set; }
        /// <summary>
        /// All the Passive Skills this servant has
        /// </summary>
        public List<PassiveSkillList> PassiveSkills { get; set; }
#if DEBUG
        /// <summary>
        /// If the servant is in cache
        /// </summary>
        public bool FromCache { get; set; }
#endif
    }

    /// <summary>
    /// Basic infomation about your Fate/Grand Order servant
    /// </summary>
    public class FateGrandOrderPersonBasic
    {
        //Got to at least instantiate them even without them (do some warnings if you want to)
        public FateGrandOrderPersonBasic() { EnglishName = ""; }

        public FateGrandOrderPersonBasic(string englishName) { EnglishName = englishName; }

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
        /// (stars)
        /// </summary>
        public string Stars { get; set; }
        /// <summary>
        /// Shows you the cost of the servant (cost)
        /// </summary>
        public string Cost { get; set; }
        /// <summary>
        /// (cc)
        /// </summary>
        public string QQQAB { get; set; }
        /// <summary>
        /// Gives you the servant max level with ascension (mlevel)
        /// </summary>
        public string MaxLevel { get; set; }
        /// <summary>
        /// Returns the servants ID (id)
        /// </summary>
        public string ID { get; set; }
        /// <summary>
        /// Shows you their attribute (Man, Earth, Sky, Beast) (attribute)
        /// </summary>
        public string Attribute { get; set; }
        /// <summary>
        /// (qhits)
        /// </summary>
        public string QuickHits { get; set; }
        /// <summary>
        /// (ahits)
        /// </summary>
        public string ArtsHits { get; set; }
        /// <summary>
        /// (bhits)
        /// </summary>
        public string BusterHits { get; set; }
        /// <summary>
        /// (ehits)
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
        /// <summary>
        /// (image)
        /// </summary>
        public List<ImageInfomation> Images { get; set; }
    }

    /// <summary>
    /// Image name and Uri
    /// </summary>
    public class ImageInfomation
    {
        /// <summary>
        /// Returns image name (img)
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Returns image Uri (img)
        /// </summary>
        public string Uri { get; set; }
    }

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

    #region Skills
    /// <summary>
    /// The core part of a skill
    /// </summary>
    public class Skill
    {
        public Skill()
        {
            Image = new ImageInfomation();
        }

        /// <summary>
        /// Returns the image infomation of this skill (img) 
        /// </summary>
        public ImageInfomation Image { get; set; }
        /// <summary>
        /// Gives you skill's name (name)
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Gives you what rank this skill is at (rank)
        /// </summary>
        public string Rank { get; set; }
        /// <summary>
        /// Gives you what effect's the skill does (effect) 
        /// </summary>
        public string[] Effect { get; set; }
    }

    /// <summary>
    /// 
    /// </summary>
    public class ActiveSkill : Skill
    {
        public ActiveSkill()
        {
            LevelEffects = new List<LevelEffect>();
        }

        /// <summary>
        /// Returns when the skill gets unlocked through Ascension ({{unlock|)
        /// </summary>
        public string WhenSkillUnlocks { get; set; }

        /// <summary>
        /// Gives you what servant's have this skill (servant)
        /// </summary>
        public string[] ServantThatHaveThisSkill { get; set; }
        /// <summary>
        /// Gives you all the cooldown and effects strength
        /// </summary>
        public List<LevelEffect> LevelEffects { get; set; }
        /// <summary>
        /// Returns if this ActiveSkill is for an NPC
        /// </summary>
        public bool ForNPC { get; set; }
    }

    /// <summary>
    /// Contains all the infomation about the level effect
    /// </summary>
    public class LevelEffect
    {
        public LevelEffect()
        {
            Level1Effect = new LevelEffectCore();
            Level2Effect = new LevelEffectCore();
            Level3Effect = new LevelEffectCore();
            Level4Effect = new LevelEffectCore();
            Level5Effect = new LevelEffectCore();
            Level6Effect = new LevelEffectCore();
            Level7Effect = new LevelEffectCore();
            Level8Effect = new LevelEffectCore();
            Level9Effect = new LevelEffectCore();
            Level10Effect = new LevelEffectCore();
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
        /// The Level Effect when the Level is Level 1 (l3 and c3)
        /// </summary>
        public LevelEffectCore Level3Effect { get; set; }
        /// <summary>
        /// The Level Effect when the Level is Level 1 (l4 and c4)
        /// </summary>
        public LevelEffectCore Level4Effect { get; set; }
        /// <summary>
        /// The Level Effect when the Level is Level 1 (l5 and c5)
        /// </summary>
        public LevelEffectCore Level5Effect { get; set; }
        /// <summary>
        /// The Level Effect when the Level is Level 1 (l6 and c6)
        /// </summary>
        public LevelEffectCore Level6Effect { get; set; }
        /// <summary>
        /// The Level Effect when the Level is Level 1 (l7 and c7)
        /// </summary>
        public LevelEffectCore Level7Effect { get; set; }
        /// <summary>
        /// The Level Effect when the Level is Level 1 (l8 and c8)
        /// </summary>
        public LevelEffectCore Level8Effect { get; set; }
        /// <summary>
        /// The Level Effect when the Level is Level 1 (l9 and c9)
        /// </summary>
        public LevelEffectCore Level9Effect { get; set; }
        /// <summary>
        /// The Level Effect when the Level is Level 1 (l10 and c10)
        /// </summary>
        public LevelEffectCore Level10Effect { get; set; }
    }

    /// <summary>
    /// The Passive Skills class with an Category string
    /// </summary>
    #region Passive Skills
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

    /// <summary>
    /// Just an emtpy class named PassiveSkills which inheritances from Skill lol
    /// </summary>
    public class PassiveSkills : Skill
    {
        //In case there something I'm missing from a passiveskill
    }
    #endregion
    #endregion
}
