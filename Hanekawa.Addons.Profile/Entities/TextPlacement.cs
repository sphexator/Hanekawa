using System.Threading.Tasks;
using Hanekawa.Addons.Database;
using Hanekawa.Addons.Database.Extensions;
using Hanekawa.Addons.Database.Tables.Account;
using Microsoft.EntityFrameworkCore;
using SixLabors.Fonts;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Processing.Text;
using SixLabors.Primitives;

namespace Hanekawa.Addons.Profile.Entities
{
    public static class TextPlacement
    {
        public static async Task ApplyTextAsync(this IImageProcessingContext<Rgba32> image, DbService db, ulong userId,
            ulong guildId)
        {
            var fields = await db.ProfileConfigs.ToListAsync();
            var userdata = await db.GetOrCreateUserData(guildId, userId);
            var globalData = await db.GetOrCreateGlobalUserData(userId);
            var font = SystemFonts.CreateFont("Times New Roman", 12, FontStyle.Regular);
            foreach (var x in fields)
            {
                image.DrawText(x.Name, font, Rgba32.White, new PointF(x.NameWidth, x.Height));
                image.DrawText(await GetValueAsync(x.Value, db, userdata, globalData), font, Rgba32.White,
                    new PointF(x.ValueWidth, x.Height));
            }
        }

        private static async Task<string> GetValueAsync(string name, DbService db, Account userdata,
            AccountGlobal globalData)
        {
            switch (name)
            {
                case "Rank":
                    return await GetRankAsync(db, userdata);
                case "Level":
                    return $"{userdata.Level}";
                case "Credit":
                     return $"{userdata.Credit}";
                case "Special Credit":
                case "Achievement Points":
                    return null;
                case "Global Rank":
                    return await GetRankAsync(db, null, globalData);
                case "Global Credit":
                    return $"{globalData.Credit}";
                default:
                    return null;
            }
        }

        private static async Task<string> GetRankAsync(DbService db, Account userdata = null,
            AccountGlobal globalData = null)
        {
            if (userdata != null)
            {
                var total = await db.Accounts.CountAsync();
                var rank = await db.Accounts.CountAsync(x => x.TotalExp >= userdata.TotalExp);
                return $"{rank}/{total}";
            }
            else
            {
                var total = await db.AccountGlobals.CountAsync();
                var rank = await db.AccountGlobals.CountAsync(x => x.TotalExp >= globalData.TotalExp);
                return $"{rank}/{total}";
            }
        }

        private static string ClubNames(string name)
        {
            switch (name)
            {
                case "clubLeader":
                    return "Leader";
                case "ClubAmount":
                    return "Members";
                case "clubRank":
                    return "Rank";
                default:
                    return null;
            }
        }
    }
}