using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;

namespace ParserBot
{
    public class GentoolParser
    {
        private WebClient client;
        private static string BASE_URL = "https://gentool.net/data/zh/";
        private static CultureInfo language = new CultureInfo("en-US");

        public GentoolParser(WebClient client) 
        { 
            this.client = client; 
        }

        public Dictionary<Player, string> ParseDay(DateTime date, Player[] players)
        {
            var url = BASE_URL + date.ToString("yyyy_MM_MMMM/", language) + date.ToString("dd_dddd/", language);
            return ParsePlayers(url, players);
        }

        public Dictionary<Player, string> ParsePlayers(string url, Player[] players)
        {
            string webpageWithPlayers = client.DownloadString(url);
            string[] allPlayers = webpageWithPlayers.Split("\n");
            var replaysTxts = new Dictionary<Player, string>();
            for (int i = 0; i < allPlayers.Length; i++)
            {
                for (var j = 0; j < players.Length; ++j)
                {
                    if (allPlayers[i].Contains(players[j].gentoolId))
                    {
                        string playerFolder = allPlayers[i].Remove(0, allPlayers[i].IndexOf("<a href=") + "<a href=".Length + 1);
                        playerFolder = playerFolder.Remove(playerFolder.IndexOf("\">"));
                        string playerPage = client.DownloadString(url + playerFolder);
                        string[] playerFiles = playerPage.Split("\n");
                        for (int k = 0; k < playerFiles.Length; k++)
                        {
                            if (playerFiles[k].Contains(".txt"))
                            {
                                string txtName = playerFiles[k].Remove(0, playerFiles[k].IndexOf("<a href=") + "<a href=".Length + 1);
                                txtName = txtName.Remove(txtName.IndexOf(">") - 1);
                                string txtFile = client.DownloadString(url + playerFolder + txtName);
                                if (txtFile.Contains(players[j].pcInfo))
                                {
                                    replaysTxts.Add(players[j], txtFile);
                                }
                            }
                        }
                    }
                }
            }
            return replaysTxts;
        }

    }
}
