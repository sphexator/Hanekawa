using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Jibril.Services.Level.Services
{
    public static class LevelRoles
    {
        /*
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
            var level = lvl;
            using (var db = new hanekawaContext())
            {
                var userdata = db.Exp.Find(user.Id);
                switch (level)
                {
                    // Ship Girl
                    case 2:
                        {
                            var result = new List<RoleLevelResponse>();
                            const string roleGain = ClassNames.Ship;
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
                    case 5:
                        {
                            var result = new List<RoleLevelResponse>();
                            const string roleGain = ClassNames.Lc;
                            const string roleLoss = "0";
                            var message = $"Reporting from command HQ.\n" +
                                          $"You've been promoted to {roleGain}!";
                            userdata.ShipClass = roleGain;
                            db.SaveChanges();

                            result.Add(new RoleLevelResponse
                            {
                                roleGain = roleGain,
                                roleLoss = roleLoss,
                                Message = message
                            });
                            return result;
                        }
                    case 10:
                        {
                            var result = new List<RoleLevelResponse>();
                            const string roleGain = ClassNames.Hc;
                            const string roleLoss = ClassNames.Lc;
                            var message = $"Reporting from command HQ.\n" +
                                          $"You've been promoted to {roleGain}!";
                            userdata.ShipClass = roleGain;
                            db.SaveChanges();

                            result.Add(new RoleLevelResponse
                            {
                                roleGain = roleGain,
                                roleLoss = roleLoss,
                                Message = message
                            });
                            return result;
                        }
                    case 20:
                        {
                            var result = new List<RoleLevelResponse>();
                            const string roleGain = ClassNames.Dd;
                            const string roleLoss = ClassNames.Hc;
                            var message = $"Reporting from command HQ.\n" +
                                          $"You've been promoted to {roleGain}!";
                            userdata.ShipClass = roleGain;
                            db.SaveChanges();

                            result.Add(new RoleLevelResponse
                            {
                                roleGain = roleGain,
                                roleLoss = roleLoss,
                                Message = message
                            });
                            return result;
                        }
                    case 30:
                        {
                            var result = new List<RoleLevelResponse>();
                            const string roleGain = ClassNames.Ac;
                            const string roleLoss = ClassNames.Dd;
                            var message = $"Reporting from command HQ.\n" +
                                          $"You've been promoted to {roleGain}!";
                            userdata.ShipClass = roleGain;
                            db.SaveChanges();

                            result.Add(new RoleLevelResponse
                            {
                                roleGain = roleGain,
                                roleLoss = roleLoss,
                                Message = message
                            });
                            return result;
                        }
                    case 40:
                        {
                            var result = new List<RoleLevelResponse>();
                            const string roleGain = ClassNames.Bb;
                            const string roleLoss = ClassNames.Ac;
                            var message = $"Reporting from command HQ.\n" +
                                          $"You've been promoted to {roleGain}!";
                            userdata.ShipClass = roleGain;
                            db.SaveChanges();

                            result.Add(new RoleLevelResponse
                            {
                                roleGain = roleGain,
                                roleLoss = roleLoss,
                                Message = message
                            });
                            return result;
                        }
                    case 50:
                        {
                            var result = new List<RoleLevelResponse>();
                            const string roleGain = ClassNames.ACr;
                            const string roleLoss = ClassNames.Bb;
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
                    case 65:
                        {
                            var result = new List<RoleLevelResponse>();
                            const string roleGain = ClassNames.Ab;
                            const string roleLoss = ClassNames.ACr;
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
                    case 80:
                        {
                            var result = new List<RoleLevelResponse>();
                            const string roleGain = ClassNames.Tc;
                            const string roleLoss = ClassNames.Ab;
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
                }

            }

            return null;
        }

        public static async Task AssignRoles(Exp userData, SocketGuildUser usr)
        {
            if (userData.Level >= 2 && userData.Level < 5)
                await usr.AddRoleAsync(usr.Guild.Roles.FirstOrDefault(x => x.Name == ClassNames.Ship));
            var roles = GetRoles(userData, usr);
            await usr.AddRolesAsync(roles);
        }

        private static IEnumerable<IRole> GetRoles(Exp userData, SocketGuildUser usr)
        {
            var role = new List<IRole> { usr.Guild.Roles.FirstOrDefault(x => x.Name == ClassNames.Ship) };
            if (userData.Level >= 5 && userData.Level < 10)
                role.Add(usr.Guild.Roles.FirstOrDefault(x => x.Name == ClassNames.Lc));
            if (userData.Level >= 10 && userData.Level < 20)
                role.Add(usr.Guild.Roles.FirstOrDefault(x => x.Name == ClassNames.Hc));
            if (userData.Level >= 20 && userData.Level < 30)
                role.Add(usr.Guild.Roles.FirstOrDefault(x => x.Name == ClassNames.Dd));
            if (userData.Level >= 30 && userData.Level < 40)
                role.Add(usr.Guild.Roles.FirstOrDefault(x => x.Name == ClassNames.Ac));
            if (userData.Level >= 40 && userData.Level < 50)
                role.Add(usr.Guild.Roles.FirstOrDefault(x => x.Name == ClassNames.Bb));
            if (userData.Level >= 50 && userData.Level < 65)
                role.Add(usr.Guild.Roles.FirstOrDefault(x => x.Name == ClassNames.ACr));
            if (userData.Level >= 65 && userData.Level < 80)
                role.Add(usr.Guild.Roles.FirstOrDefault(x => x.Name == ClassNames.Ab));
            if (userData.Level >= 80) role.Add(usr.Guild.Roles.FirstOrDefault(x => x.Name == ClassNames.Tc));
            return role;
        }
        */
    }
}