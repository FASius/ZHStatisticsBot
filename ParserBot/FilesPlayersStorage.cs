using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;

namespace ParserBot
{
    public class FilesPlayersStorage : PlayersStorage
    {
        private static readonly string path = "players.txt";
        private static readonly object locker = new object();

        public void AddPlayerToStorage(Player player)
        {
            lock (locker)
            {
                var file = File.ReadAllText(path);
                var players = JsonSerializer.Deserialize<List<Player>>(file);
                players.Add(player);
                var json = JsonSerializer.Serialize(players);
                var sw = File.AppendText(path);
                sw.Write(json);
                sw.Flush();
                sw.Close();
            }
        }

        public Player GetPlayer(ulong discordId)
        {
            string json;
            lock (locker)
            {
                json = File.ReadAllText(path);
            }
            var players = JsonSerializer.Deserialize<List<Player>>(json);
            return players.FirstOrDefault(player => player.discordId == discordId);
        }

        public List<Player> GetPlayers()
        {
            string json;
            lock (locker)
            {
                json = File.ReadAllText(path);
            }
            var players = JsonSerializer.Deserialize<List<Player>>(json);
            return players;
        }
    }
}
