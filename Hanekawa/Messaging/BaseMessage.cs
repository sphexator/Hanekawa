namespace Hanekawa.Messaging;

public class BaseMessage<TMessage> where TMessage : class
{
    public TMessage? Data { get; init; }
    public ulong TenantId { get; init; }
}

public class BaseGuildMessage<TMessage> : BaseMessage<TMessage> where TMessage : class
{
    public ulong GuildId { get; init; }
}