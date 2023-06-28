using System;
using System.Collections.Generic;
using System.Text;

namespace ParserBot
{
    public class Player
    {
        public string gentoolId;
        public string pcInfo;
        public ulong discordId;

        public Player(string gentoolId, string pcInfo, ulong discordId)
        {
            this.gentoolId = gentoolId;
            this.pcInfo = pcInfo;
            this.discordId = discordId;
        }
    }
}
