using System;
using System.IO;
using System.Net;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Parser
{
    public class values
    {
        public Dictionary<string, int> Nicknames { get; set; }
        public Dictionary<string, int[]> Maps { get; set; }
        public Dictionary<string, int[]> Opponents { get; set; }
        public int Total_time { get; set; }
        public int Total_times { get; set; }

        public void Add(string name, string map, int lenth, List<string> opponents)
        {
            if (Nicknames.ContainsKey(name))
                Nicknames[name] += 1;
            else
                Nicknames[name] = 1;
            if (Maps.ContainsKey(map))
            {
                Maps[map][0] += 1;
                Maps[map][1] += lenth;
            }
            else
                Maps[map] = new int[] { 1, lenth };
            foreach (string opp in opponents)
            {
                if (Opponents.ContainsKey(opp))
                {
                    Opponents[opp][0] += 1;
                    Opponents[opp][1] += lenth;
                }
                else
                    Opponents[opp] = new int[] { 1, lenth };
            }
            Total_time += lenth;
            Total_times += 1;
        }

        public void AddNick(string name, int times)
        {
            if (Nicknames.ContainsKey(name))
                Nicknames[name] += times;
            else
                Nicknames[name] = times;
        }

        public void AddMaps(string map, int times, int lenth)
        {
            if (Maps.ContainsKey(map))
            {
                Maps[map][0] += times;
                Maps[map][1] += lenth;
            }
            else
            {
                Maps[map] = new int[] { times, lenth };
            }
        }

        public void AddOpp(string name, int times, int lenth)
        {
            if (Opponents.ContainsKey(name))
            {
                Opponents[name][0] += times;
                Opponents[name][1] += lenth;
            }
            else
            {
                Opponents[name] = new int[] { times, lenth };
            }
        }
    }

    class Program
    {
        static values games_1v1 = new values { Nicknames = new Dictionary<string, int>(), Maps = new Dictionary<string, int[]>(), Opponents = new Dictionary<string, int[]>(), Total_time = 0, Total_times = 0 };
        static values games_2v2 = new values { Nicknames = new Dictionary<string, int>(), Maps = new Dictionary<string, int[]>(), Opponents = new Dictionary<string, int[]>(), Total_time = 0, Total_times = 0 };
        static values games_3v3 = new values { Nicknames = new Dictionary<string, int>(), Maps = new Dictionary<string, int[]>(), Opponents = new Dictionary<string, int[]>(), Total_time = 0, Total_times = 0 };
        static values games_4v4 = new values { Nicknames = new Dictionary<string, int>(), Maps = new Dictionary<string, int[]>(), Opponents = new Dictionary<string, int[]>(), Total_time = 0, Total_times = 0 };
        static values games_ffa3 = new values { Nicknames = new Dictionary<string, int>(), Maps = new Dictionary<string, int[]>(), Opponents = new Dictionary<string, int[]>(), Total_time = 0, Total_times = 0 };
        static values games_ffa4 = new values { Nicknames = new Dictionary<string, int>(), Maps = new Dictionary<string, int[]>(), Opponents = new Dictionary<string, int[]>(), Total_time = 0, Total_times = 0 };
        static values games_ffa5 = new values { Nicknames = new Dictionary<string, int>(), Maps = new Dictionary<string, int[]>(), Opponents = new Dictionary<string, int[]>(), Total_time = 0, Total_times = 0 };
        static values games_ffa6 = new values { Nicknames = new Dictionary<string, int>(), Maps = new Dictionary<string, int[]>(), Opponents = new Dictionary<string, int[]>(), Total_time = 0, Total_times = 0 };
        static values games_ffa7 = new values { Nicknames = new Dictionary<string, int>(), Maps = new Dictionary<string, int[]>(), Opponents = new Dictionary<string, int[]>(), Total_time = 0, Total_times = 0 };
        static values games_ffa8 = new values { Nicknames = new Dictionary<string, int>(), Maps = new Dictionary<string, int[]>(), Opponents = new Dictionary<string, int[]>(), Total_time = 0, Total_times = 0 };
        static values games_other = new values { Nicknames = new Dictionary<string, int>(), Maps = new Dictionary<string, int[]>(), Opponents = new Dictionary<string, int[]>(), Total_time = 0, Total_times = 0 };
        static Dictionary<string, int> game_types = new Dictionary<string, int>();
        static string gentoolurl = "https://gentool.net/data/zh/";
        static CultureInfo language = new CultureInfo("en-US");


        public static void parse_day(WebClient client, DateTime date)
        {
            string[] keys = File.ReadAllLines("keys.txt");
            string[] file_names = new string[keys.Length];
            for (int i = 0; i < keys.Length; i++)
                if (keys[i] != "")
                {
                    file_names[i] = keys[i].Split(" | ")[1];
                    keys[i] = keys[i].Split(" | ")[0];
                }
            for (int key = 0; key < keys.Length; key++)
                if (keys[key] != "")
                    GetPlayers(date.ToString("yyyy_MM_MMMM/", language), date.ToString("dd_dddd/", language), keys[key], client, file_names[key]);
        }


        public static void parse_month(string data, WebClient client, string file_name)
        {
            try
            {
                DateTime Date = DateTime.UtcNow.AddDays(-1);
                for (var day = new DateTime(Date.Year, Date.Month, 1); day <= Date; day = day.AddDays(1))
                {
                    GetPlayers(Date.ToString("yyyy_MM_MMMM/", language), day.ToString("dd_dddd/", language), data, client, file_name);
                }
                File.WriteAllText("result.txt", "OK");
            }
            catch (Exception e)
            {
                File.WriteAllText("result.txt", e.Message.ToString());
            }
        }



        static void get_battle_info(string txt_file)
        {
            string nickname = txt_file.Split("Player Name:      ")[1].Split("\nPlayer Id:")[0];
            int length = (int)TimeSpan.Parse(txt_file.Split("Match Length:     ")[1].Split("\nMatch Mode:")[0]).TotalSeconds;

            if (length < 150 || txt_file.Contains("000 " + nickname + " (Observer)"))
                return;
            string map_name = txt_file.Split("Map Name:         ")[1].Split("\nStart Cash:")[0].Replace("maps/", "");
            string match_type = txt_file.Split("Match Type:       ")[1].Split("\nMatch Length:")[0];
            List<string> opponents = new List<string>();
            txt_file = txt_file.Split("Match Mode:       ")[1].Split("Associated files:")[0];
            string[] players = txt_file.Split("\n");
            foreach (string line in players)
                if (line.Contains("000 ") && !line.Contains(" (Observer)") && !line.Contains("000 " + nickname + " ("))
                    opponents.Add(line.Remove(0, 12).Remove(line.IndexOf("(") - 13));
            if (match_type == "1v1")
                games_1v1.Add(nickname, map_name, length, opponents);
            else if (match_type == "2v2")
                games_2v2.Add(nickname, map_name, length, opponents);
            else if (match_type == "3v3")
                games_3v3.Add(nickname, map_name, length, opponents);
            else if (match_type == "4v4")
                games_4v4.Add(nickname, map_name, length, opponents);
            else if (match_type == "1v1v1")
                games_ffa3.Add(nickname, map_name, length, opponents);
            else if (match_type == "1v1v1v1")
                games_ffa4.Add(nickname, map_name, length, opponents);
            else if (match_type == "1v1v1v1v1")
                games_ffa5.Add(nickname, map_name, length, opponents);
            else if (match_type == "1v1v1v1v1v1")
                games_ffa6.Add(nickname, map_name, length, opponents);
            else if (match_type == "1v1v1v1v1v1v1")
                games_ffa7.Add(nickname, map_name, length, opponents);
            else if (match_type == "1v1v1v1v1v1v1v1")
                games_ffa8.Add(nickname, map_name, length, opponents);
            else
            {
                games_other.Add(nickname, map_name, length, opponents);
                if (game_types.ContainsKey(match_type))
                    game_types[match_type] += 1;
                else
                    game_types[match_type] = 1;
            }
        }


        public static void GetPlayers(string month, string day, string user, WebClient client, string file_name)
        {
            string webpage = client.DownloadString(gentoolurl + month + day);
            string[] players_arr = webpage.Split("\n");
            string key = user.Remove(12);
            string motherboard = user.Remove(0, 13);
            for (int i = 0; i < players_arr.Length; i++)
            {
                if (players_arr[i].Contains(key))
                {
                    string playerfolder = players_arr[i].Remove(0, players_arr[i].IndexOf("<a href=") + "<a href=".Length + 1);
                    playerfolder = playerfolder.Remove(playerfolder.IndexOf("\">"));
                    string playerpage = client.DownloadString(gentoolurl + month + day + playerfolder);
                    string[] playerfiles = playerpage.Split("\n");
                    for (int h = 0; h < playerfiles.Length; h++)
                    {
                        if (playerfiles[h].Contains(".txt"))
                        {

                            string txt_name = playerfiles[h].Remove(0, playerfiles[h].IndexOf("<a href=") + "<a href=".Length + 1);
                            txt_name = txt_name.Remove(txt_name.IndexOf(">") - 1);
                            string txt_file = client.DownloadString(gentoolurl + month + day + playerfolder + txt_name);
                            string[] txtfilearr = txt_file.Split("\n");
                            string nickname = "";
                            for (int n = 0; n < txtfilearr.Length; n++)
                                if (txtfilearr[n].Contains("Player Name:"))
                                    nickname = txtfilearr[n].Remove(0, "Player Name:      ".Length);
                            if (nickname != "")
                                if (txt_file.Contains(motherboard))
                                    get_battle_info(txt_file);
                        }
                    }
                }
            }
            output_file(month, day, file_name);
        }


        static void output_file(string month, string day, string file_name)
        {
            string output_file = "1v1 Statistic:" + Environment.NewLine;
            output_file += values_to_text(games_1v1);
            output_file += "2v2 Statistic:" + Environment.NewLine;
            output_file += values_to_text(games_2v2);
            output_file += "3v3 Statistic:" + Environment.NewLine;
            output_file += values_to_text(games_3v3);
            output_file += "4v4 Statistic:" + Environment.NewLine;
            output_file += values_to_text(games_4v4);
            output_file += "ffa 3 Statistic:" + Environment.NewLine;
            output_file += values_to_text(games_ffa3);
            output_file += "ffa 4 Statistic:" + Environment.NewLine;
            output_file += values_to_text(games_ffa4);
            output_file += "ffa 5 Statistic:" + Environment.NewLine;
            output_file += values_to_text(games_ffa5);
            output_file += "ffa 6 Statistic:" + Environment.NewLine;
            output_file += values_to_text(games_ffa6);
            output_file += "ffa 7 Statistic:" + Environment.NewLine;
            output_file += values_to_text(games_ffa7);
            output_file += "ffa 8 Statistic:" + Environment.NewLine;
            output_file += values_to_text(games_ffa8);
            output_file += "Other Statistic:" + Environment.NewLine;
            output_file += values_other_to_text(games_other, game_types);

            if (!Directory.Exists("data/"))
                Directory.CreateDirectory("data/");
            if (!Directory.Exists("data/" + month.Remove(7))) ;
            Directory.CreateDirectory("data/" + month.Remove(7));
            if (!Directory.Exists("data/" + month.Remove(7) + "/" + day.Remove(2)))
                Directory.CreateDirectory("data/" + month.Remove(7) + "/" + day.Remove(2));

            File.WriteAllText("data/" + month.Remove(7) + "/" + day.Remove(2) + "/" + file_name + ".txt", output_file);
            Console.WriteLine("Got info for: " + month + day + file_name);
            games_1v1 = new values { Nicknames = new Dictionary<string, int>(), Maps = new Dictionary<string, int[]>(), Opponents = new Dictionary<string, int[]>(), Total_time = 0, Total_times = 0 };
            games_2v2 = new values { Nicknames = new Dictionary<string, int>(), Maps = new Dictionary<string, int[]>(), Opponents = new Dictionary<string, int[]>(), Total_time = 0, Total_times = 0 };
            games_3v3 = new values { Nicknames = new Dictionary<string, int>(), Maps = new Dictionary<string, int[]>(), Opponents = new Dictionary<string, int[]>(), Total_time = 0, Total_times = 0 };
            games_4v4 = new values { Nicknames = new Dictionary<string, int>(), Maps = new Dictionary<string, int[]>(), Opponents = new Dictionary<string, int[]>(), Total_time = 0, Total_times = 0 };
            games_ffa3 = new values { Nicknames = new Dictionary<string, int>(), Maps = new Dictionary<string, int[]>(), Opponents = new Dictionary<string, int[]>(), Total_time = 0, Total_times = 0 };
            games_ffa4 = new values { Nicknames = new Dictionary<string, int>(), Maps = new Dictionary<string, int[]>(), Opponents = new Dictionary<string, int[]>(), Total_time = 0, Total_times = 0 };
            games_ffa5 = new values { Nicknames = new Dictionary<string, int>(), Maps = new Dictionary<string, int[]>(), Opponents = new Dictionary<string, int[]>(), Total_time = 0, Total_times = 0 };
            games_ffa6 = new values { Nicknames = new Dictionary<string, int>(), Maps = new Dictionary<string, int[]>(), Opponents = new Dictionary<string, int[]>(), Total_time = 0, Total_times = 0 };
            games_ffa7 = new values { Nicknames = new Dictionary<string, int>(), Maps = new Dictionary<string, int[]>(), Opponents = new Dictionary<string, int[]>(), Total_time = 0, Total_times = 0 };
            games_ffa8 = new values { Nicknames = new Dictionary<string, int>(), Maps = new Dictionary<string, int[]>(), Opponents = new Dictionary<string, int[]>(), Total_time = 0, Total_times = 0 };
            games_other = new values { Nicknames = new Dictionary<string, int>(), Maps = new Dictionary<string, int[]>(), Opponents = new Dictionary<string, int[]>(), Total_time = 0, Total_times = 0 };
        }

        public static string values_to_text(values values, string date_format = @"hh\:mm\:ss")
        {
            if (values.Total_time == 0)
                return "*****" + Environment.NewLine;
            string output_file = "Nicknames   , Times" + Environment.NewLine + "------------,-------" + Environment.NewLine;
            int map_lenth = 4;
            int nickname_lenth = 12;
            int opponent_lenth = 12;

            foreach (KeyValuePair<string, int[]> keyValues in values.Maps)
            {
                if (keyValues.Key.Length > map_lenth)
                    map_lenth = keyValues.Key.Length;
            }

            foreach (KeyValuePair<string, int[]> keyValues in values.Opponents)
            {
                if (keyValues.Key.Length > opponent_lenth)
                    opponent_lenth = keyValues.Key.Length;
            }

            foreach (var pair in values.Nicknames.OrderByDescending(pair => pair.Value))
            {
                output_file += pair.Key.PadRight(nickname_lenth) + ",  " + pair.Value.ToString() + Environment.NewLine;
            }

            output_file += Environment.NewLine + "Maps".PadRight(map_lenth) + ",Times,   Time" + Environment.NewLine + "-".PadRight(map_lenth, '-') + ",-----,----------" + Environment.NewLine;

            foreach (var pair in values.Maps.OrderByDescending(pair => pair.Value[0]))
            {
                output_file += pair.Key.PadRight(map_lenth) + ", " + pair.Value[0].ToString().PadRight(4) + ", " + TimeSpan.FromSeconds(Convert.ToDouble(pair.Value[1])).ToString(@"hh\:mm\:ss") + Environment.NewLine;
            }

            output_file += Environment.NewLine + "Nickname".PadRight(opponent_lenth) + ",Times, Time" + Environment.NewLine + "-".PadRight(opponent_lenth, '-') + ",-----,----------" + Environment.NewLine;

            foreach (var pair in values.Opponents.OrderByDescending(pair => pair.Value[0]))
            {
                output_file += pair.Key.PadRight(opponent_lenth) + ", " + pair.Value[0].ToString().PadRight(4) + ", " + TimeSpan.FromSeconds(Convert.ToDouble(pair.Value[1])).ToString(@"hh\:mm\:ss") + Environment.NewLine;
            }

            output_file += Environment.NewLine + "Total Time: " + TimeSpan.FromSeconds(Convert.ToDouble(values.Total_time)).ToString(date_format) + " Total Times: " + values.Total_times + Environment.NewLine + "*****" + Environment.NewLine;
            return output_file;
        }

        public static string values_other_to_text(values games_other, Dictionary<string, int> game_types, string date_format = @"hh\:mm\:ss")
        {
            if (games_other.Total_time == 0)
                return "*****" + Environment.NewLine;
            string output_file = "Nicknames   , Times" + Environment.NewLine + "------------,-------" + Environment.NewLine;
            int map_lenth = 4;
            int type_lenth = 10;
            int nickname_lenth = 12;
            int opponent_lenth = 12;
            foreach (KeyValuePair<string, int[]> keyValues in games_other.Maps)
            {
                if (keyValues.Key.Length > map_lenth)
                    map_lenth = keyValues.Key.Length;
            }

            foreach (KeyValuePair<string, int[]> keyValues in games_other.Opponents)
            {
                if (keyValues.Key.Length > opponent_lenth)
                    opponent_lenth = keyValues.Key.Length;
            }

            foreach (KeyValuePair<string, int> keyValues in game_types)
            {
                if (keyValues.Key.Length > type_lenth)
                    type_lenth = keyValues.Key.Length;
            }

            foreach (var pair in games_other.Nicknames.OrderByDescending(pair => pair.Value))
            {
                output_file += pair.Key.PadRight(nickname_lenth) + ",  " + pair.Value.ToString() + Environment.NewLine;
            }

            output_file += Environment.NewLine + "Game Types".PadRight(type_lenth) + ",Times" + Environment.NewLine + "-".PadRight(type_lenth, '-') + ",-----" + Environment.NewLine;

            foreach (var pair in game_types.OrderByDescending(pair => pair.Value))
            {
                output_file += pair.Key.PadRight(type_lenth) + ", " + pair.Value.ToString().PadRight(4) + Environment.NewLine;
            }

            output_file += Environment.NewLine + "Maps".PadRight(map_lenth) + ",Times,   Time" + Environment.NewLine + "-".PadRight(map_lenth, '-') + ",-----,----------" + Environment.NewLine;

            foreach (var pair in games_other.Maps.OrderByDescending(pair => pair.Value[0]))
            {
                output_file += pair.Key.PadRight(map_lenth) + ", " + pair.Value[0].ToString().PadRight(4) + ", " + TimeSpan.FromSeconds(Convert.ToDouble(pair.Value[1])).ToString(@"hh\:mm\:ss") + Environment.NewLine;
            }

            output_file += Environment.NewLine + "Nickname".PadRight(opponent_lenth) + ",Times, Time" + Environment.NewLine + "-".PadRight(opponent_lenth, '-') + ",-----,----------" + Environment.NewLine;

            foreach (var pair in games_other.Opponents.OrderByDescending(pair => pair.Value[0]))
            {
                output_file += pair.Key.PadRight(opponent_lenth) + ", " + pair.Value[0].ToString().PadRight(4) + ", " + TimeSpan.FromSeconds(Convert.ToDouble(pair.Value[1])).ToString(@"hh\:mm\:ss") + Environment.NewLine;
            }

            output_file += Environment.NewLine + "Total Time: " + TimeSpan.FromSeconds(Convert.ToDouble(games_other.Total_time)).ToString(date_format) + " Total Times: " + games_other.Total_times + Environment.NewLine + "*****" + Environment.NewLine;
            return output_file;
        }
    }
}