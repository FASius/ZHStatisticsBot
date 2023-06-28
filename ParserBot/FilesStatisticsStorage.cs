using System;
using System.IO;
using System.Numerics;
using System.Text.Json;

namespace ParserBot
{
    public class FilesStatisticsStorage : StatisticsStorage
    {
        public PlayerStats getStats(int year, int month, int day, Player player)
        {
            var path = getPath(year, month, day, player);
            if (!File.Exists(path))
            {
                return null;
            }
            var file = File.ReadAllText(path);
            return getStats(file);
        }

        private PlayerStats getStats(string file)
        {
            return JsonSerializer.Deserialize<PlayerStats>(file);
        }

        private string getPath(int year, int month, int day, Player player)
        {
            var path = $"data/{year}/{month}/{day}/{player.gentoolId}.txt";
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            return path;
        }

        public void saveStats(int year, int month, int day, Player player, PlayerStats stats)
        {
            var path = getPath(year, month, day, player);
            var json = JsonSerializer.Serialize(stats);
            File.WriteAllText(path, json);
        }

        public PlayerStats getStats(int year, int month, Player player)
        {
            var startDate = new DateTime(year, month, 1);
            var endDate = startDate.AddMonths(1);

            var allStat = new PlayerStats();
            for (var date = startDate; date < endDate; date = date.AddDays(1))
            {
                var stat = getStats(date.Year, date.Month, date.Day, player);
                if (stat != null)
                {
                    allStat += stat;
                }
            }
            return allStat;
        }
    }
}
