using System.Threading.Tasks;
using Hanekawa.Bot.Preconditions;
using Qmmands;

namespace Hanekawa.Bot.Modules.Club
{
    public partial class Club
    {
        [Name("Club Name")]
        [Command("clubname", "cn")]
        [Description("Changes club name")]
        [RequiredChannel]
        public async Task ClubNameChangeAsync([Remainder] string content) =>
            await _club.AdNameAsync(Context, content);

        [Name("Club Description")]
        [Command("clubdesc", "cd")]
        [Description("Sets description of a club")]
        [RequiredChannel]
        public async Task ClubDescriptionAsync([Remainder] string content) =>
            await _club.AdDescAsync(Context, content);

        [Name("Club Image")]
        [Command("clubimage", "clubpic", "cimage", "ci")]
        [Description("Sets a picture to a club")]
        [RequiredChannel]
        public async Task ClubImageAsync(string image) => await _club.AdImageAsync(Context, image);

        [Name("Club Icon")]
        [Command("clubicon", "cicon")]
        [Description("Sets a icon to a club")]
        [RequiredChannel]
        public async Task ClubIconAsync(string image) => await _club.AdIconAsync(Context, image);

        [Name("Club Public")]
        [Command("clubpublic", "cpublic")]
        [Description("Toggles a club to be public or not")]
        [RequiredChannel]
        public async Task ClubPublicAsync() => await _club.AdPublicAsync(Context);

        [Name("Club Advertise")]
        [Command("clubadvertise", "clubpost", "cpost")]
        [Description("Posts a advertisement of club to designated advertisement channel")]
        [RequiredChannel]
        public async Task ClubAdvertiseAsync() => await _club.AdAdvertiseAsync(Context);
    }
}