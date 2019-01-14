using System;
using System.IO;
using System.Text;
using System.Linq;
using HtmlAgilityPack;
using Newtonsoft.Json;
using System.Threading.Tasks;
using FateGrandOrderApi.Classes;
using FateGrandOrderApi.Caching;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using static FateGrandOrderApi.Logging.Logger;
using static FateGrandOrderApi.FateGrandOrderParsing.AssigningContent;

namespace FateGrandOrderApi
{
    /// <summary>
    /// Class containing parsing logic (this is where you get what you need from the api)
    /// </summary>
    public static class FateGrandOrderParsing
    {
        static FateGrandOrderParsing()
        {
            try
            {
                if (!Directory.Exists(Settings.Cache.UserFilesLocation))
                    Directory.CreateDirectory(Settings.Cache.UserFilesLocation);
                if (!Directory.Exists(Settings.Cache.GlobalFilesLocation))
                    Directory.CreateDirectory(Settings.Cache.GlobalFilesLocation);
            }
            catch (Exception e)
            {
                LogConsole(e, "Looks like something happened when setting up directory's");
                LogFile(e, "Looks like something happened when setting up directory's");

            }
        }

        internal static string FixString(string s)
        {
            if (!string.IsNullOrWhiteSpace(s))
                return s.Replace("&lt;", "<").Replace("%27", "'").Replace("<br>", "<br/>").Replace("%26", "%").Replace("<br />", "<br/>").Replace("&amp;", "&");
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
            foreach (HtmlNode col in new HtmlWeb().Load($"https://fategrandorder.fandom.com/wiki/{skillName}?action=edit").DocumentNode.SelectNodes("//textarea"))
            {
                //For in case we put the person in wrong
                if (string.IsNullOrEmpty(col.InnerText))
                    return null;

                skill = new Skill(skillName, col.InnerText);
                resultString = Regex.Split(col.InnerText, @"\n");

                if (Settings.Cache.CacheSkills)
                {
                    try
                    {
                        foreach (Skill skillC in FateGrandOrderApiCache.Skills)
                        {
                            if (skill.GeneratedWith == skillC.GeneratedWith && skillC.NamePassed == skill.NamePassed)
                            {
#if DEBUG
                                skillC.FromCache = true;
#endif
                                skill = null;
                                skillName = null;
                                resultString = null;
                                return Tuple.Create(skillC, skillC.GeneratedWith.Split('\n'));
                            }
                            else if (skill.GeneratedWith != skillC.GeneratedWith && skill.NamePassed == skillC.NamePassed)
                            {
                                skill = skillC;
                                break;
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        LogConsole(e, "Looks like something happened when accessing/using the cache for skills", $"Skill name: {skill.Name}");
                        LogFile(e, "Looks like something happened when accessing/using the cache for skills", $"Skill name: {skill.Name}");
                    }
                }
                
                if (skill != null && !FateGrandOrderApiCache.Skills.Contains(skill))
                    FateGrandOrderApiCache.Skills.Add(skill);

                foreach (string s in resultString)
                {
                    if (s.Contains("|img"))
                    {
                        var image = await Image(s);
                        skill.Image = image;
                        image = null;
                    }
                    else if (s.Contains("|name"))
                    {
                        skill.Name = await GenericAssigning(s, "|name");
                    }
                    else if (s.Contains("|rank"))
                    {
                        skill.Rank = await GenericAssigning(s, "|rank");
                    }
                    else if (s.Contains("|effect"))
                    {
                        try
                        {
                            var effects = await GenericAssigning(s, "|effect", new string[] { "''" }, new string[][] { new string[] { "<br/>", "\\" }, new string[] { "<sup>", "(" }, new string[] { "</sup>", ")" } });
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
                            LogConsole(e, $"Looks like something happened when filling up Effect in a skill called {skill.Name}", $"String used when doing this: {s}");
                            LogFile(e, $"Looks like something happened when filling up Effect in a skill called {skill.Name}", $"String used when doing this: {s}");
                        }
                        break;
                    }
                }

                await FateGrandOrderApiCache.SaveCache(FateGrandOrderApiCache.Skills);
                if (skill != null)
                {
                    skillName = null;
                    return Tuple.Create(skill, resultString);
                }
                else
                    return null;
            }
            skillName = null;
            return Tuple.Create(skill, resultString);
        }

        /// <summary>
        /// This will return a filled in ActiveSkill (If the skill isn't a ActiveSkill it will return the core skill content)
        /// </summary>
        /// <param name="skillName">The ActiveSkill name to look for</param>
        /// <returns></returns>
        public static async Task<ActiveSkill> GetActiveSkill(string skillName)
        {
            return await GetActiveSkill(new ActiveSkill(skillName, ""));
        }

        /// <summary>
        /// This will return a filled in ActiveSkill (If the skill isn't a ActiveSkill it will return the core skill content)
        /// </summary>
        /// <param name="skill">The ActiveSkill to put all the content into</param>
        /// <returns></returns>
        public static async Task<ActiveSkill> GetActiveSkill(ActiveSkill skill)
        {
            Tuple<Skill, string[]> content = null;
            string lastLevelEffect = "";
            if (skill.NamePassed != null)
                content = await GetSkill(skill.NamePassed);
            else
                content = await GetSkill(skill.Name);
            var basicSkillContent = content.Item1;
            string[] resultString = content.Item2;
            if (string.IsNullOrWhiteSpace(skill.GeneratedWith))
                skill.GeneratedWith = content.Item1.GeneratedWith;
            if (string.IsNullOrWhiteSpace(skill.NamePassed))
                skill.NamePassed = content.Item1.NamePassed;

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
                    LogConsole(e, "Looks like something happened when GetStartPart was called", $"What lastLevelEffect was when this ex happened: {lastLevelEffect}");
                    LogFile(e, "Looks like something happened when GetStartPart was called", $"What lastLevelEffect was when this ex happened: {lastLevelEffect}");
                    return "|";
                }
            }

            //For in case we put the person in wrong
            if (resultString == null)
            {
                content = null;
                basicSkillContent = null;
                resultString = null;
                return skill;
            }

            if (Settings.Cache.CacheActiveSkills)
            {
                try
                {
                    foreach (ActiveSkill activeSkillC in FateGrandOrderApiCache.ActiveSkills)
                    {
                        if (skill.GeneratedWith == activeSkillC.GeneratedWith && activeSkillC.NamePassed == skill.NamePassed)
                        {
#if DEBUG
                            activeSkillC.FromCache = true;
#endif
                            skill = null;
                            lastLevelEffect = null;
                            content = null;
                            basicSkillContent = null;
                            resultString = null;
                            return activeSkillC;
                        }
                        else if (skill.GeneratedWith != activeSkillC.GeneratedWith && skill.NamePassed == activeSkillC.NamePassed)
                        {
                            skill = activeSkillC;
                            break;
                        }
                    }
                }
                catch (Exception e)
                {
                    LogConsole(e, "Looks like something happened when accessing/using the cache for active skill", $"Active skill name: {skill.Name}");
                    LogFile(e, "Looks like something happened when accessing/using the cache for active skill", $"Active skill name: {skill.Name}");
                }
            }

            if (skill != null && !FateGrandOrderApiCache.ActiveSkills.Contains(skill))
                FateGrandOrderApiCache.ActiveSkills.Add(skill);

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

                        var servants = await GenericArrayAssigning(servantIcons, "|servanticons", '\\', new string[] { "{{" }, new string[][] { new string[] { "}}", "\\" } });
                        foreach (string servant in servants)
                        {
                            var servantP = await GetServant(servant, PresetsForInformation.BasicInformation);
                            if (servantP != null && servantP.BasicInformation != null)
                            {
                                if(skill.ServantsThatHaveThisSkill == null)
                                    skill.ServantsThatHaveThisSkill = new List<Servant>();

                                skill.ServantsThatHaveThisSkill.Add(servantP);
                            }
                            servantP = null;
                        }
                        servantIcons = null;
                        servants = null;
                    }
                    catch (Exception e)
                    {
                        LogConsole(e, $"Looks like something happened when filling up ServantsThatHaveThisSkill in active skill {skill.Name}", $"String used when doing this: {s}");
                        LogFile(e, $"Looks like something happened when filling up ServantsThatHaveThisSkill in active skill {skill.Name}", $"String used when doing this: {s}");
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
                        if(skill.LevelEffects == null)
                            skill.LevelEffects = new List<LevelEffect10>();
                        skill.LevelEffects.Add(new LevelEffect10 { LevelEffectName = await GenericAssigning(lastLevelEffect, $"{GetStartPart()}leveleffect") });
                    }
                    catch (Exception e)
                    {
                        LogConsole(e, $"Looks like something happened when making a LevelEffects in active skill {skill.Name}", $"String used when doing this: {s}");
                        LogFile(e, $"Looks like something happened when making a LevelEffects in active skill {skill.Name}", $"String used when doing this: {s}");
                    }
                }
                else if (s.Contains($"{GetStartPart()}l1 "))
                {
                    skill.LevelEffects[skill.LevelEffects.Count - 1].Level1Effect.EffectStrength = await GenericAssigning(s, $"{GetStartPart()}l1");
                }
                else if (s.Contains($"{GetStartPart()}l2"))
                {
                    skill.LevelEffects[skill.LevelEffects.Count - 1].Level2Effect.EffectStrength = await GenericAssigning(s, $"{GetStartPart()}l2");
                }
                else if (s.Contains($"{GetStartPart()}l3"))
                {
                    skill.LevelEffects[skill.LevelEffects.Count - 1].Level3Effect.EffectStrength = await GenericAssigning(s, $"{GetStartPart()}l3");
                }
                else if (s.Contains($"{GetStartPart()}l4"))
                {
                    skill.LevelEffects[skill.LevelEffects.Count - 1].Level4Effect.EffectStrength = await GenericAssigning(s, $"{GetStartPart()}l4");
                }
                else if (s.Contains($"{GetStartPart()}l5"))
                {
                    skill.LevelEffects[skill.LevelEffects.Count - 1].Level5Effect.EffectStrength = await GenericAssigning(s, $"{GetStartPart()}l5");
                }
                else if (s.Contains($"{GetStartPart()}l6"))
                {
                    skill.LevelEffects[skill.LevelEffects.Count - 1].Level6Effect.EffectStrength = await GenericAssigning(s, $"{GetStartPart()}l6");
                }
                else if (s.Contains($"{GetStartPart()}l7"))
                {
                    skill.LevelEffects[skill.LevelEffects.Count - 1].Level7Effect.EffectStrength = await GenericAssigning(s, $"{GetStartPart()}l7");
                }
                else if (s.Contains($"{GetStartPart()}l8"))
                {
                    skill.LevelEffects[skill.LevelEffects.Count - 1].Level8Effect.EffectStrength = await GenericAssigning(s, $"{GetStartPart()}l8");
                }
                else if (s.Contains($"{GetStartPart()}l9"))
                {
                    skill.LevelEffects[skill.LevelEffects.Count - 1].Level9Effect.EffectStrength = await GenericAssigning(s, $"{GetStartPart()}l9");
                }
                else if (s.Contains($"{GetStartPart()}l10"))
                {
                    skill.LevelEffects[skill.LevelEffects.Count - 1].Level10Effect.EffectStrength = await GenericAssigning(s, $"{GetStartPart()}l10");
                }
                else if (s.Contains("|c1 "))
                {
                    skill.LevelEffects[skill.LevelEffects.Count - 1].Level1Effect.Cooldown = await GenericAssigning(s, "|c1");
                }
                else if (s.Contains("|c2"))
                {
                    skill.LevelEffects[skill.LevelEffects.Count - 1].Level2Effect.Cooldown = await GenericAssigning(s, "|c2");
                }
                else if (s.Contains("|c3"))
                {
                    skill.LevelEffects[skill.LevelEffects.Count - 1].Level3Effect.Cooldown = await GenericAssigning(s, "|c3");
                }
                else if (s.Contains("|c4"))
                {
                    skill.LevelEffects[skill.LevelEffects.Count - 1].Level4Effect.Cooldown = await GenericAssigning(s, "|c4");
                }
                else if (s.Contains("|c5"))
                {
                    skill.LevelEffects[skill.LevelEffects.Count - 1].Level5Effect.Cooldown = await GenericAssigning(s, "|c5");
                }
                else if (s.Contains("|c6"))
                {
                    skill.LevelEffects[skill.LevelEffects.Count - 1].Level6Effect.Cooldown = await GenericAssigning(s, "|c6");
                }
                else if (s.Contains("|c7"))
                {
                    skill.LevelEffects[skill.LevelEffects.Count - 1].Level7Effect.Cooldown = await GenericAssigning(s, "|c7");
                }
                else if (s.Contains("|c8"))
                {
                    skill.LevelEffects[skill.LevelEffects.Count - 1].Level8Effect.Cooldown = await GenericAssigning(s, "|c8");
                }
                else if (s.Contains("|c9"))
                {
                    skill.LevelEffects[skill.LevelEffects.Count - 1].Level9Effect.Cooldown = await GenericAssigning(s, "|c9");
                }
                else if (s.Contains("|c10"))
                {
                    skill.LevelEffects[skill.LevelEffects.Count - 1].Level10Effect.Cooldown = await GenericAssigning(s, "|c10");
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
            basicSkillContent = null;
            await FateGrandOrderApiCache.SaveCache(FateGrandOrderApiCache.ActiveSkills);
            return skill;
        }
        #endregion

        /// <summary>
        /// Returns a item in the Fate/Grand Order (will return null if the item is not found)
        /// </summary>
        /// <param name="itemName">The item's name</param>
        /// <param name="enemyToNotLookFor">enemy that been found already and is known to drop this item</param>
        /// <returns></returns>
        public async static Task<Item> GetItem(string itemName, Enemy enemyToNotLookFor = null)
        {
            bool DoingLocationLogic = false;
            bool GettingItem = false;
            Item item = null;
            foreach (HtmlNode col in new HtmlWeb().Load($"https://fategrandorder.fandom.com/wiki/{itemName}?action=edit").DocumentNode.SelectNodes("//textarea"))
            {
                //For in case we put the person in wrong
                if (string.IsNullOrEmpty(col.InnerText))
                    return null;

                item = new Item(col.InnerText, itemName);

                if (Settings.Cache.CacheItems)
                {
                    try
                    {
                        foreach (Item itemC in FateGrandOrderApiCache.Items)
                        {
                            if (item.GeneratedWith == itemC.GeneratedWith && itemC.EnglishName == item.EnglishName)
                            {
#if DEBUG
                                itemC.FromCache = true;
#endif
                                item = null;
                                itemName = null;
                                return itemC;
                            }
                            else if (item.GeneratedWith != itemC.GeneratedWith && item.EnglishName == itemC.EnglishName)
                            {
                                item = itemC;
                                break;
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        LogConsole(e, "Looks like something happened when accessing/using the cache for items", $"Item name: {item.EnglishName}");
                        LogFile(e, "Looks like something happened when accessing/using the cache for items", $"Item name: {item.EnglishName}");
                    }
                }
                if (item != null && !FateGrandOrderApiCache.Items.Contains(item))
                    FateGrandOrderApiCache.Items.Add(item);

                var resultString = Regex.Split(col.InnerText, @"\n");

                foreach (string s in resultString)
                {
                    if (resultString[0].Trim() == "{{ItemBox" || resultString[1].Trim() == "{{ItemBox")
                    {
                        if (!string.IsNullOrWhiteSpace(s) && s == "}}" || FixString(s) == "}}</onlyinclude>" && DoingLocationLogic)
                        {
                            DoingLocationLogic = false;
                        }
                        else if (DoingLocationLogic)
                        {
                            await LocationLogic(s);
                        }
                        else if (s.Contains("|jpName"))
                        {
                            item.JapaneseName = await GenericAssigning(s, "|jpName", PartsToReplace: new string[][] { new string[] { "<br/>", "\r\n" } });
                        }
                        else if (s.Contains("|image"))
                        {
                            item.ItemImage = await Image(s, "|image");
                        }
                        else if (s.Contains("|enemy"))
                        {
                            var enemys = await GenericArrayAssigning(s, "|enemy", '/', OtherPartsToRemove: new string[] { "[[", "]]" }, PartsToReplace: new string[][] { new string[] { "<br/>", "/" }, new string[] { " / ", "/" } });
                            if (enemys != null && enemys.Length > 0)
                                item.AnythingThatDropsThis = new ItemDrops();
                            foreach (string enemy in enemys)
                            {
                                string enemyEdited = enemy;
                                if (enemyEdited.Contains("Shadow Servant"))
                                    enemyEdited = "Shadow Servant";
                                try
                                {
                                    if (!enemyEdited.Contains("}}-class"))
                                    {
                                        if (enemyEdited.IndexOf('|') != -1)
                                            enemyEdited = enemyEdited.Remove(0, enemyEdited.IndexOf('|') + 1);

                                        if (enemyToNotLookFor != null && enemyToNotLookFor.EnglishName == enemyEdited)
                                        {
                                            if (item.AnythingThatDropsThis.Enemies == null)
                                                item.AnythingThatDropsThis.Enemies = new List<Enemy>();
                                            item.AnythingThatDropsThis.Enemies.Add(enemyToNotLookFor);
                                        }
                                        else
                                        {
                                            var enemyP = await GetEnemy(enemyEdited, item);
                                            if (enemyP != null)
                                            {
                                                if (item.AnythingThatDropsThis.Enemies == null)
                                                    item.AnythingThatDropsThis.Enemies = new List<Enemy>();
                                                item.AnythingThatDropsThis.Enemies.Add(enemyP);
                                            }
                                            enemyP = null;
                                        }
                                    }
                                    else if (enemyEdited.Contains("}}-class"))
                                    {
                                        enemyEdited = enemyEdited.Replace("}}-class", "").Replace("{{", "");
                                        string[] strings = enemyEdited.Split(' ');
                                        await UsesLogic(strings);
                                        strings = null;
                                    }
                                }
                                catch (Exception e)
                                {
                                    LogConsole(e, $"Looks like something failed when getting the enemys that drop {item.EnglishName}", $"Item name: {item.EnglishName}\r\nEnemy name: {enemyEdited}");
                                    LogFile(e, $"Looks like something failed when getting the enemys that drop {item.EnglishName}", $"Item name: {item.EnglishName}\r\nEnemy name: {enemyEdited}");
                                }
                                enemyEdited = null;
                            }
                            enemys = null;
                        }
                        else if (s.Contains("|jdesc"))
                        {
                            item.JapaneseDescription = await GenericAssigning(s, "|jdesc", OtherPartsToRemove: new string[] { "<br/>" });
                        }
                        else if (s.Contains("|desc"))
                        {
                            item.EnglishDescription = await GenericAssigning(s, "|desc");
                        }
                        else if (s.Contains("|location"))
                        {
                            string ss = await GenericAssigning(s, "|location");
                            if (!string.IsNullOrWhiteSpace(ss))
                                await LocationLogic(ss);
                            else
                                DoingLocationLogic = true;
                            ss = null;
                        }
                        else if (s.Contains("|usedFor"))
                        {
                            if(item.Uses == null)
                                item.Uses = new List<Servant>();
                            foreach (string ss in (await GenericAssigning(s, "|usedFor")).Replace(" {{", "\\").Replace("{{", "\\").Replace("}}", "").Split('\\'))
                            {
                                var servant = await GetServant(ss, PresetsForInformation.BasicInformation);
                                if (servant != null)
                                    item.Uses.Add(servant);
                                servant = null;
                            }
                        }
                    }
                    else
                    {
                        if (s == $"== {itemName} ==" || s == $"=={itemName}==")
                        {
                            GettingItem = true;
                        }
                        else if (s == "|}")
                        {
                            GettingItem = false;
                        }
                        else if (GettingItem)
                        {
                            if (s.Contains("style") && s != "{| style=\"width: 100%; text-align: center;\" class=\"wikitable\"")
                            {
                                string thing = s.Remove(0, 1);
                                thing = thing.Remove(0, thing.IndexOf('|') + 1);
                                if (thing.StartsWith("[["))
                                {
                                    item.ItemImage = await Image(thing.Replace("file", "File").Replace("|100px|link=]]",""));
                                    item.ItemImage.Name = itemName;
                                }
                                else
                                {
                                    item.JapaneseName = thing;
                                }
                            }
                            else if (s.Contains("colspan=\"3\"") && s.Contains("[["))
                            {
                                if(item.Uses == null)
                                    item.Uses = new List<Servant>();
                                string thing = s.Remove(0, 1);
                                thing = thing.Remove(0, thing.IndexOf('|') + 1);
                                var servants = thing.Replace("]] and [[", "\\").Replace("Every [[", "").Replace("]]", "").Split('\\');
                                foreach (string ss in servants)
                                {
                                    await UsesLogic(new string[] { ss }, true);
                                }
                            }
                        }
                    }
                }
                resultString = null;
            }
            
            await FateGrandOrderApiCache.SaveCache(FateGrandOrderApiCache.Items);
            itemName = null;
            return item;

            async Task UsesLogic(string[] ServantsPage, bool GoToUses = false)
            {
                foreach (HtmlNode col2 in new HtmlWeb().Load($"https://fategrandorder.fandom.com/wiki/{ServantsPage[0]}?action=edit").DocumentNode.SelectNodes("//textarea"))
                {
                    if (col2.InnerText == null)
                        break;

                    var resultString2 = Regex.Split(col2.InnerText, @"\n");

                    foreach (string ss in resultString2)
                    {
                        if (ss.StartsWith("|[["))
                        {
                            string thing = FixString(ss);
                            try
                            {
                                if (!thing.EndsWith("]"))
                                    thing = thing.Remove(thing.LastIndexOf(']') + 1);
                                thing = thing.Replace("|[[", "").Replace("]]", "");
                                if (thing.Contains('|'))
                                    thing = thing.Remove(0, thing.IndexOf('|') + 1);

                                if (ServantsPage.Length == 1 || ServantsPage[1] == "Servant")
                                {
                                    var Servant = await GetServant(thing, PresetsForInformation.BasicInformation);
                                    if (Servant != null)
                                    {
                                        if (!GoToUses)
                                        {
                                            if (item.AnythingThatDropsThis.Servants == null)
                                                item.AnythingThatDropsThis.Servants = new List<Servant>();
                                            item.AnythingThatDropsThis.Servants.Add(Servant);
                                        }
                                        else
                                        {
                                            if (item.Uses == null)
                                                item.Uses = new List<Servant>();
                                            item.Uses.Add(Servant);
                                        }
                                    }
                                    Servant = null;
                                }
                                else if (ServantsPage[1] == "Enemy")
                                {
                                    var Enemy = await GetEnemy(thing);
                                    if (Enemy != null)
                                    {
                                        if (item.AnythingThatDropsThis.Enemies == null)
                                            item.AnythingThatDropsThis.Enemies = new List<Enemy>();
                                        item.AnythingThatDropsThis.Enemies.Add(Enemy);
                                    }
                                    Enemy = null;
                                }
                            }
                            catch (Exception e)
                            {
                                LogConsole(e, $"Looks like something failed when getting the enemys that drop {item.EnglishName}", $"Item name: {item.EnglishName}\r\nEnemy name: {thing}");
                                LogFile(e, $"Looks like something failed when getting the enemys that drop {item.EnglishName}", $"Item name: {item.EnglishName}\r\nEnemy name: {thing}");
                            }
                        }
                    }
                    resultString2 = null;
                }
            }

            async Task LocationLogic(string s)
            {
                try
                {
                    if (!string.IsNullOrWhiteSpace(s) && FixString(s) != "<tabber>" && FixString(s) != "</tabber>")
                    {
                        if (item.DropLocations == null)
                            item.DropLocations = new List<ItemDropLocationList> { new ItemDropLocationList { } };
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
                                    var thing = await GenericArrayAssigning(ss, "", ']', new string[] { "<br/>" }, new string[][] { new string[] { "[[", "[" }, new string[] { "]]", "]" } });
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
                                    thing = null;
                                }
                            }
                            catch (Exception e)
                            {
                                LogConsole(e, "Looks like something happened while filling up a item stat", $"Item name: {item.EnglishName}");
                                LogFile(e, "Looks like something happened while filling up a item stat", $"Item name: {item.EnglishName}");
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    LogConsole(e, "Looks like something happened when doing DoingLocationLogic if statement", $"Item name: {item.EnglishName}");
                    LogFile(e, "Looks like something happened when doing DoingLocationLogic if statement", $"Item name: {item.EnglishName}");
                }
            }
        }

        /// <summary>
        /// Return a enemy in Fate/Grand Order (will return null if not found)
        /// </summary>
        /// <param name="enemyName">The enemy's name</param>
        /// <param name="itemToNotLookFor">Item that already been found and is known for dropping this item</param>
        /// <returns></returns>
        public async static Task<Enemy> GetEnemy(string enemyName, Item itemToNotLookFor = null)
        {
            bool GettingImages = false;
            bool GettingRecommendedServants = false;
            Enemy enemy = null;
            foreach (HtmlNode col in new HtmlWeb().Load($"https://fategrandorder.fandom.com/wiki/{enemyName}?action=edit").DocumentNode.SelectNodes("//textarea"))
            {
                //For in case we put the person in wrong or it doesn't have a webpage
                if (string.IsNullOrEmpty(col.InnerText))
                    return null;

                enemy = new Enemy(enemyName, col.InnerHtml);

                if (Settings.Cache.CacheEnemies)
                {
                    try
                    {
                        foreach (Enemy enemyC in FateGrandOrderApiCache.Enemies)
                        {
                            if (enemy.GeneratedWith == enemyC.GeneratedWith && enemyC.EnglishName == enemy.EnglishName)
                            {
#if DEBUG
                                enemyC.FromCache = true;
#endif
                                enemy = null;
                                enemyName = null;
                                return enemyC;
                            }
                            else if (enemy.GeneratedWith != enemyC.GeneratedWith && enemy.EnglishName == enemyC.EnglishName)
                            {
                                enemy = enemyC;
                                break;
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        LogConsole(e, "Looks like something happened when accessing/using the cache for enemy", $"Enemy name: {enemy.EnglishName}");
                        LogFile(e, "Looks like something happened when accessing/using the cache for enemy", $"Enemy name: {enemy.EnglishName}");
                    }
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
                                                servants = new string[] { await GenericAssigning(servants[i], $"|{WhatToLookFor[1]}") };
                                                break;
                                            }
                                            else if (servants[i].Contains("|#default"))
                                            {
                                                servants = new string[] { await GenericAssigning(servants[i], "|#default") };
                                                break;
                                            }
                                        }
                                    }
                                    foreach (var ss in Regex.Split(servants[0], "}}}} "))
                                    {
                                        var personcontent = ss.Split('|');
                                        var servant = await GetServant(personcontent[1].Replace("{{", "").Replace("}}", ""), PresetsForInformation.BasicInformation);
                                        if (servant != null && servant.BasicInformation != null)
                                            enemy.RecommendedServants.Add(servant);
                                        personcontent = null;
                                        servant = null;
                                    }
                                    servants = null;
                                }
                                WhatToLookFor = null;
                            }
                            catch (Exception e)
                            {
                                LogConsole(e, $"Looks like something failed when assigning enemy.RecommendedServants", $"Enemy name: {enemy.EnglishName}");
                                LogFile(e, $"Looks like something failed when assigning enemy.RecommendedServants", $"Enemy name: {enemy.EnglishName}");
                            }
                        }
                    }

                    if (enemy != null && !FateGrandOrderApiCache.Enemies.Contains(enemy))
                        FateGrandOrderApiCache.Enemies.Add(enemy);

                    if (s.Contains("|image"))
                    {
                        if (FixString(s).Contains("<gallery>"))
                            GettingImages = true;
                        else
                        {
                            var image = await Image(s, "|image");
                            if (image != null)
                            {
                                if(enemy.EnemyImage == null)
                                    enemy.EnemyImage = new List<ImageInformation>();
                                enemy.EnemyImage.Add(image);
                            }
                            image = null;
                        }
                    }
                    if (GettingImages && FixString(s).Contains("</gallery>"))
                    {
                        GettingImages = false;
                    }
                    else if (GettingImages)
                    {
                        var image = await Image(s, "");
                        if (image != null)
                        {
                            if (enemy.EnemyImage == null)
                                enemy.EnemyImage = new List<ImageInformation>();
                            enemy.EnemyImage.Add(image);
                        }
                        image = null;
                    }
                    else if (s.Contains("|class"))
                    {
                        enemy.Class = await GenericArrayAssigning(s, "|class", '\\', new string[] { "{{", "}}" }, new string[][] { new string[] { "}}{{", "\\" } });
                    }
                    else if (s.Contains("|area"))
                    {
                        var thing = await GenericArrayAssigning(s, "|area", OtherPartsToRemove: new string[] { "<br/>" });
                        if (thing.Length > 0 && !string.IsNullOrWhiteSpace(thing[0]))
                        {
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
                                LogConsole(e, $"Looks like something failed when assigning enemy.Areas", $"Enemy name: {enemy.EnglishName}");
                                LogFile(e, $"Looks like something failed when assigning enemy.Areas", $"Enemy name: {enemy.EnglishName}");
                            }
                        }
                        thing = null;
                    }
                    else if (s.Contains("|jname"))
                    {
                        enemy.JapaneseName = await GenericAssigning(s, "|jname");
                    }
                    else if (s.Contains("|rank"))
                    {
                        enemy.Rank = await GenericAssigning(s, "|rank");
                    }
                    else if (s.Contains("|gender"))
                    {
                        await Gender(s, enemy);
                    }
                    else if (s.Contains("|attribute"))
                    {
                        enemy.Attribute = await GenericAssigning(s, "|attribute");
                    }
                    else if (s.Contains("|traits"))
                    {
                        enemy.Traits = await GenericArrayAssigning(s, "|traits");
                    }
                    else if (s.Contains("|drop"))
                    {
                        try
                        {
                            var items = await GenericArrayAssigning(s, "|drop", '\\', new string[] { "{{", "<br/>" }, new string[][] { new string[] { "}}", "\\" } });
                            if(items.Length > 0 && !string.IsNullOrWhiteSpace(items[0]))
                                enemy.WhatThisEnemyDrops = new List<Item>();
                            foreach (string item in items)
                            {
                                if (!string.IsNullOrWhiteSpace(item))
                                {
                                    if (itemToNotLookFor != null && items.Contains(itemToNotLookFor.EnglishName))
                                        enemy.WhatThisEnemyDrops.Add(itemToNotLookFor);
                                    else
                                    {
                                        var itemP = await GetItem(item, enemy);
                                        if(itemP != null)
                                            enemy.WhatThisEnemyDrops.Add(itemP);
                                        itemP = null;
                                    }
                                }
                            }
                            items = null;
                        }
                        catch (Exception e)
                        {
                            LogConsole(e, $"Looks like something failed when assigning enemy.WhatThisEnemyDrops", $"Enemy name: {enemy.EnglishName}");
                            LogFile(e, $"Looks like something failed when assigning enemy.WhatThisEnemyDrops", $"Enemy name: {enemy.EnglishName}");
                        }
                    }
                    else if (s == "==Recommended Servants==" | s == "== Recommended Servants ==")
                    {
                        GettingRecommendedServants = true;
                        enemy.RecommendedServants = new List<Servant>();
                    }
                }
                resultString = null;
            }

            await FateGrandOrderApiCache.SaveCache(FateGrandOrderApiCache.Enemies);
            enemyName = null;
            return enemy;
        }

        /// <summary>
        /// This will return the servant from the servant name (will return null if we are unable to find the person)
        /// </summary>
        /// <param name="ServantName">The Servant's name</param>
        /// <param name="presetsForInformation">Preset to use</param>
        /// <param name="GetBasicInformation">If to get the basic infomation</param>
        /// <param name="GetActiveSkills">If to get Active Skills</param>
        /// <param name="GetPassiveSkills">If to get Passive Skills</param>
        /// <param name="GetNoblePhantasm">If to get Noble Phantasm</param>
        /// <param name="GetAscension">If to get Ascension</param>
        /// <param name="GetSkillReinforcement">If to get the Skill Reinforcement</param>
        /// <param name="GetStats">If to get the Stats</param>
        /// <param name="GetBondLevel">If to get the Bond Levels</param>
        /// <param name="GetBiography">If to get the servant's Biography</param>
        /// <param name="GetAvailability">If to get when this servants been available</param>
        /// <param name="GetTrivia">If to get Trivia</param>
        /// <param name="GetImages">If to get the servants Images</param>
        /// <returns></returns>
        public static async Task<Servant> GetServant(string ServantName, PresetsForInformation presetsForInformation = PresetsForInformation.AllInformation, ToGrab GetBasicInformation = ToGrab.NotSet, ToGrab GetActiveSkills = ToGrab.NotSet, ToGrab GetPassiveSkills = ToGrab.NotSet, ToGrab GetNoblePhantasm = ToGrab.NotSet, ToGrab GetAscension = ToGrab.NotSet, ToGrab GetSkillReinforcement = ToGrab.NotSet, ToGrab GetStats = ToGrab.NotSet, ToGrab GetBondLevel = ToGrab.NotSet, ToGrab GetBiography = ToGrab.NotSet, ToGrab GetAvailability = ToGrab.NotSet, ToGrab GetTrivia = ToGrab.NotSet, ToGrab GetImages = ToGrab.NotSet)
        {
            Servant Servant = null;

            #region Toggles For GettingInformation
            bool _GetBasicInformation = false;
            bool _GetActiveSkills = false;
            bool _GetPassiveSkills = false;
            bool _GetNoblePhantasm = false;
            bool _GetAscension = false;
            bool _GetSkillReinforcement = false;
            bool _GetStats = false;
            bool _GetBondLevel = false;
            bool _GetBiography = false;
            bool _GetAvailability = false;
            bool _GetTrivia = false;
            bool _GetImages = false;

            if (presetsForInformation == PresetsForInformation.BasicInformation)
            {
                _GetBasicInformation = true;
            }
            else if (presetsForInformation == PresetsForInformation.AllInformation)
            {
                _GetBasicInformation = true;
                _GetActiveSkills = true;
                _GetPassiveSkills = true;
                _GetNoblePhantasm = true;
                _GetAscension = true;
                _GetSkillReinforcement = true;
                _GetStats = true;
                _GetBondLevel = true;
                _GetBiography = true;
                _GetAvailability = true;
                _GetTrivia = true;
                _GetImages = true;
            }

            if (GetBasicInformation != ToGrab.NotSet) _GetBasicInformation = FromToGrabToBool(GetBasicInformation);
            if (GetActiveSkills != ToGrab.NotSet) _GetActiveSkills = FromToGrabToBool(GetActiveSkills);
            if (GetPassiveSkills != ToGrab.NotSet) _GetPassiveSkills = FromToGrabToBool(GetPassiveSkills);
            if (GetNoblePhantasm != ToGrab.NotSet) _GetNoblePhantasm = FromToGrabToBool(GetNoblePhantasm);
            if (GetAscension != ToGrab.NotSet) _GetAscension = FromToGrabToBool(GetAscension);
            if (GetSkillReinforcement != ToGrab.NotSet) _GetSkillReinforcement = FromToGrabToBool(GetSkillReinforcement);
            if (GetStats != ToGrab.NotSet) _GetStats = FromToGrabToBool(GetStats);
            if (GetBondLevel != ToGrab.NotSet) _GetBondLevel = FromToGrabToBool(GetBondLevel);
            if (GetBiography != ToGrab.NotSet) _GetBiography = FromToGrabToBool(GetBiography);
            if (GetAvailability != ToGrab.NotSet) _GetAvailability = FromToGrabToBool(GetAvailability);
            if (GetTrivia != ToGrab.NotSet) _GetTrivia = FromToGrabToBool(GetTrivia);
            if (GetImages != ToGrab.NotSet) _GetImages = FromToGrabToBool(GetImages);
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
            bool GettingBond10 = false; //Will not need soon™
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

            if (!_GetBasicInformation)
                GettingBasicInformation = false;

            foreach (HtmlNode col in new HtmlWeb().Load($"https://fategrandorder.fandom.com/wiki/{ServantName}?action=edit").DocumentNode.SelectNodes("//textarea"))
            {
                //For in case we put the person in wrong
                if (string.IsNullOrEmpty(col.InnerText))
                    return null;

                ServantName = FixString(ServantName.Replace("_", " "));
                Servant = new Servant(col.InnerText, ServantName);

                #region Caching Logic
                if (Settings.Cache.CacheServants)
                {
                    try
                    {
                        foreach (Servant fateGrandOrderPersonC in FateGrandOrderApiCache.Servants)
                        {
                            if (fateGrandOrderPersonC.GeneratedWith == Servant.GeneratedWith && fateGrandOrderPersonC.EnglishNamePassed == Servant.EnglishNamePassed)
                            {
                                if (_GetBasicInformation && fateGrandOrderPersonC.BasicInformation != null)
                                {
                                    _GetBasicInformation = false;
                                    GettingBasicInformation = false;
                                }
                                if (_GetActiveSkills && fateGrandOrderPersonC.ActiveSkills != null)
                                {
                                    _GetActiveSkills = false;
                                }
                                if (_GetPassiveSkills && fateGrandOrderPersonC.PassiveSkills != null)
                                {
                                    _GetPassiveSkills = false;
                                }
                                if (_GetNoblePhantasm && fateGrandOrderPersonC.NoblePhantasms != null)
                                {
                                    _GetNoblePhantasm = false;
                                }
                                if (_GetAscension && fateGrandOrderPersonC.Ascension != null)
                                {
                                    _GetAscension = false;
                                }
                                if (_GetSkillReinforcement && fateGrandOrderPersonC.SkillReinforcement != null)
                                {
                                    _GetSkillReinforcement = false;
                                }
                                if (_GetStats && fateGrandOrderPersonC.Stats != null)
                                {
                                    _GetStats = false;
                                }
                                if (_GetBondLevel && fateGrandOrderPersonC.BondLevels != null)
                                {
                                    _GetBondLevel = false;
                                }
                                if (_GetBiography && fateGrandOrderPersonC.Biography != null)
                                {
                                    _GetBiography = false;
                                }
                                if (_GetAvailability && fateGrandOrderPersonC.Availability != null)
                                {
                                    _GetAvailability = false;
                                }
                                if (_GetTrivia && fateGrandOrderPersonC.Trivia != null)
                                {
                                    _GetTrivia = false;
                                }
                                if (_GetImages && fateGrandOrderPersonC.Images != null)
                                {
                                    _GetImages = false;
                                }

                                if (_GetBasicInformation == false && _GetActiveSkills == false && _GetPassiveSkills == false && _GetNoblePhantasm == false && _GetAscension == false && _GetSkillReinforcement == false && _GetStats == false && _GetBondLevel == false && _GetBiography == false && _GetAvailability == false && _GetImages == false && _GetTrivia == false)
                                {
                                    Servant = null;
                                    ServantName = null;
                                    return fateGrandOrderPersonC;
                                }
                                else
                                {
                                    Servant = fateGrandOrderPersonC;
                                }
#if DEBUG
                                fateGrandOrderPersonC.FromCache = true;
#endif
                                break;
                            }
                            else if (fateGrandOrderPersonC.GeneratedWith != Servant.GeneratedWith && fateGrandOrderPersonC.EnglishNamePassed == Servant.EnglishNamePassed)
                            {
                                Servant = fateGrandOrderPersonC;
                                break;
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        LogConsole(e, $"Looks like something failed when accessing FateGrandOrderServant cache", $"Servant name: {Servant.EnglishNamePassed}");
                        LogFile(e, $"Looks like something failed when accessing FateGrandOrderServant cache", $"Servant name: {Servant.EnglishNamePassed}");
                    }
                }
                #endregion

                #region Add/Remove to/from cache
                if (Servant != null && !FateGrandOrderApiCache.Servants.Contains(Servant))
                    FateGrandOrderApiCache.Servants.Add(Servant);
                #endregion

                var resultString = Regex.Split(col.InnerText, @"\n");

                if (_GetBasicInformation && Servant.BasicInformation == null)
                    Servant.BasicInformation = new FateGrandOrderServantBasic(ServantName);

                foreach (string s in resultString)
                {
                    #region Passive Skills
                    if (GettingPassiveSkills)
                    {
                        try
                        {
                            if (!string.IsNullOrWhiteSpace(s) && s[s.Length - 1] == '=')
                            {
                                if (Servant.PassiveSkills[Servant.PassiveSkills.Count - 1].Category != null)
                                    Servant.PassiveSkills.Add(new PassiveSkillList());
                                Servant.PassiveSkills[Servant.PassiveSkills.Count - 1].Category = s.Replace("=", "");
                                PassiveSkillsCount = 0;
                            }
                            else if (s.Contains("|img"))
                            {
                                Servant.PassiveSkills[Servant.PassiveSkills.Count - 1].PassiveSkills.Add(new PassiveSkills());
                                PassiveSkillsCount++;
                                if (PassiveSkillsCount == 1)
                                    Servant.PassiveSkills[Servant.PassiveSkills.Count - 1].PassiveSkills[Servant.PassiveSkills[Servant.PassiveSkills.Count - 1].PassiveSkills.Count - 1].Image = await Image(s);
                                else
                                    Servant.PassiveSkills[Servant.PassiveSkills.Count - 1].PassiveSkills[Servant.PassiveSkills[Servant.PassiveSkills.Count - 1].PassiveSkills.Count - 1].Image = await Image(s.Replace($"|img{PassiveSkillsCount}", "|img"));
                            }
                            else if (s.Contains("|name"))
                            {
                                if (PassiveSkillsCount == 1)
                                    Servant.PassiveSkills[Servant.PassiveSkills.Count - 1].PassiveSkills[Servant.PassiveSkills[Servant.PassiveSkills.Count - 1].PassiveSkills.Count - 1].Name = await GenericAssigning(s, "|name");
                                else
                                    Servant.PassiveSkills[Servant.PassiveSkills.Count - 1].PassiveSkills[Servant.PassiveSkills[Servant.PassiveSkills.Count - 1].PassiveSkills.Count - 1].Name = await GenericAssigning(s, $"|name{PassiveSkillsCount}");
                            }
                            else if (s.Contains("|rank"))
                            {
                                if (PassiveSkillsCount == 1)
                                    Servant.PassiveSkills[Servant.PassiveSkills.Count - 1].PassiveSkills[Servant.PassiveSkills[Servant.PassiveSkills.Count - 1].PassiveSkills.Count - 1].Rank = await GenericAssigning(s, "|rank");
                                else
                                    Servant.PassiveSkills[Servant.PassiveSkills.Count - 1].PassiveSkills[Servant.PassiveSkills[Servant.PassiveSkills.Count - 1].PassiveSkills.Count - 1].Rank = await GenericAssigning(s, $"|rank{PassiveSkillsCount}");
                            }
                            else if (s.Contains("|effect"))
                            {
                                if (PassiveSkillsCount == 1)
                                    Servant.PassiveSkills[Servant.PassiveSkills.Count - 1].PassiveSkills[Servant.PassiveSkills[Servant.PassiveSkills.Count - 1].PassiveSkills.Count - 1].Effect = await GenericArrayAssigning(s, "|effect", PartsToReplace: new string[][] { new string[] { "<br/>", "\\" } }, CharToSplitWith: '\\');
                                else
                                    Servant.PassiveSkills[Servant.PassiveSkills.Count - 1].PassiveSkills[Servant.PassiveSkills[Servant.PassiveSkills.Count - 1].PassiveSkills.Count - 1].Effect = await GenericArrayAssigning(s, $"|effect{PassiveSkillsCount}", PartsToReplace: new string[][] { new string[] { "<br/>", "\\" } }, CharToSplitWith: '\\');
                            }
                        }
                        catch (Exception e)
                        {
                            LogConsole(e, $"Looks like something failed when assigning something in fateGrandOrderPerson.PassiveSkills", $"Servant name: {Servant.EnglishNamePassed}");
                            LogFile(e, $"Looks like something failed when assigning something in fateGrandOrderPerson.PassiveSkills", $"Servant name: {Servant.EnglishNamePassed}");
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
                                Servant.ActiveSkills.Add(new ActiveSkill());
                                if (s.Contains("NPC"))
                                    Servant.ActiveSkills[Servant.ActiveSkills.Count - 1].ForNPC = true;
                            }
                            else if (s.Contains("{{unlock|"))
                            {
                                Servant.ActiveSkills[Servant.ActiveSkills.Count - 1].WhenSkillUnlocks = s.Replace("{{unlock|", "")[0].ToString();
                            }
                            else if (s.Contains(@"{{:"))
                            {
                                if (s.IndexOf("|") != -1)
                                    Servant.ActiveSkills[Servant.ActiveSkills.Count - 1].Name = s.Remove(s.IndexOf("|")).Replace(@"{{:", "");
                                else
                                    Servant.ActiveSkills[Servant.ActiveSkills.Count - 1].Name = s.Replace(@"{{:", "").Replace("}}", "");
                                Servant.ActiveSkills[Servant.ActiveSkills.Count - 1] = await GetActiveSkill(Servant.ActiveSkills[Servant.ActiveSkills.Count - 1]);
                            }
                        }
                        catch (Exception e)
                        {
                            LogConsole(e, $"Looks like something failed when assigning something in fateGrandOrderPerson.ActiveSkills", $"Servant name: {Servant.EnglishNamePassed}");
                            LogFile(e, $"Looks like something failed when assigning something in fateGrandOrderPerson.ActiveSkills", $"Servant name: {Servant.EnglishNamePassed}");
                        }
                    }
                    #endregion

                    #region Noble Phantasm
                    else if (GettingNoblePhantasm)
                    {
                        if (!string.IsNullOrWhiteSpace(s) && s[s.Length - 1] == '=')
                        {
                            Servant.NoblePhantasms[Servant.NoblePhantasms.Count - 1].Category = s.Replace("=", "");
                        }
                        if (s.Contains("[[File:"))
                        {
                            try
                            {
                                Servant.NoblePhantasms[Servant.NoblePhantasms.Count - 1].NoblePhantasm.IsVideo = true;
                                string VideoName = s.Replace("[[File:", "");
                                if (VideoName.Contains('|'))
                                    VideoName = VideoName.Remove(VideoName.IndexOf('|'));
                                Servant.NoblePhantasms[Servant.NoblePhantasms.Count - 1].NoblePhantasm.VideoInformation = await Video(VideoName);
                            }
                            catch (Exception e)
                            {
                                LogConsole(e, $"Looks like something failed when assigning something in fateGrandOrderPerson.NoblePhantasms[fateGrandOrderPerson.NoblePhantasms.Count - 1].NoblePhantasm.VideoInformation", $"Servant name: {Servant.EnglishNamePassed}");
                                LogFile(e, $"Looks like something failed when assigning something in fateGrandOrderPerson.NoblePhantasms[fateGrandOrderPerson.NoblePhantasms.Count - 1].NoblePhantasm.VideoInformation", $"Servant name: {Servant.EnglishNamePassed}");
                            }
                        }
                        else if (s == "|-|")
                        {
                            Servant.NoblePhantasms.Add(new NoblePhantasmList());
                        }
                        else if (s.Contains("|name"))
                        {
                            Servant.NoblePhantasms[Servant.NoblePhantasms.Count - 1].NoblePhantasm.Name = await GenericAssigning(s, "|name", PartsToReplace: new string[][] { new string[] { "<br/>", "\n" } });
                            if (s.Contains("Video"))
                            {
                                Servant.NoblePhantasms[Servant.NoblePhantasms.Count - 1].NoblePhantasm.IsVideo = true;
                                Servant.NoblePhantasms[Servant.NoblePhantasms.Count - 1].NoblePhantasm.VideoInformation = new VideoInformation();
                            }
                        }
                        else if (s.Contains("|rank"))
                        {
                            Servant.NoblePhantasms[Servant.NoblePhantasms.Count - 1].NoblePhantasm.Rank = await GenericAssigning(s, "|rank");
                        }
                        else if (s.Contains("|classification"))
                        {
                            Servant.NoblePhantasms[Servant.NoblePhantasms.Count - 1].NoblePhantasm.Classification = await GenericAssigning(s, "|classification");
                        }
                        else if (s.Contains("|type"))
                        {
                            Servant.NoblePhantasms[Servant.NoblePhantasms.Count - 1].NoblePhantasm.Type = await GenericAssigning(s, "|type");
                        }
                        else if (s.Contains("|hitcount"))
                        {
                            Servant.NoblePhantasms[Servant.NoblePhantasms.Count - 1].NoblePhantasm.HitCount = await GenericAssigning(s, "|hitcount");
                        }
                        else if (s.Contains("|effect"))
                        {
                            Servant.NoblePhantasms[Servant.NoblePhantasms.Count - 1].NoblePhantasm.Effects = await GenericArrayAssigning(s, "|effect", PartsToReplace: new string[][] { new string[] { "<br/>", "," } });
                        }
                        else if (s.Contains("|overchargeeffect"))
                        {
                            Servant.NoblePhantasms[Servant.NoblePhantasms.Count - 1].NoblePhantasm.OverChargeEffect = await GenericArrayAssigning(s, "|overchargeeffect", PartsToReplace: new string[][] { new string[] { "<br/>", "," } });
                        }
                        else if (s.Contains("|leveleffect"))
                        {
                            Servant.NoblePhantasms[Servant.NoblePhantasms.Count - 1].NoblePhantasm.LevelEffect = new LevelEffect { Name = await GenericAssigning(s, "|leveleffect") };
                        }
                        else if (s.Contains("|l1"))
                        {
                            Servant.NoblePhantasms[Servant.NoblePhantasms.Count - 1].NoblePhantasm.LevelEffect.NPLevel1 = await GenericAssigning(s, "|l1");
                        }
                        else if (s.Contains("|l2"))
                        {
                            Servant.NoblePhantasms[Servant.NoblePhantasms.Count - 1].NoblePhantasm.LevelEffect.NPLevel2 = await GenericAssigning(s, "|l2");
                        }
                        else if (s.Contains("|l3"))
                        {
                            Servant.NoblePhantasms[Servant.NoblePhantasms.Count - 1].NoblePhantasm.LevelEffect.NPLevel3 = await GenericAssigning(s, "|l3");
                        }
                        else if (s.Contains("|l4"))
                        {
                            Servant.NoblePhantasms[Servant.NoblePhantasms.Count - 1].NoblePhantasm.LevelEffect.NPLevel4 = await GenericAssigning(s, "|l4");
                        }
                        else if (s.Contains("|l5"))
                        {
                            Servant.NoblePhantasms[Servant.NoblePhantasms.Count - 1].NoblePhantasm.LevelEffect.NPLevel5 = await GenericAssigning(s, "|l5");
                        }
                        else if (s.Contains("|chargeeffect"))
                        {
                            Servant.NoblePhantasms[Servant.NoblePhantasms.Count - 1].NoblePhantasm.ChargeEffect = new ChargeEffect { Name = await GenericAssigning(s, "|chargeeffect") };
                            while (Servant.NoblePhantasms[Servant.NoblePhantasms.Count - 1].NoblePhantasm.ChargeEffect.Name.Contains("[[:"))
                            {
                                Servant.NoblePhantasms[Servant.NoblePhantasms.Count - 1].NoblePhantasm.ChargeEffect.Name = Servant.NoblePhantasms[Servant.NoblePhantasms.Count - 1].NoblePhantasm.ChargeEffect.Name.Remove(Servant.NoblePhantasms[Servant.NoblePhantasms.Count - 1].NoblePhantasm.ChargeEffect.Name.IndexOf("[[:"), Servant.NoblePhantasms[Servant.NoblePhantasms.Count - 1].NoblePhantasm.ChargeEffect.Name.IndexOf("]]") - (Servant.NoblePhantasms[Servant.NoblePhantasms.Count - 1].NoblePhantasm.ChargeEffect.Name.IndexOf("[[:") - 2));
                            }
                            Servant.NoblePhantasms[Servant.NoblePhantasms.Count - 1].NoblePhantasm.ChargeEffect.Name = Servant.NoblePhantasms[Servant.NoblePhantasms.Count - 1].NoblePhantasm.ChargeEffect.Name.Replace("  ", " ");
                        }
                        else if (s.Contains("|c1"))
                        {
                            Servant.NoblePhantasms[Servant.NoblePhantasms.Count - 1].NoblePhantasm.ChargeEffect.NPLevel1 = await GenericAssigning(s, "|c1");
                        }
                        else if (s.Contains("|c2"))
                        {
                            Servant.NoblePhantasms[Servant.NoblePhantasms.Count - 1].NoblePhantasm.ChargeEffect.NPLevel2 = await GenericAssigning(s, "|c2");
                        }
                        else if (s.Contains("|c3"))
                        {
                            Servant.NoblePhantasms[Servant.NoblePhantasms.Count - 1].NoblePhantasm.ChargeEffect.NPLevel3 = await GenericAssigning(s, "|c3");
                        }
                        else if (s.Contains("|c4"))
                        {
                            Servant.NoblePhantasms[Servant.NoblePhantasms.Count - 1].NoblePhantasm.ChargeEffect.NPLevel4 = await GenericAssigning(s, "|c4");
                        }
                        else if (s.Contains("|c5"))
                        {
                            Servant.NoblePhantasms[Servant.NoblePhantasms.Count - 1].NoblePhantasm.ChargeEffect.NPLevel5 = await GenericAssigning(s, "|c5");
                        }
                    }
                    #endregion

                    #region Ascension
                    else if (GettingAscension)
                    {
                        if (s.Length >= 3 && s.Remove(3) == "|11")
                        {
                            Servant.Ascension.Ascension1 = await Item(null, null, null, true, "1", Servant.Ascension.Ascension1);
                            Servant.Ascension.Ascension1.Item1 = await Item(s, Servant.Ascension.Ascension1.Item1, "11");
                        }
                        else if (s.Length >= 3 && s.Remove(3) == "|12")
                        {
                            Servant.Ascension.Ascension1.Item2 = await Item(s, Servant.Ascension.Ascension1.Item2, "12");
                        }
                        else if (s.Length >= 3 && s.Remove(3) == "|13")
                        {
                            Servant.Ascension.Ascension1.Item3 = await Item(s, Servant.Ascension.Ascension1.Item3, "13");
                        }
                        else if (s.Length >= 3 && s.Remove(3) == "|14")
                        {
                            Servant.Ascension.Ascension1.Item4 = await Item(s, Servant.Ascension.Ascension1.Item4, "14");
                        }
                        else if (s.Length >= 3 && s.Remove(3) == "|21")
                        {
                            Servant.Ascension.Ascension2 = await Item(null, null, null, true, "2", Servant.Ascension.Ascension2);
                            Servant.Ascension.Ascension2.Item1 = await Item(s, Servant.Ascension.Ascension2.Item2, "21");
                        }
                        else if (s.Length >= 3 && s.Remove(3) == "|22")
                        {
                            Servant.Ascension.Ascension2.Item2 = await Item(s, Servant.Ascension.Ascension2.Item2, "22");
                        }
                        else if (s.Length >= 3 && s.Remove(3) == "|23")
                        {
                            Servant.Ascension.Ascension2.Item3 = await Item(s, Servant.Ascension.Ascension2.Item3, "23");
                        }
                        else if (s.Length >= 3 && s.Remove(3) == "|24")
                        {
                            Servant.Ascension.Ascension2.Item4 = await Item(s, Servant.Ascension.Ascension2.Item4, "24");
                        }
                        else if (s.Length >= 3 && s.Remove(3) == "|31")
                        {
                            Servant.Ascension.Ascension3 = await Item(null, null, null, true, "3", Servant.Ascension.Ascension3);
                            Servant.Ascension.Ascension3.Item1 = await Item(s, Servant.Ascension.Ascension3.Item2, "31");
                        }
                        else if (s.Length >= 3 && s.Remove(3) == "|32")
                        {
                            Servant.Ascension.Ascension3.Item2 = await Item(s, Servant.Ascension.Ascension3.Item2, "32");
                        }
                        else if (s.Length >= 3 && s.Remove(3) == "|33")
                        {
                            Servant.Ascension.Ascension3.Item3 = await Item(s, Servant.Ascension.Ascension3.Item3, "33");
                        }
                        else if (s.Length >= 3 && s.Remove(3) == "|34")
                        {
                            Servant.Ascension.Ascension3.Item4 = await Item(s, Servant.Ascension.Ascension3.Item4, "34");
                        }
                        else if (s.Length >= 3 && s.Remove(3) == "|41")
                        {
                            Servant.Ascension.Ascension4 = await Item(null, null, null, true, "4", Servant.Ascension.Ascension4);
                            Servant.Ascension.Ascension4.Item1 = await Item(s, Servant.Ascension.Ascension4.Item2, "41");
                        }
                        else if (s.Length >= 3 && s.Remove(3) == "|42")
                        {
                            Servant.Ascension.Ascension4.Item2 = await Item(s, Servant.Ascension.Ascension4.Item2, "42");
                        }
                        else if (s.Length >= 3 && s.Remove(3) == "|43")
                        {
                            Servant.Ascension.Ascension4.Item3 = await Item(s, Servant.Ascension.Ascension4.Item3, "43");
                        }
                        else if (s.Length >= 3 && s.Remove(3) == "|44")
                        {
                            Servant.Ascension.Ascension4.Item4 = await Item(s, Servant.Ascension.Ascension4.Item4, "44");
                        }
                        else if (s.Length >= 3 && s.Remove(3) == "|51")
                        {
                            Servant.Ascension.Ascension5 = await Item(null, null, null, true, "5", Servant.Ascension.Ascension5);
                            Servant.Ascension.Ascension5.Item1 = await Item(s, Servant.Ascension.Ascension5.Item2, "51");
                        }
                        else if (s.Length >= 3 && s.Remove(3) == "|52")
                        {
                            Servant.Ascension.Ascension5.Item2 = await Item(s, Servant.Ascension.Ascension5.Item2, "52");
                        }
                        else if (s.Length >= 3 && s.Remove(3) == "|53")
                        {
                            Servant.Ascension.Ascension5.Item3 = await Item(s, Servant.Ascension.Ascension5.Item3, "53");
                        }
                        else if (s.Length >= 3 && s.Remove(3) == "|54")
                        {
                            Servant.Ascension.Ascension5.Item4 = await Item(s, Servant.Ascension.Ascension5.Item4, "54");
                        }
                        else if (s.Contains("|1qp"))
                        {
                            Servant.Ascension.Ascension1.QP = await GenericAssigning(s, "|1qp", new string[] { "{{Inum|{{QP}}|", "}}" });
                        }
                        else if (s.Contains("|2qp"))
                        {
                            Servant.Ascension.Ascension2.QP = await GenericAssigning(s, "|2qp", new string[] { "{{Inum|{{QP}}|", "}}" });
                        }
                        else if (s.Contains("|3qp"))
                        {
                            Servant.Ascension.Ascension3.QP = await GenericAssigning(s, "|3qp", new string[] { "{{Inum|{{QP}}|", "}}" });
                        }
                        else if (s.Contains("|4qp"))
                        {
                            Servant.Ascension.Ascension4.QP = await GenericAssigning(s, "|4qp", new string[] { "{{Inum|{{QP}}|", "}}" });
                        }
                        else if (s.Contains("|5qp"))
                        {
                            Servant.Ascension.Ascension5.QP = await GenericAssigning(s, "|5qp", new string[] { "{{Inum|{{QP}}|", "}}" });
                        }
                    }
                    #endregion

                    #region Skill Reinforcement
                    else if (GettingSkillReinforcement)
                    {
                        if (s.Length >= 3 && s.Remove(3) == "|11")
                        {
                            Servant.SkillReinforcement.Ascension1 = await Item(null, null, null, true, "1", Servant.SkillReinforcement.Ascension1);
                            Servant.SkillReinforcement.Ascension1.Item1 = await Item(s, Servant.SkillReinforcement.Ascension1.Item2, "11");
                        }
                        else if (s.Length >= 3 && s.Remove(3) == "|12")
                        {
                            Servant.SkillReinforcement.Ascension1.Item2 = await Item(s, Servant.SkillReinforcement.Ascension1.Item2, "12");
                        }
                        else if (s.Length >= 3 && s.Remove(3) == "|13")
                        {
                            Servant.SkillReinforcement.Ascension1.Item3 = await Item(s, Servant.SkillReinforcement.Ascension1.Item3, "13");
                        }
                        else if (s.Length >= 3 && s.Remove(3) == "|14")
                        {
                            Servant.SkillReinforcement.Ascension1.Item4 = await Item(s, Servant.SkillReinforcement.Ascension1.Item4, "14");
                        }
                        else if (s.Length >= 3 && s.Remove(3) == "|21")
                        {
                            Servant.SkillReinforcement.Ascension2 = await Item(null, null, null, true, "2", Servant.SkillReinforcement.Ascension2);
                            Servant.SkillReinforcement.Ascension2.Item1 = await Item(s, Servant.SkillReinforcement.Ascension2.Item2, "21");
                        }
                        else if (s.Length >= 3 && s.Remove(3) == "|22")
                        {
                            Servant.SkillReinforcement.Ascension2.Item2 = await Item(s, Servant.SkillReinforcement.Ascension2.Item2, "22");
                        }
                        else if (s.Length >= 3 && s.Remove(3) == "|23")
                        {
                            Servant.SkillReinforcement.Ascension2.Item3 = await Item(s, Servant.SkillReinforcement.Ascension2.Item3, "23");
                        }
                        else if (s.Length >= 3 && s.Remove(3) == "|24")
                        {
                            Servant.SkillReinforcement.Ascension2.Item4 = await Item(s, Servant.SkillReinforcement.Ascension2.Item4, "24");
                        }
                        else if (s.Length >= 3 && s.Remove(3) == "|31")
                        {
                            Servant.SkillReinforcement.Ascension3 = await Item(null, null, null, true, "3", Servant.SkillReinforcement.Ascension3);
                            Servant.SkillReinforcement.Ascension3.Item1 = await Item(s, Servant.SkillReinforcement.Ascension3.Item2, "31");
                        }
                        else if (s.Length >= 3 && s.Remove(3) == "|32")
                        {
                            Servant.SkillReinforcement.Ascension3.Item2 = await Item(s, Servant.SkillReinforcement.Ascension3.Item2, "32");
                        }
                        else if (s.Length >= 3 && s.Remove(3) == "|33")
                        {
                            Servant.SkillReinforcement.Ascension3.Item3 = await Item(s, Servant.SkillReinforcement.Ascension3.Item3, "33");
                        }
                        else if (s.Length >= 3 && s.Remove(3) == "|34")
                        {
                            Servant.SkillReinforcement.Ascension3.Item4 = await Item(s, Servant.SkillReinforcement.Ascension3.Item4, "34");
                        }
                        else if (s.Length >= 3 && s.Remove(3) == "|41")
                        {
                            Servant.SkillReinforcement.Ascension4 = await Item(null, null, null, true, "4", Servant.SkillReinforcement.Ascension4);
                            Servant.SkillReinforcement.Ascension4.Item1 = await Item(s, Servant.SkillReinforcement.Ascension4.Item2, "41");
                        }
                        else if (s.Length >= 3 && s.Remove(3) == "|42")
                        {
                            Servant.SkillReinforcement.Ascension4.Item2 = await Item(s, Servant.SkillReinforcement.Ascension4.Item2, "42");
                        }
                        else if (s.Length >= 3 && s.Remove(3) == "|43")
                        {
                            Servant.SkillReinforcement.Ascension4.Item3 = await Item(s, Servant.SkillReinforcement.Ascension4.Item3, "43");
                        }
                        else if (s.Length >= 3 && s.Remove(3) == "|44")
                        {
                            Servant.SkillReinforcement.Ascension4.Item4 = await Item(s, Servant.SkillReinforcement.Ascension4.Item4, "44");
                        }
                        else if (s.Length >= 3 && s.Remove(3) == "|51")
                        {
                            Servant.SkillReinforcement.Ascension5 = await Item(null, null, null, true, "5", Servant.SkillReinforcement.Ascension5);
                            Servant.SkillReinforcement.Ascension5.Item1 = await Item(s, Servant.SkillReinforcement.Ascension5.Item2, "51");
                        }
                        else if (s.Length >= 3 && s.Remove(3) == "|52")
                        {
                            Servant.SkillReinforcement.Ascension5.Item2 = await Item(s, Servant.SkillReinforcement.Ascension5.Item2, "52");
                        }
                        else if (s.Length >= 3 && s.Remove(3) == "|53")
                        {
                            Servant.SkillReinforcement.Ascension5.Item3 = await Item(s, Servant.SkillReinforcement.Ascension5.Item3, "53");
                        }
                        else if (s.Length >= 3 && s.Remove(3) == "|54")
                        {
                            Servant.SkillReinforcement.Ascension5.Item4 = await Item(s, Servant.SkillReinforcement.Ascension5.Item4, "54");
                        }
                        else if (s.Length >= 3 && s.Remove(3) == "|61")
                        {
                            Servant.SkillReinforcement.Ascension6 = await Item(null, null, null, true, "6", Servant.SkillReinforcement.Ascension6);
                            Servant.SkillReinforcement.Ascension6.Item1 = await Item(s, Servant.SkillReinforcement.Ascension6.Item2, "61");
                        }
                        else if (s.Length >= 3 && s.Remove(3) == "|62")
                        {
                            Servant.SkillReinforcement.Ascension6.Item2 = await Item(s, Servant.SkillReinforcement.Ascension6.Item2, "62");
                        }
                        else if (s.Length >= 3 && s.Remove(3) == "|63")
                        {
                            Servant.SkillReinforcement.Ascension6.Item3 = await Item(s, Servant.SkillReinforcement.Ascension6.Item3, "63");
                        }
                        else if (s.Length >= 3 && s.Remove(3) == "|64")
                        {
                            Servant.SkillReinforcement.Ascension6.Item4 = await Item(s, Servant.SkillReinforcement.Ascension6.Item4, "64");
                        }
                        else if (s.Length >= 3 && s.Remove(3) == "|71")
                        {
                            Servant.SkillReinforcement.Ascension7 = await Item(null, null, null, true, "7", Servant.SkillReinforcement.Ascension7);
                            Servant.SkillReinforcement.Ascension7.Item1 = await Item(s, Servant.SkillReinforcement.Ascension7.Item2, "71");
                        }
                        else if (s.Length >= 3 && s.Remove(3) == "|72")
                        {
                            Servant.SkillReinforcement.Ascension7.Item2 = await Item(s, Servant.SkillReinforcement.Ascension7.Item2, "72");
                        }
                        else if (s.Length >= 3 && s.Remove(3) == "|73")
                        {
                            Servant.SkillReinforcement.Ascension7.Item3 = await Item(s, Servant.SkillReinforcement.Ascension7.Item3, "73");
                        }
                        else if (s.Length >= 3 && s.Remove(3) == "|74")
                        {
                            Servant.SkillReinforcement.Ascension7.Item4 = await Item(s, Servant.SkillReinforcement.Ascension7.Item4, "74");
                        }
                        else if (s.Length >= 3 && s.Remove(3) == "|81")
                        {
                            Servant.SkillReinforcement.Ascension8 = await Item(null, null, null, true, "8", Servant.SkillReinforcement.Ascension8);
                            Servant.SkillReinforcement.Ascension8.Item1 = await Item(s, Servant.SkillReinforcement.Ascension8.Item2, "82");
                        }
                        else if (s.Length >= 3 && s.Remove(3) == "|82")
                        {
                            Servant.SkillReinforcement.Ascension8.Item2 = await Item(s, Servant.SkillReinforcement.Ascension8.Item2, "82");
                        }
                        else if (s.Length >= 3 && s.Remove(3) == "|83")
                        {
                            Servant.SkillReinforcement.Ascension8.Item3 = await Item(s, Servant.SkillReinforcement.Ascension8.Item3, "83");
                        }
                        else if (s.Length >= 3 && s.Remove(3) == "|84")
                        {
                            Servant.SkillReinforcement.Ascension8.Item4 = await Item(s, Servant.SkillReinforcement.Ascension8.Item4, "84");
                        }
                        else if (s.Length >= 3 && s.Remove(3) == "|91")
                        {
                            Servant.SkillReinforcement.Ascension9 = await Item(null, null, null, true, "9", Servant.SkillReinforcement.Ascension9);
                            Servant.SkillReinforcement.Ascension9.Item1 = await Item(s, Servant.SkillReinforcement.Ascension9.Item2, "92");
                        }
                        else if (s.Length >= 3 && s.Remove(3) == "|92")
                        {
                            Servant.SkillReinforcement.Ascension9.Item2 = await Item(s, Servant.SkillReinforcement.Ascension9.Item2, "92");
                        }
                        else if (s.Length >= 3 && s.Remove(3) == "|93")
                        {
                            Servant.SkillReinforcement.Ascension9.Item3 = await Item(s, Servant.SkillReinforcement.Ascension9.Item3, "93");
                        }
                        else if (s.Length >= 3 && s.Remove(3) == "|94")
                        {
                            Servant.SkillReinforcement.Ascension9.Item4 = await Item(s, Servant.SkillReinforcement.Ascension9.Item4, "94");
                        }
                        else if (s.Contains("|1qp"))
                        {
                            Servant.SkillReinforcement.Ascension1.QP = await GenericAssigning(s, "|1qp", new string[] { "{{Inum|{{QP}}|", "}}" });
                        }
                        else if (s.Contains("|2qp"))
                        {
                            Servant.SkillReinforcement.Ascension2.QP = await GenericAssigning(s, "|2qp", new string[] { "{{Inum|{{QP}}|", "}}" });
                        }
                        else if (s.Contains("|3qp"))
                        {
                            Servant.SkillReinforcement.Ascension3.QP = await GenericAssigning(s, "|3qp", new string[] { "{{Inum|{{QP}}|", "}}" });
                        }
                        else if (s.Contains("|4qp"))
                        {
                            Servant.SkillReinforcement.Ascension4.QP = await GenericAssigning(s, "|4qp", new string[] { "{{Inum|{{QP}}|", "}}" });
                        }
                        else if (s.Contains("|5qp"))
                        {
                            Servant.SkillReinforcement.Ascension5.QP = await GenericAssigning(s, "|5qp", new string[] { "{{Inum|{{QP}}|", "}}" });
                        }
                        else if (s.Contains("|6qp"))
                        {
                            Servant.SkillReinforcement.Ascension6.QP = await GenericAssigning(s, "|6qp", new string[] { "{{Inum|{{QP}}|", "}}" });
                        }
                        else if (s.Contains("|7qp"))
                        {
                            Servant.SkillReinforcement.Ascension7.QP = await GenericAssigning(s, "|7qp", new string[] { "{{Inum|{{QP}}|", "}}" });
                        }
                        else if (s.Contains("|8qp"))
                        {
                            Servant.SkillReinforcement.Ascension8.QP = await GenericAssigning(s, "|8qp", new string[] { "{{Inum|{{QP}}|", "}}" });
                        }
                        else if (s.Contains("|9qp"))
                        {
                            Servant.SkillReinforcement.Ascension9.QP = await GenericAssigning(s, "|9qp", new string[] { "{{Inum|{{QP}}|", "}}" });
                        }
                    }
                    #endregion

                    #region Stats
                    else if (GettingStats)
                    {
                        if (s.Contains("|strength"))
                        {
                            Servant.Stats.Strength.Grade = await GenericAssigning(s, "|strength");
                        }
                        else if (s.Contains("|stbar"))
                        {
                            Servant.Stats.Strength.BarNumber = await GenericAssigning(s, "|stbar");
                        }
                        else if (s.Contains("|endurance"))
                        {
                            Servant.Stats.Endurance.Grade = await GenericAssigning(s, "|endurance");
                        }
                        else if (s.Contains("|enbar"))
                        {
                            Servant.Stats.Endurance.BarNumber = await GenericAssigning(s, "|enbar");
                        }
                        else if (s.Contains("|agility"))
                        {
                            Servant.Stats.Agility.Grade = await GenericAssigning(s, "|agility");
                        }
                        else if (s.Contains("|agbar"))
                        {
                            Servant.Stats.Agility.BarNumber = await GenericAssigning(s, "|agbar");
                        }
                        else if (s.Contains("|mana"))
                        {
                            Servant.Stats.Mana.Grade = await GenericAssigning(s, "|mana");
                        }
                        else if (s.Contains("|mabar"))
                        {
                            Servant.Stats.Mana.BarNumber = await GenericAssigning(s, "|mabar");
                        }
                        else if (s.Contains("|luck"))
                        {
                            Servant.Stats.Luck.Grade = await GenericAssigning(s, "|luck");
                        }
                        else if (s.Contains("|lubar"))
                        {
                            Servant.Stats.Luck.BarNumber = await GenericAssigning(s, "|lubar");
                        }
                        else if (s.Contains("|np") && !s.Contains("|npbar"))
                        {
                            Servant.Stats.NP.Grade = await GenericAssigning(s, "|np");
                        }
                        else if (s.Contains("|npbar"))
                        {
                            Servant.Stats.NP.BarNumber = await GenericAssigning(s, "|npbar");
                        }
                    }
                    #endregion

                    #region Bond Level
                    else if (GettingBondLevel)
                    {
                        if (s.Contains("|b1") && !s.Contains("|b10"))
                        {
                            Servant.BondLevels.BondLevel1.BondRequired = await GenericAssigning(s, "|b1");
                        }
                        else if (s.Contains("|b2"))
                        {
                            Servant.BondLevels.BondLevel2.BondRequired = await GenericAssigning(s, "|b2");
                        }
                        else if (s.Contains("|b3"))
                        {
                            Servant.BondLevels.BondLevel3.BondRequired = await GenericAssigning(s, "|b3");
                        }
                        else if (s.Contains("|b4"))
                        {
                            Servant.BondLevels.BondLevel4.BondRequired = await GenericAssigning(s, "|b4");
                        }
                        else if (s.Contains("|b5"))
                        {
                            Servant.BondLevels.BondLevel5.BondRequired = await GenericAssigning(s, "|b5");
                        }
                        else if (s.Contains("|b6"))
                        {
                            Servant.BondLevels.BondLevel6.BondRequired = await GenericAssigning(s, "|b6");
                        }
                        else if (s.Contains("|b7"))
                        {
                            Servant.BondLevels.BondLevel7.BondRequired = await GenericAssigning(s, "|b7");
                        }
                        else if (s.Contains("|b8"))
                        {
                            Servant.BondLevels.BondLevel8.BondRequired = await GenericAssigning(s, "|b8");
                        }
                        else if (s.Contains("|b9"))
                        {
                            Servant.BondLevels.BondLevel9.BondRequired = await GenericAssigning(s, "|b9");
                        }
                        else if (s.Contains("|b10"))
                        {
                            Servant.BondLevels.BondLevel10.BondRequired = await GenericAssigning(s, "|b10");
                        }
                        if (s.Contains("|2b1") && !s.Contains("|2b10"))
                        {
                            Servant.BondLevels.BondLevel1.TotalBond = await GenericAssigning(s, "|2b1");
                        }
                        else if (s.Contains("|2b2"))
                        {
                            Servant.BondLevels.BondLevel2.TotalBond = await GenericAssigning(s, "|2b2");
                        }
                        else if (s.Contains("|2b3"))
                        {
                            Servant.BondLevels.BondLevel3.TotalBond = await GenericAssigning(s, "|2b3");
                        }
                        else if (s.Contains("|2b4"))
                        {
                            Servant.BondLevels.BondLevel4.TotalBond = await GenericAssigning(s, "|2b4");
                        }
                        else if (s.Contains("|2b5"))
                        {
                            Servant.BondLevels.BondLevel5.TotalBond = await GenericAssigning(s, "|2b5");
                        }
                        else if (s.Contains("|2b6"))
                        {
                            Servant.BondLevels.BondLevel6.TotalBond = await GenericAssigning(s, "|2b6");
                        }
                        else if (s.Contains("|2b7"))
                        {
                            Servant.BondLevels.BondLevel7.TotalBond = await GenericAssigning(s, "|2b7");
                        }
                        else if (s.Contains("|2b8"))
                        {
                            Servant.BondLevels.BondLevel8.TotalBond = await GenericAssigning(s, "|2b8");
                        }
                        else if (s.Contains("|2b9"))
                        {
                            Servant.BondLevels.BondLevel9.TotalBond = await GenericAssigning(s, "|2b9");
                        }
                        else if (s.Contains("|2b10"))
                        {
                            Servant.BondLevels.BondLevel10.TotalBond = await GenericAssigning(s, "|2b10");
                        }
                        else if (s.Contains("|image"))
                        {
                            Servant.BondLevels.Bond10Reward.Image = await Image(s, "|image");
                        }
                        else if (s.Contains("|effect") | GettingBond10)
                        {
                            if (s == "}}" && GettingBond10)
                            {
                                GettingBond10 = false;
                            }
                            else
                            {
                                GettingBond10 = true;
                                //'''[[SomeTextLol]]''' == Effect
                                Servant.BondLevels.Bond10Reward.Effect += (await GenericAssigning(s, "|effect", PartsToReplace: new string[][] { new string[] { "<br/>", " " }, new string[] { ".", ". " }, new string[] { "  ", " " } }, OtherPartsToRemove: new string[] { "]]", "[[", "'''" })).Replace(",", ", ");
                            }
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
                            while (Servant.Biography.Default.JapaneseText.Contains("<!--"))
                            {
                                Servant.Biography.Default.JapaneseText = Servant.Biography.Default.JapaneseText.Remove(Servant.Biography.Default.JapaneseText.IndexOf("<!--"), Servant.Biography.Default.JapaneseText.IndexOf("-->") - (Servant.Biography.Default.JapaneseText.IndexOf("<!--") - 3));
                            }
                        }
                        else if (s.Contains("|jb1"))
                        {
                            GettingDefaultBio = false;
                            GettingBond1BioJap = true;
                            while (Servant.Biography.Default.EnglishText.Contains("<!--"))
                            {
                                Servant.Biography.Default.EnglishText = Servant.Biography.Default.EnglishText.Remove(Servant.Biography.Default.EnglishText.IndexOf("<!--"), Servant.Biography.Default.EnglishText.IndexOf("-->") - (Servant.Biography.Default.EnglishText.IndexOf("<!--") - 3));
                            }
                        }
                        else if (s.Contains("|b1"))
                        {
                            GettingBond1BioJap = false;
                            GettingBond1Bio = true;
                            while (Servant.Biography.Bond1.JapaneseText.Contains("<!--"))
                            {
                                Servant.Biography.Bond1.JapaneseText = Servant.Biography.Bond1.JapaneseText.Remove(Servant.Biography.Bond1.JapaneseText.IndexOf("<!--"), Servant.Biography.Bond1.JapaneseText.IndexOf("-->") - (Servant.Biography.Bond1.JapaneseText.IndexOf("<!--") - 3));
                            }
                        }
                        else if (s.Contains("|jb2"))
                        {
                            GettingBond1Bio = false;
                            GettingBond2BioJap = true;
                            while (Servant.Biography.Bond1.EnglishText.Contains("<!--"))
                            {
                                Servant.Biography.Bond1.EnglishText = Servant.Biography.Bond1.EnglishText.Remove(Servant.Biography.Bond1.EnglishText.IndexOf("<!--"), Servant.Biography.Bond1.EnglishText.IndexOf("-->") - (Servant.Biography.Bond1.EnglishText.IndexOf("<!--") - 3));
                            }
                        }
                        else if (s.Contains("|b2"))
                        {
                            GettingBond2BioJap = false;
                            GettingBond2Bio = true;
                            while (Servant.Biography.Bond2.JapaneseText.Contains("<!--"))
                            {
                                Servant.Biography.Bond2.JapaneseText = Servant.Biography.Bond2.JapaneseText.Remove(Servant.Biography.Bond2.JapaneseText.IndexOf("<!--"), Servant.Biography.Bond2.JapaneseText.IndexOf("-->") - (Servant.Biography.Bond2.JapaneseText.IndexOf("<!--") - 3));
                            }
                        }
                        else if (s.Contains("|jb3"))
                        {
                            GettingBond2Bio = false;
                            GettingBond3BioJap = true;
                            while (Servant.Biography.Bond2.EnglishText.Contains("<!--"))
                            {
                                Servant.Biography.Bond2.EnglishText = Servant.Biography.Bond2.EnglishText.Remove(Servant.Biography.Bond2.EnglishText.IndexOf("<!--"), Servant.Biography.Bond2.EnglishText.IndexOf("-->") - (Servant.Biography.Bond2.EnglishText.IndexOf("<!--") - 3));
                            }
                        }
                        else if (s.Contains("|b3"))
                        {
                            GettingBond3BioJap = false;
                            GettingBond3Bio = true;
                            while (Servant.Biography.Bond3.JapaneseText.Contains("<!--"))
                            {
                                Servant.Biography.Bond3.JapaneseText = Servant.Biography.Bond3.JapaneseText.Remove(Servant.Biography.Bond3.JapaneseText.IndexOf("<!--"), Servant.Biography.Bond3.JapaneseText.IndexOf("-->") - (Servant.Biography.Bond3.JapaneseText.IndexOf("<!--") - 3));
                            }
                        }
                        else if (s.Contains("|jb4"))
                        {
                            GettingBond3Bio = false;
                            GettingBond4BioJap = true;
                            while (Servant.Biography.Bond3.EnglishText.Contains("<!--"))
                            {
                                Servant.Biography.Bond3.EnglishText = Servant.Biography.Bond3.EnglishText.Remove(Servant.Biography.Bond3.EnglishText.IndexOf("<!--"), Servant.Biography.Bond3.EnglishText.IndexOf("-->") - (Servant.Biography.Bond3.EnglishText.IndexOf("<!--") - 3));
                            }
                        }
                        else if (s.Contains("|b4"))
                        {
                            GettingBond4BioJap = false;
                            GettingBond4Bio = true;
                            while (Servant.Biography.Bond4.JapaneseText.Contains("<!--"))
                            {
                                Servant.Biography.Bond4.JapaneseText = Servant.Biography.Bond4.JapaneseText.Remove(Servant.Biography.Bond4.JapaneseText.IndexOf("<!--"), Servant.Biography.Bond4.JapaneseText.IndexOf("-->") - (Servant.Biography.Bond4.JapaneseText.IndexOf("<!--") - 3));
                            }
                        }
                        else if (s.Contains("|jb5"))
                        {
                            GettingBond4Bio = false;
                            GettingBond5BioJap = true;
                            while (Servant.Biography.Bond4.EnglishText.Contains("<!--"))
                            {
                                Servant.Biography.Bond4.EnglishText = Servant.Biography.Bond4.EnglishText.Remove(Servant.Biography.Bond4.EnglishText.IndexOf("<!--"), Servant.Biography.Bond4.EnglishText.IndexOf("-->") - (Servant.Biography.Bond4.EnglishText.IndexOf("<!--") - 3));
                            }
                        }
                        else if (s.Contains("|b5"))
                        {
                            GettingBond5BioJap = false;
                            GettingBond5Bio = true;
                            while (Servant.Biography.Bond5.JapaneseText.Contains("<!--"))
                            {
                                Servant.Biography.Bond5.JapaneseText = Servant.Biography.Bond5.JapaneseText.Remove(Servant.Biography.Bond5.JapaneseText.IndexOf("<!--"), Servant.Biography.Bond5.JapaneseText.IndexOf("-->") - (Servant.Biography.Bond5.JapaneseText.IndexOf("<!--") - 3));
                            }
                        }
                        else if (s.Contains("|jex"))
                        {
                            GettingBond5Bio = false;
                            GettingExtraBioJap = true;
                            while (Servant.Biography.Bond5.EnglishText.Contains("<!--"))
                            {
                                Servant.Biography.Bond5.EnglishText = Servant.Biography.Bond5.EnglishText.Remove(Servant.Biography.Bond5.EnglishText.IndexOf("<!--"), Servant.Biography.Bond5.EnglishText.IndexOf("-->") - (Servant.Biography.Bond5.EnglishText.IndexOf("<!--") - 3));
                            }
                        }
                        else if (s.Contains("|ex"))
                        {
                            GettingExtraBioJap = false;
                            GettingExtraBio = true;
                            while (Servant.Biography.Extra.JapaneseText.Contains("<!--"))
                            {
                                Servant.Biography.Extra.JapaneseText = Servant.Biography.Extra.JapaneseText.Remove(Servant.Biography.Extra.JapaneseText.IndexOf("<!--"), Servant.Biography.Extra.JapaneseText.IndexOf("-->") - (Servant.Biography.Extra.JapaneseText.IndexOf("<!--") - 3));
                            }
                        }
                        else if (GettingDefaultBioJap)
                        {
                            Servant.Biography.Default.JapaneseText += await GenericAssigning(s, "", OtherPartsToRemove: new string[] { "'''", "―――", "---", "[[", "]]", "''" }, PartsToReplace: new string[][] { new string[] { "<br/>", "/r/n" } });
                        }
                        else if (GettingDefaultBio)
                        {
                            Servant.Biography.Default.EnglishText += await GenericAssigning(s, "", OtherPartsToRemove: new string[] { "'''", "―――", "---", "[[", "]]", "''" }, PartsToReplace: new string[][] { new string[] { "<br/>", "/r/n" } });
                        }
                        else if (GettingBond1BioJap)
                        {
                            Servant.Biography.Bond1.JapaneseText += await GenericAssigning(s, "", OtherPartsToRemove: new string[] { "'''", "―――", "---", "[[", "]]", "''" }, PartsToReplace: new string[][] { new string[] { "<br/>", "/r/n" } });
                        }
                        else if (GettingBond1Bio)
                        {
                            Servant.Biography.Bond1.EnglishText += await GenericAssigning(s, "", OtherPartsToRemove: new string[] { "'''", "―――", "---", "[[", "]]", "''" }, PartsToReplace: new string[][] { new string[] { "<br/>", "/r/n" } });
                        }
                        else if (GettingBond2BioJap)
                        {
                            Servant.Biography.Bond2.JapaneseText += await GenericAssigning(s, "", OtherPartsToRemove: new string[] { "'''", "―――", "---", "[[", "]]", "''" }, PartsToReplace: new string[][] { new string[] { "<br/>", "/r/n" } });
                        }
                        else if (GettingBond2Bio)
                        {
                            Servant.Biography.Bond2.EnglishText += await GenericAssigning(s, "", OtherPartsToRemove: new string[] { "'''", "―――", "---", "[[", "]]", "''" }, PartsToReplace: new string[][] { new string[] { "<br/>", "/r/n" } });
                        }
                        else if (GettingBond3BioJap)
                        {
                            Servant.Biography.Bond3.JapaneseText += await GenericAssigning(s, "", OtherPartsToRemove: new string[] { "'''", "―――", "---", "[[", "]]", "''" }, PartsToReplace: new string[][] { new string[] { "<br/>", "/r/n" } });
                        }
                        else if (GettingBond3Bio)
                        {
                            Servant.Biography.Bond3.EnglishText += await GenericAssigning(s, "", OtherPartsToRemove: new string[] { "'''", "―――", "---", "[[", "]]", "''" }, PartsToReplace: new string[][] { new string[] { "<br/>", "/r/n" } });
                        }
                        else if (GettingBond4BioJap)
                        {
                            Servant.Biography.Bond4.JapaneseText += await GenericAssigning(s, "", OtherPartsToRemove: new string[] { "'''", "―――", "---", "[[", "]]", "''" }, PartsToReplace: new string[][] { new string[] { "<br/>", "/r/n" } });
                        }
                        else if (GettingBond4Bio)
                        {
                            Servant.Biography.Bond4.EnglishText += await GenericAssigning(s, "", OtherPartsToRemove: new string[] { "'''", "―――", "---", "[[", "]]", "''" }, PartsToReplace: new string[][] { new string[] { "<br/>", "/r/n" } });
                        }
                        else if (GettingBond5BioJap)
                        {
                            Servant.Biography.Bond5.JapaneseText += await GenericAssigning(s, "", OtherPartsToRemove: new string[] { "'''", "―――", "---", "[[", "]]", "''" }, PartsToReplace: new string[][] { new string[] { "<br/>", "/r/n" } });
                        }
                        else if (GettingBond5Bio)
                        {
                            Servant.Biography.Bond5.EnglishText += await GenericAssigning(s, "", OtherPartsToRemove: new string[] { "'''", "―――", "---", "[[", "]]", "''" }, PartsToReplace: new string[][] { new string[] { "<br/>", "/r/n" } });
                        }
                        else if (GettingExtraBioJap)
                        {
                            Servant.Biography.Extra.JapaneseText += await GenericAssigning(s, "", OtherPartsToRemove: new string[] { "'''", "―――", "---", "[[", "]]", "''", "}}" }, PartsToReplace: new string[][] { new string[] { "<br/>", "/r/n" } });
                        }
                        else if (GettingExtraBio)
                        {
                            Servant.Biography.Extra.EnglishText += await GenericAssigning(s, "", OtherPartsToRemove: new string[] { "'''", "―――", "---", "[[", "]]", "''", "}}" }, PartsToReplace: new string[][] { new string[] { "<br/>", "/r/n" } });
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
                            if (Servant.Availability == null)
                                Servant.Availability = new string[] { ToAdd };
                            else
                                Servant.Availability = new string[] { $"{Servant.Availability[0]}\\{ToAdd}" };
                            ToAdd = null;
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
                            ToAdd = ToAdd.Replace("'''", "");
                            if (Servant.Trivia == null)
                                Servant.Trivia = new string[] { ToAdd };
                            else
                                Servant.Trivia = new string[] { $"{Servant.Trivia[0]}\\{ToAdd}" };
                            ToAdd = null;
                        }
                    }
                    #endregion

                    #region Basic Information
                    else if (GettingBasicInformation)
                    {
                        if (s.Contains("|jname"))
                        {
                            Servant.BasicInformation.JapaneseName = await GenericAssigning(s, "|jname");
                        }
                        else if (s.Contains("|voicea"))
                        {
                            Servant.BasicInformation.VoiceActor = await GenericAssigning(s, "|voicea");
                        }
                        else if (s.Contains("|illus"))
                        {
                            Servant.BasicInformation.Illustrator = await GenericAssigning(s, "|illus");
                        }
                        else if (s.Contains("|class"))
                        {
                            Servant.BasicInformation.Class = await GenericAssigning(s, "|class");
                        }
                        else if (s.Contains("|atk"))
                        {
                            Servant.BasicInformation.ATK = await GenericAssigning(s, "|atk");
                        }
                        else if (s.Contains("|hp"))
                        {
                            Servant.BasicInformation.HP = await GenericAssigning(s, "|hp");
                        }
                        else if (s.Contains("|gatk"))
                        {
                            Servant.BasicInformation.GrailATK = await GenericAssigning(s, "|gatk");
                        }
                        else if (s.Contains("|ghp"))
                        {
                            Servant.BasicInformation.GrailHP = await GenericAssigning(s, "|ghp");
                        }
                        else if (s.Contains("|stars"))
                        {
                            Servant.BasicInformation.Stars = await GenericAssigning(s, "|stars");
                        }
                        else if (s.Contains("|cost"))
                        {
                            Servant.BasicInformation.Cost = await GenericAssigning(s, "|cost");
                        }
                        else if (s.Contains("|cc"))
                        {
                            Servant.BasicInformation.CommandCode = await GenericAssigning(s, "|cc");
                        }
                        else if (s.Contains("|mlevel"))
                        {
                            Servant.BasicInformation.MaxLevel = await GenericAssigning(s, "|mlevel");
                        }
                        else if (s.Contains("|id"))
                        {
                            Servant.BasicInformation.ID = await GenericAssigning(s, "|id");
                        }
                        else if (s.Contains("|attribute"))
                        {
                            Servant.BasicInformation.Attribute = await GenericAssigning(s, "|attribute");
                        }
                        else if (s.Contains("|qhits"))
                        {
                            Servant.BasicInformation.QuickHits = await GenericAssigning(s, "|qhits");
                        }
                        else if (s.Contains("|ahits"))
                        {
                            Servant.BasicInformation.ArtsHits = await GenericAssigning(s, "|ahits");
                        }
                        else if (s.Contains("|bhits"))
                        {
                            Servant.BasicInformation.BusterHits = await GenericAssigning(s, "|bhits");
                        }
                        else if (s.Contains("|ehits"))
                        {
                            Servant.BasicInformation.ExtraHits = await GenericAssigning(s, "|ehits");
                        }
                        else if (s.Contains("|deathrate"))
                        {
                            Servant.BasicInformation.DeathRate = await GenericAssigning(s, "|deathrate");
                        }
                        else if (s.Contains("|starabsorption"))
                        {
                            Servant.BasicInformation.StarAbsorption = await GenericAssigning(s, "|starabsorption");
                        }
                        else if (s.Contains("|stargeneration"))
                        {
                            Servant.BasicInformation.StarGeneration = await GenericAssigning(s, "|stargeneration");
                        }
                        else if (s.Contains("|npchargeatk"))
                        {
                            Servant.BasicInformation.NPChargeATK = await GenericAssigning(s, "|npchargeatk");
                        }
                        else if (s.Contains("|npchargedef"))
                        {
                            Servant.BasicInformation.NPChargeDEF = await GenericAssigning(s, "|npchargedef");
                        }
                        else if (s.Contains("|growthc"))
                        {
                            Servant.BasicInformation.GrowthCurve = await GenericAssigning(s, "|growthc");
                        }
                        else if (s.Contains("|aka"))
                        {
                            Servant.BasicInformation.AKA = await GenericArrayAssigning(s, "|aka", OtherPartsToRemove: new string[] { "'''", "''", "{{", "}}" }, PartsToReplace: new string[][] { new string[] { "<br/>", "," }, new string[] { "|", "," } });
                        }
                        else if (s.Contains("|traits"))
                        {
                            Servant.BasicInformation.Traits = await GenericArrayAssigning(s, "|traits");
                        }
                        else if (s.Contains("|gender"))
                        {
                            await Gender(s, Servant.BasicInformation);
                        }
                        else if (s.Contains("|alignment"))
                        {
                            Servant.BasicInformation.Alignment = await GenericAssigning(s, "|alignment");
                        }
                    }
                    #endregion

                    #region Images
                    else if (GettingImages && !string.IsNullOrWhiteSpace(s) && s != "|-|" && s.Contains('|'))
                    {
                        var image = await Image(s, "");
                        if(image != null)
                            Servant.Images.Add(image);
                        image = null;
                    }
                    #endregion

                    #region Trigger Skills Logic
                    if (_GetPassiveSkills && s == "== Passive Skills ==" | s == "==Passive Skills==")
                    {
                        if (Servant.PassiveSkills == null)
                            Servant.PassiveSkills = new List<PassiveSkillList>() { new PassiveSkillList() };
                        GettingPassiveSkills = true;
                    }
                    else if (_GetActiveSkills && s == "== Active Skills ==" | s == "==Active Skills==")
                    {
                        if (Servant.ActiveSkills == null)
                            Servant.ActiveSkills = new List<ActiveSkill>();
                        GettingActiveSkills = true;
                    }
                    else if (_GetAscension && s == "== Ascension ==" | s == "==Ascension==")
                    {
                        if (Servant.Ascension == null)
                            Servant.Ascension = new Ascension();
                        Servant.Ascension = new Ascension();
                        GettingAscension = true;
                    }
                    else if (_GetSkillReinforcement && s == "== Skill Reinforcement ==" | s == "==Skill Reinforcement==")
                    {
                        if (Servant.SkillReinforcement == null)
                            Servant.SkillReinforcement = new SkillReinforcement();
                        Servant.SkillReinforcement = new SkillReinforcement();
                        GettingSkillReinforcement = true;
                    }
                    else if (_GetNoblePhantasm && s == "== Noble Phantasm ==" | s == "==Noble Phantasm==")
                    {
                        if (Servant.NoblePhantasms == null)
                            Servant.NoblePhantasms = new List<NoblePhantasmList>();
                        GettingNoblePhantasm = true;
                        Servant.NoblePhantasms.Add(new NoblePhantasmList());
                    }
                    else if (_GetStats && s == "== Stats ==" | s == "==Stats==")
                    {
                        if (Servant.Stats == null)
                            Servant.Stats = new Stats();
                        GettingStats = true;
                    }
                    else if (_GetBondLevel && s == "== Bond Level ==" | s == "==Bond Level==")
                    {
                        if (Servant.BondLevels == null)
                            Servant.BondLevels = new BondLevels();
                        GettingBondLevel = true;
                    }
                    else if (_GetBiography && s == "== Biography ==" | s == "==Biography==")
                    {
                        if (Servant.Biography == null)
                            Servant.Biography = new Biography();
                        GettingBiography = true;
                    }
                    else if (_GetAvailability && s == "== Availability ==" | s == "==Availability==")
                    {
                        GettingAvailability = true;
                    }
                    else if (_GetTrivia && s == "== Trivia ==" | s == "==Trivia==")
                    {
                        GettingTrivia = true;
                    }
                    else if (_GetImages && s == "== Images ==" | s == "==Images==")
                    {
                        if (Servant.Images == null)
                            Servant.Images = new List<ImageInformation>();
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
                        if (Servant.PassiveSkills != null && Servant.PassiveSkills.Count > 0 && Servant.PassiveSkills[Servant.PassiveSkills.Count - 1].Category == null)
                            GettingPassiveSkills = false;
                        GettingAscension = false;
                        GettingSkillReinforcement = false;
                        GettingStats = false;
                        GettingBondLevel = false;
                        GettingBiography = false;
                        GettingBasicInformation = false;
                        if (Servant.Biography != null && Servant.Biography.Extra.EnglishText != null && Servant.Biography.Extra.EnglishText.Contains("<!--"))
                        {
                            while (Servant.Biography.Extra.EnglishText.Contains("<!--"))
                            {
                                Servant.Biography.Extra.EnglishText = Servant.Biography.Extra.EnglishText.Remove(Servant.Biography.Extra.EnglishText.IndexOf("-->"), Servant.Biography.Extra.JapaneseText.IndexOf("<!--") - 3);
                            }
                        }
                    }
                    else if (GettingAvailability && s == "|}")
                    {
                        GettingAvailability = false;
                        Servant.Availability = Servant.Availability[0].Split('\\');
                    }
                    else if (GettingTrivia && string.IsNullOrWhiteSpace(s))
                    {
                        Servant.Trivia = Servant.Trivia[0].Split('\\');
                        GettingTrivia = false;
                    }
                    #endregion
                }
                resultString = null;
            }

            if (Servant != null && Servant.BasicInformation != null && string.IsNullOrWhiteSpace(Servant.BasicInformation.Cost))
                Servant.BasicInformation.Cost = "16";

            await FateGrandOrderApiCache.SaveCache(FateGrandOrderApiCache.Servants);
            ServantName = null;
            return Servant;
        }

        /// <summary>
        /// Assigning Content
        /// </summary>
        internal class AssigningContent
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
                string[] ToReturn = null;
                s = FixString(s);
                if (!string.IsNullOrWhiteSpace(Assigning))
                    s = s.Replace(Assigning, "").Replace("=", "");
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
                    ToReturn = s.Trim().Replace($"{CharToSplitWith} ", CharToSplitWith.ToString()).Split(CharToSplitWith);
                }
                catch (Exception e)
                {
                    LogConsole(e, $"Looks like something failed when assigning something", $"Assigning string: {Assigning}");
                    LogFile(e, $"Looks like something failed when assigning something", $"Assigning string: {Assigning}");
                }
                if (ToReturn == null)
                    ToReturn = s.Split(CharToSplitWith);
                s = null;
                Assigning = null;
                OtherPartsToRemove = null;
                PartsToReplace = null;
                return ToReturn;
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
                        s = s.Replace(Assigning, "").Replace("=", "").Trim().Replace("/r/n","\r\n");
                    else
                        s = s.Replace("=", "").Trim().Replace("/r/n", "\r\n");
                }
                catch (Exception e)
                {
                    LogConsole(e, $"Looks like something failed when assigning something", $"Assigning string: {Assigning}");
                    LogFile(e, $"Looks like something failed when assigning something", $"Assigning string: {Assigning}");
                }
                Assigning = null;
                OtherPartsToRemove = null;
                PartsToReplace = null;
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
                    LogConsole(e, $"Looks like something failed when assigning someone gender", $"String used for this: {s}");
                    LogFile(e, $"Looks like something failed when assigning someone gender", $"String used for this: {s}");
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
                    OtherPartsToRemove = new string[] { "File:", "link", "[[", "]]" };

                string baseUri = "https://vignette.wikia.nocookie.net/fategrandorder/images";
                int DirDepth = 2;
                string hashPartToUse = "";
                s = await GenericAssigning(s, ImageKeyword, OtherPartsToRemove);

                if (s.Contains("px"))
                    s = s.Remove(s.IndexOf('|'), s.LastIndexOf('|') - s.IndexOf('|'));

                var Image = new ImageInformation(s);
                if (s.Contains("|"))
                    Image.FileName = s.Remove(s.IndexOf('|'));
                else
                    Image.FileName = s;

                if (!s.Contains("."))
                    Image.FileName = Image.FileName + ".png";

                try
                {
                    foreach (ImageInformation ImageC in FateGrandOrderApiCache.Images)
                    {
                        if (s == ImageC.GeneratedWith && ImageC.Name == Image.Name)
                        {
#if DEBUG
                            ImageC.FromCache = true;
#endif
                            OtherPartsToRemove = null;
                            baseUri = null;
                            hashPartToUse = null;
                            s = null;
                            Image = null;
                            return ImageC;
                        }
                        else if (s != ImageC.GeneratedWith && Image.Name == ImageC.Name)
                        {
                            Image = ImageC;
                            break;
                        }
                    }
                }
                catch (Exception e)
                {
                    LogConsole(e, "Looks like something happened when accessing/using the cache for Images", $"Image name: {Image.Name}");
                    LogFile(e, "Looks like something happened when accessing/using the cache for Images", $"Image name: {Image.Name}");
                }

                #region Add/Remove to/from cache
                if (Image != null && !FateGrandOrderApiCache.Images.Contains(Image))
                    FateGrandOrderApiCache.Images.Add(Image);
                #endregion

                if (s.Contains("|"))
                    Image.Name = s.Remove(0, s.IndexOf('|') + 1);
                else if (s.Contains("."))
                    Image.Name = s.Remove(s.LastIndexOf('.'));
                else
                    Image.Name = s;

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
                OtherPartsToRemove = null;
                baseUri = null;
                hashPartToUse = null;
                s = null;
                await FateGrandOrderApiCache.SaveCache(FateGrandOrderApiCache.Images);
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
                data = null;
                string ToReturn = sBuilder.ToString();
                sBuilder = null;
                md5Hash = null;
                input = null;

                // Return the hexadecimal string.
                return ToReturn;
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
                    video = new VideoInformation { Title = VideoName };
                    var VideoInfomation = new HtmlWeb().Load($"https://fategrandorder.fandom.com/wiki/File:{VideoName}").GetElementbyId("mw-content-text");
                    if (!string.IsNullOrWhiteSpace(VideoInfomation.InnerHtml))
                    {
                        var sometextlol = VideoInfomation.InnerHtml;
                        sometextlol = sometextlol.Remove(sometextlol.IndexOf("</script>") - 1);
                        sometextlol = sometextlol.Remove(0, sometextlol.IndexOf('{'));
                        var JSON = JsonConvert.DeserializeObject<VideoJSON>(sometextlol.Replace("\\", "\\\\"));
                        video.VideoProvider = JSON.Provider;
                        if (JSON.Provider == "youtube")
                        {
                            video.Uri = $"http://youtu.be/{JSON.JsParams.VideoId}";
                        }
                        else
                        {
                            LogConsole(null, "Don't know Video Provider Uri", $"Video name: {video.Title}\r\nVideo Provider: {JSON.Provider}");
                            LogFile(null, "Don't know Video Provider Uri", $"Video name: {video.Title}\r\nVideo Provider: {JSON.Provider}");
                        }
                        sometextlol = null;
                        JSON = null;
                    }
                    VideoInfomation = null;
                }
                catch (Exception e)
                {
                    LogConsole(e, "Looks like something happened in GetVideo Logic", $"Video name: {video.Title}");
                    LogFile(e, "Looks like something happened in GetVideo Logic", $"Video name: {video.Title}");
                }
                VideoName = null;
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
                        s = null;
                        ItemNumber = null;
                        AscensionSkillReinforcementNumber = null;
                        return AscensionToMake;
                    }
                    else
                    {
                        WhatToFill = new Item();
                        string name = await GenericAssigning(s, $"|{ItemNumber}", new string[] { "{{Inum|{{", "{{", "}}" });
                        if (name.IndexOf('|') != -1)
                            WhatToFill = await GetItem(name.Remove(name.IndexOf('|')));
                        else
                            WhatToFill = await GetItem(name);
                        name = null;
                    }
                }
                catch (Exception e)
                {
                    LogConsole(e, $"Looks like something failed when assigning an Item", $"WhatToFill.EnglishName: {WhatToFill.EnglishName}");
                    LogFile(e, $"Looks like something failed when assigning an Item", $"WhatToFill.EnglishName: {WhatToFill.EnglishName}");
                }
                s = null;
                ItemNumber = null;
                AscensionSkillReinforcementNumber = null;
                return WhatToFill;
            }

            public static bool FromToGrabToBool(ToGrab toGrab)
            {
                return toGrab == ToGrab.Grab;
            }
        }
    }
}
