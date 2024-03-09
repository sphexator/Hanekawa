using System.Threading;
using System.Threading.Tasks;
using EntityFrameworkCore.Triggered;
using Hanekawa.Application.Interfaces;
using Hanekawa.Entities;
using Microsoft.EntityFrameworkCore;

namespace Hanekawa.Infrastructure.Triggers;

internal abstract class ModLogBeforeTrigger(IDbContext dbContext) : IBeforeSaveTrigger<GuildModerationLog>
{
    /// <summary>
    /// Before saving the moderation log, set the ID to the number of moderation logs in the guild
    /// </summary>
    /// <param name="context"></param>
    /// <param name="cancellationToken"></param>
    public async Task BeforeSave(ITriggerContext<GuildModerationLog> context, CancellationToken cancellationToken)
    {
        if(context.ChangeType is not ChangeType.Added) return;
        
        context.Entity.Id = 
            await dbContext.ModerationLogs.CountAsync(x => x.GuildId == context.Entity.GuildId,
                    cancellationToken: cancellationToken).ConfigureAwait(false) + 1;
    }
}

internal abstract class ModLogAfterTrigger : IAfterSaveTrigger<GuildModerationLog>
{
    public Task AfterSave(ITriggerContext<GuildModerationLog> context, CancellationToken cancellationToken)
    {
        if (context.ChangeType is ChangeType.Added) { }
        return Task.CompletedTask;
    }
}
