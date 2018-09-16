using Discord.Commands;
using System;
using System.Globalization;
using System.Threading.Tasks;

namespace Hanekawa.TypeReaders
{
    public class DateTimeTypeReader : TypeReader
    {
        private static readonly string[] Formats = {
            "dd/MM//yyyy HH/mm",
            "d/M/yyyy HH:mm",
            "d/M HH:mm",
        };

        public override Task<TypeReaderResult> ReadAsync(ICommandContext context, string input, IServiceProvider services)
        {
            return (DateTime.TryParseExact(input.ToLowerInvariant(), Formats, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal, out var dateTime))
                ? Task.FromResult(TypeReaderResult.FromSuccess(dateTime))
                : Task.FromResult(TypeReaderResult.FromError(CommandError.ParseFailed, "Failed to parse TimeSpan"));
        }
    }
}
