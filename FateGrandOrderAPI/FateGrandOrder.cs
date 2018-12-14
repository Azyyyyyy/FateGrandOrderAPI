using HtmlAgilityPack;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using FateGrandOrderApi.Classes;
using FateGrandOrderApi.Caching;

namespace FateGrandOrderApi
{
    /// <summary>
    /// Class containing parsing logic (this is where you get your people and skills from)
    /// </summary>
    public static class FateGrandOrderParsing
    {
        private static string FixString(string s)
        {
            return s.Replace("&lt;", "<").Replace("%27", "'");
        }

        /// <summary>
        /// This will return a Skill (will return null if the skill isn't found)
        /// </summary>
        /// <param name="skillName">The Skill name to look for</param>
        /// <param name="resultString">String[] we use to make the Skill (this is exposed for other Skill types as it will contain the infomation needed to make them too)</param>
        /// <returns></returns>
        public static Skill GetSkill(string skillName, out string[] resultString)
        {			
            Skill skill = null;
            foreach (HtmlNode col in new HtmlWeb().Load($"https://fategrandorder.fandom.com/wiki/{skillName}?action=edit").DocumentNode.SelectNodes("//textarea"))
            {
                //For in case we put the person in wrong
                if (string.IsNullOrEmpty(col.InnerText))
                    break;
                else
                    skill = new Skill();

                resultString = Regex.Split(col.InnerText, @"\n");

                foreach (string s in resultString)
                {
                    if (s.Contains("|img"))
                    {
                        skill.Image.Name = s.Replace("|img =", "").TrimStart(' ');
                        skill.Image.Uri = skill.Image.Name;
                    }
                    else if (s.Contains("|name"))
                    {
                        skill.Name = s.Replace("|name =", "").TrimStart(' ');
                    }
                    else if (s.Contains("|rank"))
                    {
                        skill.Rank = s.Replace("|rank =", "").TrimStart(' ');
                    }
                    else if (s.Contains("|effect"))
                    {
                        skill.Effect = FixString(s).Replace("|effect =", "").TrimStart(' ').Replace("<br/>", ",").Replace(", ",",").Split(',');
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

        /// <summary>
        /// This will return a filled in ActiveSkill (If the skill isn't a ActiveSkill it will return the core skill content and this will return a ActiveSkill only with the ActiveSkill name you used if the skill isn't found)
        /// </summary>
        /// <param name="skillName">The ActiveSkill name to look for</param>
        /// <returns></returns>
        public static ActiveSkill GetSkills(string skillName)
        {
            return GetSkill(new ActiveSkill { Name = skillName });
        }

        /// <summary>
        /// This will return a filled in ActiveSkill (If the skill isn't a ActiveSkill it will return the core skill content and this will return what was in the ActiveSkills already if the skill isn't found)
        /// </summary>
        /// <param name="skill">The ActiveSkill to put all the content into</param>
        /// <returns></returns>
        public static ActiveSkill GetSkill(ActiveSkill skill)
        {
            string lastLevelEffect = "";
            var basicSkillContent = GetSkill(skill.Name, out string[] resultString);

            string GetStartPart()
            {
                if (string.IsNullOrWhiteSpace(lastLevelEffect))
                    return "|";
                else
                    return $"|{lastLevelEffect[1]}";
            }

            //For in case we put the person in wrong
            if (resultString == null)
                return skill;

            foreach (string s in resultString)
            {
                if (s.Contains("|servanticons"))
                {
                    skill.ServantThatHaveThisSkill = s.Replace(@"{{", "").Replace(@"}}", "").Replace("|img =", "").TrimStart(' ').Replace(", ",",").Split(',');
                }
                else if (s.Contains($"leveleffect"))
                {
                    if (int.TryParse(s[1].ToString(), out int a))
                        lastLevelEffect = s;

                    skill.LevelEffects.Add(new LevelEffect10 { LevelEffectName = s.Replace($"{GetStartPart()}leveleffect =", "").TrimStart(' ') });
                }
                else if (s.Contains($"{GetStartPart()}l1 "))
                {
                    skill.LevelEffects[skill.LevelEffects.Count - 1].Level1Effect.EffectStrength = s.Replace($"{GetStartPart()}l1 =", "").TrimStart(' ');
                }
                else if (s.Contains($"{GetStartPart()}l2"))
                {
                    skill.LevelEffects[skill.LevelEffects.Count - 1].Level2Effect.EffectStrength = s.Replace($"{GetStartPart()}l2 =", "").TrimStart(' ');
                }
                else if (s.Contains($"{GetStartPart()}l3"))
                {
                    skill.LevelEffects[skill.LevelEffects.Count - 1].Level3Effect.EffectStrength = s.Replace($"{GetStartPart()}l3 =", "").TrimStart(' ');
                }
                else if (s.Contains($"{GetStartPart()}l4"))
                {
                    skill.LevelEffects[skill.LevelEffects.Count - 1].Level4Effect.EffectStrength = s.Replace($"{GetStartPart()}l4 =", "").TrimStart(' ');
                }
                else if (s.Contains($"{GetStartPart()}l5"))
                {
                    skill.LevelEffects[skill.LevelEffects.Count - 1].Level5Effect.EffectStrength = s.Replace($"{GetStartPart()}l5 =", "").TrimStart(' ');
                }
                else if (s.Contains($"{GetStartPart()}l6"))
                {
                    skill.LevelEffects[skill.LevelEffects.Count - 1].Level6Effect.EffectStrength = s.Replace($"{GetStartPart()}l6 =", "").TrimStart(' ');
                }
                else if (s.Contains($"{GetStartPart()}l7"))
                {
                    skill.LevelEffects[skill.LevelEffects.Count - 1].Level7Effect.EffectStrength = s.Replace($"{GetStartPart()}l7 =", "").TrimStart(' ');
                }
                else if (s.Contains($"{GetStartPart()}l8"))
                {
                    skill.LevelEffects[skill.LevelEffects.Count - 1].Level8Effect.EffectStrength = s.Replace($"{GetStartPart()}l8 =", "").TrimStart(' ');
                }
                else if (s.Contains($"{GetStartPart()}l9"))
                {
                    skill.LevelEffects[skill.LevelEffects.Count - 1].Level9Effect.EffectStrength = s.Replace($"{GetStartPart()}l9 =", "").TrimStart(' ');
                }
                else if (s.Contains($"{GetStartPart()}l10"))
                {
                    skill.LevelEffects[skill.LevelEffects.Count - 1].Level10Effect.EffectStrength = s.Replace($"{GetStartPart()}l10 =", "").TrimStart(' ');
                }
                else if (s.Contains("|c1 "))
                {
                    skill.LevelEffects[skill.LevelEffects.Count - 1].Level1Effect.Cooldown = s.Replace("|c1 =", "").TrimStart(' ');
                }
                else if (s.Contains("|c2"))
                {
                    skill.LevelEffects[skill.LevelEffects.Count - 1].Level2Effect.Cooldown = s.Replace("|c2 =", "").TrimStart(' ');
                }
                else if (s.Contains("|c3"))
                {
                    skill.LevelEffects[skill.LevelEffects.Count - 1].Level3Effect.Cooldown = s.Replace("|c3 =", "").TrimStart(' ');
                }
                else if (s.Contains("|c4"))
                {
                    skill.LevelEffects[skill.LevelEffects.Count - 1].Level4Effect.Cooldown = s.Replace("|c4 =", "").TrimStart(' ');
                }
                else if (s.Contains("|c5"))
                {
                    skill.LevelEffects[skill.LevelEffects.Count - 1].Level5Effect.Cooldown = s.Replace("|c5 =", "").TrimStart(' ');
                }
                else if (s.Contains("|c6"))
                {
                    skill.LevelEffects[skill.LevelEffects.Count - 1].Level6Effect.Cooldown = s.Replace("|c6 =", "").TrimStart(' ');
                }
                else if (s.Contains("|c7"))
                {
                    skill.LevelEffects[skill.LevelEffects.Count - 1].Level7Effect.Cooldown = s.Replace("|c7 =", "").TrimStart(' ');
                }
                else if (s.Contains("|c8"))
                {
                    skill.LevelEffects[skill.LevelEffects.Count - 1].Level8Effect.Cooldown = s.Replace("|c8 =", "").TrimStart(' ');
                }
                else if (s.Contains("|c9"))
                {
                    skill.LevelEffects[skill.LevelEffects.Count - 1].Level9Effect.Cooldown = s.Replace("|c9 =", "").TrimStart(' ');
                }
                else if (s.Contains("|c10"))
                {
                    skill.LevelEffects[skill.LevelEffects.Count - 1].Level10Effect.Cooldown = s.Replace("|c10 =", "").TrimStart(' ');
                }
                else if (s == @"}}")
                {
                    //This is becasuse there can be pages with different ranks, we just want the first one
                    break;
                }
            }

            foreach (LevelEffect10 le in skill.LevelEffects)
            {
                if (skill.LevelEffects[skill.LevelEffects.Count - 1].Level10Effect == le.Level10Effect)
                    break;

                le.Level10Effect.Cooldown = skill.LevelEffects[skill.LevelEffects.Count - 1].Level10Effect.Cooldown;
                le.Level9Effect.Cooldown = skill.LevelEffects[skill.LevelEffects.Count - 1].Level9Effect.Cooldown;
                le.Level8Effect.Cooldown = skill.LevelEffects[skill.LevelEffects.Count - 1].Level8Effect.Cooldown;
                le.Level7Effect.Cooldown = skill.LevelEffects[skill.LevelEffects.Count - 1].Level7Effect.Cooldown;
                le.Level6Effect.Cooldown = skill.LevelEffects[skill.LevelEffects.Count - 1].Level6Effect.Cooldown;
                le.Level5Effect.Cooldown = skill.LevelEffects[skill.LevelEffects.Count - 1].Level5Effect.Cooldown;
                le.Level4Effect.Cooldown = skill.LevelEffects[skill.LevelEffects.Count - 1].Level4Effect.Cooldown;
                le.Level3Effect.Cooldown = skill.LevelEffects[skill.LevelEffects.Count - 1].Level3Effect.Cooldown;
                le.Level2Effect.Cooldown = skill.LevelEffects[skill.LevelEffects.Count - 1].Level2Effect.Cooldown;
                le.Level1Effect.Cooldown = skill.LevelEffects[skill.LevelEffects.Count - 1].Level1Effect.Cooldown;
            }

            if (basicSkillContent != null)
            {
                skill.Effect = basicSkillContent.Effect;
                skill.Image = basicSkillContent.Image;
                skill.Rank = basicSkillContent.Rank;
            }
            return skill;
        }

        /// <summary>
        /// This will return the person from the persons name (will return null if we are unable to find the person)
        /// </summary>
        /// <param name="person">The persons name</param>
        /// <returns></returns>
        public static FateGrandOrderPerson GetPerson(string person)
        {
            FateGrandOrderPerson fateGrandOrderPerson = null;
            bool GotPersonAlready = false;
            bool GettingActiveSkills = false;
            bool GettingPassiveSkills = false;
            bool GettingNoblePhantasm = false;
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
                                fateGrandOrderPerson.PassiveSkills.Add(new PassiveSkillList());
                                PassiveSkillsCount = 0;
                            }
                        }
                        else if (!string.IsNullOrWhiteSpace(s) && s[s.Length - 1] == '=')
                        {
                            fateGrandOrderPerson.PassiveSkills.Add(new PassiveSkillList());
                            fateGrandOrderPerson.PassiveSkills[fateGrandOrderPerson.PassiveSkills.Count - 1].Category = s.Replace("=", "");
                            PassiveSkillsCount = 0;
                        }
                        else if (s.Contains("|img"))
                        {
                            fateGrandOrderPerson.PassiveSkills[fateGrandOrderPerson.PassiveSkills.Count - 1].PassiveSkills.Add(new PassiveSkills());
                            PassiveSkillsCount++;
                            if (PassiveSkillsCount == 1)
                            {
                                fateGrandOrderPerson.PassiveSkills[fateGrandOrderPerson.PassiveSkills.Count - 1].PassiveSkills[fateGrandOrderPerson.PassiveSkills[fateGrandOrderPerson.PassiveSkills.Count - 1].PassiveSkills.Count - 1].Image.Name = s.Replace("|img =", "").TrimStart(' ');
                                fateGrandOrderPerson.PassiveSkills[fateGrandOrderPerson.PassiveSkills.Count - 1].PassiveSkills[fateGrandOrderPerson.PassiveSkills[fateGrandOrderPerson.PassiveSkills.Count - 1].PassiveSkills.Count - 1].Image.Uri = fateGrandOrderPerson.PassiveSkills[fateGrandOrderPerson.PassiveSkills.Count - 1].PassiveSkills[fateGrandOrderPerson.PassiveSkills[fateGrandOrderPerson.PassiveSkills.Count - 1].PassiveSkills.Count - 1].Image.Name;
                            }
                            else
                            {
                                fateGrandOrderPerson.PassiveSkills[fateGrandOrderPerson.PassiveSkills.Count - 1].PassiveSkills[fateGrandOrderPerson.PassiveSkills[fateGrandOrderPerson.PassiveSkills.Count - 1].PassiveSkills.Count - 1].Image.Name = s.Replace($"|img{PassiveSkillsCount} =", "").TrimStart(' ');
                                fateGrandOrderPerson.PassiveSkills[fateGrandOrderPerson.PassiveSkills.Count - 1].PassiveSkills[fateGrandOrderPerson.PassiveSkills[fateGrandOrderPerson.PassiveSkills.Count - 1].PassiveSkills.Count - 1].Image.Uri = fateGrandOrderPerson.PassiveSkills[fateGrandOrderPerson.PassiveSkills.Count - 1].PassiveSkills[fateGrandOrderPerson.PassiveSkills[fateGrandOrderPerson.PassiveSkills.Count - 1].PassiveSkills.Count - 1].Image.Name;
                            }
                        }
                        else if (s.Contains("|name"))
                        {
                            if (PassiveSkillsCount == 1)
                                fateGrandOrderPerson.PassiveSkills[fateGrandOrderPerson.PassiveSkills.Count - 1].PassiveSkills[fateGrandOrderPerson.PassiveSkills[fateGrandOrderPerson.PassiveSkills.Count - 1].PassiveSkills.Count - 1].Name = s.Replace("|name =", "").TrimStart(' ');
                            else
                                fateGrandOrderPerson.PassiveSkills[fateGrandOrderPerson.PassiveSkills.Count - 1].PassiveSkills[fateGrandOrderPerson.PassiveSkills[fateGrandOrderPerson.PassiveSkills.Count - 1].PassiveSkills.Count - 1].Name = s.Replace($"|name{PassiveSkillsCount} =", "").TrimStart(' ');
                        }
                        else if (s.Contains("|rank"))
                        {
                            if (PassiveSkillsCount == 1)
                                fateGrandOrderPerson.PassiveSkills[fateGrandOrderPerson.PassiveSkills.Count - 1].PassiveSkills[fateGrandOrderPerson.PassiveSkills[fateGrandOrderPerson.PassiveSkills.Count - 1].PassiveSkills.Count - 1].Rank = s.Replace("|rank =", "").TrimStart(' ');
                            else
                                fateGrandOrderPerson.PassiveSkills[fateGrandOrderPerson.PassiveSkills.Count - 1].PassiveSkills[fateGrandOrderPerson.PassiveSkills[fateGrandOrderPerson.PassiveSkills.Count - 1].PassiveSkills.Count - 1].Rank = s.Replace($"|rank{PassiveSkillsCount} =", "").TrimStart(' ');
                        }
                        else if (s.Contains("|effect"))
                        {
                            if (PassiveSkillsCount == 1)
                                fateGrandOrderPerson.PassiveSkills[fateGrandOrderPerson.PassiveSkills.Count - 1].PassiveSkills[fateGrandOrderPerson.PassiveSkills[fateGrandOrderPerson.PassiveSkills.Count - 1].PassiveSkills.Count - 1].Effect = s.Replace("|effect =", "").TrimStart(' ').Split(',');
                            else
                                fateGrandOrderPerson.PassiveSkills[fateGrandOrderPerson.PassiveSkills.Count - 1].PassiveSkills[fateGrandOrderPerson.PassiveSkills[fateGrandOrderPerson.PassiveSkills.Count - 1].PassiveSkills.Count - 1].Effect = s.Replace($"|effect{PassiveSkillsCount} =", "").TrimStart(' ').Replace(", ",",").Split(',');
                        }
                    }
                    #endregion

                    #region Active Skills
                    if (GettingActiveSkills)
                    {
                        if (s[s.Length - 1] == '=')
                        {
                            fateGrandOrderPerson.ActiveSkills.Add(new ActiveSkill());
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

                    #region Noble Phantasm
                    if (GettingNoblePhantasm)
                    {
                        if (!string.IsNullOrWhiteSpace(s) && s[s.Length - 1] == '=')
                        {
                            fateGrandOrderPerson.NoblePhantasms[fateGrandOrderPerson.NoblePhantasms.Count - 1].Category = s.Replace("=", "");
                        }
                        if (s.Contains("[[File:"))
                        {
                            fateGrandOrderPerson.NoblePhantasms[fateGrandOrderPerson.NoblePhantasms.Count - 1].NoblePhantasm.IsVideo = true;
                            fateGrandOrderPerson.NoblePhantasms[fateGrandOrderPerson.NoblePhantasms.Count - 1].NoblePhantasm.VideoInfomation = new VideoInfomation
                            {
                                Name = s.Replace("[[File:", ""),
                            };
                            fateGrandOrderPerson.NoblePhantasms[fateGrandOrderPerson.NoblePhantasms.Count - 1].NoblePhantasm.VideoInfomation.Name = fateGrandOrderPerson.NoblePhantasms[fateGrandOrderPerson.NoblePhantasms.Count - 1].NoblePhantasm.VideoInfomation.Name.Remove(fateGrandOrderPerson.NoblePhantasms[fateGrandOrderPerson.NoblePhantasms.Count - 1].NoblePhantasm.VideoInfomation.Name.ToLower().LastIndexOf("c") - 1);
                            if (fateGrandOrderPerson.NoblePhantasms[fateGrandOrderPerson.NoblePhantasms.Count - 1].NoblePhantasm.VideoInfomation.Name.LastIndexOf("|") != -1) { fateGrandOrderPerson.NoblePhantasms[fateGrandOrderPerson.NoblePhantasms.Count - 1].NoblePhantasm.VideoInfomation.Name = fateGrandOrderPerson.NoblePhantasms[fateGrandOrderPerson.NoblePhantasms.Count - 1].NoblePhantasm.VideoInfomation.Name.Remove(fateGrandOrderPerson.NoblePhantasms[fateGrandOrderPerson.NoblePhantasms.Count - 1].NoblePhantasm.VideoInfomation.Name.ToLower().LastIndexOf("|")); }
                            fateGrandOrderPerson.NoblePhantasms[fateGrandOrderPerson.NoblePhantasms.Count - 1].NoblePhantasm.VideoInfomation.Uri = fateGrandOrderPerson.NoblePhantasms[fateGrandOrderPerson.NoblePhantasms.Count - 1].NoblePhantasm.VideoInfomation.Name;
                        }
                        else if (s == "|-|")
                        {
                            fateGrandOrderPerson.NoblePhantasms.Add(new NoblePhantasmList());
                        }
                        else if (s.Contains("|name"))
                        {
                            fateGrandOrderPerson.NoblePhantasms[fateGrandOrderPerson.NoblePhantasms.Count - 1].NoblePhantasm.Name = FixString(s).Replace("|name =", "").TrimStart(' ').Replace("<br/>","\n");
                            if (s.Contains("Video"))
                            {
                                fateGrandOrderPerson.NoblePhantasms[fateGrandOrderPerson.NoblePhantasms.Count - 1].NoblePhantasm.IsVideo = true;
                                fateGrandOrderPerson.NoblePhantasms[fateGrandOrderPerson.NoblePhantasms.Count - 1].NoblePhantasm.VideoInfomation = new VideoInfomation();
                            }
                        }
                        else if (s.Contains("|rank"))
                        {
                            fateGrandOrderPerson.NoblePhantasms[fateGrandOrderPerson.NoblePhantasms.Count - 1].NoblePhantasm.Rank = s.Replace("|rank =", "").TrimStart(' ');
                        }
                        else if (s.Contains("|classification"))
                        {
                            fateGrandOrderPerson.NoblePhantasms[fateGrandOrderPerson.NoblePhantasms.Count - 1].NoblePhantasm.Classification = s.Replace("|classification =", "").TrimStart(' ');
                        }
                        else if (s.Contains("|type"))
                        {
                            fateGrandOrderPerson.NoblePhantasms[fateGrandOrderPerson.NoblePhantasms.Count - 1].NoblePhantasm.Type = s.Replace("|type =", "").TrimStart(' ');
                        }
                        else if (s.Contains("|hitcount"))
                        {
                            fateGrandOrderPerson.NoblePhantasms[fateGrandOrderPerson.NoblePhantasms.Count - 1].NoblePhantasm.HitCount = s.Replace("|hitcount =", "").TrimStart(' ');
                        }
                        else if (s.Contains("|effect"))
                        {
                            fateGrandOrderPerson.NoblePhantasms[fateGrandOrderPerson.NoblePhantasms.Count - 1].NoblePhantasm.Effects = FixString(s).Replace("|effect =", "").TrimStart(' ').Replace("<br/>", "").Replace(", ",",").Split(',');
                        }
                        else if (s.Contains("|overchargeeffect"))
                        {
                            fateGrandOrderPerson.NoblePhantasms[fateGrandOrderPerson.NoblePhantasms.Count - 1].NoblePhantasm.OverChargeEffect = FixString(s).Replace("|overchargeeffect =", "").TrimStart(' ').Replace("<br/>", "").Replace(", ",",").Split(',');
                        }
                        else if (s.Contains("|leveleffect"))
                        {
                            fateGrandOrderPerson.NoblePhantasms[fateGrandOrderPerson.NoblePhantasms.Count - 1].NoblePhantasm.LevelEffect = new LevelEffect();
                            fateGrandOrderPerson.NoblePhantasms[fateGrandOrderPerson.NoblePhantasms.Count - 1].NoblePhantasm.LevelEffect.Name = s.Replace("|leveleffect =", "").TrimStart(' ');
                        }
                        else if (s.Contains("|l1"))
                        {
                            fateGrandOrderPerson.NoblePhantasms[fateGrandOrderPerson.NoblePhantasms.Count - 1].NoblePhantasm.LevelEffect.NPLevel1 = s.Replace("|l1 =", "").TrimStart(' ');
                        }
                        else if (s.Contains("|l2"))
                        {
                            fateGrandOrderPerson.NoblePhantasms[fateGrandOrderPerson.NoblePhantasms.Count - 1].NoblePhantasm.LevelEffect.NPLevel2 = s.Replace("|l2 =", "").TrimStart(' ');
                        }
                        else if (s.Contains("|l3"))
                        {
                            fateGrandOrderPerson.NoblePhantasms[fateGrandOrderPerson.NoblePhantasms.Count - 1].NoblePhantasm.LevelEffect.NPLevel3 = s.Replace("|l3 =", "").TrimStart(' ');
                        }
                        else if (s.Contains("|l4"))
                        {
                            fateGrandOrderPerson.NoblePhantasms[fateGrandOrderPerson.NoblePhantasms.Count - 1].NoblePhantasm.LevelEffect.NPLevel4 = s.Replace("|l4 =", "").TrimStart(' ');
                        }
                        else if (s.Contains("|l5"))
                        {
                            fateGrandOrderPerson.NoblePhantasms[fateGrandOrderPerson.NoblePhantasms.Count - 1].NoblePhantasm.LevelEffect.NPLevel5 = s.Replace("|l5 =", "").TrimStart(' ');
                        }
                        else if (s.Contains("|chargeeffect"))
                        {
                            fateGrandOrderPerson.NoblePhantasms[fateGrandOrderPerson.NoblePhantasms.Count - 1].NoblePhantasm.ChargeEffect = new ChargeEffect();
                            fateGrandOrderPerson.NoblePhantasms[fateGrandOrderPerson.NoblePhantasms.Count - 1].NoblePhantasm.ChargeEffect.Name = s.Replace("|chargeeffect =", "").TrimStart(' ');
                        }
                        else if (s.Contains("|c1"))
                        {
                            fateGrandOrderPerson.NoblePhantasms[fateGrandOrderPerson.NoblePhantasms.Count - 1].NoblePhantasm.ChargeEffect.NPLevel1 = s.Replace("|c1 =", "").TrimStart(' ');
                        }
                        else if (s.Contains("|c2"))
                        {
                            fateGrandOrderPerson.NoblePhantasms[fateGrandOrderPerson.NoblePhantasms.Count - 1].NoblePhantasm.ChargeEffect.NPLevel2 = s.Replace("|c2 =", "").TrimStart(' ');
                        }
                        else if (s.Contains("|c3"))
                        {
                            fateGrandOrderPerson.NoblePhantasms[fateGrandOrderPerson.NoblePhantasms.Count - 1].NoblePhantasm.ChargeEffect.NPLevel3 = s.Replace("|c3 =", "").TrimStart(' ');
                        }
                        else if (s.Contains("|c4"))
                        {
                            fateGrandOrderPerson.NoblePhantasms[fateGrandOrderPerson.NoblePhantasms.Count - 1].NoblePhantasm.ChargeEffect.NPLevel4 = s.Replace("|c4 =", "").TrimStart(' ');
                        }
                        else if (s.Contains("|c5"))
                        {
                            fateGrandOrderPerson.NoblePhantasms[fateGrandOrderPerson.NoblePhantasms.Count - 1].NoblePhantasm.ChargeEffect.NPLevel5 = s.Replace("|c5 =", "").TrimStart(' ');
                        }
                    }
                    #endregion

                    #region Basic Infomation
                    #region Trigger Skills Logic
                    if (s == "== Passive Skills ==")
                    {
                        GettingPassiveSkills = true;
                    }
                    else if (s == "== Active Skills ==")
                    {
                        GettingActiveSkills = true;
                    }
                    else if (GettingActiveSkills | GettingPassiveSkills | GettingNoblePhantasm && FixString(s) == "</tabber>")
                    {
                        GettingActiveSkills = false;
                        GettingPassiveSkills = false;
                        GettingNoblePhantasm = false;
                    }
                    else if (GettingPassiveSkills && s == @"}}")
                    {
                        GettingPassiveSkills = false;
                    }
                    else if (s == "== Noble Phantasm ==" || s == "==Noble Phantasm==")
                    {
                        GettingNoblePhantasm = true;
                        fateGrandOrderPerson.NoblePhantasms.Add(new NoblePhantasmList());
                    }
                    #endregion
                    else if (s.Contains("|jname"))
                    {
                        fateGrandOrderPerson.BasicInfomation.JapaneseName = s.Replace("|jname =", "").TrimStart(' ');
                    }
                    else if (s.Contains("|voicea"))
                    {
                        fateGrandOrderPerson.BasicInfomation.VoiceActor = s.Replace("|voicea =", "").TrimStart(' '); 
                    }
                    else if (s.Contains("|illus"))
                    {
                        fateGrandOrderPerson.BasicInfomation.Illustrator = s.Replace("|illus =", "").TrimStart(' ');
                    }
                    else if (s.Contains("|class "))
                    {
                        fateGrandOrderPerson.BasicInfomation.Class = s.Replace("|class =", "").TrimStart(' ');
                    }
                    else if (s.Contains("|atk"))
                    {
                        fateGrandOrderPerson.BasicInfomation.ATK = s.Replace("|atk =", "").TrimStart(' ');
                    }
                    else if (s.Contains("|hp"))
                    {
                        fateGrandOrderPerson.BasicInfomation.HP = s.Replace("|hp =", "").TrimStart(' ');
                    }
                    else if (s.Contains("|gatk"))
                    {
                        fateGrandOrderPerson.BasicInfomation.GrailATK = s.Replace("|gatk =", "").TrimStart(' ').TrimStart(' ');
                    }
                    else if (s.Contains("|ghp"))
                    {
                        fateGrandOrderPerson.BasicInfomation.GrailHP = s.Replace("|ghp =", "").TrimStart(' ');
                    }
                    else if (s.Contains("|stars"))
                    {
                        fateGrandOrderPerson.BasicInfomation.Stars = s.Replace("|stars =", "").TrimStart(' ');
                    }
                    else if (s.Contains("|cost"))
                    {
                        fateGrandOrderPerson.BasicInfomation.Cost = s.Replace("|cost =", "").TrimStart(' ');
                    }
                    else if (s.Contains("|cc"))
                    {
                        fateGrandOrderPerson.BasicInfomation.QQQAB = s.Replace("|cc =", "").TrimStart(' ');
                    }
                    else if (s.Contains("|mlevel"))
                    {
                        fateGrandOrderPerson.BasicInfomation.MaxLevel = s.Replace("|mlevel =", "").TrimStart(' ');
                    }
                    else if (s.Contains("|id"))
                    {
                        fateGrandOrderPerson.BasicInfomation.ID = s.Replace("|id =", "").TrimStart(' ');
                    }
                    else if (s.Contains("|attribute"))
                    {
                        fateGrandOrderPerson.BasicInfomation.Attribute = s.Replace("|attribute =", "").TrimStart(' ');
                    }
                    else if (s.Contains("|qhits"))
                    {
                        fateGrandOrderPerson.BasicInfomation.QuickHits = s.Replace("|qhits =", "").TrimStart(' ');
                    }
                    else if (s.Contains("|ahits"))
                    {
                        fateGrandOrderPerson.BasicInfomation.ArtsHits = s.Replace("|ahits =", "").TrimStart(' ');
                    }
                    else if (s.Contains("|bhits"))
                    {
                        fateGrandOrderPerson.BasicInfomation.BusterHits = s.Replace("|bhits =", "").TrimStart(' ');
                    }
                    else if (s.Contains("|ehits"))
                    {
                        fateGrandOrderPerson.BasicInfomation.ExtraHits = s.Replace("|ehits =", "").TrimStart(' ');
                    }
                    else if (s.Contains("|deathrate"))
                    {
                        fateGrandOrderPerson.BasicInfomation.DeathRate = s.Replace("|deathrate =", "").TrimStart(' ');
                    }
                    else if (s.Contains("|starabsorption"))
                    {
                        fateGrandOrderPerson.BasicInfomation.StarAbsorption = s.Replace("|starabsorption =", "").TrimStart(' ');
                    }
                    else if (s.Contains("|stargeneration"))
                    {
                        fateGrandOrderPerson.BasicInfomation.StarGeneration = s.Replace("|stargeneration =", "").TrimStart(' ');
                    }
                    else if (s.Contains("|npchargeatk"))
                    {
                        fateGrandOrderPerson.BasicInfomation.NPChargeATK = s.Replace("|npchargeatk =", "").TrimStart(' ');
                    }
                    else if (s.Contains("|npchargedef"))
                    {
                        fateGrandOrderPerson.BasicInfomation.NPChargeDEF = s.Replace("|npchargedef =", "").TrimStart(' ');
                    }
                    else if (s.Contains("|growthc"))
                    {
                        fateGrandOrderPerson.BasicInfomation.GrowthCurve = s.Replace("|growthc =", "").TrimStart(' ');
                    }
                    else if (s.Contains("|aka"))
                    {
                        fateGrandOrderPerson.BasicInfomation.AKA = FixString(s).Replace("|aka =", "").TrimStart(' ').Replace("<br/>", " ").Replace("'''","").Replace("''", "").Replace(", ",",").Replace(", ",",").Split(',');
                    }
                    else if (s.Contains("|traits"))
                    {
                        fateGrandOrderPerson.BasicInfomation.Traits = s.Replace("|traits =", "").TrimStart(' ').Replace(", ",",").Split(',');
                    }
                    else if (s.Contains("|gender"))
                    {
                        var gender = s.Replace("|gender =", "").TrimStart(' ').ToLower();
                        if (gender[0] == 'f') { fateGrandOrderPerson.BasicInfomation.Gender = "Female"; }
                        else if (gender[0] == 'm') { fateGrandOrderPerson.BasicInfomation.Gender = "Male"; }
                        else { fateGrandOrderPerson.BasicInfomation.Gender = gender; }
                        gender = null;
                    }
                    else if (s.Contains("|alignment"))
                    {
                        fateGrandOrderPerson.BasicInfomation.Alignment = s.Replace("|alignment =", "").TrimStart(' ');
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
