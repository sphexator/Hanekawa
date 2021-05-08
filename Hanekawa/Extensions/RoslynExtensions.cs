using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Disqord;
using Disqord.Gateway;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Scripting;

namespace Hanekawa.Extensions
{
    public static class RoslynExtensions
    {
        private const char BackTick = '`';
        private const char NewLine = '\n';

        public static ScriptOptions RoslynScriptOptions { get; }

        public static IReadOnlyDictionary<Type, string> ParameterExampleStrings { get; } = new Dictionary<Type, string>
        {
            [typeof(IMember)] = "<@111123736660324352>",
            [typeof(IMessage)] = "827809315679240202",
            [typeof(ICustomEmoji)] = "<:naicu:469925413162975242>",
            [typeof(ITextChannel)] = "#general",
            [typeof(IRole)] = "@Admins"
        };

        private static readonly string UsingBlock;

        static RoslynExtensions()
        {
            var rawUsing = new[] {
                "Disqord",
                "Disqord.Bot",
                "Disqord.Gateway",
                "Disqord.Rest",
                "Disqord.Rest.Default",
                "Microsoft.Extensions.DependencyInjection",
                "System",
                "System.Collections.Generic",
                "System.Linq",
                "System.Text",
                "System.Threading.Tasks",
                "Qmmands"
            };
            UsingBlock = string.Concat(rawUsing.Select(str => $"using {str}; "));

            var assemblies = AppDomain.CurrentDomain.GetAssemblies()
                .Where(x => !x.IsDynamic && !string.IsNullOrWhiteSpace(x.Location));

            var namespaces = Assembly.GetEntryAssembly()?.GetTypes()
                .Where(x => !string.IsNullOrWhiteSpace(x.Namespace)).Select(x => x.Namespace).Distinct();

            RoslynScriptOptions = ScriptOptions.Default
                .WithReferences(assemblies.Select(assembly => MetadataReference.CreateFromFile(assembly.Location)))
                .AddImports(namespaces);
        }

        public static string GetCode(this string rawCode)
        {
            if (rawCode[0] != BackTick)
            {
                return rawCode;
            }

            if (rawCode[1] != BackTick)
            {
                return rawCode.Substring(1, rawCode.Length - 2);
            }

            var startIndex = rawCode.IndexOf(NewLine);
            if (startIndex == -1)
            {
                throw new ArgumentException("Code blocks not formatted correctly.");
            }

            var code = rawCode.Substring(startIndex + 1, rawCode.Length - startIndex - 5);
            return string.Concat(UsingBlock, code);
        }
    }
}
