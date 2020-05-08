using System.Linq;
using System.Threading.Tasks;
using Disqord;
using Hanekawa.Database;
using Hanekawa.Database.Extensions;
using Hanekawa.Database.Tables.Club;
using Hanekawa.Database.Tables.Config.Guild;
using Hanekawa.Extensions;
using Hanekawa.Extensions.Embed;
using Hanekawa.Shared.Command;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Quartz.Util;

namespace Hanekawa.Bot.Services.Club
{
    public partial class ClubService
    {
        public async Task AdNameAsync(HanekawaContext context, string name)
        {
            if (name.IsNullOrWhiteSpace()) return;
            using var scope = _provider.CreateScope();
            await using var db = scope.ServiceProvider.GetRequiredService<DbService>();
            var club = await db.ClubInfos.FirstOrDefaultAsync(x =>
                x.GuildId == context.Guild.Id.RawValue && x.LeaderId == context.User.Id.RawValue);
            if (club == null) return;
            await context.Message.TryDeleteMessageAsync();
            var cfg = await db.GetOrCreateClubConfigAsync(context.Guild);
            club.Name = name;
            await db.SaveChangesAsync();
            await context.ReplyAsync($"Updated club name to `{name}` !");
            if (club.AdMessage.HasValue && cfg.AdvertisementChannel.HasValue)
            {
                var msg = await context.Guild.GetTextChannel(cfg.AdvertisementChannel.Value)
                    .GetMessageAsync(club.AdMessage.Value) as IUserMessage;
                await UpdatePostNameAsync(msg, name);
            }

            if (club.Role.HasValue)
            {
                var role = context.Guild.GetRole(club.Role.Value);
                await role.ModifyAsync(x => x.Name = name);
            }

            if (club.Channel.HasValue)
            {
                var channel = context.Guild.GetTextChannel(club.Channel.Value);
                await channel.ModifyAsync(x => x.Name = name);
            }
        }

        public async Task AdDescAsync(HanekawaContext context, string desc)
        {
            using var scope = _provider.CreateScope();
            await using var db = scope.ServiceProvider.GetRequiredService<DbService>();
            var leader = await db.ClubInfos.FirstOrDefaultAsync(x =>
                x.GuildId == context.Guild.Id.RawValue && x.LeaderId == context.User.Id.RawValue);
            if (leader == null) return;
            await context.Message.TryDeleteMessageAsync();
            var cfg = await db.GetOrCreateClubConfigAsync(context.Guild);
            leader.Description = desc;
            await db.SaveChangesAsync();
            await context.ReplyAsync("Updated description of club!", Color.Green);
            if (leader.AdMessage.HasValue && cfg.AdvertisementChannel.HasValue)
            {
                var msg = await context.Guild.GetTextChannel(cfg.AdvertisementChannel.Value)
                    .GetMessageAsync(leader.AdMessage.Value) as IUserMessage;
                await UpdatePostDescriptionAsync(msg, desc);
            }
        }

        public async Task AdImageAsync(HanekawaContext context, string image)
        {
            using var scope = _provider.CreateScope();
            await using var db = scope.ServiceProvider.GetRequiredService<DbService>();
            var leader = await db.ClubInfos.FirstOrDefaultAsync(x =>
                x.GuildId == context.Guild.Id.RawValue && x.LeaderId == context.User.Id.RawValue);
            if (leader == null) return;
            await context.Message.TryDeleteMessageAsync();
            var cfg = await db.GetOrCreateClubConfigAsync(context.Guild);
            leader.ImageUrl = image;
            await db.SaveChangesAsync();
            await context.ReplyAsync("Updated description of club!", Color.Green);
            if (leader.AdMessage.HasValue && cfg.AdvertisementChannel.HasValue)
            {
                var msg = await context.Guild.GetTextChannel(cfg.AdvertisementChannel.Value)
                    .GetMessageAsync(leader.AdMessage.Value) as IUserMessage;
                await UpdatePostImageAsync(msg, image);
            }
        }

        public async Task AdIconAsync(HanekawaContext context, string icon)
        {
            using var scope = _provider.CreateScope();
            await using var db = scope.ServiceProvider.GetRequiredService<DbService>();
            var leader = await db.ClubInfos.FirstOrDefaultAsync(x =>
                x.GuildId == context.Guild.Id.RawValue && x.LeaderId == context.User.Id.RawValue);
            if (leader == null) return;
            await context.Message.TryDeleteMessageAsync();
            var cfg = await db.GetOrCreateClubConfigAsync(context.Guild);
            leader.IconUrl = icon;
            await db.SaveChangesAsync();
            await context.ReplyAsync("Updated description of club!", Color.Green);
            if (leader.AdMessage.HasValue && cfg.AdvertisementChannel.HasValue)
            {
                var msg = await context.Guild.GetTextChannel(cfg.AdvertisementChannel.Value)
                    .GetMessageAsync(leader.AdMessage.Value) as IUserMessage;
                await UpdatePostIconAsync(msg, icon);
            }
        }

        public async Task AdPublicAsync(HanekawaContext context)
        {
            using var scope = _provider.CreateScope();
            await using var db = scope.ServiceProvider.GetRequiredService<DbService>();
            var club = await db.ClubInfos.FirstOrDefaultAsync(x =>
                x.GuildId == context.Guild.Id.RawValue && x.LeaderId == context.User.Id.RawValue);
            if (club == null) return;
            if (club.Public)
            {
                club.Public = false;
                await db.SaveChangesAsync();
                await context.ReplyAsync("Club is no longer public. People need invite to enter the club.",
                    Color.Green);
                if (club.AdMessage.HasValue) await PublicHandle(db, context, club, context.Guild, false);
            }
            else
            {
                club.Public = true;
                await db.SaveChangesAsync();
                await context.ReplyAsync("Set club as public. Anyone can join!", Color.Green);
                if (club.AdMessage.HasValue) await PublicHandle(db, context, club, context.Guild, true);
            }
        }

        public async Task AdAdvertiseAsync(HanekawaContext context)
        {
            using var scope = _provider.CreateScope();
            await using var db = scope.ServiceProvider.GetRequiredService<DbService>();
            var leader = await db.ClubInfos.FirstOrDefaultAsync(x =>
                x.GuildId == context.Guild.Id.RawValue && x.LeaderId == context.User.Id.RawValue);
            if (leader == null) return;
            var cfg = await db.GetOrCreateClubConfigAsync(context.Guild);
            if (!cfg.AdvertisementChannel.HasValue)
            {
                await context.ReplyAsync("This server hasn't setup or doesn't allow club advertisement.",
                    Color.Red);
                return;
            }

            if (!leader.AdMessage.HasValue)
            {
                await SendPostAsync(db, cfg, context.Guild, leader);
                await context.ReplyAsync("Posted ad!", Color.Green);
            }
            else
            {
                var msg = await context.Guild.GetTextChannel(cfg.AdvertisementChannel.Value)
                    .GetMessageAsync(leader.AdMessage.Value);
                if (msg == null)
                {
                    await SendPostAsync(db, cfg, context.Guild, leader);
                    await context.ReplyAsync("Posted ad!", Color.Green);
                }
                else
                {
                    await msg.DeleteAsync();
                    await SendPostAsync(db, cfg, context.Guild, leader);
                    await context.ReplyAsync("Re-posted ad!", Color.Green);
                }
            }
        }

        public async Task UpdatePostNameAsync(IUserMessage msg, string name)
        {
            var embedDesc = msg.Embeds.First().ToEmbedBuilder();
            embedDesc.Author.Name = name;
            await msg.ModifyAsync(x => x.Embed = embedDesc.Build());
        }

        public async Task UpdatePostDescriptionAsync(IUserMessage msg, string content)
        {
            var embedDesc = msg.Embeds.First().ToEmbedBuilder();
            embedDesc.Description = content;
            await msg.ModifyAsync(x => x.Embed = embedDesc.Build());
        }

        public async Task UpdatePostIconAsync(IUserMessage msg, string icon)
        {
            var embedDesc = msg.Embeds.First().ToEmbedBuilder();
            embedDesc.Author.IconUrl = icon;
            await msg.ModifyAsync(x => x.Embed = embedDesc.Build());
        }

        public async Task UpdatePostImageAsync(IUserMessage msg, string image)
        {
            var embedDesc = msg.Embeds.First().ToEmbedBuilder();
            embedDesc.ImageUrl = image;
            await msg.ModifyAsync(x => x.Embed = embedDesc.Build());
        }

        private async Task SendPostAsync(DbService db, ClubConfig cfg, CachedGuild guild, ClubInformation club)
        {
            if (!cfg.AdvertisementChannel.HasValue) return;
            var embed = new LocalEmbedBuilder()
                .Create(club.Description ?? "No description added", _colourService.Get(guild.Id.RawValue))
                .WithAuthor(new LocalEmbedAuthorBuilder() {Name = club.Name})
                .WithImageUrl(club.ImageUrl);
            var msg = await guild.GetTextChannel(cfg.AdvertisementChannel.Value).ReplyAsync(embed);
            club.AdMessage = msg.Id.RawValue;
            await db.SaveChangesAsync();
            if (club.Public) await msg.AddReactionAsync(new LocalEmoji("\u2714"));
        }

        private async Task PublicHandle(DbService db, HanekawaContext context, ClubInformation club, CachedGuild guild,
            bool enabled)
        {
            var cfg = await db.GetOrCreateClubConfigAsync(context.Guild);
            if (cfg.AdvertisementChannel.HasValue)
            {
                IUserMessage msg = null;
                if (club.AdMessage.HasValue)
                    msg = await context.Guild.GetTextChannel(cfg.AdvertisementChannel.Value)
                        .GetMessageAsync(club.AdMessage.Value) as IUserMessage;
                if (enabled)
                {
                    if (msg == null) await SendPostAsync(db, cfg, guild, club);
                    else await msg.AddReactionAsync(new LocalEmoji("\u2714"));
                }
                else
                {
                    if (msg == null)
                    {
                        club.AdMessage = null;
                        await db.SaveChangesAsync();
                    }
                    else
                    {
                        await msg.ClearReactionsAsync();
                    }
                }
            }
        }
    }
}