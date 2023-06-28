using System.Collections.Generic;

namespace ParserBot
{
    public interface PlayersStorage
    {
        public void AddPlayerToStorage(Player player);

        public List<Player> GetPlayers();

        public Player GetPlayer(ulong discordId);
    }
}
