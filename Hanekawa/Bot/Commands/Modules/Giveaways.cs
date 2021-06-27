using System;
using System.Threading.Tasks;
using Disqord;
using Disqord.Bot;
using Disqord.Bot.Parsers;
using Disqord.Extensions.Interactivity;
using Disqord.Gateway;
using Disqord.Rest;
using Hanekawa.Bot.Commands.TypeReaders;
using Hanekawa.Bot.Service.Cache;
using Hanekawa.Database;
using Hanekawa.Database.Entities;
using Hanekawa.Database.Tables.Giveaway;
using Hanekawa.Entities.Color;
using Humanizer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Qmmands;

namespace Hanekawa.Bot.Commands.Modules
{
    [Name("Giveaways")]
    [Description("Commands for managing giveaways")]
    public class Giveaways : HanekawaCommandModule
    {
        // TODO: Add giveaway commands

        [Name("Giveaway Admin")]
        [Description("Creating and managing giveaways")]
        [Group("giveaway")]
        [RequireAuthorGuildPermissions(Permission.ManageGuild)]
        public class GiveawayAdmin : Giveaways
        {
            [Name("Create Giveaway")]
            [Description("Creates a giveaway based on reaction, activity or votes")]
            [Command("create")]
            public async Task<DiscordCommandResult> CreateAsync(GiveawayType type)
            {
                var giveaway = new Giveaway
                {
                    GuildId = Context.Guild.Id,
                    Creator = Context.Author.Id,
                    CreatedAtOffset = DateTimeOffset.UtcNow
                };
                var cache = Context.Services.GetRequiredService<CacheService>();
                var color = cache.GetColor(Context.GuildId);
                await Reply("Name of giveaway?", color);
                var name = await Context.WaitForMessageAsync(e =>
                    e.GuildId == Context.GuildId && e.ChannelId == Context.ChannelId &&
                    e.Member.Id == Context.Author.Id, TimeSpan.FromMinutes(1));
                giveaway.Name = name.Message.Content;
                await Reply("Description of giveaway?", color);
                var description = await Context.WaitForMessageAsync(e =>
                    e.GuildId == Context.GuildId && e.ChannelId == Context.ChannelId &&
                    e.Member.Id == Context.Author.Id, TimeSpan.FromMinutes(1));
                giveaway.Description = description.Message.Content;
                await Reply("How many winners are gonna be drawn?");
                var winnerString = await Context.WaitForMessageAsync(e =>
                    e.GuildId == Context.GuildId && e.ChannelId == Context.ChannelId &&
                    e.Member.Id == Context.Author.Id, TimeSpan.FromMinutes(1));
                var isIntW = int.TryParse(winnerString.Message.Content, out var winnerAmount);
                if (isIntW) giveaway.WinnerAmount = winnerAmount;

                await Reply("Does entries stack? (y/n) (vote multiple times for multiple entries)");
                var stackResponse = await Context.WaitForMessageAsync(e =>
                    e.GuildId == Context.GuildId && e.ChannelId == Context.ChannelId &&
                    e.Member.Id == Context.Author.Id, TimeSpan.FromMinutes(1));
                if (stackResponse.Message.Content.ToLower() == "y" || stackResponse.Message.Content.ToLower() == "yes")
                    giveaway.Stack = true;
                else giveaway.Stack = false;

                var timeParse = new TimeSpanTypeParser();

                await Reply(
                    "When does the giveaway end? \n(1d = 1 day, 2h = 2 hours, 4m = 4minutes, 1d2d4m = 1 day, 2hours and 4min. Continue with 'no'");
                var timespanStr = await Context.WaitForMessageAsync(e =>
                    e.GuildId == Context.GuildId && e.ChannelId == Context.ChannelId &&
                    e.Member.Id == Context.Author.Id, TimeSpan.FromMinutes(1));
                var timespan = await timeParse.ParseAsync(null, timespanStr.Message.Content, Context);
                if (timespan.IsSuccessful && timespan.HasValue)
                    giveaway.CloseAtOffset = DateTimeOffset.UtcNow.Add(timespan.Value);

                await Reply(
                    "Restrict participant to server age? Respond like above to define account age \n1d = 1 day, 2h = 2 hours, 4m = 4minutes, 1d2d4m = 1 day, 2hours and 4min. Continue with 'no'");
                var serverAge = await Context.WaitForMessageAsync(e =>
                    e.GuildId == Context.GuildId && e.ChannelId == Context.ChannelId &&
                    e.Member.Id == Context.Author.Id, TimeSpan.FromMinutes(1));
                var serverSpan = await timeParse.ParseAsync(null, serverAge.Message.Content, Context);
                if (serverSpan.IsSuccessful && serverSpan.HasValue) giveaway.ServerAgeRequirement = serverSpan.Value;
                Snowflake? toPost = null;
                if (type is GiveawayType.Reaction or GiveawayType.ReactAndActivity or GiveawayType.VoteAndReact or
                    GiveawayType.All) toPost = await CreateWithReactionEventAsync(color);

                var embed = new LocalEmbed
                {
                    Title = giveaway.Name,
                    Description = giveaway.Description,
                    Timestamp = giveaway.CloseAtOffset,
                    Color = color,
                    Footer = new LocalEmbedFooter {Text = "Giveaway ends in:"}
                };
                embed.AddField("Amount of winners", $"{giveaway.WinnerAmount}");
                embed.AddField("Stack Entries", $"{giveaway.Stack}");
                if (giveaway.LevelRequirement.HasValue)
                    embed.AddField("Level Requirement", $"{giveaway.LevelRequirement.Value}");
                if (giveaway.ServerAgeRequirement.HasValue)
                    embed.AddField("Server Age Restriction", $"{giveaway.ServerAgeRequirement.Value.Humanize()}");

                await Reply("Does this look good? (y/n)", embed);
                var confirm = await Context.WaitForMessageAsync(e =>
                    e.GuildId == Context.GuildId && e.ChannelId == Context.ChannelId &&
                    e.Member.Id == Context.Author.Id, TimeSpan.FromMinutes(1));
                if (confirm == null || confirm.Message.Content.ToLower() != "y" ||
                    confirm.Message.Content.ToLower() != "yes")
                    return Reply("Aborting...", HanaBaseColor.Bad());

                if (toPost.HasValue)
                {
                    var msg = await (Context.Guild.GetChannel(toPost.Value) as ITextChannel).SendMessageAsync(
                        new LocalMessage
                        {
                            Embeds = {embed},
                            AllowedMentions = LocalAllowedMentions.None,
                            IsTextToSpeech = false
                        });
                    giveaway.ReactionMessage = msg.Id;
                }

                await using var db = Context.Scope.ServiceProvider.GetRequiredService<DbService>();
                var idHistory = await db.GiveawayHistories.CountAsync(x => x.GuildId == Context.Guild.Id.RawValue);
                var idActive = await db.Giveaways.CountAsync(x => x.GuildId == Context.Guild.Id.RawValue);
                giveaway.IdNum = idActive + idHistory + 1;
                await db.Giveaways.AddAsync(giveaway);
                await db.SaveChangesAsync();
                return Reply($"Giveaway added with id {giveaway.IdNum}!");
            }

            private async Task<Snowflake?> CreateWithReactionEventAsync(Color color)
            {
                await Reply(
                    "Which channel should the message be sent?\nNo to skip and post yourself (make sure to assign add the message later with 'giveaway msgadd giveawayId msgId')",
                    color);
                var response = await Context.WaitForMessageAsync(e =>
                    e.GuildId == Context.GuildId && e.ChannelId == Context.ChannelId &&
                    e.Member.Id == Context.Author.Id, TimeSpan.FromMinutes(1));
                var channelParser = new GuildChannelTypeParser<ITextChannel>();
                var channel = await channelParser.ParseAsync(null, response.Message.Content, Context);
                return channel.Value.Id;
            }
        }
    }
}