﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Discord;
using Discord.WebSocket;
using Hanekawa.Entities;

namespace Hanekawa.Extensions
{
    public static class DictionaryExtension
    {
        public static double? ToxicityAdd(
            this ConcurrentDictionary<ulong,
                ConcurrentDictionary<ulong, ConcurrentDictionary<ulong, LinkedList<ToxicityEntry>>>> context,
            double result, IGuildUser user,
            SocketTextChannel channel, SocketMessage msg)
        {
            var toxList = context.GetOrAdd(user.GuildId,
                new ConcurrentDictionary<ulong, ConcurrentDictionary<ulong, LinkedList<ToxicityEntry>>>());
            var channelValue = toxList.GetOrAdd(channel.Id, new ConcurrentDictionary<ulong, LinkedList<ToxicityEntry>>());
            var userValue = channelValue.GetOrAdd(user.Id, new LinkedList<ToxicityEntry>());

            if (channelValue.Count == 20)
            {
                userValue.RemoveLast();
                userValue.AddFirst(new ToxicityEntry {Value = result, Time = DateTime.UtcNow, MessageId = msg.Id});
            }
            else
            {
                userValue.AddFirst(new ToxicityEntry {Value = result, Time = DateTime.UtcNow, MessageId = msg.Id});
                return null;
            }

            double totalScore = 0;

            foreach (var x in userValue) totalScore = x.Value + totalScore;

            return totalScore / channelValue.Count;
        }
    }
}