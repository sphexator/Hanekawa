using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hanekawa.Bot.Services.Club;
using Hanekawa.Core.Interactive;

namespace Hanekawa.Bot.Modules.Club
{
    public partial class Club : InteractiveBase
    {
        private readonly ClubService _club;

        public Club(ClubService club) => _club = club;


    }
}