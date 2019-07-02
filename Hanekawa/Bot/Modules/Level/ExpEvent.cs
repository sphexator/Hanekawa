using System;
using System.Threading.Tasks;
using Discord;
using Hanekawa.Bot.Preconditions;
using Hanekawa.Database;
using Hanekawa.Database.Extensions;
using Hanekawa.Extensions.Embed;
using Qmmands;

namespace Hanekawa.Bot.Modules.Level
{
    public partial class Level
    {
        [Name("Exp Multiplier")]
        [Command("expmulti")]
        [Description("Checks what the current exp multiplier is")]
        [RequiredChannel]
        public async Task ExpMultiplierAsync() 
            => await Context.ReplyAsync($"Current multiplier is: {_exp.GetMultiplier(Context.Guild.Id)}");

        [Name("Text Exp Multiplier")]
        [Command("txtexpmulti", "tem")]
        [Description("Sets a new exp multiplier permanently")]
        [RequireUserPermission(GuildPermission.ManageGuild)]
        public async Task TextExpMultiplierAsync(double multiplier)
        {
            using (var db = new DbService())
            {
                var old = _exp.GetMultiplier(Context.Guild.Id);
                var cfg = await db.GetOrCreateLevelConfigAsync(Context.Guild);
                cfg.TextExpMultiplier = multiplier;
                await db.SaveChangesAsync();
                _exp.AdjustMultiplier(Context.Guild.Id, multiplier);
                await Context.ReplyAsync($"Changed text exp multiplier from {old} to {multiplier}", Color.Green.RawValue);
            }
        }

        [Name("Voice Exp Multiplier")]
        [Command("voiceexpmulti", "vem")]
        [Description("Sets a new exp multiplier permanently")]
        [RequireUserPermission(GuildPermission.ManageGuild)]
        public async Task VoiceExpMultiplierAsync(double multiplier)
        {
            using (var db = new DbService())
            {
                var old = _exp.GetMultiplier(Context.Guild.Id);
                var cfg = await db.GetOrCreateLevelConfigAsync(Context.Guild);
                cfg.VoiceExpMultiplier = multiplier;
                await db.SaveChangesAsync();
                _exp.AdjustMultiplier(Context.Guild.Id, multiplier);
                await Context.ReplyAsync($"Changed voice exp multiplier from {old} to {multiplier}", Color.Green.RawValue);
            }
        }

        [Name("Enable/Disable Voice Exp")]
        [Command("voicexp", "ve")]
        [Description("Enable/Disables experience gained from being in voice channels")]
        [RequireUserPermission(GuildPermission.ManageGuild)]
        public async Task ToggleVoiceExp()
        {
            using (var db = new DbService())
            {
                var old = _exp.GetMultiplier(Context.Guild.Id);
                var cfg = await db.GetOrCreateLevelConfigAsync(Context.Guild);
                if (cfg.VoiceExpEnabled)
                {
                    cfg.VoiceExpEnabled = false;
                    await Context.ReplyAsync("Disabled experience gained from being in voice channels!", Color.Green.RawValue);
                }
                else
                {
                    cfg.VoiceExpEnabled = true;
                    await Context.ReplyAsync("Enabled experience gained from being in voice channels!", Color.Green.RawValue);
                }
                await db.SaveChangesAsync();
            }
        }

        [Name("Exp Event")]
        [Command("expevent")]
        [Description("Starts a exp event with a specified multiplier for a period of time")]
        [RequireUserPermission(GuildPermission.ManageGuild)]
        public async Task ExpEventAsync(double multiplier, TimeSpan? duration = null)
        {
            if (multiplier <= 0) return;
            if (!duration.HasValue) duration = TimeSpan.FromDays(1);
            using var db = new DbService();
            // TODO Create exp event
            
        }
    }
}
