using HtmlAgilityPack;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using FateGrandOrderApi.Classes;
using FateGrandOrderApi.Caching;

namespace FateGrandOrderApi
{
    public static class FateGrandOrderParsing
    {	
        public static Skills GetSkill(string skillName, out string[] resultString)
        {
			private static string FixString(string s)
			{
				s.Replace("&lt;","<").Replace("&lt;","<");
			}
			
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
                        skill.Effect = FixString(s).Replace("|effect = ", "").Replace("<br/>", ",").Split(',');
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
            string lastLevelEffect = "";
            var skill = GetSkill(skills.Name, out string[] resultString);

            string GetStartPart()
            {
                if (string.IsNullOrWhiteSpace(lastLevelEffect))
                    return "|";
                else
                    return $"|{lastLevelEffect[1]}";
            }

            //For in case we put the person in wrong
            if (resultString == null)
                return skills;

            foreach (string s in resultString)
            {
                if (s.Contains("|servanticons"))
                {
                    skills.ServantThatHaveThisSkill = s.Replace(@"{{", "").Replace(@"}}", "").Replace("|img = ", "").Split(',');
                }
                else if (s.Contains($"leveleffect"))
                {
                    if (int.TryParse(s[1].ToString(), out int a))
                        lastLevelEffect = s;

                    skills.LevelEffects.Add(new LevelEffect { LevelEffectName = s.Replace($"{GetStartPart()}leveleffect = ", "") });
                }
                else if (s.Contains($"{GetStartPart()}l1 "))
                {
                    skills.LevelEffects[skills.LevelEffects.Count - 1].Level1Effect.EffectStrength = s.Replace($"{GetStartPart()}l1 = ", "");
                }
                else if (s.Contains($"{GetStartPart()}l2"))
                {
                    skills.LevelEffects[skills.LevelEffects.Count - 1].Level2Effect.EffectStrength = s.Replace($"{GetStartPart()}l2 = ", "");
                }
                else if (s.Contains($"{GetStartPart()}l3"))
                {
                    skills.LevelEffects[skills.LevelEffects.Count - 1].Level3Effect.EffectStrength = s.Replace($"{GetStartPart()}l3 = ", "");
                }
                else if (s.Contains($"{GetStartPart()}l4"))
                {
                    skills.LevelEffects[skills.LevelEffects.Count - 1].Level4Effect.EffectStrength = s.Replace($"{GetStartPart()}l4 = ", "");
                }
                else if (s.Contains($"{GetStartPart()}l5"))
                {
                    skills.LevelEffects[skills.LevelEffects.Count - 1].Level5Effect.EffectStrength = s.Replace($"{GetStartPart()}l5 = ", "");
                }
                else if (s.Contains($"{GetStartPart()}l6"))
                {
                    skills.LevelEffects[skills.LevelEffects.Count - 1].Level6Effect.EffectStrength = s.Replace($"{GetStartPart()}l6 = ", "");
                }
                else if (s.Contains($"{GetStartPart()}l7"))
                {
                    skills.LevelEffects[skills.LevelEffects.Count - 1].Level7Effect.EffectStrength = s.Replace($"{GetStartPart()}l7 = ", "");
                }
                else if (s.Contains($"{GetStartPart()}l8"))
                {
                    skills.LevelEffects[skills.LevelEffects.Count - 1].Level8Effect.EffectStrength = s.Replace($"{GetStartPart()}l8 = ", "");
                }
                else if (s.Contains($"{GetStartPart()}l9"))
                {
                    skills.LevelEffects[skills.LevelEffects.Count - 1].Level9Effect.EffectStrength = s.Replace($"{GetStartPart()}l9 = ", "");
                }
                else if (s.Contains($"{GetStartPart()}l10"))
                {
                    skills.LevelEffects[skills.LevelEffects.Count - 1].Level10Effect.EffectStrength = s.Replace($"{GetStartPart()}l10 = ", "");
                }
                else if (s.Contains("|c1 "))
                {
                    skills.LevelEffects[skills.LevelEffects.Count - 1].Level1Effect.Cooldown = s.Replace("|c1 = ", "");
                }
                else if (s.Contains("|c2"))
                {
                    skills.LevelEffects[skills.LevelEffects.Count - 1].Level2Effect.Cooldown = s.Replace("|c2 = ", "");
                }
                else if (s.Contains("|c3"))
                {
                    skills.LevelEffects[skills.LevelEffects.Count - 1].Level3Effect.Cooldown = s.Replace("|c3 = ", "");
                }
                else if (s.Contains("|c4"))
                {
                    skills.LevelEffects[skills.LevelEffects.Count - 1].Level4Effect.Cooldown = s.Replace("|c4 = ", "");
                }
                else if (s.Contains("|c5"))
                {
                    skills.LevelEffects[skills.LevelEffects.Count - 1].Level5Effect.Cooldown = s.Replace("|c5 = ", "");
                }
                else if (s.Contains("|c6"))
                {
                    skills.LevelEffects[skills.LevelEffects.Count - 1].Level6Effect.Cooldown = s.Replace("|c6 = ", "");
                }
                else if (s.Contains("|c7"))
                {
                    skills.LevelEffects[skills.LevelEffects.Count - 1].Level7Effect.Cooldown = s.Replace("|c7 = ", "");
                }
                else if (s.Contains("|c8"))
                {
                    skills.LevelEffects[skills.LevelEffects.Count - 1].Level8Effect.Cooldown = s.Replace("|c8 = ", "");
                }
                else if (s.Contains("|c9"))
                {
                    skills.LevelEffects[skills.LevelEffects.Count - 1].Level9Effect.Cooldown = s.Replace("|c9 = ", "");
                }
                else if (s.Contains("|c10"))
                {
                    skills.LevelEffects[skills.LevelEffects.Count - 1].Level10Effect.Cooldown = s.Replace("|c10 = ", "");
                }
                else if (s == @"}}")
                {
                    //This is becasuse there can be pages with different ranks, we just want the first one
                    break;
                }
            }

            foreach (LevelEffect le in skills.LevelEffects)
            {
                if (skills.LevelEffects[skills.LevelEffects.Count - 1].Level10Effect == le.Level10Effect)
                    break;

                le.Level10Effect.Cooldown = skills.LevelEffects[skills.LevelEffects.Count - 1].Level10Effect.Cooldown;
                le.Level9Effect.Cooldown = skills.LevelEffects[skills.LevelEffects.Count - 1].Level9Effect.Cooldown;
                le.Level8Effect.Cooldown = skills.LevelEffects[skills.LevelEffects.Count - 1].Level8Effect.Cooldown;
                le.Level7Effect.Cooldown = skills.LevelEffects[skills.LevelEffects.Count - 1].Level7Effect.Cooldown;
                le.Level6Effect.Cooldown = skills.LevelEffects[skills.LevelEffects.Count - 1].Level6Effect.Cooldown;
                le.Level5Effect.Cooldown = skills.LevelEffects[skills.LevelEffects.Count - 1].Level5Effect.Cooldown;
                le.Level4Effect.Cooldown = skills.LevelEffects[skills.LevelEffects.Count - 1].Level4Effect.Cooldown;
                le.Level3Effect.Cooldown = skills.LevelEffects[skills.LevelEffects.Count - 1].Level3Effect.Cooldown;
                le.Level2Effect.Cooldown = skills.LevelEffects[skills.LevelEffects.Count - 1].Level2Effect.Cooldown;
                le.Level1Effect.Cooldown = skills.LevelEffects[skills.LevelEffects.Count - 1].Level1Effect.Cooldown;
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
                        if (s == "{{passiveskill")
                        {
                            if (fateGrandOrderPerson.PassiveSkills.Count == 0)
                            {
                                fateGrandOrderPerson.PassiveSkills.Add(new PassiveSkillsList());
                                PassiveSkillsCount = 0;
                            }
                        }
                        else if (!string.IsNullOrWhiteSpace(s) && s[s.Length - 1] == '=')
                        {
                            fateGrandOrderPerson.PassiveSkills.Add(new PassiveSkillsList());
                            fateGrandOrderPerson.PassiveSkills[fateGrandOrderPerson.PassiveSkills.Count - 1].Category = s.Replace("=", "");
                            PassiveSkillsCount = 0;
                        }
                        else if (s.Contains("|img"))
                        {
                            fateGrandOrderPerson.PassiveSkills[fateGrandOrderPerson.PassiveSkills.Count - 1].PassiveSkills.Add(new PassiveSkills());
                            PassiveSkillsCount++;
                            if (PassiveSkillsCount == 1)
                            {
                                fateGrandOrderPerson.PassiveSkills[fateGrandOrderPerson.PassiveSkills.Count - 1].PassiveSkills[fateGrandOrderPerson.PassiveSkills[fateGrandOrderPerson.PassiveSkills.Count - 1].PassiveSkills.Count - 1].Image.Name = s.Replace("|img = ", "");
                                fateGrandOrderPerson.PassiveSkills[fateGrandOrderPerson.PassiveSkills.Count - 1].PassiveSkills[fateGrandOrderPerson.PassiveSkills[fateGrandOrderPerson.PassiveSkills.Count - 1].PassiveSkills.Count - 1].Image.Uri = fateGrandOrderPerson.PassiveSkills[fateGrandOrderPerson.PassiveSkills.Count - 1].PassiveSkills[fateGrandOrderPerson.PassiveSkills[fateGrandOrderPerson.PassiveSkills.Count - 1].PassiveSkills.Count - 1].Image.Name;
                            }
                            else
                            {
                                fateGrandOrderPerson.PassiveSkills[fateGrandOrderPerson.PassiveSkills.Count - 1].PassiveSkills[fateGrandOrderPerson.PassiveSkills[fateGrandOrderPerson.PassiveSkills.Count - 1].PassiveSkills.Count - 1].Image.Name = s.Replace($"|img{PassiveSkillsCount} = ", "");
                                fateGrandOrderPerson.PassiveSkills[fateGrandOrderPerson.PassiveSkills.Count - 1].PassiveSkills[fateGrandOrderPerson.PassiveSkills[fateGrandOrderPerson.PassiveSkills.Count - 1].PassiveSkills.Count - 1].Image.Uri = fateGrandOrderPerson.PassiveSkills[fateGrandOrderPerson.PassiveSkills.Count - 1].PassiveSkills[fateGrandOrderPerson.PassiveSkills[fateGrandOrderPerson.PassiveSkills.Count - 1].PassiveSkills.Count - 1].Image.Name;
                            }
                        }
                        else if (s.Contains("|name"))
                        {
                            if (PassiveSkillsCount == 1)
                                fateGrandOrderPerson.PassiveSkills[fateGrandOrderPerson.PassiveSkills.Count - 1].PassiveSkills[fateGrandOrderPerson.PassiveSkills[fateGrandOrderPerson.PassiveSkills.Count - 1].PassiveSkills.Count - 1].Name = s.Replace("|name = ", "");
                            else
                                fateGrandOrderPerson.PassiveSkills[fateGrandOrderPerson.PassiveSkills.Count - 1].PassiveSkills[fateGrandOrderPerson.PassiveSkills[fateGrandOrderPerson.PassiveSkills.Count - 1].PassiveSkills.Count - 1].Name = s.Replace($"|name{PassiveSkillsCount} = ", "");
                        }
                        else if (s.Contains("|rank"))
                        {
                            if (PassiveSkillsCount == 1)
                                fateGrandOrderPerson.PassiveSkills[fateGrandOrderPerson.PassiveSkills.Count - 1].PassiveSkills[fateGrandOrderPerson.PassiveSkills[fateGrandOrderPerson.PassiveSkills.Count - 1].PassiveSkills.Count - 1].Rank = s.Replace("|rank = ", "");
                            else
                                fateGrandOrderPerson.PassiveSkills[fateGrandOrderPerson.PassiveSkills.Count - 1].PassiveSkills[fateGrandOrderPerson.PassiveSkills[fateGrandOrderPerson.PassiveSkills.Count - 1].PassiveSkills.Count - 1].Rank = s.Replace($"|rank{PassiveSkillsCount} = ", "");
                        }
                        else if (s.Contains("|effect"))
                        {
                            if (PassiveSkillsCount == 1)
                                fateGrandOrderPerson.PassiveSkills[fateGrandOrderPerson.PassiveSkills.Count - 1].PassiveSkills[fateGrandOrderPerson.PassiveSkills[fateGrandOrderPerson.PassiveSkills.Count - 1].PassiveSkills.Count - 1].Effect = s.Replace("|effect = ", "").Split(',');
                            else
                                fateGrandOrderPerson.PassiveSkills[fateGrandOrderPerson.PassiveSkills.Count - 1].PassiveSkills[fateGrandOrderPerson.PassiveSkills[fateGrandOrderPerson.PassiveSkills.Count - 1].PassiveSkills.Count - 1].Effect = s.Replace($"|effect{PassiveSkillsCount} = ", "").Split(',');
                        }
                    }
                    #endregion

                    #region Active Skills
                    if (GettingActiveSkills)
                    {
                        if (s[s.Length - 1] == '=')
                        {
                            fateGrandOrderPerson.ActiveSkills.Add(new ActiveSkills());
                            if (s.Contains("NPC"))
                                fateGrandOrderPerson.ActiveSkills[fateGrandOrderPerson.ActiveSkills.Count - 1].ForNPC = true;
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
                        //String[0] becomes {{nihongo|Innocent Murderer|無垢なる殺人鬼|Mukunaru Satsujinki}}&lt;br/>切り裂きジャック and me is confusion
                        fateGrandOrderPerson.BasicInfomation.AKA = FixString(s).Replace("|aka = ", "").Replace("<br/>", " ").Replace("'''","").Replace("''", "").Split(',');
                    }
                    else if (s.Contains("|traits"))
                    {
                        fateGrandOrderPerson.BasicInfomation.Traits = s.Replace("|traits = ", "").Split(',');
                    }
                    else if (s.Contains("|gender"))
                    {
                        fateGrandOrderPerson.BasicInfomation.Gender = s.Replace("|gender = ", "").ToLower()[0] == 'f' ? "Female" : "Male";
                    }
                    else if (s.Contains("|alignment"))
                    {
                        fateGrandOrderPerson.BasicInfomation.Alignment = s.Replace("|alignment = ", "");
                    }
                    else if (s == "== Passive Skills ==")
                    {
                        GettingPassiveSkills = true;
                    }
                    else if (s == "== Active Skills ==")
                    {
                        GettingActiveSkills = true;
                    }
                    else if (GettingActiveSkills | GettingPassiveSkills && FixString(s) == "</tabber>")
                    {
                        GettingActiveSkills = false;
                        GettingPassiveSkills = false;
                    }
                    else if (GettingPassiveSkills && s == @"}}")
                    {
                        GettingPassiveSkills = false;
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
}
