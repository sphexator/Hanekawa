using System.Threading.Tasks;
using Disqord;
using Hanekawa.Bot.Preconditions;
using Hanekawa.Database;
using Microsoft.Extensions.DependencyInjection;
using Qmmands;

namespace Hanekawa.Bot.Modules.Account
{
    public partial class Account
    {
        [Name("Profile")]
        [Command("profile")]
        [Description("Showcase yours or another persons profile")]
        [RequiredChannel]
        public async Task ProfileAsync(CachedMember user = null)
        {
            if (user == null) user = Context.Member;
            await Context.Channel.TriggerTypingAsync();
            await using var db = Context.Scope.ServiceProvider.GetRequiredService<DbService>();
            await using var image = await _image.ProfileBuilder(user, db);
            image.Position = 0;
            await Context.Channel.SendMessageAsync(new LocalAttachment(image, "profile.png"));
        }
    }
}