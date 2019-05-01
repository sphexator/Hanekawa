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

        public AnimeSimulCastClient()
        {
            Initialize();
        }

        public event AsyncEvent<AnimeData> AnimeAired;

        public Task StartAsync()
        {
            _ = MainAsync(new CancellationToken());
            return Task.CompletedTask;
        }

        private void Initialize()
        {
            try
            {
                var reader = XmlReader.Create(Constants.RssFeed);
                var feed = SyndicationFeed.Load(reader).Items.FirstOrDefault();
                _lastItem = feed;
            }
            catch
            {
                _lastItem = null;
            }
        }

        private async Task MainAsync(CancellationToken token)
        {
            _timer = new Timer(state =>
            {
                var feed = SyndicationFeed.Load(XmlReader.Create(Constants.RssFeed)).Items.FirstOrDefault();
                if (_lastItem == null) UpdatePoll(feed);
                if (_lastItem != null && feed?.Id != _lastItem.Id)
                {
                    UpdatePoll(feed);
                    _ = AnimeAired(ToReturnType(feed));
                }
            }, token, TimeSpan.Zero, TimeSpan.FromMilliseconds(5));
        }

        private static AnimeData ToReturnType(SyndicationItem collection)
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

        private void UpdatePoll(SyndicationItem item) => _lastItem = item;
    }
}