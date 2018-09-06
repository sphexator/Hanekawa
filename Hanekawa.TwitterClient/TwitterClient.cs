using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hanekawa.TwitterClient.Util;
using Tweetinvi;
using Tweetinvi.Events;

namespace Hanekawa.Twitter
{
    public class TwitterClient
    {
        public event AsyncEvent<IReadOnlyCollection<string>> TweetRecieved;

        public TwitterClient()
        {

        }

        public Task InitializeAsync()
        {
            var _ = Task.Run(async () =>
            {
                Auth.SetUserCredentials("i4LtAgk0vOlUmAdZkQwZb4fyt",
                    "bCn3BUlFfxh9jaAMo8lyFXNHvdwRkMgclktZOeGOks5nKoT0Yi",
                    "581282548-IDTNiTVtCsRdLkWHuXtJ5ZmrOobWhaA056V5N2xH",
                    "Xh8hVuj9fKltKqkC82zxflex2EnporrKgeOmccTwgS9wb");
                var stream = Stream.CreateAccountActivityStream(581282548);
                //stream.TweetCreated += TweetCreated;
                stream.TweetCreated += TweetCreated;
                await Task.Delay(-1).ConfigureAwait(false);
            });
            return Task.CompletedTask;
        }

        private void TweetCreated(object sender, TweetEventArgs e)
        {
            var tweet = e.Tweet;
            if (tweet.IsRetweet) return;
            var media = tweet.Entities.Medias;
            if (media.Count == 0) return;
            var result = media.Select(x => x.ExpandedURL).ToList();
            TweetRecieved?.Invoke(result);
        }
    }
}
