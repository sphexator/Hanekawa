using System.Collections.Generic;

namespace Hanekawa.Shared.API
{
    public class ModuleResponse
    {
        public string Name { get; set; }
        public IEnumerable<ModuleResponse> Modules { get; set; }
    }
}