using System.Collections.Generic;

namespace Hanekawa.Core.API
{
    public class ModuleResponse
    {
        public string Name { get; set; }
        public IEnumerable<ModuleResponse> Modules { get; set; }
    }
}