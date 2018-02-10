using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Jibril.Data.Variables;
using Jibril.Modules.Game.Services;
using Jibril.Services.Level.Lists;

namespace Jibril.Services.Level.Services
{
    public static class LevelRoles
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
        /// <RoleLevelResponse />
        private static IEnumerable<RoleLevelResponse> GetNewRole(IUser user, int lvl)
        {
            var level = lvl + 1;

            // Ship Girl
            if (level == 2)
            {
                var result = new List<RoleLevelResponse>();
                var roleGain = ClassNames.ship;
                const string roleLoss = "0";
                const string message = "0";
                result.Add(new RoleLevelResponse
                {
                    roleGain = roleGain,
                    roleLoss = roleLoss,
                    Message = message
                });
                return result;
            }

            if (level == 5)
            {
                var result = new List<RoleLevelResponse>();
                var roleGain = ClassNames.LC;
                const string roleLoss = "0";
                var message = $"Reporting from command HQ.\n" +
                              $"You've been promoted to {roleGain}!";
                GameDatabase.UpdateClass(user, roleGain);

                result.Add(new RoleLevelResponse
                {
                    roleGain = roleGain,
                    roleLoss = roleLoss,
                    Message = message
                });
                return result;
            }

            if (level == 10)
            {
                var result = new List<RoleLevelResponse>();
                var roleGain = ClassNames.HC;
                var roleLoss = ClassNames.LC;
                var message = $"Reporting from command HQ.\n" +
                              $"You've been promoted to {roleGain}!";
                GameDatabase.UpdateClass(user, roleGain);

                result.Add(new RoleLevelResponse
                {
                    roleGain = roleGain,
                    roleLoss = roleLoss,
                    Message = message
                });
                return result;
            }

            if (level == 20)
            {
                var result = new List<RoleLevelResponse>();
                var roleGain = ClassNames.DD;
                var roleLoss = ClassNames.HC;
                var message = $"Reporting from command HQ.\n" +
                              $"You've been promoted to {roleGain}!";
                GameDatabase.UpdateClass(user, roleGain);

                result.Add(new RoleLevelResponse
                {
                    roleGain = roleGain,
                    roleLoss = roleLoss,
                    Message = message
                });
                return result;
            }

            if (level == 30)
            {
                var result = new List<RoleLevelResponse>();
                var roleGain = ClassNames.AC;
                var roleLoss = ClassNames.DD;
                var message = $"Reporting from command HQ.\n" +
                              $"You've been promoted to {roleGain}!";
                GameDatabase.UpdateClass(user, roleGain);

                result.Add(new RoleLevelResponse
                {
                    roleGain = roleGain,
                    roleLoss = roleLoss,
                    Message = message
                });
                return result;
            }

            if (level == 40)
            {
                var result = new List<RoleLevelResponse>();
                var roleGain = ClassNames.BB;
                var roleLoss = ClassNames.AC;
                var message = $"Reporting from command HQ.\n" +
                              $"You've been promoted to {roleGain}!";
                GameDatabase.UpdateClass(user, roleGain);

                result.Add(new RoleLevelResponse
                {
                    roleGain = roleGain,
                    roleLoss = roleLoss,
                    Message = message
                });
                return result;
            }

            if (level == 50)
            {
                var result = new List<RoleLevelResponse>();
                var roleGain = ClassNames.ACr;
                var roleLoss = ClassNames.BB;
                var message = $"Reporting from command HQ.\n" +
                              $"You've been promoted to {roleGain}!";
                //GameDatabase.UpdateClass(user, roleGain);

                result.Add(new RoleLevelResponse
                {
                    roleGain = roleGain,
                    roleLoss = roleLoss,
                    Message = message
                });
                return result;
            }

            if (level == 65)
            {
                var result = new List<RoleLevelResponse>();
                var roleGain = ClassNames.AB;
                var roleLoss = ClassNames.ACr;
                var message = $"Reporting from command HQ.\n" +
                              $"You've been promoted to {roleGain}!";
                //GameDatabase.UpdateClass(user, roleGain);

                result.Add(new RoleLevelResponse
                {
                    roleGain = roleGain,
                    roleLoss = roleLoss,
                    Message = message
                });
                return result;
            }

            if (level == 80)
            {
                var result = new List<RoleLevelResponse>();
                var roleGain = ClassNames.TC;
                var roleLoss = ClassNames.AB;
                var message = $"Reporting from command HQ.\n" +
                              $"You've been promoted to {roleGain}!";
                //GameDatabase.UpdateClass(user, roleGain);

                result.Add(new RoleLevelResponse
                {
                    roleGain = roleGain,
                    roleLoss = roleLoss,
                    Message = message
                });
                return result;
            }

            return null;
        }

        public static async Task AssignRoles(UserData userData, SocketGuildUser usr)
        {
            if (userData.Level >= 2 && userData.Level < 5)
                await usr.AddRoleAsync(usr.Guild.Roles.FirstOrDefault(x => x.Name == ClassNames.ship));
            var roles = GetRoles(userData, usr);
            await usr.AddRolesAsync(roles);
        }

        private static IEnumerable<IRole> GetRoles(UserData userData, SocketGuildUser usr)
        {
            var role = new List<IRole> {usr.Guild.Roles.FirstOrDefault(x => x.Name == ClassNames.ship)};
            if (userData.Level >= 5 && userData.Level < 10)
                role.Add(usr.Guild.Roles.FirstOrDefault(x => x.Name == ClassNames.LC));
            if (userData.Level >= 10 && userData.Level < 20)
                role.Add(usr.Guild.Roles.FirstOrDefault(x => x.Name == ClassNames.HC));
            if (userData.Level >= 20 && userData.Level < 30)
                role.Add(usr.Guild.Roles.FirstOrDefault(x => x.Name == ClassNames.DD));
            if (userData.Level >= 30 && userData.Level < 40)
                role.Add(usr.Guild.Roles.FirstOrDefault(x => x.Name == ClassNames.AC));
            if (userData.Level >= 40 && userData.Level < 50)
                role.Add(usr.Guild.Roles.FirstOrDefault(x => x.Name == ClassNames.BB));
            if (userData.Level >= 50 && userData.Level < 65)
                role.Add(usr.Guild.Roles.FirstOrDefault(x => x.Name == ClassNames.ACr));
            if (userData.Level >= 65 && userData.Level < 80)
                role.Add(usr.Guild.Roles.FirstOrDefault(x => x.Name == ClassNames.AB));
            if (userData.Level >= 80) role.Add(usr.Guild.Roles.FirstOrDefault(x => x.Name == ClassNames.TC));
            return role;
        }
    }
}