using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.Syndication;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using Hanekawa.AnimeSimulCast.Entity;
using Hanekawa.AnimeSimulCast.Events;

namespace Hanekawa.AnimeSimulCast
{
    public class AnimeSimulCastClient
    {
        private const string Url = "https://animedaisuki.moe/rss.php";
        private List<SyndicationItem> _poll = new List<SyndicationItem>();
        private readonly Regex _horribleSubsStart = new Regex("[HorribleSubs]");
        private readonly Regex _horribleSubsEnd   = new Regex("[1080p].mkv");

        public event AsyncEvent<IReadOnlyCollection<AnimeData>> AnimeAired;

        public AnimeSimulCastClient()
        {
            Initialize();
        }

        public Task StartAsync() => MainAsync();

        private void Initialize()
        {
            var reader = XmlReader.Create(Url);
            var feed = SyndicationFeed.Load(reader);
            _poll.AddRange(feed.Items);
        }

        private async Task MainAsync()
        {
            while (true)
            {
                var reader = XmlReader.Create(Url);
                var feed = SyndicationFeed.Load(reader);
                var result = feed.Items.Except(_poll).ToList();
                /*
                if (result.Count != 0)
                {
                    UpdatePoll(feed.Items);
                    await AnimeAired(ToList(result)).ConfigureAwait(false);
                }
                */
                AnimeAired(ToList(feed.Items.FirstOrDefault()));
                await Task.Delay(TimeSpan.FromMinutes(1)).ConfigureAwait(false);
            }
        }

        private IReadOnlyCollection<AnimeData> ToList(IEnumerable<SyndicationItem> collection)
        {
            var result = collection.Select(x => new AnimeData
                {
                    Title = x.Title.Text,
                    Time = x.PublishDate
                })
                .ToList();

            return result.AsReadOnly();
        }

        private static IReadOnlyCollection<AnimeData> ToList(SyndicationItem collection)
        {
            var result = new List<AnimeData>
            {
                new AnimeData
                {
                    Title = collection.Title.Text,
                    Time = collection.PublishDate
                }
            };
            return result.AsReadOnly();
        }

        private void UpdatePoll(IEnumerable<SyndicationItem> collection)
        {
            _poll.Clear();
            _poll.AddRange(collection);
        }
    }
}
