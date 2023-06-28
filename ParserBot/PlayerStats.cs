using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace ParserBot
{
    public class PlayerStats
    {
        private readonly Dictionary<string, GameModePlayerStats> playerStatsByGameModes;

        public PlayerStats() 
        {
            playerStatsByGameModes = new Dictionary<string, GameModePlayerStats>();
        }

        public PlayerStats(MatchStats stat)
        {
            playerStatsByGameModes = new Dictionary<string, GameModePlayerStats>();
            GameModePlayerStats stats = new GameModePlayerStats();
            stats.Add(stat.ownerNickname, stat.mapName, stat.duration, stat.opponents.ToList());
            playerStatsByGameModes.Add(stat.matchMode, stats);
        }

        public GameModePlayerStats GetPlayerStats(string gameMode)
        {
            if (!playerStatsByGameModes.ContainsKey(gameMode))
            {
                return null;
            }
            return playerStatsByGameModes[gameMode];
        }

        public List<KeyValuePair<string, GameModePlayerStats>> GetAllStat()
        {
            return playerStatsByGameModes.ToList();
        }

        public void AddGameModeStats(GameModePlayerStats stat, string gameMode)
        {
            if (!playerStatsByGameModes.ContainsKey(gameMode))
            {
                playerStatsByGameModes[gameMode] = stat;
            }
            else
            {
                playerStatsByGameModes[gameMode] += stat;
            }
        }

        public static PlayerStats operator +(PlayerStats first, PlayerStats second)
        {
            var stat = new PlayerStats();
            foreach (var pair in first.playerStatsByGameModes)
            {
                stat.AddGameModeStats(pair.Value, pair.Key);
            }
            foreach (var pair in second.playerStatsByGameModes)
            {
                stat.AddGameModeStats(pair.Value, pair.Key);
            }
            return stat;
        } 

        public static PlayerStats operator +(PlayerStats playerStats, MatchStats matchStat)
        {
            if (playerStats.playerStatsByGameModes.ContainsKey(matchStat.matchMode))
            {
                playerStats.playerStatsByGameModes[matchStat.matchMode].Add(matchStat.ownerNickname, matchStat.mapName, matchStat.duration, matchStat.opponents.ToList());
            }
            else
            {
                var stat = new GameModePlayerStats();
                stat.Add(matchStat.ownerNickname, matchStat.mapName, matchStat.duration, matchStat.opponents.ToList());
                playerStats.playerStatsByGameModes[matchStat.matchMode] = stat;

            }
            return playerStats;
        }
    }
}
