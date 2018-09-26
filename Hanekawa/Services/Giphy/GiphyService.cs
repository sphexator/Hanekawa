using System;
using GiphyApiClient.NetCore.Client;
using GiphyApiClient.NetCore.Models.Input;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Hanekawa.Services.Giphy
{
    public class GiphyService
    {
        private readonly GiphyClient _client;
        public GiphyService(GiphyClient client) => _client = client;

        public async Task<Stream> GetImage(FunCommandType type)
        {
            var result = await Search(GetCommandType(type));
            return result;
        }

        public async Task<Stream> GetImage(string text)
        {
            var result = await Search(text);
            return result;
        }

        private async Task<Stream> Search(string text)
        {
            var image = await _client.SearchAsync(new SearchParams($"anime {text}")
                .WithLimit(1)
                .WithOffset(1)
                .WithRating("g")
                .WithLanguage("en"));
            var httpClient = new HttpClient();
            return await httpClient.GetStreamAsync(image.Data.data.First()?.images.fixed_height.url);
        }

        private string GetCommandType(FunCommandType type)
        {
            string result = null;
            switch (type)
            {
                case FunCommandType.HoldHands:
                    result = "Hold Hands";
                    break;
                case FunCommandType.Teehee:
                    result = "Teehee";
                    break;
                case FunCommandType.Stare:
                    result = "Stare";
                    break;
                case FunCommandType.Smile:
                    result = "Smile";
                    break;
                case FunCommandType.Cuddle:
                    result = "Cuddle";
                    break;
                case FunCommandType.Facedesk:
                    result = "Facedesk";
                    break;
                case FunCommandType.Nuzzle:
                    result = "nuzzle";
                    break;
                case FunCommandType.Pout:
                    result = "pout";
                    break;
                case FunCommandType.Tsundere:
                    result = "Tsundere";
                    break;
                case FunCommandType.Greet:
                    result = "Greet";
                    break;
                case FunCommandType.Meow:
                    result = "Meow";
                    break;
                case FunCommandType.Lewd:
                    result = "Lewd";
                    break;
                case FunCommandType.Highfive:
                    result = "High Five";
                    break;
                case FunCommandType.Tickle:
                    result = "Tickle";
                    break;
                case FunCommandType.Slap:
                    result = "Slap";
                    break;
                case FunCommandType.Pat:
                    result = "Pat";
                    break;
                case FunCommandType.Hug:
                    result = "Hug";
                    break;
                case FunCommandType.Kiss:
                    result = "Kiss";
                    break;
                case FunCommandType.Bloodsuck:
                    result = "BloodSuck";
                    break;
                case FunCommandType.Bite:
                    result = "Bite";
                    break;
                case FunCommandType.Nom:
                    result = "Nom";
                    break;
                case FunCommandType.Poke:
                    result = "Poke";
                    break;
            }

            return result;
        }
    }

    public enum FunCommandType
    {
        HoldHands,
        Teehee,
        Stare,
        Smile,
        Cuddle,
        Facedesk,
        Nuzzle,
        Pout,
        Tsundere,
        Greet,
        Meow,
        Lewd,
        Highfive,
        Tickle,
        Slap,
        Pat,
        Hug,
        Kiss,
        Bloodsuck,
        Bite,
        Nom,
        Poke
    }
}
