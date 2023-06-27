using System;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using System.Collections.Generic;
using DSharpPlus.Entities;
using System.IO;
using System.Net;

namespace ParserBot
{

    public class GetFromFiles
    {
        static string Nickname_Start = ",-------" + Environment.NewLine;
        static string Nickname_End = Environment.NewLine + Environment.NewLine + "Maps";
        static string Maps_Start = ",-----,----------" + Environment.NewLine;
        static string Maps_End = Environment.NewLine + Environment.NewLine + "Nickname";
        static string Opp_Start = ",-----,----------" + Environment.NewLine;
        static string Opp_End = Environment.NewLine + Environment.NewLine + "Total Time:";
        static string[] Modes = { "1v1 Statistic:", "2v2 Statistic:" , "3v3 Statistic:", "4v4 Statistic:", "ffa 3 Statistic:",
        "ffa 4 Statistic:", "ffa 5 Statistic:", "ffa 6 Statistic:", "ffa 7 Statistic:", "ffa 8 Statistic:", "Other Statistic:"};
        static string[] Args = { "1v1", "2v2", "3v3", "4v4", "ffa3", "ffa4", "ffa5", "ffa6", "ffa7", "ffa8", "other" };
        public static DateTime ArgsToDate(string year, string month, string day)
        {
            return new DateTime(Convert.ToInt32(year), Convert.ToInt32(month), Convert.ToInt32(day));
        }

        public static string ArgToStat(string arg)
        {
            for (int i = 0; i < Args.Length; i++)
                if (Args[i] == arg)
                    return Modes[i];
            return null;
        }

        public static (DateTime, DateTime) GiveDates(DateTime start_date, DateTime end_date, string id)
        {
            DateTime reg_date = GetDateRegById(id);
            DateTime now_time = DateTime.UtcNow.AddDays(-1).AddHours(-1);

            if (reg_date > start_date)
                start_date = reg_date;
            if (end_date > now_time)
                end_date = now_time;
            return (start_date, end_date);
        }

        public static bool CheckDate(DateTime start_date, DateTime end_date, string id)
        {
            DateTime reg_date = GetDateRegById(id);
            DateTime now_time = DateTime.UtcNow.AddDays(-1).AddHours(-1);

            if (reg_date > end_date || end_date < start_date || now_time < start_date || now_time < end_date)
                return false;
            return true;
        }

        public static DateTime GetDateRegById(string id)
        {
            string[] keys = File.ReadAllLines("keys.txt");
            foreach (string line in keys)
            {
                if (line.Contains(id.ToString()))
                    return new DateTime(Convert.ToInt32(line.Split(" | ")[2].Remove(0, 3)), Convert.ToInt32(line.Split(" | ")[2].Remove(2)), 1);
            }
            return new DateTime();
        }

        public bool ContainsRegId(string id)
        {
            string[] keys = File.ReadAllLines("keys.txt");
            if (keys.Contains("| " + id + " |"))
                return true;
            else
                return false;
        }

        public bool ContainsRegPlayer(string player_info)
        {
            string[] keys = File.ReadAllLines("keys.txt");
            if (keys.Contains(player_info + " |"))
                return true;
            else
                return false;
        }

        static List<string> GetPaths(DateTime start_date, DateTime end_date, string user_id)
        {
            var path = new List<string>();
            for (var date = start_date; date <= end_date; date = date.AddDays(1))
            {
                path.Add("data/" + date.Year.ToString() + "_" + date.Month.ToString().PadLeft(2, '0') + "/" + date.Day.ToString().PadLeft(2, '0') + "/" + user_id + ".txt");
            }
            return path;
        }

        public static string GetText(DateTime start_date, DateTime end_date, string id, string stat)
        {
            List<string> paths = GetPaths(start_date, end_date, id);
            Dictionary<string, int> game_types = new Dictionary<string, int>();
            Parser.values games = new Parser.values { Nicknames = new Dictionary<string, int>(), Maps = new Dictionary<string, int[]>(), Opponents = new Dictionary<string, int[]>(), Total_time = 0, Total_times = 0 };
            string output = stat + Environment.NewLine;
            string txt;
            string[] part_txt;
            string[] line_elements;
            foreach (string path in paths)
            {
                try
                {
                    txt = File.ReadAllText(path).Split(stat)[1].Split("*****")[0];
                }
                catch { continue; }
                if (txt == "\r\n")
                    continue;

                

                if (stat == "Other Statistic:")
                {
                    part_txt = txt.Split(Environment.NewLine + Environment.NewLine + "Game Types")[0].Split(Nickname_Start)[1].Split(Environment.NewLine);
                    foreach (string line in part_txt)
                        games.AddNick(line.Split(",")[0], Convert.ToInt32(line.Split(",")[1]));
                    part_txt = txt.Split(Nickname_End)[0].Split(",-----" + Environment.NewLine)[^1].Split(Environment.NewLine);
                    foreach (string line in part_txt)
                    {
                        line_elements = line.Split(",");
                        if (game_types.ContainsKey(line_elements[0].Trim()))
                            game_types[line_elements[0].Trim()] += Convert.ToInt32(line_elements[1].Trim());
                        else
                            game_types.Add(line_elements[0].Trim(), Convert.ToInt32(line_elements[1].Trim()));
                    }
                        
                }
                else
                {
                    part_txt = txt.Split(Nickname_End)[0].Split(Nickname_Start)[1].Split(Environment.NewLine);
                    foreach (string line in part_txt)
                        games.AddNick(line.Split(",")[0], Convert.ToInt32(line.Split(",")[1]));
                }
                part_txt = txt.Split(Maps_End)[0].Split(Maps_Start)[1].Split(Environment.NewLine);
                foreach (string line in part_txt)
                {
                    line_elements = line.Split(",");
                    games.AddMaps(line_elements[0].Trim(), Convert.ToInt32(line_elements[1]), (int)TimeSpan.Parse(line_elements[2].Replace(" ", "")).TotalSeconds);
                }

                part_txt = txt.Split(Opp_End)[0].Split(Opp_Start)[^1].Split(Environment.NewLine);
                foreach (string line in part_txt)
                {
                    line_elements = line.Split(",");
                    games.AddOpp(line_elements[0].Trim(), Convert.ToInt32(line_elements[1]), (int)TimeSpan.Parse(line_elements[2].Replace(" ", "")).TotalSeconds);
                }

                games.Total_time += (int)TimeSpan.Parse(txt.Split("Total Time: ")[^1].Split(" Total Times:")[0].Replace(" ", "")).TotalSeconds;
                games.Total_times += Convert.ToInt32(txt.Split("Total Times: ")[^1].Split(Environment.NewLine)[0]);
            }

            if (games.Total_time == 0)
                return "";
            if (stat == "Other Statistic:")
                output += Parser.Program.values_other_to_text(games, game_types, @"dd\ \d\a\y\s\ hh\:mm\:ss");
            else
                output += Parser.Program.values_to_text(games, @"dd\ \d\a\y\s\ hh\:mm\:ss");
            return output.Replace(",", "|").Replace(Environment.NewLine + "*****", "");
        }

        public static string GetStat(DateTime start_date, DateTime end_date, string id, string stat)
        {
            if (CheckDate(start_date, end_date, id))
            {
                if (stat == "all")
                {
                    string output = "";
                    foreach (string mode in Modes)
                        output += GetText(start_date, end_date, id, mode);
                    return output;
                }
                stat = ArgToStat(stat);
                if (stat == null)
                    return "Wrong arguments!";
                return GetText(start_date, end_date, id, stat);
            }
            else
                return "Wrong date!";
        }
    }


    public class Commands : BaseCommandModule
    {
        [Command("reg")]
        [Description("Registration for data gaining"), RequireRoles(RoleCheckMode.None, "Registred")]
        public async Task Reg(CommandContext ctx, [Description("Link to txt file on gentool data")] params string[] args)
        {
            if (ctx.Channel.Id != 910645825212932096 && ctx.Channel.Id != 910645759148437534)
                return;
            if (args.Length == 1)
            {
                if (!args[0].Contains("http://gentool.net/data/") && !args[0].Contains("https://gentool.net/data/") && !args[0].Contains("/www.gentool.net/data/"))
                {
                    await ctx.RespondAsync($"Wrong website! ");
                    return;
                }
                try
                {
                    WebClient client = new WebClient();
                    string txt_link = "";
                    string[] page = client.DownloadString(args[0]).Split("\n");
                    foreach (string line in page)
                        if (line.Contains(".txt</a>"))
                        {
                            txt_link = line.Split("<a href=\"")[1].Split("\">")[0];
                            break;
                        }
                    string[] page_txt = client.DownloadString(args[0] + txt_link).Split("\n");
                    string playerid = "";
                    string motherboard = "";
                    foreach (string line in page_txt)
                    {
                        if (line.Contains("System:           "))
                            motherboard = line.Replace("System:           ", "");
                        if (line.Contains("Player Id:        "))
                            playerid = line.Replace("Player Id:", "").Replace(" ", "");
                    }

                    string player_info = ctx.Message.Author.Id.ToString();
                    string key = File.ReadAllText("keys.txt");
                    if ((key.Contains(playerid) && key.Contains(motherboard)) || key.Contains(player_info))
                    {
                        await ctx.RespondAsync($"Already registired!");
                        return;
                    }

                    key += Environment.NewLine + playerid + " " + motherboard + " | " + player_info + " | " + DateTime.Now.Month + "_" + DateTime.Now.Year;
                    File.WriteAllText("keys.txt", key);
                    await ctx.RespondAsync($"Registration start!");

                    Parser.Program.parse_month(playerid + " " + motherboard, client, player_info);
                    while (!File.Exists("result.txt"))
                    {

                    }
                    string result = File.ReadAllText("result.txt");
                    if (result == "OK")
                    {
                        await ctx.RespondAsync($"Success registration!");
                        await ctx.Member.GrantRoleAsync(ctx.Guild.GetRole(910618832010350602));
                    }
                    else
                    {
                        await ctx.RespondAsync(result);
                        string[] kkey = File.ReadAllText("keys.txt").Split("\r\n");
                        string remove_key = "";
                        for (int k = 0; k < kkey.Length; k++)
                        {
                            if (kkey[k].Contains(ctx.Message.Author.Id.ToString()))
                                continue;
                            remove_key += kkey[k] + Environment.NewLine;
                        }
                        File.WriteAllText("keys.txt", remove_key);
                    }
                    File.Delete("result.txt");
                }
                catch (Exception e)
                {
                    await ctx.RespondAsync(e.Message.ToString());
                    string[] kkey = File.ReadAllText("keys.txt").Split("\r\n");
                    string key = "";
                    for (int k = 0; k < kkey.Length; k++)
                    {
                        if (kkey[k].Contains(ctx.Message.Author.Id.ToString()))
                            continue;
                        key += kkey[k] + Environment.NewLine;
                    }
                    File.WriteAllText("keys.txt", key);
                }
            }
            else
                await ctx.RespondAsync("Wrong argmuments!" + Environment.NewLine + "Try **;;reg gentool_link**" + Environment.NewLine + "For example:" + Environment.NewLine +
                    ";;reg https://gentool.net/data/zh/2021_11_November/01_Monday/FAS_0B88E7E82DD3/");
        }


        [Command("day"), RequireRoles(RoleCheckMode.Any, "Registred"), Description("Get stat about player for specific day")]
        public async Task Day(CommandContext ctx, DiscordMember member = null, params string[] args)
        {
            if (ctx.Channel.Id != 910645825212932096 && ctx.Channel.Id != 910645759148437534)
                return;
            if (args.Length != 4 && args.Length != 1)
            {
                await ctx.RespondAsync("Wrong arguments!" + Environment.NewLine + "Try: ;;day @player dd mm yyyy stat_type  or ;;day @player stat_type");
                return;
            }

            DateTime date = DateTime.UtcNow.AddDays(-1).AddHours(-1);
            string stat = args[0];
            if (args.Length == 4) // ;;day fas 09 10 2021{
            {
                date = GetFromFiles.ArgsToDate(args[2], args[1], args[0]);
                stat = args[3];
            }

            string message = GetFromFiles.GetStat(date, date, member.Id.ToString(), stat);
            string[] outputfile = message.Split(Environment.NewLine);

            if (message == "")
            {
                await ctx.RespondAsync("The member did not play that day");
                return;
            }
            else if (message == "Wrong arguments!" || message == "Wrong date!")
            {
                await ctx.RespondAsync(message);
                return;
            }
            message = date.ToString("dd.MM.yyyy") + Environment.NewLine;
            SendMsg(ctx, message, outputfile);
        }


        [Command("period"), RequireRoles(RoleCheckMode.Any, "Registred")]
        public async Task Period(CommandContext ctx, DiscordMember member = null, params string[] args)
        {
            if (ctx.Channel.Id != 910645825212932096 && ctx.Channel.Id != 910645759148437534)
                return;
            if (args.Length != 7)
            {
                await ctx.RespondAsync("Wrong arguments!" + Environment.NewLine + "Try: ;;period @player dd mm yyyy   dd mm yyyy stat_type");
                return;
            }

            DateTime start_date = GetFromFiles.ArgsToDate(args[2], args[1], args[0]);
            DateTime end_date = GetFromFiles.ArgsToDate(args[5], args[4], args[3]);
            (start_date, end_date) = GetFromFiles.GiveDates(start_date, end_date, member.Id.ToString());
            string message = GetFromFiles.GetStat(start_date, end_date, member.Id.ToString(), args[6]);
            string[] outputfile = message.Split(Environment.NewLine);

            if (message == "")
            {
                await ctx.RespondAsync("The member did not play that period");
                return;
            }
            else if (message == "Wrong arguments!" || message == "Wrong date!")
            {
                await ctx.RespondAsync(message);
                return;
            }
            message = start_date.ToString("dd.MM.yyyy") + " - " + end_date.ToString("dd.MM.yyyy") + Environment.NewLine;
            SendMsg(ctx, message, outputfile);
        }


        [Command("month"), RequireRoles(RoleCheckMode.Any, "Registred")]
        public async Task Month(CommandContext ctx, DiscordMember member, params string[] args)
        {
            if (ctx.Channel.Id != 910645825212932096 && ctx.Channel.Id != 910645759148437534)
                return;
            if (args.Length != 3)
            {
                await ctx.RespondAsync("Wrong arguments!" + Environment.NewLine + "Try: ;;month @player mm yyyy stat_type");
                return;
            }
            DateTime start_date = GetFromFiles.ArgsToDate(args[1], args[0], "1");
            DateTime end_date = GetFromFiles.ArgsToDate(args[1], args[0], DateTime.DaysInMonth(Convert.ToInt32(args[1]), Convert.ToInt32(args[0])).ToString());
            (start_date, end_date) = GetFromFiles.GiveDates(start_date, end_date, member.Id.ToString());
            string message = GetFromFiles.GetStat(start_date, end_date, member.Id.ToString(), args[2]);
            string[] outputfile = message.Split(Environment.NewLine);

            if (message == "")
            {
                await ctx.RespondAsync("The member did not play that month");
                return;
            }
            else if (message == "Wrong arguments!" || message == "Wrong date!")
            {
                await ctx.RespondAsync(message);
                return;
            }
            message = start_date.ToString("dd.MM.yyyy") + " - " + end_date.ToString("dd.MM.yyyy") + Environment.NewLine;
            SendMsg(ctx, message, outputfile);
        }

        [Command("allstat"), RequireRoles(RoleCheckMode.Any, "Registred")]
        public async Task Allstat(CommandContext ctx, DiscordMember member = null, params string[] args)
        {
            if (ctx.Channel.Id != 910645825212932096 && ctx.Channel.Id != 910645759148437534)
                return;
            if (member == null || args.Length != 1)
            {
                await ctx.RespondAsync("Wrong arguments!" + Environment.NewLine + "Try: ;;allstat @player stat_type");
                return;
            }

            DateTime start_date = GetFromFiles.GetDateRegById(member.Id.ToString());
            DateTime end_date = DateTime.UtcNow.AddDays(-1).AddHours(-1);
            (start_date, end_date) = GetFromFiles.GiveDates(start_date, end_date, member.Id.ToString());
            string message = GetFromFiles.GetStat(start_date, end_date, member.Id.ToString(), args[0]);
            string[] outputfile = message.Split(Environment.NewLine);

            if (message == "")
            {
                await ctx.RespondAsync("Player statistics not found");
                return;
            }
            else if (message == "Wrong arguments!" || message == "Wrong date!")
            {
                await ctx.RespondAsync(message);
                return;
            }
            message = start_date.ToString("dd.MM.yyyy") + " - " + end_date.ToString("dd.MM.yyyy") + Environment.NewLine;
            SendMsg(ctx, message, outputfile);
        }


        [Command("unreg"), RequireRoles(RoleCheckMode.Any, "BotController")]
        public async Task Unreg(CommandContext ctx, DiscordMember member = null)
        {
            if (ctx.Channel.Id != 910645825212932096 && ctx.Channel.Id != 910645759148437534)
                return;
            if (member == null)
            {
                await ctx.RespondAsync("Wrong arguments");
                return;
            }

            string[] keys = File.ReadAllLines("keys.txt");
            string file = "";
            for (int key = 0; key < keys.Length; key++)
            {
                if (keys[key].Contains(member.Id.ToString()))
                    keys[key] = "";
                if (keys[key] != "")
                    file += keys[key] + Environment.NewLine;
            }
            File.WriteAllText("keys.txt", file);
            try
            {
                var role = ctx.Guild.GetRole(910618832010350602);
                await member.RevokeRoleAsync(role);
            }
            catch (Exception e) 
            { await ctx.RespondAsync(e.ToString()); }
            await ctx.RespondAsync("Done");
        }

        [Command("updatedb"), RequireRoles(RoleCheckMode.Any, "BotController")]
        public async Task Updatedb(CommandContext ctx, params int[] args)
        {
            Parser.Program.parse_day(new WebClient(), new DateTime(args[2], args[1], args[0]));
            await ctx.RespondAsync("Done");
        }

        public async void SendMsg(CommandContext ctx, string message, string[] outputfile)
        {
            foreach (string line in outputfile)
            {
                if ((message + line).Length > 1950 || (line.Contains(" Statistic:") && !line.Contains("1v1")))
                {
                    await ctx.RespondAsync("```css" + Environment.NewLine + message + "```");
                    message = line + Environment.NewLine;
                }
                else
                {
                    message += line + Environment.NewLine;
                }
            }

            if (message != "")
                await ctx.RespondAsync("```css" + Environment.NewLine + message + "```");
        }
    }
}
