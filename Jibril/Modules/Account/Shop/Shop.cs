﻿using System.Collections.Generic;
using System.Linq;
using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.WebSocket;
using Jibril.Extensions;
using Jibril.Services.Entities;
using System.Threading.Tasks;
using Discord;
using Jibril.Preconditions;
using Jibril.Services.Entities.Tables;

namespace Jibril.Modules.Account.Shop
{
    public class Shop : InteractiveBase
    {
        [Command("inventory", RunMode = RunMode.Async)]
        [Alias("inv")]
        [RequiredChannel(339383206669320192)]
        public async Task InventoryAsync()
        {
            await ReplyAsync(null, false,
                new EmbedBuilder().Reply("Inventory is currently disabled", Color.Red.RawValue).Build());
        }

        [Command("shop", RunMode = RunMode.Async)]
        [RequiredChannel(339383206669320192)]
        public async Task ShopAsync()
        {
            await ReplyAsync(null, false, new EmbedBuilder().Reply("Shop is currently disabled").Build());
        }

        [Command("buy", RunMode = RunMode.Async)]
        [RequiredChannel(339383206669320192)]
        public async Task BuyAsync(uint itemId)
        {
            await ReplyAsync(null, false,
                new EmbedBuilder().Reply("Buy command is currently disabled for rework.", Color.Red.RawValue).Build());
        }
    }
}
