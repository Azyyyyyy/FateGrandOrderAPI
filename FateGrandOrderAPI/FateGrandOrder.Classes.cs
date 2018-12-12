using System.Collections.Generic;

namespace FateGrandOrderApi.Classes
{
    public class FateGrandOrderPerson
    {
        //Got to at least instantiate them even without them (do some warnings if you want to)
        public FateGrandOrderPerson() { GeneratedWith = ""; BasicInfomation = new FateGrandOrderPersonBasic(""); ActiveSkills = new List<ActiveSkills>(); PassiveSkills = new List<PassiveSkillsList>(); }

        public FateGrandOrderPerson(string generatedWith, string englishName) { GeneratedWith = generatedWith; BasicInfomation = new FateGrandOrderPersonBasic(englishName); ActiveSkills = new List<ActiveSkills>(); PassiveSkills = new List<PassiveSkillsList>(); }

        public string GeneratedWith { get; private set; }
        public FateGrandOrderPersonBasic BasicInfomation { get; set; }
        public List<ActiveSkills> ActiveSkills { get; set; }
        public List<PassiveSkillsList> PassiveSkills { get; set; }
#if DEBUG
        public bool FromCache { get; set; }
#endif
    }

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
        /// Gives you the basic ATK damage of the servant (atk)
        /// </summary>
        public string ATK { get; set; }
        /// <summary>
        /// Gives you the basic HP amount of the servant (hp)
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
        /// stars
        /// </summary>
        public string Stars { get; set; }
        /// <summary>
        /// Shows you the cost of the servant (cost)
        /// </summary>
        public string Cost { get; set; }
        /// <summary>
        /// cc
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
        /// qhits
        /// </summary>
        public string QuickHits { get; set; }
        /// <summary>
        /// ahits
        /// </summary>
        public string ArtsHits { get; set; }
        /// <summary>
        /// bhits
        /// </summary>
        public string BusterHits { get; set; }
        /// <summary>
        /// ehits
        /// </summary>
        public string ExtraHits { get; set; }
        /// <summary>
        /// deathrate 
        /// </summary>
        public string DeathRate { get; set; }
        /// <summary>
        /// starabsorption
        /// </summary>
        public string StarAbsorption { get; set; }
        /// <summary>
        /// stargeneration 
        /// </summary>
        public string StarGeneration { get; set; }
        /// <summary>
        /// npchargeatk
        /// </summary>
        public string NPChargeATK { get; set; }
        /// <summary>
        /// npchargedef 
        /// </summary>
        public string NPChargeDEF { get; set; }
        /// <summary>
        /// growthc
        /// </summary>
        public string GrowthCurve { get; set; }
        /// <summary>
        /// aka
        /// </summary>
        public string[] AKA { get; set; }
        /// <summary>
        /// traits
        /// </summary>
        public string[] Traits { get; set; }
        /// <summary>
        /// gender
        /// </summary>
        public string Gender { get; set; }
        /// <summary>
        /// alignment
        /// </summary>
        public string Alignment { get; set; }
        /// <summary>
        /// image
        /// </summary>
        public List<ImageInfomation> Images { get; set; }
    }

    public class ImageInfomation
    {
        public string Name { get; set; }
        public string Uri { get; set; }
    }

    public class LevelEffectCore
    {
        /// <summary>
        /// l{level number}
        /// </summary>
        public string EffectStrength { get; set; }
        /// <summary>
        /// c{level number}
        /// </summary>
        public string Cooldown { get; set; }
    }

    #region Skills
    public class Skills
    {
        public Skills()
        {
            Image = new ImageInfomation();
        }

        /// <summary>
        /// img 
        /// </summary>
        public ImageInfomation Image { get; set; }
        /// <summary>
        /// name
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// rank 
        /// </summary>
        public string Rank { get; set; }
        /// <summary>
        /// effect 
        /// </summary>
        public string[] Effect { get; set; }
    }

    public class ActiveSkills : Skills
    {
        public ActiveSkills()
        {
            LevelEffects = new List<LevelEffect>();
        }

        /// <summary>
        /// When Skill Gets unlocked through Ascension
        /// </summary>
        public string WhenSkillUnlocks { get; set; }

        /// <summary>
        /// servant
        /// </summary>
        public string[] ServantThatHaveThisSkill { get; set; }

        public List<LevelEffect> LevelEffects { get; set; }
        public bool ForNPC { get; set; }
    }

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
        /// leveleffect
        /// </summary>
        public string LevelEffectName { get; set; }
        /// <summary>
        /// l1 and c1
        /// </summary>
        public LevelEffectCore Level1Effect { get; set; }
        /// <summary>
        /// l2 and c2
        /// </summary>
        public LevelEffectCore Level2Effect { get; set; }
        /// <summary>
        /// l3 and c3
        /// </summary>
        public LevelEffectCore Level3Effect { get; set; }
        /// <summary>
        /// l4 and c4
        /// </summary>
        public LevelEffectCore Level4Effect { get; set; }
        /// <summary>
        /// l5 and c5
        /// </summary>
        public LevelEffectCore Level5Effect { get; set; }
        /// <summary>
        /// l6 and c6
        /// </summary>
        public LevelEffectCore Level6Effect { get; set; }
        /// <summary>
        /// l7 and c7
        /// </summary>
        public LevelEffectCore Level7Effect { get; set; }
        /// <summary>
        /// l8 and c8
        /// </summary>
        public LevelEffectCore Level8Effect { get; set; }
        /// <summary>
        /// l9 and c9
        /// </summary>
        public LevelEffectCore Level9Effect { get; set; }
        /// <summary>
        /// l10 and c10
        /// </summary>
        public LevelEffectCore Level10Effect { get; set; }
    }

    #region Passive Skills
    public class PassiveSkillsList
    {
        public PassiveSkillsList()
        {
            PassiveSkills = new List<PassiveSkills>();
        }

        public string Category { get; set; }
        public List<PassiveSkills> PassiveSkills { get; set; }
    }

    public class PassiveSkills : Skills
    {
        //In case there something I'm missing from a passiveskill
    }
    #endregion
    #endregion
}
