using System.Collections.Generic;

namespace Hanekawa.Core.API
{
    public class CommandResponse
    {
        public string Name { get; set; }
        public string Command { get; set; }
        public IEnumerable<string> Alies { get; set; }
        public string Description { get; set; }
        public IEnumerable<string> RequiredBotPermissions { get; set; }
        public IEnumerable<string> RequiredUserPermissions { get; set; }
        public string Example { get; set; }
    }
}