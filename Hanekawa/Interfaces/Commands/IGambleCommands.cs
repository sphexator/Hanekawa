namespace Hanekawa.Interfaces.Commands
{
    public interface IGambleCommands
    {
        int Roll(ulong guildId, ulong userId, int bet, int max);
        int Bet(ulong guildId, ulong userId, int bet, int max);
    }
}