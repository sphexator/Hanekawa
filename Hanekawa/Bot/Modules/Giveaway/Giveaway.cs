using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Disqord;
using Disqord.Bot;
using Disqord.Extensions.Interactivity;
using Hanekawa.Bot.Preconditions;
using Hanekawa.Bot.TypeReaders;
using Hanekawa.Database;
using Hanekawa.Database.Tables.Giveaway;
using Hanekawa.Extensions;
using Hanekawa.Shared;
using Hanekawa.Shared.Command;
using Hanekawa.Shared.Command.Extensions;
using Humanizer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Qmmands;

namespace Hanekawa.Bot.Modules.Giveaway
{
    public class Giveaway : HanekawaCommandModule
    {
        [Name("Draw")]
        [Command("draw")]
        [Description("Draw(s) winner(s) from a reaction on a message")]
        [Remarks("draw 5 <emote> 5435346235434 #general")]
        [RequireMemberGuildPermissions(Permission.ManageMessages)]
        public async Task DrawWinnerAsync(int winners, LocalCustomEmoji emote, ulong messageId, CachedTextChannel channel = null)
        {
            await Context.Message.TryDeleteMessageAsync();
            var stream = new MemoryStream();
            channel ??= Context.Channel;
            if (channel == null) return;
            if (!(await channel.GetMessageAsync(messageId) is IUserMessage message))
            {
                await Context.ReplyAsync($"Couldn't find a message with that ID in {channel.Mention}",
                    Color.Red);
                return;
            }

            var reactionAmount = GetReactionAmount(message, emote);
            var users = await message.GetReactionsAsync(emote, reactionAmount);
            if (users == null)
            {
                await Context.ReplyAsync(
                    "Couldn't find any users reacting with that emote. You sure this is a emote on this server?",
                    Color.Red);
                return;
            }

            var rnd = new Random();
            var result = users.OrderBy(item => rnd.Next());
            var winnerString = new StringBuilder();

            await using var file = new StreamWriter(stream);
            var nr = 1;
            foreach (var x in result)
            {
                if (nr <= winners) winnerString.AppendLine($"{x}");
                await file.WriteLineAsync($"{nr}: {x.Id.RawValue} - {x.Name}#{x.Discriminator}");
                nr++;
            }

            await file.FlushAsync();
            stream.Seek(0, SeekOrigin.Begin);
            await channel.SendMessageAsync(new LocalAttachment(stream, "participants.txt"),
                $"Drawing winners for giveaway with reaction {emote}:\n{winners}");
        }

        [Name("Draw")]
        [Command("draw")]
        [Description("Draw winner from an active giveaway with provided ID")]
        [Remarks("draw 2")]
        [Priority(1)]
        [RequireMemberGuildPermissions(Permission.ManageGuild)]
        public async Task DrawWinnerAsync(int id)
        {
            await using var db = Context.Scope.ServiceProvider.GetRequiredService<DbService>();
            var giveaway =
                await db.Giveaways.Include(x => x.Participants).FirstOrDefaultAsync(x => x.GuildId == Context.Guild.Id.RawValue && x.IdNum == id);
            if (giveaway == null)
            {
                await ReplyAsync(
                    "Couldn't find a giveaway with that ID, please check the ID in the list to make sure you got the correct one",
                    Color.Red);
                return;
            }

            var rand = Context.ServiceProvider.GetRequiredService<Random>();
            var winners = new ulong[giveaway.WinnerAmount];
            var stringBuilder = new StringBuilder();
            for (var i = 0; i < giveaway.WinnerAmount; i++)
            {
                var x = giveaway.Participants[rand.Next(giveaway.Participants.Count)];
                var user = await Context.Guild.GetOrFetchMemberAsync(x.UserId);
                if (user == null || winners.Contains(user.Id.RawValue))
                {
                    i--;
                    continue;
                }
                winners.SetValue(user.Id.RawValue, i);
                stringBuilder.AppendLine($"{i + 1}: {user} ({user.Id.RawValue})");
            }

            await ReplyAsync($"**Drawing winners for giveaway {giveaway.Name} with ID: {giveaway.IdNum}**\n" +
                             $"{stringBuilder}");
            await db.GiveawayHistories.AddAsync(new GiveawayHistory
            {
                Id = giveaway.Id,
                IdNum = giveaway.IdNum,
                GuildId = giveaway.GuildId,
                Creator = giveaway.Creator,
                Winner = winners,
                ClosedAtOffset = giveaway.CloseAtOffset ?? DateTimeOffset.UtcNow,
                CreatedAtOffset = giveaway.CreatedAtOffset,
                Description = giveaway.Description,
                Name = giveaway.Name,
                Type = giveaway.Type
            });
            await db.SaveChangesAsync();
            db.Giveaways.Remove(giveaway);
            await db.SaveChangesAsync();
        }

        [Name("Create Giveaway")]
        [Command("gwcreate")]
        [Description("Creates a more advanced giveaway (Top.gg vote or activity based)")]
        [RequireMemberGuildPermissions(Permission.ManageGuild)]
        public async Task CreateAsync(GiveawayType type)
        {
            var giveaway = new Database.Tables.Giveaway.Giveaway
            {
                GuildId = Context.Guild.Id.RawValue,
                Creator = Context.User.Id.RawValue,
                CreatedAtOffset = DateTimeOffset.UtcNow,
                Active = true,
                Stack = true
            };
            var interactive = Context.Bot.GetInteractivity();

            await ReplyAsync("What's the name of the giveaway u wanna do?");
            var name = await interactive.WaitForMessageAsync(x =>
                x.Message.Author.Id == Context.User.Id && x.Message.Guild.Id == Context.Guild.Id);
            giveaway.Name = name.Message.Content;

            await ReplyAsync("Description of giveaway?");
            var description = await interactive.WaitForMessageAsync(x =>
                x.Message.Author.Id == Context.User.Id && x.Message.Guild.Id == Context.Guild.Id);
            giveaway.Description = description.Message.Content;

            await ReplyAsync("How many winners are gonna be drawn?");
            var winnerString = await interactive.WaitForMessageAsync(x =>
                x.Message.Author.Id == Context.User.Id && x.Message.Guild.Id == Context.Guild.Id);
            var isIntW = int.TryParse(winnerString.Message.Content, out var winnerAmount);
            if (isIntW) giveaway.WinnerAmount = winnerAmount;

            await ReplyAsync("Does entries stack? (y/n) (vote multiple times for multiple entries) ");
            var stackResponse = await interactive.WaitForMessageAsync(x =>
                x.Message.Author.Id == Context.User.Id && x.Message.Guild.Id == Context.Guild.Id);
            if (stackResponse.Message.Content.ToLower() == "y" || stackResponse.Message.Content.ToLower() == "yes")
                giveaway.Stack = true;
            else giveaway.Stack = false;

            await ReplyAsync("Level requirement?");
            var levelStr = await interactive.WaitForMessageAsync(x =>
                x.Message.Author.Id == Context.User.Id && x.Message.Guild.Id == Context.Guild.Id);
            var isInt = int.TryParse(levelStr.Message.Content, out var level);
            if (isInt) giveaway.LevelRequirement = level;
            var timeParse = new TimeSpanTypeParser();
            
            await ReplyAsync("When does the giveaway end? \n(1d = 1 day, 2h = 2 hours, 4m = 4minutes, 1d2d4m = 1 day, 2hours and 4min. Cancel with 'no'");
            var timespanStr = await interactive.WaitForMessageAsync(x =>
                x.Message.Author.Id == Context.User.Id && x.Message.Guild.Id == Context.Guild.Id);
            var timespan = await timeParse.ParseAsync(null, timespanStr.Message.Content, Context);
            if (timespan.IsSuccessful && timespan.HasValue)
                giveaway.CloseAtOffset = DateTimeOffset.UtcNow.Add(timespan.Value);
            
            await ReplyAsync("Restrict participant to server age? Respond like above to define account age \n1d = 1 day, 2h = 2 hours, 4m = 4minutes, 1d2d4m = 1 day, 2hours and 4min. Cancel with 'no'");
            var serverAge = await interactive.WaitForMessageAsync(x =>
                x.Message.Author.Id == Context.User.Id && x.Message.Guild.Id == Context.Guild.Id);
            var serverSpan = await timeParse.ParseAsync(null, serverAge.Message.Content, Context);
            if (serverSpan.IsSuccessful && serverSpan.HasValue) giveaway.ServerAgeRequirement = serverSpan.Value;

            var embed = new LocalEmbedBuilder
            {
                Title = giveaway.Name,
                Description = giveaway.Description,
                Timestamp = giveaway.CloseAtOffset,
                Color = Context.Colour.Get(Context.Guild.Id.RawValue),
                Footer = new LocalEmbedFooterBuilder {Text = "Giveaway ends in:"}
            };
            embed.AddField("Amount of winners", $"{giveaway.WinnerAmount}");
            embed.AddField("Stack Entries", $"{giveaway.Stack}");
            if (giveaway.LevelRequirement.HasValue)
                embed.AddField("Level Requirement", $"{giveaway.LevelRequirement.Value}");
            if (giveaway.ServerAgeRequirement.HasValue)
                embed.AddField("Server Age Restriction", $"{giveaway.ServerAgeRequirement.Value.Humanize()}");
            await ReplyAsync("Does this look good? (y/n)", false, embed.Build());
            var confirm = await interactive.WaitForMessageAsync(x =>
                x.Message.Author.Id == Context.User.Id && x.Message.Guild.Id == Context.Guild.Id);
            if (confirm == null || confirm.Message.Content.ToLower() != "y")
            {
                await ReplyAsync("Aborting...");
                return;
            }

            await using var db = Context.Scope.ServiceProvider.GetRequiredService<DbService>();
            var idHistory = await db.GiveawayHistories.CountAsync(x => x.GuildId == Context.Guild.Id.RawValue);
            var idActive = await db.Giveaways.CountAsync(x => x.GuildId == Context.Guild.Id.RawValue);
            giveaway.IdNum = idActive + idHistory + 1;
            await db.Giveaways.AddAsync(giveaway);
            await db.SaveChangesAsync();
            await ReplyAsync($"Giveaway added with id {giveaway.IdNum}!");
        }

        [Name("Giveaway List")]
        [Command("gwlist")]
        [Description("List of giveaways created")]
        [RequiredChannel]
        public async Task ListAsync()
        {
            var giveaways = new List<string>();
            await using var db = Context.Scope.ServiceProvider.GetRequiredService<DbService>();
            var current = await db.Giveaways.Where(x => x.GuildId == Context.Guild.Id.RawValue).ToListAsync();
            var old = await db.GiveawayHistories.Where(x => x.GuildId == Context.Guild.Id.RawValue).ToListAsync();
            for (var i = 0; i < current.Count; i++)
            {
                var x = current[i];
                var str = new StringBuilder();
                str.AppendLine($"Id: {x.IdNum}");
                str.AppendLine($"Name: {x.Name}");
                str.AppendLine($"Created At: {x.CreatedAtOffset.Humanize()}");
                giveaways.Add(str.ToString());
            }

            for (var i = 0; i < old.Count; i++)
            {
                var x = old[i];
                var str = new StringBuilder();
                str.AppendLine($"Id: {x.IdNum} (closed)");
                str.AppendLine($"Name: {x.Name}");
                str.AppendLine($"Created At: {x.CreatedAtOffset.Humanize()}");
                giveaways.Add(str.ToString());
            }

            await Context.PaginatedReply(giveaways, Context.Guild, $"Giveaways in {Context.Guild.Name}");
        }

        private static int GetReactionAmount(IUserMessage message, LocalCustomEmoji emote)
        {
            message.Reactions.TryGetValue(emote, out var reactionData);
            return reactionData?.Count ?? 0;
        }
    }
}