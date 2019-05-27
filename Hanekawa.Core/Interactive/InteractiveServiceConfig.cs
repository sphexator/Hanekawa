using System;

namespace Hanekawa.Core.Interactive
{
    public class InteractiveServiceConfig
    {
        public TimeSpan DefaultTimeout { get; set; } = TimeSpan.FromSeconds(25);
    }
}