using Disqord;
using Hanekawa.Entities.Discord;
using Riok.Mapperly.Abstractions;

namespace Hanekawa.Bot.Mapper;

[Mapper]
public partial class Mapper
{
    public partial TextChannel MapToTextChannel(TransientInteractionChannel channel);
}