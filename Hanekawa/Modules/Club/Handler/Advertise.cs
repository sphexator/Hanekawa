﻿using Discord;
using Discord.Commands;
using Hanekawa.Addons.Database;
using Hanekawa.Addons.Database.Extensions;
using Hanekawa.Addons.Database.Tables.Club;
using Hanekawa.Addons.Database.Tables.Config.Guild;
using Hanekawa.Entities.Interfaces;
using Hanekawa.Extensions.Embed;
using Quartz.Util;
using System.Linq;
using System.Threading.Tasks;

namespace Hanekawa.Modules.Club.Handler
{
    public class Advertise : IHanaService
    {
        public async Task PublicAsync(ICommandContext context)
        {
            using (var db = new DbService())
            {
                var club = await db.IsClubLeader(context.Guild.Id, context.User.Id);
                if (club == null) return;
                if (club.Public)
                {
                    club.Public = false;
                    await db.SaveChangesAsync();
                    await context.ReplyAsync("Club is no longer public. People need invite to enter the club.",
                        Color.Green.RawValue);
                    if (club.AdMessage.HasValue) await PublicHandle(db, context, club, context.Guild, false);
                }
                else
                {
                    club.Public = true;
                    await db.SaveChangesAsync();
                    await context.ReplyAsync("Set club as public. Anyone can join!", Color.Green.RawValue);
                    if (club.AdMessage.HasValue) await PublicHandle(db, context, club, context.Guild, true);
                }
            }
        }

        public async Task AdvertiseAsync(ICommandContext context)
        {
            using (var db = new DbService())
            {
                var leader = await db.IsClubLeader(context.Guild.Id, context.User.Id);
                if (leader == null) return;
                var cfg = await db.GetOrCreateClubConfigAsync(context.Guild);
                if (!cfg.AdvertisementChannel.HasValue)
                {
                    await context.ReplyAsync("This server hasn't setup or doesn't allow club advertisement.",
                        Color.Red.RawValue);
                    return;
                }

                if (!leader.AdMessage.HasValue)
                {
                    await SendPostAsync(db, cfg, context.Guild, leader);
                    await context.ReplyAsync("Posted ad!", Color.Green.RawValue);
                }
                else
                {
                    var msg = await (await context.Guild.GetTextChannelAsync(cfg.AdvertisementChannel.Value))
                        .GetMessageAsync(leader.AdMessage.Value);
                    if (msg == null)
                    {
                        await SendPostAsync(db, cfg, context.Guild, leader);
                        await context.ReplyAsync("Posted ad!", Color.Green.RawValue);
                    }
                    else
                    {
                        await msg.DeleteAsync();
                        await SendPostAsync(db, cfg, context.Guild, leader);
                        await context.ReplyAsync("Re-posted ad!", Color.Green.RawValue);
                    }
                }
            }
        }

        public async Task NameAsync(ICommandContext context, string name)
        {
            if (name.IsNullOrWhiteSpace()) return;
            using (var db = new DbService())
            {
                var club = await db.IsClubLeader(context.Guild.Id, context.User.Id);
                if (club == null) return;
                await context.Message.DeleteAsync();
                var cfg = await db.GetOrCreateClubConfigAsync(context.Guild);
                club.Name = name;
                await db.SaveChangesAsync();
                await context.ReplyAsync($"Updated club name to `{name}` !");
                if (club.AdMessage.HasValue && cfg.AdvertisementChannel.HasValue)
                {
                    var msg = await (await context.Guild.GetTextChannelAsync(cfg.AdvertisementChannel.Value)
                        ).GetMessageAsync(club.AdMessage.Value) as IUserMessage;
                    await UpdatePostNameAsync(msg, name);
                }

                if (club.Role.HasValue)
                {
                    var role = context.Guild.GetRole(club.Role.Value);
                    await role.ModifyAsync(x => x.Name = name);
                }

                if (club.Channel.HasValue)
                {
                    var channel = await context.Guild.GetTextChannelAsync(club.Channel.Value);
                    await channel.ModifyAsync(x => x.Name = name);
                }
            }
        }

        public async Task DescriptionAsync(ICommandContext context, string desc)
        {
            using (var db = new DbService())
            {
                var leader = await db.IsClubLeader(context.Guild.Id, context.User.Id);
                if (leader == null) return;
                await context.Message.DeleteAsync();
                var cfg = await db.GetOrCreateClubConfigAsync(context.Guild);
                leader.Description = desc;
                await db.SaveChangesAsync();
                await context.ReplyAsync("Updated description of club!", Color.Green.RawValue);
                if (leader.AdMessage.HasValue && cfg.AdvertisementChannel.HasValue)
                {
                    var msg = await (await context.Guild.GetTextChannelAsync(cfg.AdvertisementChannel.Value)
                        ).GetMessageAsync(leader.AdMessage.Value) as IUserMessage;
                    await UpdatePostDescriptionAsync(msg, desc);
                }
            }
        }

        public async Task ImageAsync(ICommandContext context, string image)
        {
            using (var db = new DbService())
            {
                var leader = await db.IsClubLeader(context.Guild.Id, context.User.Id);
                if (leader == null) return;
                await context.Message.DeleteAsync();
                var cfg = await db.GetOrCreateClubConfigAsync(context.Guild);
                leader.ImageUrl = image;
                await db.SaveChangesAsync();
                await context.ReplyAsync("Updated description of club!", Color.Green.RawValue);
                if (leader.AdMessage.HasValue && cfg.AdvertisementChannel.HasValue)
                {
                    var msg = await (await context.Guild.GetTextChannelAsync(cfg.AdvertisementChannel.Value)
                        ).GetMessageAsync(leader.AdMessage.Value) as IUserMessage;
                    await UpdatePostImageAsync(msg, image);
                }
            }
        }

        public async Task IconAsync(ICommandContext context, string icon)
        {
            using (var db = new DbService())
            {
                var leader = await db.IsClubLeader(context.Guild.Id, context.User.Id);
                if (leader == null) return;
                await context.Message.DeleteAsync();
                var cfg = await db.GetOrCreateClubConfigAsync(context.Guild);
                leader.IconUrl = icon;
                await db.SaveChangesAsync();
                await context.ReplyAsync("Updated description of club!", Color.Green.RawValue);
                if (leader.AdMessage.HasValue && cfg.AdvertisementChannel.HasValue)
                {
                    var msg = await (await context.Guild.GetTextChannelAsync(cfg.AdvertisementChannel.Value)
                        ).GetMessageAsync(leader.AdMessage.Value) as IUserMessage;
                    await UpdatePostIconAsync(msg, icon);
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

        private async Task SendPostAsync(DbService db, ClubConfig cfg, IGuild guild, ClubInformation club)
        {
            if (!cfg.AdvertisementChannel.HasValue) return;
            var embed = new EmbedBuilder()
                .CreateDefault(club.Description ?? "No description added", guild.Id)
                .WithAuthor(new EmbedAuthorBuilder { Name = club.Name })
                .WithImageUrl(club.ImageUrl);
            var msg = await (await guild.GetTextChannelAsync(cfg.AdvertisementChannel.Value)).ReplyAsync(embed);
            club.AdMessage = msg.Id;
            await db.SaveChangesAsync();
            if (club.Public) await msg.AddReactionAsync(new Emoji("\u2714"));
        }

        private async Task PublicHandle(DbService db, ICommandContext context, ClubInformation club, IGuild guild, bool enabled)
        {
            var cfg = await db.GetOrCreateClubConfigAsync(context.Guild);
            if (cfg.AdvertisementChannel.HasValue)
            {
                IUserMessage msg = null;
                if(club.AdMessage.HasValue) msg = await (await context.Guild.GetTextChannelAsync(cfg.AdvertisementChannel.Value))
                    .GetMessageAsync(club.AdMessage.Value) as IUserMessage;
                if (enabled)
                {
                    if (msg == null) await SendPostAsync(db, cfg, guild, club);
                    else await msg.AddReactionAsync(new Emoji("\u2714"));
                }
                else
                {
                    if (msg == null)
                    {
                        club.AdMessage = null;
                        await db.SaveChangesAsync();
                    }
                    else await msg.RemoveAllReactionsAsync();
                }
            }
        }
    }
}