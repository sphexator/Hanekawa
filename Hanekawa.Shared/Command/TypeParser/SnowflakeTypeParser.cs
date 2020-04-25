using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Disqord;
using Qmmands;

namespace Hanekawa.Shared.Command.TypeParser
{
    public class SnowflakeTypeParser : HanekawaTypeParser<Snowflake>
    {
        public static SnowflakeTypeParser Instance => _instance ?? (_instance = new SnowflakeTypeParser());

        private static SnowflakeTypeParser _instance;

        private SnowflakeTypeParser()
        { }

        public override ValueTask<TypeParserResult<Snowflake>> ParseAsync(Parameter parameter, string value, HanekawaContext context, IServiceProvider provider)
                    => Snowflake.TryParse(value, out var snowflake)
                ? TypeParserResult<Snowflake>.Successful(snowflake)
                : TypeParserResult<Snowflake>.Unsuccessful("Invalid snowflake format.");

    }
}
