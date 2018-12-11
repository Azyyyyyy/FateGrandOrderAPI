using HtmlAgilityPack;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace FateGrandOrderApi
{
    public static class FateGrandOrderParsing
    {
        public static Skills GetSkill(string skillName, out string[] resultString)
        {
            Skills skill = null;
            foreach (HtmlNode col in new HtmlWeb().Load($"https://fategrandorder.fandom.com/wiki/{skillName}?action=edit").DocumentNode.SelectNodes("//textarea"))
            {
                //For in case we put the person in wrong
                if (string.IsNullOrEmpty(col.InnerText))
                    break;
                else
                    skill = new Skills();

                resultString = Regex.Split(col.InnerText, @"\n");

                foreach (string s in resultString)
                {
                    if (s.Contains("|img"))
                    {
                        skill.Image.Name = s.Replace("|img = ","");
                        skill.Image.Uri = skill.Image.Name;
                    }
                    else if (s.Contains("|name"))
                    {
                        skill.Name = s.Replace("|name = ", "");
                    }
                    else if (s.Contains("|rank"))
                    {
                        skill.Rank = s.Replace("|rank = ", "");
                    }
                    else if (s.Contains("|effect"))
                    {
                        skill.Effect = s.Replace("|effect = ", "").Replace("&lt;br/>", ",").Split(',');
                    }
                }

                if (skill != null)
                    return skill;
                else
                    return null;
            }
            resultString = null;
            return skill;
        }

        public static ActiveSkills GetSkill(ActiveSkills skills)
        {
            var skill = GetSkill(skills.Name, out string[] resultString);
            //For in case we put the person in wrong
            if (resultString == null)
                return skills;

            foreach (string s in resultString)
            {
                if (s.Contains("|servanticons"))
                {
                    skills.ServantThatHaveThisSkill = s.Replace(@"{{", "").Replace(@"}}", "").Replace("|img = ", "").Split(',');
                }
                else if (s.Contains("|leveleffect"))
                {
                    skills.LevelEffect = s.Replace("|leveleffect = ", "");
                }
                else if (s.Contains("|l1 "))
                {
                    skills.Level1Effect.EffectStrength = s.Replace("|l1 = ", "");
                }
                else if (s.Contains("|l2"))
                {
                    skills.Level2Effect.EffectStrength = s.Replace("|l2 = ", "");
                }
                else if (s.Contains("|l3"))
                {
                    skills.Level3Effect.EffectStrength = s.Replace("|l3 = ", "");
                }
                else if (s.Contains("|l4"))
                {
                    skills.Level4Effect.EffectStrength = s.Replace("|l4 = ", "");
                }
                else if (s.Contains("|l5"))
                {
                    skills.Level5Effect.EffectStrength = s.Replace("|l5 = ", "");
                }
                else if (s.Contains("|l6"))
                {
                    skills.Level6Effect.EffectStrength = s.Replace("|l6 = ", "");
                }
                else if (s.Contains("|l7"))
                {
                    skills.Level7Effect.EffectStrength = s.Replace("|l7 = ", "");
                }
                else if (s.Contains("|l8"))
                {
                    skills.Level8Effect.EffectStrength = s.Replace("|l8 = ", "");
                }
                else if (s.Contains("|l9"))
                {
                    skills.Level9Effect.EffectStrength = s.Replace("|l9 = ", "");
                }
                else if (s.Contains("|l10"))
                {
                    skills.Level10Effect.EffectStrength = s.Replace("|l10 = ", "");
                }
                else if (s.Contains("|c1 "))
                {
                    skills.Level1Effect.Cooldown = s.Replace("|c1 = ", "");
                }
                else if (s.Contains("|c2"))
                {
                    skills.Level2Effect.Cooldown = s.Replace("|c2 = ", "");
                }
                else if (s.Contains("|c3"))
                {
                    skills.Level3Effect.Cooldown = s.Replace("|c3 = ", "");
                }
                else if (s.Contains("|c4"))
                {
                    skills.Level4Effect.Cooldown = s.Replace("|c4 = ", "");
                }
                else if (s.Contains("|c5"))
                {
                    skills.Level5Effect.Cooldown = s.Replace("|c5 = ", "");
                }
                else if (s.Contains("|c6"))
                {
                    skills.Level6Effect.Cooldown = s.Replace("|c6 = ", "");
                }
                else if (s.Contains("|c7"))
                {
                    skills.Level7Effect.Cooldown = s.Replace("|c7 = ", "");
                }
                else if (s.Contains("|c8"))
                {
                    skills.Level8Effect.Cooldown = s.Replace("|c8 = ", "");
                }
                else if (s.Contains("|c9"))
                {
                    skills.Level9Effect.Cooldown = s.Replace("|c9 = ", "");
                }
                else if (s.Contains("|c10"))
                {
                    skills.Level10Effect.Cooldown = s.Replace("|c10 = ", "");
                }
            }

            if (skill != null)
            {
                skills.Effect = skill.Effect;
                skills.Image = skill.Image;
                skills.Rank = skill.Rank;
            }
            return skills;
        }

        public static FateGrandOrderPerson GetPerson(string person)
        {
            FateGrandOrderPerson fateGrandOrderPerson = null;
            bool GotPersonAlready = false;
            bool GettingActiveSkills = false;
            bool GettingPassiveSkills = false;
            int PassiveSkillsCount = 0;

            foreach (HtmlNode col in new HtmlWeb().Load($"https://fategrandorder.fandom.com/wiki/{person}?action=edit").DocumentNode.SelectNodes("//textarea"))
            {
                //For in case we put the person in wrong
                if (string.IsNullOrEmpty(col.InnerText))
                    break;

                FateGrandOrderPerson PersonToRemoveFromCache = null;

                if (FateGrandOrderPersonCache.FateGrandOrderPeople == null)
                    FateGrandOrderPersonCache.FateGrandOrderPeople = new List<FateGrandOrderPerson>();
                if (fateGrandOrderPerson == null)
                    fateGrandOrderPerson = new FateGrandOrderPerson(col.InnerText, person.Replace("_", " "));

                foreach (FateGrandOrderPerson fateGrandOrderPersonC in FateGrandOrderPersonCache.FateGrandOrderPeople)
                {
                    if (fateGrandOrderPersonC.GeneratedWith == fateGrandOrderPerson.GeneratedWith && fateGrandOrderPersonC.BasicInfomation.EnglishName == fateGrandOrderPerson.BasicInfomation.EnglishName)
                    {
                        GotPersonAlready = true;
                        fateGrandOrderPerson = fateGrandOrderPersonC;
#if DEBUG
                        fateGrandOrderPersonC.FromCache = true;
#endif
                        break;
                    }
                    else if (fateGrandOrderPersonC.GeneratedWith != fateGrandOrderPerson.GeneratedWith && fateGrandOrderPersonC.BasicInfomation.EnglishName == fateGrandOrderPerson.BasicInfomation.EnglishName)
                    {
                        PersonToRemoveFromCache = fateGrandOrderPersonC;
                        break;
                    }
                }

                if (GotPersonAlready)
                    break;
                if (PersonToRemoveFromCache != null)
                    FateGrandOrderPersonCache.FateGrandOrderPeople.Remove(PersonToRemoveFromCache);

                var resultString = Regex.Split(col.InnerText, @"\n");

                foreach (string s in resultString)
                {
                    #region Passive Skills
                    if (GettingPassiveSkills)
                    {
                        if (s.Contains("|img"))
                        {
                            fateGrandOrderPerson.PassiveSkills.Add(new PassiveSkills());
                            PassiveSkillsCount++;
                            if (PassiveSkillsCount == 1)
                            {
                                fateGrandOrderPerson.PassiveSkills[fateGrandOrderPerson.PassiveSkills.Count - 1].Image.Name = s.Replace("|img = ", "");
                                fateGrandOrderPerson.PassiveSkills[fateGrandOrderPerson.PassiveSkills.Count - 1].Image.Uri = fateGrandOrderPerson.PassiveSkills[fateGrandOrderPerson.PassiveSkills.Count - 1].Image.Name;
                            }
                            else
                            {
                                fateGrandOrderPerson.PassiveSkills[fateGrandOrderPerson.PassiveSkills.Count - 1].Image.Name = s.Replace($"|img{PassiveSkillsCount} = ", "");
                                fateGrandOrderPerson.PassiveSkills[fateGrandOrderPerson.PassiveSkills.Count - 1].Image.Uri = fateGrandOrderPerson.PassiveSkills[fateGrandOrderPerson.PassiveSkills.Count - 1].Image.Name;
                            }
                        }
                        else if (s.Contains("|name"))
                        {
                            if (PassiveSkillsCount == 1)
                                fateGrandOrderPerson.PassiveSkills[fateGrandOrderPerson.PassiveSkills.Count - 1].Name = s.Replace("|name = ", "");
                            else
                                fateGrandOrderPerson.PassiveSkills[fateGrandOrderPerson.PassiveSkills.Count - 1].Name = s.Replace($"|name{PassiveSkillsCount} = ", "");
                        }
                        else if (s.Contains("|rank"))
                        {
                            if (PassiveSkillsCount == 1)
                                fateGrandOrderPerson.PassiveSkills[fateGrandOrderPerson.PassiveSkills.Count - 1].Rank = s.Replace("|rank = ", "");
                            else
                                fateGrandOrderPerson.PassiveSkills[fateGrandOrderPerson.PassiveSkills.Count - 1].Rank = s.Replace($"|rank{PassiveSkillsCount} = ", "");
                        }
                        else if (s.Contains("|effect"))
                        {
                            if (PassiveSkillsCount == 1)
                                fateGrandOrderPerson.PassiveSkills[fateGrandOrderPerson.PassiveSkills.Count - 1].Effect = s.Replace("|effect = ", "").Split(',');
                            else
                                fateGrandOrderPerson.PassiveSkills[fateGrandOrderPerson.PassiveSkills.Count - 1].Effect = s.Replace($"|effect{PassiveSkillsCount} = ", "").Split(',');
                        }
                    }
                    #endregion

                    #region Active Skills
                        if (GettingActiveSkills)
                    {
                        if (s[s.Length - 1] == '=')
                        {
                            fateGrandOrderPerson.ActiveSkills.Add(new ActiveSkills());
                        }
                        else if (s.Contains("{{unlock|"))
                        {
                            fateGrandOrderPerson.ActiveSkills[fateGrandOrderPerson.ActiveSkills.Count - 1].WhenSkillUnlocks = s.Replace("{{unlock|", "")[0].ToString();
                        }
                        else if (s.Contains(@"{{:"))
                        {
                            fateGrandOrderPerson.ActiveSkills[fateGrandOrderPerson.ActiveSkills.Count - 1].Name = s.Remove(s.IndexOf("|")).Replace(@"{{:","");
                            fateGrandOrderPerson.ActiveSkills[fateGrandOrderPerson.ActiveSkills.Count - 1] = GetSkill(fateGrandOrderPerson.ActiveSkills[fateGrandOrderPerson.ActiveSkills.Count - 1]);
                        }
                    }
                    #endregion

                    #region Basic Infomation
                    if (s.Contains("|jname"))
                    {
                        fateGrandOrderPerson.BasicInfomation.JapaneseName = s.Replace("|jname = ", "");
                    }
                    else if (s.Contains("|voicea"))
                    {
                        fateGrandOrderPerson.BasicInfomation.VoiceActor = s.Replace("|voicea = ", ""); 
                    }
                    else if (s.Contains("|illus"))
                    {
                        fateGrandOrderPerson.BasicInfomation.Illustrator = s.Replace("|illus = ", "");
                    }
                    else if (s.Contains("|class "))
                    {
                        fateGrandOrderPerson.BasicInfomation.Class = s.Replace("|class = ", "");
                    }
                    else if (s.Contains("|atk"))
                    {
                        fateGrandOrderPerson.BasicInfomation.ATK = s.Replace("|atk = ", "");
                    }
                    else if (s.Contains("|hp"))
                    {
                        fateGrandOrderPerson.BasicInfomation.HP = s.Replace("|hp = ", "");
                    }
                    else if (s.Contains("|gatk"))
                    {
                        fateGrandOrderPerson.BasicInfomation.GrailATK = s.Replace("|gatk = ", "");
                    }
                    else if (s.Contains("|ghp"))
                    {
                        fateGrandOrderPerson.BasicInfomation.GrailHP = s.Replace("|ghp = ", "");
                    }
                    else if (s.Contains("|stars"))
                    {
                        fateGrandOrderPerson.BasicInfomation.Stars = s.Replace("|stars = ", "");
                    }
                    else if (s.Contains("|cost"))
                    {
                        fateGrandOrderPerson.BasicInfomation.Cost = s.Replace("|cost = ", "");
                    }
                    else if (s.Contains("|cc"))
                    {
                        fateGrandOrderPerson.BasicInfomation.QQQAB = s.Replace("|cc = ", "");
                    }
                    else if (s.Contains("|mlevel"))
                    {
                        fateGrandOrderPerson.BasicInfomation.MaxLevel = s.Replace("|mlevel = ", "");
                    }
                    else if (s.Contains("|id"))
                    {
                        fateGrandOrderPerson.BasicInfomation.ID = s.Replace("|id = ", "");
                    }
                    else if (s.Contains("|attribute"))
                    {
                        fateGrandOrderPerson.BasicInfomation.Attribute = s.Replace("|attribute = ", "");
                    }
                    else if (s.Contains("|qhits"))
                    {
                        fateGrandOrderPerson.BasicInfomation.QuickHits = s.Replace("|qhits = ", "");
                    }
                    else if (s.Contains("|ahits"))
                    {
                        fateGrandOrderPerson.BasicInfomation.ArtsHits = s.Replace("|ahits = ", "");
                    }
                    else if (s.Contains("|bhits"))
                    {
                        fateGrandOrderPerson.BasicInfomation.BusterHits = s.Replace("|bhits = ", "");
                    }
                    else if (s.Contains("|ehits"))
                    {
                        fateGrandOrderPerson.BasicInfomation.ExtraHits = s.Replace("|ehits = ", "");
                    }
                    else if (s.Contains("|deathrate"))
                    {
                        fateGrandOrderPerson.BasicInfomation.DeathRate = s.Replace("|deathrate = ", "");
                    }
                    else if (s.Contains("|starabsorption"))
                    {
                        fateGrandOrderPerson.BasicInfomation.StarAbsorption = s.Replace("|starabsorption = ", "");
                    }
                    else if (s.Contains("|stargeneration"))
                    {
                        fateGrandOrderPerson.BasicInfomation.StarGeneration = s.Replace("|stargeneration = ", "");
                    }
                    else if (s.Contains("|npchargeatk"))
                    {
                        fateGrandOrderPerson.BasicInfomation.NPChargeATK = s.Replace("|npchargeatk = ", "");
                    }
                    else if (s.Contains("|npchargedef"))
                    {
                        fateGrandOrderPerson.BasicInfomation.NPChargeDEF = s.Replace("|npchargedef = ", "");
                    }
                    else if (s.Contains("|growthc"))
                    {
                        fateGrandOrderPerson.BasicInfomation.GrowthCurve = s.Replace("|growthc = ", "");
                    }
                    else if (s.Contains("|aka"))
                    {
                        //String[0] becomes {{nihongo|Innocent Murderer|無垢なる殺人鬼|Mukunaru Satsujinki}}&lt;br/>切り裂きジャックand me is confusion
                        fateGrandOrderPerson.BasicInfomation.AKA = s.Replace("|aka = ", "").Replace("&lt;br/>", " ").Split(',');
                    }
                    else if (s.Contains("|traits"))
                    {
                        fateGrandOrderPerson.BasicInfomation.Traits = s.Replace("|traits = ", "").Split(',');
                    }
                    else if (s.Contains("|gender"))
                    {
                        fateGrandOrderPerson.BasicInfomation.Gender = s.Replace("|gender = ", "").ToLower()[0] == 'f' ? "Female" : "Male"; ///Returns the servants gender
                    }
                    else if (s.Contains("|alignment"))
                    {
                        fateGrandOrderPerson.BasicInfomation.Alignment = s.Replace("|alignment = ", ""); //Returns the servants alignment (Lawful - Neutral - Chaotic | Good - Neutral - Evil)
                    }
                    else if (s == "== Passive Skills ==")
                    {
                        GettingPassiveSkills = true;
                    }
                    else if (GettingPassiveSkills && s == "}}")
                    {
                        GettingPassiveSkills = false;
                    }
                    else if (s == "== Active Skills ==")
                    {
                        GettingActiveSkills = true;
                    }
                    else if (GettingActiveSkills && s == "&lt;/tabber>")
                    {
                        GettingActiveSkills = false;
                    }
                    #endregion
                }
            }

            if (fateGrandOrderPerson != null)
            {
                FateGrandOrderPersonCache.FateGrandOrderPeople.Add(fateGrandOrderPerson);
                return fateGrandOrderPerson;
            }
            else
            {
                return null;
            }
        }
    }

    public static class FateGrandOrderPersonCache
    {
        public static List<FateGrandOrderPerson> FateGrandOrderPeople { get; set; }
    }

    public class FateGrandOrderPerson
    {
        //Got to at least instantiate them even without them (do some warnings if you want to)
        public FateGrandOrderPerson() { GeneratedWith = ""; BasicInfomation = new FateGrandOrderPersonBasic(""); ActiveSkills = new List<ActiveSkills>(); PassiveSkills = new List<PassiveSkills>(); }

        public FateGrandOrderPerson(string generatedWith, string englishName) { GeneratedWith = generatedWith; BasicInfomation = new FateGrandOrderPersonBasic(englishName); ActiveSkills = new List<ActiveSkills>(); PassiveSkills = new List<PassiveSkills>(); }

        public string GeneratedWith { get; private set; }
        public FateGrandOrderPersonBasic BasicInfomation { get; set; }
        public List<ActiveSkills> ActiveSkills { get; set; }
        public List<PassiveSkills> PassiveSkills { get; set; }
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

    public class LevelEffect
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
            Level1Effect = new LevelEffect();
            Level2Effect = new LevelEffect();
            Level3Effect = new LevelEffect();
            Level4Effect = new LevelEffect();
            Level5Effect = new LevelEffect();
            Level6Effect = new LevelEffect();
            Level7Effect = new LevelEffect();
            Level8Effect = new LevelEffect();
            Level9Effect = new LevelEffect();
            Level10Effect = new LevelEffect();
        }

        /// <summary>
        /// When Skill Gets unlocked through Ascension
        /// </summary>
        public string WhenSkillUnlocks { get; set; }

        /// <summary>
        /// servant
        /// </summary>
        public string[] ServantThatHaveThisSkill { get; set; }
        /// <summary>
        /// leveleffect
        /// </summary>
        public string LevelEffect { get; set; }
        /// <summary>
        /// l1 and c1
        /// </summary>
        public LevelEffect Level1Effect { get; set; }
        /// <summary>
        /// l2 and c2
        /// </summary>
        public LevelEffect Level2Effect { get; set; }
        /// <summary>
        /// l3 and c3
        /// </summary>
        public LevelEffect Level3Effect { get; set; }
        /// <summary>
        /// l4 and c4
        /// </summary>
        public LevelEffect Level4Effect { get; set; }
        /// <summary>
        /// l5 and c5
        /// </summary>
        public LevelEffect Level5Effect { get; set; }
        /// <summary>
        /// l6 and c6
        /// </summary>
        public LevelEffect Level6Effect { get; set; }
        /// <summary>
        /// l7 and c7
        /// </summary>
        public LevelEffect Level7Effect { get; set; }
        /// <summary>
        /// l8 and c8
        /// </summary>
        public LevelEffect Level8Effect { get; set; }
        /// <summary>
        /// l9 and c9
        /// </summary>
        public LevelEffect Level9Effect { get; set; }
        /// <summary>
        /// l10 and c10
        /// </summary>
        public LevelEffect Level10Effect { get; set; }
    }

    public class PassiveSkills : Skills
    {
        //In case there something I'm missing from a passiveskill
    }
    #endregion
}
