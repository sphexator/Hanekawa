using Discord.Addons.Interactive;
using Discord.Commands;
using Dropbox.Api;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jibril.Modules.Test
{
    public class Dropbox : InteractiveBase
    {
        [Command("test", RunMode = RunMode.Async)]
        [RequireOwner]
        public async Task SendNudes()
        {

        }
    }
}
