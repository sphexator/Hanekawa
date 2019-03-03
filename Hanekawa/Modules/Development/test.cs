using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.WebSocket;
using Hanekawa.Extensions.Embed;
using Hanekawa.Modules.Account.Profile;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.Shapes;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Hanekawa.Addons.Database;
using Hanekawa.Addons.Database.Extensions;
using Hanekawa.Services.Level.Util;
using SixLabors.Primitives;

namespace Hanekawa.Modules.Development
{
    public class Test : InteractiveBase
    {
        private readonly ProfileGenerator _generator;

        public Test(ProfileGenerator generator)
        {
            _generator = generator;
        }

        [Command("roleid", RunMode = RunMode.Async)]
        [RequireOwner]
        public async Task GetRoleID([Remainder] string roleName)
        {
            var role = Context.Guild.Roles.FirstOrDefault(x => x.Name == roleName);
            await ReplyAsync($"{role.Name}\n" +
                             $"{role.Id}\n" +
                             $"{role.Color.RawValue}\n" +
                             $"{role.Position}");
        }

        [Command("shard")]
        [RequireOwner]
        public async Task CheckShards()
        {
            var shards = await Context.Client.GetRecommendedShardCountAsync();
            await Context.ReplyAsync($"Current recommended shard count is: {shards}");
        }

        [Command("test", RunMode = RunMode.Async)]
        [RequireOwner]
        [RequireContext(ContextType.Guild)]
        public async Task TestRules(int size)
        {
            using (var profile = await _generator.Create(Context.User as SocketGuildUser))
            {
                profile.Seek(0, SeekOrigin.Begin);
                await Context.Channel.SendFileAsync(profile, "banner.png");
            }
        }

        [Command("image2", RunMode = RunMode.Async)]
        [RequireOwner]
        [Priority(1)]
        [RequireContext(ContextType.Guild)]
        public async Task TestImageTwo()
        {
            var stream = new MemoryStream();
            using (var img = new Image<Rgba32>(300, 300))
            {
                img.Mutate(x => x
                    .BackgroundColor(Rgba32.AntiqueWhite)
                    .DrawBeziers(GraphicsOptions.Default, Rgba32.Black, 1, 
                        new PointF(10, 10), 
                        new PointF(10, 30), 
                        new PointF(30, 30),
                        new PointF(30, 30))
                );
                img.Save(stream, new PngEncoder());
            }

            stream.Seek(0, SeekOrigin.Begin);
            await Context.Channel.SendFileAsync(stream, "test.png", "test");
        }

        [Command("image", RunMode = RunMode.Async)]
        [RequireOwner]
        [RequireContext(ContextType.Guild)]
        public async Task TestImage()
        {
            var stream = new MemoryStream();
            using(var db = new DbService())
            using (var img = new Image<Rgba32>(300, 300))
            {
                var circle = new EllipsePolygon(155, 150, 50).GenerateOutline(1, new ReadOnlySpan<float>(new float[1]));
                //var circle1 = new EllipsePolygon(155, 150, 40);
                var userdata = await db.GetOrCreateUserData(Context.User as IGuildUser);
                var percentage = userdata.Exp / (float)new LevelGenerator().GetServerLevelRequirement(userdata.Level);
                img.Mutate(x => x
                    .BackgroundColor(Rgba32.AntiqueWhite)
                    .Fill(new GraphicsOptions(true), Rgba32.Black, circle));
                img.Save(stream, new PngEncoder());
            }

            stream.Seek(0, SeekOrigin.Begin);
            await Context.Channel.SendFileAsync(stream, "test.png", "test");
        }

        private int Amount(float percentage)
        {
            if (percentage >= 0.75) return 3;
            if (percentage >= 0.50) return 2;
            return percentage >= 0.25 ? 1 : 0;
        }
    }
}