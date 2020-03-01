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

        public AnimeSimulCastClient() => Initialize();

        public event AsyncEvent<AnimeData> AnimeAired;
        public event AsyncEvent<Exception> Log; 

        public async Task Start() => await Main(new CancellationToken());

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

        private Task Main(CancellationToken token)
        {
            _timer = new Timer(state =>
            {
                try
                {
                    var feed = SyndicationFeed.Load(XmlReader.Create(Constants.RssFeed)).Items.FirstOrDefault();
                    if (_lastItem == null) _lastItem = feed;
                    if (_lastItem != null && feed?.Id != _lastItem.Id)
                    {
                        _lastItem = feed;
                        _ = AnimeAired(ToReturnType(feed));
                    }
                }
                catch (Exception e)
                {
                    Log(e);
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