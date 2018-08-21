using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.Syndication;
using System.Threading.Tasks;
using System.Xml;
using Hanekawa.AnimeSimulCast.Entity;
using Hanekawa.AnimeSimulCast.Events;
using Hanekawa.AnimeSimulCast.Extensions;

namespace Hanekawa.AnimeSimulCast
{
    public class AnimeSimulCastClient
    {
        public event AsyncEvent<IReadOnlyCollection<AnimeData>> AnimeAired;
        private List<SyndicationItem> _poll = new List<SyndicationItem>();

        public AnimeSimulCastClient()
        {
            Initialize();
        }

        public Task StartAsync()
        {
            _ = MainAsync();
            return Task.CompletedTask;
        }

        private void Initialize()
        {
            var reader = XmlReader.Create(Constants.RssFeed);
            var feed = SyndicationFeed.Load(reader);
            _poll.AddRange(feed.Items);
        }

        private async Task MainAsync()
        {
            while (true)
            {
                try
                {
                    var reader = XmlReader.Create(Constants.RssFeed);
                    var feed = SyndicationFeed.Load(reader);
                    //var result = _poll.Except(feed.Items).ToList();
                    var result = feed.Items.Intersect(_poll).ToList();
                    
                    if (result.Count != 0)
                    {
                        UpdatePoll(feed.Items);
                        var collection = ParseToCollection(result);
                        await AnimeAired(collection);
                    }
                    
                    //AnimeAired(ParseToCollection(feed.Items.FirstOrDefault()));
                    await Task.Delay(TimeSpan.FromMinutes(1)).ConfigureAwait(false);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    throw;
                }
            }
        }

        private IReadOnlyCollection<AnimeData> ParseToCollection(
            IEnumerable<SyndicationItem> collection)
        {
            var result = collection.Select(x => new AnimeData
            {
                Title = x.Title.Text.Filter(),
                //Time = x.PublishDate,
                Episode = x.Title.Text.GetEpisode(),
                Season = x.Title.Text.GetSeason()
            }).ToList();

            return result.AsReadOnly();
        }

        private IReadOnlyCollection<AnimeData> ParseToCollection(SyndicationItem collection)
        {
            var result = new List<AnimeData>
            {
                new AnimeData
                {
                    Title = collection.Title.Text.Filter(),
                    //Time = collection.PublishDate,
                    Episode = collection.Title.Text.GetEpisode(),
                    Season = collection.Title.Text.GetSeason(),
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