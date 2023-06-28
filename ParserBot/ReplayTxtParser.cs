using System;

namespace ParserBot
{
    public class ReplayTxtParser
    {
        private readonly int minMatchLengthSeconds;

        public ReplayTxtParser(int minMatchLengthSeconds)
        {
            this.minMatchLengthSeconds = minMatchLengthSeconds;
        }

        public MatchStats Parse(string txt)
        {
            var nickname = txt.Split("Player Name:")[1].Split("\nPlayer Id:")[0].Trim();
            var length = (int)TimeSpan.Parse(txt.Split("Match Length:")[1].Split("\nMatch Mode:")[0].Trim()).TotalSeconds;

            if (length < minMatchLengthSeconds || IsPlayerObserver(txt, nickname))
                return null;
            var mapName = txt.Split("Map Name:")[1].Split("\nStart Cash:")[0].Trim().Replace("maps/", "");
            var matchType = txt.Split("Match Type:")[1].Split("\nMatch Length:")[0].Trim();
            var playersTxt = txt.Split("Match Mode:")[1].Split("Associated files:")[0].Trim();
            var players = playersTxt.Split("\n");
            var opponents = new string[players.Length];
            for (var i = 0; i < players.Length; ++i)
            {
                var line = players[i];
                if (line.Contains("000 ") && !line.Contains(" (Observer)") && !line.Contains("000 " + nickname + " ("))
                    opponents[i] = (line.Remove(0, 12).Remove(line.IndexOf("(") - 13));

            }
            return new MatchStats(nickname, opponents, length, mapName, matchType);
        }

        private bool IsPlayerObserver(string txt, string nickname)
        {
            return txt.Contains("000 " + nickname + " (Observer)");
        }

    }

}
