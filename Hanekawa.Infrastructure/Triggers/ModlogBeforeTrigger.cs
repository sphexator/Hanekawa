using System.Threading;
using System.Threading.Tasks;
using EntityFrameworkCore.Triggered;
using Hanekawa.Application.Interfaces;
using Hanekawa.Entities;
using Microsoft.EntityFrameworkCore;

namespace Hanekawa.Infrastructure.Triggers;

internal class ModLogBeforeTrigger : IBeforeSaveTrigger<GuildModerationLog>
{
    private readonly IDbContext _dbContext;
    private readonly IBot _bot;
    public ModLogBeforeTrigger(IDbContext dbContext, IBot bot)
    {
        _dbContext = dbContext;
        _bot = bot;
    }

    public async Task BeforeSave(ITriggerContext<GuildModerationLog> context, CancellationToken cancellationToken)
    {
        if(context.ChangeType is not ChangeType.Added) return;
        
        context.Entity.Id = 
            await _dbContext.ModerationLogs.CountAsync(x => x.GuildId == context.Entity.GuildId,
            cancellationToken: cancellationToken) + 1;
    }
}

internal class ModLogAfterTrigger : IAfterSaveTrigger<GuildModerationLog>
{
    private readonly IBot _bot;
    public ModLogAfterTrigger(IBot bot) => _bot = bot;

    public async Task AfterSave(ITriggerContext<GuildModerationLog> context, CancellationToken cancellationToken)
    {
        if (context.ChangeType is not ChangeType.Added) return;
    }
}
