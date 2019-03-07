using Hanekawa.Addons.Database;
using Hanekawa.Addons.Database.Extensions;
using Hanekawa.Addons.Database.Tables.Account;
using Hanekawa.Entities.Interfaces;
using Hanekawa.Extensions;
using Hanekawa.Services.Level.Util;
using Microsoft.EntityFrameworkCore;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.Primitives;
using System.Threading.Tasks;

namespace Hanekawa.Modules.Account.Profile
{
    public class TextPlacement : IHanaService
    {
        private readonly FontCollection _fonts;
        private readonly FontFamily _arial;
        private readonly Font _regular;
        private readonly Font _name;

        private readonly TextGraphicsOptions _nameOptions =
            new TextGraphicsOptions { HorizontalAlignment = HorizontalAlignment.Center };
        private readonly TextGraphicsOptions _rightOptions =
            new TextGraphicsOptions { HorizontalAlignment = HorizontalAlignment.Right };
        private readonly TextGraphicsOptions _leftOptions =
            new TextGraphicsOptions { HorizontalAlignment = HorizontalAlignment.Left };

        public TextPlacement()
        {
            _fonts = new FontCollection();
            _arial = _fonts.Install(@"Data/Fonts/ARIAL.TTF");
            _regular = new Font(_arial, 20, FontStyle.Regular);
            _name = new Font(_arial, 32, FontStyle.Regular);
        }

        public async Task ApplyTextAsync(Image<Rgba32> image, string name, ulong userId,
            ulong guildId,
            Addons.Database.Tables.Account.Account userdata, LevelGenerator levelGenerator)
        {
            using (var db = new DbService())
            {
                var fields = db.ProfileConfigs.ToListAsync();
                var globalData = db.GetOrCreateGlobalUserData(userId);
                await Task.WhenAll(fields, globalData);
                image.Mutate(x => x.DrawText(_nameOptions, name, _name, Rgba32.WhiteSmoke, new PointF(200, 118)));
                foreach (var x in fields.Result)

                        if (x.Name == "Achievement Points")
                        {
                            var value = await GetValueAsync(x.Name, db, userdata, globalData.Result, levelGenerator,
                                guildId);
                            image.Mutate(z => z.DrawText(_leftOptions, x.Value, _regular, Rgba32.White,
                                new PointF(x.NameWidth, x.Height)));
                            image.Mutate(z => z.DrawText(_rightOptions, value, _regular, Rgba32.White,
                                new PointF(x.ValueWidth, x.Height)));
                        }
                        else
                        {
                            var value = await GetValueAsync(x.Name, db, userdata, globalData.Result, levelGenerator,
                                guildId);
                            image.Mutate(z => z.DrawText(_leftOptions, x.Value, _regular, Rgba32.White,
                                new PointF(x.NameWidth, x.Height)));
                            image.Mutate(z => z.DrawText(_rightOptions, value, _regular, Rgba32.White,
                                new PointF(x.ValueWidth, x.Height)));
                        }
            }
        }

        private async Task<string> GetValueAsync(string name, DbService db,
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

        private async Task<string> GetRankAsync(DbService db,
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

        private async Task<int> GetAchievementPoints(DbService db, ulong userid)
        {
            var achievements = await db.AchievementUnlocks.CountAsync(x => x.UserId == userid);
            return 10 * achievements;
        }
    }
}