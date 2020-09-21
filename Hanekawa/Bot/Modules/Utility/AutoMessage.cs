using System;
using System.Threading.Tasks;
using Disqord;
using Disqord.Bot;
using Hanekawa.Bot.Services.AutoMessage;
using Hanekawa.Database;
using Hanekawa.Shared.Command;
using Hanekawa.Shared.Command.Extensions;
using Humanizer;
using Microsoft.Extensions.DependencyInjection;
using Qmmands;

namespace Hanekawa.Bot.Modules.Utility
{
    [Name("Auto Message")]
    [Description("Create a message that'll be posted automatically within a interval (ie. every hour)")]
    [RequireMemberGuildPermissions(Permission.ManageGuild)]
    public class AutoMessage : HanekawaCommandModule
    {
        private readonly AutoMessageService _service;
        public AutoMessage(AutoMessageService service) => _service = service;

        [Name("Add Auto Message")]
        [Description("Create a automated message with a interval (ie. 1h30m). You can have up to 3 active messages")]
        [Command("amadd")]
        public async Task AddAsync(string name, CachedTextChannel channel, TimeSpan interval, [Remainder] string message)
        {
            if (await _service.AddAutoMessage(Context.Member, channel, interval, name, message))
                await ReplyAsync(
                    $"Succesfully added a message by name '{name}' that'll be sent every {interval.Humanize()}",
                    Color.Green);
            else
                await ReplyAsync(
                    $"Couldn't add, you possibly already have 3 active messages, or there's already a message with this name ({name})",
                    Color.Red);
        }

        [Name("Remove Auto Message")]
        [Description("Removes a automated message by its name")]
        [Command("amremove")]
        public async Task RemoveAsync(string name)
        {
            await using var db = Context.Scope.ServiceProvider.GetRequiredService<DbService>();
            if (_service.RemoveAutoMessage(Context.Guild.Id.RawValue, name, db))
                await ReplyAsync($"Removed auto message by the name {name} !", Color.Green);
            else await ReplyAsync($"Couldn't find or remove a auto message by the name {name}", Color.Green);
        }

        [Name("List Auto Messages")]
        [Description("List all active automated messages (up to 3)")]
        [Command("amlist")]
        public async Task ListAsync()
        {
            var list = _service.GetList(Context.Guild.Id.RawValue);
            await Context.PaginatedReply(list, Context.Guild,
                $"Automated messages in {Context.Guild.Name}!");
        }

        [Name("Edit existing message with different message")]
        [Description("Edit the content of a automated message by providing a new one")]
        [Command("amedit")]
        public async Task EditContentAsync(string name, [Remainder] string editedMessage)
        {
            await using var db = Context.Scope.ServiceProvider.GetRequiredService<DbService>();
            if (await _service.EditMessageAsync(Context.Guild.Id.RawValue, name, editedMessage, db))
                await ReplyAsync($"Changed message to '{editedMessage}' !", Color.Green);
            else
                await ReplyAsync($"Couldn't find a automated message by that name ({name}), or something went wrong...",
                    Color.Red);
        }

        [Name("Edit existing message with different interval")]
        [Description("Change the interval a message is being posted, ie. from 1hr to 3hrs")]
        [Command("amedit")]
        public async Task EditedIntervalAsync(string name, TimeSpan editedInterval)
        {
            await using var db = Context.Scope.ServiceProvider.GetRequiredService<DbService>();
            if (await _service.EditIntervalAsync(Context.Guild.Id.RawValue, name, editedInterval, db))
                await ReplyAsync($"Changed message interval to '{editedInterval.Humanize()}' !", Color.Green);
            else
                await ReplyAsync(
                    $"Couldn't find a automated message by that name ({name}), or something went wrong...", Color.Red);
        }
    }
}
