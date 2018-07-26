using Discord.Commands;
using Jibril.Services.Entities.Tables;
using System.Collections.Concurrent;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Discord.WebSocket;
using Jibril.Extensions;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Processing.Drawing;
using SixLabors.ImageSharp.Processing.Transforms;
using SixLabors.Primitives;

namespace Jibril.Services.Games.ShipGame
{
    public class ShipGameService
    {
        private ConcurrentDictionary<ulong, ConcurrentDictionary<ulong, GameEnemy>> ActiveBattles { get; }
            = new ConcurrentDictionary<ulong, ConcurrentDictionary<ulong, GameEnemy>>();

        public ShipGameService() { }

        public bool isInBattle(SocketCommandContext context)
        {
            var battles = ActiveBattles.GetOrAdd(context.Guild.Id, new ConcurrentDictionary<ulong, GameEnemy>());
            var check = battles.TryGetValue(context.User.Id, out var game);
            return check;
        }

        public GameEnemy GetEnemyData(SocketCommandContext context)
        {
            var battles = ActiveBattles.GetOrAdd(context.Guild.Id, new ConcurrentDictionary<ulong, GameEnemy>());
            var check = battles.TryGetValue(context.User.Id, out var game);
            return game;
        }

        public void AddBattle(SocketCommandContext context, GameEnemy enemy)
        {
            var battles = ActiveBattles.GetOrAdd(context.Guild.Id, new ConcurrentDictionary<ulong, GameEnemy>());
            battles.TryAdd(context.User.Id, enemy);
        }

        public void RemoveBattle(SocketCommandContext context)
        {
            var battles = ActiveBattles.GetOrAdd(context.Guild.Id, new ConcurrentDictionary<ulong, GameEnemy>());
            battles.TryRemove(context.User.Id, out var game);
        }

        public async Task<Stream> CreateBanner(SocketGuildUser userOne, GameEnemy npc, string gameClass)
        {
            var stream = new MemoryStream();
            using (var img = Image.Load(@"Data\Game\background.png"))
            {
                var border = Image.Load(GetBorder(gameClass));
                var aviOne = await GetAvatarAsync(userOne);
                var aviTwo = await GetAvatarAsync(npc);
                aviOne.Seek(0, SeekOrigin.Begin);
                aviTwo.Seek(0, SeekOrigin.Begin);
                var playerOne = Image.Load(aviOne);
                var playerTwo = Image.Load(aviTwo);
                img.Mutate(x => x
                    .DrawImage(GraphicsOptions.Default, playerOne, new Point(24, 108))
                    .DrawImage(GraphicsOptions.Default, playerTwo, new Point(244, 108))
                    .DrawImage(GraphicsOptions.Default, border, new Point(0, 0)));
                img.Save(stream, new PngEncoder());
            }

            return stream;
        }

        public async Task<Stream> CreateBanner(SocketGuildUser userOne, SocketGuildUser userTwo, string gameClass)
        {
            var stream = new MemoryStream();
            using (var img = Image.Load(@"Data\Game\background.png"))
            {
                var border = Image.Load(GetBorder(gameClass));
                var aviOne = await GetAvatarAsync(userOne);
                var aviTwo = await GetAvatarAsync(userTwo);
                aviOne.Seek(0, SeekOrigin.Begin);
                aviTwo.Seek(0, SeekOrigin.Begin);
                var playerOne = Image.Load(aviOne);
                var playerTwo = Image.Load(aviTwo);
                img.Mutate(x => x
                    .DrawImage(GraphicsOptions.Default, playerOne, new Point(24, 108))
                    .DrawImage(GraphicsOptions.Default, playerTwo, new Point(244, 108))
                    .DrawImage(GraphicsOptions.Default, border, new Point(0, 0)));
                img.Save(stream, new PngEncoder());
            }

            return stream;
        }

        private static string GetBorder(string monsterClass)
        {
            return @"Data\Game\Border\Red-border.png";
        }

        private static async Task<Stream> GetAvatarAsync(SocketGuildUser user)
        {
            var stream = new MemoryStream();
            using (var client = new HttpClient())
            {
                var avatar = await client.GetStreamAsync(user.GetAvatar());
                using (var img = Image.Load(avatar))
                {
                    img.Mutate(x => x.Resize(126, 126));
                    img.Save(stream, new PngEncoder());
                }
            }
            return stream;
        }

        private static async Task<Stream> GetAvatarAsync(GameEnemy npc)
        {
            var stream = new MemoryStream();
            using (var client = new HttpClient())
            {
                var avatar = await client.GetStreamAsync(npc.Image);
                using (var img = Image.Load(avatar))
                {
                    img.Mutate(x => x.Resize(126, 126));
                    img.Save(stream, new PngEncoder());
                }
            }
            return stream;
        }
    }
}