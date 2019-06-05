using System;
using System.Net.Http;
using System.Threading.Tasks;
using Hanekawa.NekoLife.Endpoint;

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

        public async Task GetSlapAsync() { }
        public async Task GetPatAsync() { }
        public async Task GetKissAsync() { }
        public async Task GetPokeAsync() { }
        public async Task GetAvatarAsync() { }
        public async Task GetWallpaperAsync() { }
        public async Task GetNekoAsync() { }
        public async Task GetWaifuAsync() { }
        public async Task GetCuddleAsync() { }
        public async Task GetSmugAsync() { }
        public async Task GetHugAsync() { }
        public async Task GetWoofAsync() { }
        public async Task GetMeowAsync() { }
        public async Task GetBakaAsync() { }
        public async Task GetGasmAsync() { }
        public async Task GetFeedAsync() { }
        public async Task GetNsfwAsync() { }
        public async Task GetSfwAsync() { }
    }
}
