using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Hanekawa.Preconditions;
using Hanekawa.Services.Giphy;
using System.Threading.Tasks;
using Hanekawa.Extensions;

namespace Hanekawa.Modules.Fun
{
    [RequireContext(ContextType.Guild)]
    public class FunCommands : InteractiveBase
    {
        private readonly GiphyService _service;
        public FunCommands(GiphyService service)
        {
            _service = service;
        }

        [Command("holdhands")]
        [RequiredChannel]
        public async Task HoldhandsAsync(IGuildUser user)
        {
            var image = await _service.GetImage(FunCommandType.HoldHands);
            string speech = null;
            if (user != null) speech = $"**{(Context.User as IGuildUser).GetName()}** holds **{user.GetName()}** hands~";
            await Context.Channel.SendFileAsync(image, "HoldHands.gif", speech);
        }

        [Command("teehee")]
        [RequiredChannel]
        public async Task TeeheeAsync()
        {
            var image = await _service.GetImage(FunCommandType.Teehee);
            string speech = null;
            await Context.Channel.SendFileAsync(image, "Teehee.gif", speech);
        }

        [Command("stare")]
        [RequiredChannel]
        public async Task StareAsync(IGuildUser user = null)
        {
            var image = await _service.GetImage(FunCommandType.Stare);
            string speech = null;
            if (user != null) speech = $"**{(Context.User as IGuildUser).GetName()}** stares at **{user.GetName()}**~";
            await Context.Channel.SendFileAsync(image, "Stare.gif", speech);
        }

        [Command("smile")]
        [RequiredChannel]
        public async Task SmileAsync(IGuildUser user = null)
        {
            var image = await _service.GetImage(FunCommandType.Smile);
            string speech = null;
            if (user != null) speech = $"**{(Context.User as IGuildUser).GetName()}** smiles at **{user.GetName()}**~";
            await Context.Channel.SendFileAsync(image, "Smile.gif", speech);
        }

        [Command("cuddle")]
        [RequiredChannel]
        public async Task CuddleAsync(IGuildUser user = null)
        {
            var image = await _service.GetImage(FunCommandType.Cuddle);
            string speech = null;
            if (user != null) speech = $"**{(Context.User as IGuildUser).GetName()}** cuddles **{user.GetName()}**~";
            await Context.Channel.SendFileAsync(image, "Cuddle.gif", speech);
        }

        [Command("facedesk")]
        [RequiredChannel]
        public async Task FacedeskAsync()
        {
            var image = await _service.GetImage(FunCommandType.Facedesk);
            string speech = null;
            await Context.Channel.SendFileAsync(image, "Facedesk.gif", speech);
        }

        [Command("nuzzle")]
        [RequiredChannel]
        public async Task NuzzleAsync(IGuildUser user = null)
        {
            var image = await _service.GetImage(FunCommandType.Nuzzle);
            string speech = null;
            if (user != null) speech = $"**{(Context.User as IGuildUser).GetName()}** nuzzles **{user.GetName()}**~";
            await Context.Channel.SendFileAsync(image, "Nuzzle.gif", speech);
        }

        [Command("pout")]
        [RequiredChannel]
        public async Task PoutAsync(IGuildUser user = null)
        {
            var image = await _service.GetImage(FunCommandType.Pout);
            string speech = null;
            if (user != null) speech = $"**{(Context.User as IGuildUser).GetName()}** pouts at **{user.GetName()}**~";
            await Context.Channel.SendFileAsync(image, "Pout.gif", speech);
        }

        [Command("tsundere")]
        [RequiredChannel]
        public async Task TsundereAsync()
        {
            var image = await _service.GetImage(FunCommandType.Tsundere);
            string speech = null;
            await Context.Channel.SendFileAsync(image, "Tsundere.gif", speech);
        }

        [Command("greet")]
        [RequiredChannel]
        public async Task GreetAsync(IGuildUser user = null)
        {
            var image = await _service.GetImage(FunCommandType.Greet);
            string speech = null;
            if (user != null) speech = $"**{(Context.User as IGuildUser).GetName()}** greets **{user.GetName()}**~";
            await Context.Channel.SendFileAsync(image, "Greet.gif", speech);
        }

        [Command("meow")]
        [RequiredChannel]
        public async Task MeowAsync(IGuildUser user = null)
        {
            var image = await _service.GetImage(FunCommandType.Meow);
            string speech = null;
            if (user != null) speech = $"**{(Context.User as IGuildUser).GetName()}** meows at **{user.GetName()}**~";
            await Context.Channel.SendFileAsync(image, "Meow.gif", speech);
        }

        [Command("lewd")]
        [RequiredChannel]
        public async Task LewdAsync(IGuildUser user)
        {
            var image = await _service.GetImage(FunCommandType.Lewd);
            string speech = null;
            if (user != null) speech = $"**{user.GetName()}**, how lewd~";
            await Context.Channel.SendFileAsync(image, "Lewd.gif", speech);
        }

        [Command("highfive")]
        [RequiredChannel]
        public async Task HighfiveAsync(IGuildUser user = null)
        {
            var image = await _service.GetImage(FunCommandType.Highfive);
            string speech = null;
            if (user != null) speech = $"**{(Context.User as IGuildUser).GetName()}** high fives **{user.GetName()}**~";
            await Context.Channel.SendFileAsync(image, "Highfive.gif", speech);
        }

        [Command("tickle")]
        [RequiredChannel]
        public async Task TickleAsync(IGuildUser user = null)
        {
            var image = await _service.GetImage(FunCommandType.Tickle);
            string speech = null;
            if (user != null) speech = $"**{(Context.User as IGuildUser).GetName()}** tickles **{user.GetName()}**~";
            await Context.Channel.SendFileAsync(image, "Tickle.gif", speech);
        }

        [Command("slap")]
        [RequiredChannel]
        public async Task SlapAsync(IGuildUser user = null)
        {
            var image = await _service.GetImage(FunCommandType.Slap);
            string speech = null;
            if (user != null) speech = $"**{(Context.User as IGuildUser).GetName()}** slaps **{user.GetName()}**~";
            await Context.Channel.SendFileAsync(image, "Slap.gif", speech);
        }

        [Command("pat")]
        [RequiredChannel]
        public async Task PatAsync(IGuildUser user = null)
        {
            var image = await _service.GetImage(FunCommandType.Pat);
            string speech = null;
            if (user != null) speech = $"**{(Context.User as IGuildUser).GetName()}** pats **{user.GetName()}**~";
            await Context.Channel.SendFileAsync(image, "Pat.gif", speech);
        }

        [Command("hug")]
        [RequiredChannel]
        public async Task HugAsync(IGuildUser user = null)
        {
            var image = await _service.GetImage(FunCommandType.Hug);
            string speech = null;
            if (user != null) speech = $"**{(Context.User as IGuildUser).GetName()}** hugs **{user.GetName()}**~";
            await Context.Channel.SendFileAsync(image, "Hug.gif", speech);
        }

        [Command("kiss")]
        [RequiredChannel]
        public async Task KissAsync(IGuildUser user = null)
        {
            var image = await _service.GetImage(FunCommandType.Kiss);
            string speech = null;
            if (user != null) speech = $"**{(Context.User as IGuildUser).GetName()}** kisses **{user.GetName()}** <3 ~";
            await Context.Channel.SendFileAsync(image, "Kiss.gif", speech);
        }

        [Command("bloodsuck")]
        [RequiredChannel]
        public async Task BloodSuckAsync(IGuildUser user = null)
        {
            var image = await _service.GetImage(FunCommandType.Bloodsuck);
            string speech = null;
            if (user != null) speech = $"**{(Context.User as IGuildUser).GetName()}** sucks blood of **{user.GetName()}**~";
            await Context.Channel.SendFileAsync(image, "Bloodsuck.gif", speech);
        }

        [Command("bite")]
        [RequiredChannel]
        public async Task BiteAsync(IGuildUser user = null)
        {
            var image = await _service.GetImage(FunCommandType.Bite);
            string speech = null;
            if (user != null) speech = $"**{(Context.User as IGuildUser).GetName()}** bites **{user.GetName()}**~";
            await Context.Channel.SendFileAsync(image, "Bite.gif", speech);
        }

        [Command("nom")]
        [RequiredChannel]
        public async Task NomAsync(IGuildUser user = null)
        {
            var image = await _service.GetImage(FunCommandType.Nom);
            string speech = null;
            if (user != null) speech = $"**{(Context.User as IGuildUser).GetName()}** noms **{user.GetName()}**~";
            await Context.Channel.SendFileAsync(image, "Nom.gif", speech);
        }

        [Command("poke")]
        [RequiredChannel]
        public async Task PokeAsync(IGuildUser user = null)
        {
            var image = await _service.GetImage(FunCommandType.Poke);
            string speech = null;
            if (user != null) speech = $"**{(Context.User as IGuildUser).GetName()}** pokes **{user.GetName()}**~";
            await Context.Channel.SendFileAsync(image, "Poke.gif", speech);
        }
    }
}
