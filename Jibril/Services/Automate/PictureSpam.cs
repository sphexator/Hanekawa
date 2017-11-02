using Discord.WebSocket;
using Dropbox.Api;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jibril.Services.Automate
{
    public class PictureSpam
    {
        private readonly DiscordSocketClient _discord;
        private const string ApiKey = "jkjl5abxifozpz4";

        public PictureSpam(DiscordSocketClient discord)
        {
            _discord = discord;
        }

        private Task PostImages()
        {
            var _ = Task.Run(async () => 
            {
                var client = new DropboxClient(ApiKey);
                var guild = _discord.GetGuild(234505708861652993);
                var Dropboxfolder = "KanColle-bot";
                var output = "Data/Images/PictureSpam/";
                var ch = guild.TextChannels.FirstOrDefault(x => x.Id == 360140270605434882);
                var list = await client.Files.ListFolderAsync(Dropboxfolder);
                foreach(var item in list.Entries.Where(x => x.IsFile))
                {
                    await client.Files.DownloadAsync(output + item.Name);
                    await ch.SendFileAsync(output + item.Name, "");
                }
            });
            return Task.CompletedTask;
        }
    }
}