using HtmlAgilityPack;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using FateGrandOrderApi.Classes;
using FateGrandOrderApi.Caching;
using System;

namespace FateGrandOrderApi
{
    /// <summary>
    /// Class containing parsing logic (this is where you get your people and skills from)
    /// </summary>
    public static class FateGrandOrderParsing
    {
        private static string FixString(string s)
        {
            return s.Replace("&lt;", "<").Replace("%27", "'").Replace("<br>","<br/>");
        }

        #region Skills Logic
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
                        skill.Image.Name = s.Replace("|img", "").Replace("=", "").TrimStart(' ');
                        skill.Image.Uri = skill.Image.Name;
                    }
                    else if (s.Contains("|name"))
                    {
                        skill.Name = s.Replace("|name", "").Replace("=", "").TrimStart(' ');
                    }
                    else if (s.Contains("|rank"))
                    {
                        skill.Rank = s.Replace("|rank", "").Replace("=", "").TrimStart(' ');
                    }
                    else if (s.Contains("|effect"))
                    {
                        skill.Effect = FixString(s).Replace("|effect", "").Replace("=", "").TrimStart(' ').Replace("<br/>", ",").Replace(", ",",").Split(',');
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
                    skill.ServantsThatHaveThisSkill = s.Replace(@"{{", "").Replace(@"}}", ",").TrimEnd(',').Replace("|servanticons", "").Replace("=", "").TrimStart(' ').Replace(", ", ",").Split(',');
                }
                else if (s.Contains($"leveleffect"))
                {
                    if (int.TryParse(s[1].ToString(), out int a))
                        lastLevelEffect = s;

                    skill.LevelEffects.Add(new LevelEffect10 { LevelEffectName = s.Replace($"{GetStartPart()}leveleffect", "").Replace("=", "").TrimStart(' ') });
                }
                else if (s.Contains($"{GetStartPart()}l1 "))
                {
                    skill.LevelEffects[skill.LevelEffects.Count - 1].Level1Effect.EffectStrength = s.Replace($"{GetStartPart()}l1", "").Replace("=", "").TrimStart(' ');
                }
                else if (s.Contains($"{GetStartPart()}l2"))
                {
                    skill.LevelEffects[skill.LevelEffects.Count - 1].Level2Effect.EffectStrength = s.Replace($"{GetStartPart()}l2", "").Replace("=", "").TrimStart(' ');
                }
                else if (s.Contains($"{GetStartPart()}l3"))
                {
                    skill.LevelEffects[skill.LevelEffects.Count - 1].Level3Effect.EffectStrength = s.Replace($"{GetStartPart()}l3", "").Replace("=", "").TrimStart(' ');
                }
                else if (s.Contains($"{GetStartPart()}l4"))
                {
                    skill.LevelEffects[skill.LevelEffects.Count - 1].Level4Effect.EffectStrength = s.Replace($"{GetStartPart()}l4", "").Replace("=", "").TrimStart(' ');
                }
                else if (s.Contains($"{GetStartPart()}l5"))
                {
                    skill.LevelEffects[skill.LevelEffects.Count - 1].Level5Effect.EffectStrength = s.Replace($"{GetStartPart()}l5", "").Replace("=", "").TrimStart(' ');
                }
                else if (s.Contains($"{GetStartPart()}l6"))
                {
                    skill.LevelEffects[skill.LevelEffects.Count - 1].Level6Effect.EffectStrength = s.Replace($"{GetStartPart()}l6", "").Replace("=", "").TrimStart(' ');
                }
                else if (s.Contains($"{GetStartPart()}l7"))
                {
                    skill.LevelEffects[skill.LevelEffects.Count - 1].Level7Effect.EffectStrength = s.Replace($"{GetStartPart()}l7", "").Replace("=", "").TrimStart(' ');
                }
                else if (s.Contains($"{GetStartPart()}l8"))
                {
                    skill.LevelEffects[skill.LevelEffects.Count - 1].Level8Effect.EffectStrength = s.Replace($"{GetStartPart()}l8", "").Replace("=", "").TrimStart(' ');
                }
                else if (s.Contains($"{GetStartPart()}l9"))
                {
                    skill.LevelEffects[skill.LevelEffects.Count - 1].Level9Effect.EffectStrength = s.Replace($"{GetStartPart()}l9", "").Replace("=", "").TrimStart(' ');
                }
                else if (s.Contains($"{GetStartPart()}l10"))
                {
                    skill.LevelEffects[skill.LevelEffects.Count - 1].Level10Effect.EffectStrength = s.Replace($"{GetStartPart()}l10", "").Replace("=", "").TrimStart(' ');
                }
                else if (s.Contains("|c1 "))
                {
                    skill.LevelEffects[skill.LevelEffects.Count - 1].Level1Effect.Cooldown = s.Replace("|c1", "").Replace("=", "").TrimStart(' ');
                }
                else if (s.Contains("|c2"))
                {
                    skill.LevelEffects[skill.LevelEffects.Count - 1].Level2Effect.Cooldown = s.Replace("|c2", "").Replace("=", "").TrimStart(' ');
                }
                else if (s.Contains("|c3"))
                {
                    skill.LevelEffects[skill.LevelEffects.Count - 1].Level3Effect.Cooldown = s.Replace("|c3", "").Replace("=", "").TrimStart(' ');
                }
                else if (s.Contains("|c4"))
                {
                    skill.LevelEffects[skill.LevelEffects.Count - 1].Level4Effect.Cooldown = s.Replace("|c4", "").Replace("=", "").TrimStart(' ');
                }
                else if (s.Contains("|c5"))
                {
                    skill.LevelEffects[skill.LevelEffects.Count - 1].Level5Effect.Cooldown = s.Replace("|c5", "").Replace("=", "").TrimStart(' ');
                }
                else if (s.Contains("|c6"))
                {
                    skill.LevelEffects[skill.LevelEffects.Count - 1].Level6Effect.Cooldown = s.Replace("|c6", "").Replace("=", "").TrimStart(' ');
                }
                else if (s.Contains("|c7"))
                {
                    skill.LevelEffects[skill.LevelEffects.Count - 1].Level7Effect.Cooldown = s.Replace("|c7", "").Replace("=", "").TrimStart(' ');
                }
                else if (s.Contains("|c8"))
                {
                    skill.LevelEffects[skill.LevelEffects.Count - 1].Level8Effect.Cooldown = s.Replace("|c8", "").Replace("=", "").TrimStart(' ');
                }
                else if (s.Contains("|c9"))
                {
                    skill.LevelEffects[skill.LevelEffects.Count - 1].Level9Effect.Cooldown = s.Replace("|c9", "").Replace("=", "").TrimStart(' ');
                }
                else if (s.Contains("|c10"))
                {
                    skill.LevelEffects[skill.LevelEffects.Count - 1].Level10Effect.Cooldown = s.Replace("|c10", "").Replace("=", "").TrimStart(' ');
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
        #endregion

        public static Item GetItem(string itemName)
        {
            bool DoingLocationLogic = false;
            Item item = null;
            Item ItemToRemoveFromCache = null;
            foreach (HtmlNode col in new HtmlWeb().Load($"https://fategrandorder.fandom.com/wiki/{itemName}?action=edit").DocumentNode.SelectNodes("//textarea"))
            {
                //For in case we put the person in wrong
                if (string.IsNullOrEmpty(col.InnerText))
                    return null;

                item = new Item(col.InnerText, itemName);

                if (FateGrandOrderPersonCache.Items == null)
                {
                    FateGrandOrderPersonCache.Items = new List<Item>();
                }
                foreach (Item itemC in FateGrandOrderPersonCache.Items)
                {
                    if (item.GeneratedWith == itemC.GeneratedWith && itemC.EnglishName == item.EnglishName)
                    {
#if DEBUG
                        itemC.FromCache = true;
#endif
                        return itemC;
                    }
                    else if (item.GeneratedWith != itemC.GeneratedWith && item.EnglishName == itemC.EnglishName)
                    {
                        ItemToRemoveFromCache = itemC;
                        break;
                    }
                }

                var resultString = Regex.Split(col.InnerText, @"\n");

                foreach (string s in resultString)
                {
                    if (s == "}}" || FixString(s) == "}}</onlyinclude>" && DoingLocationLogic)
                    {
                        DoingLocationLogic = false;
                    }
                    else if (DoingLocationLogic)
                    {
                        if (!string.IsNullOrWhiteSpace(s) && FixString(s) != "<tabber>" && FixString(s) != "</tabber>")
                        {
                            if (s[s.Length - 1] == '=')
                            {
                                item.DropLocations[item.DropLocations.Count - 1].Category = s.Replace("=", "");
                            }
                            else if (s == "|-|")
                            {
                                item.DropLocations.Add(new ItemDropLocationList());
                            }
                            else
                            {
                                try
                                {
                                    var thing = FixString(s).Replace("[[", "[").Replace("]]", "]").Replace("<br/>", "").Split(']');
                                    item.DropLocations[item.DropLocations.Count - 1].DropLocations.Add(new ItemDropLocation
                                    {
                                        Location = thing[0].Replace("[", ""),
                                        PossibleDrops = thing[1].Replace("[", ""),
                                        APCost = thing[2].Replace("[", "")
                                    });
                                }
                                catch(Exception e) { Console.WriteLine($"Looks like something happened while filling up a item stat: {e}"); }
                            }
                        }
                    }
                    else if (s.Contains("|jpName"))
                    {
                        item.JapaneseName = s.Replace("|jpName", "").Replace("=", "").TrimStart(' ');
                    }
                    else if (s.Contains("|image"))
                    {
                        item.ItemImage = new ImageInfomation();
                        item.ItemImage.Name = s.Replace("|image", "").Replace("=", "").TrimStart(' ');
                        item.ItemImage.Uri = item.ItemImage.Name;
                    }
                    else if (s.Contains("|enemy"))
                    {
                        var enemys = s.Replace("|enemy", "").Replace("=", "").Replace("[[","").Replace("]]", "").Replace(" ","").Split('/');
                        if (enemys != null && enemys.Length > 0) item.EnemiesThatDroppedThis = new List<Enemy>();
                        foreach (string enemy in enemys)
                        {
                            item.EnemiesThatDroppedThis.Add(GetEnemy(enemy));
                        }
                    }
                    else if (s.Contains("|jdesc"))
                    {
                        item.JapaneseDescription = s.Replace("|jdesc", "").Replace("=", "").TrimStart(' ');
                    }
                    else if (s.Contains("|desc"))
                    {
                        item.EnglishDescription = s.Replace("|desc", "").Replace("=", "").TrimStart(' ');
                    }
                    else if (s.Contains("|location"))
                    {
                        item.DropLocations = new List<ItemDropLocationList>();
                        item.DropLocations.Add(new ItemDropLocationList());
                        DoingLocationLogic = true;
                    }
                }
            }
            if (ItemToRemoveFromCache != null)
            {
                FateGrandOrderPersonCache.Items.Remove(ItemToRemoveFromCache);
                FateGrandOrderPersonCache.Items.Add(item);
            }
            else if (!FateGrandOrderPersonCache.Items.Contains(item))
            {
                FateGrandOrderPersonCache.Items.Add(item);
            }
            return item;
        }

        public static Enemy GetEnemy(string enemyName)
        {
            Enemy enemy = new Enemy();
            return enemy;
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
            bool GettingAscension = false;
            bool GettingSkillReinforcement = false;
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
                        if (!string.IsNullOrWhiteSpace(s) && s[s.Length - 1] == '=')
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
                                fateGrandOrderPerson.PassiveSkills[fateGrandOrderPerson.PassiveSkills.Count - 1].PassiveSkills[fateGrandOrderPerson.PassiveSkills[fateGrandOrderPerson.PassiveSkills.Count - 1].PassiveSkills.Count - 1].Image.Name = s.Replace("|img", "").Replace("=", "").TrimStart(' ');
                                fateGrandOrderPerson.PassiveSkills[fateGrandOrderPerson.PassiveSkills.Count - 1].PassiveSkills[fateGrandOrderPerson.PassiveSkills[fateGrandOrderPerson.PassiveSkills.Count - 1].PassiveSkills.Count - 1].Image.Uri = fateGrandOrderPerson.PassiveSkills[fateGrandOrderPerson.PassiveSkills.Count - 1].PassiveSkills[fateGrandOrderPerson.PassiveSkills[fateGrandOrderPerson.PassiveSkills.Count - 1].PassiveSkills.Count - 1].Image.Name;
                            }
                            else
                            {
                                fateGrandOrderPerson.PassiveSkills[fateGrandOrderPerson.PassiveSkills.Count - 1].PassiveSkills[fateGrandOrderPerson.PassiveSkills[fateGrandOrderPerson.PassiveSkills.Count - 1].PassiveSkills.Count - 1].Image.Name = s.Replace($"|img{PassiveSkillsCount}", "").Replace("=", "").TrimStart(' ');
                                fateGrandOrderPerson.PassiveSkills[fateGrandOrderPerson.PassiveSkills.Count - 1].PassiveSkills[fateGrandOrderPerson.PassiveSkills[fateGrandOrderPerson.PassiveSkills.Count - 1].PassiveSkills.Count - 1].Image.Uri = fateGrandOrderPerson.PassiveSkills[fateGrandOrderPerson.PassiveSkills.Count - 1].PassiveSkills[fateGrandOrderPerson.PassiveSkills[fateGrandOrderPerson.PassiveSkills.Count - 1].PassiveSkills.Count - 1].Image.Name;
                            }
                        }
                        else if (s.Contains("|name"))
                        {
                            if (PassiveSkillsCount == 1)
                                fateGrandOrderPerson.PassiveSkills[fateGrandOrderPerson.PassiveSkills.Count - 1].PassiveSkills[fateGrandOrderPerson.PassiveSkills[fateGrandOrderPerson.PassiveSkills.Count - 1].PassiveSkills.Count - 1].Name = s.Replace("|name", "").Replace("=", "").TrimStart(' ');
                            else
                                fateGrandOrderPerson.PassiveSkills[fateGrandOrderPerson.PassiveSkills.Count - 1].PassiveSkills[fateGrandOrderPerson.PassiveSkills[fateGrandOrderPerson.PassiveSkills.Count - 1].PassiveSkills.Count - 1].Name = s.Replace($"|name{PassiveSkillsCount}", "").Replace("=", "").TrimStart(' ');
                        }
                        else if (s.Contains("|rank"))
                        {
                            if (PassiveSkillsCount == 1)
                                fateGrandOrderPerson.PassiveSkills[fateGrandOrderPerson.PassiveSkills.Count - 1].PassiveSkills[fateGrandOrderPerson.PassiveSkills[fateGrandOrderPerson.PassiveSkills.Count - 1].PassiveSkills.Count - 1].Rank = s.Replace("|rank", "").Replace("=", "").TrimStart(' ');
                            else
                                fateGrandOrderPerson.PassiveSkills[fateGrandOrderPerson.PassiveSkills.Count - 1].PassiveSkills[fateGrandOrderPerson.PassiveSkills[fateGrandOrderPerson.PassiveSkills.Count - 1].PassiveSkills.Count - 1].Rank = s.Replace($"|rank{PassiveSkillsCount}", "").Replace("=", "").TrimStart(' ');
                        }
                        else if (s.Contains("|effect"))
                        {
                            if (PassiveSkillsCount == 1)
                                fateGrandOrderPerson.PassiveSkills[fateGrandOrderPerson.PassiveSkills.Count - 1].PassiveSkills[fateGrandOrderPerson.PassiveSkills[fateGrandOrderPerson.PassiveSkills.Count - 1].PassiveSkills.Count - 1].Effect = s.Replace("|effect", "").Replace("=", "").TrimStart(' ').Split(',');
                            else
                                fateGrandOrderPerson.PassiveSkills[fateGrandOrderPerson.PassiveSkills.Count - 1].PassiveSkills[fateGrandOrderPerson.PassiveSkills[fateGrandOrderPerson.PassiveSkills.Count - 1].PassiveSkills.Count - 1].Effect = s.Replace($"|effect{PassiveSkillsCount}", "").Replace("=", "").TrimStart(' ').Replace(", ",",").Split(',');
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
                            if(s.IndexOf("|") != -1)
                                fateGrandOrderPerson.ActiveSkills[fateGrandOrderPerson.ActiveSkills.Count - 1].Name = s.Remove(s.IndexOf("|")).Replace(@"{{:","");
                            else
                                fateGrandOrderPerson.ActiveSkills[fateGrandOrderPerson.ActiveSkills.Count - 1].Name = s.Replace(@"{{:", "").Replace("}}","");
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
                            fateGrandOrderPerson.NoblePhantasms[fateGrandOrderPerson.NoblePhantasms.Count - 1].NoblePhantasm.Name = FixString(s).Replace("|name", "").Replace("=", "").TrimStart(' ').Replace("<br/>","\n");
                            if (s.Contains("Video"))
                            {
                                fateGrandOrderPerson.NoblePhantasms[fateGrandOrderPerson.NoblePhantasms.Count - 1].NoblePhantasm.IsVideo = true;
                                fateGrandOrderPerson.NoblePhantasms[fateGrandOrderPerson.NoblePhantasms.Count - 1].NoblePhantasm.VideoInfomation = new VideoInfomation();
                            }
                        }
                        else if (s.Contains("|rank"))
                        {
                            fateGrandOrderPerson.NoblePhantasms[fateGrandOrderPerson.NoblePhantasms.Count - 1].NoblePhantasm.Rank = s.Replace("|rank", "").Replace("=", "").TrimStart(' ');
                        }
                        else if (s.Contains("|classification"))
                        {
                            fateGrandOrderPerson.NoblePhantasms[fateGrandOrderPerson.NoblePhantasms.Count - 1].NoblePhantasm.Classification = s.Replace("|classification", "").Replace("=", "").TrimStart(' ');
                        }
                        else if (s.Contains("|type"))
                        {
                            fateGrandOrderPerson.NoblePhantasms[fateGrandOrderPerson.NoblePhantasms.Count - 1].NoblePhantasm.Type = s.Replace("|type", "").Replace("=", "").TrimStart(' ');
                        }
                        else if (s.Contains("|hitcount"))
                        {
                            fateGrandOrderPerson.NoblePhantasms[fateGrandOrderPerson.NoblePhantasms.Count - 1].NoblePhantasm.HitCount = s.Replace("|hitcount", "").Replace("=", "").TrimStart(' ');
                        }
                        else if (s.Contains("|effect"))
                        {
                            fateGrandOrderPerson.NoblePhantasms[fateGrandOrderPerson.NoblePhantasms.Count - 1].NoblePhantasm.Effects = FixString(s).Replace("|effect", "").Replace("=", "").TrimStart(' ').Replace("<br/>", "").Replace(", ",",").Split(',');
                        }
                        else if (s.Contains("|overchargeeffect"))
                        {
                            fateGrandOrderPerson.NoblePhantasms[fateGrandOrderPerson.NoblePhantasms.Count - 1].NoblePhantasm.OverChargeEffect = FixString(s).Replace("|overchargeeffect", "").Replace("=", "").TrimStart(' ').Replace("<br/>", "").Replace(", ",",").Split(',');
                        }
                        else if (s.Contains("|leveleffect"))
                        {
                            fateGrandOrderPerson.NoblePhantasms[fateGrandOrderPerson.NoblePhantasms.Count - 1].NoblePhantasm.LevelEffect = new LevelEffect();
                            fateGrandOrderPerson.NoblePhantasms[fateGrandOrderPerson.NoblePhantasms.Count - 1].NoblePhantasm.LevelEffect.Name = s.Replace("|leveleffect", "").Replace("=", "").TrimStart(' ');
                        }
                        else if (s.Contains("|l1"))
                        {
                            fateGrandOrderPerson.NoblePhantasms[fateGrandOrderPerson.NoblePhantasms.Count - 1].NoblePhantasm.LevelEffect.NPLevel1 = s.Replace("|l1", "").Replace("=", "").TrimStart(' ');
                        }
                        else if (s.Contains("|l2"))
                        {
                            fateGrandOrderPerson.NoblePhantasms[fateGrandOrderPerson.NoblePhantasms.Count - 1].NoblePhantasm.LevelEffect.NPLevel2 = s.Replace("|l2", "").Replace("=", "").TrimStart(' ');
                        }
                        else if (s.Contains("|l3"))
                        {
                            fateGrandOrderPerson.NoblePhantasms[fateGrandOrderPerson.NoblePhantasms.Count - 1].NoblePhantasm.LevelEffect.NPLevel3 = s.Replace("|l3", "").Replace("=", "").TrimStart(' ');
                        }
                        else if (s.Contains("|l4"))
                        {
                            fateGrandOrderPerson.NoblePhantasms[fateGrandOrderPerson.NoblePhantasms.Count - 1].NoblePhantasm.LevelEffect.NPLevel4 = s.Replace("|l4", "").Replace("=", "").TrimStart(' ');
                        }
                        else if (s.Contains("|l5"))
                        {
                            fateGrandOrderPerson.NoblePhantasms[fateGrandOrderPerson.NoblePhantasms.Count - 1].NoblePhantasm.LevelEffect.NPLevel5 = s.Replace("|l5", "").Replace("=", "").TrimStart(' ');
                        }
                        else if (s.Contains("|chargeeffect"))
                        {
                            fateGrandOrderPerson.NoblePhantasms[fateGrandOrderPerson.NoblePhantasms.Count - 1].NoblePhantasm.ChargeEffect = new ChargeEffect();
                            fateGrandOrderPerson.NoblePhantasms[fateGrandOrderPerson.NoblePhantasms.Count - 1].NoblePhantasm.ChargeEffect.Name = s.Replace("|chargeeffect", "").Replace("=", "").TrimStart(' ');
                        }
                        else if (s.Contains("|c1"))
                        {
                            fateGrandOrderPerson.NoblePhantasms[fateGrandOrderPerson.NoblePhantasms.Count - 1].NoblePhantasm.ChargeEffect.NPLevel1 = s.Replace("|c1", "").Replace("=", "").TrimStart(' ');
                        }
                        else if (s.Contains("|c2"))
                        {
                            fateGrandOrderPerson.NoblePhantasms[fateGrandOrderPerson.NoblePhantasms.Count - 1].NoblePhantasm.ChargeEffect.NPLevel2 = s.Replace("|c2", "").Replace("=", "").TrimStart(' ');
                        }
                        else if (s.Contains("|c3"))
                        {
                            fateGrandOrderPerson.NoblePhantasms[fateGrandOrderPerson.NoblePhantasms.Count - 1].NoblePhantasm.ChargeEffect.NPLevel3 = s.Replace("|c3", "").Replace("=", "").TrimStart(' ');
                        }
                        else if (s.Contains("|c4"))
                        {
                            fateGrandOrderPerson.NoblePhantasms[fateGrandOrderPerson.NoblePhantasms.Count - 1].NoblePhantasm.ChargeEffect.NPLevel4 = s.Replace("|c4", "").Replace("=", "").TrimStart(' ');
                        }
                        else if (s.Contains("|c5"))
                        {
                            fateGrandOrderPerson.NoblePhantasms[fateGrandOrderPerson.NoblePhantasms.Count - 1].NoblePhantasm.ChargeEffect.NPLevel5 = s.Replace("|c5", "").Replace("=", "").TrimStart(' ');
                        }
                    }
                    #endregion

                    #region Ascension
                    if (GettingAscension)
                    {
                        if (s.Length >= 3 && s.Remove(3) == "|11")
                        {
                            fateGrandOrderPerson.Ascension.Ascension1 = new AscensionSkillReinforcement();
                            fateGrandOrderPerson.Ascension.Ascension1.AscensionNumber = "1";
                            fateGrandOrderPerson.Ascension.Ascension1.Item1 = new Item();
                            fateGrandOrderPerson.Ascension.Ascension1.Item1.EnglishName = s.Replace("|11", "").Replace("=", "").TrimStart(' ').Replace("{{Inum|{{", "").Replace("{{", "").Replace("}}","");
                            if (fateGrandOrderPerson.Ascension.Ascension1.Item1.EnglishName.IndexOf('|') != -1)
                                fateGrandOrderPerson.Ascension.Ascension1.Item1 = GetItem(fateGrandOrderPerson.Ascension.Ascension1.Item1.EnglishName.Remove(fateGrandOrderPerson.Ascension.Ascension1.Item1.EnglishName.IndexOf('|')));
                            else
                                fateGrandOrderPerson.Ascension.Ascension1.Item1 = GetItem(fateGrandOrderPerson.Ascension.Ascension1.Item1.EnglishName);
                        }
                        else if (s.Length >= 3 && s.Remove(3) == "|12")
                        {
                            fateGrandOrderPerson.Ascension.Ascension1.Item2 = new Item();
                            fateGrandOrderPerson.Ascension.Ascension1.Item2.EnglishName = s.Replace("|12", "").Replace("=", "").TrimStart(' ').Replace("{{Inum|{{", "").Replace("{{", "").Replace("}}","");
                            if (fateGrandOrderPerson.Ascension.Ascension1.Item2.EnglishName.IndexOf('|') != -1)
                                fateGrandOrderPerson.Ascension.Ascension1.Item2 = GetItem(fateGrandOrderPerson.Ascension.Ascension1.Item2.EnglishName.Remove(fateGrandOrderPerson.Ascension.Ascension1.Item2.EnglishName.IndexOf('|')));
                            else
                                fateGrandOrderPerson.Ascension.Ascension1.Item2 = GetItem(fateGrandOrderPerson.Ascension.Ascension1.Item2.EnglishName);
                        }
                        else if (s.Length >= 3 && s.Remove(3) == "|13")
                        {
                            fateGrandOrderPerson.Ascension.Ascension1.Item3 = new Item();
                            fateGrandOrderPerson.Ascension.Ascension1.Item3.EnglishName = s.Replace("|13", "").Replace("=", "").TrimStart(' ').Replace("{{Inum|{{", "").Replace("{{", "").Replace("}}","");
                            if (fateGrandOrderPerson.Ascension.Ascension1.Item3.EnglishName.IndexOf('|') != -1)
                                fateGrandOrderPerson.Ascension.Ascension1.Item3 = GetItem(fateGrandOrderPerson.Ascension.Ascension1.Item3.EnglishName.Remove(fateGrandOrderPerson.Ascension.Ascension1.Item3.EnglishName.IndexOf('|')));
                            else
                                fateGrandOrderPerson.Ascension.Ascension1.Item3 = GetItem(fateGrandOrderPerson.Ascension.Ascension1.Item3.EnglishName);
                        }
                        else if (s.Length >= 3 && s.Remove(3) == "|14")
                        {
                            fateGrandOrderPerson.Ascension.Ascension1.Item4 = new Item();
                            fateGrandOrderPerson.Ascension.Ascension1.Item4.EnglishName = s.Replace("|14", "").Replace("=", "").TrimStart(' ').Replace("{{Inum|{{", "").Replace("{{", "").Replace("}}","");
                            if (fateGrandOrderPerson.Ascension.Ascension1.Item4.EnglishName.IndexOf('|') != -1)
                                fateGrandOrderPerson.Ascension.Ascension1.Item4 = GetItem(fateGrandOrderPerson.Ascension.Ascension1.Item4.EnglishName.Remove(fateGrandOrderPerson.Ascension.Ascension1.Item4.EnglishName.IndexOf('|')));
                            else
                                fateGrandOrderPerson.Ascension.Ascension1.Item4 = GetItem(fateGrandOrderPerson.Ascension.Ascension1.Item4.EnglishName);
                        }
                        else if (s.Length >= 3 && s.Remove(3) == "|21")
                        {
                            fateGrandOrderPerson.Ascension.Ascension2 = new AscensionSkillReinforcement();
                            fateGrandOrderPerson.Ascension.Ascension2.AscensionNumber = "2";
                            fateGrandOrderPerson.Ascension.Ascension2.Item1 = new Item();
                            fateGrandOrderPerson.Ascension.Ascension2.Item1.EnglishName = s.Replace("|21", "").Replace("=", "").TrimStart(' ').Replace("{{Inum|{{", "").Replace("{{", "").Replace("}}", "");
                            if (fateGrandOrderPerson.Ascension.Ascension2.Item1.EnglishName.IndexOf('|') != -1)
                                fateGrandOrderPerson.Ascension.Ascension2.Item1 = GetItem(fateGrandOrderPerson.Ascension.Ascension2.Item1.EnglishName.Remove(fateGrandOrderPerson.Ascension.Ascension2.Item1.EnglishName.IndexOf('|')));
                            else
                                fateGrandOrderPerson.Ascension.Ascension2.Item1 = GetItem(fateGrandOrderPerson.Ascension.Ascension2.Item1.EnglishName);
                        }
                        else if (s.Length >= 3 && s.Remove(3) == "|22")
                        {
                            fateGrandOrderPerson.Ascension.Ascension2.Item2 = new Item();
                            fateGrandOrderPerson.Ascension.Ascension2.Item2.EnglishName = s.Replace("|22", "").Replace("=", "").TrimStart(' ').Replace("{{Inum|{{", "").Replace("{{", "").Replace("}}", "");
                            if (fateGrandOrderPerson.Ascension.Ascension2.Item2.EnglishName.IndexOf('|') != -1)
                                fateGrandOrderPerson.Ascension.Ascension2.Item2 = GetItem(fateGrandOrderPerson.Ascension.Ascension2.Item2.EnglishName.Remove(fateGrandOrderPerson.Ascension.Ascension2.Item2.EnglishName.IndexOf('|')));
                            else
                                fateGrandOrderPerson.Ascension.Ascension2.Item2 = GetItem(fateGrandOrderPerson.Ascension.Ascension2.Item2.EnglishName);
                        }
                        else if (s.Length >= 3 && s.Remove(3) == "|23")
                        {
                            fateGrandOrderPerson.Ascension.Ascension2.Item3 = new Item();
                            fateGrandOrderPerson.Ascension.Ascension2.Item3.EnglishName = s.Replace("|23", "").Replace("=", "").TrimStart(' ').Replace("{{Inum|{{", "").Replace("{{", "").Replace("}}", "");
                            if (fateGrandOrderPerson.Ascension.Ascension2.Item3.EnglishName.IndexOf('|') != -1)
                                fateGrandOrderPerson.Ascension.Ascension2.Item3 = GetItem(fateGrandOrderPerson.Ascension.Ascension2.Item3.EnglishName.Remove(fateGrandOrderPerson.Ascension.Ascension2.Item3.EnglishName.IndexOf('|')));
                            else
                                fateGrandOrderPerson.Ascension.Ascension2.Item3 = GetItem(fateGrandOrderPerson.Ascension.Ascension2.Item3.EnglishName);
                        }
                        else if (s.Length >= 3 && s.Remove(3) == "|24")
                        {
                            fateGrandOrderPerson.Ascension.Ascension2.Item4 = new Item();
                            fateGrandOrderPerson.Ascension.Ascension2.Item4.EnglishName = s.Replace("|24", "").Replace("=", "").TrimStart(' ').Replace("{{Inum|{{", "").Replace("{{", "").Replace("}}", "");
                            if (fateGrandOrderPerson.Ascension.Ascension2.Item4.EnglishName.IndexOf('|') != -1)
                                fateGrandOrderPerson.Ascension.Ascension2.Item4 = GetItem(fateGrandOrderPerson.Ascension.Ascension2.Item4.EnglishName.Remove(fateGrandOrderPerson.Ascension.Ascension2.Item4.EnglishName.IndexOf('|')));
                            else
                                fateGrandOrderPerson.Ascension.Ascension2.Item4 = GetItem(fateGrandOrderPerson.Ascension.Ascension2.Item4.EnglishName);
                        }
                        else if (s.Length >= 3 && s.Remove(3) == "|31")
                        {
                            fateGrandOrderPerson.Ascension.Ascension3 = new AscensionSkillReinforcement();
                            fateGrandOrderPerson.Ascension.Ascension3.AscensionNumber = "3";
                            fateGrandOrderPerson.Ascension.Ascension3.Item1 = new Item();
                            fateGrandOrderPerson.Ascension.Ascension3.Item1.EnglishName = s.Replace("|31", "").Replace("=", "").TrimStart(' ').Replace("{{Inum|{{", "").Replace("{{", "").Replace("}}", "");
                            if (fateGrandOrderPerson.Ascension.Ascension3.Item1.EnglishName.IndexOf('|') != -1)
                                fateGrandOrderPerson.Ascension.Ascension3.Item1 = GetItem(fateGrandOrderPerson.Ascension.Ascension3.Item1.EnglishName.Remove(fateGrandOrderPerson.Ascension.Ascension3.Item1.EnglishName.IndexOf('|')));
                            else
                                fateGrandOrderPerson.Ascension.Ascension3.Item1 = GetItem(fateGrandOrderPerson.Ascension.Ascension3.Item1.EnglishName);
                        }
                        else if (s.Length >= 3 && s.Remove(3) == "|32")
                        {
                            fateGrandOrderPerson.Ascension.Ascension3.Item2 = new Item();
                            fateGrandOrderPerson.Ascension.Ascension3.Item2.EnglishName = s.Replace("|32", "").Replace("=", "").TrimStart(' ').Replace("{{Inum|{{", "").Replace("{{", "").Replace("}}", "");
                            if (fateGrandOrderPerson.Ascension.Ascension3.Item2.EnglishName.IndexOf('|') != -1)
                                fateGrandOrderPerson.Ascension.Ascension3.Item2 = GetItem(fateGrandOrderPerson.Ascension.Ascension3.Item2.EnglishName.Remove(fateGrandOrderPerson.Ascension.Ascension3.Item2.EnglishName.IndexOf('|')));
                            else
                                fateGrandOrderPerson.Ascension.Ascension3.Item2 = GetItem(fateGrandOrderPerson.Ascension.Ascension3.Item2.EnglishName);
                        }
                        else if (s.Length >= 3 && s.Remove(3) == "|33")
                        {
                            fateGrandOrderPerson.Ascension.Ascension3.Item3 = new Item();
                            fateGrandOrderPerson.Ascension.Ascension3.Item3.EnglishName = s.Replace("|33", "").Replace("=", "").TrimStart(' ').Replace("{{Inum|{{", "").Replace("{{", "").Replace("}}", "");
                            if (fateGrandOrderPerson.Ascension.Ascension3.Item3.EnglishName.IndexOf('|') != -1)
                                fateGrandOrderPerson.Ascension.Ascension3.Item3 = GetItem(fateGrandOrderPerson.Ascension.Ascension3.Item3.EnglishName.Remove(fateGrandOrderPerson.Ascension.Ascension3.Item3.EnglishName.IndexOf('|')));
                            else
                                fateGrandOrderPerson.Ascension.Ascension3.Item3 = GetItem(fateGrandOrderPerson.Ascension.Ascension3.Item3.EnglishName);
                        }
                        else if (s.Length >= 3 && s.Remove(3) == "|34")
                        {
                            fateGrandOrderPerson.Ascension.Ascension3.Item4 = new Item();
                            fateGrandOrderPerson.Ascension.Ascension3.Item4.EnglishName = s.Replace("|34", "").Replace("=", "").TrimStart(' ').Replace("{{Inum|{{", "").Replace("{{", "").Replace("}}", "");
                            if (fateGrandOrderPerson.Ascension.Ascension3.Item4.EnglishName.IndexOf('|') != -1)
                                fateGrandOrderPerson.Ascension.Ascension3.Item4 = GetItem(fateGrandOrderPerson.Ascension.Ascension3.Item4.EnglishName.Remove(fateGrandOrderPerson.Ascension.Ascension3.Item4.EnglishName.IndexOf('|')));
                            else
                                fateGrandOrderPerson.Ascension.Ascension3.Item4 = GetItem(fateGrandOrderPerson.Ascension.Ascension3.Item4.EnglishName);
                        }
                        else if (s.Length >= 3 && s.Remove(3) == "|41")
                        {
                            fateGrandOrderPerson.Ascension.Ascension4 = new AscensionSkillReinforcement();
                            fateGrandOrderPerson.Ascension.Ascension4.AscensionNumber = "4";
                            fateGrandOrderPerson.Ascension.Ascension4.Item1 = new Item();
                            fateGrandOrderPerson.Ascension.Ascension4.Item1.EnglishName = s.Replace("|41", "").Replace("=", "").TrimStart(' ').Replace("{{Inum|{{", "").Replace("{{", "").Replace("}}", "");
                            if (fateGrandOrderPerson.Ascension.Ascension4.Item1.EnglishName.IndexOf('|') != -1)
                                fateGrandOrderPerson.Ascension.Ascension4.Item1 = GetItem(fateGrandOrderPerson.Ascension.Ascension4.Item1.EnglishName.Remove(fateGrandOrderPerson.Ascension.Ascension4.Item1.EnglishName.IndexOf('|')));
                            else
                                fateGrandOrderPerson.Ascension.Ascension4.Item1 = GetItem(fateGrandOrderPerson.Ascension.Ascension4.Item1.EnglishName);
                        }
                        else if (s.Length >= 3 && s.Remove(3) == "|42")
                        {
                            fateGrandOrderPerson.Ascension.Ascension4.Item2 = new Item();
                            fateGrandOrderPerson.Ascension.Ascension4.Item2.EnglishName = s.Replace("|42", "").Replace("=", "").TrimStart(' ').Replace("{{Inum|{{", "").Replace("{{", "").Replace("}}", "");
                            if (fateGrandOrderPerson.Ascension.Ascension4.Item2.EnglishName.IndexOf('|') != -1)
                                fateGrandOrderPerson.Ascension.Ascension4.Item2 = GetItem(fateGrandOrderPerson.Ascension.Ascension4.Item2.EnglishName.Remove(fateGrandOrderPerson.Ascension.Ascension4.Item2.EnglishName.IndexOf('|')));
                            else
                                fateGrandOrderPerson.Ascension.Ascension4.Item2 = GetItem(fateGrandOrderPerson.Ascension.Ascension4.Item2.EnglishName);
                        }
                        else if (s.Length >= 3 && s.Remove(3) == "|43")
                        {
                            fateGrandOrderPerson.Ascension.Ascension4.Item3 = new Item();
                            fateGrandOrderPerson.Ascension.Ascension4.Item3.EnglishName = s.Replace("|43", "").Replace("=", "").TrimStart(' ').Replace("{{Inum|{{", "").Replace("{{", "").Replace("}}", "");
                            if (fateGrandOrderPerson.Ascension.Ascension4.Item3.EnglishName.IndexOf('|') != -1)
                                fateGrandOrderPerson.Ascension.Ascension4.Item3 = GetItem(fateGrandOrderPerson.Ascension.Ascension4.Item3.EnglishName.Remove(fateGrandOrderPerson.Ascension.Ascension4.Item3.EnglishName.IndexOf('|')));
                            else
                                fateGrandOrderPerson.Ascension.Ascension4.Item3 = GetItem(fateGrandOrderPerson.Ascension.Ascension4.Item3.EnglishName);
                        }
                        else if (s.Length >= 3 && s.Remove(3) == "|44")
                        {
                            fateGrandOrderPerson.Ascension.Ascension4.Item4 = new Item();
                            fateGrandOrderPerson.Ascension.Ascension4.Item4.EnglishName = s.Replace("|44", "").Replace("=", "").TrimStart(' ').Replace("{{Inum|{{", "").Replace("{{", "").Replace("}}", "");
                            if (fateGrandOrderPerson.Ascension.Ascension4.Item4.EnglishName.IndexOf('|') != -1)
                                fateGrandOrderPerson.Ascension.Ascension4.Item4 = GetItem(fateGrandOrderPerson.Ascension.Ascension4.Item4.EnglishName.Remove(fateGrandOrderPerson.Ascension.Ascension4.Item4.EnglishName.IndexOf('|')));
                            else
                                fateGrandOrderPerson.Ascension.Ascension4.Item4 = GetItem(fateGrandOrderPerson.Ascension.Ascension4.Item4.EnglishName);
                        }
                        else if (s.Length >= 3 && s.Remove(3) == "|51")
                        {
                            fateGrandOrderPerson.Ascension.Ascension5 = new AscensionSkillReinforcement();
                            fateGrandOrderPerson.Ascension.Ascension5.AscensionNumber = "5";
                            fateGrandOrderPerson.Ascension.Ascension5.Item1 = new Item();
                            fateGrandOrderPerson.Ascension.Ascension5.Item1.EnglishName = s.Replace("|51", "").Replace("=", "").TrimStart(' ').Replace("{{Inum|{{", "").Replace("{{", "").Replace("}}", "");
                            if (fateGrandOrderPerson.Ascension.Ascension5.Item1.EnglishName.IndexOf('|') != -1)
                                fateGrandOrderPerson.Ascension.Ascension5.Item1 = GetItem(fateGrandOrderPerson.Ascension.Ascension5.Item1.EnglishName.Remove(fateGrandOrderPerson.Ascension.Ascension5.Item1.EnglishName.IndexOf('|')));
                            else
                                fateGrandOrderPerson.Ascension.Ascension5.Item1 = GetItem(fateGrandOrderPerson.Ascension.Ascension5.Item1.EnglishName);
                        }
                        else if (s.Length >= 3 && s.Remove(3) == "|52")
                        {
                            fateGrandOrderPerson.Ascension.Ascension5.Item2 = new Item();
                            fateGrandOrderPerson.Ascension.Ascension5.Item2.EnglishName = s.Replace("|52", "").Replace("=", "").TrimStart(' ').Replace("{{Inum|{{", "").Replace("{{", "").Replace("}}", "");
                            if (fateGrandOrderPerson.Ascension.Ascension5.Item2.EnglishName.IndexOf('|') != -1)
                                fateGrandOrderPerson.Ascension.Ascension5.Item2 = GetItem(fateGrandOrderPerson.Ascension.Ascension5.Item2.EnglishName.Remove(fateGrandOrderPerson.Ascension.Ascension5.Item2.EnglishName.IndexOf('|')));
                            else
                                fateGrandOrderPerson.Ascension.Ascension5.Item2 = GetItem(fateGrandOrderPerson.Ascension.Ascension5.Item2.EnglishName);
                        }
                        else if (s.Length >= 3 && s.Remove(3) == "|53")
                        {
                            fateGrandOrderPerson.Ascension.Ascension5.Item3 = new Item();
                            fateGrandOrderPerson.Ascension.Ascension5.Item3.EnglishName = s.Replace("|53", "").Replace("=", "").TrimStart(' ').Replace("{{Inum|{{", "").Replace("{{", "").Replace("}}", "");
                            if (fateGrandOrderPerson.Ascension.Ascension5.Item3.EnglishName.IndexOf('|') != -1)
                                fateGrandOrderPerson.Ascension.Ascension5.Item3 = GetItem(fateGrandOrderPerson.Ascension.Ascension5.Item3.EnglishName.Remove(fateGrandOrderPerson.Ascension.Ascension5.Item3.EnglishName.IndexOf('|')));
                            else
                                fateGrandOrderPerson.Ascension.Ascension5.Item3 = GetItem(fateGrandOrderPerson.Ascension.Ascension5.Item3.EnglishName);
                        }
                        else if (s.Length >= 3 && s.Remove(3) == "|54")
                        {
                            fateGrandOrderPerson.Ascension.Ascension5.Item4 = new Item();
                            fateGrandOrderPerson.Ascension.Ascension5.Item4.EnglishName = s.Replace("|54", "").Replace("=", "").TrimStart(' ').Replace("{{Inum|{{", "").Replace("{{", "").Replace("}}", "");
                            if (fateGrandOrderPerson.Ascension.Ascension5.Item4.EnglishName.IndexOf('|') != -1)
                                fateGrandOrderPerson.Ascension.Ascension5.Item4 = GetItem(fateGrandOrderPerson.Ascension.Ascension5.Item4.EnglishName.Remove(fateGrandOrderPerson.Ascension.Ascension5.Item4.EnglishName.IndexOf('|')));
                            else
                                fateGrandOrderPerson.Ascension.Ascension5.Item4 = GetItem(fateGrandOrderPerson.Ascension.Ascension5.Item4.EnglishName);
                        }
                        if (s.Contains("|1qp"))
                        {
                            fateGrandOrderPerson.Ascension.Ascension1.QP = s.Replace("|1qp", "").Replace("=", "").TrimStart(' ').Replace("{{Inum|{{QP}}|","").Replace("}}","");
                        }
                        else if (s.Contains("|2qp"))
                        {
                            fateGrandOrderPerson.Ascension.Ascension2.QP = s.Replace("|2qp", "").Replace("=", "").TrimStart(' ').Replace("{{Inum|{{QP}}|","").Replace("}}","");
                        }
                        else if (s.Contains("|3qp"))
                        {
                            fateGrandOrderPerson.Ascension.Ascension3.QP = s.Replace("|3qp", "").Replace("=", "").TrimStart(' ').Replace("{{Inum|{{QP}}|","").Replace("}}","");
                        }
                        else if (s.Contains("|4qp"))
                        {
                            fateGrandOrderPerson.Ascension.Ascension4.QP = s.Replace("|4qp", "").Replace("=", "").TrimStart(' ').Replace("{{Inum|{{QP}}|","").Replace("}}","");
                        }
                        else if (s.Contains("|5qp"))
                        {
                            fateGrandOrderPerson.Ascension.Ascension5.QP = s.Replace("|5qp", "").Replace("=", "").TrimStart(' ').Replace("{{Inum|{{QP}}|","").Replace("}}","");
                        }
                    }
                    #endregion

                    #region Active Skills
                    if (GettingSkillReinforcement)
                    {
                        if (s.Length >= 3 && s.Remove(3) == "|11")
                        {
                            fateGrandOrderPerson.SkillReinforcement.Ascension1 = new AscensionSkillReinforcement();
                            fateGrandOrderPerson.SkillReinforcement.Ascension1.AscensionNumber = "1";
                            fateGrandOrderPerson.SkillReinforcement.Ascension1.Item1 = new Item();
                            fateGrandOrderPerson.SkillReinforcement.Ascension1.Item1.EnglishName = s.Replace("|11", "").Replace("=", "").TrimStart(' ').Replace("{{Inum|{{", "").Replace("{{", "").Replace("}}", "");
                            if (fateGrandOrderPerson.SkillReinforcement.Ascension1.Item1.EnglishName.IndexOf('|') != -1)
                                fateGrandOrderPerson.SkillReinforcement.Ascension1.Item1 = GetItem(fateGrandOrderPerson.SkillReinforcement.Ascension1.Item1.EnglishName.Remove(fateGrandOrderPerson.SkillReinforcement.Ascension1.Item1.EnglishName.IndexOf('|')));
                            else
                                fateGrandOrderPerson.SkillReinforcement.Ascension1.Item1 = GetItem(fateGrandOrderPerson.SkillReinforcement.Ascension1.Item1.EnglishName);
                        }
                        else if (s.Length >= 3 && s.Remove(3) == "|12")
                        {
                            fateGrandOrderPerson.SkillReinforcement.Ascension1.Item2 = new Item();
                            fateGrandOrderPerson.SkillReinforcement.Ascension1.Item2.EnglishName = s.Replace("|12", "").Replace("=", "").TrimStart(' ').Replace("{{Inum|{{", "").Replace("{{", "").Replace("}}", "");
                            if (fateGrandOrderPerson.SkillReinforcement.Ascension1.Item2.EnglishName.IndexOf('|') != -1)
                                fateGrandOrderPerson.SkillReinforcement.Ascension1.Item2 = GetItem(fateGrandOrderPerson.SkillReinforcement.Ascension1.Item2.EnglishName.Remove(fateGrandOrderPerson.SkillReinforcement.Ascension1.Item2.EnglishName.IndexOf('|')));
                            else
                                fateGrandOrderPerson.SkillReinforcement.Ascension1.Item2 = GetItem(fateGrandOrderPerson.SkillReinforcement.Ascension1.Item2.EnglishName);
                        }
                        else if (s.Length >= 3 && s.Remove(3) == "|13")
                        {
                            fateGrandOrderPerson.SkillReinforcement.Ascension1.Item3 = new Item();
                            fateGrandOrderPerson.SkillReinforcement.Ascension1.Item3.EnglishName = s.Replace("|13", "").Replace("=", "").TrimStart(' ').Replace("{{Inum|{{", "").Replace("{{", "").Replace("}}", "");
                            if (fateGrandOrderPerson.SkillReinforcement.Ascension1.Item3.EnglishName.IndexOf('|') != -1)
                                fateGrandOrderPerson.SkillReinforcement.Ascension1.Item3 = GetItem(fateGrandOrderPerson.SkillReinforcement.Ascension1.Item3.EnglishName.Remove(fateGrandOrderPerson.SkillReinforcement.Ascension1.Item3.EnglishName.IndexOf('|')));
                            else
                                fateGrandOrderPerson.SkillReinforcement.Ascension1.Item3 = GetItem(fateGrandOrderPerson.SkillReinforcement.Ascension1.Item3.EnglishName);
                        }
                        else if (s.Length >= 3 && s.Remove(3) == "|14")
                        {
                            fateGrandOrderPerson.SkillReinforcement.Ascension1.Item4 = new Item();
                            fateGrandOrderPerson.SkillReinforcement.Ascension1.Item4.EnglishName = s.Replace("|14", "").Replace("=", "").TrimStart(' ').Replace("{{Inum|{{", "").Replace("{{", "").Replace("}}", "");
                            if (fateGrandOrderPerson.SkillReinforcement.Ascension1.Item4.EnglishName.IndexOf('|') != -1)
                                fateGrandOrderPerson.SkillReinforcement.Ascension1.Item4 = GetItem(fateGrandOrderPerson.SkillReinforcement.Ascension1.Item4.EnglishName.Remove(fateGrandOrderPerson.SkillReinforcement.Ascension1.Item4.EnglishName.IndexOf('|')));
                            else
                                fateGrandOrderPerson.SkillReinforcement.Ascension1.Item4 = GetItem(fateGrandOrderPerson.SkillReinforcement.Ascension1.Item4.EnglishName);
                        }
                        else if (s.Length >= 3 && s.Remove(3) == "|21")
                        {
                            fateGrandOrderPerson.SkillReinforcement.Ascension2 = new AscensionSkillReinforcement();
                            fateGrandOrderPerson.SkillReinforcement.Ascension2.AscensionNumber = "2";
                            fateGrandOrderPerson.SkillReinforcement.Ascension2.Item1 = new Item();
                            fateGrandOrderPerson.SkillReinforcement.Ascension2.Item1.EnglishName = s.Replace("|21", "").Replace("=", "").TrimStart(' ').Replace("{{Inum|{{", "").Replace("{{", "").Replace("}}", "");
                            if (fateGrandOrderPerson.SkillReinforcement.Ascension2.Item1.EnglishName.IndexOf('|') != -1)
                                fateGrandOrderPerson.SkillReinforcement.Ascension2.Item1 = GetItem(fateGrandOrderPerson.SkillReinforcement.Ascension2.Item1.EnglishName.Remove(fateGrandOrderPerson.SkillReinforcement.Ascension2.Item1.EnglishName.IndexOf('|')));
                            else
                                fateGrandOrderPerson.SkillReinforcement.Ascension2.Item1 = GetItem(fateGrandOrderPerson.SkillReinforcement.Ascension2.Item1.EnglishName);
                        }
                        else if (s.Length >= 3 && s.Remove(3) == "|22")
                        {
                            fateGrandOrderPerson.SkillReinforcement.Ascension2.Item2 = new Item();
                            fateGrandOrderPerson.SkillReinforcement.Ascension2.Item2.EnglishName = s.Replace("|22", "").Replace("=", "").TrimStart(' ').Replace("{{Inum|{{", "").Replace("{{", "").Replace("}}", "");
                            if (fateGrandOrderPerson.SkillReinforcement.Ascension2.Item2.EnglishName.IndexOf('|') != -1)
                                fateGrandOrderPerson.SkillReinforcement.Ascension2.Item2 = GetItem(fateGrandOrderPerson.SkillReinforcement.Ascension2.Item2.EnglishName.Remove(fateGrandOrderPerson.SkillReinforcement.Ascension2.Item2.EnglishName.IndexOf('|')));
                            else
                                fateGrandOrderPerson.SkillReinforcement.Ascension2.Item2 = GetItem(fateGrandOrderPerson.SkillReinforcement.Ascension2.Item2.EnglishName);
                        }
                        else if (s.Length >= 3 && s.Remove(3) == "|23")
                        {
                            fateGrandOrderPerson.SkillReinforcement.Ascension2.Item3 = new Item();
                            fateGrandOrderPerson.SkillReinforcement.Ascension2.Item3.EnglishName = s.Replace("|23", "").Replace("=", "").TrimStart(' ').Replace("{{Inum|{{", "").Replace("{{", "").Replace("}}", "");
                            if (fateGrandOrderPerson.SkillReinforcement.Ascension2.Item3.EnglishName.IndexOf('|') != -1)
                                fateGrandOrderPerson.SkillReinforcement.Ascension2.Item3 = GetItem(fateGrandOrderPerson.SkillReinforcement.Ascension2.Item3.EnglishName.Remove(fateGrandOrderPerson.SkillReinforcement.Ascension2.Item3.EnglishName.IndexOf('|')));
                            else
                                fateGrandOrderPerson.SkillReinforcement.Ascension2.Item3 = GetItem(fateGrandOrderPerson.SkillReinforcement.Ascension2.Item3.EnglishName);
                        }
                        else if (s.Length >= 3 && s.Remove(3) == "|24")
                        {
                            fateGrandOrderPerson.SkillReinforcement.Ascension2.Item4 = new Item();
                            fateGrandOrderPerson.SkillReinforcement.Ascension2.Item4.EnglishName = s.Replace("|24", "").Replace("=", "").TrimStart(' ').Replace("{{Inum|{{", "").Replace("{{", "").Replace("}}", "");
                            if (fateGrandOrderPerson.SkillReinforcement.Ascension2.Item4.EnglishName.IndexOf('|') != -1)
                                fateGrandOrderPerson.SkillReinforcement.Ascension2.Item4 = GetItem(fateGrandOrderPerson.SkillReinforcement.Ascension2.Item4.EnglishName.Remove(fateGrandOrderPerson.SkillReinforcement.Ascension2.Item4.EnglishName.IndexOf('|')));
                            else
                                fateGrandOrderPerson.SkillReinforcement.Ascension2.Item4 = GetItem(fateGrandOrderPerson.SkillReinforcement.Ascension2.Item4.EnglishName);
                        }
                        else if (s.Length >= 3 && s.Remove(3) == "|31")
                        {
                            fateGrandOrderPerson.SkillReinforcement.Ascension3 = new AscensionSkillReinforcement();
                            fateGrandOrderPerson.SkillReinforcement.Ascension3.AscensionNumber = "3";
                            fateGrandOrderPerson.SkillReinforcement.Ascension3.Item1 = new Item();
                            fateGrandOrderPerson.SkillReinforcement.Ascension3.Item1.EnglishName = s.Replace("|31", "").Replace("=", "").TrimStart(' ').Replace("{{Inum|{{", "").Replace("{{", "").Replace("}}", "");
                            if (fateGrandOrderPerson.SkillReinforcement.Ascension3.Item1.EnglishName.IndexOf('|') != -1)
                                fateGrandOrderPerson.SkillReinforcement.Ascension3.Item1 = GetItem(fateGrandOrderPerson.SkillReinforcement.Ascension3.Item1.EnglishName.Remove(fateGrandOrderPerson.SkillReinforcement.Ascension3.Item1.EnglishName.IndexOf('|')));
                            else
                                fateGrandOrderPerson.SkillReinforcement.Ascension3.Item1 = GetItem(fateGrandOrderPerson.SkillReinforcement.Ascension3.Item1.EnglishName);
                        }
                        else if (s.Length >= 3 && s.Remove(3) == "|32")
                        {
                            fateGrandOrderPerson.SkillReinforcement.Ascension3.Item2 = new Item();
                            fateGrandOrderPerson.SkillReinforcement.Ascension3.Item2.EnglishName = s.Replace("|32", "").Replace("=", "").TrimStart(' ').Replace("{{Inum|{{", "").Replace("{{", "").Replace("}}", "");
                            if (fateGrandOrderPerson.SkillReinforcement.Ascension3.Item2.EnglishName.IndexOf('|') != -1)
                                fateGrandOrderPerson.SkillReinforcement.Ascension3.Item2 = GetItem(fateGrandOrderPerson.SkillReinforcement.Ascension3.Item2.EnglishName.Remove(fateGrandOrderPerson.SkillReinforcement.Ascension3.Item2.EnglishName.IndexOf('|')));
                            else
                                fateGrandOrderPerson.SkillReinforcement.Ascension3.Item2 = GetItem(fateGrandOrderPerson.SkillReinforcement.Ascension3.Item2.EnglishName);
                        }
                        else if (s.Length >= 3 && s.Remove(3) == "|33")
                        {
                            fateGrandOrderPerson.SkillReinforcement.Ascension3.Item3 = new Item();
                            fateGrandOrderPerson.SkillReinforcement.Ascension3.Item3.EnglishName = s.Replace("|33", "").Replace("=", "").TrimStart(' ').Replace("{{Inum|{{", "").Replace("{{", "").Replace("}}", "");
                            if (fateGrandOrderPerson.SkillReinforcement.Ascension3.Item3.EnglishName.IndexOf('|') != -1)
                                fateGrandOrderPerson.SkillReinforcement.Ascension3.Item3 = GetItem(fateGrandOrderPerson.SkillReinforcement.Ascension3.Item3.EnglishName.Remove(fateGrandOrderPerson.SkillReinforcement.Ascension3.Item3.EnglishName.IndexOf('|')));
                            else
                                fateGrandOrderPerson.SkillReinforcement.Ascension3.Item3 = GetItem(fateGrandOrderPerson.SkillReinforcement.Ascension3.Item3.EnglishName);
                        }
                        else if (s.Length >= 3 && s.Remove(3) == "|34")
                        {
                            fateGrandOrderPerson.SkillReinforcement.Ascension3.Item4 = new Item();
                            fateGrandOrderPerson.SkillReinforcement.Ascension3.Item4.EnglishName = s.Replace("|34", "").Replace("=", "").TrimStart(' ').Replace("{{Inum|{{", "").Replace("{{", "").Replace("}}", "");
                            if (fateGrandOrderPerson.SkillReinforcement.Ascension3.Item4.EnglishName.IndexOf('|') != -1)
                                fateGrandOrderPerson.SkillReinforcement.Ascension3.Item4 = GetItem(fateGrandOrderPerson.SkillReinforcement.Ascension3.Item4.EnglishName.Remove(fateGrandOrderPerson.SkillReinforcement.Ascension3.Item4.EnglishName.IndexOf('|')));
                            else
                                fateGrandOrderPerson.SkillReinforcement.Ascension3.Item4 = GetItem(fateGrandOrderPerson.SkillReinforcement.Ascension3.Item4.EnglishName);
                        }
                        else if (s.Length >= 3 && s.Remove(3) == "|41")
                        {
                            fateGrandOrderPerson.SkillReinforcement.Ascension4 = new AscensionSkillReinforcement();
                            fateGrandOrderPerson.SkillReinforcement.Ascension4.AscensionNumber = "4";
                            fateGrandOrderPerson.SkillReinforcement.Ascension4.Item1 = new Item();
                            fateGrandOrderPerson.SkillReinforcement.Ascension4.Item1.EnglishName = s.Replace("|41", "").Replace("=", "").TrimStart(' ').Replace("{{Inum|{{", "").Replace("{{", "").Replace("}}", "");
                            if (fateGrandOrderPerson.SkillReinforcement.Ascension4.Item1.EnglishName.IndexOf('|') != -1)
                                fateGrandOrderPerson.SkillReinforcement.Ascension4.Item1 = GetItem(fateGrandOrderPerson.SkillReinforcement.Ascension4.Item1.EnglishName.Remove(fateGrandOrderPerson.SkillReinforcement.Ascension4.Item1.EnglishName.IndexOf('|')));
                            else
                                fateGrandOrderPerson.SkillReinforcement.Ascension4.Item1 = GetItem(fateGrandOrderPerson.SkillReinforcement.Ascension4.Item1.EnglishName);
                        }
                        else if (s.Length >= 3 && s.Remove(3) == "|42")
                        {
                            fateGrandOrderPerson.SkillReinforcement.Ascension4.Item2 = new Item();
                            fateGrandOrderPerson.SkillReinforcement.Ascension4.Item2.EnglishName = s.Replace("|42", "").Replace("=", "").TrimStart(' ').Replace("{{Inum|{{", "").Replace("{{", "").Replace("}}", "");
                            if (fateGrandOrderPerson.SkillReinforcement.Ascension4.Item2.EnglishName.IndexOf('|') != -1)
                                fateGrandOrderPerson.SkillReinforcement.Ascension4.Item2 = GetItem(fateGrandOrderPerson.SkillReinforcement.Ascension4.Item2.EnglishName.Remove(fateGrandOrderPerson.SkillReinforcement.Ascension4.Item2.EnglishName.IndexOf('|')));
                            else
                                fateGrandOrderPerson.SkillReinforcement.Ascension4.Item2 = GetItem(fateGrandOrderPerson.SkillReinforcement.Ascension4.Item2.EnglishName);
                        }
                        else if (s.Length >= 3 && s.Remove(3) == "|43")
                        {
                            fateGrandOrderPerson.SkillReinforcement.Ascension4.Item3 = new Item();
                            fateGrandOrderPerson.SkillReinforcement.Ascension4.Item3.EnglishName = s.Replace("|43", "").Replace("=", "").TrimStart(' ').Replace("{{Inum|{{", "").Replace("{{", "").Replace("}}", "");
                            if (fateGrandOrderPerson.SkillReinforcement.Ascension4.Item3.EnglishName.IndexOf('|') != -1)
                                fateGrandOrderPerson.SkillReinforcement.Ascension4.Item3 = GetItem(fateGrandOrderPerson.SkillReinforcement.Ascension4.Item3.EnglishName.Remove(fateGrandOrderPerson.SkillReinforcement.Ascension4.Item3.EnglishName.IndexOf('|')));
                            else
                                fateGrandOrderPerson.SkillReinforcement.Ascension4.Item3 = GetItem(fateGrandOrderPerson.SkillReinforcement.Ascension4.Item3.EnglishName);
                        }
                        else if (s.Length >= 3 && s.Remove(3) == "|44")
                        {
                            fateGrandOrderPerson.SkillReinforcement.Ascension4.Item4 = new Item();
                            fateGrandOrderPerson.SkillReinforcement.Ascension4.Item4.EnglishName = s.Replace("|44", "").Replace("=", "").TrimStart(' ').Replace("{{Inum|{{", "").Replace("{{", "").Replace("}}", "");
                            if (fateGrandOrderPerson.SkillReinforcement.Ascension4.Item4.EnglishName.IndexOf('|') != -1)
                                fateGrandOrderPerson.SkillReinforcement.Ascension4.Item4 = GetItem(fateGrandOrderPerson.SkillReinforcement.Ascension4.Item4.EnglishName.Remove(fateGrandOrderPerson.SkillReinforcement.Ascension4.Item4.EnglishName.IndexOf('|')));
                            else
                                fateGrandOrderPerson.SkillReinforcement.Ascension4.Item4 = GetItem(fateGrandOrderPerson.SkillReinforcement.Ascension4.Item4.EnglishName);
                        }
                        else if (s.Length >= 3 && s.Remove(3) == "|51")
                        {
                            fateGrandOrderPerson.SkillReinforcement.Ascension5 = new AscensionSkillReinforcement();
                            fateGrandOrderPerson.SkillReinforcement.Ascension5.AscensionNumber = "5";
                            fateGrandOrderPerson.SkillReinforcement.Ascension5.Item1 = new Item();
                            fateGrandOrderPerson.SkillReinforcement.Ascension5.Item1.EnglishName = s.Replace("|51", "").Replace("=", "").TrimStart(' ').Replace("{{Inum|{{", "").Replace("{{", "").Replace("}}", "");
                            if (fateGrandOrderPerson.SkillReinforcement.Ascension5.Item1.EnglishName.IndexOf('|') != -1)
                                fateGrandOrderPerson.SkillReinforcement.Ascension5.Item1 = GetItem(fateGrandOrderPerson.SkillReinforcement.Ascension5.Item1.EnglishName.Remove(fateGrandOrderPerson.SkillReinforcement.Ascension5.Item1.EnglishName.IndexOf('|')));
                            else
                                fateGrandOrderPerson.SkillReinforcement.Ascension5.Item1 = GetItem(fateGrandOrderPerson.SkillReinforcement.Ascension5.Item1.EnglishName);
                        }
                        else if (s.Length >= 3 && s.Remove(3) == "|52")
                        {
                            fateGrandOrderPerson.SkillReinforcement.Ascension5.Item2 = new Item();
                            fateGrandOrderPerson.SkillReinforcement.Ascension5.Item2.EnglishName = s.Replace("|52", "").Replace("=", "").TrimStart(' ').Replace("{{Inum|{{", "").Replace("{{", "").Replace("}}", "");
                            if (fateGrandOrderPerson.SkillReinforcement.Ascension5.Item2.EnglishName.IndexOf('|') != -1)
                                fateGrandOrderPerson.SkillReinforcement.Ascension5.Item2 = GetItem(fateGrandOrderPerson.SkillReinforcement.Ascension5.Item2.EnglishName.Remove(fateGrandOrderPerson.SkillReinforcement.Ascension5.Item2.EnglishName.IndexOf('|')));
                            else
                                fateGrandOrderPerson.SkillReinforcement.Ascension5.Item2 = GetItem(fateGrandOrderPerson.SkillReinforcement.Ascension5.Item2.EnglishName);
                        }
                        else if (s.Length >= 3 && s.Remove(3) == "|53")
                        {
                            fateGrandOrderPerson.SkillReinforcement.Ascension5.Item3 = new Item();
                            fateGrandOrderPerson.SkillReinforcement.Ascension5.Item3.EnglishName = s.Replace("|53", "").Replace("=", "").TrimStart(' ').Replace("{{Inum|{{", "").Replace("{{", "").Replace("}}", "");
                            if (fateGrandOrderPerson.SkillReinforcement.Ascension5.Item3.EnglishName.IndexOf('|') != -1)
                                fateGrandOrderPerson.SkillReinforcement.Ascension5.Item3 = GetItem(fateGrandOrderPerson.SkillReinforcement.Ascension5.Item3.EnglishName.Remove(fateGrandOrderPerson.SkillReinforcement.Ascension5.Item3.EnglishName.IndexOf('|')));
                            else
                                fateGrandOrderPerson.SkillReinforcement.Ascension5.Item3 = GetItem(fateGrandOrderPerson.SkillReinforcement.Ascension5.Item3.EnglishName);
                        }
                        else if (s.Length >= 3 && s.Remove(3) == "|54")
                        {
                            fateGrandOrderPerson.SkillReinforcement.Ascension5.Item4 = new Item();
                            fateGrandOrderPerson.SkillReinforcement.Ascension5.Item4.EnglishName = s.Replace("|54", "").Replace("=", "").TrimStart(' ').Replace("{{Inum|{{", "").Replace("{{", "").Replace("}}", "");
                            if (fateGrandOrderPerson.SkillReinforcement.Ascension5.Item4.EnglishName.IndexOf('|') != -1)
                                fateGrandOrderPerson.SkillReinforcement.Ascension5.Item4 = GetItem(fateGrandOrderPerson.SkillReinforcement.Ascension5.Item4.EnglishName.Remove(fateGrandOrderPerson.SkillReinforcement.Ascension5.Item4.EnglishName.IndexOf('|')));
                            else
                                fateGrandOrderPerson.SkillReinforcement.Ascension5.Item4 = GetItem(fateGrandOrderPerson.SkillReinforcement.Ascension5.Item4.EnglishName);
                        }
                        else if (s.Length >= 3 && s.Remove(3) == "|61")
                        {
                            fateGrandOrderPerson.SkillReinforcement.Ascension6 = new AscensionSkillReinforcement();
                            fateGrandOrderPerson.SkillReinforcement.Ascension6.AscensionNumber = "5";
                            fateGrandOrderPerson.SkillReinforcement.Ascension6.Item1 = new Item();
                            fateGrandOrderPerson.SkillReinforcement.Ascension6.Item1.EnglishName = s.Replace("|51", "").Replace("=", "").TrimStart(' ').Replace("{{Inum|{{", "").Replace("{{", "").Replace("}}", "");
                            if (fateGrandOrderPerson.SkillReinforcement.Ascension6.Item1.EnglishName.IndexOf('|') != -1)
                                fateGrandOrderPerson.SkillReinforcement.Ascension6.Item1 = GetItem(fateGrandOrderPerson.SkillReinforcement.Ascension6.Item1.EnglishName.Remove(fateGrandOrderPerson.SkillReinforcement.Ascension6.Item1.EnglishName.IndexOf('|')));
                            else
                                fateGrandOrderPerson.SkillReinforcement.Ascension6.Item1 = GetItem(fateGrandOrderPerson.SkillReinforcement.Ascension6.Item1.EnglishName);
                        }
                        else if (s.Length >= 3 && s.Remove(3) == "|62")
                        {
                            fateGrandOrderPerson.SkillReinforcement.Ascension6.Item2 = new Item();
                            fateGrandOrderPerson.SkillReinforcement.Ascension6.Item2.EnglishName = s.Replace("|52", "").Replace("=", "").TrimStart(' ').Replace("{{Inum|{{", "").Replace("{{", "").Replace("}}", "");
                            if (fateGrandOrderPerson.SkillReinforcement.Ascension6.Item2.EnglishName.IndexOf('|') != -1)
                                fateGrandOrderPerson.SkillReinforcement.Ascension6.Item2 = GetItem(fateGrandOrderPerson.SkillReinforcement.Ascension6.Item2.EnglishName.Remove(fateGrandOrderPerson.SkillReinforcement.Ascension6.Item2.EnglishName.IndexOf('|')));
                            else
                                fateGrandOrderPerson.SkillReinforcement.Ascension6.Item2 = GetItem(fateGrandOrderPerson.SkillReinforcement.Ascension6.Item2.EnglishName);
                        }
                        else if (s.Length >= 3 && s.Remove(3) == "|63")
                        {
                            fateGrandOrderPerson.SkillReinforcement.Ascension6.Item3 = new Item();
                            fateGrandOrderPerson.SkillReinforcement.Ascension6.Item3.EnglishName = s.Replace("|53", "").Replace("=", "").TrimStart(' ').Replace("{{Inum|{{", "").Replace("{{", "").Replace("}}", "");
                            if (fateGrandOrderPerson.SkillReinforcement.Ascension6.Item3.EnglishName.IndexOf('|') != -1)
                                fateGrandOrderPerson.SkillReinforcement.Ascension6.Item3 = GetItem(fateGrandOrderPerson.SkillReinforcement.Ascension6.Item3.EnglishName.Remove(fateGrandOrderPerson.SkillReinforcement.Ascension6.Item3.EnglishName.IndexOf('|')));
                            else
                                fateGrandOrderPerson.SkillReinforcement.Ascension6.Item3 = GetItem(fateGrandOrderPerson.SkillReinforcement.Ascension6.Item3.EnglishName);
                        }
                        else if (s.Length >= 3 && s.Remove(3) == "|64")
                        {
                            fateGrandOrderPerson.SkillReinforcement.Ascension6.Item4 = new Item();
                            fateGrandOrderPerson.SkillReinforcement.Ascension6.Item4.EnglishName = s.Replace("|54", "").Replace("=", "").TrimStart(' ').Replace("{{Inum|{{", "").Replace("{{", "").Replace("}}", "");
                            if (fateGrandOrderPerson.SkillReinforcement.Ascension6.Item4.EnglishName.IndexOf('|') != -1)
                                fateGrandOrderPerson.SkillReinforcement.Ascension6.Item4 = GetItem(fateGrandOrderPerson.SkillReinforcement.Ascension6.Item4.EnglishName.Remove(fateGrandOrderPerson.SkillReinforcement.Ascension6.Item4.EnglishName.IndexOf('|')));
                            else
                                fateGrandOrderPerson.SkillReinforcement.Ascension6.Item4 = GetItem(fateGrandOrderPerson.SkillReinforcement.Ascension6.Item4.EnglishName);
                        }
                        else if (s.Length >= 3 && s.Remove(3) == "|71")
                        {
                            fateGrandOrderPerson.SkillReinforcement.Ascension7 = new AscensionSkillReinforcement();
                            fateGrandOrderPerson.SkillReinforcement.Ascension7.AscensionNumber = "5";
                            fateGrandOrderPerson.SkillReinforcement.Ascension7.Item1 = new Item();
                            fateGrandOrderPerson.SkillReinforcement.Ascension7.Item1.EnglishName = s.Replace("|51", "").Replace("=", "").TrimStart(' ').Replace("{{Inum|{{", "").Replace("{{", "").Replace("}}", "");
                            if (fateGrandOrderPerson.SkillReinforcement.Ascension7.Item1.EnglishName.IndexOf('|') != -1)
                                fateGrandOrderPerson.SkillReinforcement.Ascension7.Item1 = GetItem(fateGrandOrderPerson.SkillReinforcement.Ascension7.Item1.EnglishName.Remove(fateGrandOrderPerson.SkillReinforcement.Ascension7.Item1.EnglishName.IndexOf('|')));
                            else
                                fateGrandOrderPerson.SkillReinforcement.Ascension7.Item1 = GetItem(fateGrandOrderPerson.SkillReinforcement.Ascension7.Item1.EnglishName);
                        }
                        else if (s.Length >= 3 && s.Remove(3) == "|72")
                        {
                            fateGrandOrderPerson.SkillReinforcement.Ascension7.Item2 = new Item();
                            fateGrandOrderPerson.SkillReinforcement.Ascension7.Item2.EnglishName = s.Replace("|52", "").Replace("=", "").TrimStart(' ').Replace("{{Inum|{{", "").Replace("{{", "").Replace("}}", "");
                            if (fateGrandOrderPerson.SkillReinforcement.Ascension7.Item2.EnglishName.IndexOf('|') != -1)
                                fateGrandOrderPerson.SkillReinforcement.Ascension7.Item2 = GetItem(fateGrandOrderPerson.SkillReinforcement.Ascension7.Item2.EnglishName.Remove(fateGrandOrderPerson.SkillReinforcement.Ascension7.Item2.EnglishName.IndexOf('|')));
                            else
                                fateGrandOrderPerson.SkillReinforcement.Ascension7.Item2 = GetItem(fateGrandOrderPerson.SkillReinforcement.Ascension7.Item2.EnglishName);
                        }
                        else if (s.Length >= 3 && s.Remove(3) == "|73")
                        {
                            fateGrandOrderPerson.SkillReinforcement.Ascension7.Item3 = new Item();
                            fateGrandOrderPerson.SkillReinforcement.Ascension7.Item3.EnglishName = s.Replace("|53", "").Replace("=", "").TrimStart(' ').Replace("{{Inum|{{", "").Replace("{{", "").Replace("}}", "");
                            if (fateGrandOrderPerson.SkillReinforcement.Ascension7.Item3.EnglishName.IndexOf('|') != -1)
                                fateGrandOrderPerson.SkillReinforcement.Ascension7.Item3 = GetItem(fateGrandOrderPerson.SkillReinforcement.Ascension7.Item3.EnglishName.Remove(fateGrandOrderPerson.SkillReinforcement.Ascension7.Item3.EnglishName.IndexOf('|')));
                            else
                                fateGrandOrderPerson.SkillReinforcement.Ascension7.Item3 = GetItem(fateGrandOrderPerson.SkillReinforcement.Ascension7.Item3.EnglishName);
                        }
                        else if (s.Length >= 3 && s.Remove(3) == "|74")
                        {
                            fateGrandOrderPerson.SkillReinforcement.Ascension7.Item4 = new Item();
                            fateGrandOrderPerson.SkillReinforcement.Ascension7.Item4.EnglishName = s.Replace("|54", "").Replace("=", "").TrimStart(' ').Replace("{{Inum|{{", "").Replace("{{", "").Replace("}}", "");
                            if (fateGrandOrderPerson.SkillReinforcement.Ascension7.Item4.EnglishName.IndexOf('|') != -1)
                                fateGrandOrderPerson.SkillReinforcement.Ascension7.Item4 = GetItem(fateGrandOrderPerson.SkillReinforcement.Ascension7.Item4.EnglishName.Remove(fateGrandOrderPerson.SkillReinforcement.Ascension7.Item4.EnglishName.IndexOf('|')));
                            else
                                fateGrandOrderPerson.SkillReinforcement.Ascension7.Item4 = GetItem(fateGrandOrderPerson.SkillReinforcement.Ascension7.Item4.EnglishName);
                        }
                        else if (s.Length >= 3 && s.Remove(3) == "|81")
                        {
                            fateGrandOrderPerson.SkillReinforcement.Ascension8 = new AscensionSkillReinforcement();
                            fateGrandOrderPerson.SkillReinforcement.Ascension8.AscensionNumber = "5";
                            fateGrandOrderPerson.SkillReinforcement.Ascension8.Item1 = new Item();
                            fateGrandOrderPerson.SkillReinforcement.Ascension8.Item1.EnglishName = s.Replace("|51", "").Replace("=", "").TrimStart(' ').Replace("{{Inum|{{", "").Replace("{{", "").Replace("}}", "");
                            if (fateGrandOrderPerson.SkillReinforcement.Ascension8.Item1.EnglishName.IndexOf('|') != -1)
                                fateGrandOrderPerson.SkillReinforcement.Ascension8.Item1 = GetItem(fateGrandOrderPerson.SkillReinforcement.Ascension8.Item1.EnglishName.Remove(fateGrandOrderPerson.SkillReinforcement.Ascension8.Item1.EnglishName.IndexOf('|')));
                            else
                                fateGrandOrderPerson.SkillReinforcement.Ascension8.Item1 = GetItem(fateGrandOrderPerson.SkillReinforcement.Ascension8.Item1.EnglishName);
                        }
                        else if (s.Length >= 3 && s.Remove(3) == "|82")
                        {
                            fateGrandOrderPerson.SkillReinforcement.Ascension8.Item2 = new Item();
                            fateGrandOrderPerson.SkillReinforcement.Ascension8.Item2.EnglishName = s.Replace("|52", "").Replace("=", "").TrimStart(' ').Replace("{{Inum|{{", "").Replace("{{", "").Replace("}}", "");
                            if (fateGrandOrderPerson.SkillReinforcement.Ascension8.Item2.EnglishName.IndexOf('|') != -1)
                                fateGrandOrderPerson.SkillReinforcement.Ascension8.Item2 = GetItem(fateGrandOrderPerson.SkillReinforcement.Ascension8.Item2.EnglishName.Remove(fateGrandOrderPerson.SkillReinforcement.Ascension8.Item2.EnglishName.IndexOf('|')));
                            else
                                fateGrandOrderPerson.SkillReinforcement.Ascension8.Item2 = GetItem(fateGrandOrderPerson.SkillReinforcement.Ascension8.Item2.EnglishName);
                        }
                        else if (s.Length >= 3 && s.Remove(3) == "|83")
                        {
                            fateGrandOrderPerson.SkillReinforcement.Ascension8.Item3 = new Item();
                            fateGrandOrderPerson.SkillReinforcement.Ascension8.Item3.EnglishName = s.Replace("|53", "").Replace("=", "").TrimStart(' ').Replace("{{Inum|{{", "").Replace("{{", "").Replace("}}", "");
                            if (fateGrandOrderPerson.SkillReinforcement.Ascension8.Item3.EnglishName.IndexOf('|') != -1)
                                fateGrandOrderPerson.SkillReinforcement.Ascension8.Item3 = GetItem(fateGrandOrderPerson.SkillReinforcement.Ascension8.Item3.EnglishName.Remove(fateGrandOrderPerson.SkillReinforcement.Ascension8.Item3.EnglishName.IndexOf('|')));
                            else
                                fateGrandOrderPerson.SkillReinforcement.Ascension8.Item3 = GetItem(fateGrandOrderPerson.SkillReinforcement.Ascension8.Item3.EnglishName);
                        }
                        else if (s.Length >= 3 && s.Remove(3) == "|84")
                        {
                            fateGrandOrderPerson.SkillReinforcement.Ascension8.Item4 = new Item();
                            fateGrandOrderPerson.SkillReinforcement.Ascension8.Item4.EnglishName = s.Replace("|54", "").Replace("=", "").TrimStart(' ').Replace("{{Inum|{{", "").Replace("{{", "").Replace("}}", "");
                            if (fateGrandOrderPerson.SkillReinforcement.Ascension8.Item4.EnglishName.IndexOf('|') != -1)
                                fateGrandOrderPerson.SkillReinforcement.Ascension8.Item4 = GetItem(fateGrandOrderPerson.SkillReinforcement.Ascension8.Item4.EnglishName.Remove(fateGrandOrderPerson.SkillReinforcement.Ascension8.Item4.EnglishName.IndexOf('|')));
                            else
                                fateGrandOrderPerson.SkillReinforcement.Ascension8.Item4 = GetItem(fateGrandOrderPerson.SkillReinforcement.Ascension8.Item4.EnglishName);
                        }
                        else if (s.Length >= 3 && s.Remove(3) == "|91")
                        {
                            fateGrandOrderPerson.SkillReinforcement.Ascension9 = new AscensionSkillReinforcement();
                            fateGrandOrderPerson.SkillReinforcement.Ascension9.AscensionNumber = "5";
                            fateGrandOrderPerson.SkillReinforcement.Ascension9.Item1 = new Item();
                            fateGrandOrderPerson.SkillReinforcement.Ascension9.Item1.EnglishName = s.Replace("|51", "").Replace("=", "").TrimStart(' ').Replace("{{Inum|{{", "").Replace("{{", "").Replace("}}", "");
                            if (fateGrandOrderPerson.SkillReinforcement.Ascension9.Item1.EnglishName.IndexOf('|') != -1)
                                fateGrandOrderPerson.SkillReinforcement.Ascension9.Item1 = GetItem(fateGrandOrderPerson.SkillReinforcement.Ascension9.Item1.EnglishName.Remove(fateGrandOrderPerson.SkillReinforcement.Ascension9.Item1.EnglishName.IndexOf('|')));
                            else
                                fateGrandOrderPerson.SkillReinforcement.Ascension9.Item1 = GetItem(fateGrandOrderPerson.SkillReinforcement.Ascension9.Item1.EnglishName);
                        }
                        else if (s.Length >= 3 && s.Remove(3) == "|92")
                        {
                            fateGrandOrderPerson.SkillReinforcement.Ascension9.Item2 = new Item();
                            fateGrandOrderPerson.SkillReinforcement.Ascension9.Item2.EnglishName = s.Replace("|52", "").Replace("=", "").TrimStart(' ').Replace("{{Inum|{{", "").Replace("{{", "").Replace("}}", "");
                            if (fateGrandOrderPerson.SkillReinforcement.Ascension9.Item2.EnglishName.IndexOf('|') != -1)
                                fateGrandOrderPerson.SkillReinforcement.Ascension9.Item2 = GetItem(fateGrandOrderPerson.SkillReinforcement.Ascension9.Item2.EnglishName.Remove(fateGrandOrderPerson.SkillReinforcement.Ascension9.Item2.EnglishName.IndexOf('|')));
                            else
                                fateGrandOrderPerson.SkillReinforcement.Ascension9.Item2 = GetItem(fateGrandOrderPerson.SkillReinforcement.Ascension9.Item2.EnglishName);
                        }
                        else if (s.Length >= 3 && s.Remove(3) == "|93")
                        {
                            fateGrandOrderPerson.SkillReinforcement.Ascension9.Item3 = new Item();
                            fateGrandOrderPerson.SkillReinforcement.Ascension9.Item3.EnglishName = s.Replace("|53", "").Replace("=", "").TrimStart(' ').Replace("{{Inum|{{", "").Replace("{{", "").Replace("}}", "");
                            if (fateGrandOrderPerson.SkillReinforcement.Ascension9.Item3.EnglishName.IndexOf('|') != -1)
                                fateGrandOrderPerson.SkillReinforcement.Ascension9.Item3 = GetItem(fateGrandOrderPerson.SkillReinforcement.Ascension9.Item3.EnglishName.Remove(fateGrandOrderPerson.SkillReinforcement.Ascension9.Item3.EnglishName.IndexOf('|')));
                            else
                                fateGrandOrderPerson.SkillReinforcement.Ascension9.Item3 = GetItem(fateGrandOrderPerson.SkillReinforcement.Ascension9.Item3.EnglishName);
                        }
                        else if (s.Length >= 3 && s.Remove(3) == "|94")
                        {
                            fateGrandOrderPerson.SkillReinforcement.Ascension9.Item4 = new Item();
                            fateGrandOrderPerson.SkillReinforcement.Ascension9.Item4.EnglishName = s.Replace("|54", "").Replace("=", "").TrimStart(' ').Replace("{{Inum|{{", "").Replace("{{", "").Replace("}}", "");
                            if (fateGrandOrderPerson.SkillReinforcement.Ascension9.Item4.EnglishName.IndexOf('|') != -1)
                                fateGrandOrderPerson.SkillReinforcement.Ascension9.Item4 = GetItem(fateGrandOrderPerson.SkillReinforcement.Ascension9.Item4.EnglishName.Remove(fateGrandOrderPerson.SkillReinforcement.Ascension9.Item4.EnglishName.IndexOf('|')));
                            else
                                fateGrandOrderPerson.SkillReinforcement.Ascension9.Item4 = GetItem(fateGrandOrderPerson.SkillReinforcement.Ascension9.Item4.EnglishName);
                        }
                        if (s.Contains("|1qp"))
                        {
                            fateGrandOrderPerson.SkillReinforcement.Ascension1.QP = s.Replace("|1qp", "").Replace("=", "").TrimStart(' ').Replace("{{Inum|{{QP}}|", "").Replace("}}", "");
                        }
                        else if (s.Contains("|2qp"))
                        {
                            fateGrandOrderPerson.SkillReinforcement.Ascension2.QP = s.Replace("|2qp", "").Replace("=", "").TrimStart(' ').Replace("{{Inum|{{QP}}|", "").Replace("}}", "");
                        }
                        else if (s.Contains("|3qp"))
                        {
                            fateGrandOrderPerson.SkillReinforcement.Ascension3.QP = s.Replace("|3qp", "").Replace("=", "").TrimStart(' ').Replace("{{Inum|{{QP}}|", "").Replace("}}", "");
                        }
                        else if (s.Contains("|4qp"))
                        {
                            fateGrandOrderPerson.SkillReinforcement.Ascension4.QP = s.Replace("|4qp", "").Replace("=", "").TrimStart(' ').Replace("{{Inum|{{QP}}|", "").Replace("}}", "");
                        }
                        else if (s.Contains("|5qp"))
                        {
                            fateGrandOrderPerson.SkillReinforcement.Ascension5.QP = s.Replace("|5qp", "").Replace("=", "").TrimStart(' ').Replace("{{Inum|{{QP}}|", "").Replace("}}", "");
                        }
                        if (s.Contains("|6qp"))
                        {
                            fateGrandOrderPerson.SkillReinforcement.Ascension6.QP = s.Replace("|6qp", "").Replace("=", "").TrimStart(' ').Replace("{{Inum|{{QP}}|", "").Replace("}}", "");
                        }
                        else if (s.Contains("|7qp"))
                        {
                            fateGrandOrderPerson.SkillReinforcement.Ascension7.QP = s.Replace("|7qp", "").Replace("=", "").TrimStart(' ').Replace("{{Inum|{{QP}}|", "").Replace("}}", "");
                        }
                        else if (s.Contains("|8qp"))
                        {
                            fateGrandOrderPerson.SkillReinforcement.Ascension8.QP = s.Replace("|8qp", "").Replace("=", "").TrimStart(' ').Replace("{{Inum|{{QP}}|", "").Replace("}}", "");
                        }
                        else if (s.Contains("|9qp"))
                        {
                            fateGrandOrderPerson.SkillReinforcement.Ascension9.QP = s.Replace("|9qp", "").Replace("=", "").TrimStart(' ').Replace("{{Inum|{{QP}}|", "").Replace("}}", "");
                        }
                    }
                    #endregion

                    #region Trigger Skills Logic
                    if (s == "== Passive Skills ==" || s == "==Passive Skills==")
                    {
                        fateGrandOrderPerson.PassiveSkills.Add(new PassiveSkillList());
                        GettingPassiveSkills = true;
                    }
                    else if (s == "== Active Skills ==" || s == "==Active Skills==")
                    {
                        GettingActiveSkills = true;
                    }
                    else if (s == "== Ascension ==" || s == "==Ascension==")
                    {
                        fateGrandOrderPerson.Ascension = new Ascension();
                        GettingAscension = true;
                    }
                    else if (s == "== Skill Reinforcement ==" || s == "==Skill Reinforcement==")
                    {
                        fateGrandOrderPerson.SkillReinforcement = new SkillReinforcement();
                        GettingSkillReinforcement = true;
                    }
                    else if (GettingActiveSkills | GettingPassiveSkills | GettingNoblePhantasm && FixString(s) == "</tabber>")
                    {
                        GettingActiveSkills = false;
                        GettingPassiveSkills = false;
                        GettingNoblePhantasm = false;
                    }
                    else if (GettingPassiveSkills | GettingAscension | GettingSkillReinforcement && s == @"}}")
                    {
                        GettingPassiveSkills = false;
                        GettingAscension = false;
                        GettingSkillReinforcement = false;
                    }
                    else if (s == "== Noble Phantasm ==" || s == "==Noble Phantasm==")
                    {
                        GettingNoblePhantasm = true;
                        fateGrandOrderPerson.NoblePhantasms.Add(new NoblePhantasmList());
                    }
#endregion

                    #region Basic Infomation
                    else if (s.Contains("|jname"))
                    {
                        fateGrandOrderPerson.BasicInfomation.JapaneseName = s.Replace("|jname", "").Replace("=", "").TrimStart(' ');
                    }
                    else if (s.Contains("|voicea"))
                    {
                        fateGrandOrderPerson.BasicInfomation.VoiceActor = s.Replace("|voicea", "").Replace("=", "").TrimStart(' '); 
                    }
                    else if (s.Contains("|illus"))
                    {
                        fateGrandOrderPerson.BasicInfomation.Illustrator = s.Replace("|illus", "").Replace("=", "").TrimStart(' ');
                    }
                    else if (s.Contains("|class "))
                    {
                        fateGrandOrderPerson.BasicInfomation.Class = s.Replace("|class", "").Replace("=", "").TrimStart(' ');
                    }
                    else if (s.Contains("|atk"))
                    {
                        fateGrandOrderPerson.BasicInfomation.ATK = s.Replace("|atk", "").Replace("=", "").TrimStart(' ');
                    }
                    else if (s.Contains("|hp"))
                    {
                        fateGrandOrderPerson.BasicInfomation.HP = s.Replace("|hp", "").Replace("=", "").TrimStart(' ');
                    }
                    else if (s.Contains("|gatk"))
                    {
                        fateGrandOrderPerson.BasicInfomation.GrailATK = s.Replace("|gatk", "").Replace("=", "").TrimStart(' ');
                    }
                    else if (s.Contains("|ghp"))
                    {
                        fateGrandOrderPerson.BasicInfomation.GrailHP = s.Replace("|ghp", "").Replace("=", "").TrimStart(' ');
                    }
                    else if (s.Contains("|stars"))
                    {
                        fateGrandOrderPerson.BasicInfomation.Stars = s.Replace("|stars", "").Replace("=", "").TrimStart(' ');
                    }
                    else if (s.Contains("|cost"))
                    {
                        fateGrandOrderPerson.BasicInfomation.Cost = s.Replace("|cost", "").Replace("=", "").TrimStart(' ');
                    }
                    else if (s.Contains("|cc"))
                    {
                        fateGrandOrderPerson.BasicInfomation.QQQAB = s.Replace("|cc", "").Replace("=", "").TrimStart(' ');
                    }
                    else if (s.Contains("|mlevel"))
                    {
                        fateGrandOrderPerson.BasicInfomation.MaxLevel = s.Replace("|mlevel", "").Replace("=", "").TrimStart(' ');
                    }
                    else if (s.Contains("|id"))
                    {
                        fateGrandOrderPerson.BasicInfomation.ID = s.Replace("|id", "").Replace("=", "").TrimStart(' ');
                    }
                    else if (s.Contains("|attribute"))
                    {
                        fateGrandOrderPerson.BasicInfomation.Attribute = s.Replace("|attribute", "").Replace("=", "").TrimStart(' ');
                    }
                    else if (s.Contains("|qhits"))
                    {
                        fateGrandOrderPerson.BasicInfomation.QuickHits = s.Replace("|qhits", "").Replace("=", "").TrimStart(' ');
                    }
                    else if (s.Contains("|ahits"))
                    {
                        fateGrandOrderPerson.BasicInfomation.ArtsHits = s.Replace("|ahits", "").Replace("=", "").TrimStart(' ');
                    }
                    else if (s.Contains("|bhits"))
                    {
                        fateGrandOrderPerson.BasicInfomation.BusterHits = s.Replace("|bhits", "").Replace("=", "").TrimStart(' ');
                    }
                    else if (s.Contains("|ehits"))
                    {
                        fateGrandOrderPerson.BasicInfomation.ExtraHits = s.Replace("|ehits", "").Replace("=", "").TrimStart(' ');
                    }
                    else if (s.Contains("|deathrate"))
                    {
                        fateGrandOrderPerson.BasicInfomation.DeathRate = s.Replace("|deathrate", "").Replace("=", "").TrimStart(' ');
                    }
                    else if (s.Contains("|starabsorption"))
                    {
                        fateGrandOrderPerson.BasicInfomation.StarAbsorption = s.Replace("|starabsorption", "").Replace("=", "").TrimStart(' ');
                    }
                    else if (s.Contains("|stargeneration"))
                    {
                        fateGrandOrderPerson.BasicInfomation.StarGeneration = s.Replace("|stargeneration", "").Replace("=", "").TrimStart(' ');
                    }
                    else if (s.Contains("|npchargeatk"))
                    {
                        fateGrandOrderPerson.BasicInfomation.NPChargeATK = s.Replace("|npchargeatk", "").Replace("=", "").TrimStart(' ');
                    }
                    else if (s.Contains("|npchargedef"))
                    {
                        fateGrandOrderPerson.BasicInfomation.NPChargeDEF = s.Replace("|npchargedef", "").Replace("=", "").TrimStart(' ');
                    }
                    else if (s.Contains("|growthc"))
                    {
                        fateGrandOrderPerson.BasicInfomation.GrowthCurve = s.Replace("|growthc", "").Replace("=", "").TrimStart(' ');
                    }
                    else if (s.Contains("|aka"))
                    {
                        fateGrandOrderPerson.BasicInfomation.AKA = FixString(s).Replace("|aka", "").Replace("=", "").TrimStart(' ').Replace("<br/>", " ").Replace("'''","").Replace("''", "").Replace(", ",",").Replace(", ",",").Split(',');
                    }
                    else if (s.Contains("|traits"))
                    {
                        fateGrandOrderPerson.BasicInfomation.Traits = s.Replace("|traits", "").Replace("=", "").TrimStart(' ').Replace(", ",",").Split(',');
                    }
                    else if (s.Contains("|gender"))
                    {
                        var gender = s.Replace("|gender", "").Replace("=", "").TrimStart(' ').ToLower();
                        if (gender[0] == 'f') { fateGrandOrderPerson.BasicInfomation.Gender = "Female"; }
                        else if (gender[0] == 'm') { fateGrandOrderPerson.BasicInfomation.Gender = "Male"; }
                        else { fateGrandOrderPerson.BasicInfomation.Gender = gender; } //For the people who think attack helicopter is a gender ;)
                        gender = null;
                    }
                    else if (s.Contains("|alignment"))
                    {
                        fateGrandOrderPerson.BasicInfomation.Alignment = s.Replace("|alignment", "").Replace("=", "").TrimStart(' ');
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
