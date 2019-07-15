using System.Collections.Generic;

namespace Hanekawa.Shared.API
{
    public class CommandResponse
    {
        public string Name { get; set; }
        public IEnumerable<string> Command { get; set; }
        public string Description { get; set; }
        public IEnumerable<string> RequiredBotPermissions { get; set; }
        public IEnumerable<string> RequiredUserPermissions { get; set; }
        public IEnumerable<string> Parameters { get; set; }
        public string Example { get; set; }
    }
}