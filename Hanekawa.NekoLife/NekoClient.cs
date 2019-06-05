using System;
using System.Net.Http;

namespace Hanekawa.NekoLife
{
    public class NekoClient
    {
        private readonly HttpClient _client;
        private readonly Random _random;
        private readonly string _baseUrl = "https://nekos.life/api/v2";

        public NekoClient(HttpClient client, Random random)
        {
            _client = client ?? new HttpClient();
            _random = random ?? new Random();
        }
    }
}
