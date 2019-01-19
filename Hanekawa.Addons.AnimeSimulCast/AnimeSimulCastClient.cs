using System;
using System.Linq;
using System.ServiceModel.Syndication;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using Hanekawa.Addons.AnimeSimulCast.Entity;
using Hanekawa.Addons.AnimeSimulCast.Events;

namespace Hanekawa.Addons.AnimeSimulCast
{
    public class AnimeSimulCastClient
    {
        private SyndicationItem LastItem;

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
                LastItem = feed;
            }
            catch
            {
                LastItem = null;
            }
        }

        private async Task MainAsync(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
                try
                {
                    var feed = SyndicationFeed.Load(XmlReader.Create(Constants.RssFeed)).Items.FirstOrDefault();
                    if (LastItem == null) UpdatePoll(feed);
                    if (LastItem != null && feed?.Id != LastItem.Id)
                    {
                        UpdatePoll(feed);
                        _ = AnimeAired(ToReturnType(feed));
                    }

                    await Task.Delay(TimeSpan.FromMinutes(1)).ConfigureAwait(false);
                }
                catch
                {
                    await Task.Delay(TimeSpan.FromMinutes(10)).ConfigureAwait(false);
                }
        }

        private static AnimeData ToReturnType(SyndicationItem collection)
        {
            var data = new AnimeData
            {
                Title = collection.Title.Text,
                Time = collection.PublishDate,
                Url = collection.Links.FirstOrDefault().Uri.AbsoluteUri
            };
            return data;
        }

        private void UpdatePoll(SyndicationItem item) => LastItem = item;
    }
}