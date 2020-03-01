using System.Threading.Tasks;
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
        public async Task ProfileAsync()
        {
            var user = Context.User;
            //if (user == null) user = Context.User;
            await Context.Channel.TriggerTypingAsync();
            using var db = new DbService();
            using var image = await _image.ProfileBuilder(user, db);
            image.Position = 0;
            await Context.Channel.SendFileAsync(image, "profile.png", null);
        }
    }
}