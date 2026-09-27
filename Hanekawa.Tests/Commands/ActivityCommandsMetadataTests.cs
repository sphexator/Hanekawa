using System.Reflection;
using Disqord;
using Disqord.Bot.Commands.Application;
using Hanekawa.Bot.Commands.Checks;
using Hanekawa.Bot.Commands.Metas;
using Hanekawa.Bot.Commands.Slash.Activity;
using Hanekawa.Entities;

namespace Hanekawa.Tests.Commands;

public class ActivityCommandsMetadataTests
{
    [Fact]
    public void ActivityCommands_RequiresActivityModule_OnGroup()
    {
        var data = typeof(ActivityCommands).GetCustomAttributesData()
            .Single(a => a.AttributeType == typeof(RequireModuleAttribute));

        Assert.Equal(ModuleName.Activity, data.ConstructorArguments[0].Value);
    }

    [Fact]
    public void ActivityCommands_ConfigurationSubcommands_RequireManageGuild()
    {
        var methods = new[]
        {
            nameof(ActivityCommands.RewardPreviousAsync),
            nameof(ActivityCommands.RewardCurrentAsync),
            nameof(ActivityCommands.ChannelAsync),
            nameof(ActivityCommands.MessageAsync)
        };

        foreach (var methodName in methods)
        {
            var method = typeof(ActivityCommands).GetMethod(methodName, BindingFlags.Instance | BindingFlags.Public);
            Assert.NotNull(method);

            var permission = method!.GetCustomAttributes(inherit: true)
                .SingleOrDefault(a => a.GetType().Name == "RequireAuthorPermissionsAttribute");
            Assert.NotNull(permission);

            var permissions = (Permissions)permission!.GetType().GetProperty("Permissions")!.GetValue(permission)!;
            Assert.Equal(Permissions.ManageGuild, permissions);
        }
    }

    [Fact]
    public void ActivityCommands_Top_IsPublicReadOnlyCommand()
    {
        var method = typeof(ActivityCommands).GetMethod(nameof(ActivityCommands.TopAsync),
            BindingFlags.Instance | BindingFlags.Public);
        Assert.NotNull(method);
        Assert.DoesNotContain(method!.GetCustomAttributes(inherit: true),
            a => a.GetType().Name == "RequireAuthorPermissionsAttribute");
        Assert.NotNull(method.GetCustomAttribute<SlashCommandAttribute>());
    }
}
