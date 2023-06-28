using Microsoft.VisualBasic;

namespace ParserBot
{
    public interface StatisticsStorage
    {
        public PlayerStats getStats(int year, int month, int day, Player player);

        public PlayerStats getStats(int year, int month, Player player);

        public void saveStats(int year, int month, int day, Player player, PlayerStats stats);
    }
}
