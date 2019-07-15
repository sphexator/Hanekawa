using System;

namespace Hanekawa.Shared.Interactive
{
    public class InteractiveServiceConfig
    {
        public TimeSpan DefaultTimeout { get; set; } = TimeSpan.FromSeconds(25);
    }
}