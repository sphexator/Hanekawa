using System.Threading.Tasks;
using Disqord;
using Disqord.Bot;
using Hanekawa.Bot.Commands.Preconditions;
using Hanekawa.Bot.Service.Experience;
using Hanekawa.Bot.Service.ImageGeneration;
using Qmmands;

namespace Hanekawa.Bot.Commands.Modules.Account
{
    [Name("Account")]
    [Description("Commands for user levels")]
    [RequireBotGuildPermissions(Permission.EmbedLinks | Permission.AttachFiles | Permission.SendMessages)]
    public class Account : HanekawaCommandModule
    {
        private readonly ExpService _exp;
        private readonly ImageGenerationService _image;

        public Account(ImageGenerationService image, ExpService exp)
        {
            _image = image;
            _exp = exp;
        }

        [Name("Rank")]
        [Command("rank")]
        [Description("Display your rank, level and exp within the server, also global rank")]
        [Cooldown(1, 1, CooldownMeasure.Seconds, CooldownBucketType.Member)]
        [RequiredChannel]
        public async Task RankAsync(IMember user = null)
        {
            
        }

        [Name("Profile")]
        [Command("profile")]
        [Description("Showcase yours or another persons profile")]
        [Cooldown(1, 5, CooldownMeasure.Seconds, CooldownBucketType.Member)]
        [RequiredChannel]
        public async Task ProfileAsync(IMember user = null)
        {
            
        }

        [Name("Level Leaderboard")]
        [Command("top", "leaderboard", "lb")]
        [Description("Displays highest ranked users")]
        [Cooldown(1, 1, CooldownMeasure.Seconds, CooldownBucketType.Member)]
        [RequiredChannel]
        public async Task LeaderboardAsync(int amount = 100)
        {
            
        }

        [Name("Reputation")]
        [Command("rep")]
        [Description("Rewards a reputation to a user. Usable once a day")]
        [Cooldown(1, 1, CooldownMeasure.Seconds, CooldownBucketType.Member)]
        [Remarks("rep @bob#0000")]
        [RequiredChannel]
        public async Task ReputationAsync(IMember user = null)
        {
            
        }
    }
}