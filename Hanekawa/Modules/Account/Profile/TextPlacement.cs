using Hanekawa.Addons.Database;
using Hanekawa.Addons.Database.Extensions;
using Hanekawa.Addons.Database.Tables.Account;
using Hanekawa.Extensions;
using Microsoft.EntityFrameworkCore;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.Primitives;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hanekawa.Services.Level.Util;

namespace Hanekawa.Modules.Account.Profile
{
    public static class TextPlacement
    {
        public static async Task ApplyTextAsync(this IImageProcessingContext<Rgba32> image, string name, ulong userId,
            ulong guildId,
            Addons.Database.Tables.Account.Account userdata, LevelGenerator levelGenerator)
        {
            using (var db = new DbService())
            {
                var fields = db.ProfileConfigs.ToListAsync();
                var globalData = db.GetOrCreateGlobalUserData(userId);
                await Task.WhenAll(fields, globalData);
                var fonts = new FontCollection();
                var arial = fonts.Install(@"Data/Fonts/ARIAL.TTF");
                var font = new Font(arial, 20, FontStyle.Regular);//SystemFonts.CreateFont("Arial", 20, FontStyle.Regular);
                var nameFont = new Font(arial, 32, FontStyle.Regular);//SystemFonts.CreateFont("Arial", 32, FontStyle.Regular);

                var nameOptions = new TextGraphicsOptions {HorizontalAlignment = HorizontalAlignment.Center};
                var leftOptions = new TextGraphicsOptions {HorizontalAlignment = HorizontalAlignment.Left};
                var rightOptions = new TextGraphicsOptions {HorizontalAlignment = HorizontalAlignment.Right};
                image.DrawText(nameOptions, name, nameFont, Rgba32.WhiteSmoke, new PointF(200, 118));
                foreach (var x in fields.Result)
                    if (x.Name == "Achievement Points")
                    {
                        image.DrawText(leftOptions, x.Value, font, Rgba32.White, new PointF(x.NameWidth, x.Height));
                        image.DrawText(rightOptions,
                            await GetValueAsync(x.Name, db, userdata, globalData.Result, levelGenerator, guildId), font,
                            Rgba32.White,
                            new PointF(x.ValueWidth, x.Height));
                    }
                    else
                    {
                        image.DrawText(leftOptions, x.Value, font, Rgba32.White, new PointF(x.NameWidth, x.Height));
                        image.DrawText(rightOptions,
                            await GetValueAsync(x.Name, db, userdata, globalData.Result, levelGenerator, guildId), font,
                            Rgba32.White,
                            new PointF(x.ValueWidth, x.Height));
                    }
            }
        }

        public static void ApplyAchievementCircles(this IImageProcessingContext<Rgba32> image, Image<Rgba32> circle,
            IEnumerable<Image<Rgba32>> icons)
        {
            const int height = 306;
            const int width = 22;
            var images = icons.ToList();
            if (images.Count > 4)
            {
                circle.Mutate(x => x.Resize(39, 39));
                const int spacerW = 45;
                const int spacerH = 45;
                var amount = 0;
                for (var i = 0; i < 2; i++)
                for (var j = 0; j < 8; j++)
                {
                    if (amount >= images.Count) continue;
                    var icon = images[amount];
                    icon.Mutate(y => y.Resize(39, 39));
                    image.DrawImage(new GraphicsOptions(false), icon,
                        new Point(width + spacerW * j, height + spacerH * i));
                    amount++;
                }

                for (var i = 0; i < 2; i++)
                for (var j = 0; j < 8; j++)
                    image.DrawImage(new GraphicsOptions(false), circle,
                        new Point(width + spacerW * j, height + spacerH * i));
            }
            else
            {
                const int spacerW = 93;
                var amount = 0;
                foreach (var x in images)
                {
                    image.DrawImage(new GraphicsOptions(false), x, new Point(width + spacerW * amount, height));
                    amount++;
                }

                for (var i = 0; i < 4; i++)
                    image.DrawImage(new GraphicsOptions(false), circle, new Point(width + spacerW * i, height));
            }
        }

        private static async Task<string> GetValueAsync(string name, DbService db,
            Addons.Database.Tables.Account.Account userdata,
            AccountGlobal globalData, LevelGenerator levelGenerator, ulong guildId)
        {
            switch (name)
            {
                case "Server":
                    return "";
                case "Global":
                    return "";
                case "Rank":
                    return await GetRankAsync(db, userdata, guildId: guildId);
                case "Level":
                    return $"{userdata.Level}";
                case "Exp":
                    return
                        $"{userdata.Exp.FormatNumber()}/{levelGenerator.GetServerLevelRequirement(userdata.Level).FormatNumber()}";
                //case "TotalExp":
                //    return $"{userdata.TotalExp}";
                case "Credit":
                    return $"{userdata.Credit.FormatNumber()}";
                case "Special Credit":
                    return $"{userdata.CreditSpecial.FormatNumber()}";
                case "Achievement Points":
                    return $"{await GetAchievementPoints(db, userdata.UserId)}";
                case "Global Rank":
                    return await GetRankAsync(db, null, globalData);
                case "Global Credit":
                    return $"{globalData.Credit.FormatNumber()}";
                case "Global Exp":
                    return
                        $"{globalData.Exp.FormatNumber()}/{levelGenerator.GetGlobalLevelRequirement(globalData.Level).FormatNumber()}";
                case "Global TotalExp":
                    return $"{globalData.TotalExp.FormatNumber()}";
                case "Global Level":
                    return $"{globalData.Level}";
                default:
                    return "";
            }
        }

        private static async Task<string> GetRankAsync(DbService db,
            Addons.Database.Tables.Account.Account userdata = null,
            AccountGlobal globalData = null, ulong? guildId = null)
        {
            if (userdata != null)
            {
                var total = await db.Accounts.CountAsync(x => x.GuildId == guildId.Value);
                var rank = await db.Accounts.CountAsync(x =>
                    x.TotalExp >= userdata.TotalExp && x.GuildId == guildId.Value);
                return $"{rank.FormatNumber()}/{total.FormatNumber()}";
            }
            else
            {
                var total = await db.AccountGlobals.CountAsync();
                var rank = await db.AccountGlobals.CountAsync(x => x.TotalExp >= globalData.TotalExp);
                return $"{rank.FormatNumber()}/{total.FormatNumber()}";
            }
        }

        private static async Task<int> GetAchievementPoints(DbService db, ulong userid)
        {
            var achievements = await db.AchievementUnlocks.CountAsync(x => x.UserId == userid);
            return 10 * achievements;
        }
    }
}