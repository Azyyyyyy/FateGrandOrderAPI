using System;
using System.Linq;
using HtmlAgilityPack;
using System.Threading.Tasks;
using FateGrandOrderApi.Classes;
using FateGrandOrderApi.Caching;
using FateGrandOrderApi.Logging;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace FateGrandOrderApi
{
    /// <summary>
    /// Class containing parsing logic (this is where you get your people and skills from)
    /// </summary>
    public static class FateGrandOrderParsing
    {
        private static string FixString(string s)
        {
            if (!string.IsNullOrWhiteSpace(s))
                return s.Replace("&lt;", "<").Replace("%27", "'").Replace("<br>", "<br/>").Replace("%26", "%");
            else
                return s;
        }

        #region Skills Logic
        /// <summary>
        /// This will return a Skill (will return null if the skill isn't found)
        /// </summary>
        /// <param name="skillName">The Skill name to look for</param>
        /// <param name="resultString">String[] we use to make the Skill (this is exposed for other Skill types as it will contain the Information needed to make them too)</param>
        /// <returns></returns>
        public static async Task<Tuple<Skill, string[]>> GetSkill(string skillName)
        {
            string[] resultString = null;
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
                        skill.Image = await AssigningContent.Image(s);
                    }
                    else if (s.Contains("|name"))
                    {
                        skill.Name = await AssigningContent.GenericAssigning(s, "|name");
                    }
                    else if (s.Contains("|rank"))
                    {
                        skill.Rank = await AssigningContent.GenericAssigning(s, "|rank");
                    }
                    else if (s.Contains("|effect"))
                    {
                        try
                        {
                            var effects = await AssigningContent.GenericAssigning(s, "|effect", new string[] { "''" }, new string[][] { new string[] { "<br/>", "\\" }, new string[] { "<sup>", "(" }, new string[] { "</sup>", ")" } });
                            while (effects.ToLower().Contains("]]"))
                            {
                                int startpoint = 0;
                                foreach (char c in effects)
                                {
                                    if (c == '[')
                                        break;
                                    startpoint++;
                                }
                                while (effects[startpoint] != '|')
                                {
                                    effects = effects.Remove(startpoint, 1);
                                }
                                effects = effects.Remove(startpoint, 1);
                                while (effects[startpoint] != ']')
                                {
                                    startpoint++;
                                }
                                effects = effects.Remove(startpoint, 2);
                            }
                            skill.Effect = effects.Split('\\');
                        }
                        catch (Exception e)
                        {
                            Logger.LogConsole(e, $"Looks like something happened when filling up Effect in a skill called {skill.Name}", $"String used when doing this: {s}", false);
                            Logger.LogFile(e, $"Looks like something happened when filling up Effect in a skill called {skill.Name}", false, $"String used when doing this: {s}");
                        }
                    }
                }

                if (skill != null)
                    return Tuple.Create(skill, resultString);
                else
                    return null;
            }
            return Tuple.Create(skill, resultString);
        }

        /// <summary>
        /// This will return a filled in ActiveSkill (If the skill isn't a ActiveSkill it will return the core skill content and this will return a ActiveSkill only with the ActiveSkill name you used if the skill isn't found)
        /// </summary>
        /// <param name="skillName">The ActiveSkill name to look for</param>
        /// <returns></returns>
        public static async Task<ActiveSkill> GetSkills(string skillName)
        {
            return await GetSkill(new ActiveSkill { Name = skillName });
        }

        /// <summary>
        /// This will return a filled in ActiveSkill (If the skill isn't a ActiveSkill it will return the core skill content and this will return what was in the ActiveSkills already if the skill isn't found)
        /// </summary>
        /// <param name="skill">The ActiveSkill to put all the content into</param>
        /// <returns></returns>
        public static async Task<ActiveSkill> GetSkill(ActiveSkill skill)
        {
            string lastLevelEffect = "";
            var content = await GetSkill(skill.Name);
            var basicSkillContent = content.Item1;
            string[] resultString = content.Item2;

            string GetStartPart()
            {
                try
                {
                    if (string.IsNullOrWhiteSpace(lastLevelEffect))
                        return "|";
                    else
                        if (int.TryParse(lastLevelEffect[1].ToString(), out int a))
                        return $"|{a}";
                    else
                        return "|";
                }
                catch (Exception e)
                {
                    Logger.LogConsole(e, "Looks like something happened when GetStartPart was called", $"what lastLevelEffect was when this ex happened: {lastLevelEffect}", false);
                    Logger.LogFile(e, "Looks like something happened when GetStartPart was called", false, $"what lastLevelEffect was when this ex happened: {lastLevelEffect}");
                    return "|";
                }
            }

            //For in case we put the person in wrong
            if (resultString == null)
                return skill;

            foreach (string s in resultString)
            {
                if (s.Contains("|servanticons"))
                {
                    try
                    {
                        string servantIcons = s;
                        while (servantIcons.ToLower().Contains("file"))
                        {
                            int startpoint = 0;
                            foreach (char c in servantIcons)
                            {
                                if (c == '[')
                                    break;
                                startpoint++;
                            }
                            while (servantIcons[startpoint] != ']')
                            {
                                servantIcons = servantIcons.Remove(startpoint, 1);
                            }
                            servantIcons = servantIcons.Remove(startpoint, 2);
                        }
                        skill.ServantsThatHaveThisSkill = await AssigningContent.GenericArrayAssigning(servantIcons, "|servanticons", '\\', new string[] { "{{" }, new string[][] { new string[] { "}}", "\\" } });
                    }
                    catch (Exception e)
                    {
                        Logger.LogConsole(e, $"Looks like something happened when filling up ServantsThatHaveThisSkill in active skill {skill.Name}", $"String used when doing this: {s}", false);
                        Logger.LogFile(e, $"Looks like something happened when filling up ServantsThatHaveThisSkill in active skill {skill.Name}", false, $"String used when doing this: {s}");
                    }
                }
                else if (s.Contains($"leveleffect"))
                {
                    try
                    {
                        lastLevelEffect = s;
                        if (int.TryParse(s[1].ToString(), out int a))
                        {
                            while (lastLevelEffect.ToLower().Contains("]]"))
                            {
                                int startpoint = 0;
                                foreach (char c in lastLevelEffect)
                                {
                                    if (c == '[')
                                        break;
                                    startpoint++;
                                }
                                while (lastLevelEffect[startpoint] != '|')
                                {
                                    lastLevelEffect = lastLevelEffect.Remove(startpoint, 1);
                                }
                                lastLevelEffect = lastLevelEffect.Remove(startpoint, 1);
                                while (lastLevelEffect[startpoint] != ']')
                                {
                                    startpoint++;
                                }
                                lastLevelEffect = lastLevelEffect.Remove(startpoint, 2);
                            }
                        }
                        skill.LevelEffects.Add(new LevelEffect10 { LevelEffectName = await AssigningContent.GenericAssigning(lastLevelEffect, $"{GetStartPart()}leveleffect") });
                    }
                    catch (Exception e)
                    {
                        Logger.LogConsole(e, $"Looks like something happened when making a LevelEffects in active skill {skill.Name}", $"String used when doing this: {s}", false);
                        Logger.LogFile(e, $"Looks like something happened when making a LevelEffects in active skill {skill.Name}", false, $"String used when doing this: {s}");
                    }
                }
                else if (s.Contains($"{GetStartPart()}l1 "))
                {
                    skill.LevelEffects[skill.LevelEffects.Count - 1].Level1Effect.EffectStrength = await AssigningContent.GenericAssigning(s, $"{GetStartPart()}l1");
                }
                else if (s.Contains($"{GetStartPart()}l2"))
                {
                    skill.LevelEffects[skill.LevelEffects.Count - 1].Level2Effect.EffectStrength = await AssigningContent.GenericAssigning(s, $"{GetStartPart()}l2");
                }
                else if (s.Contains($"{GetStartPart()}l3"))
                {
                    skill.LevelEffects[skill.LevelEffects.Count - 1].Level3Effect.EffectStrength = await AssigningContent.GenericAssigning(s, $"{GetStartPart()}l3");
                }
                else if (s.Contains($"{GetStartPart()}l4"))
                {
                    skill.LevelEffects[skill.LevelEffects.Count - 1].Level4Effect.EffectStrength = await AssigningContent.GenericAssigning(s, $"{GetStartPart()}l4");
                }
                else if (s.Contains($"{GetStartPart()}l5"))
                {
                    skill.LevelEffects[skill.LevelEffects.Count - 1].Level5Effect.EffectStrength = await AssigningContent.GenericAssigning(s, $"{GetStartPart()}l5");
                }
                else if (s.Contains($"{GetStartPart()}l6"))
                {
                    skill.LevelEffects[skill.LevelEffects.Count - 1].Level6Effect.EffectStrength = await AssigningContent.GenericAssigning(s, $"{GetStartPart()}l6");
                }
                else if (s.Contains($"{GetStartPart()}l7"))
                {
                    skill.LevelEffects[skill.LevelEffects.Count - 1].Level7Effect.EffectStrength = await AssigningContent.GenericAssigning(s, $"{GetStartPart()}l7");
                }
                else if (s.Contains($"{GetStartPart()}l8"))
                {
                    skill.LevelEffects[skill.LevelEffects.Count - 1].Level8Effect.EffectStrength = await AssigningContent.GenericAssigning(s, $"{GetStartPart()}l8");
                }
                else if (s.Contains($"{GetStartPart()}l9"))
                {
                    skill.LevelEffects[skill.LevelEffects.Count - 1].Level9Effect.EffectStrength = await AssigningContent.GenericAssigning(s, $"{GetStartPart()}l9");
                }
                else if (s.Contains($"{GetStartPart()}l10"))
                {
                    skill.LevelEffects[skill.LevelEffects.Count - 1].Level10Effect.EffectStrength = await AssigningContent.GenericAssigning(s, $"{GetStartPart()}l10");
                }
                else if (s.Contains("|c1 "))
                {
                    skill.LevelEffects[skill.LevelEffects.Count - 1].Level1Effect.Cooldown = await AssigningContent.GenericAssigning(s, "|c1");
                }
                else if (s.Contains("|c2"))
                {
                    skill.LevelEffects[skill.LevelEffects.Count - 1].Level2Effect.Cooldown = await AssigningContent.GenericAssigning(s, "|c2");
                }
                else if (s.Contains("|c3"))
                {
                    skill.LevelEffects[skill.LevelEffects.Count - 1].Level3Effect.Cooldown = await AssigningContent.GenericAssigning(s, "|c3");
                }
                else if (s.Contains("|c4"))
                {
                    skill.LevelEffects[skill.LevelEffects.Count - 1].Level4Effect.Cooldown = await AssigningContent.GenericAssigning(s, "|c4");
                }
                else if (s.Contains("|c5"))
                {
                    skill.LevelEffects[skill.LevelEffects.Count - 1].Level5Effect.Cooldown = await AssigningContent.GenericAssigning(s, "|c5");
                }
                else if (s.Contains("|c6"))
                {
                    skill.LevelEffects[skill.LevelEffects.Count - 1].Level6Effect.Cooldown = await AssigningContent.GenericAssigning(s, "|c6");
                }
                else if (s.Contains("|c7"))
                {
                    skill.LevelEffects[skill.LevelEffects.Count - 1].Level7Effect.Cooldown = await AssigningContent.GenericAssigning(s, "|c7");
                }
                else if (s.Contains("|c8"))
                {
                    skill.LevelEffects[skill.LevelEffects.Count - 1].Level8Effect.Cooldown = await AssigningContent.GenericAssigning(s, "|c8");
                }
                else if (s.Contains("|c9"))
                {
                    skill.LevelEffects[skill.LevelEffects.Count - 1].Level9Effect.Cooldown = await AssigningContent.GenericAssigning(s, "|c9");
                }
                else if (s.Contains("|c10"))
                {
                    skill.LevelEffects[skill.LevelEffects.Count - 1].Level10Effect.Cooldown = await AssigningContent.GenericAssigning(s, "|c10");
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

        public async static Task<Item> GetItem(string itemName, Enemy enemyToNotLookFor = null)
        {
            //To add Logger try catch
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
                    FateGrandOrderPersonCache.Items = new List<Item>();

                try
                {
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
                }
                catch (Exception e)
                {
                    Logger.LogConsole(e, "Looks like something happened when accessing/using the cache for items", $"item name: {item.EnglishName}", false);
                    Logger.LogFile(e, "Looks like something happened when accessing/using the cache for items", false, $"item name: {item.EnglishName}");
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
                        try
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
                                        var thing = await AssigningContent.GenericArrayAssigning(s, "", ']', new string[] { "<br/>" }, new string[][] { new string[] { "[[", "[" }, new string[] { "]]", "]" } });
                                        if (thing.Length >= 3)
                                        {
                                            item.DropLocations[item.DropLocations.Count - 1].DropLocations.Add(new ItemDropLocation
                                            {
                                                Location = thing[0].Replace("[", ""),
                                                PossibleDrops = thing[1].Replace("[", ""),
                                                APCost = thing[2].Replace("[", "")
                                            });
                                        }
                                    }
                                    catch (Exception e) { Console.WriteLine($"Looks like something happened while filling up a item stat: {e}"); }
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            Logger.LogConsole(e, "Looks like something happened when doing DoingLocationLogic if statement", $"item name: {item.EnglishName}", false);
                            Logger.LogFile(e, "Looks like something happened when doing DoingLocationLogic if statement", false, $"item name: {item.EnglishName}");
                        }

                    }
                    else if (s.Contains("|jpName"))
                    {
                        item.JapaneseName = await AssigningContent.GenericAssigning(s, "|jpName");
                    }
                    else if (s.Contains("|image"))
                    {
                        item.ItemImage = await AssigningContent.Image(s, "|image");
                    }
                    else if (s.Contains("|enemy"))
                    {
                        var enemys = await AssigningContent.GenericArrayAssigning(s, "|enemy", '/', OtherPartsToRemove: new string[] { "[[", "]]" });
                        if (enemys != null && enemys.Length > 0) item.EnemiesThatDroppedThis = new List<Enemy>();
                        foreach (string enemy in enemys)
                        {
                            string enemyEdited = enemy;
                            try
                            {
                                if (enemyEdited.IndexOf('|') != -1)
                                    enemyEdited = enemyEdited.Remove(0, enemyEdited.IndexOf('|') + 1);

                                if (enemyToNotLookFor != null && enemyToNotLookFor.EnglishName == enemyEdited)
                                    item.EnemiesThatDroppedThis.Add(enemyToNotLookFor);
                                else
                                    item.EnemiesThatDroppedThis.Add(await GetEnemy(enemyEdited, item));
                            }
                            catch (Exception e)
                            {
                                Logger.LogConsole(e, $"Looks like something failed when getting the enemys that drop {item.EnglishName}", $"item name: {item.EnglishName}\r\nEnemy name: {enemyEdited}", false);
                                Logger.LogFile(e, $"Looks like something failed when getting the enemys that drop {item.EnglishName}", false, $"item name: {item.EnglishName}\r\nEnemy name: {enemyEdited}");
                            }
                        }
                    }
                    else if (s.Contains("|jdesc"))
                    {
                        item.JapaneseDescription = await AssigningContent.GenericAssigning(s, "|jdesc");
                    }
                    else if (s.Contains("|desc"))
                    {
                        item.EnglishDescription = await AssigningContent.GenericAssigning(s, "|desc");
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

        public async static Task<Enemy> GetEnemy(string enemyName, Item itemToNotLookFor = null)
        {
            Enemy enemy = new Enemy(enemyName);
            foreach (HtmlNode col in new HtmlWeb().Load($"https://fategrandorder.fandom.com/wiki/{enemyName}?action=edit").DocumentNode.SelectNodes("//textarea"))
            {
                //For in case we put the person in wrong or it doesn't have a webpage
                if (string.IsNullOrEmpty(col.InnerText))
                    break;

                var resultString = Regex.Split(col.InnerText, @"\n");

                foreach (string s in resultString)
                {
                    if (s.Contains("|image"))
                    {
                        //enemy.JapaneseName = s.Replace("|image", "").Replace("=", "").Trim();
                    }
                    else if (s.Contains("|class"))
                    {
                        enemy.Class = await AssigningContent.GenericArrayAssigning(s, "|class", '\\', new string[] { "{{", "}}" }, new string[][] { new string[] { "}}{{", "\\" } });
                    }
                    else if (s.Contains("|area"))
                    {
                        var thing = await AssigningContent.GenericArrayAssigning(s, "|area");
                        try
                        {
                            enemy.Areas = new string[thing.Length];
                            int count = 0;
                            foreach (string place in thing)
                            {
                                if (!place.Contains("|"))
                                    enemy.Areas[count] = place.Replace("[[", "").Replace("]]", "");
                                else
                                    enemy.Areas[count] = place.Replace("[[", "").Replace("]]", ")").Replace("|", " (");
                                count++;
                            }
                        }
                        catch (Exception e)
                        {
                            Logger.LogConsole(e, $"Looks like something failed when assigning enemy.Areas", $"enemy name: {enemy.EnglishName}", false);
                            Logger.LogFile(e, $"Looks like something failed when assigning enemy.Areas", false, $"Enemy name: {enemy.EnglishName}");
                        }
                    }
                    else if (s.Contains("|jname"))
                    {
                        enemy.JapaneseName = await AssigningContent.GenericAssigning(s, "|jname");
                    }
                    else if (s.Contains("|rank"))
                    {
                        enemy.Rank = await AssigningContent.GenericAssigning(s, "|rank");
                    }
                    else if (s.Contains("|gender"))
                    {
                        await AssigningContent.Gender(s, enemy);
                    }
                    else if (s.Contains("|attribute"))
                    {
                        enemy.Attribute = await AssigningContent.GenericAssigning(s, "|attribute");
                    }
                    else if (s.Contains("|traits"))
                    {
                        enemy.Traits = await AssigningContent.GenericArrayAssigning(s, "|traits");
                    }
                    else if (s.Contains("|drop"))
                    {
                        try
                        {
                            var items = await AssigningContent.GenericArrayAssigning(s, "|drop", '\\', new string[] { "{{" }, new string[][] { new string[] { "}}", "\\" } });
                            foreach (string item in items)
                            {
                                if (string.IsNullOrWhiteSpace(item))
                                {
                                    if (itemToNotLookFor != null && items.Contains(itemToNotLookFor.EnglishName))
                                        enemy.WhatThisEnemyDrops.Add(itemToNotLookFor);
                                    else
                                        enemy.WhatThisEnemyDrops.Add(await GetItem(item, enemy));
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            Logger.LogConsole(e, $"Looks like something failed when assigning enemy.WhatThisEnemyDrops", $"enemy name: {enemy.EnglishName}", false);
                            Logger.LogFile(e, $"Looks like something failed when assigning enemy.WhatThisEnemyDrops", false, $"enemy name: {enemy.EnglishName}");
                        }
                    }
                    else if (s == "==Recommended Servants==" | s == "== Recommended Servants ==")
                    {
                        //enemy.RecommendedServants CBA to do right now, will do soon™
                    }
                }
            }
            return enemy;
        }

        /// <summary>
        /// This will return the servant from the servant name (will return null if we are unable to find the person)
        /// </summary>
        /// <param name="ServantName">The persons name</param>
        /// <returns></returns>
        public static async Task<FateGrandOrderPerson> GetPerson(string ServantName, PresetsForInformation presetsForInformation = PresetsForInformation.NotSet, bool GetBasicInformation = true, bool GetActiveSkills = true, bool GetPassiveSkills = true, bool GetNoblePhantasm = true, bool GetAscension = true, bool GetSkillReinforcement = true)
        {
            FateGrandOrderPerson fateGrandOrderPerson = null;
            FateGrandOrderPerson PersonToRemoveFromCache = null;

            #region Toggles For GettingInformation
            if (presetsForInformation == PresetsForInformation.BasicInformation)
            {
                GetBasicInformation = true;
                GetActiveSkills = false;
                GetPassiveSkills = false;
                GetNoblePhantasm = false;
                GetAscension = false;
                GetSkillReinforcement = false;
            }
            #endregion

            #region Getting bools
            bool GettingActiveSkills = false;
            bool GettingPassiveSkills = false;
            bool GettingNoblePhantasm = false;
            bool GettingAscension = false;
            bool GettingSkillReinforcement = false;
            int PassiveSkillsCount = 0;
            #endregion

            foreach (HtmlNode col in new HtmlWeb().Load($"https://fategrandorder.fandom.com/wiki/{ServantName}?action=edit").DocumentNode.SelectNodes("//textarea"))
            {
                //For in case we put the person in wrong
                if (string.IsNullOrEmpty(col.InnerText))
                    break;

                ServantName = ServantName.Replace("_", " ");

                if (FateGrandOrderPersonCache.FateGrandOrderPeople == null)
                    FateGrandOrderPersonCache.FateGrandOrderPeople = new List<FateGrandOrderPerson>();
                fateGrandOrderPerson = new FateGrandOrderPerson(col.InnerText, ServantName);

                #region Caching Logic
                try
                {
                    foreach (FateGrandOrderPerson fateGrandOrderPersonC in FateGrandOrderPersonCache.FateGrandOrderPeople)
                    {
                        if (fateGrandOrderPersonC.GeneratedWith == fateGrandOrderPerson.GeneratedWith && fateGrandOrderPersonC.EnglishNamePassed == fateGrandOrderPerson.EnglishNamePassed)
                        {
                            if (GetBasicInformation && fateGrandOrderPersonC.BasicInformation != null)
                            {
                                GetBasicInformation = false;
                            }
                            if (GetActiveSkills && fateGrandOrderPersonC.ActiveSkills != null)
                            {
                                GetActiveSkills = false;
                            }
                            if (GetPassiveSkills && fateGrandOrderPersonC.PassiveSkills != null)
                            {
                                GetPassiveSkills = false;
                            }
                            if (GetNoblePhantasm && fateGrandOrderPersonC.NoblePhantasms != null)
                            {
                                GetNoblePhantasm = false;
                            }
                            if (GetAscension && fateGrandOrderPersonC.Ascension != null)
                            {
                                GetAscension = false;
                            }
                            if (GetSkillReinforcement && fateGrandOrderPersonC.SkillReinforcement != null)
                            {
                                GetSkillReinforcement = false;
                            }

                            if (GetBasicInformation == false && GetActiveSkills == false && GetPassiveSkills == false && GetNoblePhantasm == false && GetAscension == false && GetSkillReinforcement == false)
                            {
                                return fateGrandOrderPersonC;
                            }
                            else
                            {
                                PersonToRemoveFromCache = fateGrandOrderPersonC;
                                fateGrandOrderPerson = fateGrandOrderPersonC;
                            }
#if DEBUG
                            fateGrandOrderPersonC.FromCache = true;
#endif
                            break;
                        }
                        else if (fateGrandOrderPersonC.GeneratedWith != fateGrandOrderPerson.GeneratedWith && fateGrandOrderPersonC.EnglishNamePassed == fateGrandOrderPerson.EnglishNamePassed)
                        {
                            PersonToRemoveFromCache = fateGrandOrderPersonC;
                            break;
                        }
                    }
                }
                catch (Exception e)
                {
                    Logger.LogConsole(e, $"Looks like something failed when accessing FateGrandOrderPeople cache", $"servant name: {fateGrandOrderPerson.EnglishNamePassed}", false);
                    Logger.LogFile(e, $"Looks like something failed when accessing FateGrandOrderPeople cache", false, $"servant name: {fateGrandOrderPerson.EnglishNamePassed}");
                }
                #endregion

                #region Assigning Parts that we're going to populate
                if (GetBasicInformation && fateGrandOrderPerson.BasicInformation == null)
                {
                    fateGrandOrderPerson.BasicInformation = new FateGrandOrderPersonBasic(ServantName);
                }
                if (GetActiveSkills && fateGrandOrderPerson.ActiveSkills == null)
                {
                    fateGrandOrderPerson.ActiveSkills = new List<ActiveSkill>();
                }
                if (GetPassiveSkills && fateGrandOrderPerson.PassiveSkills == null)
                {
                    fateGrandOrderPerson.PassiveSkills = new List<PassiveSkillList>();
                }
                if (GetNoblePhantasm && fateGrandOrderPerson.NoblePhantasms == null)
                {
                    fateGrandOrderPerson.NoblePhantasms = new List<NoblePhantasmList>();
                }
                if (GetAscension && fateGrandOrderPerson.Ascension == null)
                {
                    fateGrandOrderPerson.Ascension = new Ascension();
                }
                if (GetSkillReinforcement && fateGrandOrderPerson.SkillReinforcement == null)
                {
                    fateGrandOrderPerson.SkillReinforcement = new SkillReinforcement();
                }
                #endregion

                var resultString = Regex.Split(col.InnerText, @"\n");

                foreach (string s in resultString)
                {
                    #region Passive Skills
                    if (GettingPassiveSkills)
                    {
                        try
                        {
                            if (!string.IsNullOrWhiteSpace(s) && s[s.Length - 1] == '=')
                            {
                                if (fateGrandOrderPerson.PassiveSkills[fateGrandOrderPerson.PassiveSkills.Count - 1].Category != null)
                                    fateGrandOrderPerson.PassiveSkills.Add(new PassiveSkillList());
                                fateGrandOrderPerson.PassiveSkills[fateGrandOrderPerson.PassiveSkills.Count - 1].Category = s.Replace("=", "");
                                PassiveSkillsCount = 0;
                            }
                            else if (s.Contains("|img"))
                            {
                                fateGrandOrderPerson.PassiveSkills[fateGrandOrderPerson.PassiveSkills.Count - 1].PassiveSkills.Add(new PassiveSkills());
                                PassiveSkillsCount++;
                                if (PassiveSkillsCount == 1)
                                    fateGrandOrderPerson.PassiveSkills[fateGrandOrderPerson.PassiveSkills.Count - 1].PassiveSkills[fateGrandOrderPerson.PassiveSkills[fateGrandOrderPerson.PassiveSkills.Count - 1].PassiveSkills.Count - 1].Image = await AssigningContent.Image(s);
                                else
                                    fateGrandOrderPerson.PassiveSkills[fateGrandOrderPerson.PassiveSkills.Count - 1].PassiveSkills[fateGrandOrderPerson.PassiveSkills[fateGrandOrderPerson.PassiveSkills.Count - 1].PassiveSkills.Count - 1].Image = await AssigningContent.Image(s.Replace($"|img{PassiveSkillsCount}", "|img"));
                            }
                            else if (s.Contains("|name"))
                            {
                                if (PassiveSkillsCount == 1)
                                    fateGrandOrderPerson.PassiveSkills[fateGrandOrderPerson.PassiveSkills.Count - 1].PassiveSkills[fateGrandOrderPerson.PassiveSkills[fateGrandOrderPerson.PassiveSkills.Count - 1].PassiveSkills.Count - 1].Name = await AssigningContent.GenericAssigning(s, "|name");
                                else
                                    fateGrandOrderPerson.PassiveSkills[fateGrandOrderPerson.PassiveSkills.Count - 1].PassiveSkills[fateGrandOrderPerson.PassiveSkills[fateGrandOrderPerson.PassiveSkills.Count - 1].PassiveSkills.Count - 1].Name = await AssigningContent.GenericAssigning(s, $"|name{PassiveSkillsCount}");
                            }
                            else if (s.Contains("|rank"))
                            {
                                if (PassiveSkillsCount == 1)
                                    fateGrandOrderPerson.PassiveSkills[fateGrandOrderPerson.PassiveSkills.Count - 1].PassiveSkills[fateGrandOrderPerson.PassiveSkills[fateGrandOrderPerson.PassiveSkills.Count - 1].PassiveSkills.Count - 1].Rank = await AssigningContent.GenericAssigning(s, "|rank");
                                else
                                    fateGrandOrderPerson.PassiveSkills[fateGrandOrderPerson.PassiveSkills.Count - 1].PassiveSkills[fateGrandOrderPerson.PassiveSkills[fateGrandOrderPerson.PassiveSkills.Count - 1].PassiveSkills.Count - 1].Rank = await AssigningContent.GenericAssigning(s, $"|rank{PassiveSkillsCount}");
                            }
                            else if (s.Contains("|effect"))
                            {
                                if (PassiveSkillsCount == 1)
                                    fateGrandOrderPerson.PassiveSkills[fateGrandOrderPerson.PassiveSkills.Count - 1].PassiveSkills[fateGrandOrderPerson.PassiveSkills[fateGrandOrderPerson.PassiveSkills.Count - 1].PassiveSkills.Count - 1].Effect = await AssigningContent.GenericArrayAssigning(s, "|effect", PartsToReplace: new string[][] { new string[] { "<br/>", "\\" } }, CharToSplitWith: '\\');
                                else
                                    fateGrandOrderPerson.PassiveSkills[fateGrandOrderPerson.PassiveSkills.Count - 1].PassiveSkills[fateGrandOrderPerson.PassiveSkills[fateGrandOrderPerson.PassiveSkills.Count - 1].PassiveSkills.Count - 1].Effect = await AssigningContent.GenericArrayAssigning(s, $"|effect{PassiveSkillsCount}", PartsToReplace: new string[][] { new string[] { "<br/>", "\\" } }, CharToSplitWith: '\\');
                            }
                        }
                        catch (Exception e)
                        {
                            Logger.LogConsole(e, $"Looks like something failed when assigning something in fateGrandOrderPerson.PassiveSkills", $"servant name: {fateGrandOrderPerson.EnglishNamePassed}", false);
                            Logger.LogFile(e, $"Looks like something failed when assigning something in fateGrandOrderPerson.PassiveSkills", false, $"servant name: {fateGrandOrderPerson.EnglishNamePassed}");
                        }
                    }
                    #endregion

                    #region Active Skills
                    if (GettingActiveSkills)
                    {
                        try
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
                                if (s.IndexOf("|") != -1)
                                    fateGrandOrderPerson.ActiveSkills[fateGrandOrderPerson.ActiveSkills.Count - 1].Name = s.Remove(s.IndexOf("|")).Replace(@"{{:", "");
                                else
                                    fateGrandOrderPerson.ActiveSkills[fateGrandOrderPerson.ActiveSkills.Count - 1].Name = s.Replace(@"{{:", "").Replace("}}", "");
                                fateGrandOrderPerson.ActiveSkills[fateGrandOrderPerson.ActiveSkills.Count - 1] = await GetSkill(fateGrandOrderPerson.ActiveSkills[fateGrandOrderPerson.ActiveSkills.Count - 1]);
                            }
                        }
                        catch (Exception e)
                        {
                            Logger.LogConsole(e, $"Looks like something failed when assigning something in fateGrandOrderPerson.ActiveSkills", $"servant name: {fateGrandOrderPerson.EnglishNamePassed}", false);
                            Logger.LogFile(e, $"Looks like something failed when assigning something in fateGrandOrderPerson.ActiveSkills", false, $"servant name: {fateGrandOrderPerson.EnglishNamePassed}");
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
                            try
                            {
                                fateGrandOrderPerson.NoblePhantasms[fateGrandOrderPerson.NoblePhantasms.Count - 1].NoblePhantasm.IsVideo = true;
                                fateGrandOrderPerson.NoblePhantasms[fateGrandOrderPerson.NoblePhantasms.Count - 1].NoblePhantasm.VideoInformation = new VideoInformation
                                {
                                    Name = s.Replace("[[File:", ""),
                                };
                                fateGrandOrderPerson.NoblePhantasms[fateGrandOrderPerson.NoblePhantasms.Count - 1].NoblePhantasm.VideoInformation.Name = fateGrandOrderPerson.NoblePhantasms[fateGrandOrderPerson.NoblePhantasms.Count - 1].NoblePhantasm.VideoInformation.Name.Remove(fateGrandOrderPerson.NoblePhantasms[fateGrandOrderPerson.NoblePhantasms.Count - 1].NoblePhantasm.VideoInformation.Name.ToLower().LastIndexOf("c") - 1);
                                if (fateGrandOrderPerson.NoblePhantasms[fateGrandOrderPerson.NoblePhantasms.Count - 1].NoblePhantasm.VideoInformation.Name.LastIndexOf("|") != -1) { fateGrandOrderPerson.NoblePhantasms[fateGrandOrderPerson.NoblePhantasms.Count - 1].NoblePhantasm.VideoInformation.Name = fateGrandOrderPerson.NoblePhantasms[fateGrandOrderPerson.NoblePhantasms.Count - 1].NoblePhantasm.VideoInformation.Name.Remove(fateGrandOrderPerson.NoblePhantasms[fateGrandOrderPerson.NoblePhantasms.Count - 1].NoblePhantasm.VideoInformation.Name.ToLower().LastIndexOf("|")); }
                                fateGrandOrderPerson.NoblePhantasms[fateGrandOrderPerson.NoblePhantasms.Count - 1].NoblePhantasm.VideoInformation.Uri = fateGrandOrderPerson.NoblePhantasms[fateGrandOrderPerson.NoblePhantasms.Count - 1].NoblePhantasm.VideoInformation.Name;
                            }
                            catch (Exception e)
                            {
                                Logger.LogConsole(e, $"Looks like something failed when assigning something in fateGrandOrderPerson.NoblePhantasms[fateGrandOrderPerson.NoblePhantasms.Count - 1].NoblePhantasm.VideoInformation", $"servant name: {fateGrandOrderPerson.EnglishNamePassed}", false);
                                Logger.LogFile(e, $"Looks like something failed when assigning something in fateGrandOrderPerson.NoblePhantasms[fateGrandOrderPerson.NoblePhantasms.Count - 1].NoblePhantasm.VideoInformation", false, $"servant name: {fateGrandOrderPerson.EnglishNamePassed}");
                            }
                        }
                        else if (s == "|-|")
                        {
                            fateGrandOrderPerson.NoblePhantasms.Add(new NoblePhantasmList());
                        }
                        else if (s.Contains("|name"))
                        {
                            fateGrandOrderPerson.NoblePhantasms[fateGrandOrderPerson.NoblePhantasms.Count - 1].NoblePhantasm.Name = await AssigningContent.GenericAssigning(s, "|name", PartsToReplace: new string[][] { new string[] { "<br/>", "\n" } });
                            if (s.Contains("Video"))
                            {
                                fateGrandOrderPerson.NoblePhantasms[fateGrandOrderPerson.NoblePhantasms.Count - 1].NoblePhantasm.IsVideo = true;
                                fateGrandOrderPerson.NoblePhantasms[fateGrandOrderPerson.NoblePhantasms.Count - 1].NoblePhantasm.VideoInformation = new VideoInformation();
                            }
                        }
                        else if (s.Contains("|rank"))
                        {
                            fateGrandOrderPerson.NoblePhantasms[fateGrandOrderPerson.NoblePhantasms.Count - 1].NoblePhantasm.Rank = await AssigningContent.GenericAssigning(s, "|rank");
                        }
                        else if (s.Contains("|classification"))
                        {
                            fateGrandOrderPerson.NoblePhantasms[fateGrandOrderPerson.NoblePhantasms.Count - 1].NoblePhantasm.Classification = await AssigningContent.GenericAssigning(s, "|classification");
                        }
                        else if (s.Contains("|type"))
                        {
                            fateGrandOrderPerson.NoblePhantasms[fateGrandOrderPerson.NoblePhantasms.Count - 1].NoblePhantasm.Type = await AssigningContent.GenericAssigning(s, "|type");
                        }
                        else if (s.Contains("|hitcount"))
                        {
                            fateGrandOrderPerson.NoblePhantasms[fateGrandOrderPerson.NoblePhantasms.Count - 1].NoblePhantasm.HitCount = await AssigningContent.GenericAssigning(s, "|hitcount");
                        }
                        else if (s.Contains("|effect"))
                        {
                            fateGrandOrderPerson.NoblePhantasms[fateGrandOrderPerson.NoblePhantasms.Count - 1].NoblePhantasm.Effects = await AssigningContent.GenericArrayAssigning(s, "|effect", PartsToReplace: new string[][] { new string[] { "<br/>", "," } });
                        }
                        else if (s.Contains("|overchargeeffect"))
                        {
                            fateGrandOrderPerson.NoblePhantasms[fateGrandOrderPerson.NoblePhantasms.Count - 1].NoblePhantasm.OverChargeEffect = await AssigningContent.GenericArrayAssigning(s, "|overchargeeffect", PartsToReplace: new string[][] { new string[] { "<br/>", "," } });
                        }
                        else if (s.Contains("|leveleffect"))
                        {
                            fateGrandOrderPerson.NoblePhantasms[fateGrandOrderPerson.NoblePhantasms.Count - 1].NoblePhantasm.LevelEffect = new LevelEffect();
                            fateGrandOrderPerson.NoblePhantasms[fateGrandOrderPerson.NoblePhantasms.Count - 1].NoblePhantasm.LevelEffect.Name = await AssigningContent.GenericAssigning(s, "|leveleffect");
                        }
                        else if (s.Contains("|l1"))
                        {
                            fateGrandOrderPerson.NoblePhantasms[fateGrandOrderPerson.NoblePhantasms.Count - 1].NoblePhantasm.LevelEffect.NPLevel1 = await AssigningContent.GenericAssigning(s, "|l1");
                        }
                        else if (s.Contains("|l2"))
                        {
                            fateGrandOrderPerson.NoblePhantasms[fateGrandOrderPerson.NoblePhantasms.Count - 1].NoblePhantasm.LevelEffect.NPLevel2 = await AssigningContent.GenericAssigning(s, "|l2");
                        }
                        else if (s.Contains("|l3"))
                        {
                            fateGrandOrderPerson.NoblePhantasms[fateGrandOrderPerson.NoblePhantasms.Count - 1].NoblePhantasm.LevelEffect.NPLevel3 = await AssigningContent.GenericAssigning(s, "|l3");
                        }
                        else if (s.Contains("|l4"))
                        {
                            fateGrandOrderPerson.NoblePhantasms[fateGrandOrderPerson.NoblePhantasms.Count - 1].NoblePhantasm.LevelEffect.NPLevel4 = await AssigningContent.GenericAssigning(s, "|l4");
                        }
                        else if (s.Contains("|l5"))
                        {
                            fateGrandOrderPerson.NoblePhantasms[fateGrandOrderPerson.NoblePhantasms.Count - 1].NoblePhantasm.LevelEffect.NPLevel5 = await AssigningContent.GenericAssigning(s, "|l5");
                        }
                        else if (s.Contains("|chargeeffect"))
                        {
                            fateGrandOrderPerson.NoblePhantasms[fateGrandOrderPerson.NoblePhantasms.Count - 1].NoblePhantasm.ChargeEffect = new ChargeEffect();
                            fateGrandOrderPerson.NoblePhantasms[fateGrandOrderPerson.NoblePhantasms.Count - 1].NoblePhantasm.ChargeEffect.Name = await AssigningContent.GenericAssigning(s, "|chargeeffect");
                        }
                        else if (s.Contains("|c1"))
                        {
                            fateGrandOrderPerson.NoblePhantasms[fateGrandOrderPerson.NoblePhantasms.Count - 1].NoblePhantasm.ChargeEffect.NPLevel1 = await AssigningContent.GenericAssigning(s, "|c1");
                        }
                        else if (s.Contains("|c2"))
                        {
                            fateGrandOrderPerson.NoblePhantasms[fateGrandOrderPerson.NoblePhantasms.Count - 1].NoblePhantasm.ChargeEffect.NPLevel2 = await AssigningContent.GenericAssigning(s, "|c2");
                        }
                        else if (s.Contains("|c3"))
                        {
                            fateGrandOrderPerson.NoblePhantasms[fateGrandOrderPerson.NoblePhantasms.Count - 1].NoblePhantasm.ChargeEffect.NPLevel3 = await AssigningContent.GenericAssigning(s, "|c3");
                        }
                        else if (s.Contains("|c4"))
                        {
                            fateGrandOrderPerson.NoblePhantasms[fateGrandOrderPerson.NoblePhantasms.Count - 1].NoblePhantasm.ChargeEffect.NPLevel4 = await AssigningContent.GenericAssigning(s, "|c4");
                        }
                        else if (s.Contains("|c5"))
                        {
                            fateGrandOrderPerson.NoblePhantasms[fateGrandOrderPerson.NoblePhantasms.Count - 1].NoblePhantasm.ChargeEffect.NPLevel5 = await AssigningContent.GenericAssigning(s, "|c5");
                        }
                    }
                    #endregion

                    #region Ascension
                    if (GettingAscension)
                    {
                        if (s.Length >= 3 && s.Remove(3) == "|11")
                        {
                            fateGrandOrderPerson.Ascension.Ascension1 = await AssigningContent.Item(null, null, null, true, "1", fateGrandOrderPerson.Ascension.Ascension1);
                            fateGrandOrderPerson.Ascension.Ascension1.Item1 = await AssigningContent.Item(s, fateGrandOrderPerson.Ascension.Ascension1.Item1, "11");
                        }
                        else if (s.Length >= 3 && s.Remove(3) == "|12")
                        {
                            fateGrandOrderPerson.Ascension.Ascension1.Item2 = await AssigningContent.Item(s, fateGrandOrderPerson.Ascension.Ascension1.Item2, "12");
                        }
                        else if (s.Length >= 3 && s.Remove(3) == "|13")
                        {
                            fateGrandOrderPerson.Ascension.Ascension1.Item3 = await AssigningContent.Item(s, fateGrandOrderPerson.Ascension.Ascension1.Item3, "13");
                        }
                        else if (s.Length >= 3 && s.Remove(3) == "|14")
                        {
                            fateGrandOrderPerson.Ascension.Ascension1.Item4 = await AssigningContent.Item(s, fateGrandOrderPerson.Ascension.Ascension1.Item4, "14");
                        }
                        else if (s.Length >= 3 && s.Remove(3) == "|21")
                        {
                            fateGrandOrderPerson.Ascension.Ascension2 = await AssigningContent.Item(null, null, null, true, "2", fateGrandOrderPerson.Ascension.Ascension2);
                            fateGrandOrderPerson.Ascension.Ascension2.Item1 = await AssigningContent.Item(s, fateGrandOrderPerson.Ascension.Ascension2.Item2, "21");
                        }
                        else if (s.Length >= 3 && s.Remove(3) == "|22")
                        {
                            fateGrandOrderPerson.Ascension.Ascension2.Item2 = await AssigningContent.Item(s, fateGrandOrderPerson.Ascension.Ascension2.Item2, "22");
                        }
                        else if (s.Length >= 3 && s.Remove(3) == "|23")
                        {
                            fateGrandOrderPerson.Ascension.Ascension2.Item3 = await AssigningContent.Item(s, fateGrandOrderPerson.Ascension.Ascension2.Item3, "23");
                        }
                        else if (s.Length >= 3 && s.Remove(3) == "|24")
                        {
                            fateGrandOrderPerson.Ascension.Ascension2.Item4 = await AssigningContent.Item(s, fateGrandOrderPerson.Ascension.Ascension2.Item4, "24");
                        }
                        else if (s.Length >= 3 && s.Remove(3) == "|31")
                        {
                            fateGrandOrderPerson.Ascension.Ascension3 = await AssigningContent.Item(null, null, null, true, "3", fateGrandOrderPerson.Ascension.Ascension3);
                            fateGrandOrderPerson.Ascension.Ascension3.Item1 = await AssigningContent.Item(s, fateGrandOrderPerson.Ascension.Ascension3.Item2, "31");
                        }
                        else if (s.Length >= 3 && s.Remove(3) == "|32")
                        {
                            fateGrandOrderPerson.Ascension.Ascension3.Item2 = await AssigningContent.Item(s, fateGrandOrderPerson.Ascension.Ascension3.Item2, "32");
                        }
                        else if (s.Length >= 3 && s.Remove(3) == "|33")
                        {
                            fateGrandOrderPerson.Ascension.Ascension3.Item3 = await AssigningContent.Item(s, fateGrandOrderPerson.Ascension.Ascension3.Item3, "33");
                        }
                        else if (s.Length >= 3 && s.Remove(3) == "|34")
                        {
                            fateGrandOrderPerson.Ascension.Ascension3.Item4 = await AssigningContent.Item(s, fateGrandOrderPerson.Ascension.Ascension3.Item4, "34");
                        }
                        else if (s.Length >= 3 && s.Remove(3) == "|41")
                        {
                            fateGrandOrderPerson.Ascension.Ascension4 = await AssigningContent.Item(null, null, null, true, "4", fateGrandOrderPerson.Ascension.Ascension4);
                            fateGrandOrderPerson.Ascension.Ascension4.Item1 = await AssigningContent.Item(s, fateGrandOrderPerson.Ascension.Ascension4.Item2, "41");
                        }
                        else if (s.Length >= 3 && s.Remove(3) == "|42")
                        {
                            fateGrandOrderPerson.Ascension.Ascension4.Item2 = await AssigningContent.Item(s, fateGrandOrderPerson.Ascension.Ascension4.Item2, "42");
                        }
                        else if (s.Length >= 3 && s.Remove(3) == "|43")
                        {
                            fateGrandOrderPerson.Ascension.Ascension4.Item3 = await AssigningContent.Item(s, fateGrandOrderPerson.Ascension.Ascension4.Item3, "43");
                        }
                        else if (s.Length >= 3 && s.Remove(3) == "|44")
                        {
                            fateGrandOrderPerson.Ascension.Ascension4.Item4 = await AssigningContent.Item(s, fateGrandOrderPerson.Ascension.Ascension4.Item4, "44");
                        }
                        else if (s.Length >= 3 && s.Remove(3) == "|51")
                        {
                            fateGrandOrderPerson.Ascension.Ascension5 = await AssigningContent.Item(null, null, null, true, "5", fateGrandOrderPerson.Ascension.Ascension5);
                            fateGrandOrderPerson.Ascension.Ascension5.Item1 = await AssigningContent.Item(s, fateGrandOrderPerson.Ascension.Ascension5.Item2, "51");
                        }
                        else if (s.Length >= 3 && s.Remove(3) == "|52")
                        {
                            fateGrandOrderPerson.Ascension.Ascension5.Item2 = await AssigningContent.Item(s, fateGrandOrderPerson.Ascension.Ascension5.Item2, "52");
                        }
                        else if (s.Length >= 3 && s.Remove(3) == "|53")
                        {
                            fateGrandOrderPerson.Ascension.Ascension5.Item3 = await AssigningContent.Item(s, fateGrandOrderPerson.Ascension.Ascension5.Item3, "53");
                        }
                        else if (s.Length >= 3 && s.Remove(3) == "|54")
                        {
                            fateGrandOrderPerson.Ascension.Ascension5.Item4 = await AssigningContent.Item(s, fateGrandOrderPerson.Ascension.Ascension5.Item4, "54");
                        }
                        else if (s.Contains("|1qp"))
                        {
                            fateGrandOrderPerson.Ascension.Ascension1.QP = await AssigningContent.GenericAssigning(s, "|1qp", new string[] { "{{Inum|{{QP}}|", "}}" });
                        }
                        else if (s.Contains("|2qp"))
                        {
                            fateGrandOrderPerson.Ascension.Ascension2.QP = await AssigningContent.GenericAssigning(s, "|2qp", new string[] { "{{Inum|{{QP}}|", "}}" });
                        }
                        else if (s.Contains("|3qp"))
                        {
                            fateGrandOrderPerson.Ascension.Ascension3.QP = await AssigningContent.GenericAssigning(s, "|3qp", new string[] { "{{Inum|{{QP}}|", "}}" });
                        }
                        else if (s.Contains("|4qp"))
                        {
                            fateGrandOrderPerson.Ascension.Ascension4.QP = await AssigningContent.GenericAssigning(s, "|4qp", new string[] { "{{Inum|{{QP}}|", "}}" });
                        }
                        else if (s.Contains("|5qp"))
                        {
                            fateGrandOrderPerson.Ascension.Ascension5.QP = await AssigningContent.GenericAssigning(s, "|5qp", new string[] { "{{Inum|{{QP}}|", "}}" });
                        }
                    }
                    #endregion

                    #region Skill Reinforcement
                    if (GettingSkillReinforcement)
                    {
                        if (s.Length >= 3 && s.Remove(3) == "|11")
                        {
                            fateGrandOrderPerson.SkillReinforcement.Ascension1 = await AssigningContent.Item(null, null, null, true, "1", fateGrandOrderPerson.SkillReinforcement.Ascension1);
                            fateGrandOrderPerson.SkillReinforcement.Ascension1.Item1 = await AssigningContent.Item(s, fateGrandOrderPerson.SkillReinforcement.Ascension1.Item2, "11");
                        }
                        else if (s.Length >= 3 && s.Remove(3) == "|12")
                        {
                            fateGrandOrderPerson.SkillReinforcement.Ascension1.Item2 = await AssigningContent.Item(s, fateGrandOrderPerson.SkillReinforcement.Ascension1.Item2, "12");
                        }
                        else if (s.Length >= 3 && s.Remove(3) == "|13")
                        {
                            fateGrandOrderPerson.SkillReinforcement.Ascension1.Item3 = await AssigningContent.Item(s, fateGrandOrderPerson.SkillReinforcement.Ascension1.Item3, "13");
                        }
                        else if (s.Length >= 3 && s.Remove(3) == "|14")
                        {
                            fateGrandOrderPerson.SkillReinforcement.Ascension1.Item4 = await AssigningContent.Item(s, fateGrandOrderPerson.SkillReinforcement.Ascension1.Item4, "14");
                        }
                        else if (s.Length >= 3 && s.Remove(3) == "|21")
                        {
                            fateGrandOrderPerson.SkillReinforcement.Ascension2 = await AssigningContent.Item(null, null, null, true, "2", fateGrandOrderPerson.SkillReinforcement.Ascension2);
                            fateGrandOrderPerson.SkillReinforcement.Ascension2.Item1 = await AssigningContent.Item(s, fateGrandOrderPerson.SkillReinforcement.Ascension2.Item2, "21");
                        }
                        else if (s.Length >= 3 && s.Remove(3) == "|22")
                        {
                            fateGrandOrderPerson.SkillReinforcement.Ascension2.Item2 = await AssigningContent.Item(s, fateGrandOrderPerson.SkillReinforcement.Ascension2.Item2, "22");
                        }
                        else if (s.Length >= 3 && s.Remove(3) == "|23")
                        {
                            fateGrandOrderPerson.SkillReinforcement.Ascension2.Item3 = await AssigningContent.Item(s, fateGrandOrderPerson.SkillReinforcement.Ascension2.Item3, "23");
                        }
                        else if (s.Length >= 3 && s.Remove(3) == "|24")
                        {
                            fateGrandOrderPerson.SkillReinforcement.Ascension2.Item4 = await AssigningContent.Item(s, fateGrandOrderPerson.SkillReinforcement.Ascension2.Item4, "24");
                        }
                        else if (s.Length >= 3 && s.Remove(3) == "|31")
                        {
                            fateGrandOrderPerson.SkillReinforcement.Ascension3 = await AssigningContent.Item(null, null, null, true, "3", fateGrandOrderPerson.SkillReinforcement.Ascension3);
                            fateGrandOrderPerson.SkillReinforcement.Ascension3.Item1 = await AssigningContent.Item(s, fateGrandOrderPerson.SkillReinforcement.Ascension3.Item2, "31");
                        }
                        else if (s.Length >= 3 && s.Remove(3) == "|32")
                        {
                            fateGrandOrderPerson.SkillReinforcement.Ascension3.Item2 = await AssigningContent.Item(s, fateGrandOrderPerson.SkillReinforcement.Ascension3.Item2, "32");
                        }
                        else if (s.Length >= 3 && s.Remove(3) == "|33")
                        {
                            fateGrandOrderPerson.SkillReinforcement.Ascension3.Item3 = await AssigningContent.Item(s, fateGrandOrderPerson.SkillReinforcement.Ascension3.Item3, "33");
                        }
                        else if (s.Length >= 3 && s.Remove(3) == "|34")
                        {
                            fateGrandOrderPerson.SkillReinforcement.Ascension3.Item4 = await AssigningContent.Item(s, fateGrandOrderPerson.SkillReinforcement.Ascension3.Item4, "34");
                        }
                        else if (s.Length >= 3 && s.Remove(3) == "|41")
                        {
                            fateGrandOrderPerson.SkillReinforcement.Ascension4 = await AssigningContent.Item(null, null, null, true, "4", fateGrandOrderPerson.SkillReinforcement.Ascension4);
                            fateGrandOrderPerson.SkillReinforcement.Ascension4.Item1 = await AssigningContent.Item(s, fateGrandOrderPerson.SkillReinforcement.Ascension4.Item2, "41");
                        }
                        else if (s.Length >= 3 && s.Remove(3) == "|42")
                        {
                            fateGrandOrderPerson.SkillReinforcement.Ascension4.Item2 = await AssigningContent.Item(s, fateGrandOrderPerson.SkillReinforcement.Ascension4.Item2, "42");
                        }
                        else if (s.Length >= 3 && s.Remove(3) == "|43")
                        {
                            fateGrandOrderPerson.SkillReinforcement.Ascension4.Item3 = await AssigningContent.Item(s, fateGrandOrderPerson.SkillReinforcement.Ascension4.Item3, "43");
                        }
                        else if (s.Length >= 3 && s.Remove(3) == "|44")
                        {
                            fateGrandOrderPerson.SkillReinforcement.Ascension4.Item4 = await AssigningContent.Item(s, fateGrandOrderPerson.SkillReinforcement.Ascension4.Item4, "44");
                        }
                        else if (s.Length >= 3 && s.Remove(3) == "|51")
                        {
                            fateGrandOrderPerson.SkillReinforcement.Ascension5 = await AssigningContent.Item(null, null, null, true, "5", fateGrandOrderPerson.SkillReinforcement.Ascension5);
                            fateGrandOrderPerson.SkillReinforcement.Ascension5.Item1 = await AssigningContent.Item(s, fateGrandOrderPerson.SkillReinforcement.Ascension5.Item2, "51");
                        }
                        else if (s.Length >= 3 && s.Remove(3) == "|52")
                        {
                            fateGrandOrderPerson.SkillReinforcement.Ascension5.Item2 = await AssigningContent.Item(s, fateGrandOrderPerson.SkillReinforcement.Ascension5.Item2, "52");
                        }
                        else if (s.Length >= 3 && s.Remove(3) == "|53")
                        {
                            fateGrandOrderPerson.SkillReinforcement.Ascension5.Item3 = await AssigningContent.Item(s, fateGrandOrderPerson.SkillReinforcement.Ascension5.Item3, "53");
                        }
                        else if (s.Length >= 3 && s.Remove(3) == "|54")
                        {
                            fateGrandOrderPerson.SkillReinforcement.Ascension5.Item4 = await AssigningContent.Item(s, fateGrandOrderPerson.SkillReinforcement.Ascension5.Item4, "54");
                        }
                        else if (s.Length >= 3 && s.Remove(3) == "|61")
                        {
                            fateGrandOrderPerson.SkillReinforcement.Ascension6 = await AssigningContent.Item(null, null, null, true, "6", fateGrandOrderPerson.SkillReinforcement.Ascension6);
                            fateGrandOrderPerson.SkillReinforcement.Ascension6.Item1 = await AssigningContent.Item(s, fateGrandOrderPerson.SkillReinforcement.Ascension6.Item2, "61");
                        }
                        else if (s.Length >= 3 && s.Remove(3) == "|62")
                        {
                            fateGrandOrderPerson.SkillReinforcement.Ascension6.Item2 = await AssigningContent.Item(s, fateGrandOrderPerson.SkillReinforcement.Ascension6.Item2, "62");
                        }
                        else if (s.Length >= 3 && s.Remove(3) == "|63")
                        {
                            fateGrandOrderPerson.SkillReinforcement.Ascension6.Item3 = await AssigningContent.Item(s, fateGrandOrderPerson.SkillReinforcement.Ascension6.Item3, "63");
                        }
                        else if (s.Length >= 3 && s.Remove(3) == "|64")
                        {
                            fateGrandOrderPerson.SkillReinforcement.Ascension6.Item4 = await AssigningContent.Item(s, fateGrandOrderPerson.SkillReinforcement.Ascension6.Item4, "64");
                        }
                        else if (s.Length >= 3 && s.Remove(3) == "|71")
                        {
                            fateGrandOrderPerson.SkillReinforcement.Ascension7 = await AssigningContent.Item(null, null, null, true, "7", fateGrandOrderPerson.SkillReinforcement.Ascension7);
                            fateGrandOrderPerson.SkillReinforcement.Ascension7.Item1 = await AssigningContent.Item(s, fateGrandOrderPerson.SkillReinforcement.Ascension7.Item2, "71");
                        }
                        else if (s.Length >= 3 && s.Remove(3) == "|72")
                        {
                            fateGrandOrderPerson.SkillReinforcement.Ascension7.Item2 = await AssigningContent.Item(s, fateGrandOrderPerson.SkillReinforcement.Ascension7.Item2, "72");
                        }
                        else if (s.Length >= 3 && s.Remove(3) == "|73")
                        {
                            fateGrandOrderPerson.SkillReinforcement.Ascension7.Item3 = await AssigningContent.Item(s, fateGrandOrderPerson.SkillReinforcement.Ascension7.Item3, "73");
                        }
                        else if (s.Length >= 3 && s.Remove(3) == "|74")
                        {
                            fateGrandOrderPerson.SkillReinforcement.Ascension7.Item4 = await AssigningContent.Item(s, fateGrandOrderPerson.SkillReinforcement.Ascension7.Item4, "74");
                        }
                        else if (s.Length >= 3 && s.Remove(3) == "|81")
                        {
                            fateGrandOrderPerson.SkillReinforcement.Ascension8 = await AssigningContent.Item(null, null, null, true, "8", fateGrandOrderPerson.SkillReinforcement.Ascension8);
                            fateGrandOrderPerson.SkillReinforcement.Ascension8.Item1 = await AssigningContent.Item(s, fateGrandOrderPerson.SkillReinforcement.Ascension8.Item2, "82");
                        }
                        else if (s.Length >= 3 && s.Remove(3) == "|82")
                        {
                            fateGrandOrderPerson.SkillReinforcement.Ascension8.Item2 = await AssigningContent.Item(s, fateGrandOrderPerson.SkillReinforcement.Ascension8.Item2, "82");
                        }
                        else if (s.Length >= 3 && s.Remove(3) == "|83")
                        {
                            fateGrandOrderPerson.SkillReinforcement.Ascension8.Item3 = await AssigningContent.Item(s, fateGrandOrderPerson.SkillReinforcement.Ascension8.Item3, "83");
                        }
                        else if (s.Length >= 3 && s.Remove(3) == "|84")
                        {
                            fateGrandOrderPerson.SkillReinforcement.Ascension8.Item4 = await AssigningContent.Item(s, fateGrandOrderPerson.SkillReinforcement.Ascension8.Item4, "84");
                        }
                        else if (s.Length >= 3 && s.Remove(3) == "|91")
                        {
                            fateGrandOrderPerson.SkillReinforcement.Ascension9 = await AssigningContent.Item(null, null, null, true, "9", fateGrandOrderPerson.SkillReinforcement.Ascension9);
                            fateGrandOrderPerson.SkillReinforcement.Ascension9.Item1 = await AssigningContent.Item(s, fateGrandOrderPerson.SkillReinforcement.Ascension9.Item2, "92");
                        }
                        else if (s.Length >= 3 && s.Remove(3) == "|92")
                        {
                            fateGrandOrderPerson.SkillReinforcement.Ascension9.Item2 = await AssigningContent.Item(s, fateGrandOrderPerson.SkillReinforcement.Ascension9.Item2, "92");
                        }
                        else if (s.Length >= 3 && s.Remove(3) == "|93")
                        {
                            fateGrandOrderPerson.SkillReinforcement.Ascension9.Item3 = await AssigningContent.Item(s, fateGrandOrderPerson.SkillReinforcement.Ascension9.Item3, "93");
                        }
                        else if (s.Length >= 3 && s.Remove(3) == "|94")
                        {
                            fateGrandOrderPerson.SkillReinforcement.Ascension9.Item4 = await AssigningContent.Item(s, fateGrandOrderPerson.SkillReinforcement.Ascension9.Item4, "94");
                        }
                        else if (s.Contains("|1qp"))
                        {
                            fateGrandOrderPerson.SkillReinforcement.Ascension1.QP = await AssigningContent.GenericAssigning(s, "|1qp", new string[] { "{{Inum|{{QP}}|", "}}" });
                        }
                        else if (s.Contains("|2qp"))
                        {
                            fateGrandOrderPerson.SkillReinforcement.Ascension2.QP = await AssigningContent.GenericAssigning(s, "|2qp", new string[] { "{{Inum|{{QP}}|", "}}" });
                        }
                        else if (s.Contains("|3qp"))
                        {
                            fateGrandOrderPerson.SkillReinforcement.Ascension3.QP = await AssigningContent.GenericAssigning(s, "|3qp", new string[] { "{{Inum|{{QP}}|", "}}" });
                        }
                        else if (s.Contains("|4qp"))
                        {
                            fateGrandOrderPerson.SkillReinforcement.Ascension4.QP = await AssigningContent.GenericAssigning(s, "|4qp", new string[] { "{{Inum|{{QP}}|", "}}" });
                        }
                        else if (s.Contains("|5qp"))
                        {
                            fateGrandOrderPerson.SkillReinforcement.Ascension5.QP = await AssigningContent.GenericAssigning(s, "|5qp", new string[] { "{{Inum|{{QP}}|", "}}" });
                        }
                        else if (s.Contains("|6qp"))
                        {
                            fateGrandOrderPerson.SkillReinforcement.Ascension6.QP = await AssigningContent.GenericAssigning(s, "|6qp", new string[] { "{{Inum|{{QP}}|", "}}" });
                        }
                        else if (s.Contains("|7qp"))
                        {
                            fateGrandOrderPerson.SkillReinforcement.Ascension7.QP = await AssigningContent.GenericAssigning(s, "|7qp", new string[] { "{{Inum|{{QP}}|", "}}" });
                        }
                        else if (s.Contains("|8qp"))
                        {
                            fateGrandOrderPerson.SkillReinforcement.Ascension8.QP = await AssigningContent.GenericAssigning(s, "|8qp", new string[] { "{{Inum|{{QP}}|", "}}" });
                        }
                        else if (s.Contains("|9qp"))
                        {
                            fateGrandOrderPerson.SkillReinforcement.Ascension9.QP = await AssigningContent.GenericAssigning(s, "|9qp", new string[] { "{{Inum|{{QP}}|", "}}" });
                        }
                    }
                    #endregion

                    #region Trigger Skills Logic
                    if (GetPassiveSkills && s == "== Passive Skills ==" | s == "==Passive Skills==")
                    {
                        fateGrandOrderPerson.PassiveSkills.Add(new PassiveSkillList());
                        GettingPassiveSkills = true;
                    }
                    else if (GetActiveSkills && s == "== Active Skills ==" | s == "==Active Skills==")
                    {
                        GettingActiveSkills = true;
                    }
                    else if (GetAscension && s == "== Ascension ==" | s == "==Ascension==")
                    {
                        fateGrandOrderPerson.Ascension = new Ascension();
                        GettingAscension = true;
                    }
                    else if (GetSkillReinforcement && s == "== Skill Reinforcement ==" | s == "==Skill Reinforcement==")
                    {
                        fateGrandOrderPerson.SkillReinforcement = new SkillReinforcement();
                        GettingSkillReinforcement = true;
                    }
                    else if (GetNoblePhantasm && s == "== Noble Phantasm ==" | s == "==Noble Phantasm==")
                    {
                        GettingNoblePhantasm = true;
                        fateGrandOrderPerson.NoblePhantasms.Add(new NoblePhantasmList());
                    }
                    else if (GettingActiveSkills | GettingPassiveSkills | GettingNoblePhantasm && FixString(s) == "</tabber>")
                    {
                        GettingActiveSkills = false;
                        GettingPassiveSkills = false;
                        GettingNoblePhantasm = false;
                    }
                    else if (GettingPassiveSkills | GettingAscension | GettingSkillReinforcement && s == @"}}")
                    {
                        if (fateGrandOrderPerson.PassiveSkills[fateGrandOrderPerson.PassiveSkills.Count - 1].Category == null)
                            GettingPassiveSkills = false;
                        GettingAscension = false;
                        GettingSkillReinforcement = false;
                    }
                    #endregion

                    #region Basic Information
                    if (GetBasicInformation)
                    {
                        if (s.Contains("|jname"))
                        {
                            fateGrandOrderPerson.BasicInformation.JapaneseName = await AssigningContent.GenericAssigning(s, "|jname");
                        }
                        else if (s.Contains("|voicea"))
                        {
                            fateGrandOrderPerson.BasicInformation.VoiceActor = await AssigningContent.GenericAssigning(s, "|voicea");
                        }
                        else if (s.Contains("|illus"))
                        {
                            fateGrandOrderPerson.BasicInformation.Illustrator = await AssigningContent.GenericAssigning(s, "|illus");
                        }
                        else if (s.Contains("|class"))
                        {
                            fateGrandOrderPerson.BasicInformation.Class = await AssigningContent.GenericAssigning(s, "|classification");
                        }
                        else if (s.Contains("|atk"))
                        {
                            fateGrandOrderPerson.BasicInformation.ATK = await AssigningContent.GenericAssigning(s, "|atk");
                        }
                        else if (s.Contains("|hp"))
                        {
                            fateGrandOrderPerson.BasicInformation.HP = await AssigningContent.GenericAssigning(s, "|hp");
                        }
                        else if (s.Contains("|gatk"))
                        {
                            fateGrandOrderPerson.BasicInformation.GrailATK = await AssigningContent.GenericAssigning(s, "|gatk");
                        }
                        else if (s.Contains("|ghp"))
                        {
                            fateGrandOrderPerson.BasicInformation.GrailHP = await AssigningContent.GenericAssigning(s, "|ghp");
                        }
                        else if (s.Contains("|stars"))
                        {
                            fateGrandOrderPerson.BasicInformation.Stars = await AssigningContent.GenericAssigning(s, "|stars");
                        }
                        else if (s.Contains("|cost"))
                        {
                            fateGrandOrderPerson.BasicInformation.Cost = await AssigningContent.GenericAssigning(s, "|cost");
                        }
                        else if (s.Contains("|cc"))
                        {
                            fateGrandOrderPerson.BasicInformation.QQQAB = await AssigningContent.GenericAssigning(s, "|cc");
                        }
                        else if (s.Contains("|mlevel"))
                        {
                            fateGrandOrderPerson.BasicInformation.MaxLevel = await AssigningContent.GenericAssigning(s, "|mlevel");
                        }
                        else if (s.Contains("|id"))
                        {
                            fateGrandOrderPerson.BasicInformation.ID = await AssigningContent.GenericAssigning(s, "|id");
                        }
                        else if (s.Contains("|attribute"))
                        {
                            fateGrandOrderPerson.BasicInformation.Attribute = await AssigningContent.GenericAssigning(s, "|attribute");
                        }
                        else if (s.Contains("|qhits"))
                        {
                            fateGrandOrderPerson.BasicInformation.QuickHits = await AssigningContent.GenericAssigning(s, "|qhits");
                        }
                        else if (s.Contains("|ahits"))
                        {
                            fateGrandOrderPerson.BasicInformation.ArtsHits = await AssigningContent.GenericAssigning(s, "|ahits");
                        }
                        else if (s.Contains("|bhits"))
                        {
                            fateGrandOrderPerson.BasicInformation.BusterHits = await AssigningContent.GenericAssigning(s, "|bhits");
                        }
                        else if (s.Contains("|ehits"))
                        {
                            fateGrandOrderPerson.BasicInformation.ExtraHits = await AssigningContent.GenericAssigning(s, "|ehits");
                        }
                        else if (s.Contains("|deathrate"))
                        {
                            fateGrandOrderPerson.BasicInformation.DeathRate = await AssigningContent.GenericAssigning(s, "|deathrate");
                        }
                        else if (s.Contains("|starabsorption"))
                        {
                            fateGrandOrderPerson.BasicInformation.StarAbsorption = await AssigningContent.GenericAssigning(s, "|starabsorption");
                        }
                        else if (s.Contains("|stargeneration"))
                        {
                            fateGrandOrderPerson.BasicInformation.StarGeneration = await AssigningContent.GenericAssigning(s, "|stargeneration");
                        }
                        else if (s.Contains("|npchargeatk"))
                        {
                            fateGrandOrderPerson.BasicInformation.NPChargeATK = await AssigningContent.GenericAssigning(s, "|npchargeatk");
                        }
                        else if (s.Contains("|npchargedef"))
                        {
                            fateGrandOrderPerson.BasicInformation.NPChargeDEF = await AssigningContent.GenericAssigning(s, "|npchargedef");
                        }
                        else if (s.Contains("|growthc"))
                        {
                            fateGrandOrderPerson.BasicInformation.GrowthCurve = await AssigningContent.GenericAssigning(s, "|growthc");
                        }
                        else if (s.Contains("|aka"))
                        {
                            fateGrandOrderPerson.BasicInformation.AKA = await AssigningContent.GenericArrayAssigning(s, "|aka", OtherPartsToRemove: new string[] { "'''", "''" });
                        }
                        else if (s.Contains("|traits"))
                        {
                            fateGrandOrderPerson.BasicInformation.Traits = await AssigningContent.GenericArrayAssigning(s, "|traits");
                        }
                        else if (s.Contains("|gender"))
                        {
                            await AssigningContent.Gender(s, fateGrandOrderPerson.BasicInformation);
                        }
                        else if (s.Contains("|alignment"))
                        {
                            fateGrandOrderPerson.BasicInformation.Alignment = await AssigningContent.GenericAssigning(s, "|alignment");
                        }
                    }
                    #endregion
                }
            }

            #region Add/Remove to cache and any returns we need to do
            if (PersonToRemoveFromCache != null)
                FateGrandOrderPersonCache.FateGrandOrderPeople.Remove(PersonToRemoveFromCache);
            if (fateGrandOrderPerson != null)
            {
                FateGrandOrderPersonCache.FateGrandOrderPeople.Add(fateGrandOrderPerson);
                return fateGrandOrderPerson;
            }
            else
            {
                return null;
            }
            #endregion
        }

        private class AssigningContent
        {
            public static async Task<string[]> GenericArrayAssigning(string s, string Assigning, char CharToSplitWith = ',', string[] OtherPartsToRemove = null, string[][] PartsToReplace = null)
            {
                s = FixString(s);
                try
                {
                    if (PartsToReplace != null)
                        foreach (string[] PartToReplace in PartsToReplace)
                        {
                            s = s.Replace(PartToReplace[0], PartToReplace[1]);
                        }
                    if (OtherPartsToRemove != null)
                        foreach (string PartToRemove in OtherPartsToRemove)
                        {
                            s = s.Replace(PartToRemove, "");
                        }

                    s = s.TrimEnd(CharToSplitWith);
                    if (!string.IsNullOrWhiteSpace(Assigning))
                        return s.Replace(Assigning, "").Replace("=", "").Trim().Replace($"{CharToSplitWith} ", CharToSplitWith.ToString()).Split(CharToSplitWith);
                    else
                        return s.Replace("=", "").Trim().Replace($"{CharToSplitWith} ", CharToSplitWith.ToString()).Split(CharToSplitWith);
                }
                catch (Exception e)
                {
                    Logger.LogConsole(e, $"Looks like something failed when assigning something", $"Assigning string: {Assigning}", false);
                    Logger.LogFile(e, $"Looks like something failed when assigning something", false, $"Assigning string: {Assigning}");
                }
                return s.Split(CharToSplitWith);
            }

            public static async Task<string> GenericAssigning(string s, string Assigning, string[] OtherPartsToRemove = null, string[][] PartsToReplace = null)
            {
                s = FixString(s);
                try
                {
                    if (PartsToReplace != null)
                        foreach (string[] PartToReplace in PartsToReplace)
                        {
                            s = s.Replace(PartToReplace[0], PartToReplace[1]);
                        }
                    if (OtherPartsToRemove != null)
                        foreach (string PartToRemove in OtherPartsToRemove)
                        {
                            s = s.Replace(PartToRemove, "");
                        }

                    if (!string.IsNullOrWhiteSpace(Assigning))
                        return s.Replace(Assigning, "").Replace("=", "").Trim();
                    else
                        return s.Replace("=", "").Trim();
                }
                catch (Exception e)
                {
                    Logger.LogConsole(e, $"Looks like something failed when assigning something", $"Assigning string: {Assigning}", false);
                    Logger.LogFile(e, $"Looks like something failed when assigning something", false, $"Assigning string: {Assigning}");
                }
                return s;
            }

            public static async Task Gender(string s, dynamic WhatToFill)
            {
                try
                {
                    var gender = (await GenericAssigning(s, "|gender")).ToLower();
                    if (!string.IsNullOrWhiteSpace(gender))
                    {
                        if (gender[0] == 'f') { WhatToFill.Gender = "Female"; }
                        else if (gender[0] == 'm') { WhatToFill.Gender = "Male"; }
                        else { WhatToFill.Gender = gender; } //For the people who think fruit salad is a gender ;)
                    }
                    else
                    {
                        WhatToFill.Gender = "none";
                    }
                    gender = null;
                }
                catch (Exception e)
                {
                    Logger.LogConsole(e, $"Looks like something failed when assigning someone gender", $"String used for this: {s}", false);
                    Logger.LogFile(e, $"Looks like something failed when assigning someone gender", false, $"String used for this: {s}");
                }
                if(WhatToFill.Gender == null)
                    WhatToFill.Gender = @"¯\_(ツ)_/¯";
            }

            public static async Task<ImageInformation> Image(string s, string ImageKeyword = "|img")
            {
                var Image = new ImageInformation();
                Image.Name = await GenericAssigning(s, ImageKeyword);
                Image.Uri = Image.Name;
                return Image;
            }

            public static async Task<dynamic> Item(string s, dynamic WhatToFill, string ItemNumber, bool MakeAscensionSkillReinforcement = false, string AscensionSkillReinforcementNumber = null, dynamic AscensionToMake = null)
            {
                try
                {
                    if (MakeAscensionSkillReinforcement)
                    {
                        AscensionToMake = new AscensionSkillReinforcement();
                        AscensionToMake.AscensionNumber = AscensionSkillReinforcementNumber;
                        return AscensionToMake;
                    }
                    else
                    {
                        WhatToFill = new Item();
                        WhatToFill.EnglishName = await GenericAssigning(s, $"|{ItemNumber}", new string[] { "{{Inum|{{", "{{", "}}" });
                        if (WhatToFill.EnglishName.IndexOf('|') != -1)
                            WhatToFill = await GetItem(WhatToFill.EnglishName.Remove(WhatToFill.EnglishName.IndexOf('|')));
                        else
                            WhatToFill = await GetItem(WhatToFill.EnglishName);
                        return WhatToFill;
                    }
                }
                catch (Exception e)
                {
                    Logger.LogConsole(e, $"Looks like something failed when assigning an Item", $"WhatToFill.EnglishName: {WhatToFill.EnglishName}", false);
                    Logger.LogFile(e, $"Looks like something failed when assigning an Item", false, $"WhatToFill.EnglishName: {WhatToFill.EnglishName}");
                }
                return WhatToFill;
            }
        }
    }
}
