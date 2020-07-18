using System;
using System.Linq;
using System.ServiceModel.Syndication;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using Hanekawa.AnimeSimulCast.Entity;
using Hanekawa.AnimeSimulCast.Events;

namespace Hanekawa.AnimeSimulCast
{
    public class AnimeSimulCastClient
    {
        private SyndicationItem _lastItem;
        private Timer _timer;
        private const string RssFeed = "https://www.crunchyroll.com/rss/anime?lang=enGB";
        public AnimeSimulCastClient() => Initialize();

        public event AsyncEvent<AnimeData> AnimeAired;
        public event AsyncEvent<Exception> Log;
        public async Task Start() => await Main(new CancellationToken());

        private void Initialize()
        {
            try
            {
                using var reader = XmlReader.Create(RssFeed);
                var feed = SyndicationFeed.Load(reader).Items.FirstOrDefault();
                _lastItem = feed;
            }
            catch
            {
                _lastItem = null;
            }
        }

        private Task Main(CancellationToken token)
        {
            _timer = new Timer(state =>
            {
                try
                {
                    using var reader = XmlReader.Create(RssFeed);
                    var feed = SyndicationFeed.Load(reader).Items.FirstOrDefault();
                    _lastItem ??= feed;
                    if (_lastItem == null || feed?.Id == _lastItem.Id) return;
                    _lastItem = feed;
                    _ = AnimeAired?.Invoke(ToReturnType(feed));
                }
                catch (Exception e)
                {
                    _ = Log?.Invoke(e);
                }
            }, token, TimeSpan.Zero, TimeSpan.FromMinutes(5));
            return Task.CompletedTask;
        }

        private AnimeData ToReturnType(SyndicationItem collection)
        {
            var data = new AnimeData
            {
                Title = collection.Title.Text,
                Time = collection.PublishDate
            };
            var url = collection.Links.FirstOrDefault();
            if (url != null) data.Url = url.Uri.AbsoluteUri;
            return data;
        }
    }
}