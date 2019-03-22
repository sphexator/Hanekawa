using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Hanekawa.Addons.Database;

namespace Hanekawa.Bot.Services.ImageGen
{
    public class RankImg
    {
        private readonly HttpClient _client;
        private readonly Random _random;
        private readonly DbService _db;
        private readonly ImageGenerator _image;

        public RankImg(HttpClient client, Random random, DbService db, ImageGenerator image)
        {
            _client = client;
            _random = random;
            _db = db;
            _image = image;
        }

        public async Task<Stream> Builder()
        {
            var stream = new MemoryStream();

            return stream;
        }
    }
}