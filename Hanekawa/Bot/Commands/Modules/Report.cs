using Qmmands;

namespace Hanekawa.Bot.Commands.Modules
{
    [Name("Report")]
    [Description("Commands to report a issue to the moderation team")]
    public class Report : HanekawaCommandModule
    {
        
    }

    [Name("Report Admin")]
    [Description("Commands to configure the report module")]
    [Group("Report")]
    public class ReportAdmin : Report
    {
        
    }
}