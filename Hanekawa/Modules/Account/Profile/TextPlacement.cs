using Hanekawa.Addons.Database;
using Hanekawa.Addons.Database.Extensions;
using Hanekawa.Addons.Database.Tables.Account;
using Microsoft.EntityFrameworkCore;
using SixLabors.Fonts;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Processing.Text;
using SixLabors.Primitives;
using System.Threading.Tasks;
using Hanekawa.Extensions;
using Hanekawa.Services.Level.Util;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing.Drawing;

namespace Hanekawa.Modules.Account.Profile
{
    public static class TextPlacement
    {
        public static async Task ApplyTextAsync(this IImageProcessingContext<Rgba32> image, string name, ulong userId, ulong guildId, 
            Addons.Database.Tables.Account.Account userdata)
        {
            using (var db = new DbService())
            {
                var fields = db.ProfileConfigs.ToListAsync();
                var globalData = db.GetOrCreateGlobalUserData(userId);
                await Task.WhenAll(fields, globalData);
                var calc = new Calculate();
                var font = SystemFonts.CreateFont("Arial", 20, FontStyle.Regular);
                var nameFont = SystemFonts.CreateFont("Arial", 32, FontStyle.Regular);
                var nameOptions = new TextGraphicsOptions { HorizontalAlignment = HorizontalAlignment.Center };
                var leftOptions = new TextGraphicsOptions{HorizontalAlignment = HorizontalAlignment.Left};
                var rightOptions = new TextGraphicsOptions{HorizontalAlignment = HorizontalAlignment.Right};
                image.DrawText(nameOptions, name, nameFont, Rgba32.WhiteSmoke, new PointF(200, 118));
                foreach (var x in fields.Result)
                {
                    if (x.Name == "Achievement Points")
                    {
                        image.DrawText(leftOptions, x.Value, font, Rgba32.White, new PointF(x.NameWidth, x.Height));
                        image.DrawText(rightOptions, await GetValueAsync(x.Name, db, userdata, globalData.Result, calc, guildId), font, Rgba32.White,
                            new PointF(x.ValueWidth, x.Height));
                    }
                    else
                    {
                        image.DrawText(leftOptions, x.Value, font, Rgba32.White, new PointF(x.NameWidth, x.Height + 15));
                        image.DrawText(rightOptions, await GetValueAsync(x.Name, db, userdata, globalData.Result, calc, guildId), font, Rgba32.White,
                            new PointF(x.ValueWidth, x.Height + 15));
                    }
                }
            }
        }

        public static void ApplyAchievementCircles(this IImageProcessingContext<Rgba32> image, Image<Rgba32> circle)
        {
            const int width = 22;
            const int height = 306;
            for (var i = 0; i < 3; i++)
            {
                for (var j = 0; j < 12; j++)
                {
                    image.DrawImage(new GraphicsOptions(false), circle, new Point(width + (30 * j), height + (27 * i)));
                }
            }
        }

        private static async Task<string> GetValueAsync(string name, DbService db, Addons.Database.Tables.Account.Account userdata,
            AccountGlobal globalData, Calculate calc, ulong guildId)
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
                    return $"{userdata.Exp.FormatNumber()}/{calc.GetServerLevelRequirement(userdata.Level).FormatNumber()}";
                //case "TotalExp":
                //    return $"{userdata.TotalExp}";
                case "Credit":
                    return $"{userdata.Credit.FormatNumber()}";
                case "Special Credit":
                    return $"{userdata.CreditSpecial.FormatNumber()}";
                case "Achievement Points":
                    return "0";
                case "Global Rank":
                    return await GetRankAsync(db, null, globalData);
                case "Global Credit":
                    return $"{globalData.Credit.FormatNumber()}";
                case "Global Exp":
                    return $"{globalData.Exp.FormatNumber()}/{calc.GetGlobalLevelRequirement(globalData.Level).FormatNumber()}";
                case "Global TotalExp":
                    return $"{globalData.TotalExp.FormatNumber()}";
                case "Global Level":
                    return $"{globalData.Level}";
                default:
                    return "";
            }
        }

        private static async Task<string> GetRankAsync(DbService db, Addons.Database.Tables.Account.Account userdata = null,
            AccountGlobal globalData = null, ulong? guildId = null)
        {
            if (userdata != null)
            {
                var total = await db.Accounts.CountAsync();
                var rank = await db.Accounts.CountAsync(x => x.TotalExp >= userdata.TotalExp && x.GuildId == guildId.Value);
                return $"{rank.FormatNumber()}/{total.FormatNumber()}";
            }
            else
            {
                var total = await db.AccountGlobals.CountAsync();
                var rank = await db.AccountGlobals.CountAsync(x => x.TotalExp >= globalData.TotalExp);
                return $"{rank.FormatNumber()}/{total.FormatNumber()}";
            }
        }
    }
}