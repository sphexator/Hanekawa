using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Hanekawa.Bot.Preconditions;
using Hanekawa.Core.Interactive;
using Qmmands;

namespace Hanekawa.Bot.Modules.Report
{
    [Name("Report")]
    [RequireBotPermission(GuildPermission.EmbedLinks)]
    public class Report : InteractiveBase
    {

    }
}
