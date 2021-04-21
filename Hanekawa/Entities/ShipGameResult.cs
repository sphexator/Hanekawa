using System.Collections.Generic;
using Disqord;

namespace Hanekawa.Entities
{
    public class ShipGameResult
    {
        public ShipGameResult(ShipGame game, ShipUser user, IUserMessage message)
        {
            Winner = user;
            ExpGain = game.Exp;
            CreditGain = game.Credit;
            Bet = game.Bet;
            Message = message;
        }
        
        public ShipUser Winner { get; set; }
        public int? ExpGain { get; set; }
        public int? CreditGain { get; set; }
        public int? Bet { get; set; }
        public IUserMessage Message { get; set; }
        public LinkedList<string> Log { get; set; }
    }
}