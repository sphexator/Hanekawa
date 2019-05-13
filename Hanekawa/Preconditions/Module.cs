using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Discord.Commands;
using Discord.WebSocket;

namespace Hanekawa.Preconditions
{
    public class Module : PreconditionAttribute
    {
        private readonly ConcurrentDictionary<ulong, ConcurrentDictionary<ulong, ModuleInformation>> _modules = new ConcurrentDictionary<ulong, ConcurrentDictionary<ulong, ModuleInformation>>();
        private readonly ConcurrentDictionary<ulong, bool> _ignoreAll = new ConcurrentDictionary<ulong, bool>();
        private readonly ConcurrentDictionary<ulong, List<ulong>> _listedChannels =
            new ConcurrentDictionary<ulong, List<ulong>>();

        public override Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command,
            IServiceProvider services)
        {
            if (!(context.User is SocketGuildUser user))
                return Task.FromResult(PreconditionResult.FromError("Not in a guild"));
            bool pass;
            if (user.GuildPermissions.ManageGuild) return Task.FromResult(PreconditionResult.FromSuccess());
            pass = _modules.TryGetValue(user.Guild.Id, out _) ? HandleModule(context, command, services, user) : HandleChannel(context, command, services, user);

            return Task.FromResult(pass ? PreconditionResult.FromSuccess() : PreconditionResult.FromError("Command can't be used here"));
        }

        private bool HandleModule(ICommandContext context, CommandInfo commandInfo,
            IServiceProvider services, SocketGuildUser user)
        {
            _modules.TryGetValue(user.Guild.Id, out var channels);
            if (channels == null) return HandleChannel(context, commandInfo, services, user);
            var moduleInfo = channels.GetOrAdd(context.Channel.Id, new ModuleInformation());
            var module = moduleInfo.Modules.FirstOrDefault(x => x.Name == commandInfo.Module.Name);
            var command = module?.Commands.FirstOrDefault(x => x.Name == commandInfo.Name);
            if (module != null && module.Disabled && (command == null || command.Disabled)) return false;
            if (command != null && !command.Disabled) return true;
            return HandleChannel(context, commandInfo, services, user);
        }

        private bool HandleChannel(ICommandContext context, CommandInfo command,
            IServiceProvider services, SocketGuildUser user)
        {
            var ignoreAll = _ignoreAll.GetOrAdd(user.Guild.Id, false);
            var checkTwo = _listedChannels.TryGetValue(user.Guild.Id, out var channels);
            if (!checkTwo || channels == null) return true;
            if (channels.Contains(context.Channel.Id) && ignoreAll) return true;
            return !channels.Contains(context.Guild.Id) && !ignoreAll;
        }

        public void AddModule() { }
        public void RemoveModule() { }
        public void AddCommand() { }
        public void RemoveCommand() { }
        public void AddChannel() { }
        public void IgnoreAll() { }
    }

    internal class ModuleInformation
    {
        public List<Modules> Modules { get; set; } = new List<Modules>();
    }

    internal class Modules
    {
        public string Name { get; set; }
        public bool Disabled { get; set; }
        public List<Commands> Commands { get; set; } = new List<Commands>();
    }

    internal class Commands
    {
        public string Name { get; set; }
        public bool Disabled { get; set; }
    }
}