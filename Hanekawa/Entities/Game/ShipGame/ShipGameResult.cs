using System.Collections.Generic;
using Hanekawa.Interfaces;

namespace Hanekawa.Entities.Game.ShipGame
{
    public class ShipGameResult
    {
        public ShipGameResult(ShipGame game, ShipUser user, IMessage message)
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
        public IMessage Message { get; set; }
        public LinkedList<string> Log { get; set; }
    }
}