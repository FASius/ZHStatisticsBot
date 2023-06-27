using System;
using System.IO;
using System.Net;
using System.Collections.Generic;
using System.Linq;


namespace Parser
{
    class Program
    {
        static void Main()
        {
            string name = @"C:\Users\N\Downloads\repl\";
            string[] replays = Directory.GetFiles(name);
            foreach(string replay in replays)
            {
                get_winner(File.ReadAllBytes(replay), "StaZzz");
            }
        }

        public static byte get_winner(byte[] replay, string player_id, string month, string day, string[] players_arr, WebClient client, string rep_name)
        {
            byte pos_player = 1;
            byte pos_opponent = 1;
            byte pos = 1;
            string[] rep = System.Text.Encoding.Default.GetString(replay).Split("O=N;")[1].Split(":;")[0].Split(":");
            string opp_name = "";
            // string player_name = "";
            for (int i = 0; i < rep.Length; i++)
            {
                if (rep[i].Contains("X") || rep[i] == "")
                    continue;
                pos += 1;
                if (!rep[i].Contains(",-2,"))
                {
                    if (rep[i].Contains(player_id))
                    {
                        pos_player = pos;
                        //player_name = rep[i].Split("H")[1].Split(",")[0];
                    }
                    else
                    {
                        pos_opponent = pos;
                        opp_name = rep[i].Split("H")[1].Split(",")[0].Replace("`", "").Replace("\"", "").Replace("%", "").Replace("^", "").Replace("{", "").Replace("}", "").Replace("\\", "").Replace("|", "").Replace("/", "");
                    }

                }
            }

            int[] player_lose = Locate(replay, StringToByteArray("00 45 04 00 00 0" + pos_player));
            int[] opp_lose = Locate(replay, StringToByteArray("00 45 04 00 00 0" + pos_opponent));

            if (player_lose.Length == 2)
                return 2; // p surr
            if (opp_lose.Length == 2 && player_lose.Length == 1)
                return 1; // o surr
            foreach (string player in players_arr)
            {
                if (player.Contains("<a href=\"" + opp_name + "_"))
                {
                    try
                    {
                        string path = gentoolurl + month + day + player.Split("<a href=\"")[1].Split("\">")[0] + rep_name;
                        byte[] opp_rep = client.DownloadData(path);
                        int[] opp_player_lose = Locate(opp_rep, StringToByteArray("00 45 04 00 00 0" + pos_player));
                        int[] opp_opp_lose = Locate(opp_rep, StringToByteArray("00 45 04 00 00 0" + pos_opponent));
                        if (opp_opp_lose.Length == 2)
                            return 1; // o surr
                        if (player_lose.Length == 1 && opp_lose.Length == 0 && opp_player_lose.Length != 0 && opp_opp_lose.Length == 1)
                            return 2; // p exit
                        if (player_lose.Length == 1 && opp_lose.Length == 1 && opp_opp_lose.Length == 1 && opp_player_lose.Length == 0)
                            return 1; // o exit 

                    }
                    catch { }
                }
            }
            return 0;

        } // 1 = win 

        static byte[] StringToByteArray(string hex) // converting String HEX to byte[]
        {
            hex = hex.Replace(" ", "");
            hex = hex.Replace("-", "");
            return Enumerable.Range(0, hex.Length)
                             .Where(x => x % 2 == 0)
                             .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
                             .ToArray();
        }


        static readonly int[] Empty = new int[0];

        static int[] Locate(byte[] self, byte[] candidate)
        {
            if (IsEmptyLocate(self, candidate))
                return Empty;

            var list = new List<int>();

            for (int i = 0; i < self.Length; i++)
            {
                if (!IsMatch(self, i, candidate))
                    continue;

                list.Add(i);
            }

            return list.Count == 0 ? Empty : list.ToArray();
        }


        static bool IsMatch(byte[] array, int position, byte[] candidate)
        {
            if (candidate.Length > (array.Length - position))
                return false;

            for (int i = 0; i < candidate.Length; i++)
                if (array[position + i] != candidate[i])
                    return false;

            return true;
        }


        static bool IsEmptyLocate(byte[] array, byte[] candidate)
        {
            return array == null
                | candidate == null
                | array.Length == 0
                | candidate.Length == 0
                | candidate.Length > array.Length;
        }
    }
    
}

