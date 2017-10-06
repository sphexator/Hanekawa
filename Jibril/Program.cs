using System;

namespace Jibril
{
    public class Program
    {
        static void Main(string[] args)
            => new Program().MainASync().GetAwaiter().GetResult();
        private DiscordSocketClient _client;
        private IConfiguration _config;

        public async Task MainASync()
        {

        }
    }
}