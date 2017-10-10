using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Text;
using Jibril.Preconditions;
using Discord;
using System.Threading.Tasks;
using Jibril.Services;
using System.Linq;
using Jibril.Services.Common;
using Jibril.Data.Variables;

namespace Jibril.Modules.Gambling
{
    public class Currency : ModuleBase<SocketCommandContext>
    {
        [Command("wallet")]
        [Alias("balance", "money")]
        [RequiredChannel(339383206669320192)]
        public async Task Wallet()
        {
            var user = Context.User;
            var userData = DatabaseService.UserData(user).FirstOrDefault();
            var embed = EmbedGenerator.AuthorEmbed($"Credit: ${userData.Tokens}\n" +
                $"Event Tokens: {userData.Event_tokens}", user.Mention, Colours.DefaultColour, user);
            await ReplyAsync("", false, embed.Build()).ConfigureAwait(false);
        }

        [Command("wallet")]
        [Alias("balance", "money")]
        [RequiredChannel(339383206669320192)]
        public async Task Wallet(IGuildUser user)
        {
            var userData = DatabaseService.UserData(user).FirstOrDefault();
            var embed = EmbedGenerator.AuthorEmbed($"Credit: ${userData.Tokens}\n" +
                $"Event Tokens: {userData.Event_tokens}", user.Mention, Colours.DefaultColour, user);
            await ReplyAsync("", false, embed.Build()).ConfigureAwait(false);
        }
        /* 
        [Command("Richest")]
        [RequiredChannel(339383206669320192)]
        public async Task Richest()
        {

        }
        */
    }
}
