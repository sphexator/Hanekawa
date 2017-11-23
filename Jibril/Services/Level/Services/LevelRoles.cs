using Discord;
using Discord.WebSocket;
using Jibril.Data.Variables;
using Jibril.Modules.Game.Services;
using Jibril.Services.Level.Lists;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
                var result = new List<RoleLevelResponse>();
                var roleGain = ClassNames.ship;
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
                              $"You've been promoted to {roleGain}!";
                GameDatabase.UpdateClass(user, roleGain);

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
                              $"You've been promoted to {roleGain}!";
                GameDatabase.UpdateClass(user, roleGain);

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
                              $"You've been promoted to {roleGain}!";
                GameDatabase.UpdateClass(user, roleGain);

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
                              $"You've been promoted to {roleGain}!";
                GameDatabase.UpdateClass(user, roleGain);

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
                              $"You've been promoted to {roleGain}!";
                GameDatabase.UpdateClass(user, roleGain);

                result.Add(new RoleLevelResponse
                {
                    roleGain = roleGain,
                    roleLoss = roleLoss,
                    Message = Message
                });
                return result;
            }
            // Aviation Cruiser
            if (level == 50)
            {
                var result = new List<RoleLevelResponse>();
                var roleGain = ClassNames.ACr;
                var roleLoss = ClassNames.BB;
                var Message = $"Reporting from command HQ.\n" +
                              $"You've been promoted to {roleGain}!";
                //GameDatabase.UpdateClass(user, roleGain);

                result.Add(new RoleLevelResponse
                {
                    roleGain = roleGain,
                    roleLoss = roleLoss,
                    Message = Message
                });
                return result;
            }
            // Aviation Battleship
            if (level == 60)
            {
                var result = new List<RoleLevelResponse>();
                var roleGain = ClassNames.AB;
                var roleLoss = ClassNames.ACr;
                var Message = $"Reporting from command HQ.\n" +
                              $"You've been promoted to {roleGain}!";
                //GameDatabase.UpdateClass(user, roleGain);

                result.Add(new RoleLevelResponse
                {
                    roleGain = roleGain,
                    roleLoss = roleLoss,
                    Message = Message
                });
                return result;
            }
            // Training Cruiser
            if (level == 70)
            {
                var result = new List<RoleLevelResponse>();
                var roleGain = ClassNames.TC;
                var roleLoss = ClassNames.AB;
                var Message = $"Reporting from command HQ.\n" +
                              $"You've been promoted to {roleGain}!";
                //GameDatabase.UpdateClass(user, roleGain);

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
        /*
        private static Boolean CheckUserRoles(SocketGuildUser user, ulong roleid)
        {
            var guild = user.Guild;
            var role = guild.Roles.First(x => x.Id == roleid);
            if (user.Roles.Contains(role) != true) return false;
            return false;
        }
        */
    }
}