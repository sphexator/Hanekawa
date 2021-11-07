using Hanekawa.Interfaces.Commands;

namespace Hanekawa.Application.Commands
{
    public class Gamble : IGambleCommands
    {
        public int Roll(ulong guildId, ulong userId, int bet, int max)
        {
            throw new System.NotImplementedException();
        }

        public int Bet(ulong guildId, ulong userId, int bet, int max)
        {
            throw new System.NotImplementedException();
        }
    }
}