using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Disqord;
using Disqord.Bot;
using Disqord.Extensions.Interactivity.Menus.Paged;
using Hanekawa.Bot.Commands.Preconditions;
using Qmmands;
using Quartz.Util;

namespace Hanekawa.Extensions
{
    public static class CommandExtensions
    {
        public static IEnumerable<Page> Pagination(this List<string> list, Color color, string avatarUrl, string authorTitle, int inputPerPage = 5)
        {
            var pages = new List<Page>();
            var sb = new StringBuilder();
            var maxPage = Convert.ToInt32(Math.Ceiling((double) list.Count / inputPerPage));
            for (var i = 0; i < list.Count;)
            {
                for (var j = 0; j < inputPerPage; j++)
                {
                    if (i >= list.Count) continue;
                    var x = list[i];
                    sb.AppendLine(x);
                    i++;
                }

                var page = pages.Count + 1;
                pages.Add(new Page(new LocalEmbed
                {
                    Author = new LocalEmbedAuthor {Name = authorTitle, IconUrl = avatarUrl},
                    Description = sb.ToString(),
                    Color = color,
                    Footer = new LocalEmbedFooter {Text = $"Page: {page}/{maxPage}"}
                }));
                sb.Clear();
            }

            return pages;
        }

        public static void FormatCommandText(this List<string> list, Command cmd, IPrefix prefix)
        {
            var command = cmd.FullAliases.FirstOrDefault();
            var content = new StringBuilder();
            var perms = Perm(cmd);
            content.AppendLine(!cmd.Name.IsNullOrWhiteSpace()
                ? $"**{cmd.Name}**"
                : $"**{cmd.FullAliases.FirstOrDefault()}**");
            if (perms == null || perms.Count > 0) content.AppendLine($"**Require {string.Join(" - ", perms)}** ");
            if (PremiumCheck(cmd, out var prem)) content.AppendLine(prem);
            content.AppendLine(
                $"Alias: **{cmd.FullAliases.Aggregate("", (current, cmdName) => current + $"{cmdName}, ")}**");
            if (!cmd.Description.IsNullOrWhiteSpace()) content.AppendLine($"Description: {cmd.Description}");
            if (!cmd.Remarks.IsNullOrWhiteSpace()) content.AppendLine(cmd.Remarks);
            content.AppendLine($"Usage: **{prefix}{command} {Param(cmd)}**");
            content.AppendLine($"Example: {prefix}{command} {ExampleParam(cmd)}");
            list.Add(content.ToString());
        }
        
        // Showcases the params
        public static string Param(this Command command)
        {
            var output = new StringBuilder();
            if (!command.Parameters.Any()) return output.ToString();
            foreach (var x in command.Parameters)
            {
                var name = x.Name;
                if (x.IsOptional)
                    output.Append($"[{name} = {x.DefaultValue}] ");
                else if (x.IsRemainder)
                    output.Append($"...{name} ");
                else if (x.IsMultiple)
                    output.Append($"|{name} etc...|");
                else
                    output.Append($"<{name}> ");
            }

            return output.ToString();
        }

        // Adds examples, ie. for user it adds bob#0000
        public static string ExampleParam(this Command command)
        {
            var output = new StringBuilder();
            if (!command.Parameters.Any()) return output.ToString();
            foreach (var x in command.Parameters)
            {
                var name = PermType(x);
                if (x.IsOptional)
                    output.Append($"{name} ");
                else if (x.IsRemainder)
                    output.Append($"{name} ");
                else if (x.IsMultiple)
                    output.Append($"{name} ");
                else
                    output.Append($"{name} ");
            }

            return output.ToString();
        }

        public static string PermType(this Parameter parameter) =>
            parameter.Type == typeof(IMember) ? "@Hanekawa#6683" :
            parameter.Type == typeof(IRole) ? "@everyone" :
            parameter.Type == typeof(ITextChannel) ? "#General" :
            parameter.Type == typeof(IVoiceChannel) ? "VoiceChannel" :
            parameter.Type == typeof(ICategoryChannel) ? "Category" :
            parameter.Type == typeof(TimeSpan) ? "1h2m1s" :
            parameter.Type == typeof(int) ? "5" :
            parameter.Type == typeof(string) ? "Example text" :
            parameter.Type == typeof(Snowflake) ? "431610594290827267" : parameter.Name;

        // Appends permission requirements
        public static List<string> Perm(this Command cmd)
        {
            List<string> result = null;
            foreach (var x in cmd.Module.Checks)
            {
                if (x is not RequireAuthorGuildPermissionsAttribute perm) continue;
                result ??= new List<string>();
                result.Add(perm.Permissions.ToString());
            }
            foreach (var x in cmd.Checks)
            {
                if (x is not RequireAuthorGuildPermissionsAttribute perm) continue;
                result ??= new List<string>();
                result.Add(perm.Permissions.ToString());
            }
            return result;
        }

        public static bool PremiumCheck(this Command cmd, out string perm)
        {
            var premium = cmd.Checks.FirstOrDefault(x => x is RequirePremium) ??
                          cmd.Module.Checks.FirstOrDefault(x => x is RequirePremium);
            if (premium != null)
            {
                perm = "**Require Premium**";
                return true;
            }

            perm = null;
            return false;
        }
    }
}