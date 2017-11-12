using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Jibril.Data.Variables;
using Jibril.Modules.Game.Services;
using Jibril.Services.Level.Lists;

namespace Jibril.Services.Level.Services
{
    public class LevelRoles
    {
        public static async Task AssignNewRole(IGuildUser user, int lvl)
        {
            var levelup = GetNewRole(user, lvl).FirstOrDefault();
            var guild = user.Guild;
            var dm2 = await user.GetOrCreateDMChannelAsync().ConfigureAwait(false);
            if (levelup.roleLoss == "0" && levelup.Message == "0")
            {
                var roleAssign = guild.Roles.FirstOrDefault(r => r.Name == levelup.roleGain);
                await user.AddRoleAsync(roleAssign).ConfigureAwait(false);
            }
            else if (levelup.roleLoss == "0")
            {
                var roleAssign = guild.Roles.FirstOrDefault(r => r.Name == levelup.roleGain);
                await user.AddRoleAsync(roleAssign).ConfigureAwait(false);
                try
                {
                    await dm2.SendMessageAsync(levelup.Message).ConfigureAwait(false);
                }
                catch (Exception e)
                {
                    Console.Write(e);
                }
            }
            else if (levelup.Message != "0" && levelup.roleLoss != "0")
            {
                var roleAssign = guild.Roles.FirstOrDefault(r => r.Name == levelup.roleGain);
                var roleRemove = guild.Roles.FirstOrDefault(r => r.Name == levelup.roleLoss);
                await user.AddRoleAsync(roleAssign).ConfigureAwait(false);
                await user.RemoveRoleAsync(roleRemove).ConfigureAwait(false);
                try
                {
                    await dm2.SendMessageAsync(levelup.Message).ConfigureAwait(false);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
        }

        /// List
        /// <RoleLevelResponse>
        public static List<RoleLevelResponse> GetNewRole(IUser user, int lvl)
        {
            var level = lvl + 1;

            // Ship Girl
            if (level == 2)
            {
                //var classStuff = DbService.ClassSearch(2).FirstOrDefault();
                var result = new List<RoleLevelResponse>();
                var roleGain = ClassNames.shipgirl;
                var roleLoss = "0";
                var Message = "0";
                result.Add(new RoleLevelResponse
                {
                    roleGain = roleGain,
                    roleLoss = roleLoss,
                    Message = Message
                });
                return result;
            }
            // Light Cruiser
            if (level == 5)
            {
                var result = new List<RoleLevelResponse>();
                var roleGain = ClassNames.LC;
                var roleLoss = "0";
                var Message = $"Reporting from command HQ.\n" +
                              $"You've been promoted to {ClassNames.LC}!";
                GameDatabase.UpdateClass(user, ClassNames.LC);

                result.Add(new RoleLevelResponse
                {
                    roleGain = roleGain,
                    roleLoss = roleLoss,
                    Message = Message
                });
                return result;
            }
            // Heavy Cruiser
            if (level == 10)
            {
                var result = new List<RoleLevelResponse>();
                var roleGain = ClassNames.HC;
                var roleLoss = ClassNames.LC;
                var Message = $"Reporting from command HQ.\n" +
                              $"You've been promoted to {ClassNames.HC}!";
                GameDatabase.UpdateClass(user, ClassNames.HC);

                result.Add(new RoleLevelResponse
                {
                    roleGain = roleGain,
                    roleLoss = roleLoss,
                    Message = Message
                });
                return result;
            }
            // Destroyer
            if (level == 20)
            {
                var result = new List<RoleLevelResponse>();
                var roleGain = ClassNames.DD;
                var roleLoss = ClassNames.HC;
                var Message = $"Reporting from command HQ.\n" +
                              $"You've been promoted to {ClassNames.DD}!";
                GameDatabase.UpdateClass(user, ClassNames.DD);

                result.Add(new RoleLevelResponse
                {
                    roleGain = roleGain,
                    roleLoss = roleLoss,
                    Message = Message
                });
                return result;
            }
            //Aircraft
            if (level == 30)
            {
                var result = new List<RoleLevelResponse>();
                var roleGain = ClassNames.AC;
                var roleLoss = ClassNames.DD;
                var Message = $"Reporting from command HQ.\n" +
                              $"You've been promoted to {ClassNames.AC}!";
                GameDatabase.UpdateClass(user, ClassNames.AC);

                result.Add(new RoleLevelResponse
                {
                    roleGain = roleGain,
                    roleLoss = roleLoss,
                    Message = Message
                });
                return result;
            }
            // Battleship
            if (level == 40)
            {
                var result = new List<RoleLevelResponse>();
                var roleGain = ClassNames.BB;
                var roleLoss = ClassNames.AC;
                var Message = $"Reporting from command HQ.\n" +
                              $"You've been promoted to {ClassNames.BB}!";
                GameDatabase.UpdateClass(user, ClassNames.BB);

                result.Add(new RoleLevelResponse
                {
                    roleGain = roleGain,
                    roleLoss = roleLoss,
                    Message = Message
                });
                return result;
            }
            return null;
        }
    }
}