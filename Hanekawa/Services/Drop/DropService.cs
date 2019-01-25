using Discord;
using Discord.WebSocket;
using Hanekawa.Entities.Interfaces;
using System;
using System.Linq;
using System.Threading.Tasks;
using Hanekawa.Services.Level;

namespace Hanekawa.Services.Drop
{
    public class DropService : IHanaService, IRequiredService
    {
        private readonly DiscordSocketClient _client;
        private readonly DropData _data;
        private readonly DropEmote _emote;
        private readonly Random _random;
        private readonly ExperienceHandler _experience;

        public DropService(DiscordSocketClient client, Random random, DropData data, DropEmote emote, ExperienceHandler experience)
        {
            _client = client;
            _random = random;
            _data = data;
            _emote = emote;
            _experience = experience;
            _client.MessageReceived += CrateTriggerAsync;
            _client.ReactionAdded += CrateClaimerAsync;
        }

        private Task CrateClaimerAsync(Cacheable<IUserMessage, ulong> msg, ISocketMessageChannel channel,
            SocketReaction rct)
        {
            var _ = Task.Run(async () =>
            {
                if (!msg.HasValue) return;
                if (rct.User.Value.IsBot) return;
                if (rct.Emote.Name != "realsip") return;
                if (!(rct.User.Value is SocketGuildUser user)) return;
                if (!_data.IsLootMessage(user.Guild.Id, msg.Id, out var special)) return;
                var message = await msg.GetOrDownloadAsync();
                if (special) await ClaimSpecial(message, channel, user);
                else await ClaimNormal(message, channel, user);
            });
            return Task.CompletedTask;
        }

        private Task CrateTriggerAsync(SocketMessage msg)
        {
            var _ = Task.Run(async () =>
            {
                if (!(msg.Channel is ITextChannel ch)) return;
                if (msg.Author.IsBot) return;
                if (!(msg.Author is SocketGuildUser user)) return;
                if (!_data.IsLootChannel(ch.Guild.Id, ch.Id)) return;
                if (_data.OnUserCooldown(user)) return;
                var rand = _random.Next(0, 10000);
                if (rand < 200)
                {
                    if (_data.OnGuildCooldown(ch)) return;
                    var triggerMsg = await ch.SendMessageAsync(
                        "A drop event has been triggered \nClick the roosip reaction on this message to claim it!");
                    var emotes = _emote.ReturnEmotes();
                    foreach (var x in emotes.OrderBy(x => _random.Next()).Take(emotes.Count))
                    {
                        if (x.Name == "realsip") _data.AddRegular(ch.Guild, triggerMsg);
                        await triggerMsg.AddReactionAsync(x);
                    }
                }
            });
            return Task.CompletedTask;
        }

        public async Task SpawnCrateAsync(SocketTextChannel ch, SocketGuildUser user)
        {
            var triggerMsg = await ch.SendMessageAsync(
                $"```{user.Username} has spawned a crate! \nClick the reaction on this message to claim it```");
            var emotes = _emote.ReturnEmotes();
            foreach (var x in emotes.OrderBy(x => _random.Next()).Take(emotes.Count))
                try
                {
                    if (x.Name == "realsip") _data.AddSpecial(user.Guild, triggerMsg);
                    await triggerMsg.AddReactionAsync(x);
                }
                catch
                {
                    break;
                }
        }

        private async Task ClaimSpecial(IMessage msg, ISocketMessageChannel channel, IGuildUser user)
        {
            await msg.DeleteAsync();
            _data.RemoveSpecial(user.Guild, msg);
            var rand = _random.Next(150, 250);
            await _experience.AddDropExp(user, rand, rand);
            var trgMsg =
                await channel.SendMessageAsync(
                    $"Rewarded {user.Mention} with {rand} exp & credit!");
            await Task.Delay(5000);
            await trgMsg.DeleteAsync();
        }

        private async Task ClaimNormal(IMessage msg, ISocketMessageChannel channel, IGuildUser user)
        {
            await msg.DeleteAsync();
            _data.RemoveRegular(user.Guild, msg);
            var rand = _random.Next(15, 150);
            await _experience.AddDropExp(user, rand, rand);
            var trgMsg =
                await channel.SendMessageAsync(
                    $"Rewarded {user.Mention} with {rand} exp & credit!");
            await Task.Delay(5000);
            await trgMsg.DeleteAsync();
        }
    }
}