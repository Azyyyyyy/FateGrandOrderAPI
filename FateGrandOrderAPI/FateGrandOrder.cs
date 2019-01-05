using System;
using System.Text;
using System.Linq;
using HtmlAgilityPack;
using System.Threading.Tasks;
using FateGrandOrderApi.Classes;
using FateGrandOrderApi.Logging;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using FateGrandOrderApi.Caching;

namespace FateGrandOrderApi
{
    /// <summary>
    /// Class containing parsing logic (this is where you get what you need from the api)
    /// </summary>
    public static class FateGrandOrderParsing
    {
        private static string FixString(string s)
        {
            if (!string.IsNullOrWhiteSpace(s))
                return s.Replace("&lt;", "<").Replace("%27", "'").Replace("<br>", "<br/>").Replace("%26", "%").Replace("<br />", "<br/>");
            else
                return s;
        }

        #region Skills Logic
        /// <summary>
        /// Returns a Skill (will return null if the skill isn't found)
        /// </summary>
        /// <param name="skillName">The Skill name to look for</param>
        /// <returns></returns>
        public static async Task<Tuple<Skill, string[]>> GetSkill(string skillName)
        {
            string[] resultString = null;
            Skill skill = null;
            Skill skillToRemoveFromCache = null;
            foreach (HtmlNode col in new HtmlWeb().Load($"https://fategrandorder.fandom.com/wiki/{skillName}?action=edit").DocumentNode.SelectNodes("//textarea"))
            {
                //For in case we put the person in wrong
                if (string.IsNullOrEmpty(col.InnerText))
                    break;

                skill = new Skill(skillName, col.InnerText);
                resultString = Regex.Split(col.InnerText, @"\n");
                if (FateGrandOrderPersonCache.Skills == null)
                    FateGrandOrderPersonCache.Skills = new List<Skill>();

                try
                {
                    foreach (Skill skillC in FateGrandOrderPersonCache.Skills)
                    {
                        if (skill.GeneratedWith == skillC.GeneratedWith && skillC.NamePassed == skill.NamePassed)
                        {
#if DEBUG
                            skillC.FromCache = true;
#endif
                            return Tuple.Create(skillC, resultString);
                        }
                        else if (skill.GeneratedWith != skillC.GeneratedWith && skill.NamePassed == skillC.NamePassed)
                        {
                            skillToRemoveFromCache = skillC;
                            break;
                        }
                    }
                }
                catch (Exception e)
                {
                    Logger.LogConsole(e, "Looks like something happened when accessing/using the cache for skills", $"Skill name: {skill.Name}");
                    Logger.LogFile(e, "Looks like something happened when accessing/using the cache for skills", $"Skill name: {skill.Name}");
                }

                if (skillToRemoveFromCache != null)
                    FateGrandOrderPersonCache.Skills.Remove(skillToRemoveFromCache);
                if (skill != null && !FateGrandOrderPersonCache.Skills.Contains(skill))
                    FateGrandOrderPersonCache.Skills.Add(skill);
                skillToRemoveFromCache = null;

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
                            effects = null;
                        }
                        catch (Exception e)
                        {
                            Logger.LogConsole(e, $"Looks like something happened when filling up Effect in a skill called {skill.Name}", $"String used when doing this: {s}");
                            Logger.LogFile(e, $"Looks like something happened when filling up Effect in a skill called {skill.Name}", $"String used when doing this: {s}");
                        }
                        break;
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
        /// This will return a filled in ActiveSkill (If the skill isn't a ActiveSkill it will return the core skill content)
        /// </summary>
        /// <param name="skillName">The ActiveSkill name to look for</param>
        /// <returns></returns>
        public static async Task<ActiveSkill> GetSkills(string skillName)
        {
            return await GetSkill(new ActiveSkill(skillName, ""));
        }

        /// <summary>
        /// This will return a filled in ActiveSkill (If the skill isn't a ActiveSkill it will return the core skill content)
        /// </summary>
        /// <param name="skill">The ActiveSkill to put all the content into</param>
        /// <returns></returns>
        public static async Task<ActiveSkill> GetSkill(ActiveSkill skill)
        {
            Tuple<Skill, string[]> content = null;
            string lastLevelEffect = "";
            ActiveSkill skillToRemoveFromCache = null;
            if (skill.NamePassed != null)
                content = await GetSkill(skill.NamePassed);
            else
                content = await GetSkill(skill.Name);
            var basicSkillContent = content.Item1;
            string[] resultString = content.Item2;
            if (string.IsNullOrWhiteSpace(skill.GeneratedWith))
                skill.GeneratedWith = content.Item1.GeneratedWith;

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
                    Logger.LogConsole(e, "Looks like something happened when GetStartPart was called", $"What lastLevelEffect was when this ex happened: {lastLevelEffect}");
                    Logger.LogFile(e, "Looks like something happened when GetStartPart was called", $"What lastLevelEffect was when this ex happened: {lastLevelEffect}");
                    return "|";
                }
            }

            //For in case we put the person in wrong
            if (resultString == null)
                return skill;

            if (FateGrandOrderPersonCache.ActiveSkills == null)
                FateGrandOrderPersonCache.ActiveSkills = new List<ActiveSkill>();
            try
            {
                foreach (ActiveSkill activeSkillC in FateGrandOrderPersonCache.ActiveSkills)
                {
                    if (skill.GeneratedWith == activeSkillC.GeneratedWith && activeSkillC.NamePassed == skill.NamePassed)
                    {
#if DEBUG
                        activeSkillC.FromCache = true;
#endif
                        return activeSkillC;
                    }
                    else if (skill.GeneratedWith != activeSkillC.GeneratedWith && skill.NamePassed == activeSkillC.NamePassed)
                    {
                        skillToRemoveFromCache = activeSkillC;
                        break;
                    }
                }
            }
            catch (Exception e)
            {
                Logger.LogConsole(e, "Looks like something happened when accessing/using the cache for active skill", $"Active skill name: {skill.Name}");
                Logger.LogFile(e, "Looks like something happened when accessing/using the cache for active skill", $"Active skill name: {skill.Name}");
            }

            if (skillToRemoveFromCache != null)
                FateGrandOrderPersonCache.ActiveSkills.Remove(skillToRemoveFromCache);
            if (skill != null && !FateGrandOrderPersonCache.ActiveSkills.Contains(skill))
                FateGrandOrderPersonCache.ActiveSkills.Add(skill);
            skillToRemoveFromCache = null;

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
                        var servants = await AssigningContent.GenericArrayAssigning(servantIcons, "|servanticons", '\\', new string[] { "{{" }, new string[][] { new string[] { "}}", "\\" } });
                        if (servants != null && servants.Length > 0)
                            skill.ServantsThatHaveThisSkill = new List<FateGrandOrderPerson>();

                        foreach (string servant in servants)
                        {
                            skill.ServantsThatHaveThisSkill.Add(await GetPerson(servant, PresetsForInformation.BasicInformation));
                        }
                        servantIcons = null;
                        servants = null;
                    }
                    catch (Exception e)
                    {
                        Logger.LogConsole(e, $"Looks like something happened when filling up ServantsThatHaveThisSkill in active skill {skill.Name}", $"String used when doing this: {s}");
                        Logger.LogFile(e, $"Looks like something happened when filling up ServantsThatHaveThisSkill in active skill {skill.Name}", $"String used when doing this: {s}");
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
                        Logger.LogConsole(e, $"Looks like something happened when making a LevelEffects in active skill {skill.Name}", $"String used when doing this: {s}");
                        Logger.LogFile(e, $"Looks like something happened when making a LevelEffects in active skill {skill.Name}", $"String used when doing this: {s}");
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

            resultString = null;
            lastLevelEffect = null;
            content = null;
            return skill;
        }
        #endregion

        /// <summary>
        /// 
        /// </summary>
        /// <param name="itemName"></param>
        /// <param name="enemyToNotLookFor"></param>
        /// <returns></returns>
        public async static Task<Item> GetItem(string itemName, Enemy enemyToNotLookFor = null)
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
                    Logger.LogConsole(e, "Looks like something happened when accessing/using the cache for items", $"Item name: {item.EnglishName}");
                    Logger.LogFile(e, "Looks like something happened when accessing/using the cache for items", $"Item name: {item.EnglishName}");
                }

                if (ItemToRemoveFromCache != null)
                    FateGrandOrderPersonCache.Items.Remove(ItemToRemoveFromCache);
                if (item != null && !FateGrandOrderPersonCache.Items.Contains(item))
                    FateGrandOrderPersonCache.Items.Add(item);

                var resultString = Regex.Split(col.InnerText, @"\n");

                foreach (string s in resultString)
                {
                    if (!string.IsNullOrWhiteSpace(s) && s == "}}" || FixString(s) == "}}</onlyinclude>" && DoingLocationLogic)
                    {
                        DoingLocationLogic = false;
                    }
                    else if (DoingLocationLogic)
                    {
                        LocationLogic(s);
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
                        var enemys = await AssigningContent.GenericArrayAssigning(s, "|enemy", '/', OtherPartsToRemove: new string[] { "[[", "]]" }, PartsToReplace: new string[][] { new string[] { "<br/>", "/" } });
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
                                Logger.LogConsole(e, $"Looks like something failed when getting the enemys that drop {item.EnglishName}", $"Item name: {item.EnglishName}\r\nEnemy name: {enemyEdited}");
                                Logger.LogFile(e, $"Looks like something failed when getting the enemys that drop {item.EnglishName}", $"Item name: {item.EnglishName}\r\nEnemy name: {enemyEdited}");
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
                        string ss = await AssigningContent.GenericAssigning(s, "|location");
                        if (!string.IsNullOrWhiteSpace(ss))
                            LocationLogic(ss);
                        else
                            DoingLocationLogic = true;
                    }
                    else if (s.Contains("|usedFor"))
                    {
                        item.Uses = new List<FateGrandOrderPerson>();
                        foreach (string ss in (await AssigningContent.GenericAssigning(s, "|usedFor")).Replace(" {{", "\\").Replace("{{", "\\").Replace("}}", "").Split('\\'))
                        {
                            var servant = await GetPerson(ss, PresetsForInformation.BasicInformation);
                            if (servant != null)
                                item.Uses.Add(servant);
                        }
                    }
                }
            }

            return item;

            async Task LocationLogic(string s)
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
                                foreach (string ss in FixString(s).Replace("<br/>", "\\").TrimEnd('\\').Split('\\'))
                                {
                                    var thing = await AssigningContent.GenericArrayAssigning(ss, "", ']', new string[] { "<br/>" }, new string[][] { new string[] { "[[", "[" }, new string[] { "]]", "]" } });
                                    if (thing.Length >= 3)
                                    {
                                        item.DropLocations[item.DropLocations.Count - 1].DropLocations.Add(new ItemDropLocation
                                        {
                                            Location = thing[0].Replace("[", ""),
                                            PossibleDrops = thing[1].Replace("[", ""),
                                            APCost = thing[2].Replace("[", "")
                                        });
                                    }
                                    else
                                    {
                                        item.DropLocations[item.DropLocations.Count - 1].DropLocations.Add(new ItemDropLocation
                                        {
                                            Location = thing[0].Replace("[","").Split('|').First()
                                        });
                                    }
                                }
                            }
                            catch (Exception e)
                            {
                                Logger.LogConsole(e, "Looks like something happened while filling up a item stat", $"Item name: {item.EnglishName}");
                                Logger.LogFile(e, "Looks like something happened while filling up a item stat", $"Item name: {item.EnglishName}");
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    Logger.LogConsole(e, "Looks like something happened when doing DoingLocationLogic if statement", $"Item name: {item.EnglishName}");
                    Logger.LogFile(e, "Looks like something happened when doing DoingLocationLogic if statement", $"Item name: {item.EnglishName}");
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="enemyName"></param>
        /// <param name="itemToNotLookFor"></param>
        /// <returns></returns>
        public async static Task<Enemy> GetEnemy(string enemyName, Item itemToNotLookFor = null)
        {
            bool GettingImages = false;
            bool GettingRecommendedServants = false;
            Enemy enemy = null;
            Enemy EnemyToRemoveFromCache = null;
            foreach (HtmlNode col in new HtmlWeb().Load($"https://fategrandorder.fandom.com/wiki/{enemyName}?action=edit").DocumentNode.SelectNodes("//textarea"))
            {
                //For in case we put the person in wrong or it doesn't have a webpage
                if (string.IsNullOrEmpty(col.InnerText))
                    break;

                enemy = new Enemy(enemyName, col.InnerHtml);
                if (FateGrandOrderPersonCache.Enemies == null)
                    FateGrandOrderPersonCache.Enemies = new List<Enemy>();

                try
                {
                    foreach (Enemy enemyC in FateGrandOrderPersonCache.Enemies)
                    {
                        if (enemy.GeneratedWith == enemyC.GeneratedWith && enemyC.EnglishName == enemy.EnglishName)
                        {
#if DEBUG
                            enemyC.FromCache = true;
#endif
                            return enemyC;
                        }
                        else if (enemy.GeneratedWith != enemyC.GeneratedWith && enemy.EnglishName == enemyC.EnglishName)
                        {
                            EnemyToRemoveFromCache = enemyC;
                            break;
                        }
                    }
                }
                catch (Exception e)
                {
                    Logger.LogConsole(e, "Looks like something happened when accessing/using the cache for enemy", $"Enemy name: {enemy.EnglishName}");
                    Logger.LogFile(e, "Looks like something happened when accessing/using the cache for enemy", $"Enemy name: {enemy.EnglishName}");
                }

                var resultString = Regex.Split(col.InnerText, @"\n");

                foreach (string s in resultString)
                {
                    if (s == "|}")
                    {
                        GettingRecommendedServants = false;
                    }
                    else if (GettingRecommendedServants)
                    {
                        if (s.Contains("|{{") && !s.Contains("!!"))
                        {
                            try
                            {
                                string[] WhatToLookFor = s.Replace("|{{", "").Replace("}}", "").Split('|');
                                foreach (HtmlNode col2 in new HtmlWeb().Load($"https://fategrandorder.fandom.com/wiki/Template:{WhatToLookFor[0]}?action=edit").DocumentNode.SelectNodes("//textarea"))
                                {
                                    var servants = col2.InnerText.Split('\n');
                                    if (servants.Contains("{{#switch: {{{Servant}}}"))
                                    {
                                        for (int i = 0; i < servants.Length; i++)
                                        {
                                            if (WhatToLookFor.Length > 1 && servants[i].Contains($"|{WhatToLookFor[1]}"))
                                            {
                                                servants = new string[] { await AssigningContent.GenericAssigning(servants[i], $"|{WhatToLookFor[1]}") };
                                                break;
                                            }
                                            else if (servants[i].Contains("|#default"))
                                            {
                                                servants = new string[] { await AssigningContent.GenericAssigning(servants[i], "|#default") };
                                                break;
                                            }
                                        }
                                    }
                                    foreach (var ss in Regex.Split(servants[0], "}}}} "))
                                    {
                                        var personcontent = ss.Split('|');
                                        var servant = await GetPerson(personcontent[1].Replace("{{", "").Replace("}}", ""), PresetsForInformation.BasicInformation);
                                        if (servant != null)
                                            enemy.RecommendedServants.Add(servant);
                                    }
                                }
                            }
                            catch (Exception e)
                            {
                                Logger.LogConsole(e, $"Looks like something failed when assigning enemy.RecommendedServants", $"Enemy name: {enemy.EnglishName}");
                                Logger.LogFile(e, $"Looks like something failed when assigning enemy.RecommendedServants", $"Enemy name: {enemy.EnglishName}");
                            }
                        }
                    }

                    if (EnemyToRemoveFromCache != null)
                        FateGrandOrderPersonCache.Enemies.Remove(EnemyToRemoveFromCache);
                    if (enemy != null && !FateGrandOrderPersonCache.Enemies.Contains(enemy))
                        FateGrandOrderPersonCache.Enemies.Add(enemy);

                    if (s.Contains("|image"))
                    {
                        if (FixString(s).Contains("<gallery>"))
                            GettingImages = true;
                        else
                            enemy.EnemyImage.Add(await AssigningContent.Image(s, "|image"));
                    }
                    if (GettingImages && FixString(s).Contains("</gallery>"))
                    {
                        GettingImages = false;
                    }
                    else if (GettingImages)
                    {
                        enemy.EnemyImage.Add(await AssigningContent.Image(s, ""));
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
                            Logger.LogConsole(e, $"Looks like something failed when assigning enemy.Areas", $"Enemy name: {enemy.EnglishName}");
                            Logger.LogFile(e, $"Looks like something failed when assigning enemy.Areas", $"Enemy name: {enemy.EnglishName}");
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
                            Logger.LogConsole(e, $"Looks like something failed when assigning enemy.WhatThisEnemyDrops", $"Enemy name: {enemy.EnglishName}");
                            Logger.LogFile(e, $"Looks like something failed when assigning enemy.WhatThisEnemyDrops", $"Enemy name: {enemy.EnglishName}");
                        }
                    }
                    else if (s == "==Recommended Servants==" | s == "== Recommended Servants ==")
                    {
                        GettingRecommendedServants = true;
                        enemy.RecommendedServants = new List<FateGrandOrderPerson>();
                    }
                }
            }

            return enemy;
        }

        /// <summary>
        /// This will return the servant from the servant name (will return null if we are unable to find the person)
        /// </summary>
        /// <param name="ServantName"></param>
        /// <param name="presetsForInformation"></param>
        /// <param name="GetBasicInformation"></param>
        /// <param name="GetActiveSkills"></param>
        /// <param name="GetPassiveSkills"></param>
        /// <param name="GetNoblePhantasm"></param>
        /// <param name="GetAscension"></param>
        /// <param name="GetSkillReinforcement"></param>
        /// <param name="GetStats"></param>
        /// <param name="GetBondLevel"></param>
        /// <param name="GetBiography"></param>
        /// <param name="GetAvailability"></param>
        /// <param name="GetTrivia"></param>
        /// <param name="GetImages"></param>
        /// <returns></returns>
        public static async Task<FateGrandOrderPerson> GetPerson(string ServantName, PresetsForInformation presetsForInformation = PresetsForInformation.AllInformation, bool GetBasicInformation = false, bool GetActiveSkills = false, bool GetPassiveSkills = false, bool GetNoblePhantasm = false, bool GetAscension = false, bool GetSkillReinforcement = false, bool GetStats = false, bool GetBondLevel = false, bool GetBiography = false, bool GetAvailability = false, bool GetTrivia = false, bool GetImages = false)
        {
            FateGrandOrderPerson fateGrandOrderPerson = null;
            FateGrandOrderPerson PersonToRemoveFromCache = null;

            #region Toggles For GettingInformation
            if (presetsForInformation == PresetsForInformation.BasicInformation)
            {
                GetBasicInformation = true;
            }
            else if (presetsForInformation == PresetsForInformation.AllInformation)
            {
                GetBasicInformation = true;
                GetActiveSkills = true;
                GetPassiveSkills = true;
                GetNoblePhantasm = true;
                GetAscension = true;
                GetSkillReinforcement = true;
                GetStats = true;
                GetBondLevel = true;
                GetBiography = true;
                GetAvailability = true;
                GetTrivia = true;
                GetImages = true;
            }
            #endregion

            #region Getting bools
            bool GettingActiveSkills = false;
            bool GettingPassiveSkills = false;
            bool GettingNoblePhantasm = false;
            bool GettingAscension = false;
            bool GettingSkillReinforcement = false;
            bool GettingImages = false;
            bool GettingStats = false;
            bool GettingBondLevel = false;
            bool GettingBiography = false;
            bool GettingAvailability = false;
            bool GettingTrivia = false;
            bool GettingBasicInformation = true;
            int PassiveSkillsCount = 0;

            #region Getting Biography bools
            bool GettingDefaultBioJap = false;
            bool GettingDefaultBio = false;
            bool GettingBond1BioJap = false;
            bool GettingBond1Bio = false;
            bool GettingBond2BioJap = false;
            bool GettingBond2Bio = false;
            bool GettingBond3BioJap = false;
            bool GettingBond3Bio = false;
            bool GettingBond4BioJap = false;
            bool GettingBond4Bio = false;
            bool GettingBond5BioJap = false;
            bool GettingBond5Bio = false;
            bool GettingExtraBioJap = false;
            bool GettingExtraBio = false;
            #endregion
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
                                GettingBasicInformation = false;
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
                            if (GetStats && fateGrandOrderPersonC.Stats != null)
                            {
                                GetStats = false;
                            }
                            if (GetBondLevel && fateGrandOrderPersonC.BondLevels != null)
                            {
                                GetBondLevel = false;
                            }
                            if (GetBiography && fateGrandOrderPersonC.Biography != null)
                            {
                                GetBiography = false;
                            }
                            if (GetAvailability && fateGrandOrderPersonC.Availability != null)
                            {
                                GetAvailability = false;
                            }
                            if (GetTrivia && fateGrandOrderPersonC.Trivia != null)
                            {
                                GetTrivia = false;
                            }

                            if (GetBasicInformation == false && GetActiveSkills == false && GetPassiveSkills == false && GetNoblePhantasm == false && GetAscension == false && GetSkillReinforcement == false && GetStats == false && GetBondLevel == false && GetBiography == false && GetAvailability == false)
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
                    Logger.LogConsole(e, $"Looks like something failed when accessing FateGrandOrderPeople cache", $"Servant name: {fateGrandOrderPerson.EnglishNamePassed}");
                    Logger.LogFile(e, $"Looks like something failed when accessing FateGrandOrderPeople cache", $"Servant name: {fateGrandOrderPerson.EnglishNamePassed}");
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
                if (GetStats && fateGrandOrderPerson.Stats == null)
                {
                    fateGrandOrderPerson.Stats = new Stats();
                }
                if (GetBondLevel && fateGrandOrderPerson.BondLevels == null)
                {
                    fateGrandOrderPerson.BondLevels = new BondLevels();
                }
                if (GetBiography && fateGrandOrderPerson.Biography == null)
                {
                    fateGrandOrderPerson.Biography = new Biography();
                }
                #endregion

                #region Add/Remove to cache and any returns we need to do
                if (PersonToRemoveFromCache != null)
                    FateGrandOrderPersonCache.FateGrandOrderPeople.Remove(PersonToRemoveFromCache);
                if (fateGrandOrderPerson != null)
                    FateGrandOrderPersonCache.FateGrandOrderPeople.Add(fateGrandOrderPerson);
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
                            Logger.LogConsole(e, $"Looks like something failed when assigning something in fateGrandOrderPerson.PassiveSkills", $"Servant name: {fateGrandOrderPerson.EnglishNamePassed}");
                            Logger.LogFile(e, $"Looks like something failed when assigning something in fateGrandOrderPerson.PassiveSkills", $"Servant name: {fateGrandOrderPerson.EnglishNamePassed}");
                        }
                    }
                    #endregion

                    #region Active Skills
                    else if (GettingActiveSkills)
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
                            Logger.LogConsole(e, $"Looks like something failed when assigning something in fateGrandOrderPerson.ActiveSkills", $"Servant name: {fateGrandOrderPerson.EnglishNamePassed}");
                            Logger.LogFile(e, $"Looks like something failed when assigning something in fateGrandOrderPerson.ActiveSkills", $"Servant name: {fateGrandOrderPerson.EnglishNamePassed}");
                        }
                    }
                    #endregion

                    #region Noble Phantasm
                    else if (GettingNoblePhantasm)
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
                                fateGrandOrderPerson.NoblePhantasms[fateGrandOrderPerson.NoblePhantasms.Count - 1].NoblePhantasm.VideoInformation = await AssigningContent.Video(s.Replace("[[File:", ""));
                            }
                            catch (Exception e)
                            {
                                Logger.LogConsole(e, $"Looks like something failed when assigning something in fateGrandOrderPerson.NoblePhantasms[fateGrandOrderPerson.NoblePhantasms.Count - 1].NoblePhantasm.VideoInformation", $"Servant name: {fateGrandOrderPerson.EnglishNamePassed}");
                                Logger.LogFile(e, $"Looks like something failed when assigning something in fateGrandOrderPerson.NoblePhantasms[fateGrandOrderPerson.NoblePhantasms.Count - 1].NoblePhantasm.VideoInformation", $"Servant name: {fateGrandOrderPerson.EnglishNamePassed}");
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
                    else if (GettingAscension)
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
                    else if (GettingSkillReinforcement)
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

                    #region Stats
                    else if (GettingStats)
                    {
                        if (s.Contains("|strength"))
                        {
                            fateGrandOrderPerson.Stats.Strength.Grade = await AssigningContent.GenericAssigning(s, "|strength");
                        }
                        else if (s.Contains("|stbar"))
                        {
                            fateGrandOrderPerson.Stats.Strength.BarNumber = await AssigningContent.GenericAssigning(s, "|stbar");
                        }
                        else if (s.Contains("|endurance"))
                        {
                            fateGrandOrderPerson.Stats.Endurance.Grade = await AssigningContent.GenericAssigning(s, "|endurance");
                        }
                        else if (s.Contains("|enbar"))
                        {
                            fateGrandOrderPerson.Stats.Endurance.BarNumber = await AssigningContent.GenericAssigning(s, "|enbar");
                        }
                        else if (s.Contains("|agility"))
                        {
                            fateGrandOrderPerson.Stats.Agility.Grade = await AssigningContent.GenericAssigning(s, "|agility");
                        }
                        else if (s.Contains("|agbar"))
                        {
                            fateGrandOrderPerson.Stats.Agility.BarNumber = await AssigningContent.GenericAssigning(s, "|agbar");
                        }
                        else if (s.Contains("|mana"))
                        {
                            fateGrandOrderPerson.Stats.Mana.Grade = await AssigningContent.GenericAssigning(s, "|mana");
                        }
                        else if (s.Contains("|mabar"))
                        {
                            fateGrandOrderPerson.Stats.Mana.BarNumber = await AssigningContent.GenericAssigning(s, "|mabar");
                        }
                        else if (s.Contains("|luck"))
                        {
                            fateGrandOrderPerson.Stats.Luck.Grade = await AssigningContent.GenericAssigning(s, "|luck");
                        }
                        else if (s.Contains("|lubar"))
                        {
                            fateGrandOrderPerson.Stats.Luck.BarNumber = await AssigningContent.GenericAssigning(s, "|lubar");
                        }
                        else if (s.Contains("|np") && !s.Contains("|npbar"))
                        {
                            fateGrandOrderPerson.Stats.NP.Grade = await AssigningContent.GenericAssigning(s, "|np");
                        }
                        else if (s.Contains("|npbar"))
                        {
                            fateGrandOrderPerson.Stats.NP.BarNumber = await AssigningContent.GenericAssigning(s, "|npbar");
                        }
                    }
                    #endregion

                    #region Bond Level
                    else if (GettingBondLevel)
                    {
                        if (s.Contains("|b1") && !s.Contains("|b10"))
                        {
                            fateGrandOrderPerson.BondLevels.BondLevel1.BondRequired = await AssigningContent.GenericAssigning(s, "|b1");
                        }
                        else if (s.Contains("|b2"))
                        {
                            fateGrandOrderPerson.BondLevels.BondLevel2.BondRequired = await AssigningContent.GenericAssigning(s, "|b2");
                        }
                        else if (s.Contains("|b3"))
                        {
                            fateGrandOrderPerson.BondLevels.BondLevel3.BondRequired = await AssigningContent.GenericAssigning(s, "|b3");
                        }
                        else if (s.Contains("|b4"))
                        {
                            fateGrandOrderPerson.BondLevels.BondLevel4.BondRequired = await AssigningContent.GenericAssigning(s, "|b4");
                        }
                        else if (s.Contains("|b5"))
                        {
                            fateGrandOrderPerson.BondLevels.BondLevel5.BondRequired = await AssigningContent.GenericAssigning(s, "|b5");
                        }
                        else if (s.Contains("|b6"))
                        {
                            fateGrandOrderPerson.BondLevels.BondLevel6.BondRequired = await AssigningContent.GenericAssigning(s, "|b6");
                        }
                        else if (s.Contains("|b7"))
                        {
                            fateGrandOrderPerson.BondLevels.BondLevel7.BondRequired = await AssigningContent.GenericAssigning(s, "|b7");
                        }
                        else if (s.Contains("|b8"))
                        {
                            fateGrandOrderPerson.BondLevels.BondLevel8.BondRequired = await AssigningContent.GenericAssigning(s, "|b8");
                        }
                        else if (s.Contains("|b9"))
                        {
                            fateGrandOrderPerson.BondLevels.BondLevel9.BondRequired = await AssigningContent.GenericAssigning(s, "|b9");
                        }
                        else if (s.Contains("|b10"))
                        {
                            fateGrandOrderPerson.BondLevels.BondLevel10.BondRequired = await AssigningContent.GenericAssigning(s, "|b10");
                        }
                        if (s.Contains("|2b1") && !s.Contains("|2b10"))
                        {
                            fateGrandOrderPerson.BondLevels.BondLevel1.TotalBond = await AssigningContent.GenericAssigning(s, "|2b1");
                        }
                        else if (s.Contains("|2b2"))
                        {
                            fateGrandOrderPerson.BondLevels.BondLevel2.TotalBond = await AssigningContent.GenericAssigning(s, "|2b2");
                        }
                        else if (s.Contains("|2b3"))
                        {
                            fateGrandOrderPerson.BondLevels.BondLevel3.TotalBond = await AssigningContent.GenericAssigning(s, "|2b3");
                        }
                        else if (s.Contains("|2b4"))
                        {
                            fateGrandOrderPerson.BondLevels.BondLevel4.TotalBond = await AssigningContent.GenericAssigning(s, "|2b4");
                        }
                        else if (s.Contains("|2b5"))
                        {
                            fateGrandOrderPerson.BondLevels.BondLevel5.TotalBond = await AssigningContent.GenericAssigning(s, "|2b5");
                        }
                        else if (s.Contains("|2b6"))
                        {
                            fateGrandOrderPerson.BondLevels.BondLevel6.TotalBond = await AssigningContent.GenericAssigning(s, "|2b6");
                        }
                        else if (s.Contains("|2b7"))
                        {
                            fateGrandOrderPerson.BondLevels.BondLevel7.TotalBond = await AssigningContent.GenericAssigning(s, "|2b7");
                        }
                        else if (s.Contains("|2b8"))
                        {
                            fateGrandOrderPerson.BondLevels.BondLevel8.TotalBond = await AssigningContent.GenericAssigning(s, "|2b8");
                        }
                        else if (s.Contains("|2b9"))
                        {
                            fateGrandOrderPerson.BondLevels.BondLevel9.TotalBond = await AssigningContent.GenericAssigning(s, "|2b9");
                        }
                        else if (s.Contains("|2b10"))
                        {
                            fateGrandOrderPerson.BondLevels.BondLevel10.TotalBond = await AssigningContent.GenericAssigning(s, "|2b10");
                        }
                        else if (s.Contains("|image"))
                        {
                            fateGrandOrderPerson.BondLevels.Bond10Reward.Image = await AssigningContent.Image(s, "|image");
                        }
                        else if (s.Contains("|effect"))
                        {
                            fateGrandOrderPerson.BondLevels.Bond10Reward.Effect = await AssigningContent.GenericAssigning(FixString(s).Replace("<br/>", "\\").Split('\\').Last(), "|effect");
                        }
                    }
                    #endregion

                    #region Biography
                    else if (GettingBiography)
                    {
                        if (s.Contains("|jdef"))
                        {
                            GettingDefaultBioJap = true;
                        }
                        else if (s.Contains("|def"))
                        {
                            GettingDefaultBioJap = false;
                            GettingDefaultBio = true;
                        }
                        else if (s.Contains("|jb1"))
                        {
                            GettingDefaultBio = false;
                            GettingBond1BioJap = true;
                        }
                        else if (s.Contains("|b1"))
                        {
                            GettingBond1BioJap = false;
                            GettingBond1Bio = true;
                        }
                        else if (s.Contains("|jb2"))
                        {
                            GettingBond1Bio = false;
                            GettingBond2BioJap = true;
                        }
                        else if (s.Contains("|b2"))
                        {
                            GettingBond2BioJap = false;
                            GettingBond2Bio = true;
                        }
                        else if (s.Contains("|jb3"))
                        {
                            GettingBond2Bio = false;
                            GettingBond3BioJap = true;
                        }
                        else if (s.Contains("|b3"))
                        {
                            GettingBond3BioJap = false;
                            GettingBond3Bio = true;
                        }
                        else if (s.Contains("|jb4"))
                        {
                            GettingBond3Bio = false;
                            GettingBond4BioJap = true;
                        }
                        else if (s.Contains("|b4"))
                        {
                            GettingBond4BioJap = false;
                            GettingBond4Bio = true;
                        }
                        else if (s.Contains("|jb5"))
                        {
                            GettingBond4Bio = false;
                            GettingBond5BioJap = true;
                        }
                        else if (s.Contains("|b5"))
                        {
                            GettingBond5BioJap = false;
                            GettingBond5Bio = true;
                        }
                        else if (s.Contains("|jex"))
                        {
                            GettingBond5Bio = false;
                            GettingExtraBioJap = true;
                        }
                        else if (s.Contains("|ex"))
                        {
                            GettingExtraBioJap = false;
                            GettingExtraBio = true;
                        }
                        else if (GettingDefaultBioJap)
                        {
                            fateGrandOrderPerson.Biography.Default.JapaneseText = fateGrandOrderPerson.Biography.Default.JapaneseText + await AssigningContent.GenericAssigning(s, "", OtherPartsToRemove: new string[] { "'''", "―――", "---", "[[", "]]", "''" }, PartsToReplace: new string[][] { new string[] { "<br/>", "/r/n" } });
                        }
                        else if (GettingDefaultBio)
                        {
                            fateGrandOrderPerson.Biography.Default.EnglishText = fateGrandOrderPerson.Biography.Default.EnglishText + await AssigningContent.GenericAssigning(s, "", OtherPartsToRemove: new string[] { "'''", "―――", "---", "[[", "]]", "''" }, PartsToReplace: new string[][] { new string[] { "<br/>", "/r/n" } });
                        }
                        else if (GettingBond1BioJap)
                        {
                            fateGrandOrderPerson.Biography.Bond1.JapaneseText = fateGrandOrderPerson.Biography.Bond1.JapaneseText + await AssigningContent.GenericAssigning(s, "", OtherPartsToRemove: new string[] { "'''", "―――", "---", "[[", "]]", "''" }, PartsToReplace: new string[][] { new string[] { "<br/>", "/r/n" } });
                        }
                        else if (GettingBond1Bio)
                        {
                            fateGrandOrderPerson.Biography.Bond1.EnglishText = fateGrandOrderPerson.Biography.Bond1.EnglishText + await AssigningContent.GenericAssigning(s, "", OtherPartsToRemove: new string[] { "'''", "―――", "---", "[[", "]]", "''" }, PartsToReplace: new string[][] { new string[] { "<br/>", "/r/n" } });
                        }
                        else if (GettingBond2BioJap)
                        {
                            fateGrandOrderPerson.Biography.Bond2.JapaneseText = fateGrandOrderPerson.Biography.Bond2.JapaneseText + await AssigningContent.GenericAssigning(s, "", OtherPartsToRemove: new string[] { "'''", "―――", "---", "[[", "]]", "''" }, PartsToReplace: new string[][] { new string[] { "<br/>", "/r/n" } });
                        }
                        else if (GettingBond2Bio)
                        {
                            fateGrandOrderPerson.Biography.Bond2.EnglishText = fateGrandOrderPerson.Biography.Bond2.EnglishText + await AssigningContent.GenericAssigning(s, "", OtherPartsToRemove: new string[] { "'''", "―――", "---", "[[", "]]", "''" }, PartsToReplace: new string[][] { new string[] { "<br/>", "/r/n" } });
                        }
                        else if (GettingBond3BioJap)
                        {
                            fateGrandOrderPerson.Biography.Bond3.JapaneseText = fateGrandOrderPerson.Biography.Bond3.JapaneseText + await AssigningContent.GenericAssigning(s, "", OtherPartsToRemove: new string[] { "'''", "―――", "---", "[[", "]]", "''" }, PartsToReplace: new string[][] { new string[] { "<br/>", "/r/n" } });
                        }
                        else if (GettingBond3Bio)
                        {
                            fateGrandOrderPerson.Biography.Bond3.EnglishText = fateGrandOrderPerson.Biography.Bond3.EnglishText + await AssigningContent.GenericAssigning(s, "", OtherPartsToRemove: new string[] { "'''", "―――", "---", "[[", "]]", "''" }, PartsToReplace: new string[][] { new string[] { "<br/>", "/r/n" } });
                        }
                        else if (GettingBond4BioJap)
                        {
                            fateGrandOrderPerson.Biography.Bond4.JapaneseText = fateGrandOrderPerson.Biography.Bond4.JapaneseText + await AssigningContent.GenericAssigning(s, "", OtherPartsToRemove: new string[] { "'''", "―――", "---", "[[", "]]", "''" }, PartsToReplace: new string[][] { new string[] { "<br/>", "/r/n" } });
                        }
                        else if (GettingBond4Bio)
                        {
                            fateGrandOrderPerson.Biography.Bond4.EnglishText = fateGrandOrderPerson.Biography.Bond4.EnglishText + await AssigningContent.GenericAssigning(s, "", OtherPartsToRemove: new string[] { "'''", "―――", "---", "[[", "]]", "''" }, PartsToReplace: new string[][] { new string[] { "<br/>", "/r/n" } });
                        }
                        else if (GettingBond5BioJap)
                        {
                            fateGrandOrderPerson.Biography.Bond5.JapaneseText = fateGrandOrderPerson.Biography.Bond5.JapaneseText + await AssigningContent.GenericAssigning(s, "", OtherPartsToRemove: new string[] { "'''", "―――", "---", "[[", "]]", "''" }, PartsToReplace: new string[][] { new string[] { "<br/>", "/r/n" } });
                        }
                        else if (GettingBond5Bio)
                        {
                            fateGrandOrderPerson.Biography.Bond5.EnglishText = fateGrandOrderPerson.Biography.Bond5.EnglishText + await AssigningContent.GenericAssigning(s, "", OtherPartsToRemove: new string[] { "'''", "―――", "---", "[[", "]]", "''" }, PartsToReplace: new string[][] { new string[] { "<br/>", "/r/n" } });
                        }
                        else if (GettingExtraBioJap)
                        {
                            fateGrandOrderPerson.Biography.Extra.JapaneseText = fateGrandOrderPerson.Biography.Extra.JapaneseText + await AssigningContent.GenericAssigning(s, "", OtherPartsToRemove: new string[] { "'''", "―――", "---", "[[", "]]", "''" }, PartsToReplace: new string[][] { new string[] { "<br/>", "/r/n" } });
                        }
                        else if (GettingExtraBio)
                        {
                            fateGrandOrderPerson.Biography.Extra.EnglishText = fateGrandOrderPerson.Biography.Extra.EnglishText + await AssigningContent.GenericAssigning(s, "", OtherPartsToRemove: new string[] { "'''", "―――", "---", "[[", "]]", "''" }, PartsToReplace: new string[][] { new string[] { "<br/>", "/r/n" } });
                        }
                    }
                    #endregion

                    #region Availability
                    else if (GettingAvailability)
                    {
                        if (s.Contains("|[["))
                        {
                            string ToAdd = FixString(s).Remove(0, s.IndexOf("|[[") + 3).Replace("]]", "").Trim();
                            if (ToAdd.Contains('|'))
                                ToAdd = ToAdd.Remove(ToAdd.IndexOf('|'));
                            if (fateGrandOrderPerson.Availability == null)
                                fateGrandOrderPerson.Availability = new string[] { ToAdd };
                            else
                                fateGrandOrderPerson.Availability = new string[] { $"{fateGrandOrderPerson.Availability[0]}\\{ToAdd}" };
                        }
                    }
                    #endregion

                    #region Trivia
                    else if (GettingTrivia)
                    {
                        if (!string.IsNullOrWhiteSpace(s))
                        {
                            string ToAdd = FixString(s);
                            while (ToAdd.Contains("[[") && ToAdd.Contains('|'))
                            {
                                ToAdd = ToAdd.Remove(ToAdd.IndexOf("[["), ToAdd.IndexOf('|') + 1 - ToAdd.IndexOf("[["));
                                ToAdd = ToAdd.Remove(ToAdd.IndexOf("]]"), 2);
                            }
                            ToAdd = ToAdd.Replace("*", "").Replace("[[", "").Replace("]]", "").Trim();
                            while (ToAdd.Contains("[http"))
                            {
                                ToAdd = ToAdd.Remove(ToAdd.IndexOf('['), ToAdd.LastIndexOf('/') + 2 - ToAdd.IndexOf('['));
                                ToAdd = ToAdd.Remove(ToAdd.IndexOf(']'), 1);
                            }
                            if (fateGrandOrderPerson.Trivia == null)
                                fateGrandOrderPerson.Trivia = new string[] { ToAdd };
                            else
                                fateGrandOrderPerson.Trivia = new string[] { $"{fateGrandOrderPerson.Trivia[0]}\\{ToAdd}" };
                        }
                    }
                    #endregion

                    #region Basic Information
                    else if (GettingBasicInformation)
                    {
                        if (s.Contains("|image"))
                        {
                            if (FixString(s).Contains("<gallery>"))
                                GettingImages = true;
                            else
                                fateGrandOrderPerson.BasicInformation.Images.Add(await AssigningContent.Image(s, "|image"));
                        }
                        if (GettingImages && FixString(s).Contains("</gallery>"))
                        {
                            GettingImages = false;
                        }
                        else if (GettingImages)
                        {
                            fateGrandOrderPerson.BasicInformation.Images.Add(await AssigningContent.Image(s, ""));
                        }
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
                            fateGrandOrderPerson.BasicInformation.Class = await AssigningContent.GenericAssigning(s, "|class");
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

                    #region Images
                    else if (GettingImages && !string.IsNullOrWhiteSpace(s) && s != "|-|" && s.Contains('|'))
                    {
                        var image = await AssigningContent.Image(s, "");
                        if(image != null)
                            fateGrandOrderPerson.Images.Add(image);
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
                    else if (GetStats && s == "== Stats ==" | s == "==Stats==")
                    {
                        GettingStats = true;
                    }
                    else if (GetBondLevel && s == "== Bond Level ==" | s == "==Bond Level==")
                    {
                        GettingBondLevel = true;
                    }
                    else if (GetBiography && s == "== Biography ==" | s == "==Biography==")
                    {
                        GettingBiography = true;
                    }
                    else if (GetAvailability && s == "== Availability ==" | s == "==Availability==")
                    {
                        GettingAvailability = true;
                    }
                    else if (GetTrivia && s == "== Trivia ==" | s == "==Trivia==")
                    {
                        GettingTrivia = true;
                    }
                    else if (GetImages && s == "== Images ==" | s == "==Images==")
                    {
                        GettingImages = true;
                    }
                    else if (GettingActiveSkills | GettingPassiveSkills | GettingNoblePhantasm | GettingImages && FixString(s) == "</tabber>")
                    {
                        GettingActiveSkills = false;
                        GettingPassiveSkills = false;
                        GettingNoblePhantasm = false;
                        GettingImages = false;
                    }
                    else if (GettingPassiveSkills | GettingAscension | GettingSkillReinforcement | GettingStats | GettingBondLevel | GettingBiography | GettingBasicInformation && s == @"}}")
                    {
                        if (fateGrandOrderPerson.PassiveSkills != null && fateGrandOrderPerson.PassiveSkills.Count > 0 && fateGrandOrderPerson.PassiveSkills[fateGrandOrderPerson.PassiveSkills.Count - 1].Category == null)
                            GettingPassiveSkills = false;
                        GettingAscension = false;
                        GettingSkillReinforcement = false;
                        GettingStats = false;
                        GettingBondLevel = false;
                        GettingBiography = false;
                        GettingBasicInformation = false;
                    }
                    else if (GettingAvailability && s == "|}")
                    {
                        GettingAvailability = false;
                        fateGrandOrderPerson.Availability = fateGrandOrderPerson.Availability[0].Split('\\');
                    }
                    else if (GettingTrivia && string.IsNullOrWhiteSpace(s))
                    {
                        fateGrandOrderPerson.Trivia = fateGrandOrderPerson.Trivia[0].Split('\\');
                        GettingTrivia = false;
                    }
                    #endregion
                }
            }
            return fateGrandOrderPerson;
        }

        /// <summary>
        /// 
        /// </summary>
        private class AssigningContent
        {
            /// <summary>
            /// 
            /// </summary>
            /// <param name="s"></param>
            /// <param name="Assigning"></param>
            /// <param name="CharToSplitWith"></param>
            /// <param name="OtherPartsToRemove"></param>
            /// <param name="PartsToReplace"></param>
            /// <returns></returns>
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
                    Logger.LogConsole(e, $"Looks like something failed when assigning something", $"Assigning string: {Assigning}");
                    Logger.LogFile(e, $"Looks like something failed when assigning something", $"Assigning string: {Assigning}");
                }
                return s.Split(CharToSplitWith);
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="s"></param>
            /// <param name="Assigning"></param>
            /// <param name="OtherPartsToRemove"></param>
            /// <param name="PartsToReplace"></param>
            /// <returns></returns>
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
                        return s.Replace(Assigning, "").Replace("=", "").Trim().Replace("/r/n","\r\n");
                    else
                        return s.Replace("=", "").Trim().Replace("/r/n", "\r\n");
                }
                catch (Exception e)
                {
                    Logger.LogConsole(e, $"Looks like something failed when assigning something", $"Assigning string: {Assigning}");
                    Logger.LogFile(e, $"Looks like something failed when assigning something", $"Assigning string: {Assigning}");
                }
                return s;
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="s"></param>
            /// <param name="WhatToFill"></param>
            /// <returns></returns>
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
                    Logger.LogConsole(e, $"Looks like something failed when assigning someone gender", $"String used for this: {s}");
                    Logger.LogFile(e, $"Looks like something failed when assigning someone gender", $"String used for this: {s}");
                }
                if(WhatToFill.Gender == null)
                    WhatToFill.Gender = @"¯\_(ツ)_/¯";
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="s"></param>
            /// <param name="ImageKeyword"></param>
            /// <param name="OtherPartsToRemove"></param>
            /// <returns></returns>
            public static async Task<ImageInformation> Image(string s, string ImageKeyword = "|img", string[] OtherPartsToRemove = null)
            {
                if (OtherPartsToRemove == null)
                {
                    OtherPartsToRemove = new string[] { "File:", "link", "[[", "]]" };
                }
                string baseUri = "https://vignette.wikia.nocookie.net/fategrandorder/images";
                int DirDepth = 2;
                string hashPartToUse = "";
                s = await GenericAssigning(s, ImageKeyword, OtherPartsToRemove);

                if (s.Contains("px"))
                {
                    s = s.Remove(s.IndexOf('|'), s.LastIndexOf('|') - s.IndexOf('|'));
                }

                var Image = new ImageInformation();
                if (s.Contains("|"))
                    Image.Name = s.Remove(0, s.IndexOf('|') + 1);
                else if (s.Contains("."))
                    Image.Name = s.Remove(s.LastIndexOf('.'));
                else
                    Image.Name = s;

                if (s.Contains("|"))
                    Image.FileName = s.Remove(s.IndexOf('|'));
                else
                    Image.FileName = s;

                if (!s.Contains("."))
                    Image.FileName = Image.FileName + ".png";

                if (string.IsNullOrWhiteSpace(Image.FileName) | Image.FileName == ".png")
                {
                    return null;
                }
                else if (Image.Name == "dmg up")
                {
                    Image.Name = "Atk up";
                    Image.FileName = "Atk_up.png";
                }
                else
                {
                    Image.FileName = Image.FileName[0].ToString().ToUpper() + Image.FileName.Remove(0, 1);
                    Image.FileName = Image.FileName.Replace(" ", "_");
                }

                using (MD5 md5Hash = MD5.Create())
                {
                    Image.ImageHash = GetMd5Hash(md5Hash, Image.FileName);
                }

                for (int i = 0; i < DirDepth; i++)
                {
                    hashPartToUse = hashPartToUse + Image.ImageHash[i];
                    baseUri = $"{baseUri}/{hashPartToUse}";
                }

                Image.Uri = $"{baseUri}/{Image.FileName}";
                return Image;
            }

            /// <summary>
            /// From https://docs.microsoft.com/en-us/dotnet/api/system.security.cryptography.md5?view=netframework-4.7.2
            /// </summary>
            /// <param name="md5Hash">The MD5Hash Class</param>
            /// <param name="input">Our string which will make the hash</param>
            /// <returns></returns>
            private static string GetMd5Hash(MD5 md5Hash, string input)
            {

                // Convert the input string to a byte array and compute the hash.
                byte[] data = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(input));

                // Create a new Stringbuilder to collect the bytes
                // and create a string.
                StringBuilder sBuilder = new StringBuilder();

                // Loop through each byte of the hashed data 
                // and format each one as a hexadecimal string.
                for (int i = 0; i < data.Length; i++)
                {
                    sBuilder.Append(data[i].ToString("x2"));
                }

                // Return the hexadecimal string.
                return sBuilder.ToString();
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="VideoName"></param>
            /// <returns></returns>
            public async static Task<VideoInformation> Video(string VideoName)
            {
                VideoInformation video = null;
                try
                {
                    while (VideoName.LastIndexOf("|") != -1)
                        VideoName = VideoName.Remove(VideoName.LastIndexOf("|"));

                    video = new VideoInformation { Name = VideoName, Uri = VideoName };
                    //[ytp-title-link yt-uix-sessionlink]
                    //foreach (HtmlNode col in new HtmlWeb().Load($"https://fategrandorder.fandom.com/wiki/File:{VideoName}").DocumentNode.SelectNodes("//div"))
                    //{
                    //    //For in case we put the person in wrong
                    //    if (string.IsNullOrEmpty(col.InnerText))
                    //        break;

                    //    var resultString = Regex.Split(col.InnerHtml, @"\n");

                    //    foreach (string s in resultString)
                    //    {
                    //    }
                    //}
                }
                catch (Exception e)
                {
                    Logger.LogConsole(e, "Looks like something happened in GetVideo Logic", $"Video name: {video.Name}");
                    Logger.LogFile(e, "Looks like something happened when accessing/using the cache for items", $"Video name: {video.Name}");
                }
                return video;
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="s"></param>
            /// <param name="WhatToFill"></param>
            /// <param name="ItemNumber"></param>
            /// <param name="MakeAscensionSkillReinforcement"></param>
            /// <param name="AscensionSkillReinforcementNumber"></param>
            /// <param name="AscensionToMake"></param>
            /// <returns></returns>
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
                    Logger.LogConsole(e, $"Looks like something failed when assigning an Item", $"WhatToFill.EnglishName: {WhatToFill.EnglishName}");
                    Logger.LogFile(e, $"Looks like something failed when assigning an Item", $"WhatToFill.EnglishName: {WhatToFill.EnglishName}");
                }
                return WhatToFill;
            }
        }
    }
}
