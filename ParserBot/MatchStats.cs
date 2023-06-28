using System;
using System.Collections.Generic;
using System.Text;

namespace ParserBot
{
    public class MatchStats
    {
        public string ownerNickname;
        public string[] opponents;
        public int duration;
        public string mapName;
        public string matchMode;

        public MatchStats(string ownerNickname, string[] opponents, int matchLengthSeconds, string mapName, string matchMode)
        {
            this.ownerNickname = ownerNickname;
            this.opponents = opponents;
            this.duration = matchLengthSeconds;
            this.mapName = mapName;
            this.matchMode = matchMode;
        }
    }
}
