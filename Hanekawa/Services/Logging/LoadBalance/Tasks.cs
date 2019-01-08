﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Hanekawa.Addons.Database;
using Hanekawa.Addons.Database.Data;
using Hanekawa.Addons.Database.Extensions;
using Hanekawa.Entities.Interfaces;
using Hanekawa.Entities.LogEntities;
using Hanekawa.Extensions;
using Hanekawa.Extensions.Embed;
using Humanizer;
using ActionType = Hanekawa.Entities.ActionType;

namespace Hanekawa.Services.Logging.LoadBalance
{
    public class Tasks : IHanaService, IRequiredService
    {
        private readonly DiscordSocketClient _client;

        public Tasks(DiscordSocketClient client)
        {
            _client = client;

            _client.LeftGuild += GuildCleanup;
            Console.WriteLine("Task service loaded");
        }

        public ConcurrentDictionary<ulong, ConcurrentQueue<UserBanned>> BanQueue { get; } =
            new ConcurrentDictionary<ulong, ConcurrentQueue<UserBanned>>();

        public ConcurrentDictionary<ulong, ConcurrentQueue<UserUnbanned>> UnBanQueue { get; } =
            new ConcurrentDictionary<ulong, ConcurrentQueue<UserUnbanned>>();

        public ConcurrentDictionary<ulong, ConcurrentQueue<UserJoined>> UserJoinedQueue { get; } =
            new ConcurrentDictionary<ulong, ConcurrentQueue<UserJoined>>();

        public ConcurrentDictionary<ulong, ConcurrentQueue<UserLeft>> UserLeftQueue { get; } =
            new ConcurrentDictionary<ulong, ConcurrentQueue<UserLeft>>();

        public ConcurrentDictionary<ulong, ConcurrentQueue<MessageDeleted>> MessageDeletedQueue { get; } =
            new ConcurrentDictionary<ulong, ConcurrentQueue<MessageDeleted>>();

        public ConcurrentDictionary<ulong, ConcurrentQueue<MessageUpdated>> MessageUpdatedQueue { get; } =
            new ConcurrentDictionary<ulong, ConcurrentQueue<MessageUpdated>>();

        public ConcurrentDictionary<ulong, ConcurrentQueue<GuildUserUpdated>> GuildMemberUpdatedQueue { get; } =
            new ConcurrentDictionary<ulong, ConcurrentQueue<GuildUserUpdated>>();

        public ConcurrentDictionary<ulong, ConcurrentQueue<UserUpdated>> UserUpdatedQueue { get; } =
            new ConcurrentDictionary<ulong, ConcurrentQueue<UserUpdated>>();

        private Task GuildCleanup(SocketGuild guild)
        {
            var _ = Task.Run(() =>
            {
                BanQueue.TryRemove(guild.Id, out var _);
                UnBanQueue.TryRemove(guild.Id, out var _);
                UserJoinedQueue.TryRemove(guild.Id, out var _);
                UserLeftQueue.TryRemove(guild.Id, out var _);
                MessageDeletedQueue.TryRemove(guild.Id, out var _);
                MessageUpdatedQueue.TryRemove(guild.Id, out var _);
                GuildMemberUpdatedQueue.TryRemove(guild.Id, out var _);
                UserUpdatedQueue.TryRemove(guild.Id, out var _);
            });
            return Task.CompletedTask;
        }

        public async Task ProcessJoinEvent(ulong guildId)
        {
            var queue = UserJoinedQueue.GetOrAdd(guildId, new ConcurrentQueue<UserJoined>());
            var worker = true;
            var lastRequest = DateTime.UtcNow;
            while (worker)
            {
                while (queue.TryDequeue(out var userJoined))
                {
                    var user = userJoined.User;
                    using (var db = new DbService())
                    {
                        var cfg = await db.GetOrCreateGuildConfigAsync(user.Guild).ConfigureAwait(false);
                        if (!cfg.LogJoin.HasValue) return;
                        var ch = user.Guild.GetTextChannel(cfg.LogJoin.Value);
                        if (ch == null) return;

                        await ch.ReplyAsync(new EmbedBuilder()
                            .CreateDefault($"📥 {user.Mention} has joined (*{user.Id}*)\n" +
                                           $"Account Created: {user.CreatedAt.Humanize()}", Color.Green.RawValue)
                            .WithFooter(new EmbedFooterBuilder
                                {Text = $"Username: {user.Username}#{user.Discriminator}"})
                            .WithTimestamp(new DateTimeOffset(DateTime.UtcNow)));
                        ;
                        lastRequest = DateTime.UtcNow;
                    }
                }

                await Task.Delay(TimeSpan.FromMilliseconds(500));
                if (lastRequest.AddHours(1) >= DateTime.UtcNow) worker = false;
            }
        }

        public async Task ProcessLeaveEvent(ulong guildId)
        {
            var queue = UserLeftQueue.GetOrAdd(guildId, new ConcurrentQueue<UserLeft>());
            var worker = true;
            var lastRequest = DateTime.UtcNow;
            while (worker)
            {
                while (queue.TryDequeue(out var userJoined))
                {
                    var user = userJoined.User;
                    using (var db = new DbService())
                    {
                        var cfg = await db.GetOrCreateGuildConfigAsync(user.Guild).ConfigureAwait(false);
                        if (!cfg.LogJoin.HasValue) return;
                        var ch = user.Guild.GetTextChannel(cfg.LogJoin.Value);
                        if (ch == null) return;

                        await ch.ReplyAsync(new EmbedBuilder()
                            .CreateDefault($"📤 {user.Mention} has left (*{user.Id}*)", Color.Red.RawValue)
                            .WithFooter(new EmbedFooterBuilder
                                {Text = $"Username: {user.Username}#{user.Discriminator}"})
                            .WithTimestamp(new DateTimeOffset(DateTime.UtcNow)));
                        lastRequest = DateTime.UtcNow;
                    }
                }

                await Task.Delay(TimeSpan.FromMilliseconds(500));
                if (lastRequest.AddHours(1) >= DateTime.UtcNow) worker = false;
            }
        }

        public async Task ProcessMessageDeletedEvent(ulong guildId)
        {
            var queue = MessageDeletedQueue.GetOrAdd(guildId, new ConcurrentQueue<MessageDeleted>());
            var worker = true;
            var lastRequest = DateTime.UtcNow;
            while (worker)
            {
                while (queue.TryDequeue(out var messageDeleted))
                {
                    if (messageDeleted.OptMsg.HasValue && messageDeleted.OptMsg.Value.Author.IsBot) return;

                    if (!(messageDeleted.Channel is SocketGuildChannel chx)) return;
                    using (var db = new DbService())
                    {
                        var cfg = await db.GetOrCreateGuildConfigAsync(chx.Guild).ConfigureAwait(false);
                        if (!cfg.LogMsg.HasValue) return;
                        var channel = chx.Guild.GetTextChannel(cfg.LogMsg.Value);
                        if (channel == null) return;
                        if (!messageDeleted.OptMsg.HasValue) return;
                        if (!(messageDeleted.OptMsg.Value is IUserMessage msg)) return;

                        var embed = new EmbedBuilder()
                            .CreateDefault(
                                $"{msg.Author.Mention} deleted a message in {(chx as ITextChannel)?.Mention}", guildId)
                            .WithAuthor(new EmbedAuthorBuilder {Name = "Message Deleted"})
                            .WithFooter(new EmbedFooterBuilder
                                {Text = $"User: {msg.Author.Id} | Message ID: {msg.Id}"})
                            .WithTimestamp(msg.Timestamp)
                            .WithFields(new List<EmbedFieldBuilder>
                            {
                                new EmbedFieldBuilder
                                    {IsInline = false, Name = "Message", Value = msg.Content.Truncate(900)}
                            });

                        if (msg.Attachments.Count > 0)
                        {
                            var file = msg.Attachments.FirstOrDefault();
                            if (file != null)
                            {
                                embed.AddField(x =>
                                {
                                    x.Name = "File";
                                    x.IsInline = false;
                                    x.Value = msg.Attachments.FirstOrDefault()?.Url;
                                });
                            }
                        }

                        await channel.ReplyAsync(embed);

                        lastRequest = DateTime.UtcNow;
                    }
                }

                await Task.Delay(TimeSpan.FromMilliseconds(500));
                if (lastRequest.AddHours(1) >= DateTime.UtcNow) worker = false;
            }
        }

        public async Task ProcessMessageUpdatedEvent(ulong guildId)
        {
            var queue = MessageUpdatedQueue.GetOrAdd(guildId, new ConcurrentQueue<MessageUpdated>());
            var worker = true;
            var lastRequest = DateTime.UtcNow;
            while (worker)
            {
                while (queue.TryDequeue(out var messageUpdated))
                {
                    if (messageUpdated.NewMessage.Author.IsBot) return;

                    if (!(messageUpdated.Channel is ITextChannel chtx)) return;
                    using (var db = new DbService())
                    {
                        var cfg = await db.GetOrCreateGuildConfigAsync(chtx.Guild);
                        if (!cfg.LogMsg.HasValue) return;

                        if (!messageUpdated.OldMessage.HasValue) return;
                        if (!(messageUpdated.OldMessage.Value is IUserMessage msg)) return;
                        var channel = await chtx.Guild.GetTextChannelAsync(cfg.LogMsg.Value);

                        if (channel == null) return;
                        if (msg.Author.IsBot &&
                            messageUpdated.OldMessage.Value.Content == messageUpdated.NewMessage.Content) return;
                        if (messageUpdated.OldMessage.Value.Content == messageUpdated.NewMessage.Content) return;

                        await channel.ReplyAsync(new EmbedBuilder()
                            .CreateDefault(
                                $"{messageUpdated.NewMessage.Author.Mention} updated a message in {chtx.Mention}",
                                guildId)
                            .WithAuthor(new EmbedAuthorBuilder {Name = "Message Updated"})
                            .WithFooter(new EmbedFooterBuilder {Text = $"User: {msg.Author.Id} | Message ID: {msg.Id}"})
                            .WithTimestamp(messageUpdated.NewMessage.EditedTimestamp ??
                                           messageUpdated.NewMessage.Timestamp)
                            .WithFields(new List<EmbedFieldBuilder>
                            {
                                new EmbedFieldBuilder
                                {
                                    IsInline = true, Name = "Updated Message:",
                                    Value = messageUpdated.NewMessage.Content.Truncate(900)
                                },
                                new EmbedFieldBuilder
                                    {IsInline = true, Name = "Old Message:", Value = msg.Content.Truncate(900)}
                            }));

                        lastRequest = DateTime.UtcNow;
                    }
                }

                await Task.Delay(TimeSpan.FromMilliseconds(500));
                if (lastRequest.AddHours(1) >= DateTime.UtcNow) worker = false;
            }
        }

        public async Task ProcessUserBannedEvent(ulong guildId)
        {
            var queue = BanQueue.GetOrAdd(guildId, new ConcurrentQueue<UserBanned>());
            var worker = true;
            var lastRequest = DateTime.UtcNow;
            while (worker)
            {
                while (queue.TryDequeue(out var userBanned))
                    using (var db = new DbService())
                    {
                        try
                        {
                            var cfg = await db.GetOrCreateGuildConfigAsync(userBanned.Guild).ConfigureAwait(false);
                            if (!cfg.LogBan.HasValue) return;
                            var ch = userBanned.Guild.GetTextChannel(cfg.LogBan.Value);
                            if (ch == null) return;

                            var caseId = await db.CreateCaseId(userBanned.User, userBanned.Guild, DateTime.UtcNow,
                                (ModAction) Data.Constants.ModAction.Ban);
                            var msg = await ch.ReplyAsync(new EmbedBuilder().CreateDefault(null, Color.Red.RawValue)
                                .WithAuthor(new EmbedAuthorBuilder
                                {
                                    Name =
                                        $"Case ID: {caseId.Id} - {ActionType.Bent} | {userBanned.User.Username}#{userBanned.User.Discriminator}"
                                })
                                .WithFooter(new EmbedFooterBuilder {Text = $"User ID: {userBanned.User.Id}"})
                                .WithTimestamp(new DateTimeOffset(DateTime.UtcNow))
                                .WithFields(new List<EmbedFieldBuilder>().ModLogFieldBuilders(userBanned.User)));

                            caseId.MessageId = msg.Id;
                            await db.SaveChangesAsync();
                        }
                        catch (Exception e)
                        {
                            //TODO: Add logging
                            Console.WriteLine(e);
                        }

                        lastRequest = DateTime.UtcNow;
                    }

                await Task.Delay(TimeSpan.FromMilliseconds(500));
                if (lastRequest.AddHours(10) >= DateTime.UtcNow) worker = false;
            }
        }

        public async Task ProcessUserUnbannedEvent(ulong guildId)
        {
            var queue = UnBanQueue.GetOrAdd(guildId, new ConcurrentQueue<UserUnbanned>());
            var worker = true;
            var lastRequest = DateTime.UtcNow;
            while (worker)
            {
                while (queue.TryDequeue(out var userUnbanned))
                    using (var db = new DbService())
                    {
                        try
                        {
                            var cfg = await db.GetOrCreateGuildConfigAsync(userUnbanned.Guild).ConfigureAwait(false);
                            if (!cfg.LogBan.HasValue) return;
                            var ch = userUnbanned.Guild.GetTextChannel(cfg.LogBan.Value);
                            if (ch == null) return;

                            var caseId = await db.CreateCaseId(userUnbanned.User, userUnbanned.Guild, DateTime.UtcNow,
                                (ModAction) Data.Constants.ModAction.Unban);

                            var msg = await ch.ReplyAsync(new EmbedBuilder().CreateDefault(null, Color.Red.RawValue)
                                .WithAuthor(new EmbedAuthorBuilder
                                {
                                    Name =
                                        $"Case ID: {caseId.Id} - {ActionType.UnBent} | {userUnbanned.User.Username}#{userUnbanned.User.Discriminator}"
                                })
                                .WithFooter(new EmbedFooterBuilder {Text = $"User ID: {userUnbanned.User.Id}"})
                                .WithTimestamp(new DateTimeOffset(DateTime.UtcNow))
                                .WithFields(new List<EmbedFieldBuilder>().ModLogFieldBuilders(userUnbanned.User)));
                            caseId.MessageId = msg.Id;
                            await db.SaveChangesAsync();
                        }
                        catch (Exception e)
                        {
                            //TODO: add logging
                            Console.WriteLine(e);
                        }

                        lastRequest = DateTime.UtcNow;
                    }

                await Task.Delay(TimeSpan.FromMilliseconds(500));
                if (lastRequest.AddHours(10) >= DateTime.UtcNow) worker = false;
            }
        }

        public async Task ProcessUserUpdatedEvent(ulong guildId)
        {
            var queue = UserUpdatedQueue.GetOrAdd(guildId, new ConcurrentQueue<UserUpdated>());
            var worker = true;
            var lastRequest = DateTime.UtcNow;
            while (worker)
            {
                while (queue.TryDequeue(out var userUpdated))
                {
                    if (!(userUpdated.NewUser is SocketGuildUser gusr)) return;
                    using (var db = new DbService())
                    {
                        var cfg = await db.GetOrCreateGuildConfigAsync(gusr.Guild);
                        if (!cfg.LogAvi.HasValue) return;
                        var ch = gusr.Guild.GetTextChannel(cfg.LogAvi.Value);
                        if (ch == null) return;

                        var embed = new EmbedBuilder().CreateDefault(guildId);
                        if (userUpdated.OldUser.Username != userUpdated.NewUser.Username)
                        {
                            var updated = userUpdated;
                            embed.WithTitle("Username Change")
                                .WithDescription(
                                    $"{userUpdated.OldUser.Username}#{updated.OldUser.Discriminator} || {userUpdated.OldUser.Id}")
                                .AddField(x =>
                                    x.WithName("Old Name").WithValue($"{updated.OldUser.Username}").WithIsInline(true))
                                .AddField(x =>
                                    x.WithName("New Name").WithValue($"{updated.NewUser.Username}").WithIsInline(true));
                        }
                        else if (userUpdated.OldUser.AvatarId != userUpdated.NewUser.AvatarId)
                        {
                            embed.WithTitle("Avatar Change")
                                .WithDescription(
                                    $"{userUpdated.OldUser.Username}#{userUpdated.OldUser.Discriminator} | {userUpdated.OldUser.Id}");

                            embed.WithThumbnailUrl(userUpdated.OldUser.GetAvatar());
                            embed.WithImageUrl(userUpdated.NewUser.GetAvatar());
                        }
                        else
                        {
                            return;
                        }

                        await ch.ReplyAsync(embed);

                        lastRequest = DateTime.UtcNow;
                    }
                }

                await Task.Delay(TimeSpan.FromMilliseconds(500));
                if (lastRequest.AddHours(1) >= DateTime.UtcNow) worker = false;
            }
        }

        public async Task ProcessGuildUserUpdatedEvent(ulong guildId)
        {
            var queue = GuildMemberUpdatedQueue.GetOrAdd(guildId, new ConcurrentQueue<GuildUserUpdated>());
            var worker = true;
            var lastRequest = DateTime.UtcNow;
            while (worker)
            {
                while (queue.TryDequeue(out var guildUserUpdated))
                    using (var db = new DbService())
                    {
                        var cfg = await db.GetOrCreateGuildConfigAsync(guildUserUpdated.NewUser.Guild);
                        if (!cfg.LogAvi.HasValue) return;
                        var ch = guildUserUpdated.NewUser.Guild.GetTextChannel(cfg.LogAvi.Value);
                        if (ch == null) return;

                        var embed = new EmbedBuilder().CreateDefault(guildId)
                            .WithTitle(
                                $"{guildUserUpdated.OldUser.Username}#{guildUserUpdated.OldUser.Discriminator} | {guildUserUpdated.OldUser.Id}")
                            .WithFooter(new EmbedFooterBuilder
                                {IconUrl = guildUserUpdated.NewUser.GetAvatar(), Text = ""});

                        if (guildUserUpdated.OldUser.Nickname != guildUserUpdated.NewUser.Nickname)
                        {
                            var updated = guildUserUpdated;
                            embed.WithAuthor(x => x.WithName("Nick Change"))
                                .AddField(
                                    x => x.WithName("Old Nick")
                                        .WithValue($"{updated.OldUser.Nickname}#{updated.OldUser.Discriminator}"))
                                .AddField(
                                    x => x.WithName("New Nick")
                                        .WithValue($"{updated.OldUser.Nickname}#{updated.OldUser.Discriminator}"));
                            await ch.ReplyAsync(embed);
                        }
                        else if (!guildUserUpdated.OldUser.Roles.SequenceEqual(guildUserUpdated.NewUser.Roles))
                        {
                            if (guildUserUpdated.OldUser.Roles.Count < guildUserUpdated.NewUser.Roles.Count)
                            {
                                var updated = guildUserUpdated;
                                var roleDiffer = guildUserUpdated.NewUser.Roles
                                    .Where(x => !updated.OldUser.Roles.Contains(x)).Select(x => x.Name);
                                embed.WithAuthor(x => x.WithName("User role added"))
                                    .WithDescription(string.Join(", ", roleDiffer).SanitizeMentions());
                            }
                            else if (guildUserUpdated.OldUser.Roles.Count > guildUserUpdated.NewUser.Roles.Count)
                            {
                                var updated = guildUserUpdated;
                                var roleDiffer = guildUserUpdated.OldUser.Roles
                                    .Where(x => !updated.NewUser.Roles.Contains(x)).Select(x => x.Name);
                                embed.WithAuthor(x => x.WithName("User role removed"))
                                    .WithDescription(string.Join(", ", roleDiffer).SanitizeMentions());
                            }

                            await ch.ReplyAsync(embed);
                        }

                        lastRequest = DateTime.UtcNow;
                    }

                await Task.Delay(TimeSpan.FromMilliseconds(500));
                if (lastRequest.AddHours(1) >= DateTime.UtcNow) worker = false;
            }
        }
    }
}