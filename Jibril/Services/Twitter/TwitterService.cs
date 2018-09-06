using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using Discord.WebSocket;
using System.Threading.Tasks;
using Tweetinvi;
using Tweetinvi.Events;
using Stream = Tweetinvi.Stream;

namespace Hanekawa.Services.Twitter
{
    public class TwitterService
    {
        private readonly DiscordSocketClient _client;

        public TwitterService(DiscordSocketClient client)
        {
            _client = client;
        }

        public async Task InitializeAsync()
        {
            var stream = Stream.CreateAccountActivityStream(581282548);
            stream.TweetCreated += TweetCreated;
        }

        private void TweetCreated(object sender, TweetReceivedEventArgs e)
        {
            var tweet = e.Tweet;
            if (tweet.IsRetweet) return;
            var media = tweet.Entities.Medias;
            if (media.Count == 0) return;
            var result = media.Select(x => x.ExpandedURL).ToList();
            OnTweetRecievedAsync(result);
        }

        private Task OnTweetRecievedAsync(IReadOnlyCollection<string> images)
        {
            var _ = Task.Run(async () =>
            {
                var channel = _client.GetGuild(431617676859932704).GetTextChannel(441744578920448030);
                foreach (var x in images)
                {
                    using (var httpClient = new HttpClient())
                    {
                        var img = await httpClient.GetStreamAsync(x);
                        img.Seek(0, SeekOrigin.Begin);
                        await channel.SendFileAsync(img, "image.png", null);
                    }
                }
            });
            return Task.CompletedTask;
        }
    }
}
