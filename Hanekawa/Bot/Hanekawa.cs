using System;
using System.Reflection;
using System.Threading.Tasks;
using Disqord;
using Disqord.Bot;
using Disqord.Bot.Parsers;
using Disqord.Bot.Prefixes;
using Hanekawa.Bot.TypeReaders;
using Hanekawa.Shared.Command;
using Hanekawa.Shared.Command.TypeParser;
using Microsoft.Extensions.DependencyInjection;
using Qmmands;

namespace Hanekawa.Bot
{
    public class Hanekawa : DiscordBot
    {
        public Hanekawa(TokenType tokenType, string token, IPrefixProvider prefixProvider, DiscordBotConfiguration configuration) : base(tokenType, token, prefixProvider, configuration)
        {
            AddModules(Assembly.GetEntryAssembly());
            RemoveTypeParser(GetSpecificTypeParser<CachedRole, CachedRoleTypeParser>());
            RemoveTypeParser(GetSpecificTypeParser<CachedMember, CachedMemberTypeParser>());
            RemoveTypeParser(GetSpecificTypeParser<CachedUser, CachedUserTypeParser>());
            RemoveTypeParser(GetSpecificTypeParser<CachedGuildChannel, CachedGuildChannelTypeParser<CachedGuildChannel>>());
            RemoveTypeParser(GetSpecificTypeParser<CachedTextChannel, CachedGuildChannelTypeParser<CachedTextChannel>>());
            RemoveTypeParser(GetSpecificTypeParser<CachedVoiceChannel, CachedGuildChannelTypeParser<CachedVoiceChannel>>());
            RemoveTypeParser(GetSpecificTypeParser<CachedCategoryChannel, CachedGuildChannelTypeParser<CachedCategoryChannel>>());

            AddTypeParser(new CachedRoleTypeParser(StringComparison.OrdinalIgnoreCase));
            AddTypeParser(new MemberTypeParser());
            AddTypeParser(new CachedUserTypeParser(StringComparison.OrdinalIgnoreCase));
            AddTypeParser(new CachedGuildChannelTypeParser<CachedGuildChannel>(StringComparison.OrdinalIgnoreCase));
            AddTypeParser(new CachedGuildChannelTypeParser<CachedTextChannel>(StringComparison.OrdinalIgnoreCase));
            AddTypeParser(new CachedGuildChannelTypeParser<CachedVoiceChannel>(StringComparison.OrdinalIgnoreCase));
            AddTypeParser(new CachedGuildChannelTypeParser<CachedCategoryChannel>(StringComparison.OrdinalIgnoreCase));
            AddTypeParser(new TimeSpanTypeParser());

            this.CommandExecuted += Hanekawa_CommandExecuted;
        }

        private Task Hanekawa_CommandExecuted(CommandExecutedEventArgs e)
        {
            (e.Context as HanekawaCommandContext)?.Scope.Dispose();
            return Task.CompletedTask;
        }

        protected override async ValueTask<bool> CheckMessageAsync(CachedUserMessage message) =>
            !message.Author.IsBot && !(message.Channel is IPrivateChannel);

        protected override ValueTask<DiscordCommandContext> GetCommandContextAsync(CachedUserMessage message,
            IPrefix prefix)
        {
            var scope = this.CreateScope();
            return new ValueTask<DiscordCommandContext>(new HanekawaCommandContext(scope, this, prefix, message, scope.ServiceProvider.GetRequiredService<ColourService>()));
        }
    }
}