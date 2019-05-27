using System.Threading.Tasks;
using Hanekawa.Bot.Preconditions;
using Qmmands;

namespace Hanekawa.Bot.Modules.Club
{
    public partial class Club
    {
        [Name("Club name")]
        [Command("club name", "club name")]
        [Description("Changes club name")]
        [Remarks("club name Google Town")]
        [RequiredChannel]
        public async Task ClubNameChangeAsync([Remainder] string content) => 
            await _club.AdNameAsync(Context, content);

        [Name("Club description")]
        [Command("club description", "club desc")]
        [Description("Sets description of a club")]
        [Remarks("club desc this is a description")]
        [RequiredChannel]
        public async Task ClubDescriptionAsync([Remainder] string content) =>
            await _club.AdDescAsync(Context, content);

        [Name("Club image")]
        [Command("club image", "club pic", "cimage")]
        [Description("Sets a picture to a club")]
        [Remarks("club pic https://i.imgur.com/p3Xxvij.png")]
        [RequiredChannel]
        public async Task ClubImageAsync(string image) => await _club.AdImageAsync(Context, image);

        [Name("Club icon")]
        [Command("club icon", "cicon")]
        [Description("Sets a icon to a club")]
        [Remarks("club icon https://i.imgur.com/p3Xxvij.png")]
        [RequiredChannel]
        public async Task ClubIconAsync(string image) => await _club.AdIconAsync(Context, image);

        [Name("Club public")]
        [Command("club public")]
        [Description("Toggles a club to be public or not")]
        [Remarks("club public")]
        [RequiredChannel]
        public async Task ClubPublicAsync() => await _club.AdPublicAsync(Context);

        [Name("Club advertise")]
        [Command("club advertise", "club post")]
        [Description("Posts a advertisement of club to designated advertisement channel")]
        [Remarks("club advertise")]
        [RequiredChannel]
        public async Task ClubAdvertiseAsync() => await _club.AdAdvertiseAsync(Context);
    }
}
