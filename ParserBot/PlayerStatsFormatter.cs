using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ParserBot
{
    class PlayerStatsFormatter
    {
        private PlayerStats stats;
        private static int MAX_NICKNAME_LENGTH = 15;
        private static string DATE_FORMAT = @"hh\:mm\:ss";

        public PlayerStatsFormatter(PlayerStats stats) { this.stats = stats; }

        public string formatByGameMode(string gameMode)
        {
            var stat = stats.GetPlayerStats(gameMode);
            if (stat == null)
                return null;
            var sb = new StringBuilder();

            sb.Append("Nicknames   , Times" + Environment.NewLine + "------------,-------" + Environment.NewLine);
            var nicknames = stat.nicknames.OrderByDescending(pair => pair.Value);
            formatValues(sb, nicknames, MAX_NICKNAME_LENGTH, "Times played: ");

            var maps = stat.maps.OrderByDescending(pair => pair.Value.duration);
            var pad = maps.Max(pair => pair.Key.Length);
            sb.Append(Environment.NewLine + "Maps".PadRight(pad) + ",Times,   Time" + Environment.NewLine + "-".PadRight(pad, '-') + ",-----,----------" + Environment.NewLine);
            formatValues(sb, maps, pad, 4, "|");

            sb.Append(Environment.NewLine + "Nickname".PadRight(MAX_NICKNAME_LENGTH) + ",Times, Time" + Environment.NewLine + "-".PadRight(MAX_NICKNAME_LENGTH, '-') + ",-----,----------" + Environment.NewLine);
            var opponents = stat.opponents.OrderByDescending(pair => pair.Value.duration);
            formatValues(sb, opponents, MAX_NICKNAME_LENGTH, 4, "|");

            sb.Append(Environment.NewLine + "Total Time: " + TimeSpan.FromSeconds(Convert.ToDouble(stat.totalTime)).ToString(DATE_FORMAT) + " Total Times: " + stat.timesPlayed + Environment.NewLine + "*****" + Environment.NewLine);
            return sb.ToString();
        }

        private void formatValues(StringBuilder sb, IOrderedEnumerable<KeyValuePair<string, int>> values, int padValue, string delimiter = "")
        {
            foreach (var pair in values)
            {
                sb.Append(pair.Key.PadRight(padValue) + delimiter + pair.Value + Environment.NewLine);
            }
        }

        private void formatValues(StringBuilder sb, IOrderedEnumerable<KeyValuePair<string, DurationCountStats>> values, int pad1, int pad2, string delimiter = "")
        {
            foreach (var pair in values)
            {
                sb.Append(pair.Key.PadRight(pad1) + delimiter);
                sb.Append(pair.Value.count.ToString().PadRight(pad2) + delimiter);
                var duration = TimeSpan.FromSeconds(Convert.ToDouble(pair.Value.duration)).ToString(DATE_FORMAT);
                sb.Append(duration + Environment.NewLine);
            }
        }

    }

}