namespace Hanekawa.Bot.Commands.Metas;

public abstract class SlashGroupName 
{
    public const string Greet = "greet";
    public const string Administration = "administration";
    public const string Club = "club";
    public const string Level = "level";
}

public abstract class Admin 
{
    public const string Ban = "ban";
    public const string Unban = "unban";
    public const string Kick = "kick";
    public const string Mute = "mute";
    public const string Unmute = "unmute";
    public const string Warn = "warn";
    public const string VoidWarn = "voidwarn";
    public const string ClearWarns = "clearwarns";
    public const string WarnLog = "warnlog";
    public const string Prune = "prune";
}

public abstract class Club 
{
    public const string Create = "create";
    public const string Delete = "delete";
    public const string List = "list";
    public const string Join = "join";
    public const string Leave = "leave";
    public const string Info = "info";
}

public abstract class Greet 
{
    public const string Channel = "channel";
    public const string Message = "message";
    public const string ImageUrl = "imageurl";
    public const string ImageList = "imagelist";
    public const string ImageRemove = "imageremove";
    public const string ImageToggle = "image";
}

public abstract class LevelName 
{
    public const string Add = "add";
    public const string Remove = "remove";
    public const string List = "list";
    public const string Modify = "modify";
}